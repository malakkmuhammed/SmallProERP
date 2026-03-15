using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleItemDto
{
    public class CreateSaleItemDto
    {
        [Required(ErrorMessage = "SaleId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "SaleId must be greater than 0.")]
        public int SaleId { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Unit price must be 0 or greater.")]
        public decimal UnitPrice { get; set; }
    }
}
