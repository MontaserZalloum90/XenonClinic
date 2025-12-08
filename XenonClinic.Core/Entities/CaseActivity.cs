namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an activity or task within a case
/// </summary>
public class CaseActivity
{
    public int Id { get; set; }
    public int CaseId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Type of activity: Task, Appointment, Test, FollowUp, PhoneCall, Email
    /// </summary>
    public CaseActivityType ActivityType { get; set; } = CaseActivityType.Task;

    /// <summary>
    /// Status of the activity: Pending, InProgress, Completed, Cancelled
    /// </summary>
    public CaseActivityStatus Status { get; set; } = CaseActivityStatus.Pending;

    /// <summary>
    /// User assigned to complete this activity
    /// </summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>
    /// Due date for this activity
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Date when the activity was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Priority of this activity
    /// </summary>
    public CasePriority Priority { get; set; } = CasePriority.Medium;

    /// <summary>
    /// Result or outcome of the activity
    /// </summary>
    public string? Result { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Case Case { get; set; } = null!;
    public ApplicationUser? AssignedToUser { get; set; }
}

public enum CaseActivityType
{
    Task = 1,
    Appointment = 2,
    Test = 3,
    FollowUp = 4,
    PhoneCall = 5,
    Email = 6,
    Consultation = 7,
    Review = 8
}

public enum CaseActivityStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Overdue = 5
}
