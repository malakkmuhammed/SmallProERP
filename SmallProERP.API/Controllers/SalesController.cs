using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
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
        // GET /api/sales
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll(
            [FromQuery] string? search = null)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            var sales = await _saleService.GetAllAsync(tenantId, search);
            return Ok(sales);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/sales/statistics
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("statistics")]
        public async Task<ActionResult<SaleStatisticsDto>> GetStatistics()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _saleService.GetStatisticsAsync(tenantId));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/sales/unpaid-alerts
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("unpaid-alerts")]
        public async Task<ActionResult<IEnumerable<UnpaidInvoiceAlertDto>>> GetUnpaidAlerts()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _saleService.GetUnpaidAlertsAsync(tenantId));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/sales/customer/{customerId}
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetByCustomerId(int customerId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            return Ok(await _saleService.GetByCustomerIdAsync(customerId, tenantId));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/sales/{id}
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/sales
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var created = await _saleService.CreateAsync(dto, tenantId, GetUserId());

                return CreatedAtAction(nameof(GetById), new { id = created.SaleId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/sales/{id}/items  — add a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Adds a new line item to an existing unpaid sale.
        /// UnitPrice defaults to the product's SellingPrice if not provided.
        /// Sale totals are recalculated automatically.
        /// Returns the full updated sale.
        /// </summary>
        [HttpPost("{id:int}/items")]
        public async Task<ActionResult<SaleDto>> AddItem(int id, [FromBody] AddSaleItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _saleService.AddItemAsync(id, dto, tenantId);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/sales/{id}/items/{itemId}  — update a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Updates quantity and/or price of a line item on an unpaid sale.
        /// UnitPrice keeps its current value if not provided.
        /// Sale totals are recalculated automatically.
        /// Returns the full updated sale.
        /// </summary>
        [HttpPut("{id:int}/items/{itemId:int}")]
        public async Task<ActionResult<SaleDto>> UpdateItem(
            int id, int itemId, [FromBody] UpdateSaleItemInlineDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _saleService.UpdateItemAsync(id, itemId, dto, tenantId);

                if (updated is null)
                    return NotFound(new { message = $"Item with ID {itemId} was not found on sale {id}." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/sales/{id}/items/{itemId}  — remove a single item
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Removes a line item from an unpaid sale.
        /// Sale totals are recalculated automatically.
        /// Returns the full updated sale.
        /// </summary>
        [HttpDelete("{id:int}/items/{itemId:int}")]
        public async Task<ActionResult<SaleDto>> RemoveItem(int id, int itemId)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0) return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var updated = await _saleService.RemoveItemAsync(id, itemId, tenantId);

                if (updated is null)
                    return NotFound(new { message = $"Item with ID {itemId} was not found on sale {id}." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/sales/{id}  — update header fields only
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/sales/{id}
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // PATCH /api/sales/{id}/paid
        // ─────────────────────────────────────────────────────────────────────
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
