using Evernest.API.Models;
using Evernest.API.DTOs.Chat;
using Evernest.Repository.Repositories.Interfaces;
using Google.Cloud.Firestore;

namespace Evernest.Repository.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "Messages";

        public MessageRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Message?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Message>() : null;
        }

        public async Task<List<Message>> GetChatMessagesAsync(string chatId, int limit = 50, string? lastMessageId = null)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(Message.ChatId), chatId)
                .WhereEqualTo(nameof(Message.IsDeleted), false)
                .OrderByDescending(nameof(Message.CreatedAt));

            if (!string.IsNullOrEmpty(lastMessageId))
            {
                var lastMessage = await GetByIdAsync(lastMessageId);
                if (lastMessage != null)
                {
                    query = query.WhereLessThan(nameof(Message.CreatedAt), lastMessage.CreatedAt);
                }
            }

            var snapshot = await query.Limit(limit).GetSnapshotAsync();
            var messages = snapshot.Documents.Select(doc => doc.ConvertTo<Message>()).ToList();
            messages.Reverse(); // Show oldest first
            return messages;
        }

        public async Task<List<Message>> GetUnreadMessagesAsync(string chatId, string userId)
        {
            var chatRef = _firestoreDb.Collection("Chats").Document(chatId);
            var chatSnapshot = await chatRef.GetSnapshotAsync();
            
            if (!chatSnapshot.Exists)
                return new List<Message>();

            var chat = chatSnapshot.ConvertTo<Chat>();
            var lastReadAt = chat.LastReadAt.GetValueOrDefault(userId, DateTime.MinValue);

            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(Message.ChatId), chatId)
                .WhereGreaterThan(nameof(Message.CreatedAt), lastReadAt)
                .WhereEqualTo(nameof(Message.IsDeleted), false)
                .WhereNotEqualTo(nameof(Message.SenderId), userId)
                .OrderBy(nameof(Message.CreatedAt));

            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Message>()).ToList();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            message.Id = Guid.NewGuid().ToString();
            message.CreatedAt = DateTime.UtcNow;
            message.Status = MessageStatus.Sent;

            var docRef = _firestoreDb.Collection(CollectionName).Document(message.Id);
            await docRef.SetAsync(message);
            return message;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            message.UpdatedAt = DateTime.UtcNow;
            var docRef = _firestoreDb.Collection(CollectionName).Document(message.Id);
            await docRef.SetAsync(message, SetOptions.MergeAll);
            return message;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var message = await GetByIdAsync(id);
            if (message != null)
            {
                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;
                await UpdateAsync(message);
            }
            return true;
        }

        public async Task<bool> MarkAsReadAsync(string messageId, string userId)
        {
            var message = await GetByIdAsync(messageId);
            if (message != null && !message.ReadBy.Contains(userId))
            {
                message.ReadBy.Add(userId);
                message.Status = MessageStatus.Read;
                await UpdateAsync(message);
            }
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string chatId, string userId)
        {
            var unreadMessages = await GetUnreadMessagesAsync(chatId, userId);
            foreach (var message in unreadMessages)
            {
                await MarkAsReadAsync(message.Id, userId);
            }
            return true;
        }

        public async Task<List<Message>> SearchMessagesAsync(string chatId, string query)
        {
            // Note: Firestore doesn't support full-text search natively
            // This is a simplified implementation that could be enhanced with Algolia or similar
            var messages = await GetChatMessagesAsync(chatId, 1000);
            return messages.Where(m => 
                m.Content.Contains(query, StringComparison.OrdinalIgnoreCase) && 
                !m.IsDeleted).ToList();
        }
    }
}
