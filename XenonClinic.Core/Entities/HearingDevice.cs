using System;

namespace XenonClinic.Core.Entities;

public class HearingDevice
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
