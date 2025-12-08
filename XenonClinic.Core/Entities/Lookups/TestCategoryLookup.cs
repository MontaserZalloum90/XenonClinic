namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for test categories (replaces TestCategory enum).
/// Examples: Hematology, Biochemistry, Microbiology, Immunology, Pathology, Imaging, Audiology, Cardiology
/// </summary>
public class TestCategoryLookup : SystemLookup
{
    public string? DepartmentName { get; set; }
    public int? DefaultTurnaroundTimeHours { get; set; }
    public bool RequiresSpecialization { get; set; } = false;
    public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
}
