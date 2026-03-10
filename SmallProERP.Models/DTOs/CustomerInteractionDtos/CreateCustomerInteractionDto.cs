using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerInteractionDtos
{
    public class CreateCustomerInteractionDto
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        public int? UserId { get; set; }

        [Required(ErrorMessage = "Interaction date is required.")]
        public DateTime InteractionDate { get; set; }

        [Required(ErrorMessage = "Interaction type is required.")]
        [Range(1, 5, ErrorMessage = "Type must be between 1 (Call) and 5 (Meeting).")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;
    }
}
