using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class QuotationItem
    {
        public int QuotationItemId { get; set; }

        public int QuotationId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }

        public int TenantId { get; set; }

        
        public Quotation? Quotation { get; set; }

        public Product? Product { get; set; }

        public Tenant? Tenant { get; set; }
    }
}
