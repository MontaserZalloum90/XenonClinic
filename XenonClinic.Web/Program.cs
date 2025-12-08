using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using XenonClinic.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=XenonClinic;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ClinicDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<ClinicDbContext>()
  .AddDefaultTokenProviders();

// Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Register services
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Add Data Protection for encryption
builder.Services.AddDataProtection();

// Add session support (for company context)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Multi-tenancy services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBranchScopedService, BranchScopedService>();
builder.Services.AddScoped<ILicenseGuardService, LicenseGuardService>();

// Authentication services
builder.Services.AddScoped<ISecretEncryptionService, SecretEncryptionService>();
builder.Services.AddScoped<ICompanyContext, CompanyContextService>();
builder.Services.AddScoped<ICompanyAuthConfigService, CompanyAuthConfigService>();
builder.Services.AddScoped<IExternalUserMapper, ExternalUserMapperService>();
builder.Services.AddScoped<IDynamicAuthenticationService, DynamicAuthenticationService>();

// Communication services (WhatsApp & Email)
builder.Services.AddHttpClient(); // Required for WhatsApp API calls
builder.Services.AddScoped<IConfigurationResolverService, ConfigurationResolverService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Case Management services
builder.Services.AddScoped<ICaseService, CaseService>();

// Theme services
builder.Services.AddScoped<IThemeService, ThemeService>();

// Configure localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ar")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Read culture from cookie
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable request localization
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Enable session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution middleware - must be after authentication
app.UseTenantResolution();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
