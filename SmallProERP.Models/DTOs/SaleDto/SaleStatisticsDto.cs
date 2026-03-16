using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    public class SaleStatisticsDto
    {
        public int TotalInvoices { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int OverdueCount { get; set; }
        public decimal OverdueAmount { get; set; }
    }
}
