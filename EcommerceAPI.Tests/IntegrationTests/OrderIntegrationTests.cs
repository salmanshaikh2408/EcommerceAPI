using EcommerceAPI.Controllers;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EcommerceAPI.Tests.IntegrationTests;

public class OrderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;
	private readonly string _adminToken;

	public OrderIntegrationTests()
	{
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

		_client = _factory.CreateClient();

		// ✅ Admin token generate karo
		var authRequest = new LoginRequest { Username = "admin", Password = "admin123" };
		var authResponse = _client.PostAsJsonAsync("/api/Auth/login", authRequest).Result;
		var authResult = authResponse.Content.ReadFromJsonAsync<AuthResponse>().Result;
		_adminToken = authResult!.Token;

		// ✅ Token set karo
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
	}

	[Fact]
	public async Task CreateOrder_ShouldReturnCreated_WithValidData()
	{
		var request = new OrderRequest
		{
			ProductName = "Integration Test Mouse",
			Quantity = 2
		};

		var response = await _client.PostAsJsonAsync("/api/Order", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.ProductName.Should().Be("Integration Test Mouse");
		order.Quantity.Should().Be(2);
		order.Status.Should().Be("Pending");
	}

	[Fact]
	public async Task GetOrderById_ShouldReturnOrder_WithValidId()
	{
		// Pehle order create karo
		var createRequest = new OrderRequest
		{
			ProductName = "Test Laptop",
			Quantity = 1
		};
		var createResponse = await _client.PostAsJsonAsync("/api/Order", createRequest);
		var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

		// Ab GetById call karo
		var response = await _client.GetAsync($"/api/Order/{createdOrder!.Id}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.Id.Should().Be(createdOrder.Id);
		order.ProductName.Should().Be("Test Laptop");
	}
}