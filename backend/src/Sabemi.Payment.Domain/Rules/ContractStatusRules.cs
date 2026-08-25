using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Domain.Rules;

public static class ContractStatusRules
{
    public static bool ShouldApply(PaymentEvent incoming, ContractStatus? current)
    {
        return current is null || incoming.PaymentDate >= current.PaymentDate;
    }
}
