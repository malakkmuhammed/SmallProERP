using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    

    public class CreateQuotationItemInlineDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Optional price override — if not provided, uses product SellingPrice
        
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ — line item returned inside QuotationDto
    // ─────────────────────────────────────────────────────────────────────────
    public class QuotationItemDto
    {
        public int QuotationItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ — full quotation response including all items
    // ─────────────────────────────────────────────────────────────────────────
    public class QuotationDto
    {
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompany { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public QuotationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        // Line items included in every quotation response
        public List<QuotationItemDto> Items { get; set; } = new();
        public int ItemCount { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SUMMARY — lightweight list item (no items array)
    // ─────────────────────────────────────────────────────────────────────────
    public class QuotationSummaryDto
    {
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompany { get; set; }
        public QuotationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE — one request with items array
    // ─────────────────────────────────────────────────────────────────────────
    public class CreateQuotationDto
    {
        [Required(ErrorMessage = "Quotation number is required.")]
        [MaxLength(50, ErrorMessage = "Quotation number must not exceed 50 characters.")]
        public string QuotationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Tax amount must be 0 or greater.")]
        public decimal TaxAmount { get; set; }

        [Required(ErrorMessage = "Quotation date is required.")]
        public DateTime QuotationDate { get; set; }

        public DateTime? ValidUntil { get; set; }

        // Items required — a quotation must have at least one line item
        [Required(ErrorMessage = "At least one item is required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<CreateQuotationItemInlineDto> Items { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE — header fields only
    // QuotationNumber excluded — cannot change after creation.
    // Status excluded — use PATCH /status.
    // ─────────────────────────────────────────────────────────────────────────
    public class UpdateQuotationDto
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Tax amount must be 0 or greater.")]
        public decimal TaxAmount { get; set; }

        [Required(ErrorMessage = "Quotation date is required.")]
        public DateTime QuotationDate { get; set; }

        public DateTime? ValidUntil { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS CHANGE — PATCH /api/quotations/{id}/status
    // ─────────────────────────────────────────────────────────────────────────
    public class ChangeQuotationStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [Range(1, 4, ErrorMessage = "Status must be between 1 (Draft) and 4 (Rejected).")]
        public int Status { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADD ITEM — POST /api/quotations/{id}/items
    // ─────────────────────────────────────────────────────────────────────────
    public class AddQuotationItemDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Optional — defaults to product SellingPrice if not provided
       
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE ITEM — PUT /api/quotations/{id}/items/{itemId}
    // ─────────────────────────────────────────────────────────────────────────
    public class UpdateQuotationItemInlineDto
    {
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Optional — keeps existing price if not provided
        public decimal? UnitPrice { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATISTICS
    // ─────────────────────────────────────────────────────────────────────────
    public class QuotationStatisticsDto
    {
        public int TotalQuotations { get; set; }
        public int DraftCount { get; set; }
        public int SentCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AcceptedValue { get; set; }
        public decimal PendingValue { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CONVERT TO SALE
    // ─────────────────────────────────────────────────────────────────────────
    public class ConvertQuotationToSaleDto
    {
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentNotes { get; set; }
    }
}
