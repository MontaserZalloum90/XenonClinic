using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Repository implementation for Patient entities with specialized queries.
/// </summary>
public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ClinicDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Patient?> GetByIdWithMedicalHistoryAsync(int patientId)
    {
        return await _dbSet
            .Include(p => p.MedicalHistory)
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }
}
