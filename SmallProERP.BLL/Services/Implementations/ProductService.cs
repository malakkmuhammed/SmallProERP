
using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.ProductDTOS;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly SmallProDbContext _context;

        public ProductService(SmallProDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(int tenantId)
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(int id, int tenantId)
        {
            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TenantId == tenantId);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto?> GetByCodeAsync(string code, int tenantId)
        {
            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductCode == code && p.TenantId == tenantId);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto, int tenantId)
        {
            bool codeExists = await _context.Products
                .IgnoreQueryFilters()
                .AnyAsync(p => p.TenantId == tenantId && p.ProductCode == dto.ProductCode);

            if (codeExists)
            {
                throw new InvalidOperationException(
                    $"Product with code '{dto.ProductCode}' already exists.");
            }

            if (dto.SellingPrice < dto.PurchasePrice)
            {
                throw new InvalidOperationException(
                    "Selling price cannot be less than purchase price.");
            }

            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierId == dto.SupplierId.Value && s.TenantId == tenantId);

                if (!supplierExists)
                {
                    throw new InvalidOperationException(
                        $"Supplier with ID {dto.SupplierId.Value} not found.");
                }
            }

            var product = new Product
            {
                ProductCode = dto.ProductCode,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Quantity = dto.Quantity,
                MinimumStockLevel = dto.MinimumStockLevel,
                PurchasePrice = dto.PurchasePrice,
                SellingPrice = dto.SellingPrice,
                SupplierId = dto.SupplierId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Supplier).LoadAsync();

            return MapToDto(product);
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto, int tenantId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TenantId == tenantId);

            if (product == null)
                return false;

            if (dto.SellingPrice < dto.PurchasePrice)
            {
                throw new InvalidOperationException(
                    "Selling price cannot be less than purchase price.");
            }

            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierId == dto.SupplierId.Value && s.TenantId == tenantId);

                if (!supplierExists)
                {
                    throw new InvalidOperationException(
                        $"Supplier with ID {dto.SupplierId.Value} not found.");
                }
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Quantity = dto.Quantity;
            product.MinimumStockLevel = dto.MinimumStockLevel;
            product.PurchasePrice = dto.PurchasePrice;
            product.SellingPrice = dto.SellingPrice;
            product.SupplierId = dto.SupplierId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var product = await _context.Products
                .Include(p => p.PurchaseOrderItems)
                .Include(p => p.SaleItems)
                .Include(p => p.InventoryMovements)
                .Include(p => p.QuotationItems!)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TenantId == tenantId);

            if (product == null)
                return false;

            if (product.PurchaseOrderItems?.Any() == true)
            {
                throw new InvalidOperationException(
                    "Cannot delete product that has purchase order history.");
            }

            if (product.SaleItems?.Any() == true)
            {
                throw new InvalidOperationException(
                    "Cannot delete product that has sales history.");
            }

            if (product.InventoryMovements?.Any() == true)
            {
                throw new InvalidOperationException(
                    "Cannot delete product that has inventory movement history.");
            }
            if (product.QuotationItems?.Any() == true)  // ⭐ ADDED
            {
                throw new InvalidOperationException(
                    "Cannot delete product that has been quoted to customers.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, int tenantId)
        {
            searchTerm = searchTerm.ToLower().Trim();

            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId
                         && (p.ProductCode.ToLower().Contains(searchTerm)
                          || p.Name.ToLower().Contains(searchTerm)
                          || (p.Category != null && p.Category.ToLower().Contains(searchTerm))))
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, int tenantId)
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.Category == category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetBySupplierAsync(int supplierId, int tenantId)
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.SupplierId == supplierId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int tenantId)
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.Quantity < p.MinimumStockLevel)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<bool> AdjustStockAsync(StockAdjustmentDto dto, int tenantId, int userId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId && p.TenantId == tenantId);

            if (product == null)
                return false;

            int newQuantity = product.Quantity + dto.AdjustmentQuantity;

            if (newQuantity < 0)
            {
                throw new InvalidOperationException(
                    $"Adjustment would result in negative stock. Current: {product.Quantity}, Adjustment: {dto.AdjustmentQuantity}");
            }

            product.Quantity = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            var movement = new InventoryMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.Adjustment,
                Quantity = dto.AdjustmentQuantity,
                ReferenceNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                MovementDate = DateTime.UtcNow,
                Notes = $"{dto.Reason}{(string.IsNullOrEmpty(dto.Notes) ? "" : $" - {dto.Notes}")}",
                TenantId = tenantId
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync(int tenantId)
        {
            return await _context.Products
                .Where(p => p.TenantId == tenantId && p.Category != null)
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalInventoryValueAsync(int tenantId)
        {
            return await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => p.Quantity * p.PurchasePrice);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Quantity = product.Quantity,
                MinimumStockLevel = product.MinimumStockLevel,
                PurchasePrice = product.PurchasePrice,
                SellingPrice = product.SellingPrice,
                ProfitMargin = product.SellingPrice - product.PurchasePrice,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                IsLowStock = product.Quantity < product.MinimumStockLevel,
                StockDeficit = product.Quantity < product.MinimumStockLevel
                    ? product.MinimumStockLevel - product.Quantity
                    : 0,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
