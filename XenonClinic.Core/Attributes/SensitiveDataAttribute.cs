namespace XenonClinic.Core.Attributes;

/// <summary>
/// Marks a property as containing sensitive data that should be encrypted at rest.
/// Properties marked with this attribute should be encrypted before storage
/// and decrypted when retrieved.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Indicates if this field should be masked in logs and error messages
    /// </summary>
    public bool MaskInLogs { get; set; } = true;

    /// <summary>
    /// The type of sensitive data (for audit purposes)
    /// </summary>
    public SensitiveDataType DataType { get; set; } = SensitiveDataType.Credential;
}

/// <summary>
/// Types of sensitive data for classification
/// </summary>
public enum SensitiveDataType
{
    Credential,
    ApiKey,
    Token,
    PersonalIdentifier,
    FinancialData,
    HealthData
}
