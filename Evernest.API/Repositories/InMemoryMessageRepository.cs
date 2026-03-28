using Evernest.API.Models;
using Evernest.Repository.Repositories.Interfaces;

namespace Evernest.Repository.Repositories
{
    public class InMemoryMessageRepository : IMessageRepository
    {
        private static readonly Dictionary<string, Message> _messages = new Dictionary<string, Message>();

        public async Task<Message?> GetByIdAsync(string id)
        {
            _messages.TryGetValue(id, out var message);
            return await Task.FromResult(message);
        }

        public async Task<List<Message>> GetChatMessagesAsync(string chatId, int limit = 50, string? lastMessageId = null)
        {
            var chatMessages = _messages.Values.Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt).ToList();

            if (!string.IsNullOrEmpty(lastMessageId))
            {
                var lastMessage = _messages.Values.FirstOrDefault(m => m.Id == lastMessageId);
                if (lastMessage != null)
                {
                    chatMessages = chatMessages.Where(m => m.CreatedAt > lastMessage.CreatedAt).ToList();
                }
            }

            var limitedMessages = chatMessages.Take(limit).ToList();
            return await Task.FromResult(limitedMessages);
        }

        public async Task<List<Message>> GetBySenderIdAsync(string senderId)
        {
            var senderMessages = _messages.Values.Where(m => m.SenderId == senderId).ToList();
            return await Task.FromResult(senderMessages);
        }

        public async Task<List<Message>> GetUnreadMessagesAsync(string chatId, string userId)
        {
            var unreadMessages = _messages.Values.Where(m => 
                m.ChatId == chatId && 
                m.SenderId != userId && 
                !m.ReadBy.Contains(userId)).ToList();
            return await Task.FromResult(unreadMessages);
        }

        public async Task<Message> CreateAsync(Message message)
        {
            var id = Guid.NewGuid().ToString();
            message.Id = id;
            message.CreatedAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;
            
            _messages[id] = message;
            return await Task.FromResult(message);
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            message.UpdatedAt = DateTime.UtcNow;
            _messages[message.Id] = message;
            return await Task.FromResult(message);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await Task.FromResult(_messages.Remove(id));
        }

        public async Task<bool> MarkAsReadAsync(string messageId, string userId)
        {
            if (_messages.TryGetValue(messageId, out var message))
            {
                if (!message.ReadBy.Contains(userId))
                {
                    message.ReadBy.Add(userId);
                    message.UpdatedAt = DateTime.UtcNow;
                    _messages[messageId] = message;
                }
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> MarkAllAsReadAsync(string chatId, string userId)
        {
            var messagesToMark = _messages.Values.Where(m => 
                m.ChatId == chatId && 
                m.SenderId != userId && 
                !m.ReadBy.Contains(userId)).ToList();

            foreach (var message in messagesToMark)
            {
                message.ReadBy.Add(userId);
                message.UpdatedAt = DateTime.UtcNow;
                _messages[message.Id] = message;
            }
            return await Task.FromResult(true);
        }

        public async Task<List<Message>> SearchMessagesAsync(string chatId, string query)
        {
            var searchResults = _messages.Values.Where(m => 
                m.ChatId == chatId && 
                (m.Content.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
            return await Task.FromResult(searchResults);
        }
    }
}
