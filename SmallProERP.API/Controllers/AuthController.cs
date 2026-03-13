using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.Auth;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-tenant")]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterTenantAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                tenantId = result.TenantId,
                adminUsername = result.Username
            });
        }

        [HttpPost("register-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminTenantIdClaim = User.FindFirst("TenantId");
            if (adminTenantIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: TenantId not found" });
            }

            var adminTenantId = int.Parse(adminTenantIdClaim.Value);

            var adminUserIdClaim = User.FindFirst("UserId");
            if (adminUserIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: UserId not found" });
            }

            var adminUserId = int.Parse(adminUserIdClaim.Value);

            var result = await _authService.RegisterUserAsync(dto, adminTenantId, adminUserId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                userId = result.UserId,
                username = result.Username,
                tenantId = adminTenantId
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var username = User.FindFirst("Username")?.Value;
            var fullName = User.FindFirst("FullName")?.Value;
            var email = User.FindFirst("Email")?.Value;
            var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var tenantId = User.FindFirst("TenantId")?.Value;
            var companyName = User.FindFirst("CompanyName")?.Value;

            return Ok(new
            {
                userId,
                username,
                fullName,
                email,
                role,
                tenantId,
                companyName
            });
        }
    }
}
