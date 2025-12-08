namespace XenonClinic.Web.Models;

public class ProcurementDashboardViewModel
{
    public int TotalPurchaseOrders { get; set; }
    public int PendingApprovals { get; set; }
    public int ActivePurchaseOrders { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public decimal OutstandingPayments { get; set; }
    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }
    public int GoodsReceiptsThisMonth { get; set; }
    public List<PurchaseOrderDto> RecentPurchaseOrders { get; set; } = new();
    public List<GoodsReceiptDto> RecentGoodsReceipts { get; set; } = new();
    public List<SupplierDto> TopSuppliers { get; set; } = new();
}
