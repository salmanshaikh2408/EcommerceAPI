using EcommerceAPI.Controllers;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
        public OrderDto CreateOrder(OrderDto request)
        {
            _context.Orders.Add(request);
            _context.SaveChanges();
            return request;
        }

        public bool DeleteOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return false;
            }
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return true;
        }

        public List<OrderDto> GetAllOrders()
        {
            return _context.Orders.ToList();
        }

        public OrderDto? GetOrderById(int id)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == id);
        }

        public bool UpdateOrder(OrderDto request)
        {
            _context.Orders.Update(request);
            _context.SaveChanges();
            return true;
        }
    }
}
