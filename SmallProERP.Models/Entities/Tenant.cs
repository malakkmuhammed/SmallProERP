using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public string CompanyName { get; set; } =string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<User>? Users { get; set; }
        public List<Customer>? Customers { get; set; }
        public List<CustomerInteraction>? CustomersInteraction { get; set; }
        public List<Quotation>? Quotations { get; set; }
        public List<QuotationItem>? QuotationItems { get; set; }
        public List<Sale>? Sales { get; set; }
        public List<SaleItem>? SaleItems { get; set; }
        public List<PurchaseOrder>? PurchaseOrders { get; set; }
        public List<PurchaseOrderItem>? PurchaseOrderItems { get; set; }
        public List<Product>? Products { get; set; }
        public List<Supplier>? Suppliers { get; set; }
        public List<InventoryMovement>? InventoryMovements { get; set; }
    }
}
