using Evernest.API.DTOs.Chat;
using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatDto> CreateChatAsync(string userId, CreateChatDto createDto);
        Task<List<ChatDto>> GetUserChatsAsync(string userId);
        Task<ChatDto?> GetChatByIdAsync(string chatId, string userId);
        Task<List<MessageDto>> GetChatMessagesAsync(string chatId, string userId, int limit = 50, string? lastMessageId = null);
        Task<MessageDto> SendMessageAsync(string userId, SendMessageDto messageDto);
        Task<bool> DeleteMessageAsync(string messageId, string userId);
        Task<bool> MarkMessagesAsReadAsync(string chatId, string userId);
        Task<bool> LeaveChatAsync(string chatId, string userId);
        Task<bool> AddParticipantAsync(string chatId, string userId, string participantId);
        Task<bool> RemoveParticipantAsync(string chatId, string userId, string participantId);
        Task<bool> UpdateTypingStatusAsync(string chatId, string userId, bool isTyping);
    }
}
