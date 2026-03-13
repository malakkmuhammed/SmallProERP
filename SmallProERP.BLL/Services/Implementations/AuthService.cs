using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Helpers;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.Auth;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly SmallProDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly JwtHelper _jwtHelper;

        public AuthService(
            SmallProDbContext context,
            UserManager<User> userManager,
            JwtHelper jwtHelper)
        {
            _context = context;
            _userManager = userManager;
            _jwtHelper = jwtHelper;
        }

        public async Task<(bool Success, string Message, int? TenantId, string Username)> RegisterTenantAsync(RegisterTenantDto dto)
        {
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == dto.CompanyEmail);

            if (existingTenant != null)
            {
                return (false, "Company email already exists", null, string.Empty);
            }

           
            var existingUser = await _userManager.FindByNameAsync(dto.AdminUsername);
            if (existingUser != null)
            {
                return (false, "Admin username already exists", null, string.Empty);
            }

            
            var existingEmail = await _userManager.FindByEmailAsync(dto.AdminEmail);
            if (existingEmail != null)
            {
                return (false, "Admin email already exists", null, string.Empty);
            }

            
            var tenant = new Tenant
            {
                CompanyName = dto.CompanyName,
                Email = dto.CompanyEmail,
                Phone = dto.CompanyPhone,
                IsActive = true,
                SubscriptionStartDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var admin = new User
            {
                UserName = dto.AdminUsername,
                Email = dto.AdminEmail,
                FullName = dto.AdminFullName,
                Role = UserRole.Admin,
                TenantId = tenant.TenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null
            };

            var result = await _userManager.CreateAsync(admin, dto.AdminPassword);

            if (!result.Succeeded)
            {
                
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to create admin: {errors}", null, string.Empty);
            }

            return (true, "Company and Admin registered successfully", tenant.TenantId, admin.UserName);
        }

        public async Task<(bool Success, string Message, int? UserId, string Username)> RegisterUserAsync(
            RegisterUserDto dto,
            int adminTenantId,
            int adminUserId)
        {
            
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserName == dto.Username && u.TenantId == adminTenantId);

            if (existingUser != null)
            {
                return (false, "Username already exists in your company", null, string.Empty);
            }

            
            var existingEmail = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.TenantId == adminTenantId);

            if (existingEmail != null)
            {
                return (false, "Email already exists in your company", null, string.Empty);
            }


            var newUser = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                TenantId = adminTenantId,  
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUserId
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to create user: {errors}", null, string.Empty);
            }

            return (true, "User registered successfully", newUser.Id, newUser.UserName);
        }

        public async Task<(bool Success, string Message, AuthResponseDto Data)> LoginAsync(LoginDto dto)
        {
            
            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserName == dto.Username);

            if (user == null)
            {
                return (false, "Invalid username or password", default!);
            }

            
            if (!user.IsActive)
            {
                return (false, "Account is disabled. Please contact your administrator", default!);
            }

            
            if (user.Tenant == null)
            {
                return (false, "Company account is missing. Please contact support", default!);
            }

            if (!user.Tenant.IsActive)
            {
                return (false, "Company account is disabled. Please contact support", default!);
            }

            
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordValid)
            {
                return (false, "Invalid username or password", default!);
            }

            
            var token = _jwtHelper.GenerateToken(user, user.Tenant.CompanyName);

            
            var response = new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.UserName!, 
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString(),
                TenantId = user.TenantId,
                CompanyName = user.Tenant.CompanyName
            };

            return (true, "Login successful", response);
        }
    }
}
