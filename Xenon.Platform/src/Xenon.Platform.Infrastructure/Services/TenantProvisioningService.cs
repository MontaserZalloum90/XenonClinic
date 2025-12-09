using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public interface ITenantProvisioningService
{
    Task<Tenant> CreateTenant(CreateTenantRequest request);
    Task<bool> ProvisionTenantDatabase(Guid tenantId);
    Task<bool> CheckTenantDatabaseHealth(Guid tenantId);
    string GenerateSlug(string companyName);
    Task<bool> IsSlugAvailable(string slug);
}

public record CreateTenantRequest
{
    public string CompanyName { get; init; } = string.Empty;
    public CompanyType CompanyType { get; init; }
    public ClinicType? ClinicType { get; init; }
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string AdminFirstName { get; init; } = string.Empty;
    public string AdminLastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Country { get; init; }
    public int TrialDays { get; init; } = 30;
}

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly PlatformDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        PlatformDbContext context,
        IPasswordHashingService passwordHashingService,
        IConfiguration configuration,
        ILogger<TenantProvisioningService> logger)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Tenant> CreateTenant(CreateTenantRequest request)
    {
        // Generate unique slug
        var baseSlug = GenerateSlug(request.CompanyName);
        var slug = baseSlug;
        var counter = 1;

        while (!await IsSlugAvailable(slug))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        // Get default plan (Starter)
        var starterPlan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Code == PlanCode.Starter && p.IsActive);

        // Create tenant
        var tenant = new Tenant
        {
            Name = request.CompanyName,
            Slug = slug,
            CompanyType = request.CompanyType,
            ClinicType = request.ClinicType,
            Status = TenantStatus.Trial,
            TrialStartDate = DateTime.UtcNow,
            TrialEndDate = DateTime.UtcNow.AddDays(request.TrialDays),
            ContactEmail = request.AdminEmail,
            ContactPhone = request.Phone,
            Country = request.Country,
            MaxBranches = starterPlan?.IncludedBranches ?? 1,
            MaxUsers = starterPlan?.IncludedUsers ?? 5
        };

        // Create admin user
        var admin = new TenantAdmin
        {
            TenantId = tenant.Id,
            Email = request.AdminEmail,
            PasswordHash = _passwordHashingService.HashPassword(request.AdminPassword),
            FirstName = request.AdminFirstName,
            LastName = request.AdminLastName,
            Role = "TENANT_ADMIN",
            IsActive = true
        };

        tenant.Admins.Add(admin);

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created tenant {TenantId} with slug {Slug}", tenant.Id, tenant.Slug);

        return tenant;
    }

    public async Task<bool> ProvisionTenantDatabase(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogError("Tenant {TenantId} not found for provisioning", tenantId);
            return false;
        }

        if (tenant.IsDatabaseProvisioned)
        {
            _logger.LogWarning("Tenant {TenantId} database already provisioned", tenantId);
            return true;
        }

        try
        {
            var connectionTemplate = _configuration["TenantDb:ConnectionStringTemplate"]
                ?? throw new InvalidOperationException("Tenant DB connection template not configured");

            var databaseName = $"XenonTenant_{tenant.Slug}";
            var connectionString = string.Format(connectionTemplate, databaseName);

            // Create the database
            var masterConnection = _configuration["TenantDb:MasterConnectionString"]
                ?? _configuration.GetConnectionString("PlatformDb")
                    ?.Replace("XenonPlatform", "master")
                ?? throw new InvalidOperationException("Master connection not configured");

            await using var connection = new SqlConnection(masterConnection);
            await connection.OpenAsync();

            // Check if database exists
            var checkCmd = new SqlCommand(
                $"SELECT database_id FROM sys.databases WHERE name = '{databaseName}'",
                connection);
            var exists = await checkCmd.ExecuteScalarAsync() != null;

            if (!exists)
            {
                var createCmd = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
                await createCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Created database {DatabaseName} for tenant {TenantId}",
                    databaseName, tenantId);
            }

            // Update tenant record
            tenant.DatabaseConnectionString = connectionString;
            tenant.IsDatabaseProvisioned = true;
            tenant.DatabaseProvisionedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant {TenantId} database provisioned successfully", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision database for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> CheckTenantDatabaseHealth(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null || string.IsNullOrEmpty(tenant.DatabaseConnectionString))
            return false;

        try
        {
            await using var connection = new SqlConnection(tenant.DatabaseConnectionString);
            await connection.OpenAsync();
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSlug(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return "tenant";

        // Convert to lowercase and replace spaces with hyphens
        var slug = companyName.ToLowerInvariant().Trim();

        // Remove special characters, keep only alphanumeric and hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace multiple spaces/hyphens with single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 50)
            slug = slug.Substring(0, 50).TrimEnd('-');

        return string.IsNullOrEmpty(slug) ? "tenant" : slug;
    }

    public async Task<bool> IsSlugAvailable(string slug)
    {
        return !await _context.Tenants.AnyAsync(t => t.Slug == slug);
    }
}
