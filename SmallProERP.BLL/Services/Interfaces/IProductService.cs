using SmallProERP.Models.DTOs.ProductDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(int tenantId);
        Task<ProductDto?> GetByIdAsync(int id, int tenantId);
        Task<ProductDto?> GetByCodeAsync(string code, int tenantId);
        Task<ProductDto> CreateAsync(CreateProductDto dto, int tenantId);
        Task<bool> UpdateAsync(int id, UpdateProductDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, int tenantId);
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, int tenantId);
        Task<IEnumerable<ProductDto>> GetBySupplierAsync(int supplierId, int tenantId);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int tenantId);
        Task<bool> AdjustStockAsync(StockAdjustmentDto dto, int tenantId, int userId);
        Task<IEnumerable<string>> GetAllCategoriesAsync(int tenantId);
        Task<decimal> GetTotalInventoryValueAsync(int tenantId);
    }
}
