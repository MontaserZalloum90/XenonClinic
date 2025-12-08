namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for leave request statuses (replaces LeaveStatus enum).
/// Examples: Pending, Approved, Rejected, Cancelled
/// </summary>
public class LeaveStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether this status represents an approved leave.
    /// </summary>
    public bool IsApprovedStatus { get; set; } = false;

    /// <summary>
    /// Whether this status represents a rejected leave.
    /// </summary>
    public bool IsRejectedStatus { get; set; } = false;

    /// <summary>
    /// Whether leave requests in this status can be edited.
    /// </summary>
    public bool AllowsEditing { get; set; } = true;

    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
