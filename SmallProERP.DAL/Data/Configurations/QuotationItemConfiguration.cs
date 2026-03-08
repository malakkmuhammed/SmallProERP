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
    public class QuotationItemConfiguration : IEntityTypeConfiguration<QuotationItem>
    {
        public void Configure(EntityTypeBuilder<QuotationItem> builder)
        {
            builder.HasKey(qi => qi.QuotationItemId);

            builder.Property(qi => qi.Quantity)
                .IsRequired();

            builder.Property(qi => qi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(qi => qi.LineTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasCheckConstraint("CK_QuotationItem_Quantity", "[Quantity] > 0");

            builder.HasOne(qi => qi.Quotation)
                .WithMany(q => q.QuotationItems)
                .HasForeignKey(qi => qi.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qi => qi.Product)
                .WithMany(p => p.QuotationItems)
                .HasForeignKey(qi => qi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qi => qi.Tenant)
                .WithMany()
                .HasForeignKey(qi => qi.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
