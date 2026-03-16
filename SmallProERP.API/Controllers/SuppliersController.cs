using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.API.Helpers;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.SupplierDtos;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var suppliers = await _supplierService.GetAllAsync(tenantId);
            return Ok(suppliers);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required" });

            var tenantId = GetTenantId();
            var suppliers = await _supplierService.SearchAsync(term, tenantId);
            return Ok(suppliers);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SupplierDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var supplier = await _supplierService.GetByIdAsync(id, tenantId);

            if (supplier == null)
                return NotFound(new { message = $"Supplier with ID {id} not found" });

            return Ok(supplier);
        }

        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<SupplierDetailsDto>> GetDetails(int id)
        {
            var tenantId = GetTenantId();
            var supplierDetails = await _supplierService.GetDetailsAsync(id, tenantId);

            if (supplierDetails == null)
                return NotFound(new { message = $"Supplier with ID {id} not found" });

            return Ok(supplierDetails);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            var created = await _supplierService.CreateAsync(dto, tenantId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.SupplierId },
                created);
        }

        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            var success = await _supplierService.UpdateAsync(id, dto, tenantId);

            if (!success)
                return NotFound(new { message = $"Supplier with ID {id} not found" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _supplierService.DeleteAsync(id, tenantId);

                if (!success)
                    return NotFound(new { message = $"Supplier with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}