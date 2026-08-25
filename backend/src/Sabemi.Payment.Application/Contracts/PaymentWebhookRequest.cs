namespace Sabemi.Payment.Application.Contracts;

public sealed record PaymentWebhookRequest(
    string TransactionId,
    string ContractId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string Status);
