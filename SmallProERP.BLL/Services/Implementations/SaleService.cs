using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
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

        public SaleService(SmallProDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<SaleDto>> GetAllAsync(
            int tenantId, string? search = null)
        {
            var query = _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
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
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
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
        // CREATE — sale + items in one operation
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto> CreateAsync(
            CreateSaleDto dto, int tenantId, int? userId)
        {
            // 1 — Verify customer
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            // 2 — Verify quotation if provided
            if (dto.QuotationId.HasValue)
            {
                bool quotationExists = await _context.Quotations
                    .AnyAsync(q => q.QuotationId == dto.QuotationId.Value
                               && q.TenantId == tenantId);

                if (!quotationExists)
                    throw new InvalidOperationException(
                        $"Quotation with ID {dto.QuotationId.Value} was not found in this tenant.");
            }

            // 3 — Unique InvoiceNumber
            bool invoiceExists = await _context.Sales
                .AnyAsync(s => s.TenantId == tenantId
                            && s.InvoiceNumber == dto.InvoiceNumber);

            if (invoiceExists)
                throw new InvalidOperationException(
                    $"Invoice number '{dto.InvoiceNumber}' already exists in this tenant.");

            // 4 — Validate all products and stock upfront
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId) && p.TenantId == tenantId)
                .ToListAsync();

            foreach (var itemDto in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.ProductId == itemDto.ProductId);

                if (product is null)
                    throw new InvalidOperationException(
                        $"Product with ID {itemDto.ProductId} was not found in this tenant.");

                if (product.Quantity < itemDto.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{product.Name}'. " +
                        $"Available: {product.Quantity}, Requested: {itemDto.Quantity}.");
            }

            // 5 — Build items using SellingPrice (or optional override)
            var saleItems = dto.Items.Select(i =>
            {
                var product = products.First(p => p.ProductId == i.ProductId);
                var unitPrice = product.SellingPrice;
                return new SaleItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = i.Quantity * unitPrice,
                    TenantId = tenantId
                };
            }).ToList();

            decimal subtotal = saleItems.Sum(si => si.LineTotal);
            decimal totalAmount = subtotal + dto.TaxAmount;

            // 6 — Save sale with items
            var sale = new Sale
            {
                InvoiceNumber = dto.InvoiceNumber,
                QuotationId = dto.QuotationId,
                CustomerId = dto.CustomerId,
                Subtotal = subtotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = totalAmount,
                IsPaid = false,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                PaymentMethod = dto.PaymentMethod,
                PaymentNotes = dto.PaymentNotes,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                SaleItems = saleItems
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            var created = await FindByIdAsync(sale.SaleId, tenantId);
            return MapToDto(created!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE HEADER
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

            sale.CustomerId = dto.CustomerId;
            sale.QuotationId = dto.QuotationId;
            sale.TaxAmount = dto.TaxAmount;
            sale.TotalAmount = sale.Subtotal + dto.TaxAmount;
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
        // MARK PAID
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

                if (!wasAlreadyPaid)
                    await DeductStockAsync(sale, tenantId);
            }
            else
            {
                sale.PaidDate = null;
                sale.PaymentMethod = null;
                sale.PaymentNotes = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ADD ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto> AddItemAsync(int saleId, AddSaleItemDto dto, int tenantId)
        {
            var sale = await FindByIdAsync(saleId, tenantId);

            if (sale is null)
                throw new InvalidOperationException(
                    $"Sale with ID {saleId} was not found in this tenant.");

            // Block editing paid invoices
            if (sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot add items to a paid invoice '{sale.InvoiceNumber}'.");

            // Verify product exists in this tenant
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {dto.ProductId} was not found in this tenant.");

            // Validate stock
            if (product.Quantity < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {product.Quantity}, Requested: {dto.Quantity}.");

            // Use provided price or fall back to SellingPrice
            var unitPrice =  product.SellingPrice;

            var item = new SaleItem
            {
                SaleId = saleId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = unitPrice,
                LineTotal = dto.Quantity * unitPrice,
                TenantId = tenantId
            };

            _context.SaleItems.Add(item);

            // Recalculate totals
            RecalculateTotals(sale, pendingItem: item);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(saleId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto?> UpdateItemAsync(
            int saleId, int itemId, UpdateSaleItemInlineDto dto, int tenantId)
        {
            var sale = await FindByIdAsync(saleId, tenantId);

            if (sale is null)
                return null;

            // Block editing paid invoices
            if (sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot edit items on a paid invoice '{sale.InvoiceNumber}'.");

            var item = sale.SaleItems?
                .FirstOrDefault(si => si.SaleItemId == itemId);

            if (item is null)
                return null;

            // Validate stock — available = current stock + this item's existing reservation
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {item.ProductId} was not found.");

            int availableStock = product.Quantity + item.Quantity;

            if (availableStock < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {availableStock}, Requested: {dto.Quantity}.");

            // Apply updates — keep existing price if not provided
            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice ??item.UnitPrice;
            item.LineTotal = item.Quantity * item.UnitPrice;

            // Recalculate totals
            RecalculateTotals(sale);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(saleId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // REMOVE ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto?> RemoveItemAsync(int saleId, int itemId, int tenantId)
        {
            var sale = await FindByIdAsync(saleId, tenantId);

            if (sale is null)
                return null;

            // Block editing paid invoices
            if (sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot remove items from a paid invoice '{sale.InvoiceNumber}'.");

            var item = sale.SaleItems?
                .FirstOrDefault(si => si.SaleItemId == itemId);

            if (item is null)
                return null;

            _context.SaleItems.Remove(item);

            // Remove from in-memory collection before recalculating
            sale.SaleItems!.Remove(item);
            RecalculateTotals(sale);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(saleId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — recalculate sale totals from current items
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Recalculates Subtotal and TotalAmount from current in-memory items.
        /// pendingItem — a new item not yet in the SaleItems collection.
        /// </summary>
        private static void RecalculateTotals(Sale sale, SaleItem? pendingItem = null)
        {
            decimal subtotal = sale.SaleItems?.Sum(si => si.LineTotal) ?? 0;

            if (pendingItem is not null)
                subtotal += pendingItem.LineTotal;

            sale.Subtotal = subtotal;
            sale.TotalAmount = subtotal + sale.TaxAmount;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — stock deduction on payment
        // ─────────────────────────────────────────────────────────────────────
        private async Task DeductStockAsync(Sale sale, int tenantId)
        {
            var items = await _context.SaleItems
                .Where(si => si.SaleId == sale.SaleId
                          && si.TenantId == tenantId)
                .ToListAsync();

            foreach (var item in items)
            {
                // Load product directly — ensures EF Core tracks it for update
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId
                                           && p.TenantId == tenantId);

                if (product is null) continue;

                product.Quantity -= item.Quantity;

                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = item.ProductId,
                    MovementType = MovementType.Sale,
                    Quantity = item.Quantity,
                    ReferenceNumber = sale.InvoiceNumber,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Stock deducted on payment of invoice {sale.InvoiceNumber}",
                    TenantId = tenantId
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private async Task<Sale?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
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
            CreatedBy = sale.CreatedBy,
            Items = sale.SaleItems?.Select(si => new SaleItemDto
            {
                SaleItemId = si.SaleItemId,
                ProductId = si.ProductId,
                ProductName = si.Product?.Name ?? string.Empty,
                ProductCode = si.Product?.ProductCode ?? string.Empty,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                LineTotal = si.LineTotal
            }).ToList() ?? new List<SaleItemDto>(),
            ItemCount = sale.SaleItems?.Count ?? 0
        };
    }
}
