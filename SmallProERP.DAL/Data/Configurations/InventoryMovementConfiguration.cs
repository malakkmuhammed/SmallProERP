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
    public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
    {
        public void Configure(EntityTypeBuilder<InventoryMovement> builder)
        {
            builder.HasKey(im => im.MovementId);

            builder.Property(im => im.MovementType)
                .IsRequired();

            builder.Property(im => im.Quantity)
                .IsRequired();

            builder.Property(im => im.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(im => im.MovementDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(im => im.Notes)
                .HasMaxLength(500);

            builder.HasIndex(im => new { im.ProductId, im.MovementDate });

            builder.HasOne(im => im.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(im => im.Tenant)
                .WithMany()
                .HasForeignKey(im => im.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
