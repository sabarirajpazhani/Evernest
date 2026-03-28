using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Evernest.API.Services.Interfaces;

namespace Evernest.API.Services
{
    public class AdminCodeService : IAdminCodeService
    {
        private readonly FirestoreDb _firestoreDb;
        private const string CollectionName = "adminCodes";

        public AdminCodeService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<bool> ValidateAdminCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6 || !code.All(char.IsDigit))
            {
                return false;
            }

            var snapshot = await _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(AdminCode.Code), code)
                .WhereEqualTo(nameof(AdminCode.IsActive), true)
                .GetSnapshotAsync();

            return snapshot.Documents.Count > 0;
        }

        public async Task<string> GenerateAdminCodeAsync()
        {
            var random = new Random();
            string code;
            bool exists;

            do
            {
                code = random.Next(100000, 999999).ToString();
                var snapshot = await _firestoreDb.Collection(CollectionName)
                    .WhereEqualTo(nameof(AdminCode.Code), code)
                    .GetSnapshotAsync();
                exists = snapshot.Documents.Count > 0;
            } while (exists);

            var adminCode = new AdminCode
            {
                Code = code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(adminCode);
            return code;
        }

        // Seed with a default admin code for initial setup
        public async Task<string?> SeedDefaultAdminCodeAsync()
        {
            var defaultCode = "123456"; // Default admin code for initial setup
            var existingSnapshot = await _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(AdminCode.Code), defaultCode)
                .GetSnapshotAsync();

            if (existingSnapshot.Documents.Count == 0)
            {
                var defaultAdminCode = new AdminCode
                {
                    Code = defaultCode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _firestoreDb.Collection(CollectionName).AddAsync(defaultAdminCode);
                return defaultCode;
            }

            return null; // Code already exists
        }

        public async Task<List<string>> GetAllAdminCodesAsync()
        {
            var snapshot = await _firestoreDb.Collection(CollectionName)
                .WhereEqualTo(nameof(AdminCode.IsActive), true)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<AdminCode>().Code)
                .ToList();
        }
    }

    [FirestoreData]
    public class AdminCode
    {
        [FirestoreProperty]
        public string Code { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool IsActive { get; set; } = true;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? UsedAt { get; set; }

        [FirestoreProperty]
        public string? UsedBy { get; set; }
    }
}
