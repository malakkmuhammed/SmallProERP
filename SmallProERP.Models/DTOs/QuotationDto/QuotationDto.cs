using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class QuotationDto
    {
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public QuotationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;   // e.g. "Draft", "Sent"
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}
