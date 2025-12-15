using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Doctor/Physician entity for medical staff
/// </summary>
public class Doctor : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string? UserId { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();

    public string? Title { get; set; } // Dr., Prof., etc.
    public string? Specialty { get; set; }
    public string? SubSpecialty { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? RegistrationNumber { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }

    public bool IsActive { get; set; } = true;
    public bool CanPrescribe { get; set; } = true;
    public bool CanOrder { get; set; } = true;
    public bool CanRefer { get; set; } = true;

    public string? SignaturePath { get; set; }
    public string? PhotoPath { get; set; }
    public string? Bio { get; set; }

    public decimal? ConsultationFee { get; set; }
    public int? DefaultSlotDurationMinutes { get; set; } = 15;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Appointment>? Appointments { get; set; }
}
