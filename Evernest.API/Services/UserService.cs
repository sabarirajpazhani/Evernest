using Evernest.API.DTOs.User;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDto>(user);
            userDto.FriendCount = user.FriendIds?.Count ?? 0;
            return userDto;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDto>(user);
            userDto.FriendCount = user.FriendIds?.Count ?? 0;
            return userDto;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<List<UserDto>> GetPendingUsersAsync()
        {
            var users = await _userRepository.GetPendingUsersAsync();
            return users.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<List<UserDto>> GetApprovedUsersAsync()
        {
            var users = await _userRepository.GetApprovedUsersAsync();
            return users.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<List<UserDto>> GetFriendsAsync(string userId)
        {
            var friends = await _userRepository.GetFriendsAsync(userId);
            return friends.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<List<UserDto>> SearchUsersAsync(string query, string currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(query, currentUserId);
            return users.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrEmpty(updateDto.Username))
            {
                if (await _userRepository.ExistsByUsernameAsync(updateDto.Username) && updateDto.Username != user.Username)
                    throw new InvalidOperationException("Username already exists");
                user.Username = updateDto.Username;
            }

            if (updateDto.Bio != null)
                user.Bio = updateDto.Bio;

            if (updateDto.ProfilePictureUrl != null)
                user.ProfilePictureUrl = updateDto.ProfilePictureUrl;

            var updatedUser = await _userRepository.UpdateAsync(user);
            var userDto = _mapper.Map<UserDto>(updatedUser);
            userDto.FriendCount = updatedUser.FriendIds?.Count ?? 0;
            return userDto;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ApproveUserAsync(string userId, ApproveUserDto approveDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Status = approveDto.Status;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<UserDto> UpdateProfilePictureAsync(string userId, string pictureUrl)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.ProfilePictureUrl = pictureUrl;
            var updatedUser = await _userRepository.UpdateAsync(user);
            var userDto = _mapper.Map<UserDto>(updatedUser);
            userDto.FriendCount = updatedUser.FriendIds?.Count ?? 0;
            return userDto;
        }

        public async Task<bool> UpdateLastActiveAsync(string userId)
        {
            await _userRepository.UpdateLastActiveAsync(userId);
            return true;
        }

        public async Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline)
        {
            await _userRepository.UpdateOnlineStatusAsync(userId, isOnline);
            return true;
        }
    }
}
