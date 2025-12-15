using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Employee attendance record
/// </summary>
public class AttendanceRecord : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Date { get; set; }
    public int AttendanceStatusId { get; set; }
    public Lookups.AttendanceStatusLookup? AttendanceStatus { get; set; }

    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? WorkedHours { get; set; }
    public decimal? OvertimeHours { get; set; }

    public bool IsLate { get; set; }
    public int? LateMinutes { get; set; }
    public bool IsEarlyDeparture { get; set; }
    public int? EarlyDepartureMinutes { get; set; }

    public string? Notes { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }

    public bool IsManualEntry { get; set; }
    public string? ManualEntryReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
