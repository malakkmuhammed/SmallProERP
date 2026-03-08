using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class Sale
    {
        public int SaleId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public int? QuotationId { get; set; }

        public int CustomerId { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidDate { get; set; }

        public string? PaymentMethod { get; set; }

        public string? PaymentNotes { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int TenantId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        
        public Quotation? Quotation { get; set; }

        public Customer? Customer { get; set; }

        public User? Creator { get; set; }

        public Tenant? Tenant { get; set; }

        public List<SaleItem>? SaleItems { get; set; }
    }
}
