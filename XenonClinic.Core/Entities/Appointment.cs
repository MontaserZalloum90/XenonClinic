namespace XenonClinic.Core.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Type { get; set; } = "Consultation";
    public string Status { get; set; } = "Booked";
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
