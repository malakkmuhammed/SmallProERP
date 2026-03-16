using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.PurchaseOrderDtos
{
    public class PurchaseOrderDocumentDto
    {
        public string PONumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierEmail { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierAddress { get; set; }

        
        public string CompanyName { get; set; } = string.Empty;

       
        public List<PODocumentItemDto> Items { get; set; } = new();

        
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }

        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }
    }

    
    public class PODocumentItemDto
    {
        public int ItemNumber { get; set; }  
        public string ProductCode { get; set; } = string.Empty;  
        public string ProductName { get; set; } = string.Empty;  
        public string? ProductDescription { get; set; }          
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
        
    }
}
