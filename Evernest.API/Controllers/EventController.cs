using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Evernest.API.DTOs.Event;
using Evernest.API.Services.Interfaces;
using System.Security.Claims;

namespace Evernest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.CreateEventAsync(userId, createDto);
                return CreatedAtAction(nameof(CreateEvent), result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{eventId}")]
        public async Task<ActionResult<EventDto>> GetEventById(string eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var @event = await _eventService.GetEventByIdAsync(eventId, userId);
                if (@event == null)
                    return NotFound("Event not found");

                return Ok(@event);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetUserEvents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetUserEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("created")]
        public async Task<ActionResult<List<EventDto>>> GetCreatedEvents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetCreatedEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<EventDto>>> GetUpcomingEvents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetUpcomingEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("invited")]
        public async Task<ActionResult<List<EventDto>>> GetInvitedEvents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetInvitedEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{eventId}")]
        public async Task<ActionResult<EventDto>> UpdateEvent(string eventId, [FromBody] UpdateEventDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.UpdateEventAsync(eventId, userId, updateDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult<bool>> DeleteEvent(string eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.DeleteEventAsync(eventId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{eventId}/invite")]
        public async Task<ActionResult<bool>> InviteUsers(string eventId, [FromBody] List<string> userIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.InviteUsersAsync(eventId, userId, userIds);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{eventId}/rsvp")]
        public async Task<ActionResult<bool>> RSVPEvent(string eventId, [FromBody] RSVPDto rsvpDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.RSVPEventAsync(eventId, userId, rsvpDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{eventId}/cancel-rsvp")]
        public async Task<ActionResult<bool>> CancelRSVP(string eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.CancelRSVPAsync(eventId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
