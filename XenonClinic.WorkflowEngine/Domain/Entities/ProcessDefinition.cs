namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a process definition (workflow template).
/// </summary>
[Table("WF_ProcessDefinitions")]
public class ProcessDefinition
{
    [Key]
    [MaxLength(100)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int TenantId { get; set; }

    /// <summary>
    /// Semantic key for the process (e.g., "leave-request", "invoice-approval")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Latest version number
    /// </summary>
    public int LatestVersion { get; set; } = 1;

    /// <summary>
    /// Currently published (active) version, null if none published
    /// </summary>
    public int? PublishedVersion { get; set; }

    public ProcessDefinitionStatus Status { get; set; } = ProcessDefinitionStatus.Draft;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Additional metadata (JSON object)
    /// </summary>
    public string? MetadataJson { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }

    public virtual ICollection<ProcessVersion> Versions { get; set; } = new List<ProcessVersion>();
    public virtual ICollection<ProcessInstance> Instances { get; set; } = new List<ProcessInstance>();
}

public enum ProcessDefinitionStatus
{
    Draft,
    Active,
    Deprecated,
    Archived
}

/// <summary>
/// Represents a specific version of a process definition.
/// </summary>
[Table("WF_ProcessVersions")]
public class ProcessVersion
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string ProcessDefinitionId { get; set; } = string.Empty;

    public int Version { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? ChangeNotes { get; set; }

    public ProcessVersionStatus Status { get; set; } = ProcessVersionStatus.Draft;

    /// <summary>
    /// The complete process model (JSON serialized)
    /// </summary>
    [Required]
    public string ModelJson { get; set; } = "{}";

    /// <summary>
    /// Visual layout for the designer (JSON serialized)
    /// </summary>
    public string? LayoutJson { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    [MaxLength(100)]
    public string? PublishedBy { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProcessDefinitionId))]
    public virtual ProcessDefinition? ProcessDefinition { get; set; }
}

public enum ProcessVersionStatus
{
    Draft,
    Published,
    Deprecated
}
