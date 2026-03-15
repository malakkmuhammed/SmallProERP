using SmallProERP.Models.DTOs.SaleItemDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ISaleItemService
    {
    
        Task<IEnumerable<SaleItemDto>> GetBySaleIdAsync(int saleId, int tenantId);

    
        Task<SaleItemDto?> GetByIdAsync(int id, int tenantId);

    
        Task<SaleItemDto> CreateAsync(CreateSaleItemDto dto, int tenantId);

  
        Task<bool> UpdateAsync(int id, UpdateSaleItemDto dto, int tenantId);

       
        Task<bool> DeleteAsync(int id, int tenantId);
    }
}
