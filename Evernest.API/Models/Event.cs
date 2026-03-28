using Google.Cloud.Firestore;

namespace Evernest.API.Models
{
    [FirestoreData]
    public class Event
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Title { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime StartDate { get; set; }

        [FirestoreProperty]
        public DateTime EndDate { get; set; }

        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CreatedById { get; set; } = string.Empty;

        [FirestoreProperty]
        public List<string> AttendeeIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> InvitedUserIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? UpdatedAt { get; set; }

        [FirestoreProperty]
        public string? EventPictureUrl { get; set; }

        [FirestoreProperty]
        public bool IsPrivate { get; set; } = true;

        [FirestoreProperty]
        public Dictionary<string, RSVPStatus> RSVPs { get; set; } = new Dictionary<string, RSVPStatus>();
    }

    public enum EventStatus
    {
        Upcoming = 0,
        Ongoing = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum RSVPStatus
    {
        Pending = 0,
        Attending = 1,
        NotAttending = 2,
        Maybe = 3
    }
}
