namespace XenonClinic.Web.Models;

public class SalesDashboardViewModel
{
    // Metrics
    public decimal TotalSalesToday { get; set; }
    public decimal TotalSalesThisMonth { get; set; }
    public int TotalInvoicesToday { get; set; }
    public int TotalInvoicesThisMonth { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int OverdueInvoices { get; set; }
    public int PendingQuotations { get; set; }

    // Recent Data
    public List<SaleDto> RecentSales { get; set; } = new();
    public List<SaleDto> OverdueSales { get; set; } = new();
    public List<QuotationDto> RecentQuotations { get; set; } = new();
}
