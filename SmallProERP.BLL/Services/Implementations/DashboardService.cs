

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

            var totalValue = products.Sum(p => p.Quantity * p.PurchasePrice);
            var lowStockCount = products.Count(p => p.Quantity < p.MinimumStockLevel);
            var outOfStockCount = products.Count(p => p.Quantity == 0);

            return new InventoryOverviewDto
            {
                TotalProducts = products.Count,
                TotalInventoryValue = totalValue,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount
            };
        }

        public async Task<PurchaseOrdersOverviewDto> GetPurchaseOrdersOverviewAsync(int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .ToListAsync();

            var draftCount = purchaseOrders.Count(po => po.Status == POStatus.Draft);
            var sentCount = purchaseOrders.Count(po => po.Status == POStatus.Sent);
            var receivedCount = purchaseOrders.Count(po => po.Status == POStatus.Received);
            var totalAmount = purchaseOrders.Sum(po => po.TotalAmount);
            var pendingValue = purchaseOrders
                .Where(po => po.Status == POStatus.Sent)
                .Sum(po => po.TotalAmount);

            return new PurchaseOrdersOverviewDto
            {
                TotalPurchaseOrders = purchaseOrders.Count,
                DraftCount = draftCount,
                SentCount = sentCount,
                ReceivedCount = receivedCount,
                TotalAmountAllTime = totalAmount,
                PendingPOsValue = pendingValue
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

        public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int tenantId, int limit)
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
                ProductName = m.Product?.Name ?? "",
                ProductCode = m.Product?.ProductCode ?? "",
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
                Percentage = totalValue > 0 ? Math.Round((double)(c.Value / totalValue * 100), 2) : 0
            }).OrderByDescending(c => c.Value).ToList();

            return new InventoryByCategoryDto
            {
                Categories = categories
            };
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
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            var monthNames = new List<string>();
            var amounts = new List<decimal>();
            var counts = new List<int>();

            // Fill in missing months with zeros
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var monthData = monthlyData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);

                monthNames.Add(date.ToString("MMM yyyy"));
                amounts.Add(monthData?.TotalAmount ?? 0);
                counts.Add(monthData?.OrderCount ?? 0);
            }

            return new PurchaseTrendsDto
            {
                Months = monthNames,
                Amounts = amounts,
                OrderCounts = counts
            };
        }

        public async Task<IEnumerable<TopSupplierDto>> GetTopSuppliersAsync(int tenantId, int limit)
        {
            var supplierStats = await _context.Suppliers
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

            return supplierStats;
        }

        public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int tenantId, int limit)
        {
            var topProducts = await _context.Products
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

            return topProducts;
        }

        public async Task<MovementsByTypeDto> GetMovementsByTypeAsync(int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Where(im => im.TenantId == tenantId)
                .GroupBy(im => im.MovementType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalMovements = movements.Sum(m => m.Count);

            var types = movements.Select(m => new MovementTypeCountDto
            {
                Type = m.Type.ToString(),
                Count = m.Count,
                Percentage = totalMovements > 0 ? Math.Round((double)m.Count / totalMovements * 100, 2) : 0
            }).OrderByDescending(m => m.Count).ToList();

            return new MovementsByTypeDto
            {
                Types = types
            };
        }

        #endregion
    }
}
