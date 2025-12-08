namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for expense statuses (replaces ExpenseStatus enum).
/// Examples: Pending, Approved, Paid, Rejected
/// </summary>
public class ExpenseStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether expenses with this status are considered approved.
    /// </summary>
    public bool IsApprovedStatus { get; set; } = false;

    /// <summary>
    /// Whether expenses with this status are considered paid.
    /// </summary>
    public bool IsPaidStatus { get; set; } = false;

    /// <summary>
    /// Whether expenses in this status can be edited.
    /// </summary>
    public bool AllowsEditing { get; set; } = true;

    // Navigation properties
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
