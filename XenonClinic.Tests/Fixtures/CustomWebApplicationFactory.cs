using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Tests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing with in-memory database
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string TestDatabaseName { get; } = $"XenonClinic_Test_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ClinicDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the existing DbContext
            services.RemoveAll(typeof(ClinicDbContext));

            // Add in-memory database for testing
            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseInMemoryDatabase(TestDatabaseName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ClinicDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }

    public ClinicDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(TestDatabaseName)
            .Options;

        return new ClinicDbContext(options);
    }
}
