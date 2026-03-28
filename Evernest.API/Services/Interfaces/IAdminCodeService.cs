namespace Evernest.API.Services.Interfaces
{
    public interface IAdminCodeService
    {
        Task<bool> ValidateAdminCodeAsync(string code);
        Task<string> GenerateAdminCodeAsync();
        Task<string?> SeedDefaultAdminCodeAsync();
        Task<List<string>> GetAllAdminCodesAsync();
    }
}
