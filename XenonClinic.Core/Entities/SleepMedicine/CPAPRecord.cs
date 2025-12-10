using XenonClinic.Core.Enums.SleepMedicine;

namespace XenonClinic.Core.Entities.SleepMedicine;

public class CPAPRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CPAPDeviceType DeviceType { get; set; }
    public string? DeviceManufacturer { get; set; }
    public string? DeviceModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? MaskType { get; set; }
    public string? MaskModel { get; set; }
    public string? MaskSize { get; set; }
    public decimal? PrescribedPressure { get; set; }
    public decimal? MinPressure { get; set; }
    public decimal? MaxPressure { get; set; }
    public decimal? IPAP { get; set; }
    public decimal? EPAP { get; set; }
    public bool? HumidifierUsed { get; set; }
    public int? HumidifierSetting { get; set; }
    public bool? HeatedTubing { get; set; }
    public bool? RampEnabled { get; set; }
    public int? RampTime { get; set; }
    public decimal? AverageUsageHours { get; set; }
    public decimal? CompliancePercentage { get; set; }
    public decimal? ResidualAHI { get; set; }
    public decimal? Leak95thPercentile { get; set; }
    public DateTime? LastDownloadDate { get; set; }
    public string? ComplianceIssues { get; set; }
    public string? SideEffects { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
