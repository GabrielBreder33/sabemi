using FluentValidation;
using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Validation;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Application.Services;

public sealed record ReceivePaymentResult(PaymentEvent Event, bool Duplicate);

public sealed class PaymentWebhookService(
    IPaymentEventRepository paymentEventRepository,
    IValidator<PaymentWebhookRequest>? validator = null)
{
    private readonly IValidator<PaymentWebhookRequest> _validator = validator ?? new PaymentWebhookValidator();

    public async Task<ReceivePaymentResult> ReceiveAsync(
        PaymentWebhookRequest request,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        if (!Enum.TryParse<PaymentStatus>(request.Status, true, out var paymentStatus))
        {
            throw new ValidationException("Status must be Sucesso or Erro.");
        }

        var paymentEvent = PaymentEvent.Create(
            request.TransactionId,
            request.ContractId,
            request.Amount,
            request.PaymentDate,
            paymentStatus,
            rawPayload);

        var result = await paymentEventRepository.AddPendingAsync(paymentEvent, cancellationToken);
        return new ReceivePaymentResult(result.Event, result.IsDuplicate);
    }
}
