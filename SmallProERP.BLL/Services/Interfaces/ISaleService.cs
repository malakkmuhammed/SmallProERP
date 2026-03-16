using SmallProERP.Models.DTOs.SaleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleDto>> GetAllAsync(int tenantId, string? search = null);

        Task<SaleDto?> GetByIdAsync(int id, int tenantId);

       
        Task<IEnumerable<SaleDto>> GetByCustomerIdAsync(int customerId, int tenantId);

        Task<SaleStatisticsDto> GetStatisticsAsync(int tenantId);

        
        Task<IEnumerable<UnpaidInvoiceAlertDto>> GetUnpaidAlertsAsync(int tenantId);

     
        Task<SaleDto> CreateAsync(CreateSaleDto dto, int tenantId, int? userId);

       
        Task<bool> UpdateAsync(int id, UpdateSaleDto dto, int tenantId);

   
        Task<bool> DeleteAsync(int id, int tenantId);

        Task<bool> MarkPaidAsync(int id, MarkSalePaidDto dto, int tenantId);

       
        Task<SaleDto> AddItemAsync(int saleId, AddSaleItemDto dto, int tenantId);

   
        Task<SaleDto?> UpdateItemAsync(int saleId, int itemId, UpdateSaleItemInlineDto dto, int tenantId);

        Task<SaleDto?> RemoveItemAsync(int saleId, int itemId, int tenantId);
    }
}
