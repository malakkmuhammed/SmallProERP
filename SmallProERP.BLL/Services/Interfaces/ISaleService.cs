using SmallProERP.Models.DTOs.SaleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ISaleService
    {
        /// <summary>Returns all sales with their items for the current tenant.</summary>
        Task<IEnumerable<SaleDto>> GetAllAsync(int tenantId, string? search = null);

        /// <summary>Returns a single sale with all its items.</summary>
        Task<SaleDto?> GetByIdAsync(int id, int tenantId);

        /// <summary>Returns all sales for a specific customer.</summary>
        Task<IEnumerable<SaleDto>> GetByCustomerIdAsync(int customerId, int tenantId);

        /// <summary>Returns revenue and payment statistics.</summary>
        Task<SaleStatisticsDto> GetStatisticsAsync(int tenantId);

        /// <summary>Returns unpaid invoices created 2+ days ago.</summary>
        Task<IEnumerable<UnpaidInvoiceAlertDto>> GetUnpaidAlertsAsync(int tenantId);

        /// <summary>
        /// Creates a new sale with all its line items in one operation.
        /// Validates stock for every item before saving.
        /// Subtotal and TotalAmount are auto-calculated.
        /// </summary>
        Task<SaleDto> CreateAsync(CreateSaleDto dto, int tenantId, int? userId);

        /// <summary>Updates sale header fields only (customer, dates, tax).</summary>
        Task<bool> UpdateAsync(int id, UpdateSaleDto dto, int tenantId);

        /// <summary>Hard-deletes a sale and all its items.</summary>
        Task<bool> DeleteAsync(int id, int tenantId);

        /// <summary>
        /// Marks a sale as paid or unpaid.
        /// IsPaid = true  → deducts stock, creates InventoryMovement per item.
        /// IsPaid = false → clears payment info (stock NOT restored automatically).
        /// </summary>
        Task<bool> MarkPaidAsync(int id, MarkSalePaidDto dto, int tenantId);

        // ── Item management ───────────────────────────────────────────────────

        /// <summary>
        /// Adds a new item to an existing unpaid sale.
        /// UnitPrice defaults to product SellingPrice if not provided.
        /// Validates stock availability.
        /// Recalculates sale totals automatically.
        /// Throws InvalidOperationException if sale is paid or product not found.
        /// </summary>
        Task<SaleDto> AddItemAsync(int saleId, AddSaleItemDto dto, int tenantId);

        /// <summary>
        /// Updates quantity and/or price of an existing line item.
        /// UnitPrice keeps its current value if not provided in the request.
        /// Validates stock availability for the new quantity.
        /// Recalculates sale totals automatically.
        /// Returns false if item not found.
        /// Throws InvalidOperationException if sale is paid.
        /// </summary>
        Task<SaleDto?> UpdateItemAsync(int saleId, int itemId, UpdateSaleItemInlineDto dto, int tenantId);

        /// <summary>
        /// Removes a line item from an unpaid sale.
        /// Recalculates sale totals automatically.
        /// Returns false if item not found.
        /// Throws InvalidOperationException if sale is paid.
        /// </summary>
        Task<SaleDto?> RemoveItemAsync(int saleId, int itemId, int tenantId);
    }
}
