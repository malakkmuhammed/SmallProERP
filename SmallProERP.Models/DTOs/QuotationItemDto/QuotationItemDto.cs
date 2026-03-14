using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationItemDtos

{
    public class QuotationItemDto
    {
        public int QuotationItemId { get; set; }
        public int QuotationId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;  // resolved from Product
        public string ProductCode { get; set; } = string.Empty;  // resolved from Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
