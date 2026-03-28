using Microsoft.AspNetCore.Mvc;
using Evernest.API.DTOs.Auth;
using Evernest.API.Services.Interfaces;
using Evernest.API.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Evernest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AdminCodeSettings _adminCodeSettings;

        public AuthController(IAuthService authService, AdminCodeSettings adminCodeSettings)
        {
            _authService = authService;
            _adminCodeSettings = adminCodeSettings;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
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

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(Register), result);
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

        [HttpGet("admin-code-required")]
        public ActionResult<bool> IsAdminCodeRequired()
        {
            return Ok(_adminCodeSettings.RequireAdminCode);
        }

        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateToken([FromBody] string token)
        {
            try
            {
                var result = await _authService.ValidateTokenAsync(token);
                return Ok(result);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
