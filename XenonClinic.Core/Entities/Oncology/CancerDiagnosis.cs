using XenonClinic.Core.Enums.Oncology;

namespace XenonClinic.Core.Entities.Oncology;

public class CancerDiagnosis
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public CancerType CancerType { get; set; }
    public string? CancerSubtype { get; set; }
    public string? HistologicType { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public string? PrimarySite { get; set; }
    public string? Laterality { get; set; }
    public string? Grade { get; set; }
    public string? Stage { get; set; }
    public string? TNMStaging { get; set; }
    public string? T_Stage { get; set; }
    public string? N_Stage { get; set; }
    public string? M_Stage { get; set; }
    public string? BiomarkerStatus { get; set; }
    public bool? ERPositive { get; set; }
    public bool? PRPositive { get; set; }
    public bool? HER2Positive { get; set; }
    public string? GeneticMutations { get; set; }
    public string? MolecularProfile { get; set; }
    public string? DiagnosticProcedures { get; set; }
    public string? PathologyReport { get; set; }
    public CancerStatus Status { get; set; }
    public DateTime? RemissionDate { get; set; }
    public DateTime? RecurrenceDate { get; set; }
    public string? RecurrenceSite { get; set; }
    public string? MetastaticSites { get; set; }
    public string? TreatmentHistory { get; set; }
    public string? Prognosis { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public OncologyTreatmentPlan? TreatmentPlan { get; set; }
}
