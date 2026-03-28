using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace Evernest.API.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty]
        [StringLength(500)]
        public string? Bio { get; set; }

        [FirestoreProperty]
        public string? ProfilePictureUrl { get; set; }

        [FirestoreProperty]
        public UserStatus Status { get; set; } = UserStatus.Pending;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? UpdatedAt { get; set; }

        [FirestoreProperty]
        public DateTime? LastActiveAt { get; set; }

        [FirestoreProperty]
        public bool IsOnline { get; set; } = false;

        [FirestoreProperty]
        public UserRole Role { get; set; } = UserRole.User;

        [FirestoreProperty]
        public List<string> FriendIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> ChatIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> EventIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public string? PasswordHash { get; set; }
    }

    public enum UserStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Suspended = 3
    }

    public enum UserRole
    {
        User = 0,
        Admin = 1
    }
}
