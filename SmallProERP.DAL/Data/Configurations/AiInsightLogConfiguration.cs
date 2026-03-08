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
    public class AiInsightLogConfiguration : IEntityTypeConfiguration<AiInsightLog>

    {
        public void Configure(EntityTypeBuilder<AiInsightLog> builder)
        {
            builder.HasKey(x => x.InsightLogId);

            builder.Property(x => x.InsightType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.InsightText)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
        }
    }
}
