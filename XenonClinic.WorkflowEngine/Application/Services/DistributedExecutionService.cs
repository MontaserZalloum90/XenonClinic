using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// In-memory distributed execution service implementation.
/// For production, use a proper distributed coordination service like Redis, etcd, or Consul.
/// </summary>
public class DistributedExecutionService : IDistributedExecutionService, IDisposable
{
    private readonly DistributedExecutionOptions _options;
    private readonly ILogger<DistributedExecutionService> _logger;

    // In-memory stores - replace with distributed storage in production
    private readonly ConcurrentDictionary<string, ClusterNode> _nodes = new();
    private readonly ConcurrentDictionary<string, DistributedLock> _locks = new();
    private readonly List<EventSubscription> _eventSubscriptions = new();
    private readonly object _subscriptionLock = new();

    private string? _leaderNodeId;
    private readonly Timer _healthCheckTimer;
    private int _roundRobinIndex;

    public DistributedExecutionService(
        IOptions<DistributedExecutionOptions> options,
        ILogger<DistributedExecutionService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Start health check timer
        _healthCheckTimer = new Timer(PerformHealthCheck, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public Task<ClusterNode> RegisterNodeAsync(NodeRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var node = new ClusterNode
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name ?? $"node-{DateTime.UtcNow.Ticks % 10000}",
            HostName = request.HostName,
            IpAddress = request.IpAddress ?? GetLocalIpAddress(),
            Port = request.Port,
            Status = NodeStatus.Active,
            Role = NodeRole.Worker,
            Capabilities = request.Capabilities ?? new NodeCapabilities(),
            Version = request.Version,
            Tags = request.Tags ?? new Dictionary<string, string>(),
            RegisteredAt = DateTime.UtcNow,
            LastHeartbeat = DateTime.UtcNow
        };

        _nodes[node.Id] = node;

        _logger.LogInformation("Node {NodeId} ({NodeName}) registered at {Address}:{Port}",
            node.Id, node.Name, node.IpAddress, node.Port);

        // Publish node joined event
        _ = PublishEventAsync(new NodeJoinedEvent
        {
            SourceNodeId = node.Id,
            Node = node
        }, cancellationToken);

        // Trigger leader election if no leader
        if (string.IsNullOrEmpty(_leaderNodeId))
        {
            _ = TriggerLeaderElectionAsync(cancellationToken);
        }

        return Task.FromResult(node);
    }

    public Task SendHeartbeatAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            node.LastHeartbeat = DateTime.UtcNow;

            // Update status if was unhealthy
            if (node.Status == NodeStatus.Unhealthy)
            {
                var oldStatus = node.Status;
                node.Status = NodeStatus.Active;

                _ = PublishEventAsync(new NodeStatusChangedEvent
                {
                    SourceNodeId = nodeId,
                    NodeId = nodeId,
                    OldStatus = oldStatus,
                    NewStatus = NodeStatus.Active
                }, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    public async Task DeregisterNodeAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        if (_nodes.TryRemove(nodeId, out var node))
        {
            _logger.LogInformation("Node {NodeId} ({NodeName}) deregistered", nodeId, node.Name);

            // Release any locks held by this node
            var nodeLocks = _locks.Values.Where(l => l.OwnerNodeId == nodeId).ToList();
            foreach (var @lock in nodeLocks)
            {
                _locks.TryRemove(@lock.Id, out _);
            }

            // Publish node left event
            await PublishEventAsync(new NodeLeftEvent
            {
                SourceNodeId = nodeId,
                NodeId = nodeId,
                Reason = "Deregistered"
            }, cancellationToken);

            // Trigger leader election if leader left
            if (_leaderNodeId == nodeId)
            {
                _leaderNodeId = null;
                await TriggerLeaderElectionAsync(cancellationToken);
            }
        }
    }

    public Task<ClusterState> GetClusterStateAsync(CancellationToken cancellationToken = default)
    {
        var nodes = _nodes.Values.ToList();
        var activeNodes = nodes.Count(n => n.Status == NodeStatus.Active);

        var state = new ClusterState
        {
            ClusterId = _options.ClusterId,
            Nodes = nodes,
            LeaderNodeId = _leaderNodeId,
            Health = DetermineClusterHealth(nodes),
            LastUpdated = DateTime.UtcNow
        };

        return Task.FromResult(state);
    }

    public Task<ClusterNode?> GetNodeAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        _nodes.TryGetValue(nodeId, out var node);
        return Task.FromResult(node);
    }

    public async Task<DistributedLock?> AcquireLockAsync(AcquireLockRequest request, CancellationToken cancellationToken = default)
    {
        var lockKey = $"{request.ResourceType}:{request.ResourceId}";
        var retryCount = 0;

        while (retryCount <= request.MaxRetries)
        {
            // Check if lock exists and is valid
            var existingLock = _locks.Values.FirstOrDefault(l =>
                l.ResourceType == request.ResourceType &&
                l.ResourceId == request.ResourceId);

            if (existingLock != null)
            {
                if (existingLock.ExpiresAt > DateTime.UtcNow)
                {
                    // Lock is held by someone else
                    if (request.Mode == LockMode.Shared && existingLock.Mode == LockMode.Shared)
                    {
                        // Allow shared lock
                    }
                    else
                    {
                        // Wait and retry
                        if (request.WaitTimeout.HasValue)
                        {
                            retryCount++;
                            await Task.Delay(_options.LockRetryDelay, cancellationToken);
                            continue;
                        }
                        return null;
                    }
                }
                else
                {
                    // Lock expired, remove it
                    _locks.TryRemove(existingLock.Id, out _);
                }
            }

            // Create new lock
            var newLock = new DistributedLock
            {
                Id = Guid.NewGuid().ToString(),
                ResourceId = request.ResourceId,
                ResourceType = request.ResourceType,
                OwnerId = request.OwnerId,
                OwnerNodeId = request.OwnerId.Split(':').FirstOrDefault() ?? request.OwnerId,
                AcquiredAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(request.Duration),
                Mode = request.Mode
            };

            if (_locks.TryAdd(newLock.Id, newLock))
            {
                _logger.LogDebug("Lock acquired: {LockId} for {ResourceType}:{ResourceId} by {OwnerId}",
                    newLock.Id, request.ResourceType, request.ResourceId, request.OwnerId);
                return newLock;
            }

            retryCount++;
            await Task.Delay(_options.LockRetryDelay, cancellationToken);
        }

        _logger.LogWarning("Failed to acquire lock for {ResourceType}:{ResourceId} after {Retries} retries",
            request.ResourceType, request.ResourceId, request.MaxRetries);
        return null;
    }

    public Task ReleaseLockAsync(string lockId, string ownerId, CancellationToken cancellationToken = default)
    {
        if (_locks.TryGetValue(lockId, out var @lock))
        {
            if (@lock.OwnerId == ownerId)
            {
                _locks.TryRemove(lockId, out _);
                _logger.LogDebug("Lock released: {LockId}", lockId);
            }
            else
            {
                _logger.LogWarning("Cannot release lock {LockId}: owner mismatch (expected {Expected}, got {Actual})",
                    lockId, @lock.OwnerId, ownerId);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExtendLockAsync(string lockId, string ownerId, TimeSpan extension, CancellationToken cancellationToken = default)
    {
        if (_locks.TryGetValue(lockId, out var @lock))
        {
            if (@lock.OwnerId == ownerId && @lock.ExpiresAt > DateTime.UtcNow)
            {
                @lock.ExpiresAt = @lock.ExpiresAt.Add(extension);
                @lock.ExtensionCount++;
                _logger.LogDebug("Lock extended: {LockId}, new expiry: {Expiry}", lockId, @lock.ExpiresAt);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public async Task PublishEventAsync(ClusterEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Publishing cluster event: {EventType} from {SourceNode}",
            @event.EventType, @event.SourceNodeId);

        List<EventSubscription> subscriptions;
        lock (_subscriptionLock)
        {
            subscriptions = _eventSubscriptions.ToList();
        }

        foreach (var subscription in subscriptions)
        {
            try
            {
                await subscription.Handler(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling cluster event {EventType}", @event.EventType);
            }
        }
    }

    public IDisposable SubscribeToEvents(Func<ClusterEvent, CancellationToken, Task> handler)
    {
        var subscription = new EventSubscription
        {
            Id = Guid.NewGuid().ToString(),
            Handler = handler
        };

        lock (_subscriptionLock)
        {
            _eventSubscriptions.Add(subscription);
        }

        return new EventSubscriptionDisposable(() =>
        {
            lock (_subscriptionLock)
            {
                _eventSubscriptions.Remove(subscription);
            }
        });
    }

    public async Task<string?> RouteJobAsync(JobRoutingRequest request, CancellationToken cancellationToken = default)
    {
        var availableNodes = _nodes.Values
            .Where(n => n.Status == NodeStatus.Active && n.Capabilities.CanExecuteJobs)
            .ToList();

        if (!availableNodes.Any())
        {
            _logger.LogWarning("No available nodes to route job {JobId}", request.JobId);
            return null;
        }

        // Filter by required tags
        if (request.RequiredTags?.Any() == true)
        {
            availableNodes = availableNodes
                .Where(n => request.RequiredTags.All(t => n.Tags.ContainsKey(t)))
                .ToList();

            if (!availableNodes.Any())
            {
                _logger.LogWarning("No nodes match required tags for job {JobId}", request.JobId);
                return null;
            }
        }

        // Filter by job type support
        if (!string.IsNullOrEmpty(request.JobType))
        {
            availableNodes = availableNodes
                .Where(n => !n.Capabilities.SupportedJobTypes.Any() ||
                            n.Capabilities.SupportedJobTypes.Contains(request.JobType))
                .ToList();
        }

        // Check preferred node
        if (!string.IsNullOrEmpty(request.PreferredNodeId))
        {
            var preferred = availableNodes.FirstOrDefault(n => n.Id == request.PreferredNodeId);
            if (preferred != null && preferred.Metrics.ActiveJobs < preferred.Capabilities.MaxConcurrentJobs)
            {
                return preferred.Id;
            }
        }

        // Route based on strategy
        ClusterNode? selectedNode = request.Strategy switch
        {
            RoutingStrategy.RoundRobin => SelectRoundRobin(availableNodes),
            RoutingStrategy.LeastLoaded => SelectLeastLoaded(availableNodes),
            RoutingStrategy.Random => SelectRandom(availableNodes),
            _ => SelectLeastLoaded(availableNodes)
        };

        if (selectedNode != null)
        {
            _logger.LogDebug("Routed job {JobId} to node {NodeId} using {Strategy} strategy",
                request.JobId, selectedNode.Id, request.Strategy);

            await PublishEventAsync(new WorkDistributedEvent
            {
                SourceNodeId = _leaderNodeId ?? "system",
                JobId = request.JobId,
                TargetNodeId = selectedNode.Id
            }, cancellationToken);

            return selectedNode.Id;
        }

        return null;
    }

    public Task<ClusterNode?> GetLeaderAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_leaderNodeId) && _nodes.TryGetValue(_leaderNodeId, out var leader))
        {
            return Task.FromResult<ClusterNode?>(leader);
        }
        return Task.FromResult<ClusterNode?>(null);
    }

    public async Task<bool> ParticipateInElectionAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        // Simple leader election: first node to claim leadership wins
        // For production, use a proper consensus algorithm (Raft, Paxos) or external coordination

        if (string.IsNullOrEmpty(_leaderNodeId))
        {
            if (_nodes.TryGetValue(nodeId, out var node))
            {
                // Try to become leader
                var currentLeader = Interlocked.CompareExchange(ref _leaderNodeId, nodeId, null);
                if (currentLeader == null)
                {
                    node.Role = NodeRole.Leader;

                    await PublishEventAsync(new LeaderElectedEvent
                    {
                        SourceNodeId = nodeId,
                        LeaderNodeId = nodeId
                    }, cancellationToken);

                    _logger.LogInformation("Node {NodeId} elected as leader", nodeId);
                    return true;
                }
            }
        }

        return _leaderNodeId == nodeId;
    }

    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
    }

    #region Private Methods

    private void PerformHealthCheck(object? state)
    {
        var now = DateTime.UtcNow;
        var timeout = _options.NodeTimeout;

        foreach (var node in _nodes.Values)
        {
            if (node.Status == NodeStatus.Active && (now - node.LastHeartbeat) > timeout)
            {
                var oldStatus = node.Status;
                node.Status = NodeStatus.Unhealthy;

                _logger.LogWarning("Node {NodeId} marked as unhealthy due to heartbeat timeout", node.Id);

                _ = PublishEventAsync(new NodeStatusChangedEvent
                {
                    SourceNodeId = "system",
                    NodeId = node.Id,
                    OldStatus = oldStatus,
                    NewStatus = NodeStatus.Unhealthy
                }, CancellationToken.None);

                // Trigger leader election if leader is unhealthy
                if (_leaderNodeId == node.Id)
                {
                    _leaderNodeId = null;
                    _ = TriggerLeaderElectionAsync(CancellationToken.None);
                }
            }
        }

        // Clean up expired locks
        var expiredLocks = _locks.Values.Where(l => l.ExpiresAt < now).ToList();
        foreach (var @lock in expiredLocks)
        {
            _locks.TryRemove(@lock.Id, out _);
        }
    }

    private async Task TriggerLeaderElectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Triggering leader election");

        // Simple election: node with lowest ID among active nodes becomes leader
        var activeNodes = _nodes.Values
            .Where(n => n.Status == NodeStatus.Active)
            .OrderBy(n => n.Id)
            .ToList();

        if (activeNodes.Any())
        {
            var newLeader = activeNodes.First();
            var previousLeader = _leaderNodeId;

            _leaderNodeId = newLeader.Id;
            newLeader.Role = NodeRole.Leader;

            // Set other nodes as workers
            foreach (var node in activeNodes.Skip(1))
            {
                node.Role = NodeRole.Worker;
            }

            await PublishEventAsync(new LeaderElectedEvent
            {
                SourceNodeId = "system",
                LeaderNodeId = newLeader.Id,
                PreviousLeaderNodeId = previousLeader
            }, cancellationToken);

            _logger.LogInformation("Node {NodeId} elected as new leader", newLeader.Id);
        }
    }

    private ClusterHealth DetermineClusterHealth(List<ClusterNode> nodes)
    {
        if (!nodes.Any())
            return ClusterHealth.Critical;

        var activeNodes = nodes.Count(n => n.Status == NodeStatus.Active);
        var totalNodes = nodes.Count;

        if (activeNodes == 0)
            return ClusterHealth.Critical;

        var healthyRatio = (double)activeNodes / totalNodes;

        return healthyRatio switch
        {
            >= 0.8 => ClusterHealth.Healthy,
            >= 0.5 => ClusterHealth.Degraded,
            >= 0.2 => ClusterHealth.Unhealthy,
            _ => ClusterHealth.Critical
        };
    }

    private ClusterNode? SelectRoundRobin(List<ClusterNode> nodes)
    {
        if (!nodes.Any()) return null;
        var index = Interlocked.Increment(ref _roundRobinIndex) % nodes.Count;
        return nodes[index];
    }

    private static ClusterNode? SelectLeastLoaded(List<ClusterNode> nodes)
    {
        return nodes
            .Where(n => n.Metrics.ActiveJobs < n.Capabilities.MaxConcurrentJobs)
            .OrderBy(n => n.Metrics.ActiveJobs)
            .ThenBy(n => n.Metrics.CpuUsagePercent)
            .FirstOrDefault();
    }

    private static ClusterNode? SelectRandom(List<ClusterNode> nodes)
    {
        if (!nodes.Any()) return null;
        var random = new Random();
        return nodes[random.Next(nodes.Count)];
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    #endregion

    private class EventSubscription
    {
        public string Id { get; set; } = string.Empty;
        public Func<ClusterEvent, CancellationToken, Task> Handler { get; set; } = null!;
    }

    private class EventSubscriptionDisposable : IDisposable
    {
        private readonly Action _unsubscribe;

        public EventSubscriptionDisposable(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose() => _unsubscribe();
    }
}
