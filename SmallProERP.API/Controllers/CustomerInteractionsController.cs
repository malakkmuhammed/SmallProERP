using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.CustomerInteractionDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.API.Controllers
{

    //[Authorize]
    [ApiController]
    [Route("api/customerinteractions")]
    public class CustomerInteractionsController : ControllerBase
    {
        private readonly ICustomerInteractionService _interactionService;

        public CustomerInteractionsController(ICustomerInteractionService interactionService)
        {
            _interactionService = interactionService;
        }


        private int GetTenantId()
        {
            var claim = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int tenantId))
                throw new UnauthorizedAccessException("TenantId claim is missing or invalid.");

            return tenantId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var interactions = await _interactionService.GetAllAsync(tenantId);
            return Ok(interactions);
        }


        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetByCustomerId(
            int customerId,
            [FromQuery] string? type = null)
        {
            var tenantId = GetTenantId();

            // Parse the optional type query parameter
            InteractionType? parsedType = null;

            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!Enum.TryParse<InteractionType>(type, ignoreCase: true, out var result))
                    return BadRequest(new
                    {
                        message = $"Invalid interaction type '{type}'. " +
                                  "Valid values: Call, Email, Note, WhatsApp, Meeting."
                    });

                parsedType = result;
            }

            var interactions = await _interactionService.GetByCustomerIdAsync(tenantId, customerId, parsedType);
            return Ok(interactions);
        }

       
        [HttpGet("customer/{customerId:int}/summary")]
        public async Task<ActionResult<CustomerInteractionSummaryDto>> GetSummaryByCustomerId(
            int customerId)
        {
            var tenantId = GetTenantId();
            var summary = await _interactionService.GetSummaryByCustomerIdAsync(customerId, tenantId);

            if (summary is null)
                return NotFound(new { message = $"Customer with ID {customerId} was not found." });

            return Ok(summary);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerInteractionDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var interaction = await _interactionService.GetByIdAsync(id, tenantId);

            if (interaction is null)
                return NotFound(new { message = $"Interaction with ID {id} was not found." });

            return Ok(interaction);
        }


        [HttpPost]
        public async Task<ActionResult<CustomerInteractionDto>> Create(
            [FromBody] CreateCustomerInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();

            try
            {
                var created = await _interactionService.CreateAsync(tenantId, dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.InteractionId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCustomerInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();

            try
            {
                var success = await _interactionService.UpdateAsync(id, tenantId, dto);

                if (!success)
                    return NotFound(new { message = $"Interaction with ID {id} was not found." });

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
            var success = await _interactionService.DeleteAsync(id, tenantId);

            if (!success)
                return NotFound(new { message = $"Interaction with ID {id} was not found." });

            return NoContent();
        }
    }
}
