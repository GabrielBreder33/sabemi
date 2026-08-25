using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Application.Contracts;

public sealed record PaymentQuery(
    string? ContractId = null,
    PaymentStatus? Status = null,
    ProcessingStatus? ProcessingStatus = null,
    int Page = 1,
    int PageSize = 20);

public sealed record PagedPaymentEvents(
    IReadOnlyList<Sabemi.Payment.Domain.Entities.PaymentEvent> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
