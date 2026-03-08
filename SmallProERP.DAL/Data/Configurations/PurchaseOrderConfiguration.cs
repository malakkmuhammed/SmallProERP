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
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.HasKey(po => po.PurchaseOrderId);

            builder.Property(po => po.PONumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(po => po.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(po => po.Status)
                .IsRequired()
                .HasDefaultValue(POStatus.Draft);

            builder.Property(po => po.OrderDate)
                .IsRequired();

            builder.Property(po => po.CreatedAt)
                .IsRequired();

            builder.HasIndex(po => new { po.PONumber, po.TenantId }).IsUnique();
            builder.HasIndex(po => po.Status);

            builder.HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(po => po.Tenant)
                .WithMany()
                .HasForeignKey(po => po.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
