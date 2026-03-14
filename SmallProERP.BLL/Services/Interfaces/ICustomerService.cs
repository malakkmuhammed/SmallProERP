using SmallProERP.Models.DTOs.CustomerDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
   
        Task<IEnumerable<CustomerDto>> GetAllAsync(int tenantid,
            CustomerStatus? status = null,
            string? search = null);


        Task<CustomerDto?> GetByIdAsync(int id,int tenantid);

        
        Task<CustomerDto> CreateAsync(int tenantid, CreateCustomerDto dto);

    
        Task<bool> UpdateAsync(int id, int tenantid, UpdateCustomerDto dto);


        Task<bool> DeleteAsync(int id, int tenantid);

  
        Task<bool> ChangeStatusAsync(int id,int  tenantid, int status);

       
        Task<CustomerStatisticsDto> GetStatisticsAsync(int tenantid);
    }
}
