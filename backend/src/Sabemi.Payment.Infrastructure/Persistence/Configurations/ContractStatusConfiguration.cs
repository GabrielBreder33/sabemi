using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabemi.Payment.Domain.Entities;

namespace Sabemi.Payment.Infrastructure.Persistence.Configurations;

public sealed class ContractStatusConfiguration : IEntityTypeConfiguration<ContractStatus>
{
    public void Configure(EntityTypeBuilder<ContractStatus> builder)
    {
        builder.ToTable("ContractStatuses");
        builder.HasKey(status => status.ContractId);
        builder.Property(status => status.ContractId).HasMaxLength(100);
        builder.Property(status => status.LastTransactionId).HasMaxLength(100).IsRequired();
        builder.Property(status => status.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(status => status.PaymentValue).HasPrecision(18, 2);
    }
}
