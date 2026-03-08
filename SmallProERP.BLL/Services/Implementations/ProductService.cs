using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.ProductDTOS;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{

    public class ProductService : IProductService
    {
        private readonly SmallProDbContext _context;

        // ── Temporary fixed tenant until JWT is wired in Phase 7 ─────────────
        private const int FixedTenantId = 1;

        public ProductService(SmallProDbContext context)
        {
            _context = context;
        }

        // GET ALL
        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Where(p => p.TenantId == FixedTenantId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        // GET BY ID
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await FindByIdAsync(id);
            return product is null ? null : MapToDto(product);
        }

        // CREATE
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            // Enforce unique ProductCode per tenant
            bool codeExists = await _context.Products
                .AnyAsync(p => p.TenantId == FixedTenantId
                            && p.ProductCode == dto.ProductCode);

            if (codeExists)
                throw new InvalidOperationException(
                    $"Product code '{dto.ProductCode}' already exists for this tenant.");

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
                TenantId = FixedTenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await FindByIdAsync(id);

            if (product is null)
                return false;

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

        // DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await FindByIdAsync(id);

            if (product is null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // LOW STOCK
        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await _context.Products
                .Where(p => p.TenantId == FixedTenantId
                         && p.Quantity < p.MinimumStockLevel)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        // PRIVATE HELPERS
       
        private async Task<Product?> FindByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id
                                       && p.TenantId == FixedTenantId);
        }

        /// Maps a Product entity to a ProductDto
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
                SupplierId = product.SupplierId,
                IsLowStock = product.Quantity < product.MinimumStockLevel,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
