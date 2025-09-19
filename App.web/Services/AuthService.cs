using App.web.Data;
using App.web.Models;
using App.web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App.web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<AuthResult> RegisterAsync(string fullName, string email, string password)
        {
            var existing = await _userService.FindByEmail(email);
            if (existing != null)
                return new AuthResult { Success = false, Message = "Email already registered." };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                PasswordHash = HashPassword(password),
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                LockoutEnd = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            await _userService.AddUser(user);

            var profile = new UserProfile
            {
                ProfileId = Guid.NewGuid(),
                UserId = user.UserId,
                FullName = fullName,
                PhotoUrl = "",
                TimeZone = "UTC",
                Language = "es",
                PreferencesJson = ""
            };

            await _userService.AddProfile(profile);

            var autoRole = await _userService.FindByNameRole("User");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = autoRole.RoleId
            };

            await _userService.AddUserRole(userRole);

            return await Task.FromResult(new AuthResult
            {
                Success = true,
                User = user,
                Message = "Registration successful! Please confirm your email."
            });
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userService.FindByEmail(email);
            
            if (user == null)
                return new AuthResult { Success = false, Message = "User not found" };

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Your account is bloqued until {user.LockoutEnd.Value}."
                };
            }

            if (user.PasswordHash != HashPassword(password))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // shell 15 min
                }

                await _userService.UpdateAttempts(user);

                return new AuthResult { Success = false, Message = "Invalid password" };
            }

            if (!user.EmailConfirmed)
                return new AuthResult { Success = false, Message = "Email not confirmed" };

            return await Task.FromResult(new AuthResult
            {
                Success = true,
                User = user
            });
        }

        public async Task UpdatePasswordAsync(User user, string newPassword)
        {
            user.PasswordHash = HashPassword(newPassword);
            await _userService.UpdateUser(user);
            await Task.CompletedTask;
        }

        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
