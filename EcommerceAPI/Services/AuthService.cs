using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceAPI.Services
{
    public class AuthService : IAuthService
    {
        //private readonly List<User> _users = new List<User> 
        //{
        //    new User { Id = 1, UserName = "admin", PasswordHash = "admin123", Role = "Admin" },
        //    new User { Id = 2, UserName = "user", PasswordHash = "user123", Role = "User" }
        //};
        private readonly string _secretKey = "EcommerceAPI_SecretKey_32Char_12345678";
        private readonly AppDbContext _context;
        public AuthService(AppDbContext context)
        {
            _context = context;
        }



        public string? Login(string username, string password)
        {
            try
            {

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"DEBUG: username={username ?? "null"}, password={password ?? "null"}");
                    return null;
                }

                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return null;

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in Login: {ex.Message}");
                return null;
            }
        }

        public User? Register(User user, string password)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
                return null;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password); // In a real application, hash the password
            //user.Id = (_context.Users.Any()? _context.Users.Max(u => u.Id):0) + 1;
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
    }
}
