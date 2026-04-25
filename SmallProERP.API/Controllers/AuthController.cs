using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.Auth;
using SmallProERP.Models.Enums;

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
            if (int.TryParse(dto.Role, out _))
            {
                return BadRequest(new
                {
                    message = "Role must be a text value ( Manager, InventoryManager, Salesperson ) not a number"
                });
            }

            
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var userRole))
            {
                var validRoles = string.Join(", ", Enum.GetNames(typeof(UserRole)));
                return BadRequest(new
                {
                    message = $"Invalid role '{dto.Role}'. Valid roles are: {validRoles}"
                });
            }

            
            if (!Enum.IsDefined(typeof(UserRole), userRole))
            {
                var validRoles = string.Join(", ", Enum.GetNames(typeof(UserRole)));
                return BadRequest(new
                {
                    message = $"Invalid role value. Valid roles are: {validRoles}"
                });
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

            var result = await _authService.RegisterUserAsync(dto, adminTenantId, adminUserId, userRole);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                userId = result.UserId,
                username = result.Username,
                tenantId = adminTenantId,
                role = userRole.ToString()
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
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Extract UserId from JWT token
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim);

            // Call service (for logging or future token blacklisting)
            await _authService.LogoutAsync(userId);

            return Ok(new { message = "Logged out successfully. Please delete your token." });
        }
        [HttpPost("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto dto)
        {
            
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "New password is required" });
            }

            
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var adminTenantId = int.Parse(tenantIdClaim);

            
            var result = await _authService.ResetUserPasswordAsync(
                dto.Username,
                dto.NewPassword,
                adminTenantId
            );

            if (!result)
            {
                return NotFound(new { message = "User not found or does not belong to your company" });
            }

            return Ok(new
            {
                message = "Password reset successfully",
                username = dto.Username
            });
        }
    }
}


