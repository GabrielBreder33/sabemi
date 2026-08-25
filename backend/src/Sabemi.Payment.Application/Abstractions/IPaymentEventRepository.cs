using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Application.Abstractions;

public sealed record AddPaymentEventResult(PaymentEvent Event, bool IsDuplicate);

public interface IPaymentEventRepository
{
    Task<AddPaymentEventResult> AddPendingAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken);
    Task<PaymentEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentEvent>> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken);
    Task<PagedPaymentEvents> QueryAsync(PaymentQuery query, CancellationToken cancellationToken);
    Task CompleteAsync(PaymentEvent paymentEvent, ContractStatus? contractStatus, CancellationToken cancellationToken);
    Task FailAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken);
}
