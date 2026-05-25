using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Som3a.Infrastructure.Security
{
    public class SecureStorage
    {
        private static readonly string StorageDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a");

        private static readonly string KeysPath = Path.Combine(StorageDir, "keys.json");

        private Dictionary<string, string>? _cache;

        private Dictionary<string, string> LoadCache()
        {
            if (_cache != null) return _cache;

            Directory.CreateDirectory(StorageDir);

            if (!File.Exists(KeysPath))
            {
                _cache = new Dictionary<string, string>();
                return _cache;
            }

            try
            {
                var json = File.ReadAllText(KeysPath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                _cache = new Dictionary<string, string>();
            }

            return _cache;
        }

        private void SaveCache()
        {
            if (_cache == null) return;
            Directory.CreateDirectory(StorageDir);
            var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(KeysPath, json);
        }

        public Task StoreSecretAsync(string key, string value)
        {
            var cache = LoadCache();
            var encrypted = Protect(value);
            cache[key] = encrypted;
            SaveCache();
            return Task.CompletedTask;
        }

        public Task<string?> GetSecretAsync(string key)
        {
            var cache = LoadCache();
            if (cache.TryGetValue(key, out var encrypted))
            {
                return Task.FromResult<string?>(Unprotect(encrypted));
            }
            return Task.FromResult<string?>(null);
        }

        public Task<bool> DeleteSecretAsync(string key)
        {
            var cache = LoadCache();
            var removed = cache.Remove(key);
            if (removed) SaveCache();
            return Task.FromResult(removed);
        }

        private static string Protect(string plaintext)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        private static string? Unprotect(string base64)
        {
            try
            {
                var protectedBytes = Convert.FromBase64String(base64);
                var plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return null;
            }
        }
    }
}
