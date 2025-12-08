namespace XenonClinic.Web.Models;

public class LabDashboardViewModel
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrdersToday { get; set; }
    public int UrgentOrders { get; set; }
    public int PendingResults { get; set; }
    public int AbnormalResults { get; set; }
    public int TotalTests { get; set; }
    public int ActiveTests { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<LabOrderDto> RecentOrders { get; set; } = new();
    public List<LabResultDto> PendingResults { get; set; } = new();
    public List<LabTestDto> PopularTests { get; set; } = new();
}
