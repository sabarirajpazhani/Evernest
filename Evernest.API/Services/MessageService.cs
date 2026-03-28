using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Chat;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public MessageService(
            IMessageRepository messageRepository,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<MessageDto?> GetMessageByIdAsync(string messageId, string userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                return null;

            if (!await _chatRepository.IsParticipantAsync(message.ChatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            return await MapMessageDtoAsync(message);
        }

        public async Task<List<MessageDto>> SearchMessagesAsync(string chatId, string userId, string query)
        {
            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            var messages = await _messageRepository.SearchMessagesAsync(chatId, query);
            var dtos = new List<MessageDto>();

            foreach (var message in messages)
            {
                dtos.Add(await MapMessageDtoAsync(message));
            }

            return dtos;
        }

        public async Task<MessageDto> EditMessageAsync(string messageId, string userId, string content)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                throw new KeyNotFoundException("Message not found");

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only edit your own messages");

            if (message.IsDeleted)
                throw new InvalidOperationException("Cannot edit deleted messages");

            // Only allow editing within 15 minutes of creation
            if (DateTime.UtcNow - message.CreatedAt > TimeSpan.FromMinutes(15))
                throw new InvalidOperationException("Messages can only be edited within 15 minutes of sending");

            message.Content = content;
            message.UpdatedAt = DateTime.UtcNow;

            var updatedMessage = await _messageRepository.UpdateAsync(message);
            return await MapMessageDtoAsync(updatedMessage);
        }

        public async Task<bool> DeleteMessageAsync(string messageId, string userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                return false;

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only delete your own messages");

            return await _messageRepository.DeleteAsync(messageId);
        }

        public async Task<List<MessageDto>> GetUnreadMessagesAsync(string userId)
        {
            var userChats = await _chatRepository.GetUserChatsAsync(userId);
            var allUnreadMessages = new List<Message>();

            foreach (var chat in userChats)
            {
                var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(chat.Id, userId);
                allUnreadMessages.AddRange(unreadMessages);
            }

            var dtos = new List<MessageDto>();
            foreach (var message in allUnreadMessages)
            {
                dtos.Add(await MapMessageDtoAsync(message));
            }

            return dtos.OrderBy(m => m.CreatedAt).ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var userChats = await _chatRepository.GetUserChatsAsync(userId);
            var totalUnread = 0;

            foreach (var chat in userChats)
            {
                var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(chat.Id, userId);
                totalUnread += unreadMessages.Count;
            }

            return totalUnread;
        }

        private async Task<MessageDto> MapMessageDtoAsync(Message message)
        {
            var sender = await _userRepository.GetByIdAsync(message.SenderId);
            var replyToMessage = message.ReplyToMessageId != null ?
                await _messageRepository.GetByIdAsync(message.ReplyToMessageId) : null;

            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Sender = _mapper.Map<UserDto>(sender),
                Content = message.Content,
                Type = message.Type,
                CreatedAt = message.CreatedAt,
                Status = message.Status,
                ReplyToMessage = replyToMessage != null ? await MapMessageDtoAsync(replyToMessage) : null,
                IsDeleted = message.IsDeleted
            };
        }
    }
}
