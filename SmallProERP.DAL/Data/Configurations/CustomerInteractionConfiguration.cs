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
    
        public class CustomerInteractionConfiguration : IEntityTypeConfiguration<CustomerInteraction>
        {
            public void Configure(EntityTypeBuilder<CustomerInteraction> builder)
            {
                builder.HasKey(ci => ci.InteractionId);

                builder.Property(ci => ci.Type)
                    .HasMaxLength(50);

                builder.Property(ci => ci.Description)
                    .IsRequired();

                builder.Property(ci => ci.InteractionDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                builder.HasOne(ci => ci.Customer)
                    .WithMany(c => c.Interactions)
                    .HasForeignKey(ci => ci.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(ci => ci.User)
                    .WithMany()
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(ci => ci.Tenant)
                    .WithMany()
                    .HasForeignKey(ci => ci.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    
}
