using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Persistence.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.Property(x => x.controllerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.requestJson)
            .IsRequired();

        builder.Property(x => x.responseJson)
            .IsRequired();

        builder.Property(x => x.persianDate)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Time)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ipAddress)
            .HasMaxLength(64);

        builder.Property(x => x.userId)
            .HasMaxLength(100);
    }
}
