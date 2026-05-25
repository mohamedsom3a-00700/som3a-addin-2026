using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Som3a_WPF_UI.Services
{
    public interface ILoggingService
    {
        void Write(LogEntry entry);
        void Log(string severity, string category, string message, string source = null, string exception = null);
        IReadOnlyList<LogEntry> GetRecentEntries(int count = 50);
        string CurrentLogFilePath { get; }
    }

    public sealed class LoggingService : ILoggingService
    {
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const int MaxRotatedFiles = 3;
        private const string LogDirectory = "Som3a\\Logs";
        private const string LogFilePrefix = "diagnostics-";

        private readonly string _logDir;
        private readonly object _writeLock = new object();
        private string _currentLogFilePath;

        public string CurrentLogFilePath => _currentLogFilePath;

        public LoggingService()
        {
            _logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                LogDirectory);

            try
            {
                Directory.CreateDirectory(_logDir);
            }
            catch
            {
                _logDir = Path.Combine(Path.GetTempPath(), LogDirectory);
                Directory.CreateDirectory(_logDir);
            }

            _currentLogFilePath = GenerateLogFilePath();
        }

        public void Write(LogEntry entry)
        {
            try
            {
                var line = FormatEntry(entry);
                lock (_writeLock)
                {
                    CheckRotation();
                    File.AppendAllText(_currentLogFilePath, line, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        public void Log(string severity, string category, string message, string source = null, string exception = null)
        {
            Write(new LogEntry
            {
                Timestamp = DateTime.Now,
                Severity = severity,
                Category = category,
                Message = message?.Length > 500 ? message.Substring(0, 500) : message ?? string.Empty,
                Source = source ?? string.Empty,
                Exception = exception?.Length > 1000 ? exception.Substring(0, 1000) : exception ?? string.Empty
            });
        }

        public IReadOnlyList<LogEntry> GetRecentEntries(int count = 50)
        {
            var entries = new List<LogEntry>();
            try
            {
                string fileToRead;
                lock (_writeLock)
                    fileToRead = _currentLogFilePath;

                if (!File.Exists(fileToRead))
                    return entries;

                var lines = File.ReadAllLines(fileToRead, Encoding.UTF8);
                var recent = lines.Reverse().Take(count).Reverse();

                foreach (var line in recent)
                {
                    var entry = ParseEntry(line);
                    if (entry != null)
                        entries.Add(entry);
                }
            }
            catch
            {
            }
            return entries;
        }

        private string GenerateLogFilePath()
        {
            return Path.Combine(_logDir, $"{LogFilePrefix}{DateTime.Now:yyyyMMdd}.log");
        }

        private void CheckRotation()
        {
            var current = _currentLogFilePath;
            if (!File.Exists(current))
                return;

            try
            {
                var fileInfo = new FileInfo(current);
                if (fileInfo.Length < MaxFileSize)
                    return;

                for (int i = MaxRotatedFiles - 1; i >= 1; i--)
                {
                    var older = Path.Combine(_logDir, $"{LogFilePrefix}{DateTime.Now:yyyyMMdd}.log.{i}");
                    var newer = Path.Combine(_logDir, $"{LogFilePrefix}{DateTime.Now:yyyyMMdd}.log.{i + 1}");

                    if (File.Exists(older))
                    {
                        if (i == MaxRotatedFiles - 1)
                            File.Delete(older);
                        else
                            File.Move(older, newer);
                    }
                }

                var rotated = Path.Combine(_logDir, $"{LogFilePrefix}{DateTime.Now:yyyyMMdd}.log.1");
                File.Move(current, rotated);
                _currentLogFilePath = GenerateLogFilePath();
            }
            catch
            {
            }
        }

        private static string FormatEntry(LogEntry entry)
        {
            var timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            var ex = string.IsNullOrEmpty(entry.Exception) ? "" : $" | {entry.Exception}";
            return $"[{timestamp}] [{entry.Severity}] [{entry.Source}] {entry.Message}{ex}{Environment.NewLine}";
        }

        private static LogEntry ParseEntry(string line)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line))
                    return null;

                var entry = new LogEntry();
                int idx = 0;

                if (line[idx] == '[')
                {
                    var close = line.IndexOf(']', idx);
                    if (close < 0) return null;
                    if (DateTime.TryParse(line.Substring(idx + 1, close - idx - 1), out var ts))
                        entry.Timestamp = ts;
                    idx = close + 1;
                }

                idx = SkipWhitespace(line, idx);

                if (idx < line.Length && line[idx] == '[')
                {
                    var close = line.IndexOf(']', idx + 1);
                    if (close < 0) return null;
                    entry.Severity = line.Substring(idx + 1, close - idx - 1);
                    idx = close + 1;
                }

                idx = SkipWhitespace(line, idx);

                if (idx < line.Length && line[idx] == '[')
                {
                    var close = line.IndexOf(']', idx + 1);
                    if (close < 0) return null;
                    entry.Source = line.Substring(idx + 1, close - idx - 1);
                    idx = close + 1;
                }

                idx = SkipWhitespace(line, idx);

                var pipeIdx = line.IndexOf(" | ", idx);
                if (pipeIdx >= 0)
                {
                    entry.Message = line.Substring(idx, pipeIdx - idx);
                    entry.Exception = line.Substring(pipeIdx + 3);
                }
                else
                {
                    entry.Message = idx < line.Length ? line.Substring(idx) : string.Empty;
                }

                return entry;
            }
            catch
            {
                return null;
            }
        }

        private static int SkipWhitespace(string s, int start)
        {
            while (start < s.Length && char.IsWhiteSpace(s[start]))
                start++;
            return start;
        }
    }
}
