namespace XenonClinic.Core.Entities;

/// <summary>
/// Junction table linking users to branches.
/// Note: The User navigation property is defined in Infrastructure (ApplicationUser entity).
/// </summary>
public class UserBranch
{
    public string UserId { get; set; } = string.Empty;
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
}
