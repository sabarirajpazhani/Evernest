using Evernest.API.Models;

namespace Evernest.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string username, UserRole role);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
    }
}
