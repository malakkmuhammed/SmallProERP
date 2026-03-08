using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? Company { get; set; }

        public string? Address { get; set; }

        public CustomerStatus Status { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        
        public Tenant? Tenant { get; set; }

        public List<CustomerInteraction>? Interactions { get; set; }

        public List<Quotation>? Quotations { get; set; }

        public List<Sale>? Sales { get; set; }
    }
}
