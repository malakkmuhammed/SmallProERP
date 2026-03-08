using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Quotation
    {
        public int QuotationId { get; set; }

        public string QuotationNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public QuotationStatus Status { get; set; }

        public DateTime QuotationDate { get; set; }

        public DateTime? ValidUntil { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        
        public Customer? Customer { get; set; }

        public User? Creator { get; set; }

        public Tenant? Tenant { get; set; }

        public List<QuotationItem>? QuotationItems { get; set; }

        public List<Sale>? Sales { get; set; }
    }
}
