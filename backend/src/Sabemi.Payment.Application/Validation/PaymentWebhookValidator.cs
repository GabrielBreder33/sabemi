using FluentValidation;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Application.Validation;

public sealed class PaymentWebhookValidator : AbstractValidator<PaymentWebhookRequest>
{
    public PaymentWebhookValidator()
    {
        RuleFor(request => request.TransactionId)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(request => request.ContractId)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, true);
        RuleFor(request => request.PaymentDate)
            .NotEmpty();
        RuleFor(request => request.Status)
            .Must(IsKnownStatus)
            .WithMessage("Status must be Sucesso or Erro.");
    }

    private static bool IsKnownStatus(string status)
    {
        return Enum.TryParse<PaymentStatus>(status, true, out _);
    }
}
