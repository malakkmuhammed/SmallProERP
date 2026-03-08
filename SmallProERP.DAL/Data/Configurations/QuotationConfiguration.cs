using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.DAL.Data.Configurations
{
    public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
    {
        public void Configure(EntityTypeBuilder<Quotation> builder)
        {
            builder.HasKey(q => q.QuotationId);

            builder.Property(q => q.QuotationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(q => q.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(q => q.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(q => q.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(q => q.Status)
                .IsRequired()
                .HasDefaultValue(QuotationStatus.Draft);

            builder.Property(q => q.QuotationDate)
                .IsRequired();

            builder.Property(q => q.CreatedAt)
                .IsRequired();

            builder.HasIndex(q => new { q.QuotationNumber, q.TenantId }).IsUnique();

            builder.HasOne(q => q.Customer)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(q => q.Creator)
                .WithMany()
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(q => q.Tenant)
                .WithMany()
                .HasForeignKey(q => q.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
