using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? Address { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

       
        public Tenant? Tenant { get; set; }

        public List<Product>? Products { get; set; }

        public List<PurchaseOrder>? PurchaseOrders { get; set; }
    }
}
