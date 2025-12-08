using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class EditAudiologyVisitViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    public int BranchId { get; set; }

    [Required(ErrorMessage = "Visit date is required")]
    [Display(Name = "Visit Date")]
    [DataType(DataType.Date)]
    public DateTime VisitDate { get; set; }

    [Display(Name = "Chief Complaint")]
    [StringLength(2000, ErrorMessage = "Chief complaint cannot exceed 2000 characters")]
    public string? ChiefComplaint { get; set; }

    [Display(Name = "Diagnosis")]
    [StringLength(2000, ErrorMessage = "Diagnosis cannot exceed 2000 characters")]
    public string? Diagnosis { get; set; }

    [Display(Name = "Treatment Plan")]
    [StringLength(2000, ErrorMessage = "Treatment plan cannot exceed 2000 characters")]
    public string? Plan { get; set; }
}
