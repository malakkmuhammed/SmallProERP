using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, string companyName)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("Username", user.UserName ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("CompanyName", companyName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationInHoursString = _configuration["Jwt:ExpirationInHours"];
            if (string.IsNullOrWhiteSpace(expirationInHoursString))
                throw new InvalidOperationException("JWT expiration is not configured.");

            var expirationInHours = double.Parse(expirationInHoursString);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationInHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
