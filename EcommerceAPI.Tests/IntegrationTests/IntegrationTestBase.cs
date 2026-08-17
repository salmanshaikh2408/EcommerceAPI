using EcommerceAPI.Controllers;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EcommerceAPI.Tests.IntegrationTests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase()
    {
        var connectionString = "Server=localhost;Database=EcommerceDB;User Id=sa;Password=YourStrong!Password123;TrustServerCertificate=True;";

        Factory = new WebApplicationFactory<Program>()
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

        Client = Factory.CreateClient();
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        var authRequest = new LoginRequest { Username = "admin", Password = "admin123" };
        var authResponse = await Client.PostAsJsonAsync("/api/Auth/login", authRequest);
        authResponse.EnsureSuccessStatusCode(); // ✅ Ensure 200 OK

        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.Token;
    }

    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
}