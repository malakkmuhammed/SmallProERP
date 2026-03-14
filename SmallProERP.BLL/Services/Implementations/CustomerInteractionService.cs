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


        public CustomerInteractionService(SmallProDbContext context)
        {
            _context = context;
        }

       
        public async Task<IEnumerable<CustomerInteractionDto>> GetAllAsync(int tenantId)
        {
            var interactions = await _context.CustomerInteractions
                .Where(i => i.TenantId ==  tenantId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();

            return interactions.Select(MapToDto);
        }

       
        public async Task<CustomerInteractionDto?> GetByIdAsync(int id , int tenantId)
        {
            var interaction = await FindByIdAsync(id, tenantId);
            return interaction is null ? null : MapToDto(interaction);
        }

        public async Task<IEnumerable<CustomerInteractionDto>> GetByCustomerIdAsync(int tenantId ,
            int customerId,
            InteractionType? type = null)
        {
            var query = _context.CustomerInteractions
                .Where(i => i.TenantId == tenantId
                         && i.CustomerId == customerId);

            if (type.HasValue)
                query = query.Where(i => i.Type == type.Value);

            var interactions = await query
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync();

            return interactions.Select(MapToDto);
        }

       
        public async Task<CustomerInteractionSummaryDto?> GetSummaryByCustomerIdAsync(int customerId, int tenantId)
        {
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == customerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                return null;

            var interactions = await _context.CustomerInteractions
                .Where(i => i.TenantId == tenantId
                         && i.CustomerId == customerId)
                .ToListAsync();

            var summary = new CustomerInteractionSummaryDto
            {
                CustomerId = customerId,
                TotalInteractions = interactions.Count,
                CallCount = interactions.Count(i => i.Type == InteractionType.Call),
                EmailCount = interactions.Count(i => i.Type == InteractionType.Email),
                NoteCount = interactions.Count(i => i.Type == InteractionType.Note),
                WhatsAppCount = interactions.Count(i => i.Type == InteractionType.WhatsApp),
                MeetingCount = interactions.Count(i => i.Type == InteractionType.Meeting),
                LastInteractionDate = interactions.Count > 0
                    ? interactions.Max(i => i.InteractionDate)
                    : null
            };

            return summary;
        }

        public async Task<CustomerInteractionDto> CreateAsync(int tenantId, CreateCustomerInteractionDto dto )
        {
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            if (dto.UserId.HasValue)
            {
                bool userExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.UserId.Value
                               && u.TenantId == tenantId);

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
                TenantId = tenantId
            };

            _context.CustomerInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            return MapToDto(interaction);
        }


        public async Task<bool> UpdateAsync(int id,int tenantId, UpdateCustomerInteractionDto dto)
        {
            var interaction = await FindByIdAsync(id, tenantId);

            if (interaction is null)
                return false;

            if (dto.UserId.HasValue)
            {
                bool userExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.UserId.Value
                               && u.TenantId == tenantId);

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

 
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var interaction = await FindByIdAsync(id, tenantId);

            if (interaction is null)
                return false;

            _context.CustomerInteractions.Remove(interaction);
            await _context.SaveChangesAsync();
            return true;
        }


        private async Task<CustomerInteraction?> FindByIdAsync(int id,int tenantId)
        {
            return await _context.CustomerInteractions
                .FirstOrDefaultAsync(i => i.InteractionId == id
                                       && i.TenantId == tenantId);
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
                TypeName = interaction.Type.ToString(),
                Description = interaction.Description
            };
        }
    }
}
