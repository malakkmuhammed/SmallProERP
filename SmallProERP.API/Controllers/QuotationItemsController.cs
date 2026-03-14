using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.QuotationItemDto;
using SmallProERP.Models.DTOs.QuotationItemDtos;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/quotationitems")]
    public class QuotationItemsController : ControllerBase
    {
        private readonly IQuotationItemService _quotationItemService;

        public QuotationItemsController(IQuotationItemService quotationItemService)
        {
            _quotationItemService = quotationItemService;
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

        
        [HttpGet("quotation/{quotationId:int}")]
        public async Task<ActionResult<IEnumerable<QuotationItemDto>>> GetByQuotationId(
            int quotationId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var items = await _quotationItemService.GetByQuotationIdAsync(quotationId, tenantId);
            return Ok(items);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuotationItemDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var item = await _quotationItemService.GetByIdAsync(id, tenantId);

            if (item is null)
                return NotFound(new { message = $"Quotation item with ID {id} was not found." });

            return Ok(item);
        }

     
        [HttpPost]
        public async Task<ActionResult<QuotationItemDto>> Create(
            [FromBody] CreateQuotationItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var created = await _quotationItemService.CreateAsync(dto, tenantId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.QuotationItemId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuotationItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var success = await _quotationItemService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Quotation item with ID {id} was not found." });

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

            var success = await _quotationItemService.DeleteAsync(id, tenantId);

            if (!success)
                return NotFound(new { message = $"Quotation item with ID {id} was not found." });

            return NoContent();
        }
    }
}
