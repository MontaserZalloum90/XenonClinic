using XenonClinic.Core.Entities;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for the dashboard page
/// </summary>
public class DashboardViewModel
{
    public int AppointmentsToday { get; set; }
    public int ActivePatients { get; set; }
    public int VisitsThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<VisitSeriesData> VisitsSeries { get; set; } = new();
}

public class VisitSeriesData
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
