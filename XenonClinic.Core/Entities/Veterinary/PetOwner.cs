namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a pet owner (human client of the veterinary clinic)
/// </summary>
public class PetOwner
{
    public int Id { get; set; }
    public int BranchId { get; set; }

    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public string? EmiratesId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

    /// <summary>
    /// Preferred contact method (Phone, SMS, Email, WhatsApp)
    /// </summary>
    public string PreferredContactMethod { get; set; } = "Phone";

    /// <summary>
    /// Emergency contact name
    /// </summary>
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
