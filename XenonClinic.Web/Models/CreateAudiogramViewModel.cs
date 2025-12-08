using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateAudiogramViewModel
{
    [Required]
    public int AudiologyVisitId { get; set; }

    // Left Ear - Air Conduction (dB HL)
    [Display(Name = "125 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_125 { get; set; }

    [Display(Name = "250 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_250 { get; set; }

    [Display(Name = "500 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_500 { get; set; }

    [Display(Name = "1000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_1000 { get; set; }

    [Display(Name = "2000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_2000 { get; set; }

    [Display(Name = "3000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_3000 { get; set; }

    [Display(Name = "4000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_4000 { get; set; }

    [Display(Name = "6000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_6000 { get; set; }

    [Display(Name = "8000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_AC_8000 { get; set; }

    // Left Ear - Bone Conduction (dB HL)
    [Display(Name = "250 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_250 { get; set; }

    [Display(Name = "500 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_500 { get; set; }

    [Display(Name = "1000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_1000 { get; set; }

    [Display(Name = "2000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_2000 { get; set; }

    [Display(Name = "3000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_3000 { get; set; }

    [Display(Name = "4000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? LeftEar_BC_4000 { get; set; }

    // Right Ear - Air Conduction (dB HL)
    [Display(Name = "125 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_125 { get; set; }

    [Display(Name = "250 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_250 { get; set; }

    [Display(Name = "500 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_500 { get; set; }

    [Display(Name = "1000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_1000 { get; set; }

    [Display(Name = "2000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_2000 { get; set; }

    [Display(Name = "3000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_3000 { get; set; }

    [Display(Name = "4000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_4000 { get; set; }

    [Display(Name = "6000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_6000 { get; set; }

    [Display(Name = "8000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_AC_8000 { get; set; }

    // Right Ear - Bone Conduction (dB HL)
    [Display(Name = "250 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_250 { get; set; }

    [Display(Name = "500 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_500 { get; set; }

    [Display(Name = "1000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_1000 { get; set; }

    [Display(Name = "2000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_2000 { get; set; }

    [Display(Name = "3000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_3000 { get; set; }

    [Display(Name = "4000 Hz")]
    [Range(-10, 120, ErrorMessage = "Value must be between -10 and 120 dB")]
    public int? RightEar_BC_4000 { get; set; }

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
