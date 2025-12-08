using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

public class ConfigurationResolverService : IConfigurationResolverService
{
    private readonly ClinicDbContext _db;
    private readonly ILogger<ConfigurationResolverService> _logger;

    public ConfigurationResolverService(
        ClinicDbContext db,
        ILogger<ConfigurationResolverService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<EmailConfiguration> GetEmailConfigurationAsync(int companyId)
    {
        try
        {
            var company = await _db.Companies
                .Include(c => c.Settings)
                .Include(c => c.Tenant)
                    .ThenInclude(t => t.Settings)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                _logger.LogWarning("Company {CompanyId} not found when resolving email configuration", companyId);
                return new EmailConfiguration();
            }

            var companySettings = company.Settings;
            var tenantSettings = company.Tenant?.Settings;

            // Company settings override tenant settings (if not null)
            return new EmailConfiguration
            {
                SmtpHost = companySettings?.SmtpHost ?? tenantSettings?.SmtpHost,
                SmtpPort = companySettings?.SmtpPort ?? tenantSettings?.SmtpPort ?? 587,
                SmtpUsername = companySettings?.SmtpUsername ?? tenantSettings?.SmtpUsername,
                SmtpPassword = companySettings?.SmtpPassword ?? tenantSettings?.SmtpPassword,
                SmtpUseSsl = companySettings?.SmtpUseSsl ?? tenantSettings?.SmtpUseSsl ?? true,
                DefaultSenderEmail = companySettings?.DefaultSenderEmail ?? tenantSettings?.DefaultSenderEmail,
                DefaultSenderName = companySettings?.DefaultSenderName ?? tenantSettings?.DefaultSenderName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving email configuration for company {CompanyId}", companyId);
            return new EmailConfiguration();
        }
    }

    public async Task<WhatsAppConfiguration> GetWhatsAppConfigurationAsync(int companyId)
    {
        try
        {
            var company = await _db.Companies
                .Include(c => c.Settings)
                .Include(c => c.Tenant)
                    .ThenInclude(t => t.Settings)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                _logger.LogWarning("Company {CompanyId} not found when resolving WhatsApp configuration", companyId);
                return new WhatsAppConfiguration();
            }

            var companySettings = company.Settings;
            var tenantSettings = company.Tenant?.Settings;

            // Company settings override tenant settings (if not null)
            return new WhatsAppConfiguration
            {
                EnableWhatsApp = companySettings?.EnableWhatsApp ?? tenantSettings?.EnableWhatsApp ?? false,
                Provider = companySettings?.WhatsAppProvider ?? tenantSettings?.WhatsAppProvider,
                AccountSid = companySettings?.WhatsAppAccountSid ?? tenantSettings?.WhatsAppAccountSid,
                AuthToken = companySettings?.WhatsAppAuthToken ?? tenantSettings?.WhatsAppAuthToken,
                PhoneNumber = companySettings?.WhatsAppPhoneNumber ?? tenantSettings?.WhatsAppPhoneNumber,
                BusinessApiToken = companySettings?.WhatsAppBusinessApiToken ?? tenantSettings?.WhatsAppBusinessApiToken,
                BusinessPhoneNumberId = companySettings?.WhatsAppBusinessPhoneNumberId ?? tenantSettings?.WhatsAppBusinessPhoneNumberId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving WhatsApp configuration for company {CompanyId}", companyId);
            return new WhatsAppConfiguration();
        }
    }
}
