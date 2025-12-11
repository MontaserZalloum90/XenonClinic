using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for distributed workflow execution across multiple nodes.
/// </summary>
public interface IDistributedExecutionService
{
    /// <summary>
    /// Registers this node in the cluster.
    /// </summary>
    Task<ClusterNode> RegisterNodeAsync(NodeRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a heartbeat to indicate node is alive.
    /// </summary>
    Task SendHeartbeatAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deregisters a node from the cluster.
    /// </summary>
    Task DeregisterNodeAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current cluster state.
    /// </summary>
    Task<ClusterState> GetClusterStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific node's information.
    /// </summary>
    Task<ClusterNode?> GetNodeAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a distributed lock.
    /// </summary>
    Task<DistributedLock?> AcquireLockAsync(AcquireLockRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a distributed lock.
    /// </summary>
    Task ReleaseLockAsync(string lockId, string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends a distributed lock's duration.
    /// </summary>
    Task<bool> ExtendLockAsync(string lockId, string ownerId, TimeSpan extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event to all nodes.
    /// </summary>
    Task PublishEventAsync(ClusterEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to cluster events.
    /// </summary>
    IDisposable SubscribeToEvents(Func<ClusterEvent, CancellationToken, Task> handler);

    /// <summary>
    /// Routes a job to an appropriate node.
    /// </summary>
    Task<string?> RouteJobAsync(JobRoutingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current leader node.
    /// </summary>
    Task<ClusterNode?> GetLeaderAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Participates in leader election.
    /// </summary>
    Task<bool> ParticipateInElectionAsync(string nodeId, CancellationToken cancellationToken = default);
}

#region Cluster DTOs

public class ClusterNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.Starting;
    public NodeRole Role { get; set; } = NodeRole.Worker;
    public NodeCapabilities Capabilities { get; set; } = new();
    public NodeMetrics Metrics { get; set; } = new();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public string? Version { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public enum NodeStatus
{
    Starting,
    Active,
    Draining,    // Not accepting new work
    Unhealthy,
    Offline
}

public enum NodeRole
{
    Worker,
    Leader,
    Observer
}

public class NodeCapabilities
{
    public bool CanExecuteJobs { get; set; } = true;
    public bool CanProcessTimers { get; set; } = true;
    public bool CanSendEmails { get; set; } = true;
    public bool CanExecuteWebhooks { get; set; } = true;
    public bool CanGenerateDocuments { get; set; } = true;
    public int MaxConcurrentJobs { get; set; } = 10;
    public int MaxConcurrentTimers { get; set; } = 100;
    public List<string> SupportedJobTypes { get; set; } = new();
}

public class NodeMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public int ActiveJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int CompletedJobsLast5Minutes { get; set; }
    public int FailedJobsLast5Minutes { get; set; }
    public double AverageJobDurationMs { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class NodeRegistrationRequest
{
    public string? Name { get; set; }
    public string HostName { get; set; } = Environment.MachineName;
    public string? IpAddress { get; set; }
    public int Port { get; set; }
    public NodeCapabilities? Capabilities { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

#endregion

#region Cluster State DTOs

public class ClusterState
{
    public string ClusterId { get; set; } = string.Empty;
    public List<ClusterNode> Nodes { get; set; } = new();
    public string? LeaderNodeId { get; set; }
    public ClusterHealth Health { get; set; } = ClusterHealth.Healthy;
    public int ActiveNodeCount => Nodes.Count(n => n.Status == NodeStatus.Active);
    public int TotalCapacity => Nodes.Where(n => n.Status == NodeStatus.Active).Sum(n => n.Capabilities.MaxConcurrentJobs);
    public int CurrentLoad => Nodes.Where(n => n.Status == NodeStatus.Active).Sum(n => n.Metrics.ActiveJobs);
    public double LoadPercent => TotalCapacity > 0 ? (CurrentLoad * 100.0 / TotalCapacity) : 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public enum ClusterHealth
{
    Healthy,
    Degraded,
    Unhealthy,
    Critical
}

#endregion

#region Distributed Lock DTOs

public class DistributedLock
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerNodeId { get; set; } = string.Empty;
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public int ExtensionCount { get; set; }
    public LockMode Mode { get; set; } = LockMode.Exclusive;
}

public enum LockMode
{
    Exclusive,
    Shared
}

public class AcquireLockRequest
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    public LockMode Mode { get; set; } = LockMode.Exclusive;
    public TimeSpan? WaitTimeout { get; set; }
    public int MaxRetries { get; set; } = 3;
}

#endregion

#region Cluster Events

public abstract class ClusterEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string SourceNodeId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public class NodeJoinedEvent : ClusterEvent
{
    public override string EventType => "node.joined";
    public ClusterNode Node { get; set; } = null!;
}

public class NodeLeftEvent : ClusterEvent
{
    public override string EventType => "node.left";
    public string NodeId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class NodeStatusChangedEvent : ClusterEvent
{
    public override string EventType => "node.status_changed";
    public string NodeId { get; set; } = string.Empty;
    public NodeStatus OldStatus { get; set; }
    public NodeStatus NewStatus { get; set; }
}

public class LeaderElectedEvent : ClusterEvent
{
    public override string EventType => "leader.elected";
    public string LeaderNodeId { get; set; } = string.Empty;
    public string? PreviousLeaderNodeId { get; set; }
}

public class WorkDistributedEvent : ClusterEvent
{
    public override string EventType => "work.distributed";
    public string JobId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
}

public class CacheInvalidationEvent : ClusterEvent
{
    public override string EventType => "cache.invalidation";
    public string CacheKey { get; set; } = string.Empty;
    public string? Pattern { get; set; }
}

#endregion

#region Job Routing DTOs

public class JobRoutingRequest
{
    public string JobId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public int Priority { get; set; } = 50;
    public string? PreferredNodeId { get; set; }
    public List<string>? RequiredTags { get; set; }
    public RoutingStrategy Strategy { get; set; } = RoutingStrategy.LeastLoaded;
    public Dictionary<string, object> JobData { get; set; } = new();
}

public enum RoutingStrategy
{
    RoundRobin,
    LeastLoaded,
    Random,
    Affinity,       // Route to same node as related work
    Broadcast       // Send to all nodes
}

#endregion

#region Configuration

public class DistributedExecutionOptions
{
    public string ClusterId { get; set; } = "default";
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan NodeTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan LeaderElectionTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public int MaxLockRetries { get; set; } = 5;
    public TimeSpan LockRetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public bool EnableAutoRecovery { get; set; } = true;
    public int MaxRecoveryAttempts { get; set; } = 3;
}

#endregion
