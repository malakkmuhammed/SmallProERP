using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerDtos
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        [MaxLength(200, ErrorMessage = "Email must not exceed 200 characters.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20, ErrorMessage = "Phone must not exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Company must not exceed 100 characters.")]
        public string? Company { get; set; }

        [MaxLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
        public string? Address { get; set; }
    }
}
