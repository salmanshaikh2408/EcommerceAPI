using AutoMapper;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCache = new Mock<IDistributedCache>();
        _service = new ProductService(_mockRepo.Object,_mockMapper.Object, _mockCache.Object);
    }

    [Fact]
    public void CreateProduct_WithNegativePrice_ShouldThrowException()
    {
        var request = new CreateProductDto { Name = "Test", Price = -100 };
        var exception = Assert.Throws<BadRequestException>(() =>
            _service.CreateProduct(request));
        exception.Message.Should().Contain("Price must be greater than 0");
    }

    [Fact]
    public void DeleteProduct_WithValidId_ShouldReturnTrue()
    {
        var productId = 1;
        var product = new Product { Id = 1, Name = "Laptop", Price = 50000 };
        _mockRepo.Setup(r => r.GetProductById(productId)).Returns(product);   // GetById mock
        _mockRepo.Setup(r => r.DeleteProduct(productId)).Returns(true);       // Delete mock

        var result = _service.DeleteProduct(productId);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetProductById_WithInvalidId_ShouldReturnNull()
    {
        var productId = 999;
        _mockRepo.Setup(r => r.GetProductById(productId)).Returns((Product)null);

        var result = _service.GetProductById(productId);

        result.Should().BeNull();
    }
}