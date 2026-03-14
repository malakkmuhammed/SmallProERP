using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmallProERP.Models.DTOs.QuotationItemDtos;  // ✅ works

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class QuotationDetailsDto
    {
        // ── Quotation header 
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public QuotationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        // ── Customer info (resolved from navigation) 
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerCompany { get; set; }

        // ── Financial totals 
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // ── Line items 
        public IEnumerable<QuotationItemDtos.QuotationItemDto> Items { get; set; }
                   = Enumerable.Empty<QuotationItemDtos.QuotationItemDto>();

        public int ItemCount { get; set; }
        
    }
}
