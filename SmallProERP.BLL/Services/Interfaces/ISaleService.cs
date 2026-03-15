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
        /// <summary>
        /// Returns all sales for the current tenant.
        /// Optionally filter by customer name using a search term.
        /// </summary>
        Task<IEnumerable<SaleDto>> GetAllAsync(int tenantId, string? search = null);

        /// <summary>
        /// Returns a single sale by ID.
        /// Returns null if not found or belongs to a different tenant.
        /// </summary>
        Task<SaleDto?> GetByIdAsync(int id, int tenantId);

        /// <summary>Returns all sales for a specific customer.</summary>
        Task<IEnumerable<SaleDto>> GetByCustomerIdAsync(int customerId, int tenantId);

        /// <summary>
        /// Returns total revenue, paid/unpaid counts, outstanding amount,
        /// and overdue invoice data for the current tenant.
        /// </summary>
        Task<SaleStatisticsDto> GetStatisticsAsync(int tenantId);

        /// <summary>
        /// Returns all unpaid invoices that were created 2 or more days ago.
        /// Sorted by oldest invoice first so the most urgent appear at the top.
        /// </summary>
        Task<IEnumerable<UnpaidInvoiceAlertDto>> GetUnpaidAlertsAsync(int tenantId);

        /// <summary>
        /// Creates a new sale.
        /// Throws InvalidOperationException if customer/quotation not found
        /// or InvoiceNumber already exists in this tenant.
        /// </summary>
        Task<SaleDto> CreateAsync(CreateSaleDto dto, int tenantId, int? userId);

        /// <summary>
        /// Updates an existing sale's editable fields.
        /// Returns false if not found or belongs to another tenant.
        /// </summary>
        Task<bool> UpdateAsync(int id, UpdateSaleDto dto, int tenantId);

        /// <summary>
        /// Hard-deletes a sale by ID.
        /// Returns false if not found or belongs to another tenant.
        /// </summary>
        Task<bool> DeleteAsync(int id, int tenantId);

        /// <summary>
        /// Marks a sale as paid or unpaid.
        /// IsPaid = true  → sets PaidDate, PaymentMethod, PaymentNotes.
        /// IsPaid = false → clears all payment info.
        /// Returns false if not found or belongs to another tenant.
        /// </summary>
        Task<bool> MarkPaidAsync(int id, MarkSalePaidDto dto, int tenantId);
    }
}
