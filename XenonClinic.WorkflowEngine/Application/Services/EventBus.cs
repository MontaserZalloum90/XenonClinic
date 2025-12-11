using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// In-memory event bus implementation with support for distributed event publishing.
/// </summary>
public class EventBus : IEventBus
{
    private readonly ILogger<EventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, List<SubscriptionInfo>> _typeSubscriptions = new();
    private readonly ConcurrentDictionary<string, List<TopicSubscriptionInfo>> _topicSubscriptions = new();
    private readonly List<PatternSubscriptionInfo> _patternSubscriptions = new();
    private readonly object _patternLock = new();
    private readonly List<IEventBusInterceptor> _interceptors = new();

    public EventBus(ILogger<EventBus> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void AddInterceptor(IEventBusInterceptor interceptor)
    {
        _interceptors.Add(interceptor);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : WorkflowEvent
    {
        var topic = @event.EventType;
        await PublishAsync(topic, @event, cancellationToken);
    }

    public async Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : WorkflowEvent
    {
        _logger.LogDebug("Publishing event {EventType} with ID {EventId} to topic {Topic}",
            @event.EventType, @event.EventId, topic);

        // Run interceptors
        foreach (var interceptor in _interceptors)
        {
            try
            {
                await interceptor.OnPublishAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event interceptor failed for event {EventId}", @event.EventId);
            }
        }

        var tasks = new List<Task>();

        // Notify type-based subscribers
        if (_typeSubscriptions.TryGetValue(typeof(TEvent), out var typeHandlers))
        {
            foreach (var handler in typeHandlers.ToList())
            {
                tasks.Add(InvokeHandlerAsync(handler, @event, cancellationToken));
            }
        }

        // Notify topic-based subscribers
        if (_topicSubscriptions.TryGetValue(topic, out var topicHandlers))
        {
            foreach (var handler in topicHandlers.ToList())
            {
                tasks.Add(InvokeTopicHandlerAsync(handler, @event, cancellationToken));
            }
        }

        // Notify pattern-based subscribers
        List<PatternSubscriptionInfo> matchingPatterns;
        lock (_patternLock)
        {
            matchingPatterns = _patternSubscriptions
                .Where(p => p.Pattern.IsMatch(topic))
                .ToList();
        }

        foreach (var pattern in matchingPatterns)
        {
            tasks.Add(InvokePatternHandlerAsync(pattern, @event, cancellationToken));
        }

        // Also publish to external handlers if configured
        tasks.Add(PublishToExternalHandlersAsync(@event, cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogDebug("Event {EventId} published to {HandlerCount} handlers",
            @event.EventId, tasks.Count);
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : WorkflowEvent
    {
        var subscriptionInfo = new SubscriptionInfo
        {
            Id = Guid.NewGuid().ToString(),
            Handler = async (e, ct) => await handler((TEvent)e, ct)
        };

        var handlers = _typeSubscriptions.GetOrAdd(typeof(TEvent), _ => new List<SubscriptionInfo>());
        lock (handlers)
        {
            handlers.Add(subscriptionInfo);
        }

        _logger.LogDebug("Subscription {SubscriptionId} added for event type {EventType}",
            subscriptionInfo.Id, typeof(TEvent).Name);

        return new EventSubscription(() =>
        {
            lock (handlers)
            {
                handlers.Remove(subscriptionInfo);
            }
            _logger.LogDebug("Subscription {SubscriptionId} removed", subscriptionInfo.Id);
        });
    }

    public IDisposable Subscribe<TEvent>(string topic, Func<TEvent, CancellationToken, Task> handler)
        where TEvent : WorkflowEvent
    {
        var subscriptionInfo = new TopicSubscriptionInfo
        {
            Id = Guid.NewGuid().ToString(),
            Topic = topic,
            Handler = async (e, ct) => await handler((TEvent)e, ct)
        };

        var handlers = _topicSubscriptions.GetOrAdd(topic, _ => new List<TopicSubscriptionInfo>());
        lock (handlers)
        {
            handlers.Add(subscriptionInfo);
        }

        _logger.LogDebug("Subscription {SubscriptionId} added for topic {Topic}",
            subscriptionInfo.Id, topic);

        return new EventSubscription(() =>
        {
            lock (handlers)
            {
                handlers.Remove(subscriptionInfo);
            }
            _logger.LogDebug("Subscription {SubscriptionId} removed from topic {Topic}",
                subscriptionInfo.Id, topic);
        });
    }

    public IDisposable SubscribePattern(string pattern, Func<WorkflowEvent, CancellationToken, Task> handler)
    {
        var regex = ConvertPatternToRegex(pattern);
        var subscriptionInfo = new PatternSubscriptionInfo
        {
            Id = Guid.NewGuid().ToString(),
            PatternString = pattern,
            Pattern = regex,
            Handler = handler
        };

        lock (_patternLock)
        {
            _patternSubscriptions.Add(subscriptionInfo);
        }

        _logger.LogDebug("Pattern subscription {SubscriptionId} added for pattern {Pattern}",
            subscriptionInfo.Id, pattern);

        return new EventSubscription(() =>
        {
            lock (_patternLock)
            {
                _patternSubscriptions.Remove(subscriptionInfo);
            }
            _logger.LogDebug("Pattern subscription {SubscriptionId} removed", subscriptionInfo.Id);
        });
    }

    private async Task InvokeHandlerAsync(SubscriptionInfo subscription, WorkflowEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await subscription.Handler(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler {SubscriptionId} failed for event {EventId}",
                subscription.Id, @event.EventId);
        }
    }

    private async Task InvokeTopicHandlerAsync(TopicSubscriptionInfo subscription, WorkflowEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await subscription.Handler(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Topic handler {SubscriptionId} failed for event {EventId}",
                subscription.Id, @event.EventId);
        }
    }

    private async Task InvokePatternHandlerAsync(PatternSubscriptionInfo subscription, WorkflowEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await subscription.Handler(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pattern handler {SubscriptionId} failed for event {EventId}",
                subscription.Id, @event.EventId);
        }
    }

    private async Task PublishToExternalHandlersAsync(WorkflowEvent @event, CancellationToken cancellationToken)
    {
        // Resolve external event handlers from DI
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IExternalEventHandler>();

        foreach (var handler in handlers)
        {
            try
            {
                if (await handler.CanHandleAsync(@event, cancellationToken))
                {
                    await handler.HandleAsync(@event, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External handler {HandlerType} failed for event {EventId}",
                    handler.GetType().Name, @event.EventId);
            }
        }
    }

    private static Regex ConvertPatternToRegex(string pattern)
    {
        // Convert glob-style patterns to regex
        // * matches any characters except .
        // ** matches any characters including .
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^.]*") + "$";

        return new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    private class SubscriptionInfo
    {
        public string Id { get; set; } = string.Empty;
        public Func<WorkflowEvent, CancellationToken, Task> Handler { get; set; } = null!;
    }

    private class TopicSubscriptionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public Func<WorkflowEvent, CancellationToken, Task> Handler { get; set; } = null!;
    }

    private class PatternSubscriptionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string PatternString { get; set; } = string.Empty;
        public Regex Pattern { get; set; } = null!;
        public Func<WorkflowEvent, CancellationToken, Task> Handler { get; set; } = null!;
    }
}

/// <summary>
/// Interface for event bus interceptors.
/// </summary>
public interface IEventBusInterceptor
{
    Task OnPublishAsync(WorkflowEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Interface for external event handlers that can be registered via DI.
/// </summary>
public interface IExternalEventHandler
{
    Task<bool> CanHandleAsync(WorkflowEvent @event, CancellationToken cancellationToken);
    Task HandleAsync(WorkflowEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Event bus interceptor that persists events for audit.
/// </summary>
public class AuditEventInterceptor : IEventBusInterceptor
{
    private readonly ILogger<AuditEventInterceptor> _logger;

    public AuditEventInterceptor(ILogger<AuditEventInterceptor> logger)
    {
        _logger = logger;
    }

    public Task OnPublishAsync(WorkflowEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Event {EventType} ({EventId}) at {Timestamp}",
            @event.EventType, @event.EventId, @event.Timestamp);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Event bus interceptor that forwards events to webhooks.
/// </summary>
public class WebhookForwardingInterceptor : IEventBusInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookForwardingInterceptor> _logger;

    public WebhookForwardingInterceptor(IServiceProvider serviceProvider, ILogger<WebhookForwardingInterceptor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task OnPublishAsync(WorkflowEvent @event, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var webhookService = scope.ServiceProvider.GetService<IWebhookService>();

        if (webhookService != null)
        {
            try
            {
                await webhookService.NotifyWebhooksAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to forward event {EventId} to webhooks", @event.EventId);
            }
        }
    }
}
