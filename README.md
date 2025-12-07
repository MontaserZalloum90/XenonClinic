# Xenon Clinic CRM (Demo)

This repository contains a simplified ASP.NET Core MVC solution scaffold for the **XENON** audiology clinic CRM. It follows the requested 3-project structure (Core, Infrastructure, Web) and includes seeded demo data, analytics dashboards, and a mock Emirates ID reader workflow.

## Projects
- **XenonClinic.Core** – Domain entities and shared interfaces such as `ILicenseGuardService`.
- **XenonClinic.Infrastructure** – EF Core DbContext, seed data, and license guard implementation.
- **XenonClinic.Web** – MVC UI with Bootstrap 5, dashboards, analytics, and patient entry forms.

Open the repository with the root `XenonClinic.sln` to load all three projects together in Visual Studio or JetBrains Rider.

## Running locally
1. Install .NET 8 SDK. If you do not already have it, download from https://dotnet.microsoft.com/download or use the one-line installer:
   ```bash
   curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
   export PATH="$HOME/.dotnet:$PATH"
   ```
2. Update the connection string in `XenonClinic.Web/Program.cs` if needed (defaults to LocalDB/SQL Server).
3. Restore and build from the solution root, then create the database and run the app:
   ```bash
   dotnet restore XenonClinic.sln
   dotnet ef database update --project XenonClinic.Infrastructure --startup-project XenonClinic.Web
   dotnet run --project XenonClinic.Web
   ```
4. Browse to `https://localhost:5001` and log in with `admin@xenon.local` / `Admin@123!`.

### Smoke testing
- Build the whole solution: `dotnet build XenonClinic.sln`
- Run any available automated tests (add as they are created): `dotnet test XenonClinic.sln`

> Note: This container image does not ship with the .NET SDK and outbound downloads are blocked, so commands above may not execute here. They are ready for a local environment where the SDK is installed.
