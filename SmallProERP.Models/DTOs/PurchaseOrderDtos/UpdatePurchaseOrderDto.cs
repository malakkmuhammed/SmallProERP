using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.PurchaseOrderDtos
{

    public class UpdatePurchaseOrderDto
    {
        [Required(ErrorMessage = "Supplier ID is required")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "Purchase order must contain at least one item")]
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
