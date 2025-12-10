namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents an individual exercise within a program
/// </summary>
public class ExerciseProgramItem
{
    public int Id { get; set; }
    public int ExerciseProgramId { get; set; }

    /// <summary>
    /// Order in the program sequence
    /// </summary>
    public int Sequence { get; set; }

    public string ExerciseName { get; set; } = string.Empty;

    /// <summary>
    /// Category (Warmup, Strengthening, Stretching, Balance, Cardio, Cooldown)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Target muscle/body part
    /// </summary>
    public string? TargetMuscle { get; set; }

    /// <summary>
    /// Number of sets
    /// </summary>
    public int? Sets { get; set; }

    /// <summary>
    /// Number of repetitions per set
    /// </summary>
    public int? Repetitions { get; set; }

    /// <summary>
    /// Hold duration in seconds (for static exercises)
    /// </summary>
    public int? HoldSeconds { get; set; }

    /// <summary>
    /// Duration in minutes (for timed exercises)
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Rest period between sets in seconds
    /// </summary>
    public int? RestSeconds { get; set; }

    /// <summary>
    /// Resistance/weight used
    /// </summary>
    public string? Resistance { get; set; }

    /// <summary>
    /// Equipment needed
    /// </summary>
    public string? Equipment { get; set; }

    /// <summary>
    /// Starting position description
    /// </summary>
    public string? StartingPosition { get; set; }

    /// <summary>
    /// Exercise instructions
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Common mistakes to avoid
    /// </summary>
    public string? CommonMistakes { get; set; }

    /// <summary>
    /// Modifications for easier version
    /// </summary>
    public string? Modifications { get; set; }

    /// <summary>
    /// Progression for harder version
    /// </summary>
    public string? Progression { get; set; }

    /// <summary>
    /// Instructional image/diagram path
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Video demonstration link
    /// </summary>
    public string? VideoUrl { get; set; }

    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ExerciseProgram? Program { get; set; }
}
