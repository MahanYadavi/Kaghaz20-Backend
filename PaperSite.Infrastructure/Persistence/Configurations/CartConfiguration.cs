using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.User).WithOne(x => x.Cart).HasForeignKey<Cart>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Items).WithOne(x => x.Cart).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
    }
}
