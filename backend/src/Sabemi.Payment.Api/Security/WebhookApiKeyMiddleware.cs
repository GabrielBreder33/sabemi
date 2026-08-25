using System.Security.Cryptography;

namespace Sabemi.Payment.Api.Security;

public sealed class WebhookApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.Equals("/webhooks/pagamento", StringComparison.OrdinalIgnoreCase) ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            await next(context);
            return;
        }

        var configuredKey = configuration["Webhook:ApiKey"];
        var requestKey = context.Request.Headers["X-Api-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configuredKey) || !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(configuredKey),
                System.Text.Encoding.UTF8.GetBytes(requestKey)))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Unauthorized", detail = "A valid X-Api-Key is required." });
            return;
        }

        await next(context);
    }
}
