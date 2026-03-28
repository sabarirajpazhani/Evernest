using Evernest.API.Models;
using Evernest.API.DTOs.Event;

namespace Evernest.Repository.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(string id);
        Task<List<Event>> GetUserEventsAsync(string userId);
        Task<List<Event>> GetCreatedEventsAsync(string userId);
        Task<List<Event>> GetUpcomingEventsAsync(string userId);
        Task<List<Event>> GetInvitedEventsAsync(string userId);
        Task<Event> CreateAsync(Event eventData);
        Task<Event> UpdateAsync(Event eventData);
        Task<bool> DeleteAsync(string id);
        Task<bool> AddAttendeeAsync(string eventId, string userId);
        Task<bool> RemoveAttendeeAsync(string eventId, string userId);
        Task<bool> InviteUserAsync(string eventId, string userId);
        Task<bool> UpdateRSVPAsync(string eventId, string userId, RSVPStatus status);
        Task<bool> IsAttendeeAsync(string eventId, string userId);
        Task<bool> IsInvitedAsync(string eventId, string userId);
    }
}
