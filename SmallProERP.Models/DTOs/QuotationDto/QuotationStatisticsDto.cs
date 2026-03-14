using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.QuotationDto
{
    public class QuotationStatisticsDto
    {
        public int TotalQuotations { get; set; }
        public int DraftCount { get; set; }
        public int SentCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }

        // Financial totals
        public decimal TotalValue { get; set; }          // sum of all TotalAmount
        public decimal AcceptedValue { get; set; }       // sum of Accepted quotations
        public decimal PendingValue { get; set; }        // sum of Draft + Sent quotations
    }
}
