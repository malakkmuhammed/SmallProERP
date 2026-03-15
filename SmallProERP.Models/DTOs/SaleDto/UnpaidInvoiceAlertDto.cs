using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.SaleDto
{
    public class UnpaidInvoiceAlertDto
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysSinceInvoice { get; set; }    // how many days since invoice was created
        public bool IsOverdue { get; set; }          // true if past DueDate
        public int? DaysOverdue { get; set; }        // how many days past DueDate (null if not overdue)
    }
}
