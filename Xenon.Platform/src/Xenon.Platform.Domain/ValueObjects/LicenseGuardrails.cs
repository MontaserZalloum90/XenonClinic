namespace Xenon.Platform.Domain.ValueObjects;

public record LicenseGuardrails
{
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }

    public bool CanAddBranch => CurrentBranches < MaxBranches;
    public bool CanAddUser => CurrentUsers < MaxUsers;

    public int RemainingBranches => Math.Max(0, MaxBranches - CurrentBranches);
    public int RemainingUsers => Math.Max(0, MaxUsers - CurrentUsers);

    public double BranchUsagePercent => MaxBranches > 0 ? (double)CurrentBranches / MaxBranches * 100 : 0;
    public double UserUsagePercent => MaxUsers > 0 ? (double)CurrentUsers / MaxUsers * 100 : 0;
}
