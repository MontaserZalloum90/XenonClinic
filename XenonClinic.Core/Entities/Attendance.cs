using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class Attendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public bool IsLate { get; set; }
    public int? LateMinutes { get; set; }
    public decimal? WorkedHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee Employee { get; set; } = null!;
}
