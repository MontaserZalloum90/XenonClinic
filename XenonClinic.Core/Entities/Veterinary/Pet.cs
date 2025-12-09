namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a pet/animal patient in a veterinary clinic
/// </summary>
public class Pet
{
    public int Id { get; set; }
    public int PetOwnerId { get; set; }
    public int BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Species (e.g., Dog, Cat, Bird, Rabbit, Hamster, Fish, Reptile)
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// Breed (e.g., Golden Retriever, Persian, Parakeet)
    /// </summary>
    public string? Breed { get; set; }

    public string? Color { get; set; }
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gender (Male, Female, Unknown)
    /// </summary>
    public string Gender { get; set; } = "Unknown";

    /// <summary>
    /// Weight in kilograms
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Microchip ID number
    /// </summary>
    public string? MicrochipId { get; set; }

    /// <summary>
    /// Whether the pet is spayed/neutered
    /// </summary>
    public bool IsNeutered { get; set; }

    /// <summary>
    /// Whether the pet is active (alive and under care)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Deceased date if applicable
    /// </summary>
    public DateTime? DeceasedDate { get; set; }

    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? DietaryRequirements { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Photo path
    /// </summary>
    public string? PhotoPath { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public PetOwner? Owner { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<VetVisit> Visits { get; set; } = new List<VetVisit>();
    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
    public ICollection<GroomingAppointment> GroomingAppointments { get; set; } = new List<GroomingAppointment>();
    public ICollection<BoardingReservation> BoardingReservations { get; set; } = new List<BoardingReservation>();
}
