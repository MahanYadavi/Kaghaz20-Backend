using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Infrastructure.Persistence.Configurations
{
    public class OtpCodeConfiguration
        : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(
            EntityTypeBuilder<OtpCode> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(
                x => !x.IsDeleted
            );

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.CodeHash)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.FailedAttempts)
                .HasDefaultValue(0);

            builder.HasIndex(x => new
            {
                x.PhoneNumber,
                x.CreatedAt
            });
        }
    }
}
