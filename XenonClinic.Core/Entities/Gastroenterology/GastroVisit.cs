using XenonClinic.Core.Enums.Gastroenterology;

namespace XenonClinic.Core.Entities.Gastroenterology;

public class GastroVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public bool? AbdominalPain { get; set; }
    public string? PainLocation { get; set; }
    public int? PainScale { get; set; }
    public bool? Nausea { get; set; }
    public bool? Vomiting { get; set; }
    public bool? Diarrhea { get; set; }
    public bool? Constipation { get; set; }
    public bool? Bloating { get; set; }
    public bool? Heartburn { get; set; }
    public bool? Dysphagia { get; set; }
    public bool? RectalBleeding { get; set; }
    public bool? Melena { get; set; }
    public bool? Hematemesis { get; set; }
    public bool? WeightLoss { get; set; }
    public decimal? WeightLossAmount { get; set; }
    public bool? AppetiteChange { get; set; }
    public bool? Jaundice { get; set; }
    public string? BowelHabits { get; set; }
    public string? AbdominalExam { get; set; }
    public string? LiverExam { get; set; }
    public string? SpleenExam { get; set; }
    public string? RectalExam { get; set; }
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
    public ICollection<EndoscopyRecord> Endoscopies { get; set; } = new List<EndoscopyRecord>();
    public ICollection<GastroProcedure> Procedures { get; set; } = new List<GastroProcedure>();
     public ICollection<LiverFunctionTest> LiverFunctionTests { get; set; } = new List<LiverFunctionTest>();
     public ICollection<EndoscopyRecord> EndoscopyRecords { get; set; } = new List<EndoscopyRecord>();
 }
