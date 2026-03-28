using Google.Cloud.Firestore;

namespace Evernest.API.Models
{
    [FirestoreData]
    public class Message
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ChatId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SenderId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Content { get; set; } = string.Empty;

        [FirestoreProperty]
        public MessageType Type { get; set; } = MessageType.Text;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? UpdatedAt { get; set; }

        [FirestoreProperty]
        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        [FirestoreProperty]
        public List<string> ReadBy { get; set; } = new List<string>();

        [FirestoreProperty]
        public string? ReplyToMessageId { get; set; }

        [FirestoreProperty]
        public Dictionary<string, object>? Metadata { get; set; }

        [FirestoreProperty]
        public bool IsDeleted { get; set; } = false;

        [FirestoreProperty]
        public DateTime? DeletedAt { get; set; }
    }

    public enum MessageType
    {
        Text = 0,
        Image = 1,
        File = 2,
        Voice = 3,
        System = 4
    }

    public enum MessageStatus
    {
        Sent = 0,
        Delivered = 1,
        Read = 2,
        Failed = 3
    }
}
