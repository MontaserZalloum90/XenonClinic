using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateLabOrderViewModel
{
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int PatientId { get; set; }

    public int? ExternalLabId { get; set; }

    public DateTime? ExpectedCompletionDate { get; set; }

    public bool IsUrgent { get; set; } = false;

    public string? ClinicalNotes { get; set; }

    public string? Notes { get; set; }

    // Test IDs (will be added dynamically via JavaScript)
    public List<int> SelectedTestIds { get; set; } = new();
}
