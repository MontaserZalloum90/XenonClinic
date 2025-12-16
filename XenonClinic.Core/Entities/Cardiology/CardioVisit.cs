using XenonClinic.Core.Enums.Cardiology;

namespace XenonClinic.Core.Entities.Cardiology;

public class CardioVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public int? SystolicBP { get; set; }
    public int? DiastolicBP { get; set; }
    public int? HeartRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public string? HeartSounds { get; set; }
    public string? LungSounds { get; set; }
    public bool? JugularVenousDistension { get; set; }
    public bool? PeripheralEdema { get; set; }
    public string? EdemaLocation { get; set; }
    public int? EdemaGrade { get; set; }
    public string? ChestPainCharacter { get; set; }
    public bool? Dyspnea { get; set; }
    public bool? Orthopnea { get; set; }
    public bool? ParoxysmalNocturnalDyspnea { get; set; }
    public bool? Palpitations { get; set; }
    public bool? Syncope { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<ECGRecord> ECGRecords { get; set; } = new List<ECGRecord>();
    public ICollection<EchoResult> EchoResults { get; set; } = new List<EchoResult>();
    public ICollection<StressTest> StressTests { get; set; } = new List<StressTest>();
    public ICollection<CardiacProcedure> CardiacProcedures { get; set; } = new List<CardiacProcedure>();

    /// <summary>
    /// Alias for CardiacProcedures for compatibility
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public ICollection<CardiacProcedure> Procedures { get => CardiacProcedures; set => CardiacProcedures = value; }
}
