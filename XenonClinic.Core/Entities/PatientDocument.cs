namespace XenonClinic.Core.Entities;

public class PatientDocument
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    // Document Information
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // ConsentForm, MedicalRecord, IDCopy, InsuranceCard, LabReport, Prescription, Other
    public string? Description { get; set; }

    // File Information
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    // Metadata
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Tags for search
    public string? Tags { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
}
