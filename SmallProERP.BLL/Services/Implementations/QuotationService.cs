using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.QuotationDto;
using SmallProERP.Models.DTOs.QuotationItemDto;
using SmallProERP.Models.DTOs.QuotationItemDtos;
using SmallProERP.Models.DTOs.SaleDto;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class QuotationService : IQuotationService
    {
        private readonly SmallProDbContext _context;

        public QuotationService(SmallProDbContext context)
        {
            _context = context;
        }

        
        public async Task<IEnumerable<QuotationSummaryDto>> GetAllAsync(
            int tenantId,
            string? search = null)
        {
            // Start with base query scoped to tenant, join Customer
            var query = _context.Quotations
                .Where(q => q.TenantId == tenantId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                .AsQueryable();

            // Apply search filter if provided
            // Searches against Customer.Name and Customer.Company (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(q =>
                    q.Customer != null &&
                    (q.Customer.Name.ToLower().Contains(term) ||
                    (q.Customer.Company != null &&
                     q.Customer.Company.ToLower().Contains(term))));
            }

            var quotations = await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quotations.Select(MapToSummaryDto);
        }

        
        public async Task<QuotationDto?> GetByIdAsync(int id, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);
            return quotation is null ? null : MapToDto(quotation);
        }

     
        public async Task<QuotationDetailsDto?> GetDetailsByIdAsync(int id, int tenantId)
        {
            var quotation = await _context.Quotations
                .Where(q => q.QuotationId == id && q.TenantId == tenantId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync();

            return quotation is null ? null : MapToDetailsDto(quotation);
        }

       
        public async Task<IEnumerable<QuotationSummaryDto>> GetByCustomerIdAsync(
            int customerId, int tenantId)
        {
            var quotations = await _context.Quotations
                .Where(q => q.TenantId == tenantId
                         && q.CustomerId == customerId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return quotations.Select(MapToSummaryDto);
        }

      
        public async Task<QuotationStatisticsDto> GetStatisticsAsync(int tenantId)
        {
            var quotations = await _context.Quotations
                .Where(q => q.TenantId == tenantId)
                .ToListAsync();

            return new QuotationStatisticsDto
            {
                TotalQuotations = quotations.Count,
                DraftCount = quotations.Count(q => q.Status == QuotationStatus.Draft),
                SentCount = quotations.Count(q => q.Status == QuotationStatus.Sent),
                AcceptedCount = quotations.Count(q => q.Status == QuotationStatus.Accepted),
                RejectedCount = quotations.Count(q => q.Status == QuotationStatus.Rejected),
                TotalValue = quotations.Sum(q => q.TotalAmount),
                AcceptedValue = quotations
                                    .Where(q => q.Status == QuotationStatus.Accepted)
                                    .Sum(q => q.TotalAmount),
                PendingValue = quotations
                                    .Where(q => q.Status == QuotationStatus.Draft
                                             || q.Status == QuotationStatus.Sent)
                                    .Sum(q => q.TotalAmount)
            };
        }


        public async Task<QuotationDto> CreateAsync(
            CreateQuotationDto dto, int tenantId, int? userId)
        {
            bool customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId
                            && c.TenantId == tenantId);

            if (!customerExists)
                throw new InvalidOperationException(
                    $"Customer with ID {dto.CustomerId} was not found in this tenant.");

            bool numberExists = await _context.Quotations
                .AnyAsync(q => q.TenantId == tenantId
                            && q.QuotationNumber == dto.QuotationNumber);

            if (numberExists)
                throw new InvalidOperationException(
                    $"Quotation number '{dto.QuotationNumber}' already exists in this tenant.");

            var quotation = new Quotation
            {
                QuotationNumber = dto.QuotationNumber,
                CustomerId = dto.CustomerId,
                Subtotal = dto.Subtotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                Status = QuotationStatus.Draft,
                QuotationDate = dto.QuotationDate,
                ValidUntil = dto.ValidUntil,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();

            return MapToDto(quotation);
        }


        public async Task<bool> UpdateAsync(int id, UpdateQuotationDto dto, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            if (quotation.CustomerId != dto.CustomerId)
            {
                bool customerExists = await _context.Customers
                    .AnyAsync(c => c.CustomerId == dto.CustomerId
                                && c.TenantId == tenantId);

                if (!customerExists)
                    throw new InvalidOperationException(
                        $"Customer with ID {dto.CustomerId} was not found in this tenant.");
            }

            quotation.CustomerId = dto.CustomerId;
            quotation.Subtotal = dto.Subtotal;
            quotation.TaxAmount = dto.TaxAmount;
            quotation.TotalAmount = dto.TotalAmount;
            quotation.QuotationDate = dto.QuotationDate;
            quotation.ValidUntil = dto.ValidUntil;

            await _context.SaveChangesAsync();
            return true;
        }

       
        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeStatusAsync(int id, int status, int tenantId)
        {
            if (!Enum.IsDefined(typeof(QuotationStatus), status))
                throw new InvalidOperationException(
                    $"Invalid status value '{status}'. " +
                    "Valid values: 1=Draft, 2=Sent, 3=Accepted, 4=Rejected.");

            var quotation = await FindByIdAsync(id, tenantId);

            if (quotation is null)
                return false;

            quotation.Status = (QuotationStatus)status;
            await _context.SaveChangesAsync();
            return true;
        }

       
        public async Task<SaleDto> ConvertToSaleAsync(
            int quotationId, ConvertQuotationToSaleDto dto, int tenantId, int? userId)
        {
            var quotation = await FindByIdAsync(quotationId, tenantId);

            if (quotation is null)
                throw new InvalidOperationException(
                    $"Quotation with ID {quotationId} was not found in this tenant.");

            if (quotation.Status != QuotationStatus.Accepted)
                throw new InvalidOperationException(
                    $"Only Accepted quotations can be converted to a sale. " +
                    $"Current status: {quotation.Status}.");

            bool alreadyConverted = await _context.Sales
                .AnyAsync(s => s.QuotationId == quotationId
                            && s.TenantId == tenantId);

            if (alreadyConverted)
                throw new InvalidOperationException(
                    $"Quotation with ID {quotationId} has already been converted to a sale.");

            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                QuotationId = quotation.QuotationId,
                CustomerId = quotation.CustomerId,
                Subtotal = quotation.Subtotal,
                TaxAmount = quotation.TaxAmount,
                TotalAmount = quotation.TotalAmount,
                IsPaid = false,
                InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
                DueDate = dto.DueDate,
                PaymentMethod = dto.PaymentMethod,
                PaymentNotes = dto.PaymentNotes,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return MapSaleToDto(sale);
        }

       
        private async Task<Quotation?> FindByIdAsync(int id, int tenantId)
        {
            return await _context.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == id
                                       && q.TenantId == tenantId);
        }

        private static QuotationDto MapToDto(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuotationNumber = q.QuotationNumber,
            CustomerId = q.CustomerId,
            Subtotal = q.Subtotal,
            TaxAmount = q.TaxAmount,
            TotalAmount = q.TotalAmount,
            Status = q.Status,
            StatusName = q.Status.ToString(),
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            TenantId = q.TenantId,
            CreatedAt = q.CreatedAt,
            CreatedBy = q.CreatedBy
        };

        private static QuotationSummaryDto MapToSummaryDto(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuotationNumber = q.QuotationNumber,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.Name ?? string.Empty,
            CustomerCompany = q.Customer?.Company,
            Status = q.Status,
            StatusName = q.Status.ToString(),
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            TotalAmount = q.TotalAmount,
            ItemCount = q.QuotationItems?.Count ?? 0,
            CreatedAt = q.CreatedAt
        };

        private static QuotationDetailsDto MapToDetailsDto(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuotationNumber = q.QuotationNumber,
            Status = q.Status,
            StatusName = q.Status.ToString(),
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            CreatedAt = q.CreatedAt,
            CreatedBy = q.CreatedBy,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.Name ?? string.Empty,
            CustomerEmail = q.Customer?.Email,
            CustomerPhone = q.Customer?.Phone,
            CustomerCompany = q.Customer?.Company,
            Subtotal = q.Subtotal,
            TaxAmount = q.TaxAmount,
            TotalAmount = q.TotalAmount,
            Items = q.QuotationItems?.Select(qi => new QuotationItemDto
            {
                QuotationItemId = qi.QuotationItemId,
                QuotationId = qi.QuotationId,
                ProductId = qi.ProductId,
                ProductName = qi.Product?.Name ?? string.Empty,
                ProductCode = qi.Product?.ProductCode ?? string.Empty,
                Quantity = qi.Quantity,
                UnitPrice = qi.UnitPrice,
                LineTotal = qi.LineTotal
            }) ?? Enumerable.Empty<QuotationItemDto>(),
            ItemCount = q.QuotationItems?.Count ?? 0
        };

        private static SaleDto MapSaleToDto(Sale sale) => new()
        {
            SaleId = sale.SaleId,
            InvoiceNumber = sale.InvoiceNumber,
            QuotationId = sale.QuotationId,
            CustomerId = sale.CustomerId,
            Subtotal = sale.Subtotal,
            TaxAmount = sale.TaxAmount,
            TotalAmount = sale.TotalAmount,
            IsPaid = sale.IsPaid,
            PaidDate = sale.PaidDate,
            PaymentMethod = sale.PaymentMethod,
            PaymentNotes = sale.PaymentNotes,
            InvoiceDate = sale.InvoiceDate,
            DueDate = sale.DueDate,
            TenantId = sale.TenantId,
            CreatedAt = sale.CreatedAt,
            CreatedBy = sale.CreatedBy
        };
    }
}
