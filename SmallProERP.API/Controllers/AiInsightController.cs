using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.AIDtos;
using SmallProERP.Models.Entities;
using System.Security.Claims;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/insights")]
    [Authorize]
    public class AiInsightController : ControllerBase
    {
        private readonly IAiInsightService _insightService;

        public AiInsightController(IAiInsightService insightService)
        {
            _insightService = insightService;
        }


        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<AiInsightDto>> Generate(
            [FromBody] GenerateInsightRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            try
            {
                var insight = await _insightService.GenerateInsightAsync(
                    request, tenantId, GetUserId());
                return Ok(insight);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<AiInsightSummaryDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            var insights = await _insightService.GetAllAsync(tenantId);
            return Ok(insights);
        }


        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<AiInsightDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return Unauthorized(new { message = "TenantId claim is missing." });

            var insight = await _insightService.GetByIdAsync(id, tenantId);
            if (insight is null)
                return NotFound(new { message = $"Insight with ID {id} was not found." });

            return Ok(insight);
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