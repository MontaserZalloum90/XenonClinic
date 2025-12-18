using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Tests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing XenonClinic API.
/// Configures in-memory database and test-specific services.
/// </summary>
public class XenonClinicWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public XenonClinicWebApplicationFactory()
    {
        _databaseName = $"XenonClinicTest_{Guid.NewGuid():N}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ClinicDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove existing ClinicDbContext registration
            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ClinicDbContext));
            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Replace cache service with in-memory implementation
            var cacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICacheService));
            if (cacheDescriptor != null)
            {
                services.Remove(cacheDescriptor);
            }
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, Infrastructure.Services.CacheService>();

            // Build service provider and initialize database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Creates an HTTP client with default configuration for API testing.
    /// </summary>
    public HttpClient CreateApiClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client with a mock JWT token for authenticated requests.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string userId = "test-user", string role = "Admin")
    {
        var client = CreateApiClient();

        // Add mock authentication header
        // In a real scenario, this would use a test JWT token
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        return client;
    }
}

/// <summary>
/// Collection definition for sharing XenonClinicWebApplicationFactory across tests.
/// </summary>
[CollectionDefinition("XenonClinic")]
public class XenonClinicCollection : ICollectionFixture<XenonClinicWebApplicationFactory>
{
}
