# Xenon Clinic CRM (Demo)

This repository contains a simplified ASP.NET Core MVC solution scaffold for the **XENON** audiology clinic CRM. It follows the requested 3-project structure (Core, Infrastructure, Web) and includes seeded demo data, analytics dashboards, and a mock Emirates ID reader workflow.

## Projects
- **XenonClinic.Core** – Domain entities and shared interfaces such as `ILicenseGuardService`.
- **XenonClinic.Infrastructure** – EF Core DbContext, seed data, and license guard implementation.
- **XenonClinic.Web** – MVC UI with Bootstrap 5, dashboards, analytics, and patient entry forms.

## Running locally
1. Install .NET 8 SDK.
2. Update the connection string in `XenonClinic.Web/Program.cs` if needed (defaults to LocalDB/SQL Server).
3. From the `XenonClinic.Web` directory run:
   ```bash
   dotnet restore
   dotnet ef database update  # create the database schema
   dotnet run
   ```
4. Browse to `https://localhost:5001` and log in with `admin@xenon.local` / `Admin@123!`.

> Note: This container image does not ship with the .NET SDK, so commands above may not execute here, but the solution layout is ready for a local environment.
