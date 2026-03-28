using Google.Cloud.Firestore;

namespace Evernest.API.Models
{
    [FirestoreData]
    public class FriendRequest
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SenderId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ReceiverId { get; set; } = string.Empty;

        [FirestoreProperty]
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? RespondedAt { get; set; }

        [FirestoreProperty]
        public string? Message { get; set; }
    }

    public enum FriendRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Cancelled = 3
    }
}
