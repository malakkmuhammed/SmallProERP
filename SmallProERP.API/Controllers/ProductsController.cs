

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.API.Helpers;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.ProductDTOS;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
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

        private int GetUserId()
        {
            // ⭐ TEST MODE: Return hardcoded value
            if (TestHelper.IsTestMode)
            {
                return TestHelper.TestUserId;  // Returns 1
            }

            // ⭐ PRODUCTION MODE: Extract from token
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("UserId not found in token");

            return int.Parse(userIdClaim.Value);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var products = await _productService.GetAllAsync(tenantId);
            return Ok(products);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStock()
        {
            var tenantId = GetTenantId();
            var products = await _productService.GetLowStockProductsAsync(tenantId);
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required" });

            var tenantId = GetTenantId();
            var products = await _productService.SearchAsync(term, tenantId);
            return Ok(products);
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<ProductDto>> GetByCode(string code)
        {
            var tenantId = GetTenantId();
            var product = await _productService.GetByCodeAsync(code, tenantId);

            if (product == null)
                return NotFound(new { message = $"Product with code '{code}' not found" });

            return Ok(product);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string category)
        {
            var tenantId = GetTenantId();
            var products = await _productService.GetByCategoryAsync(category, tenantId);
            return Ok(products);
        }

        [HttpGet("supplier/{supplierId:int}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetBySupplier(int supplierId)
        {
            var tenantId = GetTenantId();
            var products = await _productService.GetBySupplierAsync(supplierId, tenantId);
            return Ok(products);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var tenantId = GetTenantId();
            var categories = await _productService.GetAllCategoriesAsync(tenantId);
            return Ok(categories);
        }

        [HttpGet("inventory-value")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> GetInventoryValue()
        {
            var tenantId = GetTenantId();
            var value = await _productService.GetTotalInventoryValueAsync(tenantId);
            return Ok(new { totalValue = value });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var product = await _productService.GetByIdAsync(id, tenantId);

            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            return Ok(product);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var created = await _productService.CreateAsync(dto, tenantId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.ProductId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var success = await _productService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _productService.DeleteAsync(id, tenantId);

                if (!success)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("adjust-stock")]
        //[Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();
                var success = await _productService.AdjustStockAsync(dto, tenantId, userId);

                if (!success)
                    return NotFound(new { message = $"Product with ID {dto.ProductId} not found" });

                return Ok(new { message = "Stock adjusted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
