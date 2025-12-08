namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for case note types (replaces CaseNoteType enum).
/// Examples: General, Clinical, Administrative, Follow-up, Important
/// </summary>
public class CaseNoteTypeLookup : SystemLookup
{
    /// <summary>
    /// Whether notes of this type require manager review.
    /// </summary>
    public bool RequiresReview { get; set; } = false;

    /// <summary>
    /// Whether notes of this type are visible to patients.
    /// </summary>
    public bool IsVisibleToPatient { get; set; } = false;

    // Navigation properties
    public ICollection<CaseNote> CaseNotes { get; set; } = new List<CaseNote>();
}
