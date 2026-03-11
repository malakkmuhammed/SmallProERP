using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.CustomerInteractionDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class CustomerInteractionService : ICustomerInteractionService
    {
        private readonly SmallProDbContext _context;

        // ── Temporary fixed tenant until JWT is wired in Phase 7 ─────────────
        private const int FixedTenantId = 1;

        public CustomerInteractionService(SmallProDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerInteractionDto>> GetAllAsync()
        {
            var interactions = await _context.CustomerInteractions
                .Where(i => i.TenantId == FixedTenantId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();

            return interactions.Select(MapToDto);
        }


        public async Task<CustomerInteractionDto?> GetByIdAsync(int id)
        {
            var interaction = await FindByIdAsync(id);
            return interaction is null ? null : MapToDto(interaction);
        }

  
        public async Task<IEnumerable<CustomerInteractionDto>> GetByCustomerIdAsync(int customerId)
        {
            var interactions = await _context.CustomerInteractions
                .Where(i => i.TenantId == FixedTenantId
                         && i.CustomerId == customerId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();

            return interactions.Select(MapToDto);
        }

    
        public async Task<CustomerInteractionDto> CreateAsync(CreateCustomerInteractionDto dto)
        {
           
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == FixedTenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            // If a UserId is provided, verify the user exists and belongs to this tenant
            if (dto.UserId.HasValue)
            {
                bool userExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.UserId.Value
                              && u.TenantId == FixedTenantId);

                if (!userExists)
                    throw new InvalidOperationException(
                        $"User with ID {dto.UserId.Value} was not found in this tenant.");
            }

            var interaction = new CustomerInteraction
            {
                CustomerId = dto.CustomerId,
                UserId = dto.UserId,
                InteractionDate = dto.InteractionDate,
                Type = (InteractionType)dto.Type,
                Description = dto.Description,
                TenantId = FixedTenantId
            };

            _context.CustomerInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            return MapToDto(interaction);
        }

       
        public async Task<bool> UpdateAsync(int id, UpdateCustomerInteractionDto dto)
        {
            var interaction = await FindByIdAsync(id);

            if (interaction is null)
                return false;

            
            if (dto.UserId.HasValue)
            {
                bool userExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.UserId.Value
                               && u.TenantId == FixedTenantId);

                if (!userExists)
                    throw new InvalidOperationException(
                        $"User with ID {dto.UserId.Value} was not found in this tenant.");
            }

            interaction.UserId = dto.UserId;
            interaction.InteractionDate = dto.InteractionDate;
            interaction.Type = (InteractionType)dto.Type;
            interaction.Description = dto.Description;

            await _context.SaveChangesAsync();
            return true;
        }

  
        public async Task<bool> DeleteAsync(int id)
        {
            var interaction = await FindByIdAsync(id);

            if (interaction is null)
                return false;

            _context.CustomerInteractions.Remove(interaction);
            await _context.SaveChangesAsync();
            return true;
        }

   
        private async Task<CustomerInteraction?> FindByIdAsync(int id)
        {
            return await _context.CustomerInteractions
                .FirstOrDefaultAsync(i => i.InteractionId == id
                                       && i.TenantId == FixedTenantId);
        }


        private static CustomerInteractionDto MapToDto(CustomerInteraction interaction)
        {
            return new CustomerInteractionDto
            {
                InteractionId = interaction.InteractionId,
                CustomerId = interaction.CustomerId,
                UserId = interaction.UserId,
                InteractionDate = interaction.InteractionDate,
                Type = interaction.Type,
                TypeName = interaction.Type.ToString(),   // e.g. "Call", "Email"
                Description = interaction.Description
            };
        }
    }
}
