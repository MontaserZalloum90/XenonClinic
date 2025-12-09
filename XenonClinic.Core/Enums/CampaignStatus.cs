namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the status of a marketing campaign
/// </summary>
public enum CampaignStatus
{
    /// <summary>
    /// Campaign is in draft state, not yet active
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Campaign is scheduled to start in the future
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Campaign is currently active and running
    /// </summary>
    Active = 2,

    /// <summary>
    /// Campaign has been paused
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Campaign has been completed
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Campaign has been cancelled
    /// </summary>
    Cancelled = 5
}
