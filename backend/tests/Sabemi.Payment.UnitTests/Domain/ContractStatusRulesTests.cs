using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Enums;
using Sabemi.Payment.Domain.Rules;

namespace Sabemi.Payment.UnitTests.Domain;

public sealed class ContractStatusRulesTests
{
    [Fact]
    public void Applies_event_when_payment_date_is_newer()
    {
        var current = ContractStatus.Create("CTR-1", "TRX-OLD", PaymentStatus.Sucesso, 10m, DateTimeOffset.Parse("2026-08-24T10:00:00Z"));
        var incoming = PaymentEvent.Create("TRX-NEW", "CTR-1", 20m, DateTimeOffset.Parse("2026-08-25T10:00:00Z"), PaymentStatus.Erro, "{}");

        Assert.True(ContractStatusRules.ShouldApply(incoming, current));
    }

    [Fact]
    public void Does_not_apply_older_event()
    {
        var current = ContractStatus.Create("CTR-1", "TRX-NEW", PaymentStatus.Sucesso, 20m, DateTimeOffset.Parse("2026-08-25T10:00:00Z"));
        var incoming = PaymentEvent.Create("TRX-OLD", "CTR-1", 10m, DateTimeOffset.Parse("2026-08-24T10:00:00Z"), PaymentStatus.Erro, "{}");

        Assert.False(ContractStatusRules.ShouldApply(incoming, current));
    }

    [Fact]
    public void Applies_event_when_dates_are_equal()
    {
        var current = ContractStatus.Create("CTR-1", "TRX-FIRST", PaymentStatus.Sucesso, 10m, DateTimeOffset.Parse("2026-08-25T10:00:00Z"));
        var incoming = PaymentEvent.Create("TRX-LAST", "CTR-1", 20m, DateTimeOffset.Parse("2026-08-25T10:00:00Z"), PaymentStatus.Erro, "{}");

        Assert.True(ContractStatusRules.ShouldApply(incoming, current));
    }
}
