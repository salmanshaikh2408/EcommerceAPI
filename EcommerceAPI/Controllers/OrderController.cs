using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger) 
        {
            _orderService = orderService;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetOrders()
        {
            _logger.LogInformation("Getorder endpoint called at {Time}", DateTime.UtcNow);
            var orders = _orderService.GetOrders();
            return Ok(orders);
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            _logger.LogInformation("CreateOrder called for product: {Product}", request.ProductName);
            //try
            //{

                var result = _orderService.CreateOrder(request);
                _logger.LogInformation("Order created with ID: {OrderId}", result.Id);
                return CreatedAtAction(nameof(GetOrders), new { id = result.Id }, result);
            //}
            //catch(Exception ex)
            //{
            //    _logger.LogError(ex, "Error while creating order for product: {Product}", request.ProductName);
            //    return BadRequest("Something went wrong. Please try again.");
            //}
        }

        [HttpGet("{id}")]
        public IActionResult GetOrderById(int id)
        {
            var order = _orderService.GetOrderById(id);
            if(order == null)
            {
                return NotFound($"Order with ID = {id} is not found.");
            }
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            _logger.LogWarning("DeleteOrder called for ID: {Id}", id);
            var deleted = _orderService.DeleteOrder(id);
            if (!deleted)
            {
                return NotFound($"Order with ID = {id} is not found.");
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, [FromBody] OrderRequest request)
        {
            _logger.LogInformation("UpdateOrder called for ID: {Id}", id);

            var updated = _orderService.UpdateOrder(id, request);
            if (!updated)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            return Ok($"Order {id} updated successfully.");
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            _logger.LogInformation("UpdateOrderStatus called for ID: {Id}", id);

            var updated = _orderService.UpdateOrderStatus(id, newStatus);
            if (!updated)
            {
                return NotFound($"Order with ID {id} not found or invalid status.");
            }

            return Ok($"Order {id} status updated successfully.");
        }

        [HttpGet("status/{status}")]
        public IActionResult GetOrdersByStatus(string status)
        {
            _logger.LogInformation("GetOrdersByStatus called for status: {Status}", status);
            var orders = _orderService.GetOrdersByStatus(status);
            return Ok(orders);
        }
    }

    public class OrderRequest
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}