using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Base Visit entity for general visit tracking across all visit types
/// </summary>
public class Visit : IBranchEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public string VisitType { get; set; } = string.Empty; // Clinical, Dental, Cardiology, etc.
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public string? VitalSigns { get; set; }

    // Visit flags
    public bool HasDiagnosis { get; set; }
    public bool HasPrescription { get; set; }
    public bool HasLabOrders { get; set; }

    // Diagnoses (stored as JSON or separate collection)
    public string? Diagnoses { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
