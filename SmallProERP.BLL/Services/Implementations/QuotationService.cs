using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.QuotationItemDto;
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
    public class QuotationService : IQuotationService
    {
        private readonly SmallProDbContext _context;

        public QuotationService(SmallProDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<QuotationSummaryDto>> GetAllAsync(
            int tenantId, string? search = null)
        {
            var query = _context.Quotations
                .Where(q => q.TenantId == tenantId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(q =>
                    q.Customer != null &&
                    (q.Customer.Name.ToLower().Contains(term) ||
                    (q.Customer.Company != null &&
                     q.Customer.Company.ToLower().Contains(term))));
            }

            var quotations = await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quotations.Select(MapToSummaryDto);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID — full response with items
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationDto?> GetByIdAsync(int id, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);
            return quotation is null ? null : MapToDto(quotation);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY CUSTOMER ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<QuotationSummaryDto>> GetByCustomerIdAsync(
            int customerId, int tenantId)
        {
            var quotations = await _context.Quotations
                .Where(q => q.TenantId == tenantId
                         && q.CustomerId == customerId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quotations.Select(MapToSummaryDto);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET STATISTICS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationStatisticsDto> GetStatisticsAsync(int tenantId)
        {
            var quotations = await _context.Quotations
                .Where(q => q.TenantId == tenantId)
                .ToListAsync();

            return new QuotationStatisticsDto
            {
                TotalQuotations = quotations.Count,
                DraftCount = quotations.Count(q => q.Status == QuotationStatus.Draft),
                SentCount = quotations.Count(q => q.Status == QuotationStatus.Sent),
                AcceptedCount = quotations.Count(q => q.Status == QuotationStatus.Accepted),
                RejectedCount = quotations.Count(q => q.Status == QuotationStatus.Rejected),
                TotalValue = quotations.Sum(q => q.TotalAmount),
                AcceptedValue = quotations.Where(q => q.Status == QuotationStatus.Accepted)
                                            .Sum(q => q.TotalAmount),
                PendingValue = quotations.Where(q => q.Status == QuotationStatus.Draft
                                                     || q.Status == QuotationStatus.Sent)
                                            .Sum(q => q.TotalAmount)
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE — quotation + items in one operation
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationDto> CreateAsync(
            CreateQuotationDto dto, int tenantId, int? userId)
        {
            // 1 — Verify customer
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            // 2 — Unique QuotationNumber per tenant
            bool numberExists = await _context.Quotations
                .AnyAsync(q => q.TenantId == tenantId
                            && q.QuotationNumber == dto.QuotationNumber);

            if (numberExists)
                throw new InvalidOperationException(
                    $"Quotation number '{dto.QuotationNumber}' already exists in this tenant.");

            // 3 — Validate all products and stock upfront
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

            // 4 — Build items using SellingPrice (or optional override)
            var quotationItems = dto.Items.Select(i =>
            {
                var product = products.First(p => p.ProductId == i.ProductId);
                var unitPrice =  product.SellingPrice;
                return new QuotationItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = i.Quantity * unitPrice,
                    TenantId = tenantId
                };
            }).ToList();

            decimal subtotal = quotationItems.Sum(qi => qi.LineTotal);

            // 5 — Save quotation with items
            var quotation = new Quotation
            {
                QuotationNumber = dto.QuotationNumber,
                CustomerId = dto.CustomerId,
                Subtotal = subtotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = subtotal + dto.TaxAmount,
                Status = QuotationStatus.Draft,
                QuotationDate = dto.QuotationDate,
                ValidUntil = dto.ValidUntil,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                QuotationItems = quotationItems
            };

            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();

            var created = await FindByIdAsync(quotation.QuotationId, tenantId);
            return MapToDto(created!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE HEADER
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateQuotationDto dto, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            if (quotation.CustomerId != dto.CustomerId)
            {
                bool customerExists = await _context.Customers
                    .AnyAsync(c => c.CustomerId == dto.CustomerId
                                && c.TenantId == tenantId);

                if (!customerExists)
                    throw new InvalidOperationException(
                        $"Customer with ID {dto.CustomerId} was not found in this tenant.");
            }

            quotation.CustomerId = dto.CustomerId;
            quotation.TaxAmount = dto.TaxAmount;
            quotation.TotalAmount = quotation.Subtotal + dto.TaxAmount;
            quotation.QuotationDate = dto.QuotationDate;
            quotation.ValidUntil = dto.ValidUntil;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHANGE STATUS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> ChangeStatusAsync(int id, int status, int tenantId)
        {
            if (!Enum.IsDefined(typeof(QuotationStatus), status))
                throw new InvalidOperationException(
                    $"Invalid status value '{status}'. " +
                    "Valid values: 1=Draft, 2=Sent, 3=Accepted, 4=Rejected.");

            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            quotation.Status = (QuotationStatus)status;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CONVERT TO SALE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleDto> ConvertToSaleAsync(
            int quotationId, ConvertQuotationToSaleDto dto, int tenantId, int? userId)
        {
            var quotation = await FindByIdAsync(quotationId, tenantId);

            if (quotation is null)
                throw new InvalidOperationException(
                    $"Quotation with ID {quotationId} was not found in this tenant.");

            if (quotation.Status != QuotationStatus.Accepted)
                throw new InvalidOperationException(
                    $"Only Accepted quotations can be converted. Current status: {quotation.Status}.");

            bool alreadyConverted = await _context.Sales
                .AnyAsync(s => s.QuotationId == quotationId && s.TenantId == tenantId);

            if (alreadyConverted)
                throw new InvalidOperationException(
                    $"Quotation with ID {quotationId} has already been converted to a sale.");

            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            // Copy line items from quotation to sale
            var saleItems = quotation.QuotationItems?.Select(qi => new SaleItem
            {
                ProductId = qi.ProductId,
                Quantity = qi.Quantity,
                UnitPrice = qi.UnitPrice,
                LineTotal = qi.LineTotal,
                TenantId = tenantId
            }).ToList() ?? new List<SaleItem>();

            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                QuotationId = quotation.QuotationId,
                CustomerId = quotation.CustomerId,
                Subtotal = quotation.Subtotal,
                TaxAmount = quotation.TaxAmount,
                TotalAmount = quotation.TotalAmount,
                IsPaid = false,
                InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
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

            return MapSaleToDto(sale, quotation);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ADD ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationDto> AddItemAsync(
            int quotationId, AddQuotationItemDto dto, int tenantId)
        {
            var quotation = await FindByIdAsync(quotationId, tenantId);

            if (quotation is null)
                throw new InvalidOperationException(
                    $"Quotation with ID {quotationId} was not found in this tenant.");

            // Block editing Accepted or Rejected quotations
            if (quotation.Status == QuotationStatus.Accepted ||
                quotation.Status == QuotationStatus.Rejected)
                throw new InvalidOperationException(
                    $"Cannot add items to a {quotation.Status} quotation '{quotation.QuotationNumber}'. " +
                    $"Only Draft and Sent quotations can be edited.");

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

            var unitPrice = product.SellingPrice;

            var item = new QuotationItem
            {
                QuotationId = quotationId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = unitPrice,
                LineTotal = dto.Quantity * unitPrice,
                TenantId = tenantId
            };

            _context.QuotationItems.Add(item);

            // Recalculate totals
            RecalculateTotals(quotation, pendingItem: item);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(quotationId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationDto?> UpdateItemAsync(
            int quotationId, int itemId, UpdateQuotationItemInlineDto dto, int tenantId)
        {
            var quotation = await FindByIdAsync(quotationId, tenantId);

            if (quotation is null)
                return null;

            // Block editing Accepted or Rejected quotations
            if (quotation.Status == QuotationStatus.Accepted ||
                quotation.Status == QuotationStatus.Rejected)
                throw new InvalidOperationException(
                    $"Cannot edit items on a {quotation.Status} quotation '{quotation.QuotationNumber}'.");

            var item = quotation.QuotationItems?
                .FirstOrDefault(qi => qi.QuotationItemId == itemId);

            if (item is null)
                return null;

            // Validate stock — available = current stock + existing reservation
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

            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice ?? item.UnitPrice;
            item.LineTotal = item.Quantity * item.UnitPrice;

            RecalculateTotals(quotation);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(quotationId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // REMOVE ITEM
        // ─────────────────────────────────────────────────────────────────────
        public async Task<QuotationDto?> RemoveItemAsync(
            int quotationId, int itemId, int tenantId)
        {
            var quotation = await FindByIdAsync(quotationId, tenantId);

            if (quotation is null)
                return null;

            // Block editing Accepted or Rejected quotations
            if (quotation.Status == QuotationStatus.Accepted ||
                quotation.Status == QuotationStatus.Rejected)
                throw new InvalidOperationException(
                    $"Cannot remove items from a {quotation.Status} quotation '{quotation.QuotationNumber}'.");

            var item = quotation.QuotationItems?
                .FirstOrDefault(qi => qi.QuotationItemId == itemId);

            if (item is null)
                return null;

            _context.QuotationItems.Remove(item);
            quotation.QuotationItems!.Remove(item);
            RecalculateTotals(quotation);

            await _context.SaveChangesAsync();

            var updated = await FindByIdAsync(quotationId, tenantId);
            return MapToDto(updated!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — recalculate totals from current items
        // ─────────────────────────────────────────────────────────────────────
        private static void RecalculateTotals(
            Quotation quotation, QuotationItem? pendingItem = null)
        {
            decimal subtotal = quotation.QuotationItems?.Sum(qi => qi.LineTotal) ?? 0;

            if (pendingItem is not null)
                subtotal += pendingItem.LineTotal;

            quotation.Subtotal = subtotal;
            quotation.TotalAmount = subtotal + quotation.TaxAmount;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private async Task<Quotation?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync(q => q.QuotationId == id
                                       && q.TenantId == tenantId);
        }

        private static QuotationDto MapToDto(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuotationNumber = q.QuotationNumber,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.Name ?? string.Empty,
            CustomerCompany = q.Customer?.Company,
            Subtotal = q.Subtotal,
            TaxAmount = q.TaxAmount,
            TotalAmount = q.TotalAmount,
            Status = q.Status,
            StatusName = q.Status.ToString(),
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            TenantId = q.TenantId,
            CreatedAt = q.CreatedAt,
            CreatedBy = q.CreatedBy,
            Items = q.QuotationItems?.Select(qi => new QuotationItemDto
            {
                QuotationItemId = qi.QuotationItemId,
                ProductId = qi.ProductId,
                ProductName = qi.Product?.Name ?? string.Empty,
                ProductCode = qi.Product?.ProductCode ?? string.Empty,
                Quantity = qi.Quantity,
                UnitPrice = qi.UnitPrice,
                LineTotal = qi.LineTotal
            }).ToList() ?? new List<QuotationItemDto>(),
            ItemCount = q.QuotationItems?.Count ?? 0
        };

        private static QuotationSummaryDto MapToSummaryDto(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuotationNumber = q.QuotationNumber,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.Name ?? string.Empty,
            CustomerCompany = q.Customer?.Company,
            Status = q.Status,
            StatusName = q.Status.ToString(),
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            TotalAmount = q.TotalAmount,
            ItemCount = q.QuotationItems?.Count ?? 0,
            CreatedAt = q.CreatedAt
        };

        private static SaleDto MapSaleToDto(Sale sale, Quotation quotation) => new()
        {
            SaleId = sale.SaleId,
            InvoiceNumber = sale.InvoiceNumber,
            QuotationId = sale.QuotationId,
            CustomerId = sale.CustomerId,
            CustomerName = quotation.Customer?.Name ?? string.Empty,
            Subtotal = sale.Subtotal,
            TaxAmount = sale.TaxAmount,
            TotalAmount = sale.TotalAmount,
            IsPaid = sale.IsPaid,
            InvoiceDate = sale.InvoiceDate,
            DueDate = sale.DueDate,
            PaymentMethod = sale.PaymentMethod,
            PaymentNotes = sale.PaymentNotes,
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
