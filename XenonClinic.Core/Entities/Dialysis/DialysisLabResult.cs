namespace XenonClinic.Core.Entities.Dialysis;

public class DialysisLabResult
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime LabDate { get; set; }
    public string? LabType { get; set; }
    public decimal? BUN { get; set; }
    public decimal? Creatinine { get; set; }
    public decimal? Potassium { get; set; }
    public decimal? Sodium { get; set; }
    public decimal? Chloride { get; set; }
    public decimal? Bicarbonate { get; set; }
    public decimal? Calcium { get; set; }
    public decimal? Phosphorus { get; set; }
    public decimal? Magnesium { get; set; }
    public decimal? Albumin { get; set; }
    public decimal? Hemoglobin { get; set; }
    public decimal? Hematocrit { get; set; }
    public decimal? WBC { get; set; }
    public decimal? Platelets { get; set; }
    public decimal? Ferritin { get; set; }
    public decimal? TSAT { get; set; }
    public decimal? Iron { get; set; }
    public decimal? TIBC { get; set; }
    public decimal? PTH { get; set; }
    public decimal? VitaminD { get; set; }
    public decimal? Kt_V { get; set; }
    public decimal? URR { get; set; }
    public decimal? HepatitisBSurfaceAntibody { get; set; }
    public bool? HepatitisBPositive { get; set; }
    public bool? HepatitisCPositive { get; set; }
    public bool? HIVPositive { get; set; }
    public string? Interpretation { get; set; }
    public string? ActionRequired { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
