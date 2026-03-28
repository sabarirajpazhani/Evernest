using Evernest.API.Services.Interfaces;

namespace Evernest.API.Services
{
    public class InMemoryAdminCodeService : IAdminCodeService
    {
        private static readonly HashSet<string> _adminCodes = new HashSet<string> { "123456" };

        public Task<bool> ValidateAdminCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6 || !code.All(char.IsDigit))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_adminCodes.Contains(code));
        }

        public Task<string> GenerateAdminCodeAsync()
        {
            var random = new Random();
            string code;
            int attempts = 0;

            do
            {
                code = random.Next(100000, 999999).ToString();
                attempts++;
                if (attempts > 100) break;
            } while (_adminCodes.Contains(code));

            _adminCodes.Add(code);
            return Task.FromResult(code);
        }

        public Task<string?> SeedDefaultAdminCodeAsync()
        {
            const string defaultCode = "123456";
            if (!_adminCodes.Contains(defaultCode))
            {
                _adminCodes.Add(defaultCode);
                return Task.FromResult<string?>(defaultCode);
            }
            return Task.FromResult<string?>(null);
        }

        public Task<List<string>> GetAllAdminCodesAsync()
        {
            return Task.FromResult(_adminCodes.ToList());
        }
    }
}
