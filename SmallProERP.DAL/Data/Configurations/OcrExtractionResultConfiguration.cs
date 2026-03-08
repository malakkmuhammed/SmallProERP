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
    public class OcrExtractionResultConfiguration : IEntityTypeConfiguration<OcrExtractionResult>
    {
        public void Configure(EntityTypeBuilder<OcrExtractionResult> builder)
        {
            builder.HasKey(x => x.OcrResultId);

            builder.Property(x => x.ImagePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Success");

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
