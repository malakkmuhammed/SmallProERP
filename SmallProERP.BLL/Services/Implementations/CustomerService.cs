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

        //// Temporary fixed identifiers until JWT 
        //private const int FixedTenantId = 1;
        //private const int FixedUserId = 1;

        public CustomerService(SmallProDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<CustomerDto>> GetAllAsync(int tenantid,
            CustomerStatus? status = null,
            string? search = null)
        {
            // Start with base query scoped to tenant
            var query = _context.Customers
                .Where(c => c.TenantId == tenantid);

            // Conditionally apply status filter
            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            // Conditionally apply search — case-insensitive contains on Name or Company
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Company != null && c.Company.ToLower().Contains(term)));
            }

            var customers = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return customers.Select(MapToDto);
        }

 
        public async Task<CustomerDto?> GetByIdAsync(int id,int tenantid)
        {
            var customer = await FindByIdAsync(id, tenantid);
            return customer is null ? null : MapToDto(customer);
        }


        public async Task<CustomerDto> CreateAsync(int tenantid, CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Company = dto.Company,
                Address = dto.Address,
                Status = CustomerStatus.NewLead,
                TenantId = tenantid,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<bool> UpdateAsync(int id,int tenantid, UpdateCustomerDto dto)
        {
            var customer = await FindByIdAsync(id, tenantid);

            if (customer is null)
                return false;

            customer.Name = dto.Name;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.Company = dto.Company;
            customer.Address = dto.Address;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = tenantid;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id,int tenantid)
        {
            var customer = await FindByIdAsync(id, tenantid);

            if (customer is null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeStatusAsync(int id,int tenantid, int status)
        {
            if (!Enum.IsDefined(typeof(CustomerStatus), status))
                throw new InvalidOperationException(
                    $"Invalid status value '{status}'. " +
                    "Valid values: 1=NewLead, 2=Interested, 3=Opportunity, 4=Won, 5=Lost.");

            var customer = await FindByIdAsync(id, tenantid);

            if (customer is null)
                return false;

            customer.Status = (CustomerStatus)status;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = tenantid;

            await _context.SaveChangesAsync();
            return true;
        }

     
        public async Task<CustomerStatisticsDto> GetStatisticsAsync(int tenantid)
        {
            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantid)
                .ToListAsync();

            return new CustomerStatisticsDto
            {
                TotalCustomers = customers.Count,
                NewLeadCount = customers.Count(c => c.Status == CustomerStatus.NewLead),
                InterestedCount = customers.Count(c => c.Status == CustomerStatus.Interested),
                OpportunityCount = customers.Count(c => c.Status == CustomerStatus.Opportunity),
                WonCount = customers.Count(c => c.Status == CustomerStatus.Won),
                LostCount = customers.Count(c => c.Status == CustomerStatus.Lost)
            };
        }

        private async Task<Customer?> FindByIdAsync(int id,int tenantid)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id
                                       && c.TenantId == tenantid);
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
                StatusName = customer.Status.ToString(),
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}
