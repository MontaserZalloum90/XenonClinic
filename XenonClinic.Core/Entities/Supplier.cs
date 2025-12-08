namespace XenonClinic.Core.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
