using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Friend;
using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IFriendService
    {
        Task<FriendRequestDto> SendFriendRequestAsync(string senderId, SendFriendRequestDto request);
        Task<List<FriendRequestDto>> GetSentRequestsAsync(string userId);
        Task<List<FriendRequestDto>> GetReceivedRequestsAsync(string userId);
        Task<FriendRequestDto> RespondToFriendRequestAsync(string requestId, string userId, RespondFriendRequestDto response);
        Task<bool> CancelFriendRequestAsync(string requestId, string userId);
        Task<List<UserDto>> GetFriendsAsync(string userId);
        Task<bool> RemoveFriendAsync(string userId, string friendId);
        Task<bool> IsFriendAsync(string userId, string friendId);
    }
}
