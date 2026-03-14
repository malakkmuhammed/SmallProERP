using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class ChangeQuotationStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [Range(1, 4, ErrorMessage = "Status must be between 1 (Draft) and 4 (Rejected).")]
        public int Status { get; set; }
    }
}
