using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.InventoryDtos
{
    public class InventoryMovementDto
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public MovementType MovementType { get; set; }
        public string MovementTypeText { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime MovementDate { get; set; }
        public string? Notes { get; set; }
    }
}
