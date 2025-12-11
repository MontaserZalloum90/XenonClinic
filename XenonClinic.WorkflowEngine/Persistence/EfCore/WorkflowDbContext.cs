namespace XenonClinic.WorkflowEngine.Persistence.EfCore;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext for workflow engine persistence.
/// </summary>
public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinitionEntity> WorkflowDefinitions => Set<WorkflowDefinitionEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<WorkflowExecutionHistoryEntity> ExecutionHistory => Set<WorkflowExecutionHistoryEntity>();
    public DbSet<WorkflowBookmarkEntity> Bookmarks => Set<WorkflowBookmarkEntity>();
    public DbSet<WorkflowTimerEntity> Timers => Set<WorkflowTimerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WorkflowDefinition - composite key on Id + Version
        modelBuilder.Entity<WorkflowDefinitionEntity>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Version });
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.IsActive, e.IsDraft });
        });

        // WorkflowInstance indexes
        modelBuilder.Entity<WorkflowInstanceEntity>(entity =>
        {
            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.ScheduledStartTime);
            entity.HasIndex(e => new { e.LockHolder, e.LockExpiry });
        });

        // ExecutionHistory indexes
        modelBuilder.Entity<WorkflowExecutionHistoryEntity>(entity =>
        {
            entity.HasIndex(e => e.InstanceId);
            entity.HasIndex(e => e.Timestamp);
        });

        // Bookmark indexes
        modelBuilder.Entity<WorkflowBookmarkEntity>(entity =>
        {
            entity.HasIndex(e => e.InstanceId);
            entity.HasIndex(e => e.Name);
        });

        // Timer indexes
        modelBuilder.Entity<WorkflowTimerEntity>(entity =>
        {
            entity.HasIndex(e => e.InstanceId);
            entity.HasIndex(e => new { e.FireAt, e.IsTriggered });
        });
    }
}
