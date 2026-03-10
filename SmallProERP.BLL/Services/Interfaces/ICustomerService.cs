using SmallProERP.Models.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();

      
        Task<CustomerDto?> GetByIdAsync(int id);

 
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);

    
        Task<bool> UpdateAsync(int id, UpdateCustomerDto dto);

      
        Task<bool> DeleteAsync(int id);

      
        Task<bool> ChangeStatusAsync(int id, int status);
    }
}
