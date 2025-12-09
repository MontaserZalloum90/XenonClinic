namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the status of a marketing lead
/// </summary>
public enum LeadStatus
{
    /// <summary>
    /// New lead, not yet contacted
    /// </summary>
    New = 0,

    /// <summary>
    /// Lead has been contacted
    /// </summary>
    Contacted = 1,

    /// <summary>
    /// Lead has shown interest and is being nurtured
    /// </summary>
    Qualified = 2,

    /// <summary>
    /// Lead is in active negotiation
    /// </summary>
    Negotiating = 3,

    /// <summary>
    /// Lead has been successfully converted to a customer
    /// </summary>
    Converted = 4,

    /// <summary>
    /// Lead was lost or disqualified
    /// </summary>
    Lost = 5,

    /// <summary>
    /// Lead has been archived
    /// </summary>
    Archived = 6
}
