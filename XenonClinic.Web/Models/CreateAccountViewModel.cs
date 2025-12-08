using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateAccountViewModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Account_Code")]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Account_Name")]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Account_Type")]
    public AccountType AccountType { get; set; }

    [Display(Name = "Parent_Account")]
    public int? ParentAccountId { get; set; }

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Opening_Balance")]
    public decimal Balance { get; set; }

    [Display(Name = "Is_Active")]
    public bool IsActive { get; set; } = true;
}
