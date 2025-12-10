using XenonClinic.Core.Enums.Psychiatry;

namespace XenonClinic.Core.Entities.Psychiatry;

public class TherapySession
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? MentalHealthVisitId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime SessionDate { get; set; }
    public int SessionNumber { get; set; }
    public TherapyType TherapyType { get; set; }
    public string? TherapyModality { get; set; }
    public int DurationMinutes { get; set; }
    public string? SessionFormat { get; set; }
    public string? PresentingIssues { get; set; }
    public int? MoodRating { get; set; }
    public int? AnxietyRating { get; set; }
    public string? TopicsDiscussed { get; set; }
    public string? InterventionsUsed { get; set; }
    public string? ClientResponse { get; set; }
    public string? TherapistObservations { get; set; }
    public string? ProgressNotes { get; set; }
    public string? HomeworkAssigned { get; set; }
    public string? HomeworkReview { get; set; }
    public string? GoalsAddressed { get; set; }
    public string? BarriersIdentified { get; set; }
    public bool? RiskAssessmentCompleted { get; set; }
    public string? RiskLevel { get; set; }
    public string? SafetyConcerns { get; set; }
    public string? PlanForNextSession { get; set; }
    public string? Therapist { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public MentalHealthVisit? MentalHealthVisit { get; set; }
    public Appointment? Appointment { get; set; }
}
