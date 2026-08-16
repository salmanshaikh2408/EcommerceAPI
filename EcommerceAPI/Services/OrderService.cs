using EcommerceAPI.Controllers;
using EcommerceAPI.Data;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Runtime;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderSettings _settings;
        private readonly IOrderRepository _orderRepository;
        private static List<OrderDto> _orders;

        public OrderService(IOrderRepository orderRepository, OrderSettings settings)
        {
            _orderRepository = orderRepository;
            _settings = settings;
            //_orders = new List<OrderDto>
            //{
            //    new OrderDto { Id = 1, ProductName = "Laptop", Quantity = 1, Price = 50000 },
            //    new OrderDto { Id = 2, ProductName = "Mouse", Quantity = 2, Price = 1500 }
            //};
        }


        public OrderDto CreateOrder(OrderRequest request)
        {
            if (request.Quantity >= _settings.MaxOrderQuantity)
            {
                throw new BadRequestException($"Quantity cannot exceed {_settings.MaxOrderQuantity}");
            }
            var newOrder = new OrderDto
            {
                ProductName = request.ProductName,
                Quantity = request.Quantity,
                Price = request.Quantity * _settings.DefaultPricePerUnit
            };
            return _orderRepository.CreateOrder(newOrder); ;
        }

        public bool DeleteOrder(int id)
        {
            return _orderRepository.DeleteOrder(id);
        }

        public OrderDto? GetOrderById(int id)
        {
            var result = _orderRepository.GetOrderById(id);
            return result;
        }

        public List<OrderDto> GetOrders()
        {
            return _orderRepository.GetAllOrders();
        }

        public bool UpdateOrder(int id, OrderRequest request)
        {
            // Pehle check karo ki order exist karta hai ya nahi
            var order = _orderRepository.GetOrderById(id);
            if (order == null)
            {
                throw new NotFoundException($"Order with ID {id} not found.");
            }

            // Fields update karo
            order.ProductName = request.ProductName;
            order.Quantity = request.Quantity;
            order.Price = request.Quantity * _settings.DefaultPricePerUnit;

            // Repository ko update karne bolo
            return _orderRepository.UpdateOrder(order);
        }

        public bool UpdateOrderStatus(int id, string newStatus)
        {
            var order = _orderRepository.GetOrderById(id);
            if (order == null)
            {
                throw new NotFoundException($"Order with ID {id} not found.");
            }

            var validateStatuses = new List<string> { "Pending", "Shipped", "Delivered", "Cancelled", "Paid" };
            if (!validateStatuses.Contains(newStatus))
            {
                throw new BadRequestException($"Invalid status: {newStatus}. Allowed: Pending, Paid, Shipped, Delivered, Cancelled");
            }

            order.Status = newStatus;
            _orderRepository.UpdateOrder(order);
            return true;
        }

        public List<OrderDto> GetOrdersByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return _orderRepository.GetAllOrders();
            }
            var orders = _orderRepository.GetAllOrders().Where(x => x.Status == status).ToList();
            return orders;
        }
    }
}
