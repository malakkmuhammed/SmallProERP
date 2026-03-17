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
        //  Inventory & Purchasing 
        Task<InventoryOverviewDto> GetInventoryOverviewAsync(int tenantId);
        Task<PurchaseOrdersOverviewDto> GetPurchaseOrdersOverviewAsync(int tenantId);
        Task<SuppliersOverviewDto> GetSuppliersOverviewAsync(int tenantId);
        Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int tenantId);
        Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int tenantId, int limit);
        Task<InventoryByCategoryDto> GetInventoryByCategoryAsync(int tenantId);
        Task<PurchaseTrendsDto> GetPurchaseTrendsAsync(int tenantId, int months);
        Task<IEnumerable<TopSupplierDto>> GetTopSuppliersAsync(int tenantId, int limit);
        Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int tenantId, int limit);
        Task<MovementsByTypeDto> GetMovementsByTypeAsync(int tenantId);

        //  Sales & CRM 
        Task<SalesOverviewDto> GetSalesOverviewAsync(int tenantId);
        Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int tenantId, int limit);
        Task<SalesTrendsDto> GetSalesTrendsAsync(int tenantId, int months);
        Task<CrmPipelineDto> GetCrmPipelineAsync(int tenantId);
        Task<IEnumerable<RecentInvoiceDto>> GetRecentInvoicesAsync(int tenantId, int limit);

     
        Task<SalesComparisonDto> GetSalesComparisonAsync(int tenantId);

     
        Task<IEnumerable<BestSellingProductDto>> GetBestSellingProductsAsync(int tenantId, int limit);
    }
}
