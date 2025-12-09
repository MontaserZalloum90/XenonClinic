namespace XenonClinic.Core.Enums.Dental;

/// <summary>
/// Status of a dental treatment plan
/// </summary>
public enum DentalTreatmentPlanStatus
{
    Draft = 0,
    Proposed = 1,
    Accepted = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}
