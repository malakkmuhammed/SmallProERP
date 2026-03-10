using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.ProductDTOS
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product code is required")]
        [StringLength(50, ErrorMessage = "Product code cannot exceed 50 characters")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Minimum stock level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level must be 0 or greater")]
        public int MinimumStockLevel { get; set; } = 5;

        [Required(ErrorMessage = "Purchase price is required")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Purchase price must be greater than 0")]

        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Purchase price must be 0 or greater.")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "Selling price is required")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Selling price must be greater than 0")]
        public decimal SellingPrice { get; set; }

        public int? SupplierId { get; set; }
    }
}
