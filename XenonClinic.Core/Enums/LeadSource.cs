namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the source of a marketing lead
/// </summary>
public enum LeadSource
{
    /// <summary>
    /// Lead from website inquiry
    /// </summary>
    Website = 0,

    /// <summary>
    /// Lead from referral
    /// </summary>
    Referral = 1,

    /// <summary>
    /// Lead from social media
    /// </summary>
    SocialMedia = 2,

    /// <summary>
    /// Lead from advertising
    /// </summary>
    Advertising = 3,

    /// <summary>
    /// Lead from event attendance
    /// </summary>
    Event = 4,

    /// <summary>
    /// Walk-in lead
    /// </summary>
    WalkIn = 5,

    /// <summary>
    /// Lead from phone inquiry
    /// </summary>
    Phone = 6,

    /// <summary>
    /// Lead from email inquiry
    /// </summary>
    Email = 7,

    /// <summary>
    /// Lead from partner organization
    /// </summary>
    Partner = 8,

    /// <summary>
    /// Lead from marketing campaign
    /// </summary>
    Campaign = 9,

    /// <summary>
    /// Other source
    /// </summary>
    Other = 10
}
