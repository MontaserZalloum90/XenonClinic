namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for lab result statuses (replaces LabResultStatus enum).
/// Examples: Pending, In Progress, Completed, Reviewed, Verified
/// </summary>
public class LabResultStatusLookup : SystemLookup
{
    public bool IsCompletedStatus { get; set; } = false;
    public bool RequiresVerification { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
}
