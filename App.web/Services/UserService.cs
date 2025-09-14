using App.web.Data;
using App.web.Models;
using App.web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace App.web.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
                return null;

            var normalizedEmail = email.Trim().ToLower();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            return user;
        }

        public async Task<User?> FindByIdAndEmail(Guid id, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id && u.Email.ToLower() == normalizedEmail);
        }

        public async Task ConfirmEmail(User user)
        {
            if (user == null) return;

            user.EmailConfirmed = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task AddUser(User user)
        {
            if (user == null) return;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task AddProfile(UserProfile profile)
        {
            if (profile == null) return;

            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            if (user == null) return;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAttempts(User user)
        {
            if (user == null) return;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
