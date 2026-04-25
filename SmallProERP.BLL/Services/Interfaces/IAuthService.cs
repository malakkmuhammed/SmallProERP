using SmallProERP.Models.DTOs.Auth;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int? TenantId, string Username)> RegisterTenantAsync(RegisterTenantDto dto);
        Task<(bool Success, string Message, int? UserId, string Username)> RegisterUserAsync(RegisterUserDto dto, int adminTenantId, int adminUserId, UserRole userRole);
        Task<(bool Success, string Message, AuthResponseDto Data)> LoginAsync(LoginDto dto);
        Task<bool> ResetUserPasswordAsync(string username, string newPassword, int adminTenantId);
        Task<bool> LogoutAsync(int userId); 
    }
}
