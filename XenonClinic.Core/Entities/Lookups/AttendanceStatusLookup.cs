namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for attendance statuses (replaces AttendanceStatus enum).
/// Examples: Present, Absent, Late, Half Day, On Leave, Holiday
/// </summary>
public class AttendanceStatusLookup : SystemLookup
{
    public bool CountsAsPresent { get; set; } = true;
    public decimal WorkHoursMultiplier { get; set; } = 1.0m; // 1.0 for full day, 0.5 for half day, 0 for absent
    public bool RequiresJustification { get; set; } = false;
    public bool AffectsSalary { get; set; } = true;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
