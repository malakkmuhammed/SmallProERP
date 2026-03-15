using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class SaleService : ISaleService
    {
        private readonly SmallProDbContext _context;
        private readonly SaleItemService _saleItemService;

        public SaleService(SmallProDbContext context, SaleItemService saleItemService)
        {
            _context = context;
            _saleItemService = saleItemService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL — with optional search by customer name or company
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<SaleDto>> GetAllAsync(
            int tenantId, string? search = null)
        {
            var query = _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s =>
                    s.Customer != null &&
                    (s.Customer.Name.ToLower().Contains(term) ||
                    (s.Customer.Company != null &&
                     s.Customer.Company.ToLower().Contains(term))));
            }

            var sales = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sales.Select(MapToDto);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto?> GetByIdAsync(int id, int tenantId)
        {
            var sale = await FindByIdAsync(id, tenantId);
            return sale is null ? null : MapToDto(sale);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY CUSTOMER ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<SaleDto>> GetByCustomerIdAsync(
            int customerId, int tenantId)
        {
            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId
                         && s.CustomerId == customerId)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sales.Select(MapToDto);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET STATISTICS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleStatisticsDto> GetStatisticsAsync(int tenantId)
        {
            var now = DateTime.UtcNow;
            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .ToListAsync();

            var paid = sales.Where(s => s.IsPaid).ToList();
            var unpaid = sales.Where(s => !s.IsPaid).ToList();
            var overdue = unpaid
                .Where(s => s.DueDate.HasValue && s.DueDate.Value < now)
                .ToList();

            return new SaleStatisticsDto
            {
                TotalInvoices = sales.Count,
                PaidCount = paid.Count,
                UnpaidCount = unpaid.Count,
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                CollectedRevenue = paid.Sum(s => s.TotalAmount),
                OutstandingAmount = unpaid.Sum(s => s.TotalAmount),
                OverdueCount = overdue.Count,
                OverdueAmount = overdue.Sum(s => s.TotalAmount)
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET UNPAID ALERTS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<UnpaidInvoiceAlertDto>> GetUnpaidAlertsAsync(int tenantId)
        {
            var now = DateTime.UtcNow;
            var threshold = now.AddDays(-2);

            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId
                         && !s.IsPaid
                         && s.CreatedAt <= threshold)
                .Include(s => s.Customer)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return sales.Select(s =>
            {
                var daysSince = (int)(now - s.CreatedAt).TotalDays;
                var isOverdue = s.DueDate.HasValue && s.DueDate.Value < now;
                var daysOverdue = isOverdue
                    ? (int)(now - s.DueDate!.Value).TotalDays
                    : null as int?;

                return new UnpaidInvoiceAlertDto
                {
                    SaleId = s.SaleId,
                    InvoiceNumber = s.InvoiceNumber,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer?.Name ?? string.Empty,
                    CustomerPhone = s.Customer?.Phone,
                    CustomerEmail = s.Customer?.Email,
                    TotalAmount = s.TotalAmount,
                    InvoiceDate = s.InvoiceDate,
                    DueDate = s.DueDate,
                    DaysSinceInvoice = daysSince,
                    IsOverdue = isOverdue,
                    DaysOverdue = daysOverdue
                };
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto> CreateAsync(
            CreateSaleDto dto, int tenantId, int? userId)
        {
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            if (dto.QuotationId.HasValue)
            {
                bool quotationExists = await _context.Quotations
                    .AnyAsync(q => q.QuotationId == dto.QuotationId.Value
                               && q.TenantId == tenantId);

                if (!quotationExists)
                    throw new InvalidOperationException(
                        $"Quotation with ID {dto.QuotationId.Value} was not found in this tenant.");
            }

            bool invoiceExists = await _context.Sales
                .AnyAsync(s => s.TenantId == tenantId
                            && s.InvoiceNumber == dto.InvoiceNumber);

            if (invoiceExists)
                throw new InvalidOperationException(
                    $"Invoice number '{dto.InvoiceNumber}' already exists in this tenant.");

            var sale = new Sale
            {
                InvoiceNumber = dto.InvoiceNumber,
                QuotationId = dto.QuotationId,
                CustomerId = dto.CustomerId,
                Subtotal = dto.Subtotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                IsPaid = false,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                PaymentMethod = dto.PaymentMethod,
                PaymentNotes = dto.PaymentNotes,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            await _context.Entry(sale).Reference(s => s.Customer).LoadAsync();

            return MapToDto(sale);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateSaleDto dto, int tenantId)
        {
            var sale = await FindByIdAsync(id, tenantId);

            if (sale is null)
                return false;

            if (sale.CustomerId != dto.CustomerId)
            {
                bool customerExists = await _context.Customers
                    .AnyAsync(c => c.CustomerId == dto.CustomerId
                                && c.TenantId == tenantId);

                if (!customerExists)
                    throw new InvalidOperationException(
                        $"Customer with ID {dto.CustomerId} was not found in this tenant.");
            }

            if (dto.QuotationId.HasValue && dto.QuotationId != sale.QuotationId)
            {
                bool quotationExists = await _context.Quotations
                    .AnyAsync(q => q.QuotationId == dto.QuotationId.Value
                               && q.TenantId == tenantId);

                if (!quotationExists)
                    throw new InvalidOperationException(
                        $"Quotation with ID {dto.QuotationId.Value} was not found in this tenant.");
            }

            sale.CustomerId = dto.CustomerId;
            sale.QuotationId = dto.QuotationId;
            sale.Subtotal = dto.Subtotal;
            sale.TaxAmount = dto.TaxAmount;
            sale.TotalAmount = dto.TotalAmount;
            sale.InvoiceDate = dto.InvoiceDate;
            sale.DueDate = dto.DueDate;
            sale.PaymentMethod = dto.PaymentMethod;
            sale.PaymentNotes = dto.PaymentNotes;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var sale = await FindByIdAsync(id, tenantId);

            if (sale is null)
                return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // MARK PAID  ← stock deduction happens here
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> MarkPaidAsync(int id, MarkSalePaidDto dto, int tenantId)
        {
            var sale = await FindByIdAsync(id, tenantId);

            if (sale is null)
                return false;

            bool wasAlreadyPaid = sale.IsPaid;

            sale.IsPaid = dto.IsPaid;

            if (dto.IsPaid)
            {
                sale.PaidDate = dto.PaidDate ?? DateTime.UtcNow;
                sale.PaymentMethod = dto.PaymentMethod;
                sale.PaymentNotes = dto.PaymentNotes;

                // ── STOCK DEDUCTION ──────────────────────────────────────────
                // Only deduct stock on the first time IsPaid becomes true.
                // If the sale was already paid, skip to avoid double-deduction.
                if (!wasAlreadyPaid)
                    await _saleItemService.DeductStockForSaleAsync(id, tenantId);
            }
            else
            {
                // Marking as unpaid — clear payment info
                // Note: stock is NOT restored automatically.
                // If stock restoration is needed, handle it manually via stock adjustment.
                sale.PaidDate = null;
                sale.PaymentMethod = null;
                sale.PaymentNotes = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private async Task<Sale?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.SaleId == id
                                       && s.TenantId == tenantId);
        }

        private static SaleDto MapToDto(Sale sale) => new()
        {
            SaleId = sale.SaleId,
            InvoiceNumber = sale.InvoiceNumber,
            QuotationId = sale.QuotationId,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.Name ?? string.Empty,
            Subtotal = sale.Subtotal,
            TaxAmount = sale.TaxAmount,
            TotalAmount = sale.TotalAmount,
            IsPaid = sale.IsPaid,
            PaidDate = sale.PaidDate,
            PaymentMethod = sale.PaymentMethod,
            PaymentNotes = sale.PaymentNotes,
            InvoiceDate = sale.InvoiceDate,
            DueDate = sale.DueDate,
            TenantId = sale.TenantId,
            CreatedAt = sale.CreatedAt,
            CreatedBy = sale.CreatedBy
        };
    }
}
