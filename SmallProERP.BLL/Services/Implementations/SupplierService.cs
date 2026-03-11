using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.ProductDTOS;
using SmallProERP.Models.DTOs.PurchaseOrderDtos;
using SmallProERP.Models.DTOs.SupplierDtos;
using SmallProERP.Models.Entities;

namespace SmallProERP.BLL.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly SmallProDbContext _context;

        public SupplierService(SmallProDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync(int tenantId)
        {

            var suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var supplierDtos = new List<SupplierDto>();

            foreach (var supplier in suppliers)
            {
                var productCount = await _context.Products
                    .CountAsync(p => p.SupplierId == supplier.SupplierId && p.TenantId == tenantId);

                var poCount = await _context.PurchaseOrders
                    .CountAsync(po => po.SupplierId == supplier.SupplierId && po.TenantId == tenantId);

                supplierDtos.Add(MapToDto(supplier, productCount, poCount));
            }

            return supplierDtos;

        }

        public async Task<SupplierDto?> GetByIdAsync(int id, int tenantId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);

            if (supplier == null)
                return null;

            var productCount = await _context.Products
                .CountAsync(p => p.SupplierId == supplier.SupplierId && p.TenantId == tenantId);

            var poCount = await _context.PurchaseOrders
                .CountAsync(po => po.SupplierId == supplier.SupplierId && po.TenantId == tenantId);

            return MapToDto(supplier, productCount, poCount);
        }

        public async Task<SupplierDetailsDto?> GetDetailsAsync(int id, int tenantId)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products!)
                .Include(s => s.PurchaseOrders!)
                    .ThenInclude(po => po.PurchaseOrderItems!)
                .FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);

            if (supplier == null)
                return null;

            return new SupplierDetailsDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                CreatedAt = supplier.CreatedAt,
                Products = supplier.Products?.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Quantity = p.Quantity,
                    MinimumStockLevel = p.MinimumStockLevel,
                    PurchasePrice = p.PurchasePrice,
                    SellingPrice = p.SellingPrice,
                    ProfitMargin = p.SellingPrice - p.PurchasePrice,
                    IsLowStock = p.Quantity < p.MinimumStockLevel,
                    StockDeficit = p.Quantity < p.MinimumStockLevel ? p.MinimumStockLevel - p.Quantity : 0,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<ProductDto>(),
                PurchaseOrders = supplier.PurchaseOrders?.Select(po => new PurchaseOrderDto
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    PONumber = po.PONumber,
                    SupplierId = po.SupplierId,
                    SupplierName = supplier.Name,
                    TotalAmount = po.TotalAmount,
                    Status = po.Status,
                    StatusText = po.Status.ToString(),
                    OrderDate = po.OrderDate,
                    ReceivedDate = po.ReceivedDate,
                    CreatedAt = po.CreatedAt
                }).ToList() ?? new List<PurchaseOrderDto>(),
                TotalProductsSupplied = supplier.Products?.Count ?? 0,
                TotalPurchaseOrders = supplier.PurchaseOrders?.Count ?? 0,
                TotalAmountSpent = supplier.PurchaseOrders?.Sum(po => po.TotalAmount) ?? 0
            };
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int tenantId)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                Address = dto.Address,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return MapToDto(supplier, 0, 0);
        }

        public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto, int tenantId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);

            if (supplier == null)
                return false;

            supplier.Name = dto.Name;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone ?? string.Empty;
            supplier.Address = dto.Address;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);

            if (supplier == null)
                return false;

            if (supplier.Products?.Any() == true)
            {
                throw new InvalidOperationException(
                    "Cannot delete supplier that has products. Please reassign or delete products first.");
            }

            if (supplier.PurchaseOrders?.Any() == true)
            {
                throw new InvalidOperationException(
                    "Cannot delete supplier that has purchase order history.");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SupplierDto>> SearchAsync(string searchTerm, int tenantId)
        {
            searchTerm = searchTerm.ToLower().Trim();

            var suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId
                         && (s.Name.ToLower().Contains(searchTerm)
                          || (s.Email != null && s.Email.ToLower().Contains(searchTerm))
                          || (s.Phone != null && s.Phone.Contains(searchTerm))))
                .OrderBy(s => s.Name)
                .ToListAsync();

            var supplierDtos = new List<SupplierDto>();

            foreach (var supplier in suppliers)
            {
                var productCount = await _context.Products
                    .CountAsync(p => p.SupplierId == supplier.SupplierId && p.TenantId == tenantId);

                var poCount = await _context.PurchaseOrders
                    .CountAsync(po => po.SupplierId == supplier.SupplierId && po.TenantId == tenantId);

                supplierDtos.Add(MapToDto(supplier, productCount, poCount));
            }

            return supplierDtos;
        }

        private static SupplierDto MapToDto(Supplier supplier, int productCount, int poCount)
        {
            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                ProductCount = productCount,
                PurchaseOrderCount = poCount,
                CreatedAt = supplier.CreatedAt
            };
        }
    }
}