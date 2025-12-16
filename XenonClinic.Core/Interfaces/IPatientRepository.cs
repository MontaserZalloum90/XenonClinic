using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Repository interface for Patient entities with specialized queries.
/// </summary>
public interface IPatientRepository : IRepository<Patient>
{
    /// <summary>
    /// Gets a patient by ID with their medical history loaded.
    /// </summary>
    /// <param name="patientId">The patient ID</param>
    /// <returns>The patient with medical history, or null if not found</returns>
    Task<Patient?> GetByIdWithMedicalHistoryAsync(int patientId);
}
