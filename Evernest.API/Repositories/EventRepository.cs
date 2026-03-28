using Evernest.API.Models;
using Evernest.API.DTOs.Event;
using Evernest.Repository.Repositories.Interfaces;
using Google.Cloud.Firestore;

namespace Evernest.Repository.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "Events";

        public EventRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Event?> GetByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Event>() : null;
        }

        public async Task<List<Event>> GetUserEventsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereArrayContains(nameof(Event.AttendeeIds), userId)
                .OrderBy(nameof(Event.StartDate));
            var snapshot = await query.GetSnapshotAsync();
            var events = snapshot.Documents.Select(doc => doc.ConvertTo<Event>()).ToList();
            
            // Filter events that are upcoming (client-side filtering for date comparison)
            return events.Where(e => e.StartDate >= DateTime.UtcNow).ToList();
        }

        public async Task<List<Event>> GetCreatedEventsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(Event.CreatedById), userId)
                .OrderByDescending(nameof(Event.CreatedAt));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Event>()).ToList();
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereArrayContains(nameof(Event.AttendeeIds), userId)
                .OrderBy(nameof(Event.StartDate));
            var snapshot = await query.GetSnapshotAsync();
            var events = snapshot.Documents.Select(doc => doc.ConvertTo<Event>()).ToList();
            
            // Filter events that are upcoming (client-side filtering for date comparison)
            return events.Where(e => e.StartDate >= DateTime.UtcNow).ToList();
        }

        public async Task<List<Event>> GetInvitedEventsAsync(string userId)
        {
            var query = _firestoreDb.Collection(CollectionName)
                .WhereArrayContains(nameof(Event.InvitedUserIds), userId)
                .WhereNotEqualTo(nameof(Event.CreatedById), userId)
                .OrderBy(nameof(Event.StartDate));
            var snapshot = await query.GetSnapshotAsync();
            var events = snapshot.Documents.Select(doc => doc.ConvertTo<Event>()).ToList();
            
            // Filter events that are upcoming (client-side filtering for date comparison)
            return events.Where(e => e.StartDate >= DateTime.UtcNow).ToList();
        }

        public async Task<Event> CreateAsync(Event eventData)
        {
            eventData.Id = Guid.NewGuid().ToString();
            eventData.CreatedAt = DateTime.UtcNow;
            eventData.UpdatedAt = DateTime.UtcNow;
            eventData.Status = EventStatus.Upcoming;

            var docRef = _firestoreDb.Collection(CollectionName).Document(eventData.Id);
            await docRef.SetAsync(eventData);
            return eventData;
        }

        public async Task<Event> UpdateAsync(Event eventData)
        {
            eventData.UpdatedAt = DateTime.UtcNow;
            var docRef = _firestoreDb.Collection(CollectionName).Document(eventData.Id);
            await docRef.SetAsync(eventData, SetOptions.MergeAll);
            return eventData;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
            return true;
        }

        public async Task<bool> AddAttendeeAsync(string eventId, string userId)
        {
            var eventData = await GetByIdAsync(eventId);
            if (eventData == null)
                return false;

            if (!eventData.AttendeeIds.Contains(userId))
                eventData.AttendeeIds.Add(userId);

            eventData.RSVPs[userId] = RSVPStatus.Attending;
            await UpdateAsync(eventData);
            return true;
        }

        public async Task<bool> RemoveAttendeeAsync(string eventId, string userId)
        {
            var eventData = await GetByIdAsync(eventId);
            if (eventData == null)
                return false;

            eventData.AttendeeIds.Remove(userId);
            eventData.RSVPs.Remove(userId);
            await UpdateAsync(eventData);
            return true;
        }

        public async Task<bool> InviteUserAsync(string eventId, string userId)
        {
            var eventData = await GetByIdAsync(eventId);
            if (eventData == null)
                return false;

            if (!eventData.InvitedUserIds.Contains(userId))
                eventData.InvitedUserIds.Add(userId);

            eventData.RSVPs[userId] = RSVPStatus.Pending;
            await UpdateAsync(eventData);
            return true;
        }

        public async Task<bool> UpdateRSVPAsync(string eventId, string userId, RSVPStatus status)
        {
            var eventData = await GetByIdAsync(eventId);
            if (eventData == null)
                return false;

            eventData.RSVPs[userId] = status;

            if (status == RSVPStatus.Attending && !eventData.AttendeeIds.Contains(userId))
            {
                eventData.AttendeeIds.Add(userId);
            }
            else if (status == RSVPStatus.NotAttending)
            {
                eventData.AttendeeIds.Remove(userId);
            }

            await UpdateAsync(eventData);
            return true;
        }

        public async Task<bool> IsAttendeeAsync(string eventId, string userId)
        {
            var eventData = await GetByIdAsync(eventId);
            return eventData?.AttendeeIds.Contains(userId) ?? false;
        }

        public async Task<bool> IsInvitedAsync(string eventId, string userId)
        {
            var eventData = await GetByIdAsync(eventId);
            return eventData?.InvitedUserIds.Contains(userId) ?? false;
        }
    }
}
