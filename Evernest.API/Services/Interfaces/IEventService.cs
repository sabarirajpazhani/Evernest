using Evernest.API.DTOs.Event;
using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDto> CreateEventAsync(string userId, CreateEventDto createDto);
        Task<EventDto?> GetEventByIdAsync(string eventId, string userId);
        Task<List<EventDto>> GetUserEventsAsync(string userId);
        Task<List<EventDto>> GetCreatedEventsAsync(string userId);
        Task<List<EventDto>> GetUpcomingEventsAsync(string userId);
        Task<List<EventDto>> GetInvitedEventsAsync(string userId);
        Task<EventDto> UpdateEventAsync(string eventId, string userId, UpdateEventDto updateDto);
        Task<bool> DeleteEventAsync(string eventId, string userId);
        Task<bool> InviteUsersAsync(string eventId, string userId, List<string> userIds);
        Task<bool> RSVPEventAsync(string eventId, string userId, RSVPDto rsvpDto);
        Task<bool> CancelRSVPAsync(string eventId, string userId);
    }
}
