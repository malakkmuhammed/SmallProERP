using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class AiInsightLog
    {
        public int InsightLogId { get; set; }

        public int TenantId { get; set; }
        public string InsightType { get; set; } = null!;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? MetricsJson { get; set; }   // JSON
        public string InsightText { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public User? CreatedByUser { get; set; }
    }
}
