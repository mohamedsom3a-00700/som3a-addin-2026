using System.Threading.Tasks;

namespace Som3a.Infrastructure.Security
{
    public class ApiKeyEncryption
    {
        private readonly SecureStorage _storage;

        public ApiKeyEncryption(SecureStorage storage)
        {
            _storage = storage;
        }

        public Task StoreApiKeyAsync(string providerId, string apiKey)
        {
            var key = $"provider:{providerId}:apikey";
            return _storage.StoreSecretAsync(key, apiKey);
        }

        public Task<string?> GetApiKeyAsync(string providerId)
        {
            var key = $"provider:{providerId}:apikey";
            return _storage.GetSecretAsync(key);
        }

        public Task<bool> DeleteApiKeyAsync(string providerId)
        {
            var key = $"provider:{providerId}:apikey";
            return _storage.DeleteSecretAsync(key);
        }

        public static string BuildStorageKey(string providerId)
        {
            return $"provider:{providerId}:apikey";
        }
    }
}
