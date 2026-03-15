using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        
        private int GetTenantId()
        {
            var claim = User.FindFirst("TenantId")?.Value;
            return int.TryParse(claim, out int tenantId) ? tenantId : 0;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int userId) ? userId : null;
        }

    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll(
            [FromQuery] string? search = null)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var sales = await _saleService.GetAllAsync(tenantId, search);
            return Ok(sales);
        }

       
        [HttpGet("statistics")]
        public async Task<ActionResult<SaleStatisticsDto>> GetStatistics()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var statistics = await _saleService.GetStatisticsAsync(tenantId);
            return Ok(statistics);
        }

    
        [HttpGet("unpaid-alerts")]
        public async Task<ActionResult<IEnumerable<UnpaidInvoiceAlertDto>>> GetUnpaidAlerts()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var alerts = await _saleService.GetUnpaidAlertsAsync(tenantId);
            return Ok(alerts);
        }

      
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetByCustomerId(int customerId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var sales = await _saleService.GetByCustomerIdAsync(customerId, tenantId);
            return Ok(sales);
        }

     
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SaleDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var sale = await _saleService.GetByIdAsync(id, tenantId);

            if (sale is null)
                return NotFound(new { message = $"Sale with ID {id} was not found." });

            return Ok(sale);
        }

        
        [HttpPost]
        public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var userId = GetUserId();

            try
            {
                var created = await _saleService.CreateAsync(dto, tenantId, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.SaleId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var success = await _saleService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Sale with ID {id} was not found." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

  
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var success = await _saleService.DeleteAsync(id, tenantId);

            if (!success)
                return NotFound(new { message = $"Sale with ID {id} was not found." });

            return NoContent();
        }

     
        [HttpPatch("{id:int}/paid")]
        public async Task<IActionResult> MarkPaid(int id, [FromBody] MarkSalePaidDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var success = await _saleService.MarkPaidAsync(id, dto, tenantId);

            if (!success)
                return NotFound(new { message = $"Sale with ID {id} was not found." });

            return NoContent();
        }
    }
}
