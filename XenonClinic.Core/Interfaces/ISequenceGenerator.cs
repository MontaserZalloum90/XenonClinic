namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for generating sequential numbers/codes for various entities.
/// Centralizes sequence generation logic to eliminate code duplication.
/// </summary>
public interface ISequenceGenerator
{
    /// <summary>
    /// Generates a sequential number with a prefix based on date.
    /// Format: {prefix}-{datePattern}-{sequenceNumber}
    /// Example: LAB-20241209-0001
    /// </summary>
    /// <param name="branchId">Branch ID for scoping the sequence</param>
    /// <param name="prefix">Prefix for the sequence (e.g., "LAB", "INV", "RAD")</param>
    /// <param name="sequenceType">Type of sequence for determining the entity table</param>
    /// <param name="dateFormat">Date format pattern (default: "yyyyMMdd")</param>
    /// <param name="numberWidth">Width of the sequence number with leading zeros (default: 4)</param>
    /// <returns>The generated sequence string</returns>
    Task<string> GenerateSequenceAsync(
        int branchId,
        string prefix,
        SequenceType sequenceType,
        string dateFormat = "yyyyMMdd",
        int numberWidth = 4);

    /// <summary>
    /// Generates a lab order number.
    /// Format: LAB-{yyyyMMdd}-{0001}
    /// </summary>
    Task<string> GenerateLabOrderNumberAsync(int branchId);

    /// <summary>
    /// Generates a radiology order number.
    /// Format: RAD-{yyyyMMdd}-{0001}
    /// </summary>
    Task<string> GenerateRadiologyOrderNumberAsync(int branchId);

    /// <summary>
    /// Generates an invoice number.
    /// Format: INV-{yyyyMMdd}-{0001}
    /// </summary>
    Task<string> GenerateInvoiceNumberAsync(int branchId);

    /// <summary>
    /// Generates an employee code.
    /// Format: EMP-{yyyyMM}-{001}
    /// </summary>
    Task<string> GenerateEmployeeCodeAsync(int branchId);

    /// <summary>
    /// Generates an appointment reference number.
    /// Format: APT-{yyyyMMdd}-{0001}
    /// </summary>
    Task<string> GenerateAppointmentNumberAsync(int branchId);

    /// <summary>
    /// Generates a patient MRN (Medical Record Number).
    /// Format: MRN-{yyyyMMdd}-{0001}
    /// </summary>
    Task<string> GeneratePatientMRNAsync(int branchId);
}

/// <summary>
/// Types of sequences supported by the generator.
/// </summary>
public enum SequenceType
{
    LabOrder,
    RadiologyOrder,
    Invoice,
    Sale,
    Employee,
    Appointment,
    Patient,
    Prescription,
    Quotation
}
