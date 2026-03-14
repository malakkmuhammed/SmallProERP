using SmallProERP.Models.DTOs.QuotationItemDto;
using SmallProERP.Models.DTOs.QuotationItemDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IQuotationItemService
    {
       
        Task<IEnumerable<QuotationItemDto>> GetByQuotationIdAsync(int quotationId, int tenantId);

        Task<QuotationItemDto?> GetByIdAsync(int id, int tenantId);

       
        Task<QuotationItemDto> CreateAsync(CreateQuotationItemDto dto, int tenantId);

     
        Task<bool> UpdateAsync(int id, UpdateQuotationItemDto dto, int tenantId);

        
        Task<bool> DeleteAsync(int id, int tenantId);
    }
}
