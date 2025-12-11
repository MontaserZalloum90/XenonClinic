namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a tenant in the multi-tenant workflow system.
/// </summary>
[Table("WF_Tenants")]
public class Tenant
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique slug for URL-friendly identification (e.g., "acme-corp")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// JSON serialized tenant settings
    /// </summary>
    public string? SettingsJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<ProcessDefinition> ProcessDefinitions { get; set; } = new List<ProcessDefinition>();
    public virtual ICollection<ProcessInstance> ProcessInstances { get; set; } = new List<ProcessInstance>();
}

public enum TenantStatus
{
    Active,
    Suspended,
    Deactivated
}

/// <summary>
/// Tenant-specific settings
/// </summary>
public class TenantSettings
{
    public string Timezone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public string DefaultLanguage { get; set; } = "en";
    public int MaxConcurrentInstances { get; set; } = 1000;
    public int TaskRetentionDays { get; set; } = 365;
    public int AuditRetentionDays { get; set; } = 730;
    public BusinessCalendarSettings? DefaultCalendar { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class BusinessCalendarSettings
{
    public string Name { get; set; } = "Default";
    public List<DayOfWeek> WorkingDays { get; set; } = new()
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday
    };
    public TimeSpan WorkdayStart { get; set; } = TimeSpan.FromHours(9);
    public TimeSpan WorkdayEnd { get; set; } = TimeSpan.FromHours(17);
    public List<DateTime> Holidays { get; set; } = new();
}
