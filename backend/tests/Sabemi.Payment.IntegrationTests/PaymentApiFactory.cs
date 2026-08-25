using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Sabemi.Payment.IntegrationTests;

public sealed class PaymentApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("sabemi_payments_test")
        .WithUsername("sabemi")
        .WithPassword("test-password")
        .Build();

    public const string ApiKey = "integration-api-key";
    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Payments", ConnectionString);
        builder.UseSetting("Webhook:ApiKey", ApiKey);
        builder.UseSetting("APPLY_MIGRATIONS", "true");
    }
}
