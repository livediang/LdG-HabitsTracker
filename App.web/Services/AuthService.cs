using App.web.Data;
using App.web.Models;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;

namespace App.web.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;

        public AuthService(ApplicationDbContext context, TokenService tokenService, EmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<User> RegisterAsync(string fullName, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("This email has already registered.");

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

            _context.Users.Add(user);

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

            _context.UserProfiles.Add(profile);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
                return new AuthResult { Success = false, Message = "Mail user or Password incorrect." };

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Success = false, Message = $"Your account is bloqued until {user.LockoutEnd.Value}."
                };
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // shell 15 min
                }

                _context.SaveChanges();

                return new AuthResult { Success = false, Message = "Mail user or Password incorrect." };
            }

            if (!user.EmailConfirmed)
                return new AuthResult { Success = false, Message = "Please confirm your mail." };

            // Reset 
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            _context.SaveChanges();

            return new AuthResult { Success = true, User = user };
        }

        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
