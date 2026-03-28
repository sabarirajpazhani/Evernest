using Evernest.API.DTOs.Auth;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.API.Configuration;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly IAdminCodeService _adminCodeService;
        private readonly AdminCodeSettings _adminCodeSettings;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, IMapper mapper, IAdminCodeService adminCodeService, AdminCodeSettings adminCodeSettings)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _mapper = mapper;
            _adminCodeService = adminCodeService;
            _adminCodeSettings = adminCodeSettings;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            if (user.Status != UserStatus.Approved)
                throw new UnauthorizedAccessException("Your account is not approved yet");

            if (!await VerifyPasswordAsync(request.Password, user.PasswordHash ?? string.Empty))
                throw new UnauthorizedAccessException("Invalid email or password");

            await _userRepository.UpdateLastActiveAsync(user.Id);

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username, user.Role);
            var userDto = _mapper.Map<UserDto>(user);

            return new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new InvalidOperationException("Email already exists");

            if (await _userRepository.ExistsByUsernameAsync(request.Username))
                throw new InvalidOperationException("Username already exists");

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = await HashPasswordAsync(request.Password),
                Status = UserStatus.Approved,
                Role = UserRole.User
            };

            var createdUser = await _userRepository.CreateAsync(user);
            var userDto = _mapper.Map<UserDto>(createdUser);

            // Generate token since user is auto-approved
            var token = _jwtService.GenerateToken(createdUser.Id, createdUser.Email, createdUser.Username, createdUser.Role);

            return new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return _jwtService.ValidateToken(token);
        }

        public async Task<string?> GetUserIdFromTokenAsync(string token)
        {
            return _jwtService.GetUserIdFromToken(token);
        }

        public async Task<string> HashPasswordAsync(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
