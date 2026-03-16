using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    public class CreateSaleDto
    {
        [Required(ErrorMessage = "Invoice number is required.")]
        [MaxLength(50, ErrorMessage = "Invoice number must not exceed 50 characters.")]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int? QuotationId { get; set; } 
           

        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Tax amount must be 0 or greater.")]
        public decimal TaxAmount { get; set; }

        [Required(ErrorMessage = "Invoice date is required.")]
        public DateTime InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(50, ErrorMessage = "Payment method must not exceed 50 characters.")]
        public string? PaymentMethod { get; set; }

        [MaxLength(500, ErrorMessage = "Payment notes must not exceed 500 characters.")]
        public string? PaymentNotes { get; set; }

        // Items are required — a sale must have at least one line item
        [Required(ErrorMessage = "At least one item is required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<CreateSaleItemInlineDto> Items { get; set; } = new();
    }

    public class CreateSaleItemInlineDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
