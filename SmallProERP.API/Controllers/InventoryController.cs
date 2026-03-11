
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.API.Helpers;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.InventoryDtos;
using SmallProERP.Models.Enums;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    //[Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        private int GetTenantId()
        {
            // ⭐ TEST MODE: Return hardcoded value
            if (TestHelper.IsTestMode)
            {
                return TestHelper.TestTenantId;  // Returns 1
            }

            // ⭐ PRODUCTION MODE: Extract from token
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim == null)
                throw new UnauthorizedAccessException("TenantId not found in token");

            return int.Parse(tenantIdClaim.Value);
        }

        [HttpGet("movements")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetAllMovements()
        {
            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetAllMovementsAsync(tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/product/{productId:int}")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetMovementsByProduct(int productId)
        {
            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetMovementsByProductAsync(productId, tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/type/{type:int}")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetMovementsByType(int type)
        {
            if (!Enum.IsDefined(typeof(MovementType), type))
                return BadRequest(new { message = "Invalid movement type" });

            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetMovementsByTypeAsync((MovementType)type, tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/date-range")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetMovementsByDateRange(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest(new { message = "From date cannot be after To date" });

            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetMovementsByDateRangeAsync(from, to, tenantId);
            return Ok(movements);
        }
    }
}
