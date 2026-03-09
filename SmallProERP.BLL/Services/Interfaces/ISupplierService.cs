using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmallProERP.Models.DTOs.SupplierDtos;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllAsync();

     
        Task<SupplierDto?> GetByIdAsync(int id);

        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);

   
        Task<bool> UpdateAsync(int id, UpdateSupplierDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
