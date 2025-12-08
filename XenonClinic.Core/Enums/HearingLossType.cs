namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the type of hearing loss
/// </summary>
public enum HearingLossType
{
    /// <summary>
    /// No hearing loss detected
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Sensorineural hearing loss (inner ear or nerve damage)
    /// </summary>
    Sensorineural = 1,

    /// <summary>
    /// Conductive hearing loss (outer or middle ear issues)
    /// </summary>
    Conductive = 2,

    /// <summary>
    /// Mixed hearing loss (both sensorineural and conductive)
    /// </summary>
    Mixed = 3
}
