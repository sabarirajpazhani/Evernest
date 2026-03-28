using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Chat;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ChatService(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ChatDto> CreateChatAsync(string userId, CreateChatDto createDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Validate participants
            foreach (var participantId in createDto.ParticipantIds)
            {
                if (!await _userRepository.ExistsByUsernameAsync(string.Empty))
                {
                    var participant = await _userRepository.GetByIdAsync(participantId);
                    if (participant == null)
                        throw new KeyNotFoundException($"Participant {participantId} not found");
                }
            }

            // For private chats, check if chat already exists
            if (createDto.Type == ChatType.Private && createDto.ParticipantIds.Count == 1)
            {
                var existingChat = await _chatRepository.GetPrivateChatAsync(userId, createDto.ParticipantIds[0]);
                if (existingChat != null)
                    return await MapChatDtoAsync(existingChat, userId);
            }

            var chat = new Chat
            {
                Type = createDto.Type,
                Name = createDto.Name ?? string.Empty,
                ParticipantIds = new List<string> { userId },
                CreatedById = userId
            };

            chat.ParticipantIds.AddRange(createDto.ParticipantIds);

            var createdChat = await _chatRepository.CreateAsync(chat);

            // Update user's chat list
            foreach (var participantId in chat.ParticipantIds)
            {
                var participant = await _userRepository.GetByIdAsync(participantId);
                if (participant != null && !participant.ChatIds.Contains(createdChat.Id))
                {
                    participant.ChatIds.Add(createdChat.Id);
                    await _userRepository.UpdateAsync(participant);
                }
            }

            return await MapChatDtoAsync(createdChat, userId);
        }

        public async Task<List<ChatDto>> GetUserChatsAsync(string userId)
        {
            var chats = await _chatRepository.GetUserChatsAsync(userId);
            var dtos = new List<ChatDto>();

            foreach (var chat in chats)
            {
                dtos.Add(await MapChatDtoAsync(chat, userId));
            }

            return dtos.OrderByDescending(c => c.LastMessageAt).ToList();
        }

        public async Task<ChatDto?> GetChatByIdAsync(string chatId, string userId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat == null)
                return null;

            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            return await MapChatDtoAsync(chat, userId);
        }

        public async Task<List<MessageDto>> GetChatMessagesAsync(string chatId, string userId, int limit = 50, string? lastMessageId = null)
        {
            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            var messages = await _messageRepository.GetChatMessagesAsync(chatId, limit, lastMessageId);
            var dtos = new List<MessageDto>();

            foreach (var message in messages)
            {
                dtos.Add(await MapMessageDtoAsync(message));
            }

            return dtos;
        }

        public async Task<MessageDto> SendMessageAsync(string userId, SendMessageDto messageDto)
        {
            if (!await _chatRepository.IsParticipantAsync(messageDto.ChatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            var message = new Message
            {
                ChatId = messageDto.ChatId,
                SenderId = userId,
                Content = messageDto.Content,
                Type = messageDto.Type,
                ReplyToMessageId = messageDto.ReplyToMessageId
            };

            var createdMessage = await _messageRepository.CreateAsync(message);
            await _chatRepository.UpdateLastMessageAsync(messageDto.ChatId, createdMessage.Id, createdMessage.CreatedAt);

            return await MapMessageDtoAsync(createdMessage);
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

        public async Task<bool> MarkMessagesAsReadAsync(string chatId, string userId)
        {
            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            await _messageRepository.MarkAllAsReadAsync(chatId, userId);
            await _chatRepository.MarkAsReadAsync(chatId, userId);
            return true;
        }

        public async Task<bool> LeaveChatAsync(string chatId, string userId)
        {
            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            return await _chatRepository.RemoveParticipantAsync(chatId, userId);
        }

        public async Task<bool> AddParticipantAsync(string chatId, string userId, string participantId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat == null)
                throw new KeyNotFoundException("Chat not found");

            if (chat.CreatedById != userId)
                throw new UnauthorizedAccessException("Only chat creator can add participants");

            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            return await _chatRepository.AddParticipantAsync(chatId, participantId);
        }

        public async Task<bool> RemoveParticipantAsync(string chatId, string userId, string participantId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat == null)
                throw new KeyNotFoundException("Chat not found");

            if (chat.CreatedById != userId && participantId != userId)
                throw new UnauthorizedAccessException("Only chat creator can remove participants");

            return await _chatRepository.RemoveParticipantAsync(chatId, participantId);
        }

        public async Task<bool> UpdateTypingStatusAsync(string chatId, string userId, bool isTyping)
        {
            if (!await _chatRepository.IsParticipantAsync(chatId, userId))
                throw new UnauthorizedAccessException("You are not a participant in this chat");

            await _chatRepository.UpdateTypingStatusAsync(chatId, userId, isTyping);
            return true;
        }

        private async Task<ChatDto> MapChatDtoAsync(Chat chat, string currentUserId)
        {
            var participants = new List<UserDto>();
            foreach (var participantId in chat.ParticipantIds)
            {
                var participant = await _userRepository.GetByIdAsync(participantId);
                if (participant != null)
                {
                    var userDto = _mapper.Map<UserDto>(participant);
                    userDto.FriendCount = participant.FriendIds?.Count ?? 0;
                    participants.Add(userDto);
                }
            }

            var createdBy = await _userRepository.GetByIdAsync(chat.CreatedById);
            var lastMessage = chat.LastMessageId != null ? 
                await _messageRepository.GetByIdAsync(chat.LastMessageId) : null;

            var unreadCount = (await _messageRepository.GetUnreadMessagesAsync(chat.Id, currentUserId)).Count;

            return new ChatDto
            {
                Id = chat.Id,
                Type = chat.Type,
                Name = chat.Name,
                Participants = participants,
                LastMessage = lastMessage != null ? await MapMessageDtoAsync(lastMessage) : null,
                LastMessageAt = chat.LastMessageAt,
                CreatedAt = chat.CreatedAt,
                GroupPictureUrl = chat.GroupPictureUrl,
                CreatedBy = _mapper.Map<UserDto>(createdBy),
                UnreadCount = unreadCount,
                IsTyping = chat.TypingUsers.ContainsKey(currentUserId)
            };
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
