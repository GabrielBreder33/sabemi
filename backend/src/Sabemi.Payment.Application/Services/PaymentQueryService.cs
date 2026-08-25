using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Contracts;

namespace Sabemi.Payment.Application.Services;

public sealed class PaymentQueryService(
    IPaymentEventRepository paymentEventRepository,
    IContractStatusRepository contractStatusRepository)
{
    public async Task<PaymentPageResponse> GetPageAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        var page = await paymentEventRepository.QueryAsync(query, cancellationToken);
        return new PaymentPageResponse(
            page.Items.Select(paymentEvent => PaymentResponseMapper.ToResponse(paymentEvent)).ToList(),
            page.Page,
            page.PageSize,
            page.TotalItems,
            page.TotalPages);
    }

    public async Task<PaymentResponse?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        var paymentEvent = await paymentEventRepository.GetByTransactionIdAsync(transactionId, cancellationToken);
        return paymentEvent is null ? null : PaymentResponseMapper.ToResponse(paymentEvent, true);
    }

    public async Task<ContractStatusResponse?> GetContractAsync(string contractId, CancellationToken cancellationToken)
    {
        var status = await contractStatusRepository.GetAsync(contractId, cancellationToken);
        return status is null ? null : PaymentResponseMapper.ToResponse(status);
    }
}
