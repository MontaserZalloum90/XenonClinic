namespace XenonClinic.Core.Entities;

public class PerformanceReview
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty; // e.g., "Q1 2024", "2023"
    public string ReviewedBy { get; set; } = string.Empty; // Manager/HR name

    // Performance Ratings (1-5 scale)
    public int QualityOfWorkRating { get; set; }
    public int ProductivityRating { get; set; }
    public int PunctualityRating { get; set; }
    public int TeamworkRating { get; set; }
    public int CommunicationRating { get; set; }
    public int ProblemSolvingRating { get; set; }

    // Overall
    public decimal OverallRating { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string AreasForImprovement { get; set; } = string.Empty;
    public string Goals { get; set; } = string.Empty;
    public string? Comments { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Navigation properties
    public Employee Employee { get; set; } = null!;
}
