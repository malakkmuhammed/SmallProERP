
using Microsoft.EntityFrameworkCore;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.DAL.Data;
using SmallProERP.Models.DTOs.PurchaseOrderDtos;
using SmallProERP.Models.Entities;
using SmallProERP.Models.Enums;

namespace SmallProERP.BLL.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly SmallProDbContext _context;

        public PurchaseOrderService(SmallProDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD

        public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync(int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Where(po => po.TenantId == tenantId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();

            return purchaseOrders.Select(MapToDto);
        }

        public async Task<PurchaseOrderDto?> GetByIdAsync(int id, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            return purchaseOrder == null ? null : MapToDto(purchaseOrder);
        }

        public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, int tenantId)
        {
            // Validate: Supplier exists
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.SupplierId == dto.SupplierId && s.TenantId == tenantId);

            if (!supplierExists)
            {
                throw new InvalidOperationException($"Supplier with ID {dto.SupplierId} not found.");
            }

            // Validate: All products exist
            foreach (var itemDto in dto.Items)
            {
                var productExists = await _context.Products
                    .AnyAsync(p => p.ProductId == itemDto.ProductId && p.TenantId == tenantId);

                if (!productExists)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                }
            }

           
            var poNumber = await GeneratePONumberAsync(tenantId);

            
            var purchaseOrder = new PurchaseOrder
            {
                PONumber = poNumber,
                SupplierId = dto.SupplierId,
                Status = POStatus.Draft,
                OrderDate = DateTime.UtcNow,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                Notes = dto.Notes,
                PurchaseOrderItems = new List<PurchaseOrderItem>()
            };

            decimal totalAmount = 0;


            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.Quantity * itemDto.UnitPrice;
                totalAmount += lineTotal;

                var poItem = new PurchaseOrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    LineTotal = lineTotal,
                    TenantId = tenantId
                };

                purchaseOrder.PurchaseOrderItems.Add(poItem);
            }

            purchaseOrder.TotalAmount = totalAmount;

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(purchaseOrder).Reference(po => po.Supplier).LoadAsync();
            await _context.Entry(purchaseOrder).Collection(po => po.PurchaseOrderItems!).LoadAsync();

            foreach (var item in purchaseOrder.PurchaseOrderItems!)
            {
                await _context.Entry(item).Reference(poi => poi.Product).LoadAsync();
            }

            return MapToDto(purchaseOrder);
        }

        public async Task<bool> UpdateAsync(int id, UpdatePurchaseOrderDto dto, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderItems)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            if (purchaseOrder == null)
                return false;

            // Validate: Can only update Draft POs
            if (purchaseOrder.Status != POStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Cannot update purchase order with status '{purchaseOrder.Status}'. Only Draft purchase orders can be updated.");
            }

            // Validate: Supplier exists
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.SupplierId == dto.SupplierId && s.TenantId == tenantId);

            if (!supplierExists)
            {
                throw new InvalidOperationException($"Supplier with ID {dto.SupplierId} not found.");
            }

            // Validate: All products exist
            foreach (var itemDto in dto.Items)
            {
                var productExists = await _context.Products
                    .AnyAsync(p => p.ProductId == itemDto.ProductId && p.TenantId == tenantId);

                if (!productExists)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                }
            }

            // Update supplier
            purchaseOrder.SupplierId = dto.SupplierId;

            // Remove old items
            _context.PurchaseOrderItems.RemoveRange(purchaseOrder.PurchaseOrderItems!);

            // Add new items
            decimal totalAmount = 0;
            var newItems = new List<PurchaseOrderItem>();

            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.Quantity * itemDto.UnitPrice;
                totalAmount += lineTotal;

                var poItem = new PurchaseOrderItem
                {
                    PurchaseOrderId = purchaseOrder.PurchaseOrderId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    LineTotal = lineTotal,
                    TenantId = tenantId
                };

                newItems.Add(poItem);
            }

            _context.PurchaseOrderItems.AddRange(newItems);
            purchaseOrder.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderItems)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            if (purchaseOrder == null)
                return false;

            // Validate: Can only delete Draft POs
            if (purchaseOrder.Status != POStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Cannot delete purchase order with status '{purchaseOrder.Status}'. Only Draft purchase orders can be deleted.");
            }

            _context.PurchaseOrders.Remove(purchaseOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Status Management

        public async Task<bool> SendPurchaseOrderAsync(int id, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            if (purchaseOrder == null)
                return false;

            // Validate: Can only send Draft POs
            if (purchaseOrder.Status != POStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Cannot send purchase order with status '{purchaseOrder.Status}'. Only Draft purchase orders can be sent.");
            }

            purchaseOrder.Status = POStatus.Sent;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReceivePurchaseOrderAsync(int id, ReceivePurchaseOrderDto dto, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            if (purchaseOrder == null)
                return false;

            // Validate: Can only receive Sent POs
            if (purchaseOrder.Status != POStatus.Sent)
            {
                throw new InvalidOperationException(
                    $"Cannot receive purchase order with status '{purchaseOrder.Status}'. Only Sent purchase orders can be received.");
            }

            // Process ALL items (full delivery - no partial receiving)
            foreach (var poItem in purchaseOrder.PurchaseOrderItems!)
            {
                var product = poItem.Product;
                if (product != null)
                {
                    // Update product quantity (add FULL ordered quantity)
                    product.Quantity += poItem.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    // Create inventory movement record
                    var movement = new InventoryMovement
                    {
                        ProductId = product.ProductId,
                        MovementType = MovementType.Purchase,
                        Quantity = poItem.Quantity,
                        ReferenceNumber = purchaseOrder.PONumber,
                        MovementDate = DateTime.UtcNow,
                        Notes = $"Received from PO {purchaseOrder.PONumber} - Supplier: {purchaseOrder.Supplier?.Name}" +
                                (string.IsNullOrEmpty(dto.Notes) ? "" : $" - {dto.Notes}"),
                        TenantId = tenantId
                    };

                    _context.InventoryMovements.Add(movement);
                }
            }

            // Update PO status
            purchaseOrder.Status = POStatus.Received;
            purchaseOrder.ReceivedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Filtering

        public async Task<IEnumerable<PurchaseOrderDto>> GetByStatusAsync(POStatus status, int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Where(po => po.TenantId == tenantId && po.Status == status)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();

            return purchaseOrders.Select(MapToDto);
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetBySupplierAsync(int supplierId, int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Where(po => po.TenantId == tenantId && po.SupplierId == supplierId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();

            return purchaseOrders.Select(MapToDto);
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetPendingReceiptAsync(int tenantId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Where(po => po.TenantId == tenantId && po.Status == POStatus.Sent)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return purchaseOrders.Select(MapToDto);
        }

        #endregion
        public async Task<PurchaseOrderDocumentDto?> GetDocumentAsync(int id, int tenantId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems!)
                    .ThenInclude(poi => poi.Product)
                .Include(po => po.Tenant)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id && po.TenantId == tenantId);

            if (purchaseOrder == null)
                return null;

            if (purchaseOrder.Status != POStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Cannot generate document for purchase order with status '{purchaseOrder.Status}'. Only Draft purchase orders can be converted to documents.");
            }

            var document = new PurchaseOrderDocumentDto
            {
                PONumber = purchaseOrder.PONumber,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.OrderDate.AddDays(14), 

                SupplierName = purchaseOrder.Supplier?.Name ?? "",
                SupplierEmail = purchaseOrder.Supplier?.Email,
                SupplierPhone = purchaseOrder.Supplier?.Phone,
                SupplierAddress = purchaseOrder.Supplier?.Address,

                
                CompanyName = purchaseOrder.Tenant?.CompanyName ?? "",

                Items = purchaseOrder.PurchaseOrderItems?
                    .Select((poi, index) => new PODocumentItemDto
                    {
                        ItemNumber = index + 1, 
                        ProductCode = poi.Product?.ProductCode ?? "",
                        ProductName = poi.Product?.Name ?? "",
                        ProductDescription = poi.Product?.Description,
                        Quantity = poi.Quantity,
                        UnitPrice = poi.UnitPrice,
                        LineTotal = poi.LineTotal
                    }).ToList() ?? new List<PODocumentItemDto>(),

               
                TotalAmount = purchaseOrder.TotalAmount,
                TotalItems = purchaseOrder.PurchaseOrderItems?.Count ?? 0,
                TotalQuantity = purchaseOrder.PurchaseOrderItems?.Sum(poi => poi.Quantity) ?? 0,
                Notes= purchaseOrder.Notes


            };

            return document;
        }

        #region Private Helpers

        private async Task<string> GeneratePONumberAsync(int tenantId)
        {
            var lastPO = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .OrderByDescending(po => po.PurchaseOrderId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPO != null)
            {
                var lastNumber = lastPO.PONumber.Split('-').LastOrDefault();
                if (int.TryParse(lastNumber, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            return $"PO-{nextNumber:D4}";
        }

        private static PurchaseOrderDto MapToDto(PurchaseOrder purchaseOrder)
        {
            return new PurchaseOrderDto
            {
                PurchaseOrderId = purchaseOrder.PurchaseOrderId,
                PONumber = purchaseOrder.PONumber,
                SupplierId = purchaseOrder.SupplierId,
                SupplierName = purchaseOrder.Supplier?.Name ?? "",
                TotalAmount = purchaseOrder.TotalAmount,
                Status = purchaseOrder.Status,
                StatusText = purchaseOrder.Status.ToString(),
                OrderDate = purchaseOrder.OrderDate,
                ReceivedDate = purchaseOrder.ReceivedDate,
                CreatedAt = purchaseOrder.CreatedAt,
                Notes = purchaseOrder.Notes,
                Items = purchaseOrder.PurchaseOrderItems?.Select(poi => new PurchaseOrderItemDto
                {
                    PurchaseOrderItemId = poi.PurchaseOrderItemId,
                    ProductId = poi.ProductId,
                    ProductCode = poi.Product?.ProductCode ?? "",
                    ProductName = poi.Product?.Name ?? "",
                    ProductDescription = poi.Product?.Description,
                    Quantity = poi.Quantity,
                    UnitPrice = poi.UnitPrice,
                    LineTotal = poi.LineTotal
                }).ToList() ?? new List<PurchaseOrderItemDto>(),
                TotalItems = purchaseOrder.PurchaseOrderItems?.Count ?? 0,
                TotalQuantity = purchaseOrder.PurchaseOrderItems?.Sum(poi => poi.Quantity) ?? 0
            };
        }

        #endregion
        
    }
}