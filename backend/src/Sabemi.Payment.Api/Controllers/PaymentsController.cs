using Microsoft.AspNetCore.Mvc;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Services;
using Sabemi.Payment.Domain.Enums;

namespace Sabemi.Payment.Api.Controllers;

[ApiController]
[Route("api/pagamentos")]
public sealed class PaymentsController(PaymentQueryService paymentQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaymentPageResponse>> GetPage(
        [FromQuery] string? contratoId,
        [FromQuery] string? status,
        [FromQuery] string? processingStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new PaymentQuery(
            contratoId,
            ParseEnum<PaymentStatus>(status),
            ParseEnum<ProcessingStatus>(processingStatus),
            page,
            pageSize);

        return Ok(await paymentQueryService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{transactionId}")]
    public async Task<ActionResult<PaymentResponse>> GetByTransactionId(string transactionId, CancellationToken cancellationToken)
    {
        var result = await paymentQueryService.GetByTransactionIdAsync(transactionId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed) ? parsed : null;
}
