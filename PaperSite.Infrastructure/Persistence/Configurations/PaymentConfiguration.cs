using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.GatewayOrderId).IsUnique();
        builder.HasIndex(x => x.RefId).IsUnique().HasFilter("[RefId] IS NOT NULL");
        builder.Property(x => x.AmountToman).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.RefId).HasMaxLength(128);
        builder.Property(x => x.ResCode).HasMaxLength(16);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.HasOne(x => x.Order).WithMany(x => x.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
