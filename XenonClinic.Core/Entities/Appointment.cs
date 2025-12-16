using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class Appointment : IBranchEntity
{
    public int Id { get; set; }
    public string? ReferenceNumber { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ProviderId { get; set; } // Optional: Healthcare provider/employee assigned to this appointment
    public int? DoctorId { get; set; } // Optional: Doctor assigned to this appointment
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime AppointmentDate { get; set; } // Date of the appointment
    public Enums.AppointmentType Type { get; set; } = Enums.AppointmentType.Consultation;
    public string? AppointmentType { get; set; } // String representation for compatibility
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public string? Reason { get; set; }
    public string? Location { get; set; }
    public bool IsTelemedicine { get; set; }
    public DateTime? RescheduledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool CancelledByPatient { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Employee? Provider { get; set; }
    public Doctor? Doctor { get; set; }
}
