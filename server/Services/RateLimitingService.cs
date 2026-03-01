using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace EmailApi.Services;

public interface IRateLimitingService
{
    bool IsAllowed(string email, out string lastReceivedAt);
    void RecordRequest(string email);
}

public class RateLimitingService : IRateLimitingService
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(3);
    private static readonly object _lock = new object();
    private readonly ILogger<RateLimitingService> _logger;
    
    private static readonly ConcurrentDictionary<string, (DateTime time, string originalEmail)> LastRequests = 
        new ConcurrentDictionary<string, (DateTime time, string originalEmail)>();

    public RateLimitingService(ILogger<RateLimitingService> logger)
    {
        _logger = logger;
    }

    public bool IsAllowed(string email, out string lastReceivedAt)
    {
        var now = DateTime.UtcNow;
        var key = email.ToLowerInvariant();

        lastReceivedAt = string.Empty;

        lock (_lock)
        {
            _logger.LogInformation($"[RateLimit] Checking email: {key} at {now:yyyy-MM-dd HH:mm:ss.fff}");
            
            if (LastRequests.TryGetValue(key, out var entry))
            {
                var timeSinceLastRequest = now - entry.time;
                _logger.LogInformation($"[RateLimit] Last request at {entry.time:yyyy-MM-dd HH:mm:ss.fff}, time since: {timeSinceLastRequest.TotalMilliseconds}ms");
                
                if (timeSinceLastRequest < Window)
                {
                    lastReceivedAt = entry.time.ToString("o");
                    _logger.LogInformation($"[RateLimit] BLOCKED - within {Window.TotalSeconds}s window");
                    return false;
                }
            }
            
            // Request is allowed - record it immediately while holding the lock
            LastRequests[key] = (now, email);
            _logger.LogInformation($"[RateLimit] ALLOWED - recorded at {now:yyyy-MM-dd HH:mm:ss.fff}");
            return true;
        }
    }

    public void RecordRequest(string email)
    {
        // This method is now unused since recording happens in IsAllowed
        // but keeping it for interface compatibility
        var key = email.ToLowerInvariant();
        
        lock (_lock)
        {
            LastRequests[key] = (DateTime.UtcNow, email);
        }
    }
}
