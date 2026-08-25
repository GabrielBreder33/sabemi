using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Rules;

namespace Sabemi.Payment.Application.Services;

public sealed class PaymentProcessor(
    IPaymentEventRepository paymentEventRepository,
    IContractStatusRepository contractStatusRepository,
    TimeSpan? processingDelay = null)
{
    private readonly TimeSpan _processingDelay = processingDelay ?? TimeSpan.FromSeconds(2);

    public async Task ProcessAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_processingDelay, cancellationToken);
            var currentStatus = await contractStatusRepository.GetAsync(paymentEvent.ContractId, cancellationToken);
            ContractStatus? updatedStatus = null;

            if (ContractStatusRules.ShouldApply(paymentEvent, currentStatus))
            {
                updatedStatus = currentStatus ?? ContractStatus.Create(
                    paymentEvent.ContractId,
                    paymentEvent.TransactionId,
                    paymentEvent.PaymentStatus,
                    paymentEvent.Amount,
                    paymentEvent.PaymentDate);

                if (currentStatus is not null)
                {
                    currentStatus.Apply(paymentEvent);
                    updatedStatus = currentStatus;
                }
            }

            paymentEvent.MarkProcessed();
            await paymentEventRepository.CompleteAsync(paymentEvent, updatedStatus, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            paymentEvent.MarkFailed(exception.Message);
            await paymentEventRepository.FailAsync(paymentEvent, cancellationToken);
        }
    }
}
