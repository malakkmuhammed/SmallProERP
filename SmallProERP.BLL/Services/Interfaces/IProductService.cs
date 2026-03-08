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
        Task<IEnumerable<ProductDto>> GetAllAsync();

        
        /// Returns a single product by ID.
        /// Returns null if not found or belongs to a different tenant.
        
        Task<ProductDto?> GetByIdAsync(int id);

        
        /// Creates a new product.
        /// Throws InvalidOperationException if ProductCode already exists for this tenant.
       
        Task<ProductDto> CreateAsync(CreateProductDto dto);

        /// Updates an existing product.
        /// Returns false if the product was not found (or belongs to another tenant).
        
        Task<bool> UpdateAsync(int id, UpdateProductDto dto);

        
        /// Deletes a product by ID.
        /// Returns false if the product was not found (or belongs to another tenant).
       
        Task<bool> DeleteAsync(int id);

        /// Returns products where Quantity is below MinimumStockLevel
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    }
}
