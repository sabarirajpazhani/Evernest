using Evernest.API.DTOs.Chat;
using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto?> GetMessageByIdAsync(string messageId, string userId);
        Task<List<MessageDto>> SearchMessagesAsync(string chatId, string userId, string query);
        Task<MessageDto> EditMessageAsync(string messageId, string userId, string content);
        Task<bool> DeleteMessageAsync(string messageId, string userId);
        Task<List<MessageDto>> GetUnreadMessagesAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
