using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;
using XenonClinic.WorkflowEngine.Extensions;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;

namespace XenonClinic.WorkflowEngine.Tests.Testing;

/// <summary>
/// Test fixture for workflow engine testing.
/// Provides a pre-configured test environment with in-memory stores.
/// </summary>
public class WorkflowTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly List<ProcessDefinition> _deployedDefinitions = new();
    private readonly List<ProcessInstance> _createdInstances = new();

    public IServiceProvider Services => _serviceProvider;
    public IProcessDefinitionService ProcessDefinitionService => _serviceProvider.GetRequiredService<IProcessDefinitionService>();
    public IProcessExecutionService ProcessExecutionService => _serviceProvider.GetRequiredService<IProcessExecutionService>();
    public IHumanTaskService HumanTaskService => _serviceProvider.GetRequiredService<IHumanTaskService>();
    public ITimerService TimerService => _serviceProvider.GetRequiredService<ITimerService>();
    public IBusinessRulesEngine RulesEngine => _serviceProvider.GetRequiredService<IBusinessRulesEngine>();
    public IEventBus EventBus => _serviceProvider.GetRequiredService<IEventBus>();

    public WorkflowTestFixture() : this(null) { }

    public WorkflowTestFixture(Action<IServiceCollection>? configureServices)
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance));

        // Add workflow engine with in-memory stores
        services.AddWorkflowEngine(builder =>
        {
            builder.UseInMemoryStores();
        });

        // Add test-specific mocks
        services.AddSingleton<TestEventCollector>();
        services.AddSingleton<MockEmailService>();
        services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<MockEmailService>());

        // Allow custom configuration
        configureServices?.Invoke(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates and deploys a simple sequential process for testing.
    /// </summary>
    public async Task<ProcessDefinition> CreateSimpleProcessAsync(
        string key = "test-process",
        string name = "Test Process",
        CancellationToken cancellationToken = default)
    {
        var request = new CreateProcessDefinitionRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = name,
            Description = "A simple test process",
            Category = "Test",
            ProcessModel = new ProcessModel
            {
                Id = key,
                Name = name,
                Elements = new List<ProcessElement>
                {
                    new ProcessElement
                    {
                        Id = "start",
                        Type = "startEvent",
                        Name = "Start"
                    },
                    new ProcessElement
                    {
                        Id = "task1",
                        Type = "userTask",
                        Name = "First Task",
                        Properties = new Dictionary<string, object>
                        {
                            ["assignee"] = "testuser"
                        }
                    },
                    new ProcessElement
                    {
                        Id = "end",
                        Type = "endEvent",
                        Name = "End"
                    }
                },
                Flows = new List<SequenceFlow>
                {
                    new SequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "task1" },
                    new SequenceFlow { Id = "flow2", SourceRef = "task1", TargetRef = "end" }
                }
            }
        };

        var definition = await ProcessDefinitionService.CreateAsync(request, cancellationToken);
        var deployed = await ProcessDefinitionService.DeployAsync(definition.Id, cancellationToken);

        _deployedDefinitions.Add(deployed);
        return deployed;
    }

    /// <summary>
    /// Creates and deploys a process with exclusive gateway for decision testing.
    /// </summary>
    public async Task<ProcessDefinition> CreateDecisionProcessAsync(
        string key = "decision-process",
        CancellationToken cancellationToken = default)
    {
        var request = new CreateProcessDefinitionRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = "Decision Process",
            Description = "Process with exclusive gateway",
            Category = "Test",
            ProcessModel = new ProcessModel
            {
                Id = key,
                Name = "Decision Process",
                Elements = new List<ProcessElement>
                {
                    new ProcessElement { Id = "start", Type = "startEvent", Name = "Start" },
                    new ProcessElement
                    {
                        Id = "gateway",
                        Type = "exclusiveGateway",
                        Name = "Decision"
                    },
                    new ProcessElement { Id = "approveTask", Type = "userTask", Name = "Approve Task" },
                    new ProcessElement { Id = "rejectTask", Type = "userTask", Name = "Reject Task" },
                    new ProcessElement { Id = "end", Type = "endEvent", Name = "End" }
                },
                Flows = new List<SequenceFlow>
                {
                    new SequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "gateway" },
                    new SequenceFlow
                    {
                        Id = "flow2",
                        SourceRef = "gateway",
                        TargetRef = "approveTask",
                        ConditionExpression = "approved == true"
                    },
                    new SequenceFlow
                    {
                        Id = "flow3",
                        SourceRef = "gateway",
                        TargetRef = "rejectTask",
                        ConditionExpression = "approved == false"
                    },
                    new SequenceFlow { Id = "flow4", SourceRef = "approveTask", TargetRef = "end" },
                    new SequenceFlow { Id = "flow5", SourceRef = "rejectTask", TargetRef = "end" }
                }
            }
        };

        var definition = await ProcessDefinitionService.CreateAsync(request, cancellationToken);
        var deployed = await ProcessDefinitionService.DeployAsync(definition.Id, cancellationToken);

        _deployedDefinitions.Add(deployed);
        return deployed;
    }

    /// <summary>
    /// Creates and deploys a process with parallel gateway.
    /// </summary>
    public async Task<ProcessDefinition> CreateParallelProcessAsync(
        string key = "parallel-process",
        CancellationToken cancellationToken = default)
    {
        var request = new CreateProcessDefinitionRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = "Parallel Process",
            Description = "Process with parallel gateway",
            Category = "Test",
            ProcessModel = new ProcessModel
            {
                Id = key,
                Name = "Parallel Process",
                Elements = new List<ProcessElement>
                {
                    new ProcessElement { Id = "start", Type = "startEvent", Name = "Start" },
                    new ProcessElement { Id = "split", Type = "parallelGateway", Name = "Split" },
                    new ProcessElement { Id = "task1", Type = "userTask", Name = "Task 1" },
                    new ProcessElement { Id = "task2", Type = "userTask", Name = "Task 2" },
                    new ProcessElement { Id = "join", Type = "parallelGateway", Name = "Join" },
                    new ProcessElement { Id = "end", Type = "endEvent", Name = "End" }
                },
                Flows = new List<SequenceFlow>
                {
                    new SequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "split" },
                    new SequenceFlow { Id = "flow2", SourceRef = "split", TargetRef = "task1" },
                    new SequenceFlow { Id = "flow3", SourceRef = "split", TargetRef = "task2" },
                    new SequenceFlow { Id = "flow4", SourceRef = "task1", TargetRef = "join" },
                    new SequenceFlow { Id = "flow5", SourceRef = "task2", TargetRef = "join" },
                    new SequenceFlow { Id = "flow6", SourceRef = "join", TargetRef = "end" }
                }
            }
        };

        var definition = await ProcessDefinitionService.CreateAsync(request, cancellationToken);
        var deployed = await ProcessDefinitionService.DeployAsync(definition.Id, cancellationToken);

        _deployedDefinitions.Add(deployed);
        return deployed;
    }

    /// <summary>
    /// Creates and deploys a process with timer events.
    /// </summary>
    public async Task<ProcessDefinition> CreateTimerProcessAsync(
        string key = "timer-process",
        CancellationToken cancellationToken = default)
    {
        var request = new CreateProcessDefinitionRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = "Timer Process",
            Description = "Process with timer events",
            Category = "Test",
            ProcessModel = new ProcessModel
            {
                Id = key,
                Name = "Timer Process",
                Elements = new List<ProcessElement>
                {
                    new ProcessElement { Id = "start", Type = "startEvent", Name = "Start" },
                    new ProcessElement
                    {
                        Id = "task1",
                        Type = "userTask",
                        Name = "Task with Timer",
                        BoundaryEvents = new List<BoundaryEventDefinition>
                        {
                            new BoundaryEventDefinition
                            {
                                Id = "timer1",
                                Type = "timer",
                                TimerType = TimerType.Duration,
                                TimerValue = "PT1H", // 1 hour
                                CancelActivity = true
                            }
                        }
                    },
                    new ProcessElement { Id = "timeout", Type = "endEvent", Name = "Timeout" },
                    new ProcessElement { Id = "end", Type = "endEvent", Name = "Complete" }
                },
                Flows = new List<SequenceFlow>
                {
                    new SequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "task1" },
                    new SequenceFlow { Id = "flow2", SourceRef = "task1", TargetRef = "end" },
                    new SequenceFlow { Id = "flow3", SourceRef = "timer1", TargetRef = "timeout" }
                }
            }
        };

        var definition = await ProcessDefinitionService.CreateAsync(request, cancellationToken);
        var deployed = await ProcessDefinitionService.DeployAsync(definition.Id, cancellationToken);

        _deployedDefinitions.Add(deployed);
        return deployed;
    }

    /// <summary>
    /// Starts a process instance.
    /// </summary>
    public async Task<ProcessInstance> StartProcessAsync(
        string processDefinitionKey,
        Dictionary<string, object>? variables = null,
        string? businessKey = null,
        CancellationToken cancellationToken = default)
    {
        var request = new StartProcessRequest
        {
            ProcessDefinitionKey = processDefinitionKey,
            Variables = variables ?? new Dictionary<string, object>(),
            BusinessKey = businessKey,
            InitiatorUserId = "test-user"
        };

        var instance = await ProcessExecutionService.StartProcessAsync(request, cancellationToken);
        _createdInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Gets active tasks for a process instance.
    /// </summary>
    public async Task<IList<HumanTask>> GetActiveTasksAsync(
        string processInstanceId,
        CancellationToken cancellationToken = default)
    {
        var query = new TaskQuery
        {
            ProcessInstanceId = processInstanceId,
            Status = TaskStatus.Active
        };

        var result = await HumanTaskService.QueryTasksAsync(query, cancellationToken);
        return result.Tasks;
    }

    /// <summary>
    /// Completes a task.
    /// </summary>
    public async Task CompleteTaskAsync(
        string taskId,
        Dictionary<string, object>? variables = null,
        CancellationToken cancellationToken = default)
    {
        var request = new CompleteTaskRequest
        {
            TaskId = taskId,
            Variables = variables ?? new Dictionary<string, object>(),
            UserId = "test-user"
        };

        await HumanTaskService.CompleteTaskAsync(request, cancellationToken);
    }

    /// <summary>
    /// Waits for the process to reach a specific state.
    /// </summary>
    public async Task<ProcessInstance> WaitForProcessStateAsync(
        string processInstanceId,
        ProcessInstanceState expectedState,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            var instance = await ProcessExecutionService.GetInstanceAsync(processInstanceId, cancellationToken);
            if (instance?.State == expectedState)
            {
                return instance;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Process did not reach state {expectedState} within {timeout}");
    }

    /// <summary>
    /// Waits for a task to be created.
    /// </summary>
    public async Task<HumanTask> WaitForTaskAsync(
        string processInstanceId,
        string? taskDefinitionKey = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            var tasks = await GetActiveTasksAsync(processInstanceId, cancellationToken);
            var task = taskDefinitionKey != null
                ? tasks.FirstOrDefault(t => t.TaskDefinitionKey == taskDefinitionKey)
                : tasks.FirstOrDefault();

            if (task != null)
            {
                return task;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Task was not created within {timeout}");
    }

    /// <summary>
    /// Cleans up all created resources.
    /// </summary>
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

/// <summary>
/// Collects events published to the event bus for test assertions.
/// </summary>
public class TestEventCollector
{
    private readonly List<object> _events = new();
    private readonly object _lock = new();

    public IReadOnlyList<object> Events
    {
        get
        {
            lock (_lock)
            {
                return _events.ToList();
            }
        }
    }

    public void AddEvent(object @event)
    {
        lock (_lock)
        {
            _events.Add(@event);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    public IEnumerable<T> GetEvents<T>() where T : class
    {
        lock (_lock)
        {
            return _events.OfType<T>().ToList();
        }
    }

    public bool HasEvent<T>() where T : class
    {
        lock (_lock)
        {
            return _events.OfType<T>().Any();
        }
    }

    public bool HasEvent<T>(Func<T, bool> predicate) where T : class
    {
        lock (_lock)
        {
            return _events.OfType<T>().Any(predicate);
        }
    }

    public T? GetLastEvent<T>() where T : class
    {
        lock (_lock)
        {
            return _events.OfType<T>().LastOrDefault();
        }
    }
}

/// <summary>
/// Mock email service for testing.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly List<EmailMessage> _sentEmails = new();
    private readonly object _lock = new();

    public IReadOnlyList<EmailMessage> SentEmails
    {
        get
        {
            lock (_lock)
            {
                return _sentEmails.ToList();
            }
        }
    }

    public Task<SendEmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _sentEmails.Add(message);
        }

        return Task.FromResult(new SendEmailResult
        {
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        });
    }

    public Task<SendEmailResult> SendTemplatedAsync(TemplatedEmailRequest request, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            Subject = request.TemplateId,
            To = request.To
        };

        lock (_lock)
        {
            _sentEmails.Add(message);
        }

        return Task.FromResult(new SendEmailResult
        {
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        });
    }

    public Task<SendEmailResult> SendBulkAsync(BulkEmailRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SendEmailResult
        {
            Success = true,
            MessageId = Guid.NewGuid().ToString()
        });
    }

    public Task<EmailTemplate> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EmailTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Key = request.Key,
            Name = request.Name,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody
        });
    }

    public Task<EmailTemplate> UpdateTemplateAsync(string templateId, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EmailTemplate
        {
            Id = templateId
        });
    }

    public Task DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<EmailTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<EmailTemplate?>(null);
    }

    public Task<IList<EmailTemplate>> ListTemplatesAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IList<EmailTemplate>>(new List<EmailTemplate>());
    }

    public Task<EmailTrackingInfo?> GetEmailStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<EmailTrackingInfo?>(null);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _sentEmails.Clear();
        }
    }

    public bool HasEmailTo(string email)
    {
        lock (_lock)
        {
            return _sentEmails.Any(e => e.To.Any(t => t.Email == email));
        }
    }
}
