using EcommerceAPI.Data;
using EcommerceAPI.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace EcommerceAPI.Tests.IntegrationTests;

public class ProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public ProductIntegrationTests()
	{
		// Real SQL Server connection string (Local / GitHub Actions / Docker)
		var connectionString = "Server=localhost;Database=EcommerceDB;User Id=sa;Password=YourStrong!Password123;TrustServerCertificate=True;";

		_factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					// Remove existing DbContext registration
					var descriptor = services.SingleOrDefault(
						d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
					if (descriptor != null)
						services.Remove(descriptor);

					// Add real database connection
					services.AddDbContext<AppDbContext>(options =>
						options.UseSqlServer(connectionString));
				});
			});
	}

	[Fact]
	public async Task CreateProduct_ShouldReturnCreated_WithValidData()
	{
		// Arrange
		var client = _factory.CreateClient();
		var request = new CreateProductDto
		{
			Name = "Integration Test Product",
			Description = "Test Description",
			Price = 1000,
			StockQuantity = 10
		};

		// Act
		var response = await client.PostAsJsonAsync("/api/Product", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Name.Should().Be("Integration Test Product");
		product.Id.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task GetProductById_ShouldReturnProduct_WithValidId()
	{
		// Arrange
		var client = _factory.CreateClient();

		// Pehle ek product create karo
		var createRequest = new CreateProductDto
		{
			Name = "Test Product",
			Description = "Test",
			Price = 500,
			StockQuantity = 5
		};
		var createResponse = await client.PostAsJsonAsync("/api/Product", createRequest);
		var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

		// Act
		var response = await client.GetAsync($"/api/Product/{createdProduct!.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Id.Should().Be(createdProduct.Id);
		product.Name.Should().Be("Test Product");
	}
}