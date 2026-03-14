using NaviProject.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NaviProject.Core.Services
{
    public class UserService(IUserRepository userRepo, IAuthService authService)
    {
        public async Task<(bool Success, string Message, string? Token)> RegisterAsync(
            string username, string email, string password)
        {
            if (await userRepo.GetByUsernameAsync(username) != null)
                return (false, "Username already exists.", null);

            if (await userRepo.GetByEmailAsync(email) != null)
                return (false, "Email already exists.", null);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var userId = await userRepo.CreateUserAsync(username, email, passwordHash);
            var token = authService.GenerateToken(userId, username);

            return (true, "Registration successful.", token);
        }

        public async Task<(bool Success, string Message, string? Token)> LoginAsync(
            string username, string password)
        {
            var user = await userRepo.GetByUsernameAsync(username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, "Invalid username or password.", null);

            var token = authService.GenerateToken(user.Id, user.Username);
            return (true, "Login successful.", token);
        }
    }
}
