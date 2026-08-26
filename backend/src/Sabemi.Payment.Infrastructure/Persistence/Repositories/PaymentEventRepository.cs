using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Infrastructure.Persistence.Repositories;

public sealed class PaymentEventRepository(PaymentDbContext dbContext) : IPaymentEventRepository
{
    public async Task<AddPaymentEventResult> AddPendingAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        return await AddAsync(paymentEvent, cancellationToken);
    }

    public async Task<AddPaymentEventResult> AddInvalidAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        return await AddAsync(paymentEvent, cancellationToken);
    }

    private async Task<AddPaymentEventResult> AddAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        dbContext.PaymentEvents.Add(paymentEvent);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new AddPaymentEventResult(paymentEvent, false);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(paymentEvent).State = EntityState.Detached;
            var existing = paymentEvent.TransactionId is null
                ? null
                : await GetByTransactionIdAsync(paymentEvent.TransactionId, cancellationToken);
            return new AddPaymentEventResult(
                existing ?? throw new InvalidOperationException("The duplicate payment event could not be loaded."),
                true);
        }
    }

    public Task<PaymentEvent?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        return dbContext.PaymentEvents.SingleOrDefaultAsync(paymentEvent => paymentEvent.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEvent>> ClaimPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var events = await dbContext.PaymentEvents
            .FromSqlInterpolated($"SELECT * FROM \"PaymentEvents\" WHERE \"ProcessingStatus\" = 'Pending' ORDER BY \"ReceivedAt\" LIMIT {batchSize} FOR UPDATE SKIP LOCKED")
            .ToListAsync(cancellationToken);

        foreach (var paymentEvent in events)
        {
            paymentEvent.MarkProcessing();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return events;
    }

    public async Task<PagedPaymentEvents> QueryAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var source = dbContext.PaymentEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.ContractId)) source = source.Where(paymentEvent => paymentEvent.ContractId == query.ContractId);
        if (query.Status is not null) source = source.Where(paymentEvent => paymentEvent.PaymentStatus == query.Status);
        if (query.ProcessingStatus is not null) source = source.Where(paymentEvent => paymentEvent.ProcessingStatus == query.ProcessingStatus);

        var totalItems = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(paymentEvent => paymentEvent.ReceivedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedPaymentEvents(items, page, pageSize, totalItems);
    }

    public async Task CompleteAsync(PaymentEvent paymentEvent, ContractStatus? contractStatus, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (contractStatus is not null)
        {
            var existingStatus = await dbContext.ContractStatuses.FindAsync([contractStatus.ContractId], cancellationToken);
            if (existingStatus is null)
            {
                dbContext.ContractStatuses.Add(contractStatus);
            }
            else if (!ReferenceEquals(existingStatus, contractStatus))
            {
                dbContext.ContractStatuses.Update(contractStatus);
            }
        }
        dbContext.PaymentEvents.Update(paymentEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task FailAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        dbContext.PaymentEvents.Update(paymentEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
