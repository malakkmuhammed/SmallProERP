

using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.InventoryDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.BLL.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly SmallProDbContext _context;

        public InventoryService(SmallProDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetAllMovementsAsync(int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Include(im => im.Product)
                .Where(im => im.TenantId == tenantId)
                .OrderByDescending(im => im.MovementDate)
                .ToListAsync();

            return movements.Select(MapToDto);
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetMovementsByProductAsync(int productId, int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Include(im => im.Product)
                .Where(im => im.TenantId == tenantId && im.ProductId == productId)
                .OrderByDescending(im => im.MovementDate)
                .ToListAsync();

            return movements.Select(MapToDto);
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetMovementsByTypeAsync(MovementType type, int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Include(im => im.Product)
                .Where(im => im.TenantId == tenantId && im.MovementType == type)
                .OrderByDescending(im => im.MovementDate)
                .ToListAsync();

            return movements.Select(MapToDto);
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetMovementsByDateRangeAsync(DateTime from, DateTime to, int tenantId)
        {
            var movements = await _context.InventoryMovements
                .Include(im => im.Product)
                .Where(im => im.TenantId == tenantId
                          && im.MovementDate >= from
                          && im.MovementDate <= to)
                .OrderByDescending(im => im.MovementDate)
                .ToListAsync();

            return movements.Select(MapToDto);
        }

        private static InventoryMovementDto MapToDto(InventoryMovement movement)
        {
            return new InventoryMovementDto
            {
                MovementId = movement.MovementId,
                ProductId = movement.ProductId,
                ProductCode = movement.Product?.ProductCode ?? "",
                ProductName = movement.Product?.Name ?? "",
                MovementType = movement.MovementType,
                MovementTypeText = movement.MovementType.ToString(),
                Quantity = movement.Quantity,
                ReferenceNumber = movement.ReferenceNumber,
                MovementDate = movement.MovementDate,
                Notes = movement.Notes
            };
        }
    }
}