using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.PurchaseOrderDtos
{
    public class PurchaseOrderDto
    {
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public POStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }

        public List<PurchaseOrderItemDto> Items { get; set; } = new();

        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
    }
}
