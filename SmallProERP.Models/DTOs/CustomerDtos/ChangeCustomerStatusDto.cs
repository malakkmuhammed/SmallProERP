using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerDtos
{
    public class ChangeCustomerStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [Range(1, 5, ErrorMessage = "Status must be between 1 (NewLead) and 5 (Lost).")]
        public int Status { get; set; }
    }
}
