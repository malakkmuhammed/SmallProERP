using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.SaleItemDto;
using SmallProERP.Models.Entities;

namespace SmallProERP.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/saleitems")]
    public class SaleItemsController : ControllerBase
    {
        private readonly ISaleItemService _saleItemService;

        public SaleItemsController(ISaleItemService saleItemService)
        {
            _saleItemService = saleItemService;
        }

       
        private int GetTenantId()
        {
            var claim = User.FindFirst("TenantId")?.Value;
            return int.TryParse(claim, out int tenantId) ? tenantId : 0;
        }

  
        [HttpGet("sale/{saleId:int}")]
        public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetBySaleId(int saleId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var items = await _saleItemService.GetBySaleIdAsync(saleId, tenantId);
            return Ok(items);
        }

       
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SaleItemDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var item = await _saleItemService.GetByIdAsync(id, tenantId);

            if (item is null)
                return NotFound(new { message = $"Sale item with ID {id} was not found." });

            return Ok(item);
        }

  
        [HttpPost]
        public async Task<ActionResult<SaleItemDto>> Create([FromBody] CreateSaleItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var created = await _saleItemService.CreateAsync(dto, tenantId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.SaleItemId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var success = await _saleItemService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Sale item with ID {id} was not found." });

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

            try
            {
                var success = await _saleItemService.DeleteAsync(id, tenantId);

                if (!success)
                    return NotFound(new { message = $"Sale item with ID {id} was not found." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
