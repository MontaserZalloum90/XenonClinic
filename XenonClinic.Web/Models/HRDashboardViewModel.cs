namespace XenonClinic.Web.Models;

public class HRDashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PresentToday { get; set; }
    public int OnLeaveToday { get; set; }
    public int PendingLeaveRequests { get; set; }
    public List<EmployeeDto> RecentHires { get; set; } = new();
    public List<LeaveRequestDto> PendingLeaves { get; set; } = new();
    public List<AttendanceDto> TodayAttendance { get; set; } = new();
}
