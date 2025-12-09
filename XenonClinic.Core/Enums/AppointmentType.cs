namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the type of appointment across all clinic types
/// </summary>
public enum AppointmentType
{
    // ========================================
    // General/Common Types
    // ========================================

    /// <summary>
    /// General consultation
    /// </summary>
    Consultation = 0,

    /// <summary>
    /// Follow-up appointment
    /// </summary>
    FollowUp = 1,

    /// <summary>
    /// Emergency appointment
    /// </summary>
    Emergency = 2,

    /// <summary>
    /// New patient appointment
    /// </summary>
    NewPatient = 3,

    // ========================================
    // Audiology Types
    // ========================================

    /// <summary>
    /// Hearing test appointment
    /// </summary>
    HearingTest = 10,

    /// <summary>
    /// Hearing aid fitting appointment
    /// </summary>
    HearingAidFitting = 11,

    /// <summary>
    /// Hearing device repair appointment
    /// </summary>
    HearingDeviceRepair = 12,

    /// <summary>
    /// Audiogram review
    /// </summary>
    AudiogramReview = 13,

    // ========================================
    // Dental Types
    // ========================================

    /// <summary>
    /// Dental checkup/cleaning
    /// </summary>
    DentalCheckup = 20,

    /// <summary>
    /// Dental procedure
    /// </summary>
    DentalProcedure = 21,

    /// <summary>
    /// Orthodontic appointment
    /// </summary>
    Orthodontic = 22,

    /// <summary>
    /// Dental X-Ray
    /// </summary>
    DentalXRay = 23,

    /// <summary>
    /// Root canal treatment
    /// </summary>
    RootCanal = 24,

    /// <summary>
    /// Tooth extraction
    /// </summary>
    Extraction = 25,

    // ========================================
    // Veterinary Types
    // ========================================

    /// <summary>
    /// Pet wellness checkup
    /// </summary>
    PetWellness = 30,

    /// <summary>
    /// Pet vaccination
    /// </summary>
    PetVaccination = 31,

    /// <summary>
    /// Pet grooming
    /// </summary>
    Grooming = 32,

    /// <summary>
    /// Pet boarding check-in
    /// </summary>
    BoardingCheckIn = 33,

    /// <summary>
    /// Pet boarding check-out
    /// </summary>
    BoardingCheckOut = 34,

    /// <summary>
    /// Pet surgery
    /// </summary>
    PetSurgery = 35,

    // ========================================
    // Ophthalmology Types
    // ========================================

    /// <summary>
    /// Eye exam/vision test
    /// </summary>
    EyeExam = 40,

    /// <summary>
    /// Contact lens fitting
    /// </summary>
    ContactLensFitting = 41,

    /// <summary>
    /// Glaucoma screening
    /// </summary>
    GlaucomaScreening = 42,

    /// <summary>
    /// Cataract evaluation
    /// </summary>
    CataractEvaluation = 43,

    /// <summary>
    /// Retinal exam
    /// </summary>
    RetinalExam = 44,

    /// <summary>
    /// LASIK consultation
    /// </summary>
    LasikConsultation = 45,

    // ========================================
    // Dermatology Types
    // ========================================

    /// <summary>
    /// Skin consultation
    /// </summary>
    SkinConsultation = 50,

    /// <summary>
    /// Skin biopsy
    /// </summary>
    SkinBiopsy = 51,

    /// <summary>
    /// Mole check/skin cancer screening
    /// </summary>
    MoleCheck = 52,

    /// <summary>
    /// Cosmetic procedure
    /// </summary>
    CosmeticProcedure = 53,

    /// <summary>
    /// Laser treatment
    /// </summary>
    LaserTreatment = 54,

    /// <summary>
    /// Acne treatment
    /// </summary>
    AcneTreatment = 55,

    // ========================================
    // Physiotherapy Types
    // ========================================

    /// <summary>
    /// Initial physiotherapy assessment
    /// </summary>
    PhysioAssessment = 60,

    /// <summary>
    /// Physiotherapy treatment session
    /// </summary>
    PhysioSession = 61,

    /// <summary>
    /// Post-operative rehabilitation
    /// </summary>
    PostOpRehab = 62,

    /// <summary>
    /// Sports injury treatment
    /// </summary>
    SportsInjury = 63,

    /// <summary>
    /// Manual therapy session
    /// </summary>
    ManualTherapy = 64,

    // ========================================
    // Pediatrics Types
    // ========================================

    /// <summary>
    /// Well-child visit
    /// </summary>
    WellChildVisit = 70,

    /// <summary>
    /// Sick child visit
    /// </summary>
    SickChildVisit = 71,

    /// <summary>
    /// Child vaccination
    /// </summary>
    ChildVaccination = 72,

    /// <summary>
    /// Developmental screening
    /// </summary>
    DevelopmentalScreening = 73,

    /// <summary>
    /// Newborn checkup
    /// </summary>
    NewbornCheckup = 74,

    /// <summary>
    /// School/sports physical
    /// </summary>
    SchoolPhysical = 75
}
