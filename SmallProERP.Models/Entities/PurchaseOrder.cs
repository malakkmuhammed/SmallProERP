using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }

        public string PONumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public decimal TotalAmount { get; set; }

        public POStatus Status { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public string? Notes { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

        
        public Supplier? Supplier { get; set; }

        public Tenant? Tenant { get; set; }

        public List<PurchaseOrderItem>? PurchaseOrderItems { get; set; }
    }
}
