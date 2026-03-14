using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/quotations")]
    public class QuotationsController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationsController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
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
        public async Task<ActionResult<IEnumerable<QuotationSummaryDto>>> GetAll(
            [FromQuery] string? search = null)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var quotations = await _quotationService.GetAllAsync(tenantId, search);
            return Ok(quotations);
        }


        [HttpGet("statistics")]
        public async Task<ActionResult<QuotationStatisticsDto>> GetStatistics()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var statistics = await _quotationService.GetStatisticsAsync(tenantId);
            return Ok(statistics);
        }

   
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<QuotationSummaryDto>>> GetByCustomerId(
            int customerId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var quotations = await _quotationService.GetByCustomerIdAsync(customerId, tenantId);
            return Ok(quotations);
        }

   
        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuotationDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var quotation = await _quotationService.GetByIdAsync(id, tenantId);

            if (quotation is null)
                return NotFound(new { message = $"Quotation with ID {id} was not found." });

            return Ok(quotation);
        }

       
        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<QuotationDetailsDto>> GetDetails(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var details = await _quotationService.GetDetailsByIdAsync(id, tenantId);

            if (details is null)
                return NotFound(new { message = $"Quotation with ID {id} was not found." });

            return Ok(details);
        }

        [HttpPost]
        public async Task<ActionResult<QuotationDto>> Create([FromBody] CreateQuotationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var userId = GetUserId();

            try
            {
                var created = await _quotationService.CreateAsync(dto, tenantId, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.QuotationId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

     
        [HttpPost("{id:int}/convert")]
        public async Task<ActionResult<SaleDto>> ConvertToSale(
            int id, [FromBody] ConvertQuotationToSaleDto dto)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var userId = GetUserId();

            try
            {
                var sale = await _quotationService.ConvertToSaleAsync(id, dto, tenantId, userId);
                return Ok(sale);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuotationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var success = await _quotationService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Quotation with ID {id} was not found." });

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

            var success = await _quotationService.DeleteAsync(id, tenantId);

            if (!success)
                return NotFound(new { message = $"Quotation with ID {id} was not found." });

            return NoContent();
        }


        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(
            int id, [FromBody] ChangeQuotationStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var success = await _quotationService.ChangeStatusAsync(id, dto.Status, tenantId);

                if (!success)
                    return NotFound(new { message = $"Quotation with ID {id} was not found." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
