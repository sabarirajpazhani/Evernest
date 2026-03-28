using Evernest.API.Models;
using Evernest.API.DTOs.Chat;

namespace Evernest.Repository.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<Chat?> GetByIdAsync(string id);
        Task<List<Chat>> GetUserChatsAsync(string userId);
        Task<Chat?> GetPrivateChatAsync(string userId1, string userId2);
        Task<List<Chat>> GetGroupChatsAsync(string userId);
        Task<Chat> CreateAsync(Chat chat);
        Task<Chat> UpdateAsync(Chat chat);
        Task<bool> DeleteAsync(string id);
        Task<bool> AddParticipantAsync(string chatId, string userId);
        Task<bool> RemoveParticipantAsync(string chatId, string userId);
        Task<bool> IsParticipantAsync(string chatId, string userId);
        Task UpdateLastMessageAsync(string chatId, string messageId, DateTime messageTime);
        Task UpdateTypingStatusAsync(string chatId, string userId, bool isTyping);
        Task MarkAsReadAsync(string chatId, string userId);
    }
}
