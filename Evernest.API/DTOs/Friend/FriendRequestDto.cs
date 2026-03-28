using System.ComponentModel.DataAnnotations;
using Evernest.API.Models;
using Evernest.API.DTOs.User;

namespace Evernest.API.DTOs.Friend
{
    public class FriendRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public UserDto Sender { get; set; } = new();
        public UserDto Receiver { get; set; } = new();
        public FriendRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; }
    }

    public class SendFriendRequestDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Message { get; set; }
    }

    public class RespondFriendRequestDto
    {
        [Required]
        public FriendRequestStatus Status { get; set; }
    }
}
