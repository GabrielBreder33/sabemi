using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Services;

namespace Sabemi.Payment.Api.Background;

public sealed class PaymentProcessingWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentProcessingWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IPaymentEventRepository>();
                var processor = scope.ServiceProvider.GetRequiredService<PaymentProcessor>();
                var events = await eventRepository.ClaimPendingAsync(10, stoppingToken);

                if (events.Count == 0)
                {
                    await Task.Delay(PollingInterval, stoppingToken);
                    continue;
                }

                foreach (var paymentEvent in events)
                {
                    logger.LogInformation("Processing payment event {TransactionId}, attempt {AttemptCount}", paymentEvent.TransactionId, paymentEvent.AttemptCount);
                    await processor.ProcessAsync(paymentEvent, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Payment processing worker iteration failed");
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
    }
}
