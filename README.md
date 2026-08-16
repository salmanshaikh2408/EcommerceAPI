# EcommerceAPI 🛒

A complete E-commerce API built with .NET 8, JWT Authentication, SQL Server, EF Core, and Unit Tests.

## 🚀 Tech Stack
- .NET 8 (Web API)
- SQL Server (EF Core)
- JWT Authentication + Role-based Authorization
- Repository Pattern
- Global Exception Handling
- xUnit + Moq + Testcontainers (Unit & Integration Tests)

## 📦 Features
- Product Management (CRUD)
- Order Management (CRUD + Status Update)
- User Registration & Login (JWT)
- Role-based Access (Admin vs User)
- Logging Middleware

## ▶️ How to Run
1. Clone the repo
2. Update `appsettings.json` with your connection string
3. Run `dotnet ef database update`
4. Run `dotnet run`
5. Open `https://localhost:7098/swagger`
