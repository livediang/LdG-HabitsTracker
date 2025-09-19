using App.web.Models;
using System.Threading.Tasks;

namespace App.web.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByEmail(string email);
        Task<User?> FindByIdAndEmail(Guid id, string email);
        Task<Role?> FindByNameRole(string autoRole);
        Task ConfirmEmail(User user);
        Task AddUser(User user);
        Task AddProfile(UserProfile profile);
        Task AddUserRole(UserRole userRole);
        Task UpdateUser(User user);
        Task UpdateAttempts(User user);
    }
}
