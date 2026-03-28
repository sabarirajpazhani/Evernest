using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Evernest.API.DTOs.Chat;
using Evernest.API.Services.Interfaces;
using System.Security.Claims;

namespace Evernest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost]
        public async Task<ActionResult<ChatDto>> CreateChat([FromBody] CreateChatDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.CreateChatAsync(userId, createDto);
                return CreatedAtAction(nameof(CreateChat), result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var chats = await _chatService.GetUserChatsAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{chatId}")]
        public async Task<ActionResult<ChatDto>> GetChatById(string chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var chat = await _chatService.GetChatByIdAsync(chatId, userId);
                if (chat == null)
                    return NotFound("Chat not found");

                return Ok(chat);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<List<MessageDto>>> GetChatMessages(string chatId, [FromQuery] int limit = 50, [FromQuery] string? lastMessageId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var messages = await _chatService.GetChatMessagesAsync(chatId, userId, limit, lastMessageId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send-message")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto messageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _chatService.SendMessageAsync(userId, messageDto);
                return CreatedAtAction(nameof(SendMessage), message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("messages/{messageId}")]
        public async Task<ActionResult<bool>> DeleteMessage(string messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.DeleteMessageAsync(messageId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/mark-read")]
        public async Task<ActionResult<bool>> MarkMessagesAsRead(string chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.MarkMessagesAsReadAsync(chatId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/leave")]
        public async Task<ActionResult<bool>> LeaveChat(string chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.LeaveChatAsync(chatId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/add-participant")]
        public async Task<ActionResult<bool>> AddParticipant(string chatId, [FromBody] string participantId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.AddParticipantAsync(chatId, userId, participantId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{chatId}/remove-participant/{participantId}")]
        public async Task<ActionResult<bool>> RemoveParticipant(string chatId, string participantId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.RemoveParticipantAsync(chatId, userId, participantId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{chatId}/typing")]
        public async Task<ActionResult<bool>> UpdateTypingStatus(string chatId, [FromBody] bool isTyping)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.UpdateTypingStatusAsync(chatId, userId, isTyping);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
