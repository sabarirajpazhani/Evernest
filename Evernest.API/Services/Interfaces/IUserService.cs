using Evernest.API.DTOs.User;
using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<UserDto>> GetPendingUsersAsync();
        Task<List<UserDto>> GetApprovedUsersAsync();
        Task<List<UserDto>> GetFriendsAsync(string userId);
        Task<List<UserDto>> SearchUsersAsync(string query, string currentUserId);
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateDto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<bool> ApproveUserAsync(string userId, ApproveUserDto approveDto);
        Task<bool> DeleteUserAsync(string userId);
        Task<UserDto> UpdateProfilePictureAsync(string userId, string pictureUrl);
        Task<bool> UpdateLastActiveAsync(string userId);
        Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline);
    }
}
