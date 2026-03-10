using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.CustomerDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly SmallProDbContext _context;

        // Temporary  until JWT
        private const int FixedTenantId = 1;
        private const int FixedUserId = 1;   // Populates UpdatedBy audit field

        public CustomerService(SmallProDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.TenantId == FixedTenantId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return customers.Select(MapToDto);
        }

  
        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await FindByIdAsync(id);
            return customer is null ? null : MapToDto(customer);
        }


        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Company = dto.Company,
                Address = dto.Address,
                Status = CustomerStatus.NewLead,  // Every new customer starts as a lead
                TenantId = FixedTenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

     
        public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await FindByIdAsync(id);

            if (customer is null)
                return false;

            customer.Name = dto.Name;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.Company = dto.Company;
            customer.Address = dto.Address;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = FixedUserId;

            await _context.SaveChangesAsync();
            return true;
        }

     
        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await FindByIdAsync(id);

            if (customer is null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

     
        public async Task<bool> ChangeStatusAsync(int id, int status)
        {
            if (!Enum.IsDefined(typeof(CustomerStatus), status))
                throw new InvalidOperationException(
                    $"Invalid status value '{status}'. " +
                    "Valid values: 1=NewLead, 2=Interested, 3=Opportunity, 4=Won, 5=Lost.");

            var customer = await FindByIdAsync(id);

            if (customer is null)
                return false;

            customer.Status = (CustomerStatus)status;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = FixedUserId;

            await _context.SaveChangesAsync();
            return true;
        }

   
        private async Task<Customer?> FindByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id
                                       && c.TenantId == FixedTenantId);
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Company = customer.Company,
                Address = customer.Address,
                Status = customer.Status,
                StatusName = customer.Status.ToString(),   // e.g. "NewLead", "Won"
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}
