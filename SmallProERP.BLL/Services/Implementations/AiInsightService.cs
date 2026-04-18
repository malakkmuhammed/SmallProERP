using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.AIDtos;
using SmallProERP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Implementations
{
    public class AiInsightService : IAiInsightService
    {
        private readonly SmallProDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AiInsightService> _logger;
        private readonly string _groqApiKey;
        private readonly string _groqModel;

        // Groq uses the OpenAI-compatible chat completions endpoint
        private const string GroqUrl =
            "https://api.groq.com/openai/v1/chat/completions";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        private static readonly string[] ValidInsightTypes =
            { "Revenue", "LowStock", "TopCustomers", "Full" };

        public AiInsightService(
            SmallProDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<AiInsightService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            // Read API key from appsettings.json — never hardcode
            _groqApiKey = configuration["Groq:ApiKey"]
                ?? throw new InvalidOperationException(
                    "Groq:ApiKey is not configured in appsettings.json.");

            _groqModel = configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";
        }


        public async Task<AiInsightDto> GenerateInsightAsync(
            GenerateInsightRequestDto request, int tenantId, int? userId)
        {
            _logger.LogInformation(
                "Generating {InsightType} insight for tenant {TenantId}",
                request.InsightType, tenantId);

            // 1 — Validate insight type
            if (!ValidInsightTypes.Contains(request.InsightType))
                throw new InvalidOperationException(
                    $"Invalid InsightType '{request.InsightType}'. " +
                    $"Valid values: {string.Join(", ", ValidInsightTypes)}");

            // 2 — Collect real metrics from DB
            var metrics = await CollectMetricsAsync(tenantId, request.FromDate, request.ToDate);
            _logger.LogInformation("Metrics collected — {TotalInvoices} invoices, {TotalProducts} products",
                metrics.TotalInvoices, metrics.TotalProducts);

            // 3 — Call Groq API for real AI insight
            var insightText = await CallGroqAsync(request.InsightType, metrics);
            _logger.LogInformation("Groq API returned insight text ({Length} chars)", insightText.Length);

            // 4 — Persist to DB
            var log = new AiInsightLog
            {
                TenantId = tenantId,
                InsightType = request.InsightType,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                MetricsJson = JsonSerializer.Serialize(metrics, JsonOptions),
                InsightText = insightText,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.AiInsightLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Insight saved with ID {InsightLogId}", log.InsightLogId);

            return MapToDto(log, metrics);
        }


        public async Task<IEnumerable<AiInsightSummaryDto>> GetAllAsync(int tenantId)
        {
            return await _context.AiInsightLogs
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new AiInsightSummaryDto
                {
                    InsightLogId = l.InsightLogId,
                    InsightType = l.InsightType,
                    InsightText = l.InsightText.Length > 200
                        ? l.InsightText.Substring(0, 200) + "..."
                        : l.InsightText,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }


        public async Task<AiInsightDto?> GetByIdAsync(int id, int tenantId)
        {
            var log = await _context.AiInsightLogs
                .FirstOrDefaultAsync(l => l.InsightLogId == id
                                       && l.TenantId == tenantId);

            if (log is null) return null;

            var metrics = TryDeserialize<BusinessMetricsDto>(log.MetricsJson);
            return MapToDto(log, metrics);
        }


        private async Task<string> CallGroqAsync(
            string insightType, BusinessMetricsDto metrics)
        {
            var metricsJson = JsonSerializer.Serialize(metrics, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Build a focused prompt based on the requested insight type
            var focusInstruction = insightType switch
            {
                "Revenue" => "Focus ONLY on revenue performance, collection rates, outstanding payments, and overdue invoices.",
                "LowStock" => "Focus ONLY on inventory health, low/out-of-stock products, and restocking recommendations.",
                "TopCustomers" => "Focus ONLY on customer performance, top revenue contributors, and retention opportunities.",
                "Full" => "Cover all areas: revenue, inventory health, and customer performance.",
                _ => "Provide a complete business analysis."
            };

            var systemMessage =
                "You are a professional ERP business analyst. " +
                "Analyze real business metrics and deliver sharp, actionable insights. " +
                "Use emoji section headers for readability. " +
                "Write like a consultant presenting to management. " +
                "Never use markdown code fences. Never mention JSON.";

            var userMessage =
                $"Today is {DateTime.UtcNow:MMMM dd, yyyy}.\n\n" +
                $"{focusInstruction}\n\n" +
                $"BUSINESS METRICS:\n{metricsJson}\n\n" +
                $"Write a professional business insight report (200-300 words).\n" +
                $"Structure:\n" +
                $"1. One-sentence executive summary\n" +
                $"2. Key findings with emoji headers and real numbers\n" +
                $"3. Exactly 3 specific, actionable recommendations\n" +
                $"Plain text only — no markdown code fences.";

            // Build the Groq request body (OpenAI-compatible format)
            var requestBody = new
            {
                model = _groqModel,
                max_tokens = 1024,
                temperature = 0.6,
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user",   content = userMessage   }
                }
            };

            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            var client = _httpClientFactory.CreateClient("Groq");

            _logger.LogInformation("Sending request to Groq API — model: {Model}", _groqModel);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, GroqUrl);
            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _groqApiKey);
            httpRequest.Content =
                new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Groq API returned {StatusCode}: {Error}",
                    (int)response.StatusCode, errorBody);
                throw new InvalidOperationException(
                    $"Groq API error {(int)response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Groq API responded successfully");

            // Parse OpenAI-compatible response: choices[0].message.content
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

            return content?.Trim() ?? string.Empty;
        }

        private async Task<BusinessMetricsDto> CollectMetricsAsync(
            int tenantId, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;

            // Sales filtered by tenant and optional date range
            var salesQuery = _context.Sales.Where(s => s.TenantId == tenantId);
            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.InvoiceDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.InvoiceDate <= toDate.Value);

            var sales = await salesQuery.Include(s => s.Customer).ToListAsync();
            var paid = sales.Where(s => s.IsPaid).ToList();
            var unpaid = sales.Where(s => !s.IsPaid).ToList();
            var overdue = unpaid
                .Where(s => s.DueDate.HasValue && s.DueDate.Value < now)
                .ToList();

            // Products for inventory metrics
            var products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .ToListAsync();

            // Top 5 customers by revenue
            var topCustomers = sales
                .GroupBy(s => s.CustomerId)
                .Select(g => new TopCustomerMetricDto
                {
                    CustomerName = g.First().Customer?.Name ?? "Unknown",
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    InvoiceCount = g.Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(5)
                .ToList();

            return new BusinessMetricsDto
            {
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                CollectedRevenue = paid.Sum(s => s.TotalAmount),
                OutstandingAmount = unpaid.Sum(s => s.TotalAmount),
                TotalInvoices = sales.Count,
                PaidInvoices = paid.Count,
                UnpaidInvoices = unpaid.Count,
                OverdueInvoices = overdue.Count,
                TotalProducts = products.Count,
                LowStockCount = products.Count(p => p.Quantity < p.MinimumStockLevel),
                OutOfStockCount = products.Count(p => p.Quantity == 0),
                TotalInventoryValue = products.Sum(p => p.Quantity * p.PurchasePrice),
                TopCustomers = topCustomers
            };
        }

        private static AiInsightDto MapToDto(AiInsightLog l, BusinessMetricsDto? metrics) => new()
        {
            InsightLogId = l.InsightLogId,
            InsightType = l.InsightType,
            FromDate = l.FromDate,
            ToDate = l.ToDate,
            InsightText = l.InsightText,
            Metrics = metrics,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        };

        private static T? TryDeserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch { return null; }
        }
    }
}
