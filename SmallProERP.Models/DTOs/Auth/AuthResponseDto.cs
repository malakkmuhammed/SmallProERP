using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public int TenantId { get; set; }
        public required string CompanyName { get; set; }
    }
}
