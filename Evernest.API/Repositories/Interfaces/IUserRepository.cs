using Evernest.API.Models;
using Evernest.API.DTOs.User;

namespace Evernest.Repository.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetPendingUsersAsync();
        Task<List<User>> GetApprovedUsersAsync();
        Task<List<User>> GetFriendsAsync(string userId);
        Task<List<User>> SearchUsersAsync(string query, string currentUserId);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> AddFriendAsync(string userId, string friendId);
        Task<bool> RemoveFriendAsync(string userId, string friendId);
        Task<bool> IsFriendAsync(string userId, string friendId);
        Task UpdateLastActiveAsync(string userId);
        Task UpdateOnlineStatusAsync(string userId, bool isOnline);
    }
}
