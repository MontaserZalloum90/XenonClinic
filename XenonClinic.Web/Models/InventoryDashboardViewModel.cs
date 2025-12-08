namespace XenonClinic.Web.Models;

public class InventoryDashboardViewModel
{
    public int TotalItems { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<InventoryItemDto> RecentlyAddedItems { get; set; } = new();
    public List<InventoryItemDto> LowStockAlerts { get; set; } = new();
    public List<InventoryTransactionDto> RecentTransactions { get; set; } = new();
}
