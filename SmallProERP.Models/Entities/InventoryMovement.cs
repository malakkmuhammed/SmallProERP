using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class InventoryMovement
    {
        public int MovementId { get; set; }

        public int ProductId { get; set; }

        public MovementType MovementType { get; set; }

        public int Quantity { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTime MovementDate { get; set; }

        public string? Notes { get; set; }

        public int TenantId { get; set; }

        
        public Product? Product { get; set; }

        public Tenant? Tenant { get; set; }
    }
}
