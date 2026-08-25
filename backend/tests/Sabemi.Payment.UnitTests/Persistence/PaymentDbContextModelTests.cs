using Microsoft.EntityFrameworkCore;
using Sabemi.Payment.Domain.Entities;
using Sabemi.Payment.Infrastructure.Persistence;

namespace Sabemi.Payment.UnitTests.Persistence;

public sealed class PaymentDbContextModelTests
{
    [Fact]
    public void Payment_event_transaction_id_has_unique_index()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase("model-test")
            .Options;

        using var context = new PaymentDbContext(options);
        var entity = context.Model.FindEntityType(typeof(PaymentEvent));
        var uniqueIndex = entity!.GetIndexes().Single(index => index.Properties.Any(property => property.Name == nameof(PaymentEvent.TransactionId)));

        Assert.True(uniqueIndex.IsUnique);
    }
}
