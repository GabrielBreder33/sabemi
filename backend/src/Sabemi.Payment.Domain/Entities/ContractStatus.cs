using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Domain.Entities;

public sealed class ContractStatus
{
    private ContractStatus()
    {
    }

    public string ContractId { get; private set; } = string.Empty;
    public string LastTransactionId { get; private set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; private set; }
    public decimal PaymentValue { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ContractStatus Create(
        string contractId,
        string transactionId,
        PaymentStatus paymentStatus,
        decimal paymentValue,
        DateTimeOffset paymentDate)
    {
        return new ContractStatus
        {
            ContractId = contractId,
            LastTransactionId = transactionId,
            PaymentStatus = paymentStatus,
            PaymentValue = decimal.Round(paymentValue, 2),
            PaymentDate = paymentDate,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Apply(PaymentEvent paymentEvent)
    {
        if (paymentEvent.TransactionId is null ||
            paymentEvent.PaymentStatus is null ||
            paymentEvent.Amount is null ||
            paymentEvent.PaymentDate is null)
        {
            throw new InvalidOperationException("Only valid payment events can update contract status.");
        }

        LastTransactionId = paymentEvent.TransactionId;
        PaymentStatus = paymentEvent.PaymentStatus.Value;
        PaymentValue = paymentEvent.Amount.Value;
        PaymentDate = paymentEvent.PaymentDate.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
