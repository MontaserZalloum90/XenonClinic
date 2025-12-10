namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a boarding/accommodation reservation for a pet
/// </summary>
public class BoardingReservation
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int BranchId { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime ExpectedCheckOutDate { get; set; }
    public DateTime? ActualCheckOutDate { get; set; }

    /// <summary>
    /// Status (Reserved, CheckedIn, CheckedOut, Cancelled, NoShow)
    /// </summary>
    public string Status { get; set; } = "Reserved";

    /// <summary>
    /// Accommodation type (Standard Kennel, Deluxe Suite, Cat Condo, Exotic Habitat)
    /// </summary>
    public string AccommodationType { get; set; } = "Standard Kennel";

    /// <summary>
    /// Kennel/Room number assigned
    /// </summary>
    public string? KennelNumber { get; set; }

    /// <summary>
    /// Feeding instructions
    /// </summary>
    public string? FeedingInstructions { get; set; }

    /// <summary>
    /// Medications to be administered
    /// </summary>
    public string? MedicationInstructions { get; set; }

    /// <summary>
    /// Special care requirements
    /// </summary>
    public string? SpecialCareInstructions { get; set; }

    /// <summary>
    /// Items brought by owner (toys, bedding, etc.)
    /// </summary>
    public string? BelongingsList { get; set; }

    /// <summary>
    /// Emergency contact for this reservation
    /// </summary>
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }

    /// <summary>
    /// Daily rate
    /// </summary>
    public decimal DailyRate { get; set; }

    /// <summary>
    /// Additional services (e.g., extra walks, playtime, grooming)
    /// </summary>
    public string? AdditionalServices { get; set; }
    public decimal? AdditionalServicesFee { get; set; }

    public decimal? TotalFee { get; set; }
    public bool IsPaid { get; set; }

    /// <summary>
    /// Daily care notes recorded by staff
    /// </summary>
    public string? DailyCareNotes { get; set; }

    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Pet? Pet { get; set; }
    public Branch? Branch { get; set; }
}
