using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    // ─────────────────────────────────────────────────────────────────────────
    // ADD ITEM — POST /api/sales/{id}/items
    // ─────────────────────────────────────────────────────────────────────────
    public class AddSaleItemDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        
        
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE ITEM — PUT /api/sales/{saleId}/items/{itemId}
    // ─────────────────────────────────────────────────────────────────────────
    public class UpdateSaleItemInlineDto
    {
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        // Optional — keeps existing price if not provided
        public decimal? UnitPrice { get; set; } //test


    }
}
