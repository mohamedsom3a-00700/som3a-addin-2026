using System;
using System.IO;
using System.Text.Json;

namespace Som3a.Infrastructure.Configuration
{
    public class AppConfiguration
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a");

        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

        private Dictionary<string, JsonElement> _values = new();

        public AppConfiguration()
        {
            Load();
        }

        public string GetValue(string key, string defaultValue = "")
        {
            if (_values.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.String)
                return element.GetString() ?? defaultValue;
            return defaultValue;
        }

        public int GetIntValue(string key, int defaultValue = 0)
        {
            if (_values.TryGetValue(key, out var element) && element.TryGetInt32(out var v))
                return v;
            return defaultValue;
        }

        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            if (_values.TryGetValue(key, out var element))
            {
                if (element.ValueKind == JsonValueKind.True) return true;
                if (element.ValueKind == JsonValueKind.False) return false;
            }
            return defaultValue;
        }

        public void SetValue(string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            _values[key] = JsonSerializer.Deserialize<JsonElement>(json);
        }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(_values, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        private void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                _values = new Dictionary<string, JsonElement>();
                return;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                _values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                    ?? new Dictionary<string, JsonElement>();
            }
            catch
            {
                _values = new Dictionary<string, JsonElement>();
            }
        }
    }
}
