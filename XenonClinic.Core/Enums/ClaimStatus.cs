namespace XenonClinic.Core.Enums;

/// <summary>
/// Status of an insurance claim
/// </summary>
public enum ClaimStatus
{
    Draft = 1,
    Submitted = 2,
    Acknowledged = 3,
    InReview = 4,
    Pending = 5,
    Approved = 6,
    PartiallyApproved = 7,
    Denied = 8,
    Appealed = 9,
    Paid = 10,
    PartiallyPaid = 11,
    Voided = 12,
    Resubmitted = 13
}
