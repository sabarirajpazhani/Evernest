using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Evernest.API.Services.Interfaces;
using Evernest.API.Services;

namespace Evernest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminCodeController : ControllerBase
    {
        private readonly IAdminCodeService _adminCodeService;

        public AdminCodeController(IAdminCodeService adminCodeService)
        {
            _adminCodeService = adminCodeService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<string>> GenerateAdminCode()
        {
            try
            {
                var code = await _adminCodeService.GenerateAdminCodeAsync();
                return Ok(new { Code = code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to generate admin code", Details = ex.Message });
            }
        }

        [HttpPost("seed-default")]
        public async Task<ActionResult<string>> SeedDefaultAdminCode()
        {
            try
            {
                var code = await _adminCodeService.SeedDefaultAdminCodeAsync();
                if (code != null)
                {
                    return Ok(new { Message = "Default admin code seeded successfully", Code = code });
                }
                return Ok(new { Message = "Default admin code already exists" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to seed default admin code", Details = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<string>>> GetAllAdminCodes()
        {
            try
            {
                var codes = await _adminCodeService.GetAllAdminCodesAsync();
                return Ok(codes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to get admin codes", Details = ex.Message });
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ValidateAdminCode([FromBody] ValidateAdminCodeRequest request)
        {
            try
            {
                var isValid = await _adminCodeService.ValidateAdminCodeAsync(request.Code);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to validate admin code", Details = ex.Message });
            }
        }
    }

    public class ValidateAdminCodeRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}
