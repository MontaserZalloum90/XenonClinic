using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateLeaveRequestViewModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required] [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
