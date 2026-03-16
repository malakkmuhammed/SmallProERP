using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.DashboardDtos
{
    public class InventoryOverviewDto
    {
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class PurchaseOrdersOverviewDto
    {
        public int TotalPurchaseOrders { get; set; }
        public int DraftCount { get; set; }
        public int SentCount { get; set; }
        public int ReceivedCount { get; set; }
        public decimal TotalAmountAllTime { get; set; }
        public decimal PendingPOsValue { get; set; }
    }

    public class SuppliersOverviewDto
    {
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliersCount { get; set; }
        public int TotalProductsFromSuppliers { get; set; }
        public decimal TotalSpent { get; set; }
    }

    

    public class LowStockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumLevel { get; set; }
        public int Deficit { get; set; }
        public string? SupplierName { get; set; }
    }

   

    public class RecentActivityDto
    {
        public int MovementId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime MovementDate { get; set; }
        public string? Notes { get; set; }
    }

   

    public class InventoryByCategoryDto
    {
        public List<CategoryValueDto> Categories { get; set; } = new();
    }

    public class CategoryValueDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int ProductCount { get; set; }
        public double Percentage { get; set; }
    }

    public class PurchaseTrendsDto
    {
        public List<string> Months { get; set; } = new();
        public List<decimal> Amounts { get; set; } = new();
        public List<int> OrderCounts { get; set; } = new();
    }

    public class TopSupplierDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public string? SupplierName { get; set; }
    }

    public class MovementsByTypeDto
    {
        public List<MovementTypeCountDto> Types { get; set; } = new();
    }

    public class MovementTypeCountDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
