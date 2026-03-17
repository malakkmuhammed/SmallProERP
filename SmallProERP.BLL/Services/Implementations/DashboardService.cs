

using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.DashboardDtos;
using SmallProERP.Models.Enums;

namespace SmallProERP.BLL.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly SmallProDbContext _context;

        public DashboardService(SmallProDbContext context)
        {
            _context = context;
        }

        

        #region Overview Cards

        public async Task<InventoryOverviewDto> GetInventoryOverviewAsync(int tenantId)
        {
            var products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .ToListAsync();

            return new InventoryOverviewDto
            {
                TotalProducts = products.Count,
                TotalInventoryValue = products.Sum(p => p.Quantity * p.PurchasePrice),
                LowStockCount = products.Count(p => p.Quantity < p.MinimumStockLevel),
                OutOfStockCount = products.Count(p => p.Quantity == 0)
            };
        }

        public async Task<PurchaseOrdersOverviewDto> GetPurchaseOrdersOverviewAsync(int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .ToListAsync();

            return new PurchaseOrdersOverviewDto
            {
                TotalPurchaseOrders = purchaseOrders.Count,
                DraftCount = purchaseOrders.Count(po => po.Status == POStatus.Draft),
                SentCount = purchaseOrders.Count(po => po.Status == POStatus.Sent),
                ReceivedCount = purchaseOrders.Count(po => po.Status == POStatus.Received),
                TotalAmountAllTime = purchaseOrders.Sum(po => po.TotalAmount),
                PendingPOsValue = purchaseOrders.Where(po => po.Status == POStatus.Sent)
                                                    .Sum(po => po.TotalAmount)
            };
        }

        public async Task<SuppliersOverviewDto> GetSuppliersOverviewAsync(int tenantId)
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .ToListAsync();

            var activeSuppliers = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .Select(po => po.SupplierId)
                .Distinct()
                .CountAsync();

            var totalProducts = await _context.Products
                .Where(p => p.TenantId == tenantId && p.SupplierId != null)
                .CountAsync();

            var totalSpent = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .SumAsync(po => po.TotalAmount);

            return new SuppliersOverviewDto
            {
                TotalSuppliers = suppliers.Count,
                ActiveSuppliersCount = activeSuppliers,
                TotalProductsFromSuppliers = totalProducts,
                TotalSpent = totalSpent
            };
        }

        #endregion

        #region Alerts

        public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int tenantId)
        {
            var lowStockProducts = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.Quantity < p.MinimumStockLevel)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            return lowStockProducts.Select(p => new LowStockAlertDto
            {
                ProductId = p.ProductId,
                ProductName = p.Name,
                ProductCode = p.ProductCode,
                Category = p.Category,
                CurrentQuantity = p.Quantity,
                MinimumLevel = p.MinimumStockLevel,
                Deficit = p.MinimumStockLevel - p.Quantity,
                SupplierName = p.Supplier?.Name
            });
        }

        #endregion

        #region Recent Activities

        public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(
            int tenantId, int limit)
        {
            var movements = await _context.InventoryMovements
                .Include(im => im.Product)
                .Where(im => im.TenantId == tenantId)
                .OrderByDescending(im => im.MovementDate)
                .Take(limit)
                .ToListAsync();

            return movements.Select(m => new RecentActivityDto
            {
                MovementId = m.MovementId,
                MovementType = m.MovementType.ToString(),
                ProductName = m.Product?.Name ?? string.Empty,
                ProductCode = m.Product?.ProductCode ?? string.Empty,
                Quantity = m.Quantity,
                ReferenceNumber = m.ReferenceNumber,
                MovementDate = m.MovementDate,
                Notes = m.Notes
            });
        }

        #endregion

        #region Charts

        public async Task<InventoryByCategoryDto> GetInventoryByCategoryAsync(int tenantId)
        {
            var productsByCategory = await _context.Products
                .Where(p => p.TenantId == tenantId && p.Category != null)
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Value = g.Sum(p => p.Quantity * p.PurchasePrice),
                    ProductCount = g.Count()
                })
                .ToListAsync();

            var totalValue = productsByCategory.Sum(c => c.Value);

            var categories = productsByCategory.Select(c => new CategoryValueDto
            {
                Name = c.Category ?? "Uncategorized",
                Value = c.Value,
                ProductCount = c.ProductCount,
                Percentage = totalValue > 0
                    ? Math.Round((double)(c.Value / totalValue * 100), 2) : 0
            }).OrderByDescending(c => c.Value).ToList();

            return new InventoryByCategoryDto { Categories = categories };
        }

        public async Task<PurchaseTrendsDto> GetPurchaseTrendsAsync(int tenantId, int months)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);

            var orders = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId && po.OrderDate >= startDate)
                .ToListAsync();

            var monthlyData = orders
                .GroupBy(po => new { po.OrderDate.Year, po.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(po => po.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Year).ThenBy(d => d.Month)
                .ToList();

            var monthNames = new List<string>();
            var amounts = new List<decimal>();
            var counts = new List<int>();

            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var monthData = monthlyData.FirstOrDefault(
                    d => d.Year == date.Year && d.Month == date.Month);

                monthNames.Add(date.ToString("MMM yyyy"));
                amounts.Add(monthData?.TotalAmount ?? 0);
                counts.Add(monthData?.OrderCount ?? 0);
            }

            return new PurchaseTrendsDto { Months = monthNames, Amounts = amounts, OrderCounts = counts };
        }

        public async Task<IEnumerable<TopSupplierDto>> GetTopSuppliersAsync(
            int tenantId, int limit)
        {
            return await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .Select(s => new TopSupplierDto
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    TotalSpent = s.PurchaseOrders!.Sum(po => po.TotalAmount),
                    OrderCount = s.PurchaseOrders!.Count(),
                    ProductCount = s.Products!.Count()
                })
                .OrderByDescending(s => s.TotalSpent)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(
            int tenantId, int limit)
        {
            return await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.Quantity)
                .Take(limit)
                .Select(p => new TopProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    ProductCode = p.ProductCode,
                    Category = p.Category,
                    Quantity = p.Quantity,
                    TotalValue = p.Quantity * p.PurchasePrice,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : null
                })
                .ToListAsync();
        }

        public async Task<MovementsByTypeDto> GetMovementsByTypeAsync(int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Where(im => im.TenantId == tenantId)
                .GroupBy(im => im.MovementType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalMovements = movements.Sum(m => m.Count);

            var types = movements.Select(m => new MovementTypeCountDto
            {
                Type = m.Type.ToString(),
                Count = m.Count,
                Percentage = totalMovements > 0
                    ? Math.Round((double)m.Count / totalMovements * 100, 2) : 0
            }).OrderByDescending(m => m.Count).ToList();

            return new MovementsByTypeDto { Types = types };
        }

        #endregion

       

        #region Sales Overview

        public async Task<SalesOverviewDto> GetSalesOverviewAsync(int tenantId)
        {
            var now = DateTime.UtcNow;
            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .ToListAsync();

            var paid = sales.Where(s => s.IsPaid).ToList();
            var unpaid = sales.Where(s => !s.IsPaid).ToList();
            var overdue = unpaid
                .Where(s => s.DueDate.HasValue && s.DueDate.Value < now)
                .ToList();

            return new SalesOverviewDto
            {
                TotalInvoices = sales.Count,
                PaidCount = paid.Count,
                UnpaidCount = unpaid.Count,
                OverdueCount = overdue.Count,
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                CollectedRevenue = paid.Sum(s => s.TotalAmount),
                OutstandingAmount = unpaid.Sum(s => s.TotalAmount),
                OverdueAmount = overdue.Sum(s => s.TotalAmount)
            };
        }

        #endregion

        #region Top Customers

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(
            int tenantId, int limit)
        {
            var customerSales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .ToListAsync();

            var grouped = customerSales
                .GroupBy(s => s.CustomerId)
                .Select(g =>
                {
                    var customer = g.First().Customer;
                    return new TopCustomerDto
                    {
                        CustomerId = g.Key,
                        CustomerName = customer?.Name ?? string.Empty,
                        CustomerCompany = customer?.Company,
                        InvoiceCount = g.Count(),
                        TotalRevenue = g.Sum(s => s.TotalAmount),
                        PaidRevenue = g.Where(s => s.IsPaid).Sum(s => s.TotalAmount),
                        LastInvoiceDate = g.Max(s => (DateTime?)s.InvoiceDate)
                    };
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(limit)
                .ToList();

            return grouped;
        }

        #endregion

        #region Sales Trends

        public async Task<SalesTrendsDto> GetSalesTrendsAsync(int tenantId, int months)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);

            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId
                         && s.InvoiceDate >= startDate)
                .ToListAsync();

            var monthlyData = sales
                .GroupBy(s => new { s.InvoiceDate.Year, s.InvoiceDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(s => s.TotalAmount),
                    CollectedAmount = g.Where(s => s.IsPaid).Sum(s => s.TotalAmount),
                    InvoiceCount = g.Count()
                })
                .ToList();

            var monthNames = new List<string>();
            var totalAmounts = new List<decimal>();
            var collectedAmounts = new List<decimal>();
            var invoiceCounts = new List<int>();

            // Fill all months including ones with no data (show as 0)
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var monthData = monthlyData.FirstOrDefault(
                    d => d.Year == date.Year && d.Month == date.Month);

                monthNames.Add(date.ToString("MMM yyyy"));
                totalAmounts.Add(monthData?.TotalAmount ?? 0);
                collectedAmounts.Add(monthData?.CollectedAmount ?? 0);
                invoiceCounts.Add(monthData?.InvoiceCount ?? 0);
            }

            return new SalesTrendsDto
            {
                Months = monthNames,
                TotalAmounts = totalAmounts,
                CollectedAmounts = collectedAmounts,
                InvoiceCounts = invoiceCounts
            };
        }

        #endregion

        #region CRM Pipeline

        public async Task<CrmPipelineDto> GetCrmPipelineAsync(int tenantId)
        {
            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .ToListAsync();

            var wonCount = customers.Count(c => c.Status == CustomerStatus.Won);
            var lostCount = customers.Count(c => c.Status == CustomerStatus.Lost);

            // Conversion rate = Won / (Won + Lost) — only counts closed deals
            double conversionRate = (wonCount + lostCount) > 0
                ? Math.Round((double)wonCount / (wonCount + lostCount) * 100, 2)
                : 0;

            return new CrmPipelineDto
            {
                TotalCustomers = customers.Count,
                NewLeadCount = customers.Count(c => c.Status == CustomerStatus.NewLead),
                InterestedCount = customers.Count(c => c.Status == CustomerStatus.Interested),
                OpportunityCount = customers.Count(c => c.Status == CustomerStatus.Opportunity),
                WonCount = wonCount,
                LostCount = lostCount,
                ConversionRate = conversionRate
            };
        }

        #endregion

        #region Recent Invoices

        public async Task<IEnumerable<RecentInvoiceDto>> GetRecentInvoicesAsync(
            int tenantId, int limit)
        {
            var now = DateTime.UtcNow;
            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return sales.Select(s => new RecentInvoiceDto
            {
                SaleId = s.SaleId,
                InvoiceNumber = s.InvoiceNumber,
                CustomerName = s.Customer?.Name ?? string.Empty,
                TotalAmount = s.TotalAmount,
                IsPaid = s.IsPaid,
                InvoiceDate = s.InvoiceDate,
                DueDate = s.DueDate,
                IsOverdue = !s.IsPaid && s.DueDate.HasValue && s.DueDate.Value < now
            });
        }

        #endregion
       

        #region Sales Comparison

        public async Task<SalesComparisonDto> GetSalesComparisonAsync(int tenantId)
        {
            var now = DateTime.UtcNow;

            // This month: from 1st of current month to now
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Last month: from 1st to last day of previous month
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddSeconds(-1);

            var allSales = await _context.Sales
                .Where(s => s.TenantId == tenantId
                         && s.InvoiceDate >= lastMonthStart)
                .ToListAsync();

            var thisMonthSales = allSales.Where(s => s.InvoiceDate >= thisMonthStart).ToList();
            var lastMonthSales = allSales.Where(s => s.InvoiceDate >= lastMonthStart
                                                  && s.InvoiceDate <= lastMonthEnd).ToList();

            var thisMonth = BuildMonthDto(thisMonthSales, now.ToString("MMMM yyyy"));
            var lastMonth = BuildMonthDto(lastMonthSales, lastMonthStart.ToString("MMMM yyyy"));

            // Calculate growth
            decimal revenueGrowth = thisMonth.TotalRevenue - lastMonth.TotalRevenue;

            double revenueGrowthPercent = lastMonth.TotalRevenue > 0
                ? Math.Round((double)(revenueGrowth / lastMonth.TotalRevenue * 100), 2)
                : (thisMonth.TotalRevenue > 0 ? 100 : 0);

            int invoiceCountDiff = thisMonth.InvoiceCount - lastMonth.InvoiceCount;

            double invoiceGrowthPercent = lastMonth.InvoiceCount > 0
                ? Math.Round((double)invoiceCountDiff / lastMonth.InvoiceCount * 100, 2)
                : (thisMonth.InvoiceCount > 0 ? 100 : 0);

            return new SalesComparisonDto
            {
                ThisMonth = thisMonth,
                LastMonth = lastMonth,
                RevenueGrowthAmount = revenueGrowth,
                RevenueGrowthPercent = revenueGrowthPercent,
                InvoiceCountDiff = invoiceCountDiff,
                InvoiceCountGrowthPercent = invoiceGrowthPercent
            };
        }

        // Helper — builds a MonthSalesDto from a list of sales
        private static MonthSalesDto BuildMonthDto(
            List<SmallProERP.Models.Entities.Sale> sales, string monthName)
        {
            var paid = sales.Where(s => s.IsPaid).ToList();
            var unpaid = sales.Where(s => !s.IsPaid).ToList();

            return new MonthSalesDto
            {
                MonthName = monthName,
                InvoiceCount = sales.Count,
                PaidCount = paid.Count,
                UnpaidCount = unpaid.Count,
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                CollectedRevenue = paid.Sum(s => s.TotalAmount),
                OutstandingAmount = unpaid.Sum(s => s.TotalAmount)
            };
        }

        #endregion

        #region Best Selling Products

        public async Task<IEnumerable<BestSellingProductDto>> GetBestSellingProductsAsync(
            int tenantId, int limit)
        {
         
            var soldItems = await _context.SaleItems
                .Where(si => si.TenantId == tenantId
                          && si.Sale != null
                          && si.Sale.IsPaid == true)
                .Include(si => si.Product)
                .ToListAsync();

            var grouped = soldItems
                .GroupBy(si => si.ProductId)
                .Select(g =>
                {
                    var product = g.First().Product;
                    return new BestSellingProductDto
                    {
                        ProductId = g.Key,
                        ProductName = product?.Name ?? string.Empty,
                        ProductCode = product?.ProductCode ?? string.Empty,
                        Category = product?.Category,
                        TotalQuantitySold = g.Sum(si => si.Quantity),
                        TotalRevenue = g.Sum(si => si.LineTotal),
                        CurrentStock = product?.Quantity ?? 0,
                        IsLowStock = product != null
                                             && product.Quantity < product.MinimumStockLevel
                    };
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(limit)
                .ToList();

            return grouped;
        }

        #endregion

    }
}
