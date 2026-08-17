using System.Net;
using System.Net.Http.Json;
using EcommerceAPI.Models;
using FluentAssertions;

namespace EcommerceAPI.Tests.IntegrationTests;

public class ProductIntegrationTests : IntegrationTestBase
{
	public ProductIntegrationTests() : base()
	{
		// ✅ Constructor me token fetch karo
		var token = GetAdminTokenAsync().GetAwaiter().GetResult();
		SetAuthorizationHeader(token);
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

		var response = await Client.PostAsJsonAsync("/api/Product", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Name.Should().Be("Integration Test Product");
		product.Id.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task GetProductById_ShouldReturnProduct_WithValidId()
	{
		var createRequest = new CreateProductDto
		{
			Name = "Test Product",
			Description = "Test",
			Price = 500,
			StockQuantity = 5
		};
		var createResponse = await Client.PostAsJsonAsync("/api/Product", createRequest);
		var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

		var response = await Client.GetAsync($"/api/Product/{createdProduct!.Id}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var product = await response.Content.ReadFromJsonAsync<ProductDto>();
		product.Should().NotBeNull();
		product!.Id.Should().Be(createdProduct.Id);
		product.Name.Should().Be("Test Product");
	}
}