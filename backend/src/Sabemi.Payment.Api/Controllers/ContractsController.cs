using Microsoft.AspNetCore.Mvc;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Services;

namespace Sabemi.Payment.Api.Controllers;

[ApiController]
[Route("api/contratos")]
public sealed class ContractsController(PaymentQueryService paymentQueryService) : ControllerBase
{
    [HttpGet("{contractId}")]
    public async Task<ActionResult<ContractStatusResponse>> Get(string contractId, CancellationToken cancellationToken)
    {
        var result = await paymentQueryService.GetContractAsync(contractId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
