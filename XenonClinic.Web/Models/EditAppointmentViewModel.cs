using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for editing an existing appointment
/// </summary>
public class EditAppointmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Appointment date is required")]
    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    [Display(Name = "Start Time")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Duration is required")]
    [Display(Name = "Duration (minutes)")]
    [Range(15, 480, ErrorMessage = "Duration must be between 15 minutes and 8 hours")]
    public int DurationMinutes { get; set; }

    [Required(ErrorMessage = "Appointment type is required")]
    [Display(Name = "Appointment Type")]
    public AppointmentType Type { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    public AppointmentStatus Status { get; set; }

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public int BranchId { get; set; }

    // Computed properties
    public DateTime StartDateTime => AppointmentDate.Date.Add(StartTime);
    public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);
}
