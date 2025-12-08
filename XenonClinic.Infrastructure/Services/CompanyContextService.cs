using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for detecting and resolving company context at login time.
/// Supports multiple detection strategies: subdomain, path, query string, cookie.
/// </summary>
public class CompanyContextService : ICompanyContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<CompanyContextService> _logger;

    private const string CompanyCookieName = "XenonClinic.CompanyCode";
    private const string CompanySessionKey = "CurrentCompanyCode";

    public CompanyContextService(
        IHttpContextAccessor httpContextAccessor,
        ClinicDbContext dbContext,
        ILogger<CompanyContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Company?> GetCurrentCompanyAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        string? companyCode = null;

        // Strategy 1: Check subdomain (e.g., company1.xenonclinic.com)
        companyCode = TryGetCompanyFromSubdomain(httpContext);

        // Strategy 2: Check route/path (e.g., /login/company-code)
        if (string.IsNullOrEmpty(companyCode))
        {
            companyCode = TryGetCompanyFromPath(httpContext);
        }

        // Strategy 3: Check query string (e.g., ?company=company-code)
        if (string.IsNullOrEmpty(companyCode))
        {
            companyCode = TryGetCompanyFromQuery(httpContext);
        }

        // Strategy 4: Check session
        if (string.IsNullOrEmpty(companyCode))
        {
            companyCode = httpContext.Session?.GetString(CompanySessionKey);
        }

        // Strategy 5: Check cookie
        if (string.IsNullOrEmpty(companyCode))
        {
            companyCode = httpContext.Request.Cookies[CompanyCookieName];
        }

        if (string.IsNullOrEmpty(companyCode))
            return null;

        return await GetCompanyByCodeAsync(companyCode);
    }

    /// <inheritdoc />
    public async Task<Company?> GetCompanyByCodeAsync(string companyCode)
    {
        if (string.IsNullOrEmpty(companyCode))
            return null;

        return await _dbContext.Companies
            .Include(c => c.Tenant)
            .Include(c => c.AuthSettings)
                .ThenInclude(a => a!.IdentityProviders)
            .FirstOrDefaultAsync(c => c.Code == companyCode && c.IsActive);
    }

    /// <inheritdoc />
    public async Task<Company?> GetCompanyByIdAsync(int companyId)
    {
        return await _dbContext.Companies
            .Include(c => c.Tenant)
            .Include(c => c.AuthSettings)
                .ThenInclude(a => a!.IdentityProviders)
            .FirstOrDefaultAsync(c => c.Id == companyId && c.IsActive);
    }

    /// <inheritdoc />
    public void SetCurrentCompany(string companyCode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        // Set in session
        httpContext.Session?.SetString(CompanySessionKey, companyCode);

        // Set in cookie (persistent for 30 days)
        httpContext.Response.Cookies.Append(CompanyCookieName, companyCode, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        _logger.LogDebug("Set current company to {CompanyCode}", companyCode);
    }

    /// <inheritdoc />
    public void ClearCurrentCompany()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        httpContext.Session?.Remove(CompanySessionKey);
        httpContext.Response.Cookies.Delete(CompanyCookieName);

        _logger.LogDebug("Cleared current company context");
    }

    /// <inheritdoc />
    public async Task<IList<Company>> GetAllActiveCompaniesAsync()
    {
        return await _dbContext.Companies
            .Include(c => c.Tenant)
            .Where(c => c.IsActive && c.Tenant!.IsActive)
            .OrderBy(c => c.Tenant!.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    private string? TryGetCompanyFromSubdomain(HttpContext context)
    {
        var host = context.Request.Host.Host;

        // Skip localhost and IP addresses
        if (host == "localhost" || host.StartsWith("127.") || host.StartsWith("192.168."))
            return null;

        // Extract subdomain (e.g., "company1" from "company1.xenonclinic.com")
        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            var subdomain = parts[0];
            // Common subdomains to ignore
            if (subdomain != "www" && subdomain != "api" && subdomain != "admin")
            {
                _logger.LogDebug("Detected company from subdomain: {Subdomain}", subdomain);
                return subdomain;
            }
        }

        return null;
    }

    private string? TryGetCompanyFromPath(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
            return null;

        // Pattern: /login/{company-code} or /Account/Login/{company-code}
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Check for /login/{code}
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i].Equals("login", StringComparison.OrdinalIgnoreCase) ||
                segments[i].Equals("signin", StringComparison.OrdinalIgnoreCase))
            {
                var potentialCode = segments[i + 1];
                _logger.LogDebug("Detected company from path: {CompanyCode}", potentialCode);
                return potentialCode;
            }
        }

        return null;
    }

    private string? TryGetCompanyFromQuery(HttpContext context)
    {
        // Check for ?company=code or ?org=code or ?c=code
        var query = context.Request.Query;

        var companyCode = query["company"].FirstOrDefault() ??
                          query["org"].FirstOrDefault() ??
                          query["c"].FirstOrDefault();

        if (!string.IsNullOrEmpty(companyCode))
        {
            _logger.LogDebug("Detected company from query string: {CompanyCode}", companyCode);
        }

        return companyCode;
    }
}
