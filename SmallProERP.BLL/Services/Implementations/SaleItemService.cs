using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.SaleItemDto;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class SaleItemService : ISaleItemService
    {
        private readonly SmallProDbContext _context;

        public SaleItemService(SmallProDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY SALE ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<SaleItemDto>> GetBySaleIdAsync(int saleId, int tenantId)
        {
            var items = await _context.SaleItems
                .Where(si => si.SaleId == saleId
                          && si.TenantId == tenantId)
                .Include(si => si.Product)
                .OrderBy(si => si.SaleItemId)
                .ToListAsync();

            return items.Select(MapToDto);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleItemDto?> GetByIdAsync(int id, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);
            return item is null ? null : MapToDto(item);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<SaleItemDto> CreateAsync(CreateSaleItemDto dto, int tenantId)
        {
            // 1 — Verify sale exists in this tenant
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == dto.SaleId
                                       && s.TenantId == tenantId);

            if (sale is null)
                throw new InvalidOperationException(
                    $"Sale with ID {dto.SaleId} was not found in this tenant.");

            // 2 — Block editing a paid invoice
            if (sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot add items to a paid invoice. Invoice '{sale.InvoiceNumber}' is already paid.");

            // 3 — Verify product exists in this tenant
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {dto.ProductId} was not found in this tenant.");

            // 4 — Block if insufficient stock
            if (product.Quantity < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {product.Quantity}, Requested: {dto.Quantity}.");

            // 5 — Build and save the item
            var item = new SaleItem
            {
                SaleId = dto.SaleId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                LineTotal = dto.Quantity * dto.UnitPrice,
                TenantId = tenantId
            };

            _context.SaleItems.Add(item);

            // 6 — Recalculate parent sale totals
            await RecalculateSaleTotalsAsync(sale, dto.SaleId, tenantId, pendingItem: item);

            await _context.SaveChangesAsync();

            // Load product navigation for the response
            item.Product = product;

            return MapToDto(item);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateSaleItemDto dto, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);

            if (item is null)
                return false;

            // Block editing a paid invoice
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == item.SaleId
                                       && s.TenantId == tenantId);

            if (sale is not null && sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot edit items on a paid invoice. Invoice '{sale.InvoiceNumber}' is already paid.");

            // Validate stock — available = current stock + qty already reserved by this item
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId
                                       && p.TenantId == tenantId);

            if (product is null)
                throw new InvalidOperationException(
                    $"Product with ID {item.ProductId} was not found in this tenant.");

            int availableStock = product.Quantity + item.Quantity;

            if (availableStock < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {availableStock}, Requested: {dto.Quantity}.");

            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice;
            item.LineTotal = dto.Quantity * dto.UnitPrice;

            // Recalculate parent sale totals
            if (sale is not null)
                await RecalculateSaleTotalsAsync(sale, item.SaleId, tenantId);

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var item = await FindByIdAsync(id, tenantId);

            if (item is null)
                return false;

            // Block editing a paid invoice
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == item.SaleId
                                       && s.TenantId == tenantId);

            if (sale is not null && sale.IsPaid)
                throw new InvalidOperationException(
                    $"Cannot remove items from a paid invoice. Invoice '{sale.InvoiceNumber}' is already paid.");

            var saleId = item.SaleId;
            _context.SaleItems.Remove(item);

            // Recalculate parent sale totals after removal
            if (sale is not null)
                await RecalculateSaleTotalsAsync(
                    sale, saleId, tenantId, excludeItemId: id);

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task<SaleItem?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.SaleItems
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.SaleItemId == id
                                        && si.TenantId == tenantId);
        }

        /// <summary>
        /// Recalculates Subtotal and TotalAmount on the parent Sale
        /// by summing all current line items.
        ///
        /// pendingItem   — a new item not yet saved, included in the sum.
        /// excludeItemId — an item being deleted, excluded from the sum.
        ///
        /// TaxAmount is preserved as-is (set manually on the Sale header).
        /// TotalAmount = Subtotal + TaxAmount.
        /// </summary>
        private async Task RecalculateSaleTotalsAsync(
            Sale sale,
            int saleId,
            int tenantId,
            SaleItem? pendingItem = null,
            int? excludeItemId = null)
        {
            var savedItems = await _context.SaleItems
                .Where(si => si.SaleId == saleId
                          && si.TenantId == tenantId)
                .ToListAsync();

            // Exclude the item being deleted (not yet removed from DB)
            if (excludeItemId.HasValue)
                savedItems = savedItems
                    .Where(si => si.SaleItemId != excludeItemId.Value)
                    .ToList();

            decimal subtotal = savedItems.Sum(si => si.LineTotal);

            // Include the pending new item (not yet in DB)
            if (pendingItem is not null)
                subtotal += pendingItem.LineTotal;

            sale.Subtotal = subtotal;
            sale.TotalAmount = subtotal + sale.TaxAmount;
        }

        /// <summary>
        /// Called from SaleService.MarkPaidAsync when IsPaid transitions to true.
        /// Deducts stock for every item in the sale and creates InventoryMovement records.
        /// This method is public so SaleService can call it after marking a sale as paid.
        /// </summary>
        public async Task DeductStockForSaleAsync(int saleId, int tenantId)
        {
            var items = await _context.SaleItems
                .Where(si => si.SaleId == saleId
                          && si.TenantId == tenantId)
                .Include(si => si.Product)
                .ToListAsync();

            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == saleId
                                       && s.TenantId == tenantId);

            foreach (var item in items)
            {
                if (item.Product is null) continue;

                // Deduct stock from product
                item.Product.Quantity -= item.Quantity;

                // Create InventoryMovement record for audit trail
                var movement = new InventoryMovement
                {
                    ProductId = item.ProductId,
                    MovementType = MovementType.Sale,
                    Quantity = item.Quantity,
                    ReferenceNumber = sale?.InvoiceNumber,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Stock deducted on payment of invoice {sale?.InvoiceNumber}",
                    TenantId = tenantId
                };

                _context.InventoryMovements.Add(movement);
            }

            await _context.SaveChangesAsync();
        }

        private static SaleItemDto MapToDto(SaleItem item) => new()
        {
            SaleItemId = item.SaleItemId,
            SaleId = item.SaleId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductCode = item.Product?.ProductCode ?? string.Empty,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal
        };
    }
}
