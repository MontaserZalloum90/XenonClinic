namespace XenonClinic.Core.Entities.Gastroenterology;

public class LiverFunctionTest
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? GastroVisitId { get; set; }
    public DateTime TestDate { get; set; }
    public decimal? ALT { get; set; }
    public decimal? AST { get; set; }
    public decimal? ALP { get; set; }
    public decimal? GGT { get; set; }
    public decimal? TotalBilirubin { get; set; }
    public decimal? DirectBilirubin { get; set; }
    public decimal? IndirectBilirubin { get; set; }
    public decimal? Albumin { get; set; }
    public decimal? TotalProtein { get; set; }
    public decimal? PT { get; set; }
    public decimal? INR { get; set; }
    public decimal? Ammonia { get; set; }
    public decimal? AFP { get; set; }
    public bool? HepatitisBSurfaceAg { get; set; }
    public bool? HepatitisBCoreAb { get; set; }
    public bool? HepatitisCAbPositive { get; set; }
    public decimal? HepatitisCViralLoad { get; set; }
    public string? FibroScanResult { get; set; }
    public decimal? FibroScanKPa { get; set; }
    public string? Interpretation { get; set; }
    public string? ChildPughScore { get; set; }
    public int? MELDScore { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public GastroVisit? GastroVisit { get; set; }
}
