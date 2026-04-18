using Microsoft.AspNetCore.Http;
using SmallProERP.Models.DTOs.AIDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.BLL.Services.Interfaces
{


    public interface IOcrService
    {
        Task<OcrResultDto> ProcessOcrAsync(IFormFile image, int tenantId, int? userId);

        Task<IEnumerable<OcrSummaryDto>> GetAllAsync(int tenantId);

        Task<OcrResultDto?> GetByIdAsync(int id, int tenantId);
    }



    public interface IAiInsightService
    {
        Task<AiInsightDto> GenerateInsightAsync(
            GenerateInsightRequestDto request, int tenantId, int? userId);

        Task<IEnumerable<AiInsightSummaryDto>> GetAllAsync(int tenantId);

        Task<AiInsightDto?> GetByIdAsync(int id, int tenantId);
    }
}
