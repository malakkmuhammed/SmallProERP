using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.SupplierDtos;
using SmallProERP.Models.Entities;

namespace SmallProERP.BLL.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly SmallProDbContext _context;

        // ── Temporary fixed tenant until JWT is wired in Phase 7 ─────────────
        private const int FixedTenantId = 1;

        public SupplierService(SmallProDbContext context)
        {
            _context = context;
        }

        // GET ALL
        
        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.TenantId == FixedTenantId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return suppliers.Select(MapToDto);
        }

        // GET BY ID
        public async Task<SupplierDto?> GetByIdAsync(int id)
        {
            var supplier = await FindByIdAsync(id);
            return supplier is null ? null : MapToDto(supplier);
        }

        // CREATE
        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                TenantId = FixedTenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return MapToDto(supplier);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await FindByIdAsync(id);

            if (supplier is null)
                return false;

            supplier.Name = dto.Name;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;

            await _context.SaveChangesAsync();
            return true;
        }

        // DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await FindByIdAsync(id);

            if (supplier is null)
                return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        
        private async Task<Supplier?> FindByIdAsync(int id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id
                                       && s.TenantId == FixedTenantId);
        }

        private static SupplierDto MapToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                CreatedAt = supplier.CreatedAt
            };
        }
    }
}
