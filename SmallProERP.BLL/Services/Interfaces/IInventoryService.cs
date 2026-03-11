using SmallProERP.Models.DTOs.InventoryDtos;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryMovementDto>> GetAllMovementsAsync(int tenantId);
        Task<IEnumerable<InventoryMovementDto>> GetMovementsByProductAsync(int productId, int tenantId);
        Task<IEnumerable<InventoryMovementDto>> GetMovementsByTypeAsync(MovementType type, int tenantId);
        Task<IEnumerable<InventoryMovementDto>> GetMovementsByDateRangeAsync(DateTime from, DateTime to, int tenantId);
    }
}
