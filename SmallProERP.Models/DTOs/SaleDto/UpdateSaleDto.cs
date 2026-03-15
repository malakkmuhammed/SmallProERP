using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    public class UpdateSaleDto
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        public int? QuotationId { get; set; }

        [Required(ErrorMessage = "Subtotal is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Subtotal must be 0 or greater.")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Tax amount must be 0 or greater.")]
        public decimal TaxAmount { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Total amount must be 0 or greater.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Invoice date is required.")]
        public DateTime InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(50, ErrorMessage = "Payment method must not exceed 50 characters.")]
        public string? PaymentMethod { get; set; }

        [MaxLength(500, ErrorMessage = "Payment notes must not exceed 500 characters.")]
        public string? PaymentNotes { get; set; }
    }
}
