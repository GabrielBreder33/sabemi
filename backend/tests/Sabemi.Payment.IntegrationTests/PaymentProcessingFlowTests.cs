using System.Net;

namespace Sabemi.Payment.IntegrationTests;

public sealed class PaymentProcessingFlowTests : IClassFixture<PaymentApiFactory>
{
    private readonly PaymentApiFactory _factory;

    public PaymentProcessingFlowTests(PaymentApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_and_health_are_available()
    {
        var client = _factory.CreateClient();

        var health = await client.GetAsync("/health");
        var swagger = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
    }
}
