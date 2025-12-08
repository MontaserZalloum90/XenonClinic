namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for leave types (replaces LeaveType enum).
/// Examples: Annual, Sick, Emergency, Maternity, Paternity, Unpaid, Study, Hajj
/// </summary>
public class LeaveTypeLookup : SystemLookup
{
    /// <summary>
    /// Whether this leave type is paid.
    /// </summary>
    public bool IsPaid { get; set; } = true;

    /// <summary>
    /// Maximum days allowed per year for this leave type.
    /// </summary>
    public int? MaxDaysPerYear { get; set; }

    /// <summary>
    /// Whether this leave type requires documentation.
    /// </summary>
    public bool RequiresDocumentation { get; set; } = false;

    /// <summary>
    /// Minimum notice days required before taking this leave.
    /// </summary>
    public int? MinimumNoticeDays { get; set; }

    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
