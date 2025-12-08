namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for specimen types (replaces SpecimenType enum).
/// Examples: Blood, Urine, Saliva, Tissue, Swab, Stool
/// </summary>
public class SpecimenTypeLookup : SystemLookup
{
    public string? CollectionInstructions { get; set; }
    public string? StorageRequirements { get; set; }
    public int? ExpiryHours { get; set; }
    public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
}
