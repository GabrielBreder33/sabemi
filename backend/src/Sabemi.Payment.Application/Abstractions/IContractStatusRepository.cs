using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Application.Abstractions;

public interface IContractStatusRepository
{
    Task<ContractStatus?> GetAsync(string contractId, CancellationToken cancellationToken);
    Task UpsertAsync(ContractStatus contractStatus, CancellationToken cancellationToken);
}
