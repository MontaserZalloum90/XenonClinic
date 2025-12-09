namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the type of marketing campaign
/// </summary>
public enum CampaignType
{
    /// <summary>
    /// Email marketing campaign
    /// </summary>
    Email = 0,

    /// <summary>
    /// Social media marketing campaign
    /// </summary>
    Social = 1,

    /// <summary>
    /// SMS/Text message campaign
    /// </summary>
    SMS = 2,

    /// <summary>
    /// Promotional event or webinar
    /// </summary>
    Event = 3,

    /// <summary>
    /// Referral program campaign
    /// </summary>
    Referral = 4,

    /// <summary>
    /// Patient recall/retention campaign
    /// </summary>
    Recall = 5,

    /// <summary>
    /// Seasonal promotion
    /// </summary>
    Seasonal = 6,

    /// <summary>
    /// Digital advertising (Google, Facebook ads, etc.)
    /// </summary>
    Digital = 7,

    /// <summary>
    /// Print/traditional media campaign
    /// </summary>
    Print = 8,

    /// <summary>
    /// Other type of campaign
    /// </summary>
    Other = 9
}
