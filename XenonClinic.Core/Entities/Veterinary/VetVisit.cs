namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a veterinary visit/consultation
/// </summary>
public class VetVisit
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int BranchId { get; set; }
    public DateTime VisitDate { get; set; }

    /// <summary>
    /// Type of visit (Wellness, Sick, Emergency, Surgery, Follow-up, Dental, Vaccination)
    /// </summary>
    public string VisitType { get; set; } = "Wellness";
    public string? ChiefComplaint { get; set; }

    /// <summary>
    /// Weight at time of visit (kg)
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Temperature (Celsius)
    /// </summary>
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Heart rate (BPM)
    /// </summary>
    public int? HeartRate { get; set; }

    /// <summary>
    /// Respiratory rate (breaths per minute)
    /// </summary>
    public int? RespiratoryRate { get; set; }
    public string? PhysicalExamFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentProvided { get; set; }
    public string? Prescriptions { get; set; }
    public string? LabResultsSummary { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public string? VeterinarianId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Pet? Pet { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<VetProcedure> Procedures { get; set; } = new List<VetProcedure>();
}
