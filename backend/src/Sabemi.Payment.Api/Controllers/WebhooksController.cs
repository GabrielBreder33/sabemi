using System.Text.Json;
using FluentValidation;
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
            await paymentWebhookService.RecordInvalidAsync(rawPayload, "Invalid JSON.", null, null, cancellationToken);
            return BadRequest(new { title = "Invalid JSON", status = 400 });
        }

        if (request is null)
        {
            await paymentWebhookService.RecordInvalidAsync(rawPayload, "Invalid JSON.", null, null, cancellationToken);
            return BadRequest(new { title = "Invalid JSON", status = 400 });
        }

        try
        {
            var result = await paymentWebhookService.ReceiveAsync(request, rawPayload, cancellationToken);
            return Accepted(new
            {
                transactionId = result.Event.TransactionId,
                processingStatus = result.Event.ProcessingStatus.ToString(),
                duplicate = result.Duplicate
            });
        }
        catch (ValidationException exception)
        {
            var errorMessage = string.Join("; ", exception.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));
            if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = exception.Message;

            await paymentWebhookService.RecordInvalidAsync(
                rawPayload,
                errorMessage,
                request.TransactionId,
                request.ContractId,
                cancellationToken);

            return BadRequest(new
            {
                title = "Validation failed",
                status = 400,
                errors = exception.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
    }
}
