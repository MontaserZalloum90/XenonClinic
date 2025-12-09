using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Dermatology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Pediatrics;
using XenonClinic.Core.Entities.Physiotherapy;

namespace XenonClinic.Core.Entities;

public class Patient
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HearingLossType { get; set; }
    public string? Notes { get; set; }

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
}
