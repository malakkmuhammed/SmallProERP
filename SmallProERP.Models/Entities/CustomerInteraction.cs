using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class CustomerInteraction
    {
        public int InteractionId { get; set; }

        public int CustomerId { get; set; }

        public int? UserId { get; set; }

        public DateTime InteractionDate { get; set; }

        public string? Type { get; set; }

        public string Description { get; set; } = string.Empty;

        public int TenantId { get; set; }

       
        public Customer? Customer { get; set; }

        public User? User { get; set; }

        public Tenant? Tenant { get; set; }
    }
}
