using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.DAL.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.TenantId);

            builder.Property(t => t.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.Email).IsUnique();
            builder.HasIndex(t => t.CompanyName).IsUnique();

            builder.Property(t => t.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(t => t.CreatedAt)
                .IsRequired();
        }
    }
}
