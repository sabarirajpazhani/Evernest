using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Event;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public EventService(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<EventDto> CreateEventAsync(string userId, CreateEventDto createDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (createDto.StartDate < DateTime.UtcNow)
                throw new ArgumentException("Event start date cannot be in the past");

            if (createDto.EndDate <= createDto.StartDate)
                throw new ArgumentException("Event end date must be after start date");

            var @event = new Event
            {
                Title = createDto.Title,
                Description = createDto.Description,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Location = createDto.Location,
                CreatedById = userId,
                InvitedUserIds = createDto.InviteUserIds ?? new List<string>(),
                IsPrivate = createDto.IsPrivate,
                Status = EventStatus.Upcoming
            };

            // Validate invited users
            foreach (var inviteUserId in @event.InvitedUserIds)
            {
                var invitedUser = await _userRepository.GetByIdAsync(inviteUserId);
                if (invitedUser == null)
                    throw new KeyNotFoundException($"Invited user {inviteUserId} not found");
            }

            var createdEvent = await _eventRepository.CreateAsync(@event);

            // Update user's event list
            var currentUser = await _userRepository.GetByIdAsync(userId);
            if (currentUser != null && !currentUser.EventIds.Contains(createdEvent.Id))
            {
                currentUser.EventIds.Add(createdEvent.Id);
                await _userRepository.UpdateAsync(currentUser);
            }

            return await MapEventDtoAsync(createdEvent, userId);
        }

        public async Task<EventDto?> GetEventByIdAsync(string eventId, string userId)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                return null;

            // Check if user has access (creator, attendee, or invited)
            if (@event.CreatedById != userId && 
                !@event.AttendeeIds.Contains(userId) && 
                !@event.InvitedUserIds.Contains(userId))
                throw new UnauthorizedAccessException("You don't have access to this event");

            return await MapEventDtoAsync(@event, userId);
        }

        public async Task<List<EventDto>> GetUserEventsAsync(string userId)
        {
            var events = await _eventRepository.GetUserEventsAsync(userId);
            var dtos = new List<EventDto>();

            foreach (var @event in events)
            {
                dtos.Add(await MapEventDtoAsync(@event, userId));
            }

            return dtos.OrderBy(e => e.StartDate).ToList();
        }

        public async Task<List<EventDto>> GetCreatedEventsAsync(string userId)
        {
            var events = await _eventRepository.GetCreatedEventsAsync(userId);
            var dtos = new List<EventDto>();

            foreach (var @event in events)
            {
                dtos.Add(await MapEventDtoAsync(@event, userId));
            }

            return dtos.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public async Task<List<EventDto>> GetUpcomingEventsAsync(string userId)
        {
            var events = await _eventRepository.GetUpcomingEventsAsync(userId);
            var dtos = new List<EventDto>();

            foreach (var @event in events)
            {
                dtos.Add(await MapEventDtoAsync(@event, userId));
            }

            return dtos.OrderBy(e => e.StartDate).ToList();
        }

        public async Task<List<EventDto>> GetInvitedEventsAsync(string userId)
        {
            var events = await _eventRepository.GetInvitedEventsAsync(userId);
            var dtos = new List<EventDto>();

            foreach (var @event in events)
            {
                dtos.Add(await MapEventDtoAsync(@event, userId));
            }

            return dtos.OrderBy(e => e.StartDate).ToList();
        }

        public async Task<EventDto> UpdateEventAsync(string eventId, string userId, UpdateEventDto updateDto)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                throw new KeyNotFoundException("Event not found");

            if (@event.CreatedById != userId)
                throw new UnauthorizedAccessException("Only event creator can update the event");

            if (updateDto.StartDate.HasValue && updateDto.StartDate < DateTime.UtcNow)
                throw new ArgumentException("Event start date cannot be in the past");

            if (updateDto.StartDate.HasValue && updateDto.EndDate.HasValue && updateDto.EndDate <= updateDto.StartDate)
                throw new ArgumentException("Event end date must be after start date");

            if (!string.IsNullOrEmpty(updateDto.Title))
                @event.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Description))
                @event.Description = updateDto.Description;

            if (updateDto.StartDate.HasValue)
                @event.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                @event.EndDate = updateDto.EndDate.Value;

            if (!string.IsNullOrEmpty(updateDto.Location))
                @event.Location = updateDto.Location;

            if (updateDto.Status.HasValue)
                @event.Status = updateDto.Status.Value;

            var updatedEvent = await _eventRepository.UpdateAsync(@event);
            return await MapEventDtoAsync(updatedEvent, userId);
        }

        public async Task<bool> DeleteEventAsync(string eventId, string userId)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                return false;

            if (@event.CreatedById != userId)
                throw new UnauthorizedAccessException("Only event creator can delete the event");

            return await _eventRepository.DeleteAsync(eventId);
        }

        public async Task<bool> InviteUsersAsync(string eventId, string userId, List<string> userIds)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                throw new KeyNotFoundException("Event not found");

            if (@event.CreatedById != userId)
                throw new UnauthorizedAccessException("Only event creator can invite users");

            foreach (var inviteUserId in userIds)
            {
                var invitedUser = await _userRepository.GetByIdAsync(inviteUserId);
                if (invitedUser == null)
                    throw new KeyNotFoundException($"User {inviteUserId} not found");

                await _eventRepository.InviteUserAsync(eventId, inviteUserId);
            }

            return true;
        }

        public async Task<bool> RSVPEventAsync(string eventId, string userId, RSVPDto rsvpDto)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                throw new KeyNotFoundException("Event not found");

            if (!@event.InvitedUserIds.Contains(userId) && @event.CreatedById != userId)
                throw new UnauthorizedAccessException("You are not invited to this event");

            return await _eventRepository.UpdateRSVPAsync(eventId, userId, rsvpDto.Status);
        }

        public async Task<bool> CancelRSVPAsync(string eventId, string userId)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                throw new KeyNotFoundException("Event not found");

            return await _eventRepository.UpdateRSVPAsync(eventId, userId, RSVPStatus.NotAttending);
        }

        private async Task<EventDto> MapEventDtoAsync(Event @event, string currentUserId)
        {
            var createdBy = await _userRepository.GetByIdAsync(@event.CreatedById);
            var attendees = new List<UserDto>();
            var invitedUsers = new List<UserDto>();

            foreach (var attendeeId in @event.AttendeeIds)
            {
                var attendee = await _userRepository.GetByIdAsync(attendeeId);
                if (attendee != null)
                {
                    var userDto = _mapper.Map<UserDto>(attendee);
                    userDto.FriendCount = attendee.FriendIds?.Count ?? 0;
                    attendees.Add(userDto);
                }
            }

            foreach (var invitedUserId in @event.InvitedUserIds)
            {
                var invitedUser = await _userRepository.GetByIdAsync(invitedUserId);
                if (invitedUser != null)
                {
                    var userDto = _mapper.Map<UserDto>(invitedUser);
                    userDto.FriendCount = invitedUser.FriendIds?.Count ?? 0;
                    invitedUsers.Add(userDto);
                }
            }

            var userRSVP = @event.RSVPs.GetValueOrDefault(currentUserId, RSVPStatus.Pending);

            return new EventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                CreatedBy = _mapper.Map<UserDto>(createdBy),
                Attendees = attendees,
                InvitedUsers = invitedUsers,
                Status = @event.Status,
                CreatedAt = @event.CreatedAt,
                EventPictureUrl = @event.EventPictureUrl,
                IsPrivate = @event.IsPrivate,
                UserRSVP = userRSVP,
                AttendingCount = @event.AttendeeIds.Count,
                InvitedCount = @event.InvitedUserIds.Count
            };
        }
    }
}
