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
        Task<IEnumerable<SupplierDto>> GetAllAsync(int tenantId);
        Task<SupplierDto?> GetByIdAsync(int id, int tenantId);
        Task<SupplierDetailsDto?> GetDetailsAsync(int id, int tenantId);
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int tenantId);
        Task<bool> UpdateAsync(int id, UpdateSupplierDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<IEnumerable<SupplierDto>> SearchAsync(string searchTerm, int tenantId);
    }
}
