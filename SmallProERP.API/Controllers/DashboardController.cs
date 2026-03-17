
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.DashboardDtos;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private int GetTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim == null)
                throw new UnauthorizedAccessException("TenantId not found in token");

            return int.Parse(tenantIdClaim.Value);
        }

        

        [HttpGet("inventory-overview")]
        public async Task<ActionResult<InventoryOverviewDto>> GetInventoryOverview()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetInventoryOverviewAsync(tenantId));
        }

        [HttpGet("purchase-orders-overview")]
        public async Task<ActionResult<PurchaseOrdersOverviewDto>> GetPurchaseOrdersOverview()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetPurchaseOrdersOverviewAsync(tenantId));
        }

        [HttpGet("suppliers-overview")]
        public async Task<ActionResult<SuppliersOverviewDto>> GetSuppliersOverview()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetSuppliersOverviewAsync(tenantId));
        }

        [HttpGet("low-stock-alerts")]
        public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlerts()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetLowStockAlertsAsync(tenantId));
        }

        [HttpGet("recent-activities")]
        public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetRecentActivities(
            [FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetRecentActivitiesAsync(tenantId, limit));
        }

        [HttpGet("inventory-by-category")]
        public async Task<ActionResult<InventoryByCategoryDto>> GetInventoryByCategory()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetInventoryByCategoryAsync(tenantId));
        }

        [HttpGet("purchase-trends")]
        public async Task<ActionResult<PurchaseTrendsDto>> GetPurchaseTrends(
            [FromQuery] int months = 6)
        {
            if (months < 1 || months > 24)
                return BadRequest(new { message = "Months must be between 1 and 24." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetPurchaseTrendsAsync(tenantId, months));
        }

        [HttpGet("top-suppliers")]
        public async Task<ActionResult<IEnumerable<TopSupplierDto>>> GetTopSuppliers(
            [FromQuery] int limit = 5)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(new { message = "Limit must be between 1 and 50." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetTopSuppliersAsync(tenantId, limit));
        }

        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
            [FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetTopProductsAsync(tenantId, limit));
        }

        [HttpGet("movements-by-type")]
        public async Task<ActionResult<MovementsByTypeDto>> GetMovementsByType()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetMovementsByTypeAsync(tenantId));
        }

        

       
        [HttpGet("sales-overview")]
        public async Task<ActionResult<SalesOverviewDto>> GetSalesOverview()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetSalesOverviewAsync(tenantId));
        }

       
        [HttpGet("top-customers")]
        public async Task<ActionResult<IEnumerable<TopCustomerDto>>> GetTopCustomers(
            [FromQuery] int limit = 5)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(new { message = "Limit must be between 1 and 50." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetTopCustomersAsync(tenantId, limit));
        }

    
        [HttpGet("sales-trends")]
        public async Task<ActionResult<SalesTrendsDto>> GetSalesTrends(
            [FromQuery] int months = 6)
        {
            if (months < 1 || months > 24)
                return BadRequest(new { message = "Months must be between 1 and 24." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetSalesTrendsAsync(tenantId, months));
        }

      
        [HttpGet("crm-pipeline")]
        public async Task<ActionResult<CrmPipelineDto>> GetCrmPipeline()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetCrmPipelineAsync(tenantId));
        }

     
        [HttpGet("recent-invoices")]
        public async Task<ActionResult<IEnumerable<RecentInvoiceDto>>> GetRecentInvoices(
            [FromQuery] int limit = 5)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(new { message = "Limit must be between 1 and 50." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetRecentInvoicesAsync(tenantId, limit));
        }

    
        [HttpGet("sales-comparison")]
        public async Task<ActionResult<SalesComparisonDto>> GetSalesComparison()
        {
            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetSalesComparisonAsync(tenantId));
        }

      
        [HttpGet("best-selling-products")]
        public async Task<ActionResult<IEnumerable<BestSellingProductDto>>> GetBestSellingProducts(
            [FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100." });

            var tenantId = GetTenantId();
            return Ok(await _dashboardService.GetBestSellingProductsAsync(tenantId, limit));
        }

    }
}