using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IAuthService
    {
        string? Login(string username, string password);
        User? Register(User user, string password);
    }
}
