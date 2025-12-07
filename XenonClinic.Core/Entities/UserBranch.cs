namespace XenonClinic.Core.Entities;

public class UserBranch
{
    public string UserId { get; set; } = string.Empty;
    public int BranchId { get; set; }

    public ApplicationUser? User { get; set; }
    public Branch? Branch { get; set; }
}
