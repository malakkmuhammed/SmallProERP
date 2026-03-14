using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class CreateQuotationDto
    {
        [Required(ErrorMessage = "Quotation number is required.")]
        [MaxLength(50, ErrorMessage = "Quotation number must not exceed 50 characters.")]
        public string QuotationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Subtotal is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Subtotal must be 0 or greater.")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Tax amount must be 0 or greater.")]
        public decimal TaxAmount { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Total amount must be 0 or greater.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Quotation date is required.")]
        public DateTime QuotationDate { get; set; }

        // Optional — quotation may not have an expiry date
        public DateTime? ValidUntil { get; set; }
    }
}
