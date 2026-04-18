using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.AIDtos;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class OcrService : IOcrService
    {
        private readonly SmallProDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OcrService> _logger;

        // Status constants — matches OcrExtractionResult.Status (plain string)
        private const string StatusCompleted = "Completed";
        private const string StatusFailed = "Failed";

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public OcrService(
            SmallProDbContext context,
            IWebHostEnvironment env,
            ILogger<OcrService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }


        public async Task<OcrResultDto> ProcessOcrAsync(
            IFormFile image, int tenantId, int? userId)
        {
            _logger.LogInformation("OCR processing started for tenant {TenantId}", tenantId);

            // 1 — Validate image
            ValidateImage(image);

            // 2 — Save image to wwwroot/uploads
            var (imagePath, _) = await SaveImageAsync(image);

            // 3 — Simulate OCR extraction
            OcrExtractedDataDto? extractedData = null;
            string rawText = string.Empty;
            string status = StatusCompleted;

            try
            {
                (rawText, extractedData) = await RunMockOcrAsync(image.FileName);
            }
            catch (Exception ex)
            {

                status = StatusFailed;


            }

            // 4 — Persist to DB
            var record = new OcrExtractionResult
            {
                TenantId = tenantId,
                ImagePath = imagePath,
                RawText = rawText,
                ExtractedData = extractedData is null
                    ? null
                    : JsonSerializer.Serialize(extractedData, JsonOptions),
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.OcrExtractionResults.Add(record);
            await _context.SaveChangesAsync();

            _logger.LogInformation("OCR result saved with ID {OcrResultId}", record.OcrResultId);

            return MapToDto(record, extractedData);
        }


        public async Task<IEnumerable<OcrSummaryDto>> GetAllAsync(int tenantId)
        {
            return await _context.OcrExtractionResults
                .Where(o => o.TenantId == tenantId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OcrSummaryDto
                {
                    OcrResultId = o.OcrResultId,
                    ImagePath = o.ImagePath,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }


        public async Task<OcrResultDto?> GetByIdAsync(int id, int tenantId)
        {
            var record = await _context.OcrExtractionResults
                .FirstOrDefaultAsync(o => o.OcrResultId == id
                                       && o.TenantId == tenantId);

            if (record is null) return null;

            var extracted = TryDeserialize<OcrExtractedDataDto>(record.ExtractedData);
            return MapToDto(record, extracted);
        }

        private Task<(string rawText, OcrExtractedDataDto extracted)> RunMockOcrAsync(
            string fileName)
        {
            // Simulate a realistic extracted invoice
            var rawText = $"INVOICE\nInvoice No: INV-MOCK-001\nDate: {DateTime.UtcNow:yyyy-MM-dd}\n" +
                          $"Supplier: Mock Supplier Co.\nPhone: +971501234567\n" +
                          $"Item 1: Laptop Pro x2 @ 2000.00 = 4000.00\n" +
                          $"Item 2: Mouse x5 @ 50.00 = 250.00\n" +
                          $"Subtotal: 4250.00\nTax (5%): 212.50\nTotal: 4462.50";

            var extracted = new OcrExtractedDataDto
            {
                InvoiceNumber = "INV-MOCK-001",
                SupplierName = "Mock Supplier Co.",
                SupplierPhone = "+971501234567",
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30),
                Subtotal = 4250.00m,
                TaxAmount = 212.50m,
                TotalAmount = 4462.50m,
                LineItems = new List<OcrLineItemDto>
                {
                    new() { Description = "Laptop Pro", Quantity = 2,
                            UnitPrice = 2000.00m, LineTotal = 4000.00m },
                    new() { Description = "Mouse",      Quantity = 5,
                            UnitPrice = 50.00m,   LineTotal = 250.00m }
                },
                Notes = $"Extracted from file: {fileName}"
            };

            return Task.FromResult((rawText, extracted));
        }


        private static void ValidateImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new InvalidOperationException("No image file provided.");

            var ext = Path.GetExtension(image.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"Invalid file type '{ext}'. Allowed: jpg, jpeg, png, webp, bmp.");

            if (image.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("Image size must not exceed 10 MB.");
        }

        private async Task<(string imagePath, string filePath)> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(image.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var imagePath = $"/uploads/{fileName}";

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return (imagePath, filePath);
        }

        private static OcrResultDto MapToDto(
            OcrExtractionResult r, OcrExtractedDataDto? extracted) => new()
            {
                OcrResultId = r.OcrResultId,
                ImagePath = r.ImagePath,
                RawText = r.RawText,
                ExtractedData = extracted,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy
            };

        private static T? TryDeserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch { return null; }
        }
    }
}