// SmallProERP.API/Controllers/DashboardController.cs

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

        // GET /api/dashboard/inventory-overview
        [HttpGet("inventory-overview")]
        public async Task<ActionResult<InventoryOverviewDto>> GetInventoryOverview()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetInventoryOverviewAsync(tenantId);
            return Ok(data);
        }

        // GET /api/dashboard/purchase-orders-overview
        [HttpGet("purchase-orders-overview")]
        public async Task<ActionResult<PurchaseOrdersOverviewDto>> GetPurchaseOrdersOverview()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetPurchaseOrdersOverviewAsync(tenantId);
            return Ok(data);
        }

        // GET /api/dashboard/suppliers-overview
        [HttpGet("suppliers-overview")]
        public async Task<ActionResult<SuppliersOverviewDto>> GetSuppliersOverview()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetSuppliersOverviewAsync(tenantId);
            return Ok(data);
        }

        // GET /api/dashboard/low-stock-alerts
        [HttpGet("low-stock-alerts")]
        public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlerts()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetLowStockAlertsAsync(tenantId);
            return Ok(data);
        }

        // GET /api/dashboard/recent-activities?limit=10
        [HttpGet("recent-activities")]
        public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetRecentActivities([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100" });

            var tenantId = GetTenantId();
            var data = await _dashboardService.GetRecentActivitiesAsync(tenantId, limit);
            return Ok(data);
        }

        // GET /api/dashboard/inventory-by-category
        [HttpGet("inventory-by-category")]
        public async Task<ActionResult<InventoryByCategoryDto>> GetInventoryByCategory()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetInventoryByCategoryAsync(tenantId);
            return Ok(data);
        }

        // GET /api/dashboard/purchase-trends?months=6
        [HttpGet("purchase-trends")]
        public async Task<ActionResult<PurchaseTrendsDto>> GetPurchaseTrends([FromQuery] int months = 6)
        {
            if (months < 1 || months > 24)
                return BadRequest(new { message = "Months must be between 1 and 24" });

            var tenantId = GetTenantId();
            var data = await _dashboardService.GetPurchaseTrendsAsync(tenantId, months);
            return Ok(data);
        }

        // GET /api/dashboard/top-suppliers?limit=5
        [HttpGet("top-suppliers")]
        public async Task<ActionResult<IEnumerable<TopSupplierDto>>> GetTopSuppliers([FromQuery] int limit = 5)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(new { message = "Limit must be between 1 and 50" });

            var tenantId = GetTenantId();
            var data = await _dashboardService.GetTopSuppliersAsync(tenantId, limit);
            return Ok(data);
        }

        // GET /api/dashboard/top-products?limit=10
        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100" });

            var tenantId = GetTenantId();
            var data = await _dashboardService.GetTopProductsAsync(tenantId, limit);
            return Ok(data);
        }

        // GET /api/dashboard/movements-by-type
        [HttpGet("movements-by-type")]
        public async Task<ActionResult<MovementsByTypeDto>> GetMovementsByType()
        {
            var tenantId = GetTenantId();
            var data = await _dashboardService.GetMovementsByTypeAsync(tenantId);
            return Ok(data);
        }
    }
}