using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Chiropractic;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Dermatology;
using XenonClinic.Core.Entities.Dialysis;
using XenonClinic.Core.Entities.ENT;
using XenonClinic.Core.Entities.Fertility;
using XenonClinic.Core.Entities.Gastroenterology;
using XenonClinic.Core.Entities.Gynecology;
using XenonClinic.Core.Entities.Neurology;
using XenonClinic.Core.Entities.Oncology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Orthopedics;
using XenonClinic.Core.Entities.PainManagement;
using XenonClinic.Core.Entities.Pediatrics;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Entities.Podiatry;
using XenonClinic.Core.Entities.Psychiatry;
using XenonClinic.Core.Entities.SleepMedicine;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class Patient : ISoftDelete, IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    /// <summary>
    /// First name - computed from FullNameEn for service compatibility
    /// </summary>
    public string FirstName => FullNameEn?.Split(' ').FirstOrDefault() ?? string.Empty;
    /// <summary>
    /// Last name - computed from FullNameEn for service compatibility
    /// </summary>
    public string LastName => FullNameEn?.Split(' ').Skip(1).FirstOrDefault() ??
                              (FullNameEn?.Split(' ').Length > 1 ? string.Join(" ", FullNameEn.Split(' ').Skip(1)) : string.Empty);
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HearingLossType { get; set; }
    public string? Notes { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? MRN { get; set; } // Medical Record Number
    public string? EncryptedSSN { get; set; } // Encrypted Social Security Number for compliance
    public bool IsActive { get; set; } = true;

    // Soft delete support for healthcare compliance
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public PatientMedicalHistory? MedicalHistory { get; set; }
    public ICollection<PatientDocument> Documents { get; set; } = new List<PatientDocument>();

    // Audiology (existing)
    public ICollection<AudiologyVisit> AudiologyVisits { get; set; } = new List<AudiologyVisit>();
    public ICollection<HearingDevice> HearingDevices { get; set; } = new List<HearingDevice>();

    // Dental
    public ICollection<DentalVisit> DentalVisits { get; set; } = new List<DentalVisit>();
    public ICollection<DentalTreatmentPlan> DentalTreatmentPlans { get; set; } = new List<DentalTreatmentPlan>();
    public ToothChart? ToothChart { get; set; }

    // Ophthalmology
    public ICollection<OphthalmologyVisit> OphthalmologyVisits { get; set; } = new List<OphthalmologyVisit>();
    public ICollection<EyePrescription> EyePrescriptions { get; set; } = new List<EyePrescription>();
    public ICollection<EyeCondition> EyeConditions { get; set; } = new List<EyeCondition>();

    // Dermatology
    public ICollection<DermatologyVisit> DermatologyVisits { get; set; } = new List<DermatologyVisit>();
    public ICollection<SkinCondition> SkinConditions { get; set; } = new List<SkinCondition>();
    public ICollection<LesionRecord> LesionRecords { get; set; } = new List<LesionRecord>();
    public ICollection<SkinTreatmentPlan> SkinTreatmentPlans { get; set; } = new List<SkinTreatmentPlan>();

    // Physiotherapy
    public ICollection<PhysioAssessment> PhysioAssessments { get; set; } = new List<PhysioAssessment>();
    public ICollection<PhysioSession> PhysioSessions { get; set; } = new List<PhysioSession>();
    public ICollection<ExerciseProgram> ExercisePrograms { get; set; } = new List<ExerciseProgram>();
    public ICollection<RangeOfMotionRecord> RangeOfMotionRecords { get; set; } = new List<RangeOfMotionRecord>();

    // Pediatrics
    public ICollection<PediatricVisit> PediatricVisits { get; set; } = new List<PediatricVisit>();
    public ICollection<GrowthRecord> GrowthRecords { get; set; } = new List<GrowthRecord>();
    public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    public ICollection<DevelopmentalMilestone> DevelopmentalMilestones { get; set; } = new List<DevelopmentalMilestone>();
    public NewbornRecord? NewbornRecord { get; set; }

    // Cardiology
    public ICollection<CardioVisit> CardioVisits { get; set; } = new List<CardioVisit>();
    public ICollection<ECGRecord> ECGRecords { get; set; } = new List<ECGRecord>();
    public ICollection<EchoResult> EchoResults { get; set; } = new List<EchoResult>();
    public ICollection<StressTest> StressTests { get; set; } = new List<StressTest>();
    public ICollection<CardiacProcedure> CardiacProcedures { get; set; } = new List<CardiacProcedure>();
    public ICollection<HeartCondition> HeartConditions { get; set; } = new List<HeartCondition>();

    // Orthopedics
    public ICollection<OrthoVisit> OrthoVisits { get; set; } = new List<OrthoVisit>();
    public ICollection<OrthoInjury> OrthoInjuries { get; set; } = new List<OrthoInjury>();
    public ICollection<OrthoProcedure> OrthoProcedures { get; set; } = new List<OrthoProcedure>();
    public ICollection<JointAssessment> JointAssessments { get; set; } = new List<JointAssessment>();
    public ICollection<CastRecord> CastRecords { get; set; } = new List<CastRecord>();
    public ICollection<OrthoImaging> OrthoImagings { get; set; } = new List<OrthoImaging>();

    // ENT
    public ICollection<ENTVisit> ENTVisits { get; set; } = new List<ENTVisit>();
    public ICollection<HearingScreening> HearingScreenings { get; set; } = new List<HearingScreening>();
    public ICollection<SinusAssessment> SinusAssessments { get; set; } = new List<SinusAssessment>();
    public ICollection<ThroatExam> ThroatExams { get; set; } = new List<ThroatExam>();
    public ICollection<ENTProcedure> ENTProcedures { get; set; } = new List<ENTProcedure>();
    public ICollection<AllergyTest> AllergyTests { get; set; } = new List<AllergyTest>();

    // Gynecology/Obstetrics
    public ICollection<GynVisit> GynVisits { get; set; } = new List<GynVisit>();
    public ICollection<PregnancyRecord> PregnancyRecords { get; set; } = new List<PregnancyRecord>();
    public ICollection<PrenatalVisit> PrenatalVisits { get; set; } = new List<PrenatalVisit>();
    public ICollection<ObUltrasound> ObUltrasounds { get; set; } = new List<ObUltrasound>();
    public ICollection<GynProcedure> GynProcedures { get; set; } = new List<GynProcedure>();
    public ICollection<PapSmearRecord> PapSmearRecords { get; set; } = new List<PapSmearRecord>();

    // Psychiatry/Psychology
    public ICollection<MentalHealthVisit> MentalHealthVisits { get; set; } = new List<MentalHealthVisit>();
    public ICollection<PsychAssessment> PsychAssessments { get; set; } = new List<PsychAssessment>();
    public ICollection<TherapySession> TherapySessions { get; set; } = new List<TherapySession>();
    public ICollection<PsychMedicationPlan> PsychMedicationPlans { get; set; } = new List<PsychMedicationPlan>();
    public ICollection<MoodRecord> MoodRecords { get; set; } = new List<MoodRecord>();
    public ICollection<TreatmentGoal> TreatmentGoals { get; set; } = new List<TreatmentGoal>();

    // Gastroenterology
    public ICollection<GastroVisit> GastroVisits { get; set; } = new List<GastroVisit>();
    public ICollection<EndoscopyRecord> EndoscopyRecords { get; set; } = new List<EndoscopyRecord>();
    public ICollection<LiverFunctionTest> LiverFunctionTests { get; set; } = new List<LiverFunctionTest>();
    public ICollection<GastroProcedure> GastroProcedures { get; set; } = new List<GastroProcedure>();
    public ICollection<DigestiveCondition> DigestiveConditions { get; set; } = new List<DigestiveCondition>();

    // Neurology
    public ICollection<NeuroVisit> NeuroVisits { get; set; } = new List<NeuroVisit>();
    public ICollection<NeuroExam> NeuroExams { get; set; } = new List<NeuroExam>();
    public ICollection<EEGRecord> EEGRecords { get; set; } = new List<EEGRecord>();
    public ICollection<NerveStudy> NerveStudies { get; set; } = new List<NerveStudy>();
    public ICollection<NeuroProcedure> NeuroProcedures { get; set; } = new List<NeuroProcedure>();
    public ICollection<NeuroCondition> NeuroConditions { get; set; } = new List<NeuroCondition>();

    // Fertility/IVF
    public ICollection<FertilityVisit> FertilityVisits { get; set; } = new List<FertilityVisit>();
    public ICollection<FertilityAssessment> FertilityAssessments { get; set; } = new List<FertilityAssessment>();
    public ICollection<IVFCycle> IVFCycles { get; set; } = new List<IVFCycle>();
    public ICollection<EmbryoRecord> EmbryoRecords { get; set; } = new List<EmbryoRecord>();
    public ICollection<HormoneLevel> HormoneLevels { get; set; } = new List<HormoneLevel>();
    public ICollection<SpermAnalysis> SpermAnalyses { get; set; } = new List<SpermAnalysis>();

    // Pain Management
    public ICollection<PainVisit> PainVisits { get; set; } = new List<PainVisit>();
    public ICollection<PainAssessment> PainAssessments { get; set; } = new List<PainAssessment>();
    public ICollection<PainProcedure> PainProcedures { get; set; } = new List<PainProcedure>();
    public ICollection<PainMedicationRegimen> PainMedicationRegimens { get; set; } = new List<PainMedicationRegimen>();
    public ICollection<PainScore> PainScores { get; set; } = new List<PainScore>();
    public ICollection<TriggerPointRecord> TriggerPointRecords { get; set; } = new List<TriggerPointRecord>();

    // Sleep Medicine
    public ICollection<SleepVisit> SleepVisits { get; set; } = new List<SleepVisit>();
    public ICollection<SleepStudy> SleepStudies { get; set; } = new List<SleepStudy>();
    public ICollection<SleepDiary> SleepDiaries { get; set; } = new List<SleepDiary>();
    public ICollection<CPAPRecord> CPAPRecords { get; set; } = new List<CPAPRecord>();
    public ICollection<SleepDisorder> SleepDisorders { get; set; } = new List<SleepDisorder>();

    // Dialysis
    public ICollection<DialysisPatientRecord> DialysisPatientRecords { get; set; } = new List<DialysisPatientRecord>();
    public ICollection<DialysisSession> DialysisSessions { get; set; } = new List<DialysisSession>();
    public ICollection<DialysisAccessRecord> DialysisAccessRecords { get; set; } = new List<DialysisAccessRecord>();
    public ICollection<DialysisLabResult> DialysisLabResults { get; set; } = new List<DialysisLabResult>();
    public ICollection<FluidBalance> FluidBalances { get; set; } = new List<FluidBalance>();

    // Oncology
    public ICollection<OncologyVisit> OncologyVisits { get; set; } = new List<OncologyVisit>();
    public ICollection<CancerDiagnosis> CancerDiagnoses { get; set; } = new List<CancerDiagnosis>();
    public ICollection<ChemotherapySession> ChemotherapySessions { get; set; } = new List<ChemotherapySession>();
    public ICollection<RadiationRecord> RadiationRecords { get; set; } = new List<RadiationRecord>();
    public ICollection<TumorMarker> TumorMarkers { get; set; } = new List<TumorMarker>();
    public ICollection<OncologyTreatmentPlan> OncologyTreatmentPlans { get; set; } = new List<OncologyTreatmentPlan>();

    // Chiropractic
    public ICollection<ChiroVisit> ChiroVisits { get; set; } = new List<ChiroVisit>();
    public ICollection<SpinalAssessment> SpinalAssessments { get; set; } = new List<SpinalAssessment>();
    public ICollection<ChiroAdjustment> ChiroAdjustments { get; set; } = new List<ChiroAdjustment>();
    public ICollection<PostureAnalysis> PostureAnalyses { get; set; } = new List<PostureAnalysis>();
    public ICollection<ChiroXRayFinding> ChiroXRayFindings { get; set; } = new List<ChiroXRayFinding>();
    public ICollection<ChiroTreatmentPlan> ChiroTreatmentPlans { get; set; } = new List<ChiroTreatmentPlan>();

    // Podiatry
    public ICollection<PodiatryVisit> PodiatryVisits { get; set; } = new List<PodiatryVisit>();
    public ICollection<FootAssessment> FootAssessments { get; set; } = new List<FootAssessment>();
    public ICollection<PodiatryGaitAnalysis> PodiatryGaitAnalyses { get; set; } = new List<PodiatryGaitAnalysis>();
    public ICollection<PodiatryProcedure> PodiatryProcedures { get; set; } = new List<PodiatryProcedure>();
    public ICollection<OrthoticPrescription> OrthoticPrescriptions { get; set; } = new List<OrthoticPrescription>();
    public ICollection<FootCondition> FootConditions { get; set; } = new List<FootCondition>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
