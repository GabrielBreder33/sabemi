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
            var currentStatus = paymentEvent.ContractId is null
                ? null
                : await contractStatusRepository.GetAsync(paymentEvent.ContractId, cancellationToken);
            ContractStatus? updatedStatus = null;

            if (paymentEvent.ContractId is not null &&
                paymentEvent.TransactionId is not null &&
                paymentEvent.PaymentStatus is not null &&
                paymentEvent.Amount is not null &&
                paymentEvent.PaymentDate is not null &&
                ContractStatusRules.ShouldApply(paymentEvent, currentStatus))
            {
                updatedStatus = currentStatus ?? ContractStatus.Create(
                    paymentEvent.ContractId,
                    paymentEvent.TransactionId,
                    paymentEvent.PaymentStatus.Value,
                    paymentEvent.Amount.Value,
                    paymentEvent.PaymentDate.Value);

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
