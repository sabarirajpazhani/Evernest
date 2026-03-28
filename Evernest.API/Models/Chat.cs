using Google.Cloud.Firestore;

namespace Evernest.API.Models
{
    [FirestoreData]
    public class Chat
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public ChatType Type { get; set; } = ChatType.Private;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty]
        public List<string> ParticipantIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public string? LastMessageId { get; set; }

        [FirestoreProperty]
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? UpdatedAt { get; set; }

        [FirestoreProperty]
        public string? GroupPictureUrl { get; set; }

        [FirestoreProperty]
        public string CreatedById { get; set; } = string.Empty;

        [FirestoreProperty]
        public Dictionary<string, DateTime> LastReadAt { get; set; } = new Dictionary<string, DateTime>();

        [FirestoreProperty]
        public Dictionary<string, bool> TypingUsers { get; set; } = new Dictionary<string, bool>();
    }

    public enum ChatType
    {
        Private = 0,
        Group = 1
    }
}
