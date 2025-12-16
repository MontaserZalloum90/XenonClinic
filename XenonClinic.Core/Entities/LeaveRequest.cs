using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; } // User who approved/rejected
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? AttachmentPath { get; set; } // For medical certificates, etc.
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee Employee { get; set; } = null!;

    // Computed property
    public bool IsActive => Status == LeaveStatus.Approved &&
        StartDate <= DateTime.UtcNow.Date &&
        EndDate >= DateTime.UtcNow.Date;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
