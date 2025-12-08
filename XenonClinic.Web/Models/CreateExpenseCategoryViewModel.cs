using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateExpenseCategoryViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Category_Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Account")]
    public int? AccountId { get; set; }

    [Display(Name = "Is_Active")]
    public bool IsActive { get; set; } = true;
}
