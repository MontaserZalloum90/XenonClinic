namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a note or comment added to a case
/// </summary>
public class CaseNote
{
    public int Id { get; set; }
    public int CaseId { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type of note: General, Clinical, Administrative, FollowUp
    /// </summary>
    public CaseNoteType NoteType { get; set; } = CaseNoteType.General;

    /// <summary>
    /// Whether this note is visible to the patient (for patient portal)
    /// </summary>
    public bool IsVisibleToPatient { get; set; } = false;

    /// <summary>
    /// Whether this note is pinned to the top
    /// </summary>
    public bool IsPinned { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Case Case { get; set; } = null!;
}

public enum CaseNoteType
{
    General = 1,
    Clinical = 2,
    Administrative = 3,
    FollowUp = 4,
    Important = 5
}
