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

public class ProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;
	private readonly string _adminToken;

	public ProductIntegrationTests()
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

		// ✅ Admin token generate karo (AuthService ke through)
		var authRequest = new LoginRequest { Username = "admin", Password = "admin123" };
		var authResponse = _client.PostAsJsonAsync("/api/Auth/login", authRequest).Result;
		var authResult = authResponse.Content.ReadFromJsonAsync<AuthResponse>().Result;
		_adminToken = authResult!.Token;

		// ✅ Token ko default header mein set karo
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
	}

	[Fact]
	public async Task CreateProduct_ShouldReturnCreated_WithValidData()
	{
		var request = new CreateProductDto
		{
			Name = "Integration Test Product",
			Description = "Test Description",
			Price = 1000,
			StockQuantity = 10
		};

		var response = await _client.PostAsJsonAsync("/api/Product", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Name.Should().Be("Integration Test Product");
		product.Id.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task GetProductById_ShouldReturnProduct_WithValidId()
	{
		// Pehle product create karo
		var createRequest = new CreateProductDto
		{
			Name = "Test Product",
			Description = "Test",
			Price = 500,
			StockQuantity = 5
		};
		var createResponse = await _client.PostAsJsonAsync("/api/Product", createRequest);
		var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

		// Ab GetById call karo
		var response = await _client.GetAsync($"/api/Product/{createdProduct!.Id}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Id.Should().Be(createdProduct.Id);
		product.Name.Should().Be("Test Product");
	}
}

// ✅ AuthResponse helper class
public class AuthResponse
{
	public string Token { get; set; } = string.Empty;
}