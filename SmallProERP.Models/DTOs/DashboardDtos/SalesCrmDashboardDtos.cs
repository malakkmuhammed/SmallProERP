using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.DashboardDtos
{
   
    public class SalesOverviewDto
    {
        public int TotalInvoices { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int OverdueCount { get; set; }           // unpaid + past DueDate
        public decimal TotalRevenue { get; set; }       // sum of all invoices
        public decimal CollectedRevenue { get; set; }   // sum of paid invoices
        public decimal OutstandingAmount { get; set; }  // sum of unpaid invoices
        public decimal OverdueAmount { get; set; }      // sum of overdue invoices
    }

   
    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompany { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalRevenue { get; set; }       // sum of all their invoices
        public decimal PaidRevenue { get; set; }        // sum of paid invoices only
        public DateTime? LastInvoiceDate { get; set; }
    }

   
    public class SalesTrendsDto
    {
        public List<string> Months { get; set; } = new();
        public List<decimal> TotalAmounts { get; set; } = new();    // all invoices
        public List<decimal> CollectedAmounts { get; set; } = new(); // paid only
        public List<int> InvoiceCounts { get; set; } = new();
    }

   
    public class CrmPipelineDto
    {
        public int TotalCustomers { get; set; }
        public int NewLeadCount { get; set; }
        public int InterestedCount { get; set; }
        public int OpportunityCount { get; set; }
        public int WonCount { get; set; }
        public int LostCount { get; set; }

        // Conversion rate: Won / (Won + Lost) * 100
        public double ConversionRate { get; set; }
    }


    public class RecentInvoiceDto
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    
    public class SalesComparisonDto
    {
        public MonthSalesDto ThisMonth { get; set; } = new();
        public MonthSalesDto LastMonth { get; set; } = new();

        // Growth indicators (positive = growth, negative = decline)
        public decimal RevenueGrowthAmount { get; set; }    // ThisMonth - LastMonth
        public double RevenueGrowthPercent { get; set; }    // % change in revenue
        public int InvoiceCountDiff { get; set; }           // difference in invoice count
        public double InvoiceCountGrowthPercent { get; set; }
    }

    public class MonthSalesDto
    {
        public string MonthName { get; set; } = string.Empty;   // e.g. "March 2026"
        public int InvoiceCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal OutstandingAmount { get; set; }
    }


    public class BestSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalQuantitySold { get; set; }    // sum of qty across all paid sale items
        public decimal TotalRevenue { get; set; }      // sum of LineTotal across paid sale items
        public int CurrentStock { get; set; }          // current product quantity
        public bool IsLowStock { get; set; }           // quantity < minimumStockLevel
    }
}
