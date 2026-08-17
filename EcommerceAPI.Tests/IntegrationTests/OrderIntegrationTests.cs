using EcommerceAPI.Controllers;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace EcommerceAPI.Tests.IntegrationTests;

public class OrderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public OrderIntegrationTests()
	{
		// Real SQL Server connection string
		var connectionString = "Server=localhost;Database=EcommerceDB;User Id=sa;Password=YourStrong!Password123;TrustServerCertificate=True;";

		_factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					var descriptor = services.SingleOrDefault(
						d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
					if (descriptor != null)
						services.Remove(descriptor);

					services.AddDbContext<AppDbContext>(options =>
						options.UseSqlServer(connectionString));
				});
			});
	}

	[Fact]
	public async Task CreateOrder_ShouldReturnCreated_WithValidData()
	{
		// Arrange
		var client = _factory.CreateClient();
		var request = new OrderRequest
		{
			ProductName = "Integration Test Mouse",
			Quantity = 2
		};

		// Act
		var response = await client.PostAsJsonAsync("/api/Order", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.ProductName.Should().Be("Integration Test Mouse");
		order.Quantity.Should().Be(2);
		order.Status.Should().Be("Pending"); // Default status
	}

	[Fact]
	public async Task GetOrderById_ShouldReturnOrder_WithValidId()
	{
		// Arrange
		var client = _factory.CreateClient();

		// Pehle order create karo
		var createRequest = new OrderRequest
		{
			ProductName = "Test Laptop",
			Quantity = 1
		};
		var createResponse = await client.PostAsJsonAsync("/api/Order", createRequest);
		var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

		// Act
		var response = await client.GetAsync($"/api/Order/{createdOrder!.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.Id.Should().Be(createdOrder.Id);
		order.ProductName.Should().Be("Test Laptop");
	}
}