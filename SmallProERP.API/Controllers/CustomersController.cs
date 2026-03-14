using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.CustomerDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }


        private int GetTenantId()
        {
            var claim = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int tenantId))
                throw new UnauthorizedAccessException("TenantId claim is missing or invalid.");

            return tenantId;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll( 
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            var tenantId = GetTenantId();
            CustomerStatus? parsedStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<CustomerStatus>(status, ignoreCase: true, out var result))
                    return BadRequest(new
                    {
                        message = $"Invalid status '{status}'. " +
                                  "Valid values: NewLead, Interested, Opportunity, Won, Lost."
                    });

                parsedStatus = result;
            }


            var customers = await _customerService.GetAllAsync(tenantId, parsedStatus, search);
            return Ok(customers);
        }

 
        [HttpGet("statistics")]
        public async Task<ActionResult<CustomerStatisticsDto>> GetStatistics()
        {
            var tenantId = GetTenantId();
            var statistics = await _customerService.GetStatisticsAsync(tenantId);
            return Ok(statistics);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var customer = await _customerService.GetByIdAsync(id, tenantId);

            if (customer is null)
                return NotFound(new { message = $"Customer with ID {id} was not found." });

            return Ok(customer);
        }

   
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto)
        {
            var tenantId = GetTenantId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _customerService.CreateAsync(tenantId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.CustomerId },
                created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            var tenantId = GetTenantId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _customerService.UpdateAsync(id, tenantId, dto);

            if (!success)
                return NotFound(new { message = $"Customer with ID {id} was not found." });

            return NoContent();
        }

   
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = GetTenantId();
            var success = await _customerService.DeleteAsync(id, tenantId);

            if (!success)
                return NotFound(new { message = $"Customer with ID {id} was not found." });

            return NoContent();
        }

    
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(
            int id,
            [FromBody] ChangeCustomerStatusDto dto)
        {
            var tenantId = GetTenantId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _customerService.ChangeStatusAsync(id, tenantId, dto.Status);

                if (!success)
                    return NotFound(new { message = $"Customer with ID {id} was not found." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
