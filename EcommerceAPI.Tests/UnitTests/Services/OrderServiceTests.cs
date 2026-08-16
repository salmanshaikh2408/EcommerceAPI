using Xunit;
using Moq;
using FluentAssertions;
using EcommerceAPI.Services;
using EcommerceAPI.Repositories;
using EcommerceAPI.Models;
using EcommerceAPI.Controllers;
using EcommerceAPI.Exceptions;

namespace EcommerceAPI.Tests.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepo;
    private readonly OrderService _service;
    private readonly OrderSettings _settings;

    public OrderServiceTests()
    {
        _mockRepo = new Mock<IOrderRepository>();
        _settings = new OrderSettings { MaxOrderQuantity = 10, DefaultPricePerUnit = 1000 };
        _service = new OrderService(_mockRepo.Object, _settings);
    }

    [Fact]
    public void CreateOrder_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var request = new OrderRequest { ProductName = "Laptop", Quantity = 3 };
        _mockRepo.Setup(r => r.CreateOrder(It.IsAny<OrderDto>()))
                 .Returns((OrderDto o) => o);

        // Act
        var result = _service.CreateOrder(request);   // ✅ request pass karo

        // Assert
        result.Should().NotBeNull();
        result.ProductName.Should().Be("Laptop");
        result.Quantity.Should().Be(3);
        result.Price.Should().Be(3000);
    }

    [Fact]
    public void CreateOrder_WithExceedingQuantity_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new OrderRequest { ProductName = "Laptop", Quantity = 15 };
        // Act
        Action act = () => _service.CreateOrder(request);
        // Assert
        act.Should().Throw<BadRequestException>()
           .WithMessage($"Quantity cannot exceed {_settings.MaxOrderQuantity}");
    }

    [Fact]
    public void GetOrderById_WithExistingId_ShouldReturnOrder()
    {
        // Arrange
        var order = new OrderDto { Id = 1, ProductName = "Laptop", Quantity = 2 };
        _mockRepo.Setup(r => r.GetOrderById(1)).Returns(order);

        var result = _service.GetOrderById(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.ProductName.Should().Be("Laptop");
    }
}