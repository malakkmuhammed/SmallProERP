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
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(s => s.SaleId);

            builder.Property(s => s.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(s => s.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.IsPaid)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(s => s.PaymentNotes)
                .HasMaxLength(500);

            builder.Property(s => s.InvoiceDate)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.HasIndex(s => new { s.InvoiceNumber, s.TenantId }).IsUnique();
            builder.HasIndex(s => s.IsPaid);

            builder.HasOne(s => s.Quotation)
                .WithMany(q => q.Sales)
                .HasForeignKey(s => s.QuotationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
