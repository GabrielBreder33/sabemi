using FluentValidation.TestHelper;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Validation;

namespace Sabemi.Payment.UnitTests.Validation;

public sealed class PaymentWebhookValidatorTests
{
    private readonly PaymentWebhookValidator _validator = new();

    [Fact]
    public void Rejects_non_positive_amount()
    {
        var result = _validator.TestValidate(new PaymentWebhookRequest("TRX-1", "CTR-1", 0m, DateTimeOffset.UtcNow, "Sucesso"));

        result.ShouldHaveValidationErrorFor(request => request.Amount);
    }

    [Fact]
    public void Rejects_unknown_payment_status()
    {
        var result = _validator.TestValidate(new PaymentWebhookRequest("TRX-1", "CTR-1", 10m, DateTimeOffset.UtcNow, "Pendente"));

        result.ShouldHaveValidationErrorFor(request => request.Status);
    }

    [Fact]
    public void Accepts_valid_payment_webhook()
    {
        var result = _validator.TestValidate(new PaymentWebhookRequest("TRX-1", "CTR-1", 10m, DateTimeOffset.UtcNow, "Sucesso"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
