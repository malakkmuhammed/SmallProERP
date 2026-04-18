using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.AIDtos
{

    public class OcrResultDto
    {
        public int OcrResultId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string? RawText { get; set; }
        public OcrExtractedDataDto? ExtractedData { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }

    /// <summary>Structured invoice fields extracted during OCR simulation.</summary>
    public class OcrExtractedDataDto
    {
        public string? InvoiceNumber { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<OcrLineItemDto> LineItems { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class OcrLineItemDto
    {
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? LineTotal { get; set; }
    }

    public class OcrSummaryDto
    {
        public int OcrResultId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }


    public class GenerateInsightRequestDto
    {
        [Required]
        public string InsightType { get; set; } = "Full";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>Full AI insight result including raw metrics.</summary>
    public class AiInsightDto
    {
        public int InsightLogId { get; set; }
        public string InsightType { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string InsightText { get; set; } = string.Empty;
        public BusinessMetricsDto? Metrics { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }

    /// <summary>Lightweight insight used in list endpoints.</summary>
    public class AiInsightSummaryDto
    {
        public int InsightLogId { get; set; }
        public string InsightType { get; set; } = string.Empty;
        public string InsightText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }


    public class BusinessMetricsDto
    {
        // Revenue
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int UnpaidInvoices { get; set; }
        public int OverdueInvoices { get; set; }

        // Inventory
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }

        // Customers
        public List<TopCustomerMetricDto> TopCustomers { get; set; } = new();
    }

    public class TopCustomerMetricDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int InvoiceCount { get; set; }
    }
}