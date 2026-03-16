
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallProERP.API.Helpers;
using SmallProERP.BLL.Services.Interfaces;
using SmallProERP.Models.DTOs.PurchaseOrderDtos;
using SmallProERP.Models.Enums;

namespace SmallProERP.API.Controllers
{
    [ApiController]
    [Route("api/purchase-orders")]
    [Authorize]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        private int GetTenantId()
        {
            // ⭐ TEST MODE: Return hardcoded value
            if (TestHelper.IsTestMode)
            {
                return TestHelper.TestTenantId;  // Returns 1
            }

            // ⭐ PRODUCTION MODE: Extract from token
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim == null)
                throw new UnauthorizedAccessException("TenantId not found in token");

            return int.Parse(tenantIdClaim.Value);
        }

        // GET /api/purchase-orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAll()
        {
            var tenantId = GetTenantId();
            var purchaseOrders = await _purchaseOrderService.GetAllAsync(tenantId);
            return Ok(purchaseOrders);
        }

        // GET /api/purchase-orders/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetPending()
        {
            var tenantId = GetTenantId();
            var purchaseOrders = await _purchaseOrderService.GetPendingReceiptAsync(tenantId);
            return Ok(purchaseOrders);
        }

        // GET /api/purchase-orders/status/{status}
        // Status: 1=Draft, 2=Sent, 3=Received
        [HttpGet("status/{status:int}")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetByStatus(int status)
        {
            if (!Enum.IsDefined(typeof(POStatus), status))
                return BadRequest(new { message = "Invalid status value. Valid values: 1=Draft, 2=Sent, 3=Received" });

            var tenantId = GetTenantId();
            var purchaseOrders = await _purchaseOrderService.GetByStatusAsync((POStatus)status, tenantId);
            return Ok(purchaseOrders);
        }

        // GET /api/purchase-orders/supplier/{supplierId}
        [HttpGet("supplier/{supplierId:int}")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetBySupplier(int supplierId)
        {
            var tenantId = GetTenantId();
            var purchaseOrders = await _purchaseOrderService.GetBySupplierAsync(supplierId, tenantId);
            return Ok(purchaseOrders);
        }

        // GET /api/purchase-orders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PurchaseOrderDto>> GetById(int id)
        {
            var tenantId = GetTenantId();
            var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id, tenantId);

            if (purchaseOrder == null)
                return NotFound(new { message = $"Purchase order with ID {id} not found" });

            return Ok(purchaseOrder);
        }

        // POST /api/purchase-orders
        [HttpPost]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PurchaseOrderDto>> Create([FromBody] CreatePurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var created = await _purchaseOrderService.CreateAsync(dto, tenantId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.PurchaseOrderId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/purchase-orders/{id}
        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var success = await _purchaseOrderService.UpdateAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/purchase-orders/{id}
        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _purchaseOrderService.DeleteAsync(id, tenantId);

                if (!success)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/purchase-orders/{id}/send
        [HttpPost("{id:int}/send")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Send(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _purchaseOrderService.SendPurchaseOrderAsync(id, tenantId);

                if (!success)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                return Ok(new { message = "Purchase order sent successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/purchase-orders/{id}/receive
        [HttpPost("{id:int}/receive")]
        //[Authorize(Roles = "Admin,Manager,InventoryManager")]
        public async Task<IActionResult> Receive(int id, [FromBody] ReceivePurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantId = GetTenantId();
                var success = await _purchaseOrderService.ReceivePurchaseOrderAsync(id, dto, tenantId);

                if (!success)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                return Ok(new { message = "Purchase order received successfully. All product quantities have been updated." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // GET /api/purchase-orders/{id}/document
        [HttpGet("{id:int}/document")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PurchaseOrderDocumentDto>> GetDocument(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var document = await _purchaseOrderService.GetDocumentAsync(id, tenantId);

                if (document == null)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                return Ok(document);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
