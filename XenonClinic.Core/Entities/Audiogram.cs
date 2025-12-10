namespace XenonClinic.Core.Entities;

public class Audiogram
{
    public int Id { get; set; }
    public int AudiologyVisitId { get; set; }
    public string RawDataJson { get; set; } = "{}";
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public AudiologyVisit? Visit { get; set; }
}
