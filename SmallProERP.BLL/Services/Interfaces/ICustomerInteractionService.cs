using SmallProERP.Models.DTOs.CustomerInteractionDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ICustomerInteractionService
    {
        Task<IEnumerable<CustomerInteractionDto>> GetAllAsync();

      
        Task<CustomerInteractionDto?> GetByIdAsync(int id);

    
        Task<IEnumerable<CustomerInteractionDto>> GetByCustomerIdAsync(int customerId);

  
        Task<CustomerInteractionDto> CreateAsync(CreateCustomerInteractionDto dto);


        Task<bool> UpdateAsync(int id, UpdateCustomerInteractionDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
