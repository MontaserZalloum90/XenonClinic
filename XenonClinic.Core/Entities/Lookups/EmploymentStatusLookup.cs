namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for employment statuses (replaces EmploymentStatus enum).
/// Examples: Active, On Leave, Suspended, Terminated, Resigned
/// </summary>
public class EmploymentStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether employees with this status are considered active.
    /// </summary>
    public bool IsActiveStatus { get; set; } = true;

    /// <summary>
    /// Whether employees with this status can access the system.
    /// </summary>
    public bool HasSystemAccess { get; set; } = true;

    /// <summary>
    /// Whether employees with this status accrue benefits.
    /// </summary>
    public bool AccruesBenefits { get; set; } = true;

    // Navigation properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
