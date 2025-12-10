using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Serilog;
using Xenon.Platform.Api.Middleware;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/platform-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Xenon Platform API",
        Version = "v1",
        Description = "Platform API for tenant management, monitoring, and reports"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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

// Database
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PlatformDb")));

// JWT Configuration - Read from environment variable in production
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JWT_SECRET_KEY environment variable or Jwt:SecretKey in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "xenon-platform";
var tenantAudience = builder.Configuration["Jwt:TenantAudience"] ?? "xenon-tenant";
var adminAudience = builder.Configuration["Jwt:AdminAudience"] ?? "xenon-admin";

// Validate JWT secret key length
if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for security.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("TenantScheme", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = tenantAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
.AddJwtBearer("AdminScheme", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = adminAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // Default scheme accepts both audiences
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudiences = new[] { tenantAudience, adminAudience },
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("realm", "tenant"));

    options.AddPolicy("PlatformAdminPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("realm", "platform-admin"));

    options.AddPolicy("SuperAdminPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("realm", "platform-admin")
              .RequireClaim("role", "SUPER_ADMIN"));
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Strict rate limit for authentication endpoints
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Standard rate limit for public endpoints
    options.AddFixedWindowLimiter("public", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var response = new { success = false, error = "Too many requests. Please try again later." };
        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };
});

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Infrastructure services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IPricingCalculatorService, PricingCalculatorService>();
builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Security services
builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ISecurityEventService, SecurityEventService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddSingleton<IIpBlockingService, IpBlockingService>();

// Application services
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.IPlatformAuthService, PlatformAuthService>();
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.ITenantAuthService, TenantAuthService>();
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.ITenantManagementService, TenantManagementService>();
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.IUsageService, UsageService>();
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.ILicenseService, LicenseService>();
builder.Services.AddScoped<Xenon.Platform.Application.Interfaces.IDemoRequestService, DemoRequestService>();

// Background services
builder.Services.AddHostedService<Xenon.Platform.Api.BackgroundServices.TenantHealthCheckService>();
builder.Services.AddHostedService<Xenon.Platform.Api.BackgroundServices.SubscriptionExpiryService>();
builder.Services.AddHostedService<Xenon.Platform.Api.BackgroundServices.SecurityCleanupService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebsitePolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlatformDbContext>();

// Problem Details for standardized error responses
builder.Services.AddProblemDetails();

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            success = false,
            error = app.Environment.IsDevelopment()
                ? "An internal server error occurred. Check logs for details."
                : "An unexpected error occurred. Please try again later."
        };

        await context.Response.WriteAsJsonAsync(errorResponse);

        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandlerFeature?.Error != null)
        {
            Log.Error(exceptionHandlerFeature.Error, "Unhandled exception occurred");
        }
    });
});

// Security Headers Middleware
app.Use(async (context, next) =>
{
    // Prevent clickjacking attacks
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Enable XSS filtering
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Referrer policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Content Security Policy (basic)
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none';");

    // Permissions Policy
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    await next();
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // HSTS for production
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("WebsitePolicy");

// IP blocking should come before rate limiting to block suspicious IPs early
app.UseIpBlocking();

// Rate limiting must come before authentication
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await context.Database.MigrateAsync();
        await SeedData.SeedAsync(context, scope.ServiceProvider.GetRequiredService<IPasswordHashingService>());
        logger.LogInformation("Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

Log.Information("Xenon Platform API starting...");
app.Run();

// Data seeder
public static class SeedData
{
    public static async Task SeedAsync(PlatformDbContext context, IPasswordHashingService passwordService)
    {
        // Seed plans if not exists
        if (!await context.Plans.AnyAsync())
        {
            var plans = new[]
            {
                new Xenon.Platform.Domain.Entities.Plan
                {
                    Code = Xenon.Platform.Domain.Enums.PlanCode.Starter,
                    Name = "Starter",
                    Description = "Perfect for small clinics or trading companies just getting started",
                    MonthlyPrice = 499,
                    AnnualPrice = 4490,
                    IncludedBranches = 1,
                    IncludedUsers = 5,
                    ExtraBranchPrice = 199,
                    ExtraUserPrice = 49,
                    FeaturesJson = "[\"Patient/Customer Management\",\"Appointments & Scheduling\",\"Basic Billing & Invoicing\",\"Inventory Tracking\",\"Standard Reports\",\"Email Support\",\"99.5% Uptime SLA\"]",
                    SupportLevel = "Email support (48h response)",
                    IsActive = true,
                    SortOrder = 1
                },
                new Xenon.Platform.Domain.Entities.Plan
                {
                    Code = Xenon.Platform.Domain.Enums.PlanCode.Growth,
                    Name = "Growth",
                    Description = "For growing practices with multiple locations and staff",
                    MonthlyPrice = 999,
                    AnnualPrice = 8990,
                    IncludedBranches = 3,
                    IncludedUsers = 15,
                    ExtraBranchPrice = 149,
                    ExtraUserPrice = 39,
                    FeaturesJson = "[\"Everything in Starter\",\"Multi-Branch Management\",\"Advanced RBAC\",\"Laboratory Module\",\"Analytics Dashboard\",\"Audit Trail\",\"API Access\",\"Priority Email & Chat Support\",\"99.9% Uptime SLA\"]",
                    SupportLevel = "Priority support (24h response)",
                    IsActive = true,
                    IsRecommended = true,
                    SortOrder = 2
                },
                new Xenon.Platform.Domain.Entities.Plan
                {
                    Code = Xenon.Platform.Domain.Enums.PlanCode.Enterprise,
                    Name = "Enterprise",
                    Description = "For large organizations requiring advanced features and dedicated support",
                    MonthlyPrice = 2499,
                    AnnualPrice = 22490,
                    IncludedBranches = 10,
                    IncludedUsers = 50,
                    ExtraBranchPrice = 99,
                    ExtraUserPrice = 29,
                    FeaturesJson = "[\"Everything in Growth\",\"Unlimited Branches\",\"Custom Integrations\",\"SSO/SAML Support\",\"Advanced Analytics\",\"Custom Reports\",\"Dedicated Account Manager\",\"On-Prem Deployment Option\",\"Phone & Priority Support\",\"99.95% Uptime SLA\",\"Custom SLA Available\"]",
                    SupportLevel = "Dedicated support (4h response)",
                    IsActive = true,
                    SortOrder = 3
                }
            };

            context.Plans.AddRange(plans);
            await context.SaveChangesAsync();
        }

        // Seed platform admin if not exists
        if (!await context.PlatformAdmins.AnyAsync())
        {
            var admin = new Xenon.Platform.Domain.Entities.PlatformAdmin
            {
                Email = "admin@xenon.ae",
                PasswordHash = passwordService.HashPassword("Admin@123!"),
                FirstName = "Platform",
                LastName = "Admin",
                Role = "SUPER_ADMIN",
                Permissions = "tenants:read,tenants:write,reports:read,reports:export,monitoring:read",
                IsActive = true
            };

            context.PlatformAdmins.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
