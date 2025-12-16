using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ClinicDbContext>()
    .AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Infrastructure services
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserContext>();
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ILabService, LabService>();
builder.Services.AddScoped<IPharmacyService, PharmacyService>();
builder.Services.AddScoped<IHRService, HRService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IRadiologyService, RadiologyService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPatientPortalService, PatientPortalService>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<ICustomReportService, CustomReportService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IDicomService, DicomService>();
builder.Services.AddScoped<IFhirService, FhirService>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<ISecurityConfigurationService, SecurityConfigurationService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IClinicalVisitService, ClinicalVisitService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Validators - FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClinicDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Global exception handler
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
