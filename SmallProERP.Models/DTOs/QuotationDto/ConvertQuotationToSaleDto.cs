using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class ConvertQuotationToSaleDto
    {
        // Optional overrides — if not provided, values are copied from the quotation
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentNotes { get; set; }
    }
}
