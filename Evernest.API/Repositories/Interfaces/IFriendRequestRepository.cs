using Evernest.API.Models;
using Evernest.API.DTOs.Friend;

namespace Evernest.Repository.Repositories.Interfaces
{
    public interface IFriendRequestRepository
    {
        Task<FriendRequest?> GetByIdAsync(string id);
        Task<List<FriendRequest>> GetSentRequestsAsync(string senderId);
        Task<List<FriendRequest>> GetReceivedRequestsAsync(string receiverId);
        Task<FriendRequest?> GetPendingRequestAsync(string senderId, string receiverId);
        Task<List<FriendRequest>> GetAllAsync();
        Task<FriendRequest> CreateAsync(FriendRequest friendRequest);
        Task<FriendRequest> UpdateAsync(FriendRequest friendRequest);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string senderId, string receiverId);
    }
}
