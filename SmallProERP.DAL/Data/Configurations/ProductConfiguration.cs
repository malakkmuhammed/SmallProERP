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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.ProductCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.Category)
                .HasMaxLength(100);

            builder.Property(p => p.Quantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.MinimumStockLevel)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(p => p.PurchasePrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.SellingPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.HasIndex(p => new { p.ProductCode, p.TenantId }).IsUnique();

            builder.HasCheckConstraint("CK_Product_SellingPrice", "[SellingPrice] >= [PurchasePrice]");
            builder.HasCheckConstraint("CK_Product_Quantity", "[Quantity] >= 0");

            builder.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Tenant)
                .WithMany(t => t.Products)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
