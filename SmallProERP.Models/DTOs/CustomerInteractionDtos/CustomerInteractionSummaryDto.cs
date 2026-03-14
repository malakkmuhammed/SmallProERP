using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerInteractionDtos
{
    public class CustomerInteractionSummaryDto
    {
        public int CustomerId { get; set; }
        public int TotalInteractions { get; set; }
        public int CallCount { get; set; }
        public int EmailCount { get; set; }
        public int NoteCount { get; set; }
        public int WhatsAppCount { get; set; }
        public int MeetingCount { get; set; }
        public DateTime? LastInteractionDate { get; set; }
    }
}
