using SmallProERP.Models.DTOs.ProductDTOS;
using SmallProERP.Models.DTOs.PurchaseOrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SupplierDtos
{

    public class SupplierDetailsDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ProductDto> Products { get; set; } = new();
        public List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();

        public int TotalProductsSupplied { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalAmountSpent { get; set; }
    }
}
