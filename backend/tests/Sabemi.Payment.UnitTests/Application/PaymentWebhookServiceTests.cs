using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Services;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Enums;
using FluentValidation;

namespace Sabemi.Payment.UnitTests.Application;

public sealed class PaymentWebhookServiceTests
{
    [Fact]
    public async Task Valid_payload_is_saved_as_pending()
    {
        var repository = new InMemoryPaymentEventRepository();
        var service = new PaymentWebhookService(repository);

        var result = await service.ReceiveAsync(
            new PaymentWebhookRequest("TRX-1", "CTR-1", 25.90m, DateTimeOffset.UtcNow, "Sucesso"),
            "{\"id_transacao\":\"TRX-1\"}",
            CancellationToken.None);

        Assert.False(result.Duplicate);
        Assert.Equal(ProcessingStatus.Pending, result.Event.ProcessingStatus);
        Assert.Equal(1, repository.InsertCount);
    }

    [Fact]
    public async Task Duplicate_transaction_is_accepted_without_second_insert()
    {
        var repository = new InMemoryPaymentEventRepository();
        var service = new PaymentWebhookService(repository);
        var request = new PaymentWebhookRequest("TRX-1", "CTR-1", 25.90m, DateTimeOffset.UtcNow, "Sucesso");

        await service.ReceiveAsync(request, "{}", CancellationToken.None);
        var result = await service.ReceiveAsync(request, "{}", CancellationToken.None);

        Assert.True(result.Duplicate);
        Assert.Equal(1, repository.InsertCount);
    }

    [Fact]
    public async Task Invalid_payload_is_rejected_before_persistence()
    {
        var repository = new InMemoryPaymentEventRepository();
        var service = new PaymentWebhookService(repository);

        await Assert.ThrowsAsync<ValidationException>(() => service.ReceiveAsync(
            new PaymentWebhookRequest("", "CTR-1", 25.90m, DateTimeOffset.UtcNow, "Sucesso"),
            "{}",
            CancellationToken.None));

        Assert.Equal(0, repository.InsertCount);
    }

    [Fact]
    public async Task Invalid_payload_is_persisted_as_validation_failed()
    {
        var repository = new InMemoryPaymentEventRepository();
        var service = new PaymentWebhookService(repository);

        var result = await service.RecordInvalidAsync(
            "{invalid-json",
            "Invalid JSON.",
            null,
            null,
            CancellationToken.None);

        Assert.Equal(ProcessingStatus.ValidationFailed, result.Event.ProcessingStatus);
        Assert.Equal("{invalid-json", result.Event.RawPayload);
        Assert.Equal("Invalid JSON.", result.Event.ErrorMessage);
        Assert.Equal(1, repository.InsertCount);
    }

    private sealed class InMemoryPaymentEventRepository : IPaymentEventRepository
    {
        private readonly List<PaymentEvent> _events = [];
        public int InsertCount { get; private set; }

        public Task<AddPaymentEventResult> AddPendingAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
        {
            var existing = _events.SingleOrDefault(item => item.TransactionId == paymentEvent.TransactionId);
            if (existing is not null) return Task.FromResult(new AddPaymentEventResult(existing, true));
            _events.Add(paymentEvent);
            InsertCount++;
            return Task.FromResult(new AddPaymentEventResult(paymentEvent, false));
        }

        public Task<AddPaymentEventResult> AddInvalidAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
        {
            _events.Add(paymentEvent);
            InsertCount++;
            return Task.FromResult(new AddPaymentEventResult(paymentEvent, false));
        }

        public Task<PaymentEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken) =>
            Task.FromResult(_events.SingleOrDefault(item => item.TransactionId == transactionId));

        public Task<IReadOnlyList<PaymentEvent>> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PaymentEvent>>(_events.Where(item => item.ProcessingStatus == ProcessingStatus.Pending).Take(batchSize).ToList());

        public Task<PagedPaymentEvents> QueryAsync(PaymentQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new PagedPaymentEvents(_events, 1, 20, _events.Count));

        public Task CompleteAsync(PaymentEvent paymentEvent, ContractStatus? contractStatus, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task FailAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
