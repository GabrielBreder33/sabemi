using System.Net;
using System.Net.Http.Json;
using System.Text;
using Npgsql;

namespace Sabemi.Payment.IntegrationTests;

public sealed class PaymentWebhookConcurrencyTests : IClassFixture<PaymentApiFactory>
{
    private readonly PaymentApiFactory _factory;

    public PaymentWebhookConcurrencyTests(PaymentApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Simultaneous_webhooks_create_one_event()
    {
        var transactionId = $"TRX-CONCURRENT-{Guid.NewGuid():N}";
        var requests = Enumerable.Range(0, 2).Select(_ => SendWebhookAsync(transactionId));
        var responses = await Task.WhenAll(requests);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Accepted, response.StatusCode));
        Assert.Equal(1, await CountEventsAsync(transactionId));
    }

    [Fact]
    public async Task Invalid_api_key_is_rejected()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/pagamento")
        {
            Content = JsonContent.Create(new
            {
                id_transacao = $"TRX-UNAUTHORIZED-{Guid.NewGuid():N}",
                id_contrato = "CTR-1",
                valor = 10.0m,
                data_pagamento = DateTimeOffset.UtcNow,
                status = "Sucesso"
            })
        };
        request.Headers.Add("X-Api-Key", "wrong-key");

        var response = await _factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_json_is_rejected_but_recorded_for_dashboard()
    {
        var rawPayload = "{invalid-json";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/pagamento")
        {
            Content = new StringContent(rawPayload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Api-Key", PaymentApiFactory.ApiKey);

        var response = await _factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, await CountValidationFailuresAsync(rawPayload));
    }

    [Fact]
    public async Task Valid_webhook_is_processed_by_background_worker()
    {
        var transactionId = $"TRX-PROCESS-{Guid.NewGuid():N}";
        await SendWebhookAsync(transactionId);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            if (await GetProcessingStatusAsync(transactionId) == "Processed") return;
            await Task.Delay(1000);
        }

        Assert.Equal("Processed", await GetProcessingStatusAsync(transactionId));
    }

    private async Task<HttpResponseMessage> SendWebhookAsync(string transactionId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/pagamento")
        {
            Content = JsonContent.Create(new
            {
                id_transacao = transactionId,
                id_contrato = "CTR-INTEGRATION",
                valor = 25.90m,
                data_pagamento = DateTimeOffset.UtcNow,
                status = "Sucesso"
            })
        };
        request.Headers.Add("X-Api-Key", PaymentApiFactory.ApiKey);
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<int> CountEventsAsync(string transactionId)
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"PaymentEvents\" WHERE \"TransactionId\" = @transactionId", connection);
        command.Parameters.AddWithValue("transactionId", transactionId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<string?> GetProcessingStatusAsync(string transactionId)
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT \"ProcessingStatus\" FROM \"PaymentEvents\" WHERE \"TransactionId\" = @transactionId", connection);
        command.Parameters.AddWithValue("transactionId", transactionId);
        return (string?)await command.ExecuteScalarAsync();
    }

    private async Task<int> CountValidationFailuresAsync(string rawPayload)
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"PaymentEvents\" WHERE \"ProcessingStatus\" = 'ValidationFailed' AND \"RawPayload\" = @rawPayload", connection);
        command.Parameters.AddWithValue("rawPayload", rawPayload);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
