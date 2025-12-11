using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class Appointment : IBranchEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ProviderId { get; set; } // Optional: Healthcare provider/employee assigned to this appointment
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.Consultation;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Employee? Provider { get; set; }
}
