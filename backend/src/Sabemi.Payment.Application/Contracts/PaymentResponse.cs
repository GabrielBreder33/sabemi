using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Application.Contracts;

public sealed record PaymentResponse(
    Guid Id,
    string TransactionId,
    string ContractId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string PaymentStatus,
    string ProcessingStatus,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? ProcessedAt,
    string? ErrorMessage,
    string? RawPayload);

public sealed record PaymentPageResponse(
    IReadOnlyList<PaymentResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ContractStatusResponse(
    string ContractId,
    string LastTransactionId,
    string PaymentStatus,
    decimal PaymentValue,
    DateTimeOffset PaymentDate,
    DateTimeOffset UpdatedAt);

public static class PaymentResponseMapper
{
    public static PaymentResponse ToResponse(PaymentEvent paymentEvent, bool includeRawPayload = false) => new(
        paymentEvent.Id,
        paymentEvent.TransactionId,
        paymentEvent.ContractId,
        paymentEvent.Amount,
        paymentEvent.PaymentDate,
        paymentEvent.PaymentStatus.ToString(),
        paymentEvent.ProcessingStatus.ToString(),
        paymentEvent.ReceivedAt,
        paymentEvent.ProcessedAt,
        paymentEvent.ErrorMessage,
        includeRawPayload ? paymentEvent.RawPayload : null);

    public static ContractStatusResponse ToResponse(ContractStatus status) => new(
        status.ContractId,
        status.LastTransactionId,
        status.PaymentStatus.ToString(),
        status.PaymentValue,
        status.PaymentDate,
        status.UpdatedAt);
}
