using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.AIDtos;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/ocr")]
    [Authorize]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<ActionResult<OcrResultDto>> Upload(IFormFile image)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var result = await _ocrService.ProcessOcrAsync(image, tenantId, GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<ActionResult<IEnumerable<OcrSummaryDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            var results = await _ocrService.GetAllAsync(tenantId);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<ActionResult<OcrResultDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            var result = await _ocrService.GetByIdAsync(id, tenantId);
            if (result is null)
                return NotFound(new { message = $"OCR result with ID {id} was not found." });

            return Ok(result);
        }


        private int GetTenantId()
        {
            var claim = User.FindFirst("TenantId")?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("UserId")?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}