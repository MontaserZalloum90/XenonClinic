namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a grooming appointment for a pet
/// </summary>
public class GroomingAppointment
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int BranchId { get; set; }

    public DateTime AppointmentDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Status (Scheduled, InProgress, Completed, Cancelled, NoShow)
    /// </summary>
    public string Status { get; set; } = "Scheduled";

    /// <summary>
    /// Package type (Basic, Standard, Premium, Full Grooming)
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// Individual services requested (JSON array or comma-separated)
    /// Bath, Brush, Haircut, NailTrim, EarCleaning, TeethBrushing, DeShedding, FleaTreatment
    /// </summary>
    public string? Services { get; set; }

    /// <summary>
    /// Special instructions from owner
    /// </summary>
    public string? SpecialInstructions { get; set; }

    /// <summary>
    /// Any skin or coat issues noted
    /// </summary>
    public string? SkinCoatNotes { get; set; }

    /// <summary>
    /// Behavior notes during grooming
    /// </summary>
    public string? BehaviorNotes { get; set; }

    public string? GroomerId { get; set; }
    public decimal? TotalFee { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Before grooming photo path
    /// </summary>
    public string? BeforePhotoPath { get; set; }

    /// <summary>
    /// After grooming photo path
    /// </summary>
    public string? AfterPhotoPath { get; set; }

    public Pet? Pet { get; set; }
    public Branch? Branch { get; set; }
}
