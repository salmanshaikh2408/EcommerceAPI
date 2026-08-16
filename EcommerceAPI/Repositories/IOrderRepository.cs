using EcommerceAPI.Controllers;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        OrderDto CreateOrder(OrderDto request);
        bool DeleteOrder(int id);
        bool UpdateOrder(OrderDto request);
        OrderDto? GetOrderById(int id);
        List<OrderDto> GetAllOrders();

    }
}
