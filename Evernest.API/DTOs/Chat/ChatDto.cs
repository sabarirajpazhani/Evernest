using System.ComponentModel.DataAnnotations;
using Evernest.API.Models;
using Evernest.API.DTOs.User;

namespace Evernest.API.DTOs.Chat
{
    public class ChatDto
    {
        public string Id { get; set; } = string.Empty;
        public ChatType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<UserDto> Participants { get; set; } = new();
        public MessageDto? LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? GroupPictureUrl { get; set; }
        public UserDto CreatedBy { get; set; } = new();
        public int UnreadCount { get; set; }
        public bool IsTyping { get; set; }
    }

    public class MessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public UserDto Sender { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageStatus Status { get; set; }
        public MessageDto? ReplyToMessage { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SendMessageDto
    {
        [Required]
        public string ChatId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        public string? ReplyToMessageId { get; set; }
    }

    public class CreateChatDto
    {
        [Required]
        public ChatType Type { get; set; } = ChatType.Private;

        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        public List<string> ParticipantIds { get; set; } = new();
    }
}
