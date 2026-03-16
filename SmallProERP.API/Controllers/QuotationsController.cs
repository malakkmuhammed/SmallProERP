using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/quotations")]
    public class QuotationsController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationsController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLAIM HELPERS
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/quotations
        // GET /api/quotations?search=ahmed
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationSummaryDto>>> GetAll(
            [FromQuery] string? search = null)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _quotationService.GetAllAsync(tenantId, search));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/quotations/statistics
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("statistics")]
        public async Task<ActionResult<QuotationStatisticsDto>> GetStatistics()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _quotationService.GetStatisticsAsync(tenantId));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/quotations/customer/{customerId}
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<QuotationSummaryDto>>> GetByCustomerId(
            int customerId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _quotationService.GetByCustomerIdAsync(customerId, tenantId));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/quotations/{id}
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/quotations
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Creates a new quotation with all items in one request.
        /// UnitPrice defaults to product SellingPrice if not provided per item.
        /// Status defaults to Draft.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<QuotationDto>> Create([FromBody] CreateQuotationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var created = await _quotationService.CreateAsync(dto, tenantId, GetUserId());

                return CreatedAtAction(nameof(GetById), new { id = created.QuotationId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/quotations/{id}/items  — add a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Adds a new line item to a Draft or Sent quotation.
        /// UnitPrice defaults to product SellingPrice if not provided.
        /// Blocked on Accepted or Rejected quotations.
        /// Returns the full updated quotation.
        /// </summary>
        [HttpPost("{id:int}/items")]
        public async Task<ActionResult<QuotationDto>> AddItem(
            int id, [FromBody] AddQuotationItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _quotationService.AddItemAsync(id, dto, tenantId);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/quotations/{id}/items/{itemId}  — update a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Updates quantity and/or price of a line item.
        /// UnitPrice keeps current value if not provided.
        /// Blocked on Accepted or Rejected quotations.
        /// Returns the full updated quotation.
        /// </summary>
        [HttpPut("{id:int}/items/{itemId:int}")]
        public async Task<ActionResult<QuotationDto>> UpdateItem(
            int id, int itemId, [FromBody] UpdateQuotationItemInlineDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _quotationService.UpdateItemAsync(id, itemId, dto, tenantId);

                if (updated is null)
                    return NotFound(new { message = $"Item {itemId} was not found on quotation {id}." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/quotations/{id}/items/{itemId}  — remove a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Removes a line item from a Draft or Sent quotation.
        /// Blocked on Accepted or Rejected quotations.
        /// Returns the full updated quotation.
        /// </summary>
        [HttpDelete("{id:int}/items/{itemId:int}")]
        public async Task<ActionResult<QuotationDto>> RemoveItem(int id, int itemId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _quotationService.RemoveItemAsync(id, itemId, tenantId);

                if (updated is null)
                    return NotFound(new { message = $"Item {itemId} was not found on quotation {id}." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/quotations/{id}/convert
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Converts an Accepted quotation into a Sale. Copies all line items.</summary>
        [HttpPost("{id:int}/convert")]
        public async Task<ActionResult<SaleDto>> ConvertToSale(
            int id, [FromBody] ConvertQuotationToSaleDto dto)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var sale = await _quotationService.ConvertToSaleAsync(id, dto, tenantId, GetUserId());
                return Ok(sale);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/quotations/{id}  — header fields only
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/quotations/{id}
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // PATCH /api/quotations/{id}/status
        // ─────────────────────────────────────────────────────────────────────
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
