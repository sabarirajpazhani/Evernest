using Evernest.API.Models;
using Evernest.Repository.Repositories.Interfaces;

namespace Evernest.Repository.Repositories
{
    public class InMemoryEventRepository : IEventRepository
    {
        private static readonly Dictionary<string, Event> _events = new Dictionary<string, Event>();

        public async Task<Event?> GetByIdAsync(string id)
        {
            _events.TryGetValue(id, out var eventItem);
            return await Task.FromResult(eventItem);
        }

        public async Task<List<Event>> GetUserEventsAsync(string userId)
        {
            var userEvents = _events.Values.Where(e => 
                e.CreatedById == userId || e.AttendeeIds.Contains(userId) || e.InvitedUserIds.Contains(userId)).ToList();
            return await Task.FromResult(userEvents);
        }

        public async Task<List<Event>> GetCreatedEventsAsync(string userId)
        {
            var createdEvents = _events.Values.Where(e => e.CreatedById == userId).ToList();
            return await Task.FromResult(createdEvents);
        }

        public async Task<List<Event>> GetInvitedEventsAsync(string userId)
        {
            var invitedEvents = _events.Values.Where(e => 
                e.CreatedById != userId && e.InvitedUserIds.Contains(userId)).ToList();
            return await Task.FromResult(invitedEvents);
        }

        public async Task<bool> AddAttendeeAsync(string eventId, string userId)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                if (!eventItem.AttendeeIds.Contains(userId))
                {
                    eventItem.AttendeeIds.Add(userId);
                    eventItem.UpdatedAt = DateTime.UtcNow;
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> RemoveAttendeeAsync(string eventId, string userId)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                var removed = eventItem.AttendeeIds.Remove(userId);
                if (removed)
                {
                    eventItem.UpdatedAt = DateTime.UtcNow;
                }
                return await Task.FromResult(removed);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> InviteUserAsync(string eventId, string userId)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                if (!eventItem.InvitedUserIds.Contains(userId))
                {
                    eventItem.InvitedUserIds.Add(userId);
                    eventItem.UpdatedAt = DateTime.UtcNow;
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> UpdateRSVPAsync(string eventId, string userId, RSVPStatus status)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                eventItem.RSVPs[userId] = status;
                eventItem.UpdatedAt = DateTime.UtcNow;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> IsAttendeeAsync(string eventId, string userId)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                return await Task.FromResult(eventItem.AttendeeIds.Contains(userId));
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> IsInvitedAsync(string eventId, string userId)
        {
            if (_events.TryGetValue(eventId, out var eventItem))
            {
                return await Task.FromResult(eventItem.InvitedUserIds.Contains(userId));
            }
            return await Task.FromResult(false);
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(string userId)
        {
            var upcomingEvents = _events.Values.Where(e => 
                e.StartDate > DateTime.UtcNow && 
                (e.CreatedById == userId || e.AttendeeIds.Contains(userId) || e.InvitedUserIds.Contains(userId))).ToList();
            return await Task.FromResult(upcomingEvents);
        }

        public async Task<List<Event>> GetPastEventsAsync(string userId)
        {
            var pastEvents = _events.Values.Where(e => 
                e.EndDate < DateTime.UtcNow && 
                (e.CreatedById == userId || e.AttendeeIds.Contains(userId) || e.InvitedUserIds.Contains(userId))).ToList();
            return await Task.FromResult(pastEvents);
        }

        public async Task<Event> CreateAsync(Event eventItem)
        {
            var id = Guid.NewGuid().ToString();
            eventItem.Id = id;
            eventItem.CreatedAt = DateTime.UtcNow;
            eventItem.UpdatedAt = DateTime.UtcNow;
            
            _events[id] = eventItem;
            return await Task.FromResult(eventItem);
        }

        public async Task<Event> UpdateAsync(Event eventItem)
        {
            eventItem.UpdatedAt = DateTime.UtcNow;
            _events[eventItem.Id] = eventItem;
            return await Task.FromResult(eventItem);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await Task.FromResult(_events.Remove(id));
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await Task.FromResult(_events.Values.ToList());
        }
    }
}
