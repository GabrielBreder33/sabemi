using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Infrastructure.Persistence.Configurations;

public sealed class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("PaymentEvents");
        builder.HasKey(paymentEvent => paymentEvent.Id);
        builder.HasIndex(paymentEvent => paymentEvent.TransactionId).IsUnique();
        builder.Property(paymentEvent => paymentEvent.TransactionId).HasMaxLength(100).IsRequired(false);
        builder.Property(paymentEvent => paymentEvent.ContractId).HasMaxLength(100).IsRequired(false);
        builder.Property(paymentEvent => paymentEvent.Amount).HasPrecision(18, 2).IsRequired(false);
        builder.Property(paymentEvent => paymentEvent.PaymentDate).IsRequired(false);
        builder.Property(paymentEvent => paymentEvent.PaymentStatus).HasConversion<string>().HasMaxLength(30).IsRequired(false);
        builder.Property(paymentEvent => paymentEvent.ProcessingStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(paymentEvent => paymentEvent.RawPayload).HasColumnType("text").IsRequired();
        builder.Property(paymentEvent => paymentEvent.ErrorMessage).HasMaxLength(2000);
    }
}
