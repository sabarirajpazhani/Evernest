using Evernest.API.Models;
using Evernest.API.DTOs.Chat;
using Evernest.Repository.Repositories.Interfaces;
using Google.Cloud.Firestore;

namespace Evernest.Repository.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "Chats";

        public ChatRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Chat?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Chat>() : null;
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereArrayContains(nameof(Chat.ParticipantIds), userId)
                .OrderByDescending(nameof(Chat.LastMessageAt));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Chat>()).ToList();
        }

        public async Task<Chat?> GetPrivateChatAsync(string userId1, string userId2)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(Chat.Type), ChatType.Private)
                .WhereArrayContains(nameof(Chat.ParticipantIds), userId1)
                .WhereArrayContains(nameof(Chat.ParticipantIds), userId2)
                .Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault()?.ConvertTo<Chat>();
        }

        public async Task<List<Chat>> GetGroupChatsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(Chat.Type), ChatType.Group)
                .WhereArrayContains(nameof(Chat.ParticipantIds), userId)
                .OrderByDescending(nameof(Chat.LastMessageAt));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Chat>()).ToList();
        }

        public async Task<Chat> CreateAsync(Chat chat)
        {
            chat.Id = Guid.NewGuid().ToString();
            chat.CreatedAt = DateTime.UtcNow;
            chat.UpdatedAt = DateTime.UtcNow;

            var docRef = _firestoreDb.Collection(CollectionName).Document(chat.Id);
            await docRef.SetAsync(chat);
            return chat;
        }

        public async Task<Chat> UpdateAsync(Chat chat)
        {
            chat.UpdatedAt = DateTime.UtcNow;
            var docRef = _firestoreDb.Collection(CollectionName).Document(chat.Id);
            await docRef.SetAsync(chat, SetOptions.MergeAll);
            return chat;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
            return true;
        }

        public async Task<bool> AddParticipantAsync(string chatId, string userId)
        {
            var chat = await GetByIdAsync(chatId);
            if (chat == null || chat.ParticipantIds.Contains(userId))
                return false;

            chat.ParticipantIds.Add(userId);
            await UpdateAsync(chat);
            return true;
        }

        public async Task<bool> RemoveParticipantAsync(string chatId, string userId)
        {
            var chat = await GetByIdAsync(chatId);
            if (chat == null || !chat.ParticipantIds.Contains(userId))
                return false;

            chat.ParticipantIds.Remove(userId);
            await UpdateAsync(chat);
            return true;
        }

        public async Task<bool> IsParticipantAsync(string chatId, string userId)
        {
            var chat = await GetByIdAsync(chatId);
            return chat?.ParticipantIds.Contains(userId) ?? false;
        }

        public async Task UpdateLastMessageAsync(string chatId, string messageId, DateTime messageTime)
        {
            var chat = await GetByIdAsync(chatId);
            if (chat != null)
            {
                chat.LastMessageId = messageId;
                chat.LastMessageAt = messageTime;
                await UpdateAsync(chat);
            }
        }

        public async Task UpdateTypingStatusAsync(string chatId, string userId, bool isTyping)
        {
            var chat = await GetByIdAsync(chatId);
            if (chat != null)
            {
                if (isTyping)
                    chat.TypingUsers[userId] = true;
                else
                    chat.TypingUsers.Remove(userId);
                
                await UpdateAsync(chat);
            }
        }

        public async Task MarkAsReadAsync(string chatId, string userId)
        {
            var chat = await GetByIdAsync(chatId);
            if (chat != null)
            {
                chat.LastReadAt[userId] = DateTime.UtcNow;
                await UpdateAsync(chat);
            }
        }
    }
}
