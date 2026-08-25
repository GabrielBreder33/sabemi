using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Domain.Entities;

public sealed class PaymentEvent
{
    private PaymentEvent()
    {
    }

    public Guid Id { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;
    public string ContractId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string RawPayload { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public ProcessingStatus ProcessingStatus { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int AttemptCount { get; private set; }

    public static PaymentEvent Create(
        string transactionId,
        string contractId,
        decimal amount,
        DateTimeOffset paymentDate,
        PaymentStatus paymentStatus,
        string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("TransactionId is required.", nameof(transactionId));
        if (string.IsNullOrWhiteSpace(contractId)) throw new ArgumentException("ContractId is required.", nameof(contractId));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(rawPayload)) throw new ArgumentException("RawPayload is required.", nameof(rawPayload));

        return new PaymentEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId.Trim(),
            ContractId = contractId.Trim(),
            Amount = decimal.Round(amount, 2),
            PaymentDate = paymentDate,
            PaymentStatus = paymentStatus,
            RawPayload = rawPayload,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending,
            AttemptCount = 0
        };
    }

    public void MarkProcessing()
    {
        ProcessingStatus = ProcessingStatus.Processing;
        AttemptCount++;
    }

    public void MarkProcessed()
    {
        ProcessingStatus = ProcessingStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkFailed(string message)
    {
        ProcessingStatus = ProcessingStatus.Failed;
        ErrorMessage = message[..Math.Min(message.Length, 2000)];
    }
}
