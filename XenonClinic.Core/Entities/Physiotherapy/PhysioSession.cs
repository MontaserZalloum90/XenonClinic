namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents a physiotherapy treatment session
/// </summary>
public class PhysioSession
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? ExerciseProgramId { get; set; }
    public DateTime SessionDate { get; set; }

    /// <summary>
    /// Session number in the treatment course
    /// </summary>
    public int SessionNumber { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Status (Scheduled, Completed, Cancelled, NoShow)
    /// </summary>
    public string Status { get; set; } = "Completed";

    /// <summary>
    /// Pain level before session (0-10)
    /// </summary>
    public int? PainLevelBefore { get; set; }

    /// <summary>
    /// Pain level after session (0-10)
    /// </summary>
    public int? PainLevelAfter { get; set; }

    /// <summary>
    /// Subjective report from patient
    /// </summary>
    public string? SubjectiveReport { get; set; }

    /// <summary>
    /// Objective findings
    /// </summary>
    public string? ObjectiveFindings { get; set; }

    /// <summary>
    /// Modalities used (e.g., Ultrasound, TENS, Heat, Ice, Traction)
    /// </summary>
    public string? ModalitiesUsed { get; set; }

    /// <summary>
    /// Manual therapy techniques applied
    /// </summary>
    public string? ManualTherapy { get; set; }

    /// <summary>
    /// Therapeutic exercises performed
    /// </summary>
    public string? ExercisesPerformed { get; set; }

    /// <summary>
    /// Patient response to treatment
    /// </summary>
    public string? PatientResponse { get; set; }

    /// <summary>
    /// Progress toward goals
    /// </summary>
    public string? ProgressNotes { get; set; }

    /// <summary>
    /// Home exercise compliance (Excellent, Good, Fair, Poor)
    /// </summary>
    public string? HomeExerciseCompliance { get; set; }

    /// <summary>
    /// Plan for next session
    /// </summary>
    public string? PlanForNext { get; set; }

    /// <summary>
    /// Home exercise instructions given
    /// </summary>
    public string? HomeInstructions { get; set; }

    public string? PhysiotherapistId { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ExerciseProgram? ExerciseProgram { get; set; }
}
