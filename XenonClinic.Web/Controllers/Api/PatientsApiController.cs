using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientsApiController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsApiController> _logger;

    public PatientsApiController(IPatientService patientService, ILogger<PatientsApiController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    // GET: api/PatientsApi
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // TODO: Get branchId from user context
            var branchId = 1;
            var patients = await _patientService.GetPatientsByBranchIdAsync(branchId);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            return StatusCode(500, new { message = "Error retrieving patients" });
        }
    }

    // GET: api/PatientsApi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient {PatientId}", id);
            return StatusCode(500, new { message = "Error retrieving patient" });
        }
    }

    // GET: api/PatientsApi/search?searchTerm=john
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term is required" });

            var branchId = 1;
            var patients = await _patientService.SearchPatientsAsync(branchId, searchTerm);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients");
            return StatusCode(500, new { message = "Error searching patients" });
        }
    }

    // GET: api/PatientsApi/emirates/784-1234-1234567-1
    [HttpGet("emirates/{emiratesId}")]
    public async Task<IActionResult> GetByEmiratesId(string emiratesId)
    {
        try
        {
            var branchId = 1;
            var patient = await _patientService.GetPatientByEmiratesIdAsync(emiratesId, branchId);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient by Emirates ID");
            return StatusCode(500, new { message = "Error retrieving patient" });
        }
    }

    // POST: api/PatientsApi
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient patient)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            patient.BranchId = 1; // TODO: Get from user context
            var createdPatient = await _patientService.CreatePatientAsync(patient);
            return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, new { message = "Error creating patient" });
        }
    }

    // PUT: api/PatientsApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Patient patient)
    {
        try
        {
            if (id != patient.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _patientService.UpdatePatientAsync(patient);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {PatientId}", id);
            return StatusCode(500, new { message = "Error updating patient" });
        }
    }

    // DELETE: api/PatientsApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            await _patientService.DeletePatientAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient {PatientId}", id);
            return StatusCode(500, new { message = "Error deleting patient" });
        }
    }

    // GET: api/PatientsApi/5/medical-history
    [HttpGet("{id}/medical-history")]
    public async Task<IActionResult> GetMedicalHistory(int id)
    {
        try
        {
            var medicalHistory = await _patientService.GetPatientMedicalHistoryAsync(id);
            if (medicalHistory == null)
                return NotFound(new { message = "Medical history not found" });

            return Ok(medicalHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical history for patient {PatientId}", id);
            return StatusCode(500, new { message = "Error retrieving medical history" });
        }
    }

    // POST: api/PatientsApi/medical-history
    [HttpPost("medical-history")]
    public async Task<IActionResult> CreateOrUpdateMedicalHistory([FromBody] PatientMedicalHistory medicalHistory)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _patientService.CreateOrUpdateMedicalHistoryAsync(medicalHistory);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving medical history");
            return StatusCode(500, new { message = "Error saving medical history" });
        }
    }

    // GET: api/PatientsApi/5/documents
    [HttpGet("{id}/documents")]
    public async Task<IActionResult> GetDocuments(int id)
    {
        try
        {
            var documents = await _patientService.GetPatientDocumentsAsync(id);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for patient {PatientId}", id);
            return StatusCode(500, new { message = "Error retrieving documents" });
        }
    }

    // GET: api/PatientsApi/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = 1;
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            var statistics = new
            {
                TotalPatients = await _patientService.GetTotalPatientsCountAsync(branchId),
                NewPatientsThisMonth = await _patientService.GetNewPatientsCountAsync(branchId, thirtyDaysAgo, now),
                ActiveCases = await _patientService.GetActiveCasesCountAsync(branchId),
                OverdueCases = await _patientService.GetOverdueCasesCountAsync(branchId),
                GenderDistribution = await _patientService.GetPatientsByGenderDistributionAsync(branchId)
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }
}
