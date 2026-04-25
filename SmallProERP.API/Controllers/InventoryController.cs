
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
    [Authorize]
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
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetAllMovements()
        {
            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetAllMovementsAsync(tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/product/{productId:int}")]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetMovementsByProduct(int productId)
        {
            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetMovementsByProductAsync(productId, tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/type/{type}")]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<ActionResult<IEnumerable<InventoryMovementDto>>> GetMovementsByType(string type)
        {
            
            if (int.TryParse(type, out _))
            {
                return BadRequest(new
                {
                    message = "Movement type must be text (Purchase, Sale, Adjustment), not a number"
                });
            }

          
            if (!Enum.TryParse<MovementType>(type, true, out var movementType))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(MovementType)));
                return BadRequest(new
                {
                    message = $"Invalid movement type '{type}'. Valid types are: {validTypes}"
                });
            }

            
            if (!Enum.IsDefined(typeof(MovementType), movementType))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(MovementType)));
                return BadRequest(new
                {
                    message = $"Invalid movement type. Valid types are: {validTypes}"
                });
            }

            var tenantId = GetTenantId();
            var movements = await _inventoryService.GetMovementsByTypeAsync(movementType, tenantId);
            return Ok(movements);
        }

        [HttpGet("movements/date-range")]
        [Authorize(Roles = "Admin,InventoryManager")]
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
