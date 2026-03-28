using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Friend;
using Evernest.API.Services.Interfaces;
using System.Security.Claims;

namespace Evernest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost("send-request")]
        public async Task<ActionResult<FriendRequestDto>> SendFriendRequest([FromBody] SendFriendRequestDto request)
        {
            try
            {
                var senderId = GetCurrentUserId();
                var result = await _friendService.SendFriendRequestAsync(senderId, request);
                return CreatedAtAction(nameof(SendFriendRequest), result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("sent-requests")]
        public async Task<ActionResult<List<FriendRequestDto>>> GetSentRequests()
        {
            try
            {
                var userId = GetCurrentUserId();
                var requests = await _friendService.GetSentRequestsAsync(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("received-requests")]
        public async Task<ActionResult<List<FriendRequestDto>>> GetReceivedRequests()
        {
            try
            {
                var userId = GetCurrentUserId();
                var requests = await _friendService.GetReceivedRequestsAsync(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("respond/{requestId}")]
        public async Task<ActionResult<FriendRequestDto>> RespondToFriendRequest(string requestId, [FromBody] RespondFriendRequestDto response)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.RespondToFriendRequestAsync(requestId, userId, response);
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("cancel/{requestId}")]
        public async Task<ActionResult<bool>> CancelFriendRequest(string requestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.CancelFriendRequestAsync(requestId, userId);
                return Ok(result);
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

        [HttpGet("list")]
        public async Task<ActionResult<List<UserDto>>> GetFriends()
        {
            try
            {
                var userId = GetCurrentUserId();
                var friends = await _friendService.GetFriendsAsync(userId);
                return Ok(friends);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("remove/{friendId}")]
        public async Task<ActionResult<bool>> RemoveFriend(string friendId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.RemoveFriendAsync(userId, friendId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("is-friend/{friendId}")]
        public async Task<ActionResult<bool>> IsFriend(string friendId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.IsFriendAsync(userId, friendId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
