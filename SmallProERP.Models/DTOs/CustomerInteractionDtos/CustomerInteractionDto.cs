using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerInteractionDtos
{
    public class CustomerInteractionDto
    {
        public int InteractionId { get; set; }
        public int CustomerId { get; set; }
        public int? UserId { get; set; }
        public DateTime InteractionDate { get; set; }
        public InteractionType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;   // e.g. "Call", "Email"
        public string Description { get; set; } = string.Empty;
    }
}
