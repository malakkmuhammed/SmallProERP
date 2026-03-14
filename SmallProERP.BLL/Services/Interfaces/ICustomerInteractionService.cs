using SmallProERP.Models.DTOs.CustomerInteractionDtos;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ICustomerInteractionService
    {
        Task<IEnumerable<CustomerInteractionDto>> GetAllAsync(int tenantId);

        Task<CustomerInteractionDto?> GetByIdAsync(int id, int tenantId);

        Task<IEnumerable<CustomerInteractionDto>> GetByCustomerIdAsync(int tenantId,
            int customerId,
            InteractionType? type = null);

        Task<CustomerInteractionSummaryDto?> GetSummaryByCustomerIdAsync(int customerId, int tenantId);

        Task<CustomerInteractionDto> CreateAsync(int tenantId, CreateCustomerInteractionDto dto);


        Task<bool> UpdateAsync(int id, int tenantId, UpdateCustomerInteractionDto dto);


        Task<bool> DeleteAsync(int id, int tenantId);
    }
}
