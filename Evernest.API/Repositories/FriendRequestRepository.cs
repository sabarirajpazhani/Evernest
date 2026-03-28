using Evernest.API.Models;
using Evernest.API.DTOs.Friend;
using Evernest.Repository.Repositories.Interfaces;
using Google.Cloud.Firestore;

namespace Evernest.Repository.Repositories
{
    public class FriendRequestRepository : IFriendRequestRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "FriendRequests";

        public FriendRequestRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<FriendRequest?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<FriendRequest>() : null;
        }

        public async Task<List<FriendRequest>> GetSentRequestsAsync(string senderId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(FriendRequest.SenderId), senderId)
                .OrderByDescending(nameof(FriendRequest.CreatedAt));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<FriendRequest>()).ToList();
        }

        public async Task<List<FriendRequest>> GetReceivedRequestsAsync(string receiverId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(FriendRequest.ReceiverId), receiverId)
                .OrderByDescending(nameof(FriendRequest.CreatedAt));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<FriendRequest>()).ToList();
        }

        public async Task<FriendRequest?> GetPendingRequestAsync(string senderId, string receiverId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(FriendRequest.SenderId), senderId)
                .WhereEqualTo(nameof(FriendRequest.ReceiverId), receiverId)
                .WhereEqualTo(nameof(FriendRequest.Status), FriendRequestStatus.Pending)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault()?.ConvertTo<FriendRequest>();
        }

        public async Task<List<FriendRequest>> GetAllAsync()
        {
            var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<FriendRequest>()).ToList();
        }

        public async Task<FriendRequest> CreateAsync(FriendRequest friendRequest)
        {
            friendRequest.Id = Guid.NewGuid().ToString();
            friendRequest.CreatedAt = DateTime.UtcNow;

            var docRef = _firestoreDb.Collection(CollectionName).Document(friendRequest.Id);
            await docRef.SetAsync(friendRequest);
            return friendRequest;
        }

        public async Task<FriendRequest> UpdateAsync(FriendRequest friendRequest)
        {
            friendRequest.RespondedAt = DateTime.UtcNow;
            var docRef = _firestoreDb.Collection(CollectionName).Document(friendRequest.Id);
            await docRef.SetAsync(friendRequest, SetOptions.MergeAll);
            return friendRequest;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string senderId, string receiverId)
        {
            var request = await GetPendingRequestAsync(senderId, receiverId);
            return request != null;
        }
    }
}
