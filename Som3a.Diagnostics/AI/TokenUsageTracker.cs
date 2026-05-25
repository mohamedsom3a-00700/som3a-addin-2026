namespace Som3a.Diagnostics.AI
{
    public class TokenUsageTracker
    {
        private readonly Dictionary<string, SessionUsage> _sessions = new();

        public void TrackUsage(string sessionId, string providerId, int promptTokens, int completionTokens)
        {
            if (!_sessions.ContainsKey(sessionId))
                _sessions[sessionId] = new SessionUsage { SessionId = sessionId };

            var session = _sessions[sessionId];

            if (!session.Providers.TryGetValue(providerId, out var provider))
            {
                provider = new ProviderUsage { ProviderId = providerId };
                session.Providers[providerId] = provider;
            }

            provider.PromptTokens += promptTokens;
            provider.CompletionTokens += completionTokens;
            provider.RequestCount++;
            session.TotalTokens += promptTokens + completionTokens;
        }

        public SessionUsage? GetSession(string sessionId)
        {
            return _sessions.TryGetValue(sessionId, out var session) ? session : null;
        }

        public Dictionary<string, SessionUsage> GetAllSessions() => _sessions;
    }

    public class SessionUsage
    {
        public string SessionId { get; set; } = string.Empty;
        public int TotalTokens { get; set; }
        public Dictionary<string, ProviderUsage> Providers { get; set; } = new();
    }

    public class ProviderUsage
    {
        public string ProviderId { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int RequestCount { get; set; }
    }
}
