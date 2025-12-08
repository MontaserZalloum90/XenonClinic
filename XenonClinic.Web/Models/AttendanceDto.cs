using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public int? LateMinutes { get; set; }
    public decimal? WorkedHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string? Notes { get; set; }
}
