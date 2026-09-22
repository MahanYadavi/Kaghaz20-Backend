using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Persistence.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Province).IsRequired().HasMaxLength(50);
        builder.Property(x => x.City).IsRequired().HasMaxLength(60);
        builder.Property(x => x.FullAddress).IsRequired().HasMaxLength(400);
        builder.Property(x => x.PostalCode).IsRequired().HasMaxLength(10);
        builder.HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        // آدرس پیش‌فرض اول، سپس جدیدترین؛ ایندکس برای GetMyAddresses.
        builder.HasIndex(x => new { x.UserId, x.IsDefault, x.CreatedAt });
    }
}