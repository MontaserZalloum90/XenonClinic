namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a patient's complete dental chart
/// </summary>
public class ToothChart
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// Whether this is a pediatric (primary teeth) chart
    /// </summary>
    public bool IsPediatricChart { get; set; }

    /// <summary>
    /// Overall periodontal status (e.g., Healthy, Gingivitis, Mild Periodontitis, Moderate Periodontitis, Severe Periodontitis)
    /// </summary>
    public string? PeriodontalStatus { get; set; }

    /// <summary>
    /// Occlusion classification (e.g., Class I, Class II Division 1, Class II Division 2, Class III)
    /// </summary>
    public string? OcclusionClass { get; set; }

    /// <summary>
    /// JSON data for periodontal charting (pocket depths, bleeding points, etc.)
    /// </summary>
    public string? PeriodontalChartJson { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<ToothRecord> TeethRecords { get; set; } = new List<ToothRecord>();
}
