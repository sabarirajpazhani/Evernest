using Evernest.API.Models;
using Evernest.Repository.Repositories.Interfaces;

namespace Evernest.Repository.Repositories
{
    public class InMemoryFriendRequestRepository : IFriendRequestRepository
    {
        private static readonly Dictionary<string, FriendRequest> _friendRequests = new Dictionary<string, FriendRequest>();

        public async Task<FriendRequest?> GetByIdAsync(string id)
        {
            _friendRequests.TryGetValue(id, out var friendRequest);
            return await Task.FromResult(friendRequest);
        }

        public async Task<List<FriendRequest>> GetSentRequestsAsync(string senderId)
        {
            var requests = _friendRequests.Values.Where(fr => fr.SenderId == senderId).ToList();
            return await Task.FromResult(requests);
        }

        public async Task<List<FriendRequest>> GetReceivedRequestsAsync(string receiverId)
        {
            var requests = _friendRequests.Values.Where(fr => fr.ReceiverId == receiverId).ToList();
            return await Task.FromResult(requests);
        }

        public async Task<FriendRequest?> GetPendingRequestAsync(string senderId, string receiverId)
        {
            var request = _friendRequests.Values.FirstOrDefault(fr => 
                fr.SenderId == senderId && 
                fr.ReceiverId == receiverId && 
                fr.Status == FriendRequestStatus.Pending);
            return await Task.FromResult(request);
        }

        public async Task<List<FriendRequest>> GetAllAsync()
        {
            return await Task.FromResult(_friendRequests.Values.ToList());
        }

        public async Task<bool> ExistsAsync(string senderId, string receiverId)
        {
            var exists = _friendRequests.Values.Any(fr => 
                fr.SenderId == senderId && fr.ReceiverId == receiverId);
            return await Task.FromResult(exists);
        }

        public async Task<FriendRequest> CreateAsync(FriendRequest friendRequest)
        {
            var id = Guid.NewGuid().ToString();
            friendRequest.Id = id;
            friendRequest.CreatedAt = DateTime.UtcNow;
            
            _friendRequests[id] = friendRequest;
            return await Task.FromResult(friendRequest);
        }

        public async Task<FriendRequest> UpdateAsync(FriendRequest friendRequest)
        {
            friendRequest.RespondedAt = DateTime.UtcNow;
            _friendRequests[friendRequest.Id] = friendRequest;
            return await Task.FromResult(friendRequest);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await Task.FromResult(_friendRequests.Remove(id));
        }

        public async Task<List<FriendRequest>> GetFriendsAsync(string userId)
        {
            var friends = _friendRequests.Values.Where(fr => 
                (fr.SenderId == userId || fr.ReceiverId == userId) && 
                fr.Status == FriendRequestStatus.Accepted).ToList();
            return await Task.FromResult(friends);
        }
    }
}
