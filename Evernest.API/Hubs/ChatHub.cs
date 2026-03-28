using Microsoft.AspNetCore.SignalR;
using Evernest.API.DTOs.Chat;
using Evernest.API.Services.Interfaces;
using System.Security.Claims;

namespace Evernest.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatService chatService,
            IUserService userService,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await _userService.UpdateOnlineStatusAsync(userId, true);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                _logger.LogInformation($"User {userId} connected");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await _userService.UpdateOnlineStatusAsync(userId, false);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                _logger.LogInformation($"User {userId} disconnected");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string chatId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                var chat = await _chatService.GetChatByIdAsync(chatId, userId);
                if (chat == null)
                {
                    await Clients.Caller.SendAsync("Error", "Chat not found or access denied");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                await Clients.Group($"chat_{chatId}").SendAsync("UserJoined", new { UserId = userId, ChatId = chatId });
                
                _logger.LogInformation($"User {userId} joined chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat {chatId} for user {userId}");
                await Clients.Caller.SendAsync("Error", "Failed to join chat");
            }
        }

        public async Task LeaveChat(string chatId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                await Clients.Group($"chat_{chatId}").SendAsync("UserLeft", new { UserId = userId, ChatId = chatId });
                
                _logger.LogInformation($"User {userId} left chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat {chatId} for user {userId}");
                await Clients.Caller.SendAsync("Error", "Failed to leave chat");
            }
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                var message = await _chatService.SendMessageAsync(userId, messageDto);
                
                // Send to all participants in the chat
                await Clients.Group($"chat_{messageDto.ChatId}").SendAsync("ReceiveMessage", message);
                
                // Update chat list for all participants
                var chat = await _chatService.GetChatByIdAsync(messageDto.ChatId, userId);
                if (chat != null)
                {
                    foreach (var participant in chat.Participants)
                    {
                        await Clients.Group($"user_{participant.Id}").SendAsync("ChatUpdated", chat);
                    }
                }
                
                _logger.LogInformation($"User {userId} sent message in chat {messageDto.ChatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message in chat {messageDto.ChatId} for user {userId}");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task MarkAsRead(string chatId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                await _chatService.MarkMessagesAsReadAsync(chatId, userId);
                
                // Notify other participants that messages were read
                await Clients.Group($"chat_{chatId}").SendAsync("MessagesRead", new { UserId = userId, ChatId = chatId });
                
                _logger.LogInformation($"User {userId} marked messages as read in chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read in chat {chatId} for user {userId}");
                await Clients.Caller.SendAsync("Error", "Failed to mark messages as read");
            }
        }

        public async Task Typing(string chatId, bool isTyping)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            try
            {
                await _chatService.UpdateTypingStatusAsync(chatId, userId, isTyping);
                
                // Notify other participants in the chat
                await Clients.Group($"chat_{chatId}").SendAsync("UserTyping", new { UserId = userId, ChatId = chatId, IsTyping = isTyping });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating typing status in chat {chatId} for user {userId}");
            }
        }

        public async Task GetOnlineUsers(string chatId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                var chat = await _chatService.GetChatByIdAsync(chatId, userId);
                if (chat == null)
                {
                    await Clients.Caller.SendAsync("Error", "Chat not found or access denied");
                    return;
                }

                var onlineUsers = chat.Participants.Where(p => p.IsOnline).ToList();
                await Clients.Caller.SendAsync("OnlineUsers", new { ChatId = chatId, Users = onlineUsers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting online users for chat {chatId}");
                await Clients.Caller.SendAsync("Error", "Failed to get online users");
            }
        }
    }
}
