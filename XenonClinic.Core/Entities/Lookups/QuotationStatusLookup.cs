namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for quotation statuses (replaces QuotationStatus enum).
/// Examples: Draft, Sent, Accepted, Rejected, Expired
/// </summary>
public class QuotationStatusLookup : SystemLookup
{
    public bool IsFinalStatus { get; set; } = false;
    public bool AllowsEditing { get; set; } = true;
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
