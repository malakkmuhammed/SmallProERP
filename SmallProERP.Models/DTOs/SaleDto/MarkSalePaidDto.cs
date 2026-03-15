using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    public class MarkSalePaidDto
    {
        [Required(ErrorMessage = "IsPaid is required.")]
        public bool IsPaid { get; set; }

        // Optional — if not provided, defaults to UtcNow when IsPaid = true
        public DateTime? PaidDate { get; set; }

        [MaxLength(50, ErrorMessage = "Payment method must not exceed 50 characters.")]
        public string? PaymentMethod { get; set; }

        [MaxLength(500, ErrorMessage = "Payment notes must not exceed 500 characters.")]
        public string? PaymentNotes { get; set; }
    }
}
