namespace XenonClinic.WorkflowEngine.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// DbContext for the enterprise workflow engine.
/// </summary>
public class WorkflowEngineDbContext : DbContext
{
    public WorkflowEngineDbContext(DbContextOptions<WorkflowEngineDbContext> options) : base(options)
    {
    }

    // Multi-tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // Process Definitions
    public DbSet<ProcessDefinition> ProcessDefinitions => Set<ProcessDefinition>();
    public DbSet<ProcessVersion> ProcessVersions => Set<ProcessVersion>();

    // Process Instances
    public DbSet<ProcessInstance> ProcessInstances => Set<ProcessInstance>();
    public DbSet<ProcessVariable> ProcessVariables => Set<ProcessVariable>();
    public DbSet<ActivityInstance> ActivityInstances => Set<ActivityInstance>();

    // Human Tasks
    public DbSet<HumanTask> HumanTasks => Set<HumanTask>();
    public DbSet<TaskAction> TaskActions => Set<TaskAction>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<TaskCandidate> TaskCandidates => Set<TaskCandidate>();

    // Timers & Jobs
    public DbSet<ProcessTimer> ProcessTimers => Set<ProcessTimer>();
    public DbSet<AsyncJob> AsyncJobs => Set<AsyncJob>();

    // Audit & Analytics
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<ProcessMetric> ProcessMetrics => Set<ProcessMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================================
        // TENANT
        // =====================================================================
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // =====================================================================
        // PROCESS DEFINITION
        // =====================================================================
        modelBuilder.Entity<ProcessDefinition>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Category });
            entity.HasIndex(e => new { e.TenantId, e.Status });

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.ProcessDefinitions)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // PROCESS VERSION
        // =====================================================================
        modelBuilder.Entity<ProcessVersion>(entity =>
        {
            entity.HasIndex(e => new { e.ProcessDefinitionId, e.Version }).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.ProcessDefinition)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.ProcessDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // PROCESS INSTANCE
        // =====================================================================
        modelBuilder.Entity<ProcessInstance>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.BusinessKey });
            entity.HasIndex(e => e.ProcessDefinitionId);
            entity.HasIndex(e => e.ParentInstanceId);
            entity.HasIndex(e => new { e.LockHolder, e.LockExpiry });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Status, e.DueDate });

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.ProcessInstances)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessDefinition)
                .WithMany(d => d.Instances)
                .HasForeignKey(e => e.ProcessDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentInstance)
                .WithMany(p => p.ChildInstances)
                .HasForeignKey(e => e.ParentInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // PROCESS VARIABLE
        // =====================================================================
        modelBuilder.Entity<ProcessVariable>(entity =>
        {
            entity.HasIndex(e => e.ProcessInstanceId);
            entity.HasIndex(e => new { e.ProcessInstanceId, e.Name, e.Scope });

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.NumberValue)
                .HasPrecision(28, 10);

            entity.HasOne(e => e.ProcessInstance)
                .WithMany(p => p.Variables)
                .HasForeignKey(e => e.ProcessInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ACTIVITY INSTANCE
        // =====================================================================
        modelBuilder.Entity<ActivityInstance>(entity =>
        {
            entity.HasIndex(e => new { e.ProcessInstanceId, e.Status });
            entity.HasIndex(e => e.ParentActivityInstanceId);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.ProcessInstance)
                .WithMany(p => p.ActivityInstances)
                .HasForeignKey(e => e.ProcessInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentActivityInstance)
                .WithMany(a => a.ChildActivityInstances)
                .HasForeignKey(e => e.ParentActivityInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // HUMAN TASK
        // =====================================================================
        modelBuilder.Entity<HumanTask>(entity =>
        {
            entity.HasIndex(e => e.ActivityInstanceId).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.AssigneeUserId, e.Status })
                .IncludeProperties(e => new { e.Priority, e.DueDate });
            entity.HasIndex(e => new { e.TenantId, e.Status, e.Priority, e.DueDate });
            entity.HasIndex(e => e.ProcessInstanceId);
            entity.HasIndex(e => new { e.TenantId, e.DueDate, e.Status });
            entity.HasIndex(e => e.BusinessKey);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Priority)
                .HasConversion<int>();

            entity.HasOne(e => e.ActivityInstance)
                .WithOne(a => a.HumanTask)
                .HasForeignKey<HumanTask>(e => e.ActivityInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProcessInstance)
                .WithMany()
                .HasForeignKey(e => e.ProcessInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // TASK CANDIDATES (for efficient querying)
        // =====================================================================
        modelBuilder.Entity<TaskCandidate>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.CandidateType, e.CandidateId });
            entity.HasIndex(e => new { e.CandidateType, e.CandidateId, e.TaskId });

            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // TASK ACTION
        // =====================================================================
        modelBuilder.Entity<TaskAction>(entity =>
        {
            entity.HasIndex(e => new { e.TaskId, e.Timestamp });

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Actions)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // TASK COMMENT
        // =====================================================================
        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasIndex(e => e.TaskId);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // TASK ATTACHMENT
        // =====================================================================
        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.HasIndex(e => e.TaskId);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // PROCESS TIMER
        // =====================================================================
        modelBuilder.Entity<ProcessTimer>(entity =>
        {
            entity.HasIndex(e => new { e.Status, e.FireAt })
                .IncludeProperties(e => e.ProcessInstanceId);
            entity.HasIndex(e => e.ProcessInstanceId);

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.ProcessInstance)
                .WithMany(p => p.Timers)
                .HasForeignKey(e => e.ProcessInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =====================================================================
        // ASYNC JOB
        // =====================================================================
        modelBuilder.Entity<AsyncJob>(entity =>
        {
            entity.HasIndex(e => new { e.Status, e.NextRetryAt })
                .HasFilter("[Status] IN ('Pending', 'Retrying')");
            entity.HasIndex(e => new { e.LockOwner, e.LockExpiry });
            entity.HasIndex(e => e.ProcessInstanceId);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(e => e.ProcessInstance)
                .WithMany()
                .HasForeignKey(e => e.ProcessInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ActivityInstance)
                .WithMany()
                .HasForeignKey(e => e.ActivityInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =====================================================================
        // AUDIT EVENT
        // =====================================================================
        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId, e.Timestamp });
            entity.HasIndex(e => new { e.ProcessInstanceId, e.Timestamp });
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => e.CorrelationId);
        });

        // =====================================================================
        // PROCESS METRICS
        // =====================================================================
        modelBuilder.Entity<ProcessMetric>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ProcessDefinitionId, e.PeriodStart });
            entity.HasIndex(e => new { e.TenantId, e.PeriodStart });
        });
    }
}
