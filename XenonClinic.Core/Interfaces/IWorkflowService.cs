using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Workflow engine service.
/// </summary>
public interface IWorkflowService
{
    // Workflow Definitions
    Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(CreateWorkflowDefinitionRequest request);
    Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(int workflowId);
    Task<WorkflowDefinition?> GetWorkflowDefinitionByCodeAsync(string code);
    Task<IEnumerable<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string? entityType = null, bool activeOnly = true);
    Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(int workflowId, UpdateWorkflowDefinitionRequest request);
    Task ActivateWorkflowDefinitionAsync(int workflowId);
    Task DeactivateWorkflowDefinitionAsync(int workflowId);

    // Workflow Steps
    Task<WorkflowStep> AddWorkflowStepAsync(int workflowId, AddWorkflowStepRequest request);
    Task<WorkflowStep> UpdateWorkflowStepAsync(int stepId, UpdateWorkflowStepRequest request);
    Task DeleteWorkflowStepAsync(int stepId);
    Task ReorderWorkflowStepsAsync(int workflowId, List<int> stepIdsInOrder);

    // Workflow Instances
    Task<WorkflowInstance> StartWorkflowAsync(StartWorkflowRequest request);
    Task<WorkflowInstance?> GetWorkflowInstanceAsync(int instanceId);
    Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesAsync(WorkflowInstanceSearchCriteria criteria);
    Task<WorkflowInstance?> GetWorkflowInstanceForEntityAsync(string entityType, int entityId);
    Task CancelWorkflowAsync(int instanceId, string reason);

    // Workflow Actions
    Task<WorkflowInstance> ApproveStepAsync(int instanceId, ApproveStepRequest request);
    Task<WorkflowInstance> RejectStepAsync(int instanceId, RejectStepRequest request);
    Task<WorkflowInstance> RequestMoreInfoAsync(int instanceId, RequestMoreInfoRequest request);
    Task<WorkflowInstance> DelegateStepAsync(int instanceId, DelegateStepRequest request);

    // Tasks
    Task<IEnumerable<WorkflowTask>> GetMyTasksAsync(string userId, TaskFilterCriteria? filter = null);
    Task<IEnumerable<WorkflowTask>> GetDepartmentTasksAsync(int departmentId, TaskFilterCriteria? filter = null);
    Task<WorkflowTask?> GetTaskAsync(int taskId);
    Task<int> GetPendingTaskCountAsync(string userId);
    Task ClaimTaskAsync(int taskId, string userId);

    // Delegations
    Task<ApprovalDelegation> CreateDelegationAsync(CreateDelegationRequest request);
    Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsAsync(int employeeId);
    Task CancelDelegationAsync(int delegationId);
    Task<string?> GetDelegateForUserAsync(int employeeId, string? workflowCode = null);

    // History & Audit
    Task<IEnumerable<WorkflowHistory>> GetWorkflowHistoryAsync(int instanceId);
    Task<WorkflowAuditReport> GetWorkflowAuditReportAsync(int instanceId);

    // Notifications
    Task SendReminderAsync(int stepInstanceId);
    Task EscalateStepAsync(int stepInstanceId);
    Task ProcessOverdueStepsAsync();

    // Reporting
    Task<WorkflowDashboard> GetDashboardAsync(string userId);
    Task<WorkflowStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate, string? workflowCode = null);
}

// Request DTOs
public record CreateWorkflowDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    string? Category,
    string EntityType,
    int? SLAHours,
    bool AllowParallelApproval = false,
    bool RequireAllApprovers = false
);

public record UpdateWorkflowDefinitionRequest(
    string? Name,
    string? Description,
    int? SLAHours,
    bool? AllowParallelApproval,
    bool? RequireAllApprovers
);

public record AddWorkflowStepRequest(
    int StepOrder,
    string Name,
    string? Description,
    StepType Type,
    ApproverType ApproverType,
    int? ApproverRoleId,
    int? ApproverEmployeeId,
    int? ApproverDepartmentId,
    string? ApproverExpression,
    bool AllowDelegation = true,
    bool CanReject = true,
    int? EscalationHours,
    int? EscalateToRoleId
);

public record UpdateWorkflowStepRequest(
    string? Name,
    string? Description,
    int? ApproverRoleId,
    int? ApproverEmployeeId,
    int? EscalationHours,
    string? NotificationTemplate
);

public record StartWorkflowRequest(
    string WorkflowCode,
    string EntityType,
    int EntityId,
    string? EntityReference,
    string? InitiatorComments
);

public record WorkflowInstanceSearchCriteria(
    string? WorkflowCode = null,
    string? EntityType = null,
    int? EntityId = null,
    WorkflowStatus? Status = null,
    string? InitiatedBy = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
);

public record ApproveStepRequest(
    string? Comments,
    Dictionary<string, object>? AdditionalData = null
);

public record RejectStepRequest(
    string Reason,
    string? Comments
);

public record RequestMoreInfoRequest(
    string Question,
    string? RequestedFrom
);

public record DelegateStepRequest(
    int DelegateToEmployeeId,
    string Reason
);

public record TaskFilterCriteria(
    WorkflowTaskType? Type = null,
    WorkflowTaskPriority? Priority = null,
    WorkflowTaskStatus? Status = null,
    string? EntityType = null,
    bool OverdueOnly = false
);

public record CreateDelegationRequest(
    int DelegatorEmployeeId,
    int DelegateEmployeeId,
    DateTime StartDate,
    DateTime EndDate,
    List<string>? WorkflowCodes,
    string? Reason
);

// Response DTOs
public record WorkflowAuditReport(
    int InstanceId,
    string WorkflowName,
    string EntityType,
    int EntityId,
    string? EntityReference,
    WorkflowStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? InitiatedBy,
    TimeSpan TotalDuration,
    List<StepAuditDetail> StepDetails
);

public record StepAuditDetail(
    int StepOrder,
    string StepName,
    string? AssignedTo,
    DateTime? AssignedAt,
    DateTime? ActionAt,
    string? Action,
    string? ActionBy,
    string? Comments,
    TimeSpan? Duration
);

public record WorkflowDashboard(
    int PendingApprovals,
    int OverdueTasks,
    int CompletedToday,
    int RejectedThisWeek,
    List<TaskSummaryByType> TasksByType,
    List<PendingWorkflowSummary> PendingWorkflows,
    List<RecentActivity> RecentActivities
);

public record TaskSummaryByType(
    string WorkflowName,
    int PendingCount,
    int OverdueCount,
    decimal AverageCompletionHours
);

public record PendingWorkflowSummary(
    int InstanceId,
    string WorkflowName,
    string EntityReference,
    string CurrentStep,
    DateTime DueDate,
    bool IsOverdue
);

public record RecentActivity(
    DateTime Timestamp,
    string Action,
    string WorkflowName,
    string EntityReference,
    string? ActionBy
);

public record WorkflowStatistics(
    int TotalInstances,
    int CompletedInstances,
    int RejectedInstances,
    int CancelledInstances,
    int InProgressInstances,
    decimal ApprovalRate,
    decimal AverageCompletionHours,
    decimal AverageStepsPerWorkflow,
    List<WorkflowTypeStats> StatsByWorkflow,
    List<ApproverStats> TopApprovers
);

public record WorkflowTypeStats(
    string WorkflowCode,
    string WorkflowName,
    int TotalInstances,
    int Completed,
    int Rejected,
    decimal ApprovalRate,
    decimal AverageHours
);

public record ApproverStats(
    string ApproverName,
    int TotalApprovals,
    int Rejections,
    decimal AverageResponseHours
);
