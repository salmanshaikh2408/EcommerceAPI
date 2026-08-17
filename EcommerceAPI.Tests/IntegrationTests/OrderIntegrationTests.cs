using EcommerceAPI.Controllers;
using EcommerceAPI.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EcommerceAPI.Tests.IntegrationTests;

public class OrderIntegrationTests : IntegrationTestBase
{
	public OrderIntegrationTests() : base()
	{
		var token = GetAdminTokenAsync().GetAwaiter().GetResult();
		SetAuthorizationHeader(token);
	}

	[Fact]
	public async Task CreateOrder_ShouldReturnCreated_WithValidData()
	{
		var request = new OrderRequest
		{
			ProductName = "Integration Test Mouse",
			Quantity = 2
		};

		var response = await Client.PostAsJsonAsync("/api/Order", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.ProductName.Should().Be("Integration Test Mouse");
		order.Quantity.Should().Be(2);
	}

	[Fact]
	public async Task GetOrderById_ShouldReturnOrder_WithValidId()
	{
		var createRequest = new OrderRequest
		{
			ProductName = "Test Laptop",
			Quantity = 1
		};
		var createResponse = await Client.PostAsJsonAsync("/api/Order", createRequest);
		var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

		var response = await Client.GetAsync($"/api/Order/{createdOrder!.Id}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var order = await response.Content.ReadFromJsonAsync<OrderDto>();
		order.Should().NotBeNull();
		order!.Id.Should().Be(createdOrder.Id);
		order.ProductName.Should().Be("Test Laptop");
	}
}