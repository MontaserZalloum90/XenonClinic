namespace XenonClinic.Core.Enums;

/// <summary>
/// Status of insurance pre-authorization request
/// </summary>
public enum PreAuthStatus
{
    Draft = 1,
    Submitted = 2,
    InReview = 3,
    Approved = 4,
    PartiallyApproved = 5,
    Denied = 6,
    Expired = 7,
    Cancelled = 8,
    PendingInfo = 9
}
