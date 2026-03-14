using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.QuotationItemDto;
using SmallProERP.Models.DTOs.QuotationItemDtos;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class QuotationItemService : IQuotationItemService
    {
        private readonly SmallProDbContext _context;

        public QuotationItemService(SmallProDbContext context)
        {
            _context = context;
        }

        
        public async Task<IEnumerable<QuotationItemDto>> GetByQuotationIdAsync(
            int quotationId, int tenantId)
        {
            var items = await _context.QuotationItems
                .Where(qi => qi.QuotationId == quotationId
                          && qi.TenantId == tenantId)
                .Include(qi => qi.Product)
                .OrderBy(qi => qi.QuotationItemId)
                .ToListAsync();

            return items.Select(MapToDto);
        }

     
        public async Task<QuotationItemDto?> GetByIdAsync(int id, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);
            return item is null ? null : MapToDto(item);
        }

        
        public async Task<QuotationItemDto> CreateAsync(
            CreateQuotationItemDto dto, int tenantId)
        {
            // 1 — Verify quotation exists in this tenant
            var quotation = await _context.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == dto.QuotationId
                                       && q.TenantId == tenantId);

            if (quotation is null)
                throw new InvalidOperationException(
                    $"Quotation with ID {dto.QuotationId} was not found in this tenant.");

            // 2 — Verify product exists in this tenant
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {dto.ProductId} was not found in this tenant.");

            // 3 — Validate stock availability
            if (product.Quantity < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {product.Quantity}, Requested: {dto.Quantity}.");

            // 4 — Calculate LineTotal
            var lineTotal = dto.Quantity * dto.UnitPrice;

            var item = new QuotationItem
            {
                QuotationId = dto.QuotationId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                LineTotal = lineTotal,
                TenantId = tenantId
            };

            _context.QuotationItems.Add(item);

            // 5 — Recalculate parent quotation totals
            await RecalculateQuotationTotalsAsync(quotation, dto.QuotationId, tenantId, pendingItem: item);

            await _context.SaveChangesAsync();

            // Load product navigation for the response DTO
            item.Product = product;

            return MapToDto(item);
        }

  
        public async Task<bool> UpdateAsync(
            int id, UpdateQuotationItemDto dto, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);

            if (item is null)
                return false;

            // Validate stock — allow if new qty <= old qty (no extra stock needed)
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {item.ProductId} was not found in this tenant.");

            // Available stock = current stock + what this item already reserved
            int availableStock = product.Quantity + item.Quantity;

            if (availableStock < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {availableStock}, Requested: {dto.Quantity}.");

            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice;
            item.LineTotal = dto.Quantity * dto.UnitPrice;

            // Recalculate parent quotation totals
            var quotation = await _context.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == item.QuotationId
                                       && q.TenantId == tenantId);

            if (quotation is not null)
                await RecalculateQuotationTotalsAsync(quotation, item.QuotationId, tenantId);

            await _context.SaveChangesAsync();
            return true;
        }

 
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);

            if (item is null)
                return false;

            var quotationId = item.QuotationId;

            _context.QuotationItems.Remove(item);

            // Recalculate parent quotation totals after removal
            var quotation = await _context.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId
                                       && q.TenantId == tenantId);

            if (quotation is not null)
                await RecalculateQuotationTotalsAsync(
                    quotation, quotationId, tenantId, excludeItemId: id);

            await _context.SaveChangesAsync();
            return true;
        }

        

        private async Task<QuotationItem?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.QuotationItems
                .Include(qi => qi.Product)
                .FirstOrDefaultAsync(qi => qi.QuotationItemId == id
                                        && qi.TenantId == tenantId);
        }

        private async Task RecalculateQuotationTotalsAsync(
            Quotation quotation,
            int quotationId,
            int tenantId,
            QuotationItem? pendingItem = null,
            int? excludeItemId = null)
        {
            // Load all saved items for this quotation
            var savedItems = await _context.QuotationItems
                .Where(qi => qi.QuotationId == quotationId
                          && qi.TenantId == tenantId)
                .ToListAsync();

            // Exclude item being deleted (it hasn't been removed from DB yet)
            if (excludeItemId.HasValue)
                savedItems = savedItems
                    .Where(qi => qi.QuotationItemId != excludeItemId.Value)
                    .ToList();

            decimal subtotal = savedItems.Sum(qi => qi.LineTotal);

            // Include the pending new item (not yet in DB)
            if (pendingItem is not null)
                subtotal += pendingItem.LineTotal;

            quotation.Subtotal = subtotal;
            quotation.TotalAmount = subtotal + quotation.TaxAmount;
        }

        
        private static QuotationItemDto MapToDto(QuotationItem item)
        {
            return new QuotationItemDto
            {
                QuotationItemId = item.QuotationItemId,
                QuotationId = item.QuotationId,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductCode = item.Product?.ProductCode ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            };
        }
    }
}
