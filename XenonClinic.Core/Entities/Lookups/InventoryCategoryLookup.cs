namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for inventory categories (replaces InventoryCategory enum).
/// Examples: Hearing Aids, Batteries, Accessories, Cleaning Supplies, Ear Molds, Testing Equipment, Consumables
/// </summary>
public class InventoryCategoryLookup : SystemLookup
{
    /// <summary>
    /// Whether items in this category require serial number tracking.
    /// </summary>
    public bool RequiresSerialNumber { get; set; } = false;

    /// <summary>
    /// Whether items in this category have expiry dates.
    /// </summary>
    public bool HasExpiryDate { get; set; } = false;

    /// <summary>
    /// Default warranty period in months for items in this category.
    /// </summary>
    public int? DefaultWarrantyMonths { get; set; }

    // Navigation properties
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
