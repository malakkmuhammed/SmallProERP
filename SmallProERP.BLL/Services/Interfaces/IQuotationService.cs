using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.SaleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IQuotationService
    {
        
        Task<IEnumerable<QuotationSummaryDto>> GetAllAsync(int tenantId, string? search = null);

        
        Task<QuotationDto?> GetByIdAsync(int id, int tenantId);

        
        Task<IEnumerable<QuotationSummaryDto>> GetByCustomerIdAsync(int customerId, int tenantId);

       
        Task<QuotationStatisticsDto> GetStatisticsAsync(int tenantId);

       
        Task<QuotationDto> CreateAsync(CreateQuotationDto dto, int tenantId, int? userId);

        Task<bool> UpdateAsync(int id, UpdateQuotationDto dto, int tenantId);

        Task<bool> DeleteAsync(int id, int tenantId);

        Task<bool> ChangeStatusAsync(int id, int status, int tenantId);

      
        Task<SaleDto> ConvertToSaleAsync(int quotationId, ConvertQuotationToSaleDto dto, int tenantId, int? userId);


        
        Task<QuotationDto> AddItemAsync(int quotationId, AddQuotationItemDto dto, int tenantId);

        Task<QuotationDto?> UpdateItemAsync(int quotationId, int itemId, UpdateQuotationItemInlineDto dto, int tenantId);

    
        Task<QuotationDto?> RemoveItemAsync(int quotationId, int itemId, int tenantId);
    }
}
