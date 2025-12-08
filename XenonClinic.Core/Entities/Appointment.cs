using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.Consultation;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
