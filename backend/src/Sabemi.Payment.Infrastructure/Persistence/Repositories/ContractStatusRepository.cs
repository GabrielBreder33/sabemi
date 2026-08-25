using Microsoft.EntityFrameworkCore;
using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Infrastructure.Persistence.Repositories;

public sealed class ContractStatusRepository(PaymentDbContext dbContext) : IContractStatusRepository
{
    public Task<ContractStatus?> GetAsync(string contractId, CancellationToken cancellationToken)
    {
        return dbContext.ContractStatuses.SingleOrDefaultAsync(status => status.ContractId == contractId, cancellationToken);
    }

    public async Task UpsertAsync(ContractStatus contractStatus, CancellationToken cancellationToken)
    {
        dbContext.ContractStatuses.Update(contractStatus);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
