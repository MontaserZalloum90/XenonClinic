using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Xenon.Platform.Infrastructure.Services;

/// <summary>
/// Service for temporarily blocking IP addresses that exhibit suspicious behavior.
/// Uses in-memory caching for fast lookups with configurable block duration.
/// </summary>
public interface IIpBlockingService
{
    bool IsBlocked(string ipAddress);
    void BlockIp(string ipAddress, TimeSpan? duration = null, string? reason = null);
    void UnblockIp(string ipAddress);
    void RecordFailedAttempt(string ipAddress);
    int GetFailedAttemptCount(string ipAddress);
    IEnumerable<BlockedIpInfo> GetBlockedIps();
}

public class IpBlockingService : IIpBlockingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IpBlockingService> _logger;
    private readonly TimeSpan _defaultBlockDuration;
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _failedAttemptWindow;

    private const string BlockedIpPrefix = "blocked_ip_";
    private const string FailedAttemptsPrefix = "failed_attempts_";
    private const string BlockedIpsListKey = "blocked_ips_list";

    public IpBlockingService(
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<IpBlockingService> logger)
    {
        _cache = cache;
        _logger = logger;

        var blockDurationMinutes = int.Parse(configuration["Security:IpBlocking:DefaultBlockDurationMinutes"] ?? "30");
        _defaultBlockDuration = TimeSpan.FromMinutes(blockDurationMinutes);

        _maxFailedAttempts = int.Parse(configuration["Security:IpBlocking:MaxFailedAttempts"] ?? "10");

        var attemptWindowMinutes = int.Parse(configuration["Security:IpBlocking:FailedAttemptWindowMinutes"] ?? "15");
        _failedAttemptWindow = TimeSpan.FromMinutes(attemptWindowMinutes);
    }

    public bool IsBlocked(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return false;

        var key = BlockedIpPrefix + ipAddress;
        return _cache.TryGetValue<BlockedIpInfo>(key, out _);
    }

    public void BlockIp(string ipAddress, TimeSpan? duration = null, string? reason = null)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return;

        var blockDuration = duration ?? _defaultBlockDuration;
        var key = BlockedIpPrefix + ipAddress;

        var blockedInfo = new BlockedIpInfo
        {
            IpAddress = ipAddress,
            BlockedAt = DateTime.UtcNow,
            BlockedUntil = DateTime.UtcNow.Add(blockDuration),
            Reason = reason ?? "Suspicious activity"
        };

        _cache.Set(key, blockedInfo, blockDuration);

        // Track blocked IPs for listing
        UpdateBlockedIpsList(ipAddress, add: true);

        _logger.LogWarning(
            "IP address {IpAddress} blocked until {BlockedUntil}. Reason: {Reason}",
            ipAddress, blockedInfo.BlockedUntil, reason);
    }

    public void UnblockIp(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return;

        var key = BlockedIpPrefix + ipAddress;
        _cache.Remove(key);

        // Clear failed attempts
        var attemptsKey = FailedAttemptsPrefix + ipAddress;
        _cache.Remove(attemptsKey);

        // Remove from list
        UpdateBlockedIpsList(ipAddress, add: false);

        _logger.LogInformation("IP address {IpAddress} unblocked", ipAddress);
    }

    public void RecordFailedAttempt(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return;

        var key = FailedAttemptsPrefix + ipAddress;

        var currentCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _failedAttemptWindow;
            return 0;
        });

        var newCount = currentCount + 1;
        _cache.Set(key, newCount, _failedAttemptWindow);

        if (newCount >= _maxFailedAttempts)
        {
            BlockIp(ipAddress, reason: $"Exceeded {_maxFailedAttempts} failed attempts");
        }
    }

    public int GetFailedAttemptCount(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return 0;

        var key = FailedAttemptsPrefix + ipAddress;
        return _cache.TryGetValue<int>(key, out var count) ? count : 0;
    }

    public IEnumerable<BlockedIpInfo> GetBlockedIps()
    {
        var list = _cache.GetOrCreate<HashSet<string>>(BlockedIpsListKey, _ => new HashSet<string>());
        var result = new List<BlockedIpInfo>();

        foreach (var ip in list.ToList())
        {
            var key = BlockedIpPrefix + ip;
            if (_cache.TryGetValue<BlockedIpInfo>(key, out var info))
            {
                result.Add(info);
            }
            else
            {
                // Clean up expired entries
                list.Remove(ip);
            }
        }

        return result;
    }

    private void UpdateBlockedIpsList(string ipAddress, bool add)
    {
        var list = _cache.GetOrCreate<HashSet<string>>(BlockedIpsListKey, entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;
            return new HashSet<string>();
        });

        lock (list)
        {
            if (add)
            {
                list.Add(ipAddress);
            }
            else
            {
                list.Remove(ipAddress);
            }
        }
    }
}

public class BlockedIpInfo
{
    public string IpAddress { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; }
    public DateTime BlockedUntil { get; set; }
    public string? Reason { get; set; }
    public TimeSpan RemainingTime => BlockedUntil > DateTime.UtcNow
        ? BlockedUntil - DateTime.UtcNow
        : TimeSpan.Zero;
}
