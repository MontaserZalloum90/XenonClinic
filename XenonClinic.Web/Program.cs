using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection string (override in appsettings.json if you want)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? "Server=(localdb)\\mssqllocaldb;Database=XenonClinic;Trusted_Connection=True;MultipleActiveResultSets=true";

// DbContext
builder.Services.AddDbContext<XenonClinicDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<XenonClinicDbContext>()
    .AddDefaultTokenProviders();

// App services
builder.Services.AddScoped<ILicenseGuardService, LicenseGuardService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
