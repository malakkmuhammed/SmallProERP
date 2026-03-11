using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.CustomerInteractionDtos;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/customerinteractions")]
    public class CustomerInteractionsController : ControllerBase
    {
        private readonly ICustomerInteractionService _interactionService;

        public CustomerInteractionsController(ICustomerInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetAll()
        {
            var interactions = await _interactionService.GetAllAsync();
            return Ok(interactions);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetByCustomerId(int customerId)
        {
            var interactions = await _interactionService.GetByCustomerIdAsync(customerId);
            return Ok(interactions);
        }

 
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerInteractionDto>> GetById(int id)
        {
            var interaction = await _interactionService.GetByIdAsync(id);

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

            try
            {
                var created = await _interactionService.CreateAsync(dto);

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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _interactionService.UpdateAsync(id, dto);

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
            var success = await _interactionService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = $"Interaction with ID {id} was not found." });

            return NoContent();
        }
    }
}
