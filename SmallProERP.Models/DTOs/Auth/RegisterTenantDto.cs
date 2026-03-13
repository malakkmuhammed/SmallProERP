using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.Auth
{
    public class RegisterTenantDto
    {
        
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public required string CompanyName { get; set; }

        [Required(ErrorMessage = "Company email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public required string CompanyEmail { get; set; }

        [Required(ErrorMessage = "Company phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public required string CompanyPhone { get; set; }

        
        [Required(ErrorMessage = "Admin username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string AdminUsername { get; set; }

        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public required string AdminEmail { get; set; }

        [Required(ErrorMessage = "Admin password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public required string AdminPassword { get; set; }

        [Required(ErrorMessage = "Admin full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public required string AdminFullName { get; set; }
    }
}
