using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Services;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.UnitTests.Application;

public sealed class PaymentProcessorTests
{
    [Fact]
    public async Task Processing_success_marks_event_processed_and_updates_contract()
    {
        var paymentEvent = CreateEvent("TRX-1", DateTimeOffset.UtcNow);
        paymentEvent.MarkProcessing();
        var eventRepository = new FakeEventRepository();
        var contractRepository = new FakeContractRepository();
        var processor = new PaymentProcessor(eventRepository, contractRepository, TimeSpan.Zero);

        await processor.ProcessAsync(paymentEvent, CancellationToken.None);

        Assert.Equal(ProcessingStatus.Processed, paymentEvent.ProcessingStatus);
        Assert.NotNull(eventRepository.CompletedContract);
        Assert.Equal("TRX-1", eventRepository.CompletedContract!.LastTransactionId);
    }

    [Fact]
    public async Task Processing_failure_marks_event_failed_with_error_message()
    {
        var paymentEvent = CreateEvent("TRX-FAIL", DateTimeOffset.UtcNow);
        paymentEvent.MarkProcessing();
        var eventRepository = new FakeEventRepository();
        var contractRepository = new FakeContractRepository { Exception = new InvalidOperationException("business rule failed") };
        var processor = new PaymentProcessor(eventRepository, contractRepository, TimeSpan.Zero);

        await processor.ProcessAsync(paymentEvent, CancellationToken.None);

        Assert.Equal(ProcessingStatus.Failed, paymentEvent.ProcessingStatus);
        Assert.Contains("business rule failed", paymentEvent.ErrorMessage);
        Assert.Same(paymentEvent, eventRepository.FailedEvent);
    }

    [Fact]
    public async Task Older_event_does_not_regress_contract_status()
    {
        var paymentEvent = CreateEvent("TRX-OLD", DateTimeOffset.Parse("2026-08-24T10:00:00Z"));
        paymentEvent.MarkProcessing();
        var eventRepository = new FakeEventRepository();
        var contractRepository = new FakeContractRepository
        {
            Current = ContractStatus.Create("CTR-1", "TRX-NEW", PaymentStatus.Sucesso, 100m, DateTimeOffset.Parse("2026-08-25T10:00:00Z"))
        };
        var processor = new PaymentProcessor(eventRepository, contractRepository, TimeSpan.Zero);

        await processor.ProcessAsync(paymentEvent, CancellationToken.None);

        Assert.Equal(ProcessingStatus.Processed, paymentEvent.ProcessingStatus);
        Assert.Null(eventRepository.CompletedContract);
        Assert.Equal("TRX-NEW", contractRepository.Current!.LastTransactionId);
    }

    private static PaymentEvent CreateEvent(string transactionId, DateTimeOffset date) =>
        PaymentEvent.Create(transactionId, "CTR-1", 50m, date, PaymentStatus.Sucesso, "{}");

    private sealed class FakeEventRepository : IPaymentEventRepository
    {
        public ContractStatus? CompletedContract { get; private set; }
        public PaymentEvent? FailedEvent { get; private set; }

        public Task<AddPaymentEventResult> AddPendingAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AddPaymentEventResult> AddInvalidAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<PaymentEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<PaymentEvent>> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<PagedPaymentEvents> QueryAsync(PaymentQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task CompleteAsync(PaymentEvent paymentEvent, ContractStatus? contractStatus, CancellationToken cancellationToken)
        {
            CompletedContract = contractStatus;
            return Task.CompletedTask;
        }

        public Task FailAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
        {
            FailedEvent = paymentEvent;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContractRepository : IContractStatusRepository
    {
        public ContractStatus? Current { get; set; }
        public Exception? Exception { get; init; }

        public Task<ContractStatus?> GetAsync(string contractId, CancellationToken cancellationToken)
        {
            if (Exception is not null) throw Exception;
            return Task.FromResult(Current);
        }

        public Task UpsertAsync(ContractStatus contractStatus, CancellationToken cancellationToken)
        {
            Current = contractStatus;
            return Task.CompletedTask;
        }
    }
}
