using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sabemi.Payment.Application.Contracts;
using Sabemi.Payment.Application.Services;

namespace Sabemi.Payment.Api.Controllers;

[ApiController]
[Route("webhooks")]
public sealed class WebhooksController(PaymentWebhookService paymentWebhookService) : ControllerBase
{
    [HttpPost("pagamento")]
    public async Task<IActionResult> ReceivePayment(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);
        PaymentWebhookRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PaymentWebhookRequest>(rawPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return BadRequest(new { title = "Invalid JSON", status = 400 });
        }

        if (request is null)
        {
            return BadRequest(new { title = "Invalid JSON", status = 400 });
        }

        var result = await paymentWebhookService.ReceiveAsync(request, rawPayload, cancellationToken);
        return Accepted(new
        {
            transactionId = result.Event.TransactionId,
            processingStatus = result.Event.ProcessingStatus.ToString(),
            duplicate = result.Duplicate
        });
    }
}
