using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using XenonClinic.Core.Abstractions;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Modules;
using XenonClinic.Infrastructure.Services;
using XenonClinic.Web.Configuration;
using XenonClinic.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERILOG CONFIGURATION ====================
builder.AddSerilogLogging();

Console.WriteLine("================================================================================");
Console.WriteLine("  XenonClinic - Modular Healthcare Management System");
Console.WriteLine("================================================================================");

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

// ==================== JWT AUTHENTICATION FOR SPA ====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["SecretKey"] ?? "XenonClinic-SecureKey-12345678901234567890123456789012"; // Min 32 chars
var jwtIssuer = jwtSettings["Issuer"] ?? "XenonClinic";
var jwtAudience = jwtSettings["Audience"] ?? "XenonClinicReact";

builder.Services.AddAuthentication(options =>
{
    // Keep cookie auth as default for MVC
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ==================== CORS FOR REACT SPA ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vite default + CRA default
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register services
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Add Data Protection for encryption
builder.Services.AddDataProtection();

// ==================== RATE LIMITING ====================
builder.Services.AddCustomRateLimiting(builder.Configuration);

// ==================== REDIS/DISTRIBUTED CACHING ====================
var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisCacheOptions>() ?? new RedisCacheOptions();
if (redisOptions.Enabled && !string.IsNullOrEmpty(redisOptions.ConnectionString) && redisOptions.ConnectionString != "localhost:6379")
{
    // Use Redis for distributed caching (production)
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisOptions.ConnectionString;
        options.InstanceName = redisOptions.InstanceName;
    });
    Console.WriteLine("[System] Redis distributed cache configured");
}
else
{
    // Use in-memory distributed cache (development/fallback)
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("[System] In-memory distributed cache configured (Redis disabled or not configured)");
}
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// ==================== AUDIT SERVICE ====================
builder.Services.AddScoped<IAuditService, AuditService>();

// Add session support (for company context)
// Note: Distributed cache is already configured above
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

// Lookup services
builder.Services.AddScoped<ILookupService, LookupService>();

// License validation services
builder.Services.AddSingleton<ILicenseValidator, LicenseValidator>();

// ==================== MODULE SYSTEM ====================
Console.WriteLine("\n[System] Initializing Module System...");

// Register Module Manager
builder.Services.AddSingleton<IModuleManager, ModuleManager>();

// Discover and register modules
var licenseValidator = new LicenseValidator(builder.Configuration);
var moduleManager = new ModuleManager(builder.Configuration, licenseValidator);
var availableModules = new List<IModule>
{
    new CaseManagementModule(),
    new AudiologyModule(),
    new LaboratoryModule(),
    new HRModule(),
    new FinancialModule(),
    new InventoryModule(),
    new SalesModule(),
    new ProcurementModule()
};

Console.WriteLine($"[System] Found {availableModules.Count} available modules");

foreach (var module in availableModules)
{
    try
    {
        // Register module with manager
        moduleManager.RegisterModule(module);

        // Check if module is enabled
        if (moduleManager.IsModuleEnabled(module.Name))
        {
            Console.WriteLine($"[System] Loading module: {module.DisplayName} v{module.Version}");

            // Initialize module
            await module.OnInitializingAsync(builder.Services.BuildServiceProvider());

            // Configure module services
            module.ConfigureServices(builder.Services, builder.Configuration);

            // Store module for later use
            builder.Services.AddSingleton(module);
        }
        else
        {
            Console.WriteLine($"[System] Module disabled: {module.DisplayName} (skipped)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to load module {module.DisplayName}: {ex.Message}");
        if (module.IsRequired)
        {
            throw new Exception($"Required module {module.DisplayName} failed to load", ex);
        }
    }
}

// Store module manager
builder.Services.AddSingleton(moduleManager);

Console.WriteLine($"[System] Module system initialized - {moduleManager.GetEnabledModules().Count()} modules enabled");
Console.WriteLine("================================================================================\n");

// ==================== END MODULE SYSTEM ====================

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

// ==================== SWAGGER/OPENAPI FOR API DOCUMENTATION ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "XenonClinic API",
        Version = "v1",
        Description = "REST API for XenonClinic Healthcare Management System - React SPA Integration",
        Contact = new OpenApiContact
        {
            Name = "XenonClinic Team"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure module routes
Console.WriteLine("\n[System] Configuring module routes...");
var enabledModules = app.Services.GetServices<IModule>();
foreach (var module in enabledModules)
{
    module.ConfigureRoutes(app);
}
Console.WriteLine($"[System] Routes configured for {enabledModules.Count()} modules\n");

// ==================== MIDDLEWARE PIPELINE ====================
// Order matters! Security headers should be early in the pipeline

// 1. Request/Response Logging with Correlation IDs (earliest to capture all requests)
app.UseRequestResponseLogging();

// 2. Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 3. Security headers (should be added to all responses)
app.UseSecurityHeaders(options =>
{
    // In development, be more lenient with CSP for hot reload, Swagger, etc.
    if (app.Environment.IsDevelopment())
    {
        options.EnableContentSecurityPolicy = false;
        options.EnableHsts = false;
    }
});

// 4. Serilog request logging
app.UseSerilogRequestLogging();

// 5. Rate limiting
app.UseRateLimiter();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "XenonClinic API v1");
        c.RoutePrefix = "api/docs"; // Access Swagger at /api/docs
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS
app.UseCors("ReactApp");

// Enable request localization
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Enable session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution middleware - must be after authentication
app.UseTenantResolution();

// License validation middleware - validates module licenses
app.UseLicenseValidation();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
