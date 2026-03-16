using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.SaleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IQuotationService
    {
        /// <summary>Returns all quotations as summary list with customer name + item count.</summary>
        Task<IEnumerable<QuotationSummaryDto>> GetAllAsync(int tenantId, string? search = null);

        /// <summary>Returns a single quotation with all its items.</summary>
        Task<QuotationDto?> GetByIdAsync(int id, int tenantId);

        /// <summary>Returns all quotations for a specific customer.</summary>
        Task<IEnumerable<QuotationSummaryDto>> GetByCustomerIdAsync(int customerId, int tenantId);

        /// <summary>Returns count and value totals per status.</summary>
        Task<QuotationStatisticsDto> GetStatisticsAsync(int tenantId);

        /// <summary>
        /// Creates a new quotation with all items in one operation.
        /// Validates stock for every item before saving.
        /// Subtotal and TotalAmount are auto-calculated from items.
        /// UnitPrice defaults to product SellingPrice if not provided.
        /// </summary>
        Task<QuotationDto> CreateAsync(CreateQuotationDto dto, int tenantId, int? userId);

        /// <summary>Updates quotation header fields only.</summary>
        Task<bool> UpdateAsync(int id, UpdateQuotationDto dto, int tenantId);

        /// <summary>Hard-deletes a quotation and all its items.</summary>
        Task<bool> DeleteAsync(int id, int tenantId);

        /// <summary>
        /// Changes quotation status (1=Draft, 2=Sent, 3=Accepted, 4=Rejected).
        /// Returns false if not found or belongs to another tenant.
        /// </summary>
        Task<bool> ChangeStatusAsync(int id, int status, int tenantId);

        /// <summary>
        /// Converts an Accepted quotation into a Sale.
        /// Throws InvalidOperationException if not Accepted or already converted.
        /// </summary>
        Task<SaleDto> ConvertToSaleAsync(int quotationId, ConvertQuotationToSaleDto dto, int tenantId, int? userId);

        // ── Item management ───────────────────────────────────────────────────

        /// <summary>
        /// Adds a new item to a Draft or Sent quotation.
        /// UnitPrice defaults to product SellingPrice if not provided.
        /// Validates stock availability.
        /// Recalculates totals automatically.
        /// Throws InvalidOperationException if quotation is Accepted or Rejected.
        /// </summary>
        Task<QuotationDto> AddItemAsync(int quotationId, AddQuotationItemDto dto, int tenantId);

        /// <summary>
        /// Updates quantity and/or price of an existing item.
        /// UnitPrice keeps current value if not provided.
        /// Validates stock availability.
        /// Recalculates totals automatically.
        /// Throws InvalidOperationException if quotation is Accepted or Rejected.
        /// </summary>
        Task<QuotationDto?> UpdateItemAsync(int quotationId, int itemId, UpdateQuotationItemInlineDto dto, int tenantId);

        /// <summary>
        /// Removes a line item from a Draft or Sent quotation.
        /// Recalculates totals automatically.
        /// Throws InvalidOperationException if quotation is Accepted or Rejected.
        /// </summary>
        Task<QuotationDto?> RemoveItemAsync(int quotationId, int itemId, int tenantId);
    }
}
