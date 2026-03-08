using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public int Quantity { get; set; }

        public int MinimumStockLevel { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SellingPrice { get; set; }

        public int? SupplierId { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        
        public Supplier? Supplier { get; set; }

        public Tenant? Tenant { get; set; }

        public List<QuotationItem>? QuotationItems { get; set; }

        public List<SaleItem>? SaleItems { get; set; }

        public List<PurchaseOrderItem>? PurchaseOrderItems { get; set; }

        public List<InventoryMovement>? InventoryMovements { get; set; }
    }
}
