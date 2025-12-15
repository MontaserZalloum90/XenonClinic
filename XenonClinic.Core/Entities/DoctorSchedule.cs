using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Doctor availability and scheduling
/// </summary>
public class DoctorSchedule : IBranchEntity
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Schedule timing
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Slot configuration
    public int SlotDurationMinutes { get; set; } = 30;
    public int MaxPatientsPerSlot { get; set; } = 1;
    public int BufferMinutes { get; set; } = 5; // Buffer between appointments

    // Schedule type
    public string ScheduleType { get; set; } = "Regular"; // Regular, OnCall, Emergency
    public bool IsActive { get; set; } = true;

    // Override dates (for holidays, vacations, etc.)
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }

    // Room/Location
    public string? RoomNumber { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
