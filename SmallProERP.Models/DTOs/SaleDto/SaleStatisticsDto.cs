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

        public decimal TotalRevenue { get; set; }        // sum of all TotalAmount
        public decimal CollectedRevenue { get; set; }    // sum of paid invoices
        public decimal OutstandingAmount { get; set; }   // sum of unpaid invoices

        public int OverdueCount { get; set; }            // unpaid AND past DueDate
        public decimal OverdueAmount { get; set; }       // sum of overdue invoices
    }
}
