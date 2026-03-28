using Evernest.API.Models;
using Evernest.Repository.Repositories.Interfaces;

namespace Evernest.Repository.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private static readonly Dictionary<string, Chat> _chats = new Dictionary<string, Chat>();

        public async Task<Chat?> GetByIdAsync(string id)
        {
            _chats.TryGetValue(id, out var chat);
            return await Task.FromResult(chat);
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            var userChats = _chats.Values.Where(c => 
                c.ParticipantIds.Contains(userId)).ToList();
            return await Task.FromResult(userChats);
        }

        public async Task<Chat?> GetPrivateChatAsync(string userId1, string userId2)
        {
            var privateChat = _chats.Values.FirstOrDefault(c => 
                c.Type == ChatType.Private &&
                c.ParticipantIds.Contains(userId1) &&
                c.ParticipantIds.Contains(userId2) &&
                c.ParticipantIds.Count == 2);
            return await Task.FromResult(privateChat);
        }

        public async Task<List<Chat>> GetGroupChatsAsync(string userId)
        {
            var groupChats = _chats.Values.Where(c => 
                c.Type == ChatType.Group &&
                c.ParticipantIds.Contains(userId)).ToList();
            return await Task.FromResult(groupChats);
        }

        public async Task<bool> AddParticipantAsync(string chatId, string userId)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                if (!chat.ParticipantIds.Contains(userId))
                {
                    chat.ParticipantIds.Add(userId);
                    chat.UpdatedAt = DateTime.UtcNow;
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> RemoveParticipantAsync(string chatId, string userId)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                var removed = chat.ParticipantIds.Remove(userId);
                if (removed)
                {
                    chat.UpdatedAt = DateTime.UtcNow;
                }
                return await Task.FromResult(removed);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> IsParticipantAsync(string chatId, string userId)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                return await Task.FromResult(chat.ParticipantIds.Contains(userId));
            }
            return await Task.FromResult(false);
        }

        public async Task UpdateLastMessageAsync(string chatId, string messageId, DateTime messageTime)
        {
            if (_chats.TryGetValue(chatId, out var chat))
            {
                chat.LastMessageId = messageId;
                chat.LastMessageAt = messageTime;
                chat.UpdatedAt = DateTime.UtcNow;
            }
            await Task.CompletedTask;
        }

        public async Task UpdateTypingStatusAsync(string chatId, string userId, bool isTyping)
        {
            // For in-memory implementation, we could store typing status in a separate dictionary
            // For now, this is a placeholder implementation
            await Task.CompletedTask;
        }

        public async Task MarkAsReadAsync(string chatId, string userId)
        {
            // For in-memory implementation, we could store read status in a separate dictionary
            // For now, this is a placeholder implementation
            await Task.CompletedTask;
        }

        public async Task<Chat> CreateAsync(Chat chat)
        {
            var id = Guid.NewGuid().ToString();
            chat.Id = id;
            chat.CreatedAt = DateTime.UtcNow;
            chat.UpdatedAt = DateTime.UtcNow;
            
            _chats[id] = chat;
            return await Task.FromResult(chat);
        }

        public async Task<Chat> UpdateAsync(Chat chat)
        {
            chat.UpdatedAt = DateTime.UtcNow;
            _chats[chat.Id] = chat;
            return await Task.FromResult(chat);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await Task.FromResult(_chats.Remove(id));
        }

        public async Task<List<Chat>> GetAllAsync()
        {
            return await Task.FromResult(_chats.Values.ToList());
        }
    }
}
