using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models.Account;

public class LoginViewModel
{
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    // Company context
    public int? CompanyId { get; set; }
    public string? CompanyCode { get; set; }
    public string? CompanyName { get; set; }

    // Authentication options
    public bool ShowLocalLogin { get; set; } = true;
    public bool ShowExternalLogin { get; set; }
    public bool ShowCompanySelector { get; set; }

    // Custom messages
    public string? LoginPageMessage { get; set; }
    public string? LoginPageMessageAr { get; set; }

    // External providers
    public IList<ExternalProviderViewModel> ExternalProviders { get; set; } = new List<ExternalProviderViewModel>();

    // Company selection
    public IList<CompanySelectionViewModel> AvailableCompanies { get; set; } = new List<CompanySelectionViewModel>();
}

public class ExternalProviderViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string IconClass { get; set; } = "bi bi-box-arrow-in-right";
    public string ButtonClass { get; set; } = "btn-outline-primary";
    public bool IsDefault { get; set; }
}

public class CompanySelectionViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
}

public class LoginWith2faViewModel
{
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Authenticator code")]
    public string? TwoFactorCode { get; set; }

    [Display(Name = "Remember this machine")]
    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
