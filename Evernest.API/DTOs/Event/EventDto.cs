using System.ComponentModel.DataAnnotations;
using Evernest.API.Models;
using Evernest.API.DTOs.User;

namespace Evernest.API.DTOs.Event
{
    public class EventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public UserDto CreatedBy { get; set; } = new();
        public List<UserDto> Attendees { get; set; } = new();
        public List<UserDto> InvitedUsers { get; set; } = new();
        public EventStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? EventPictureUrl { get; set; }
        public bool IsPrivate { get; set; }
        public RSVPStatus UserRSVP { get; set; }
        public int AttendingCount { get; set; }
        public int InvitedCount { get; set; }
    }

    public class CreateEventDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        public List<string> InviteUserIds { get; set; } = new();
        public bool IsPrivate { get; set; } = true;
    }

    public class UpdateEventDto
    {
        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public EventStatus? Status { get; set; }
    }

    public class RSVPDto
    {
        [Required]
        public RSVPStatus Status { get; set; }
    }
}
