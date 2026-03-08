using Microsoft.AspNetCore.Identity;                   
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore;                    
using Microsoft.AspNetCore.Http;                        
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmallProERP.DAL.Data.Configurations;

namespace SmallProERP.DAL.Data
{
    public class SmallProDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _currentTenantId;

        public SmallProDbContext(
            DbContextOptions<SmallProDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;

            if (_httpContextAccessor.HttpContext?.Items.ContainsKey("TenantId") == true)
            {
                _currentTenantId = (int)_httpContextAccessor.HttpContext.Items["TenantId"];
            }
        }

        
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerInteraction> CustomerInteractions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        public DbSet<OcrExtractionResult> OcrExtractionResults { get; set; }
        public DbSet<AiInsightLog> AiInsightLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerInteractionConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new SupplierConfiguration());
            modelBuilder.ApplyConfiguration(new QuotationConfiguration());
            modelBuilder.ApplyConfiguration(new QuotationItemConfiguration());
            modelBuilder.ApplyConfiguration(new  SaleConfiguration());
            modelBuilder.ApplyConfiguration(new SaleItemConfiguration());
            modelBuilder.ApplyConfiguration(new  PurchaseOrderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseOrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryMovementConfiguration());
           

            
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.TenantId == _currentTenantId);

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => c.TenantId == _currentTenantId);

            modelBuilder.Entity<CustomerInteraction>()
                .HasQueryFilter(ci => ci.TenantId == _currentTenantId);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => p.TenantId == _currentTenantId);

            modelBuilder.Entity<Supplier>()
                .HasQueryFilter(s => s.TenantId == _currentTenantId);

            modelBuilder.Entity<Quotation>()
                .HasQueryFilter(q => q.TenantId == _currentTenantId);

            modelBuilder.Entity<QuotationItem>()
                .HasQueryFilter(qi => qi.TenantId == _currentTenantId);

            modelBuilder.Entity<Sale>()
                .HasQueryFilter(s => s.TenantId == _currentTenantId);

            modelBuilder.Entity<SaleItem>()
                .HasQueryFilter(si => si.TenantId == _currentTenantId);

            modelBuilder.Entity<PurchaseOrder>()
                .HasQueryFilter(po => po.TenantId == _currentTenantId);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasQueryFilter(poi => poi.TenantId == _currentTenantId);

            modelBuilder.Entity<InventoryMovement>()
                .HasQueryFilter(im => im.TenantId == _currentTenantId);
        }
    }
}
