# Xenon Clinic CRM – Executive Deck

## 1. Brand & Vision
- **Brand**: XENON – premium, tech-forward audiology CRM.
- **Promise**: Faster clinic operations, consistent patient experiences, and actionable insights.
- **Visual language**: Deep blues, neon accents, rounded cards, soft shadows.
- **Responsiveness**: Mobile-first Bootstrap 5 layout.

## 2. Product Snapshot
- Multi-branch, license-governed SaaS for audiology clinics.
- Admin + role-based access (Admin, BranchAdmin, Audiologist, Receptionist, Technician).
- Modern MVC + Razor Pages UI with Identity auth and clean Bootstrap skin.
- Optimized for Emirates ID capture, analytics, and high-throughput front desk workflows.

## 3. Architecture Overview
- **Solution**: ASP.NET Core 8, clean 3-project split.
  - `XenonClinic.Core`: Domain entities, enums, interfaces, services.
  - `XenonClinic.Infrastructure`: EF Core, SQL Server, DbContext, repositories, seed.
  - `XenonClinic.Web`: MVC + Razor Pages, controllers, views, assets.
- **Persistence**: EF Core Code-First + migrations; seeded demo data.
- **Identity**: Custom `ApplicationUser` with branch affinity + user-branch join.
- **Charts**: Chart.js for dashboards and analytics.

## 4. Branding Implementation
- Branch-specific logo + primary color applied to login, sidebar, navbar, dashboards, invoices.
- Fallback to default **XENON** logo when a branch logo is missing.
- Global footer: “Powered by XENON Technologies”.

## 5. Core Domain Model
- **Branch** ↔ **ApplicationUser** (primary + many-to-many via `UserBranch`).
- **Patients** with demographics, Emirates ID, hearing loss type.
- **Appointments** (type, status) linked to branches and patients.
- **AudiologyVisits** + **Audiograms** with raw JSON data storage.
- **HearingDevices**, **Invoices** (amount, payment status).
- **LicenseConfig** governing branch and user caps.

## 6. License Guardrails
- `ILicenseGuardService` enforces MaxBranches / MaxUsers and expiry / active flags.
- Controllers use guardrails before creating branches/users; show friendly blocking messages.
- License config seeded (3 branches, 20 users, active) for demo.

## 7. Emirates ID (Mock) Flow
- Patient Create page offers **“Read Emirates ID”**.
- AJAX call hits `/Patients/ReadEid`, simulates `http://localhost:5005/eid/read`.
- Returns EmiratesId, English/Arabic names, DOB, gender, nationality.
- JS auto-fills the form; detects duplicates and prompts to open existing record.

## 8. UX Highlights
- Left sidebar with branch branding + core navigation.
- Top navbar: active branch selector (for admins) + profile menu.
- Dashboard: KPI cards, upcoming appointments table, 7-day visits chart.
- DataTable-like lists: search, filter, paging; client + server validation.
- Consistent rounded cards, shadows, iconography (Bootstrap Icons / FontAwesome).

## 9. Analytics – Operational
- Overview KPIs: total patients, appointments (period), completed visits, revenue.
- Filters: date ranges (7/30 days, month, custom) + branch selector.
- Charts: appointments per day, revenue per day, appointment type breakdown, top device models.
- Branch-aware data scoping respecting user role and primary branch.

## 10. Analytics – Audiology Insights
- Diagnosis counts (top 5) and hearing loss type distribution (pie).
- Age distribution buckets (<20, 20–40, 40–60, 60+).
- Repeat-visit rate (patients with >3 visits).
- Efficient LINQ queries with grouping; scoped to branch context.

## 11. Seed Data
- LicenseConfig: active, MaxBranches=3, MaxUsers=20.
- Demo branches: Dubai, Sharjah, Abu Dhabi with placeholder logos/colors.
- Users: Admin, Branch Admin, Audiologist, Receptionist seeded with roles.
- Patients: 30–50 realistic names across branches with mixed hearing loss types.
- Appointments: 100+ across past/future; varied types and statuses.
- Visits/Audiograms: Linked to completed appointments with sample audiogram JSON.
- Devices & Invoices: Realistic models, warranties, amounts (300–8000), paid/pending/cancelled.

## 12. Security & Roles
- ASP.NET Core Identity cookie auth.
- Role-guarded controllers/actions; branch scoping for non-admins.
- Admins can switch branches; normal users locked to primary branch.

## 13. Deployment & Ops
- SQL Server backend; EF Core migrations for schema.
- `dotnet publish` for hosting; environment-configurable connection strings.
- Static assets bundled under `wwwroot`; CDN-friendly.

## 14. Implementation Status (Repo)
- Complete solution scaffold with projects, DbContext, entities, services, seeds, controllers, views, layout, and analytics pages committed.
- README documents SDK install, restore, migrations, run, and smoke testing steps.

## 15. Next Steps / Enhancements
- Add automated UI tests (Playwright) and API integration tests.
- Introduce feature flags for premium analytics packs.
- Add offline-capable EID reader plugin when device SDK is available.
- Harden audit logging and observability (structured logs, tracing).
