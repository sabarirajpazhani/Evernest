using Evernest.API.Models;
using Evernest.API.DTOs.User;
using Evernest.Repository.Repositories.Interfaces;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

namespace Evernest.Repository.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "Users";

        public UserRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Email), email)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Username), username)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
        }

        public async Task<List<User>> GetAllAsync()
        {
            var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
        }

        public async Task<List<User>> GetPendingUsersAsync()
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Status), UserStatus.Pending);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
        }

        public async Task<List<User>> GetApprovedUsersAsync()
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Status), UserStatus.Approved);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
        }

        public async Task<List<User>> GetFriendsAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user?.FriendIds == null || !user.FriendIds.Any())
                return new List<User>();

            var query = _firestoreDb.Collection(CollectionName)
                .WhereIn(FieldPath.DocumentId, user.FriendIds.Take(10));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
        }

        public async Task<List<User>> SearchUsersAsync(string query, string currentUserId)
        {
            var users = new List<User>();
            
            // Search by username - get all approved users and filter client-side
            var usernameQuery = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Status), UserStatus.Approved)
                .Limit(50); // Limit for performance
            
            var usernameSnapshot = await usernameQuery.GetSnapshotAsync();
            users.AddRange(usernameSnapshot.Documents
                .Where(doc => doc.Id != currentUserId)
                .Select(doc => doc.ConvertTo<User>())
                .Where(u => u.Username.StartsWith(query, StringComparison.OrdinalIgnoreCase)));

            // Search by name - get all approved users and filter client-side
            var nameQuery = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(User.Status), UserStatus.Approved)
                .Limit(50); // Limit for performance
            
            var nameSnapshot = await nameQuery.GetSnapshotAsync();
            users.AddRange(nameSnapshot.Documents
                .Where(doc => doc.Id != currentUserId && !users.Exists(u => u.Id == doc.Id))
                .Select(doc => doc.ConvertTo<User>())
                .Where(u => u.Username.StartsWith(query, StringComparison.OrdinalIgnoreCase)));

            return users.DistinctBy(u => u.Id).Take(20).ToList();
        }

        public async Task<User> CreateAsync(User user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var docRef = _firestoreDb.Collection(CollectionName).Document(user.Id);
            await docRef.SetAsync(user);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var docRef = _firestoreDb.Collection(CollectionName).Document(user.Id);
            await docRef.SetAsync(user, SetOptions.MergeAll);
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
            return true;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var user = await GetByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var user = await GetByUsernameAsync(username);
            return user != null;
        }

        public async Task<bool> AddFriendAsync(string userId, string friendId)
        {
            var user = await GetByIdAsync(userId);
            var friend = await GetByIdAsync(friendId);

            if (user == null || friend == null)
                return false;

            if (!user.FriendIds.Contains(friendId))
                user.FriendIds.Add(friendId);

            if (!friend.FriendIds.Contains(userId))
                friend.FriendIds.Add(userId);

            await UpdateAsync(user);
            await UpdateAsync(friend);

            return true;
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            var user = await GetByIdAsync(userId);
            var friend = await GetByIdAsync(friendId);

            if (user == null || friend == null)
                return false;

            user.FriendIds.Remove(friendId);
            friend.FriendIds.Remove(userId);

            await UpdateAsync(user);
            await UpdateAsync(friend);

            return true;
        }

        public async Task<bool> IsFriendAsync(string userId, string friendId)
        {
            var user = await GetByIdAsync(userId);
            return user?.FriendIds.Contains(friendId) ?? false;
        }

        public async Task UpdateLastActiveAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastActiveAt = DateTime.UtcNow;
                await UpdateAsync(user);
            }
        }

        public async Task UpdateOnlineStatusAsync(string userId, bool isOnline)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsOnline = isOnline;
                user.LastActiveAt = DateTime.UtcNow;
                await UpdateAsync(user);
            }
        }
    }
}
