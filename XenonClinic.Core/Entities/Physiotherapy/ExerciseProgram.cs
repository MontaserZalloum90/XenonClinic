namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents a prescribed exercise program for a patient
/// </summary>
public class ExerciseProgram
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PhysioAssessmentId { get; set; }

    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Program type (Strengthening, Stretching, Balance, Cardio, Functional, Combined)
    /// </summary>
    public string ProgramType { get; set; } = "Combined";

    /// <summary>
    /// Target area (e.g., Lower Back, Shoulder, Knee, Core)
    /// </summary>
    public string? TargetArea { get; set; }

    /// <summary>
    /// Status (Active, Completed, Modified, Discontinued)
    /// </summary>
    public string Status { get; set; } = "Active";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Difficulty level (Beginner, Intermediate, Advanced)
    /// </summary>
    public string DifficultyLevel { get; set; } = "Beginner";

    /// <summary>
    /// Recommended frequency per week
    /// </summary>
    public int? FrequencyPerWeek { get; set; }

    /// <summary>
    /// Duration of each session in minutes
    /// </summary>
    public int? SessionDurationMinutes { get; set; }

    /// <summary>
    /// General instructions for the program
    /// </summary>
    public string? GeneralInstructions { get; set; }

    /// <summary>
    /// Precautions and contraindications
    /// </summary>
    public string? Precautions { get; set; }

    /// <summary>
    /// Progression criteria
    /// </summary>
    public string? ProgressionCriteria { get; set; }

    public string? PrescribedById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PhysioAssessment? Assessment { get; set; }
    public ICollection<ExerciseProgramItem> Exercises { get; set; } = new List<ExerciseProgramItem>();
    public ICollection<PhysioSession> Sessions { get; set; } = new List<PhysioSession>();
}
