using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesOrderApp.Repositories;

namespace SalesOrderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {
        private readonly SalesOrderRepository _salesOrderRepository;

        public SalesOrderController(SalesOrderRepository salesOrderRepository)
        {
            _salesOrderRepository = salesOrderRepository;
        }

        // GET: api/Orders/{orderNumber}
        [HttpGet("{orderNumber}")]
        public async Task<IActionResult> GetOrderDetails(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return BadRequest("Order number is required.");
            }

            var salesOrders = await _salesOrderRepository.GetAllAsync();
            var filteredOrders = salesOrders.Where(so => (bool)so.OrderHeader?.OrderNumber?.Trim().ToLower().Contains(orderNumber.Trim().ToLower()));
            
            if (filteredOrders == null)
            {
                return NotFound("No orders found for order number");
            }

            return Ok(filteredOrders);
        }
    }
}
