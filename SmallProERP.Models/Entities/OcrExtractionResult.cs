using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class OcrExtractionResult
    {
        public int OcrResultId { get; set; }

        public int TenantId { get; set; }
        public string ImagePath { get; set; } = null!;
        public string? RawText { get; set; }
        public string? ExtractedData { get; set; }   // JSON
        public string Status { get; set; } = "Success";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public User? CreatedByUser { get; set; }

    }
}
