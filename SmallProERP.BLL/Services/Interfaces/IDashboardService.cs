using SmallProERP.Models.DTOs.DashboardDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        // Overview Cards
        Task<InventoryOverviewDto> GetInventoryOverviewAsync(int tenantId);
        Task<PurchaseOrdersOverviewDto> GetPurchaseOrdersOverviewAsync(int tenantId);
        Task<SuppliersOverviewDto> GetSuppliersOverviewAsync(int tenantId);

        // Alerts
        Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int tenantId);

        // Recent Activities
        Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int tenantId, int limit);

        // Charts
        Task<InventoryByCategoryDto> GetInventoryByCategoryAsync(int tenantId);
        Task<PurchaseTrendsDto> GetPurchaseTrendsAsync(int tenantId, int months);
        Task<IEnumerable<TopSupplierDto>> GetTopSuppliersAsync(int tenantId, int limit);
        Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int tenantId, int limit);
        Task<MovementsByTypeDto> GetMovementsByTypeAsync(int tenantId);
    }
}
