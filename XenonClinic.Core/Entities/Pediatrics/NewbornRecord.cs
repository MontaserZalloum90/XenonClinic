namespace XenonClinic.Core.Entities.Pediatrics;

/// <summary>
/// Represents newborn-specific medical information
/// </summary>
public class NewbornRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    // Birth Information
    public DateTime DateOfBirth { get; set; }
    public TimeSpan? TimeOfBirth { get; set; }

    /// <summary>
    /// Gestational age in weeks at birth
    /// </summary>
    public int? GestationalAgeWeeks { get; set; }

    /// <summary>
    /// Birth weight in kg
    /// </summary>
    public decimal? BirthWeight { get; set; }

    /// <summary>
    /// Birth length in cm
    /// </summary>
    public decimal? BirthLength { get; set; }

    /// <summary>
    /// Birth head circumference in cm
    /// </summary>
    public decimal? BirthHeadCircumference { get; set; }

    /// <summary>
    /// Delivery type (Vaginal, C-Section, VBAC, Assisted)
    /// </summary>
    public string? DeliveryType { get; set; }

    /// <summary>
    /// Delivery complications
    /// </summary>
    public string? DeliveryComplications { get; set; }

    /// <summary>
    /// APGAR score at 1 minute
    /// </summary>
    public int? ApgarScore1Min { get; set; }

    /// <summary>
    /// APGAR score at 5 minutes
    /// </summary>
    public int? ApgarScore5Min { get; set; }

    /// <summary>
    /// APGAR score at 10 minutes (if needed)
    /// </summary>
    public int? ApgarScore10Min { get; set; }

    /// <summary>
    /// Resuscitation needed
    /// </summary>
    public bool ResuscitationNeeded { get; set; }

    /// <summary>
    /// Resuscitation details
    /// </summary>
    public string? ResuscitationDetails { get; set; }

    // Maternal Information
    /// <summary>
    /// Maternal age at delivery
    /// </summary>
    public int? MaternalAge { get; set; }

    /// <summary>
    /// Maternal blood type
    /// </summary>
    public string? MaternalBloodType { get; set; }

    /// <summary>
    /// GBS (Group B Strep) status
    /// </summary>
    public string? GbsStatus { get; set; }

    /// <summary>
    /// Pregnancy complications
    /// </summary>
    public string? PregnancyComplications { get; set; }

    /// <summary>
    /// Maternal medications during pregnancy
    /// </summary>
    public string? MaternalMedications { get; set; }

    // Newborn Screening
    /// <summary>
    /// Hearing screening result (Pass, Refer, Not Done)
    /// </summary>
    public string? HearingScreeningResult { get; set; }
    public DateTime? HearingScreeningDate { get; set; }

    /// <summary>
    /// Critical congenital heart disease screening result
    /// </summary>
    public string? CchdScreeningResult { get; set; }
    public DateTime? CchdScreeningDate { get; set; }

    /// <summary>
    /// Metabolic screening status
    /// </summary>
    public string? MetabolicScreeningStatus { get; set; }
    public DateTime? MetabolicScreeningDate { get; set; }

    /// <summary>
    /// Bilirubin level
    /// </summary>
    public decimal? BilirubinLevel { get; set; }

    /// <summary>
    /// Blood type
    /// </summary>
    public string? BloodType { get; set; }

    /// <summary>
    /// Coombs test result
    /// </summary>
    public string? CoombsTestResult { get; set; }

    // Feeding
    /// <summary>
    /// Initial feeding type (Breast, Formula, Both)
    /// </summary>
    public string? InitialFeedingType { get; set; }

    /// <summary>
    /// Breastfeeding initiated
    /// </summary>
    public bool BreastfeedingInitiated { get; set; }

    /// <summary>
    /// Feeding problems
    /// </summary>
    public string? FeedingProblems { get; set; }

    // Hospital Stay
    /// <summary>
    /// NICU admission
    /// </summary>
    public bool NicuAdmission { get; set; }

    /// <summary>
    /// NICU admission reason
    /// </summary>
    public string? NicuAdmissionReason { get; set; }

    /// <summary>
    /// Days in NICU
    /// </summary>
    public int? DaysInNicu { get; set; }

    /// <summary>
    /// Hospital discharge date
    /// </summary>
    public DateTime? DischargeDate { get; set; }

    /// <summary>
    /// Discharge weight in kg
    /// </summary>
    public decimal? DischargeWeight { get; set; }

    /// <summary>
    /// Discharge instructions
    /// </summary>
    public string? DischargeInstructions { get; set; }

    /// <summary>
    /// Vitamin K given
    /// </summary>
    public bool VitaminKGiven { get; set; }

    /// <summary>
    /// Hepatitis B vaccine given at birth
    /// </summary>
    public bool HepBVaccineGiven { get; set; }

    /// <summary>
    /// Eye prophylaxis given
    /// </summary>
    public bool EyeProphylaxisGiven { get; set; }

    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
