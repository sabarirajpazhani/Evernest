using Evernest.API.Models;
using Evernest.API.DTOs.Chat;

namespace Evernest.Repository.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(string id);
        Task<List<Message>> GetChatMessagesAsync(string chatId, int limit = 50, string? lastMessageId = null);
        Task<List<Message>> GetUnreadMessagesAsync(string chatId, string userId);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(string id);
        Task<bool> MarkAsReadAsync(string messageId, string userId);
        Task<bool> MarkAllAsReadAsync(string chatId, string userId);
        Task<List<Message>> SearchMessagesAsync(string chatId, string query);
    }
}
