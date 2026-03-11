using SmallProERP.Models.DTOs.PurchaseOrderDtos;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        
        Task<IEnumerable<PurchaseOrderDto>> GetAllAsync(int tenantId);
        Task<PurchaseOrderDto?> GetByIdAsync(int id, int tenantId);
        Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, int tenantId);
        Task<bool> UpdateAsync(int id, UpdatePurchaseOrderDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);

        
        Task<bool> SendPurchaseOrderAsync(int id, int tenantId);
        Task<bool> ReceivePurchaseOrderAsync(int id, ReceivePurchaseOrderDto dto, int tenantId);

        
        Task<IEnumerable<PurchaseOrderDto>> GetByStatusAsync(POStatus status, int tenantId);
        Task<IEnumerable<PurchaseOrderDto>> GetBySupplierAsync(int supplierId, int tenantId);
        Task<IEnumerable<PurchaseOrderDto>> GetPendingReceiptAsync(int tenantId);
    }
}
