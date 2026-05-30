using Microsoft.Extensions.Logging;
using Som3a.Contracts;

namespace Som3a.Diagnostics
{
    public class SafeLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly long _maxLogSizeBytes;
        private readonly object _lock = new();
        private static readonly HashSet<string> PiiPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwd", "pwd", "secret", "token", "apikey", "api_key",
            "connectionstring", "connection_string", "certificate", "cert",
            "username", "user_name", "email", "ssn", "creditcard", "credit_card"
        };

        public SafeLogger(string categoryName, string logDirectory, int maxLogSizeMB = 100)
        {
            _categoryName = categoryName;
            _maxLogSizeBytes = maxLogSizeMB * 1024L * 1024L;
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "diagnostics.log");
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = SanitizeMessage(formatter(state, exception));
            var entry = $"{DateTimeOffset.UtcNow:O} [{logLevel}] [{_categoryName}] {message}";

            if (exception != null)
                entry += $"\n  Exception: {SanitizeMessage(exception.ToString())}";

            lock (_lock)
            {
                try
                {
                    EnforceLogCap();
                    File.AppendAllText(_logFilePath, entry + Environment.NewLine);
                }
                catch
                {
                    // Silently handle — logging failure must not crash the application
                }
            }
        }

        private string SanitizeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            foreach (var pattern in PiiPatterns)
            {
                var index = message.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var endIndex = message.IndexOfAny(new[] { ' ', '\n', '\r', '"', '\'' }, index + pattern.Length + 1);
                    if (endIndex < 0) endIndex = message.Length;
                    var redacted = message[..(index + pattern.Length + 1)] + "***REDACTED***";
                    message = redacted + message[endIndex..];
                }
            }

            return message;
        }

        private void EnforceLogCap()
        {
            if (!File.Exists(_logFilePath))
                return;

            var fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Length <= _maxLogSizeBytes)
                return;

            var content = File.ReadAllLines(_logFilePath);
            var trimmed = content.Length > 1000
                ? content[^1000..]
                : content;
            File.WriteAllLines(_logFilePath, trimmed);
        }
    }

    public class SafeLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly int _maxLogSizeMB;

        public SafeLoggerProvider(string logDirectory, int maxLogSizeMB = 100)
        {
            _logDirectory = logDirectory;
            _maxLogSizeMB = maxLogSizeMB;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SafeLogger(categoryName, _logDirectory, _maxLogSizeMB);
        }

        public void Dispose() { }
    }
}
