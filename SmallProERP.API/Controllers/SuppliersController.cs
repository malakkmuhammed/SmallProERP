using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.SupplierDtos;

namespace SmallProERP.API.Controllers
{

    [ApiController]
    [Route("api/suppliers")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET /api/suppliers
    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
        {
            var suppliers = await _supplierService.GetAllAsync();
            return Ok(suppliers);
        }

        // GET /api/suppliers/{id}
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SupplierDto>> GetById(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier is null)
                return NotFound(new { message = $"Supplier with ID {id} was not found." });

            return Ok(supplier);
        }

        // POST /api/suppliers
       
        [HttpPost]
        public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _supplierService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.SupplierId },
                created);
        }

        // PUT /api/suppliers/{id}
      
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _supplierService.UpdateAsync(id, dto);

            if (!success)
                return NotFound(new { message = $"Supplier with ID {id} was not found." });

            return NoContent();
        }

        // DELETE /api/suppliers/{id}
    
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _supplierService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = $"Supplier with ID {id} was not found." });

            return NoContent();
        }
    }
}
