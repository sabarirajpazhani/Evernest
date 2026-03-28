using Evernest.API.Models;
using Evernest.Repository.Repositories.Interfaces;

namespace Evernest.Repository.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private static readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private static readonly Dictionary<string, string> _emailToIdMap = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _usernameToIdMap = new Dictionary<string, string>();
        private static readonly Dictionary<string, HashSet<string>> _friendships = new Dictionary<string, HashSet<string>>();

        public async Task<User?> GetByIdAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return await Task.FromResult(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (_emailToIdMap.TryGetValue(email, out var id))
            {
                _users.TryGetValue(id, out var user);
                return await Task.FromResult(user);
            }
            return await Task.FromResult<User?>(null);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (_usernameToIdMap.TryGetValue(username, out var id))
            {
                _users.TryGetValue(id, out var user);
                return await Task.FromResult(user);
            }
            return await Task.FromResult<User?>(null);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await Task.FromResult(_users.Values.ToList());
        }

        public async Task<List<User>> GetPendingUsersAsync()
        {
            var pendingUsers = _users.Values.Where(u => u.Status == UserStatus.Pending).ToList();
            return await Task.FromResult(pendingUsers);
        }

        public async Task<List<User>> GetApprovedUsersAsync()
        {
            var approvedUsers = _users.Values.Where(u => u.Status == UserStatus.Approved).ToList();
            return await Task.FromResult(approvedUsers);
        }

        public async Task<List<User>> GetFriendsAsync(string userId)
        {
            if (_friendships.TryGetValue(userId, out var friendIds))
            {
                var friends = friendIds.SelectMany(id => 
                    _users.TryGetValue(id, out var user) ? new[] { user } : Array.Empty<User>()).ToList();
                return await Task.FromResult(friends);
            }
            return await Task.FromResult(new List<User>());
        }

        public async Task<List<User>> SearchUsersAsync(string query, string currentUserId)
        {
            var searchResults = _users.Values
                .Where(u => u.Id != currentUserId && 
                           (u.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return await Task.FromResult(searchResults);
        }

        public async Task<User> CreateAsync(User user)
        {
            var id = Guid.NewGuid().ToString();
            user.Id = id;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            _users[id] = user;
            _emailToIdMap[user.Email] = id;
            _usernameToIdMap[user.Username] = id;
            _friendships[id] = new HashSet<string>();
            
            return await Task.FromResult(user);
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _users[user.Id] = user;
            return await Task.FromResult(user);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (_users.TryGetValue(id, out var user))
            {
                _users.Remove(id);
                _emailToIdMap.Remove(user.Email);
                _usernameToIdMap.Remove(user.Username);
                _friendships.Remove(id);
                
                // Remove from other users' friend lists
                foreach (var friendList in _friendships.Values)
                {
                    friendList.Remove(id);
                }
                
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await Task.FromResult(_emailToIdMap.ContainsKey(email));
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await Task.FromResult(_usernameToIdMap.ContainsKey(username));
        }

        public async Task<bool> AddFriendAsync(string userId, string friendId)
        {
            if (!_friendships.ContainsKey(userId))
                _friendships[userId] = new HashSet<string>();
            if (!_friendships.ContainsKey(friendId))
                _friendships[friendId] = new HashSet<string>();

            _friendships[userId].Add(friendId);
            _friendships[friendId].Add(userId);
            
            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            if (_friendships.TryGetValue(userId, out var userFriends))
                userFriends.Remove(friendId);
            if (_friendships.TryGetValue(friendId, out var friendFriends))
                friendFriends.Remove(userId);
                
            return await Task.FromResult(true);
        }

        public async Task<bool> IsFriendAsync(string userId, string friendId)
        {
            var isFriend = _friendships.TryGetValue(userId, out var friends) && 
                         friends.Contains(friendId);
            return await Task.FromResult(isFriend);
        }

        public async Task UpdateLastActiveAsync(string userId)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                user.LastActiveAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _users[userId] = user;
            }
            await Task.CompletedTask;
        }

        public async Task UpdateOnlineStatusAsync(string userId, bool isOnline)
        {
            if (_users.TryGetValue(userId, out var user))
            {
                user.IsOnline = isOnline;
                user.UpdatedAt = DateTime.UtcNow;
                _users[userId] = user;
            }
            await Task.CompletedTask;
        }
    }
}
