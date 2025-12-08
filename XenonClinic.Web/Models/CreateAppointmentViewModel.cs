using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for creating a new appointment
/// </summary>
public class CreateAppointmentViewModel
{
    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Display(Name = "Provider")]
    public int? ProviderId { get; set; }

    [Required(ErrorMessage = "Appointment date is required")]
    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Start time is required")]
    [Display(Name = "Start Time")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0); // 9:00 AM

    [Required(ErrorMessage = "Duration is required")]
    [Display(Name = "Duration (minutes)")]
    [Range(15, 480, ErrorMessage = "Duration must be between 15 minutes and 8 hours")]
    public int DurationMinutes { get; set; } = 30;

    [Required(ErrorMessage = "Appointment type is required")]
    [Display(Name = "Appointment Type")]
    public AppointmentType Type { get; set; } = AppointmentType.Consultation;

    [Display(Name = "Status")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public int BranchId { get; set; }

    // Computed properties
    public DateTime StartDateTime => AppointmentDate.Date.Add(StartTime);
    public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);
}
