using AutoMapper;
using EcommerceAPI.Data;
using EcommerceAPI.Mapping;
using EcommerceAPI.Middlewares;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT Authentication
var key = Encoding.ASCII.GetBytes("EcommerceAPI_SecretKey_32Char_12345678");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add services to the container.

builder.Services.AddControllers();
// AutoMapper manually register karo
builder.Services.AddSingleton<IMapper>(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()).CreateMapper());

var orderSettings = builder.Configuration.GetSection("OrderSettings").Get<OrderSettings>();
builder.Services.AddSingleton(orderSettings);

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Redis Cache Register
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "EcommerceAPI";
});

//Database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Agar Local Windows me chal rahe ho (GitHub Actions nahi)
if (OperatingSystem.IsWindows() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
{
    // Windows Authentication switch karo (sa password hatao)
    connectionString = connectionString
        .Replace("User Id=sa;Password=12345;", "")
        .Replace("User Id=sa;Password=12345;", ""); // Double safety

    // Agar connection string me Trusted_Connection nahi hai toh add karo
    if (!connectionString.Contains("Trusted_Connection"))
    {
        connectionString += "Trusted_Connection=True;";
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen
    (c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Ecommerce API",
            Version = "v1"
        });
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });
    });

//Global error handling middleware
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddMemoryCache();
var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp => { });
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapControllers();

app.Run();
public partial class Program { }
