namespace XenonClinic.Core.Enums.Veterinary;

/// <summary>
/// Status of a boarding reservation
/// </summary>
public enum BoardingStatus
{
    Reserved = 0,
    CheckedIn = 1,
    CheckedOut = 2,
    Cancelled = 3,
    NoShow = 4
}
