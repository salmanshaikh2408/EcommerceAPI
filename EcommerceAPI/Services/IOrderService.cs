using EcommerceAPI.Controllers;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IOrderService
    {
        List<OrderDto> GetOrders();
        OrderDto CreateOrder(OrderRequest request);
        OrderDto? GetOrderById(int Id);
        bool DeleteOrder(int Id);
        bool UpdateOrder(int id, OrderRequest request);
        bool UpdateOrderStatus(int id, string newStatus);
        List<OrderDto> GetOrdersByStatus(string status);
    }
}
