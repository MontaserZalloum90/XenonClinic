# Technology Detection Report

**Generated:** 2025-12-13
**Repository:** XenonClinic
**Purpose:** Production Readiness Audit - Phase 0

---

## Executive Summary

XenonClinic is a **multi-tenant healthcare management system** with:

- **Backend:** .NET 8 ASP.NET Core Web API (Clean Architecture)
- **Frontend:** React 19 + TypeScript + Vite (2 apps: Admin + Public)
- **Database:** PostgreSQL 16 (via EF Core 8) + SQL Server support
- **Cache:** Redis 7
- **Auth:** JWT Bearer + Role-Based Access Control (RBAC)
- **Testing:** xUnit + Moq + FluentAssertions (backend), Vitest (frontend), Playwright (E2E)

---

## 1. Frontend Applications

### 1.1 Admin Dashboard (XenonClinic.React)

| Attribute      | Value                             | Evidence                               |
| -------------- | --------------------------------- | -------------------------------------- |
| Framework      | React 19.2.0                      | `XenonClinic.React/package.json:23`    |
| Language       | TypeScript 5.9.3                  | `XenonClinic.React/package.json:46`    |
| Build Tool     | Vite 7.2.4                        | `XenonClinic.React/package.json:48`    |
| State Mgmt     | Zustand 5.0.9                     | `XenonClinic.React/package.json:27`    |
| Data Fetching  | TanStack Query 5.90.12            | `XenonClinic.React/package.json:19`    |
| Routing        | React Router DOM 7.10.1           | `XenonClinic.React/package.json:25`    |
| Form Handling  | React Hook Form 7.68.0            | `XenonClinic.React/package.json:24`    |
| HTTP Client    | Axios 1.13.2                      | `XenonClinic.React/package.json:20`    |
| UI Components  | HeadlessUI 2.2.9, HeroIcons 2.2.0 | `XenonClinic.React/package.json:17-18` |
| Styling        | TailwindCSS 3.4.18                | `XenonClinic.React/package.json:45`    |
| Charts         | Recharts 3.5.1                    | `XenonClinic.React/package.json:26`    |
| Test Framework | Vitest 4.0.15                     | `XenonClinic.React/package.json:49`    |
| Test Library   | Testing Library React 16.3.0      | `XenonClinic.React/package.json:33`    |

**Key Files:**

- Entry point: `XenonClinic.React/src/main.tsx`
- App routing: `XenonClinic.React/src/App.tsx` (882 lines, 100+ routes)
- Auth context: `XenonClinic.React/src/contexts/AuthContext.tsx`
- Protected routes: `XenonClinic.React/src/components/ProtectedRoute.tsx`

### 1.2 Public Website (Xenon.PublicWebsite)

| Attribute  | Value            | Evidence                                 |
| ---------- | ---------------- | ---------------------------------------- |
| Framework  | React (via Vite) | `Xenon.PublicWebsite/package.json`       |
| Build Tool | Vite             | `Xenon.PublicWebsite/vite.config.ts`     |
| Styling    | TailwindCSS      | `Xenon.PublicWebsite/tailwind.config.js` |

### 1.3 Shared UI Library (Shared.UI)

| Attribute      | Value     | Evidence                          |
| -------------- | --------- | --------------------------------- |
| Package Name   | @xenon/ui | `Shared.UI/package.json`          |
| Component Docs | Storybook | `Shared.UI/.storybook/` directory |
| Test Framework | Vitest    | `Shared.UI/vitest.config.ts`      |

**Shared Components:** Toast, Modal, DataTable, Pagination, FormField, Badge, LoadingSkeleton

---

## 2. Backend Services

### 2.1 Platform API (Xenon.Platform.Api)

| Attribute      | Value                                                      | Evidence                                                            |
| -------------- | ---------------------------------------------------------- | ------------------------------------------------------------------- |
| Framework      | ASP.NET Core 8.0                                           | `Xenon.Platform/src/Xenon.Platform.Api/Xenon.Platform.Api.csproj:4` |
| Architecture   | Clean Architecture (Domain/Application/Infrastructure/Api) | Directory structure `Xenon.Platform/src/`                           |
| API Versioning | Asp.Versioning.Mvc 8.1.0                                   | `Xenon.Platform.Api.csproj:11`                                      |
| API Docs       | Swashbuckle.AspNetCore 6.5.0 (Swagger)                     | `Xenon.Platform.Api.csproj:13`                                      |
| Logging        | Serilog 8.0.1 (Console + File sinks)                       | `Xenon.Platform.Api.csproj:14-16`                                   |
| Rate Limiting  | AspNetCoreRateLimit 5.0.0                                  | `Xenon.Platform.Api.csproj:17`                                      |
| Authentication | JWT Bearer 8.0.10                                          | `Xenon.Platform.Api.csproj:18`                                      |

**Key Domains:**

- Tenant Management
- Platform Authentication
- License Management
- Usage Tracking
- Demo Request Processing

### 2.2 Clinic Core Libraries

#### XenonClinic.Core (Domain Layer)

| Attribute    | Value                                    | Evidence                                     |
| ------------ | ---------------------------------------- | -------------------------------------------- | ------ |
| Framework    | .NET 8.0                                 | `XenonClinic.Core/XenonClinic.Core.csproj:3` |
| Purpose      | Domain entities, interfaces, DTOs, enums | Directory structure                          |
| Entity Count | 252 entity files                         | `find -name "_.cs" -path "_/Entities/\*"     | wc -l` |

**Entity Categories (from file paths):**

- Core: Patient, Appointment, Employee, Invoice, Payment, Inventory
- Clinical: LabOrder, LabResult, DicomStudy
- Specialty Modules: Dental, Cardiology, Ophthalmology, Orthopedics, Dermatology, Oncology, Neurology, Pediatrics, Gynecology, ENT, Fertility, Dialysis, Physiotherapy, Pain Management, Psychiatry, Gastroenterology, Podiatry, Chiropractic, Veterinary, Sleep Medicine
- Lookups: 27 lookup entities for status/type management
- Multi-tenancy: Tenant, Company, Branch, TenantSettings, TenantFeature

#### XenonClinic.Infrastructure (Infrastructure Layer)

| Attribute   | Value                                | Evidence                               |
| ----------- | ------------------------------------ | -------------------------------------- |
| ORM         | Entity Framework Core 8.0.8          | `XenonClinic.Infrastructure.csproj:11` |
| DB Provider | SQL Server (EF Core SqlServer 8.0.8) | `XenonClinic.Infrastructure.csproj:12` |
| Identity    | ASP.NET Core Identity 8.0.8          | `XenonClinic.Infrastructure.csproj:17` |
| Caching     | Redis (StackExchange 8.0.8)          | `XenonClinic.Infrastructure.csproj:19` |

**Key Services (65 interfaces in `/Interfaces/`):**

- `IPatientService`, `IAppointmentService`, `IClinicalVisitService`
- `ILabService`, `IPharmacyService`, `IRadiologyService`
- `IFinancialService`, `IPayrollService`, `IInventoryService`
- `IRbacService`, `IAuditService`, `ICurrentUserContext`
- `ITenantService`, `ICompanyService`, `IBranchScopedService`

### 2.3 Workflow Engine (XenonClinic.WorkflowEngine)

| Attribute | Value                       | Evidence                                                       |
| --------- | --------------------------- | -------------------------------------------------------------- |
| Framework | .NET 8.0                    | `XenonClinic.WorkflowEngine/XenonClinic.WorkflowEngine.csproj` |
| Purpose   | Business process automation | Directory structure                                            |

**Key Entities:**

- `ProcessDefinition`, `ProcessInstance`, `ActivityInstance`
- `HumanTask`, `ProcessTimer`, `AuditEvent`

---

## 3. Database & Persistence

### 3.1 Primary Database

| Attribute  | Value                                  | Evidence                               |
| ---------- | -------------------------------------- | -------------------------------------- |
| Engine     | PostgreSQL 16 (Docker default)         | `docker-compose.yml:6`                 |
| Alt Engine | SQL Server (EF Core provider)          | `XenonClinic.Infrastructure.csproj:12` |
| ORM        | Entity Framework Core 8.0.8            | Project references                     |
| Connection | `ConnectionStrings__DefaultConnection` | `docker-compose.yml:48`                |

### 3.2 Caching

| Attribute | Value                                | Evidence                          |
| --------- | ------------------------------------ | --------------------------------- |
| Engine    | Redis 7                              | `docker-compose.yml:24`           |
| Purpose   | Distributed caching, session storage | `Redis__ConnectionString` env var |

### 3.3 Multi-Tenancy Architecture

| Pattern           | Implementation                           | Evidence                           |
| ----------------- | ---------------------------------------- | ---------------------------------- |
| Tenant Isolation  | Database-per-tenant + shared platform DB | `MULTI_TENANT_ARCHITECTURE.md`     |
| Tenant Resolution | Header/subdomain based                   | `ITenantContextAccessor` interface |
| Data Scoping      | `IBranchEntity`, `IBranchScopedService`  | Interface files                    |

---

## 4. Authentication & Authorization

### 4.1 Authentication

| Attribute      | Value                 | Evidence                                            |
| -------------- | --------------------- | --------------------------------------------------- |
| Method         | JWT Bearer Tokens     | `Jwt__Secret`, `Jwt__Issuer` env vars               |
| Identity       | ASP.NET Core Identity | `ApplicationUser.cs` in Infrastructure              |
| MFA Support    | Yes                   | `UserMfaConfiguration.cs`, `IMfaService.cs`         |
| OAuth          | Yes                   | `OAuthLinkedAccount.cs`, `IOAuthService.cs`         |
| Password Reset | Yes                   | `IPasswordResetService.cs`, `PasswordResetToken.cs` |

### 4.2 Authorization

| Attribute   | Value                                          | Evidence                          |
| ----------- | ---------------------------------------------- | --------------------------------- |
| Model       | Role-Based Access Control (RBAC)               | `IRbacService.cs` interface       |
| Enforcement | Server-side via `BranchAuthorizationAttribute` | `BranchAuthorizationAttribute.cs` |
| UI Guards   | `ProtectedRoute` component with role checks    | `ProtectedRoute.tsx`              |
| Permissions | Granular permission codes per action           | `IRbacService.HasPermissionAsync` |

**Defined Roles (from `App.tsx`):**

- `ADMIN`, `DOCTOR`, `NURSE`, `RECEPTIONIST`
- `LAB_TECHNICIAN`, `PHARMACIST`, `RADIOLOGIST`
- `HR_MANAGER`, `ACCOUNTANT`, `MARKETING_MANAGER`

**Authorization Attributes:**

- `BranchAuthorizationAttribute` - Branch-level access control
- `RequireBranchAccessAttribute` - TypeFilter for branch validation

### 4.3 Multi-Level Access Control

| Level         | Implementation              | Evidence                             |
| ------------- | --------------------------- | ------------------------------------ |
| Super Admin   | `IsSuperAdmin` bypass       | `BranchAuthorizationAttribute.cs:40` |
| Company Admin | `IsCompanyAdmin` check      | `BranchAuthorizationAttribute.cs:47` |
| Branch Access | `HasBranchAccess(branchId)` | `BranchAuthorizationAttribute.cs:70` |

---

## 5. Testing Infrastructure

### 5.1 Backend Tests (XenonClinic.Tests)

| Attribute    | Value                            | Evidence                         |
| ------------ | -------------------------------- | -------------------------------- |
| Framework    | xUnit 2.6.2                      | `XenonClinic.Tests.csproj:13`    |
| Mocking      | Moq 4.20.70                      | `XenonClinic.Tests.csproj:17`    |
| Assertions   | FluentAssertions 6.12.0          | `XenonClinic.Tests.csproj:18`    |
| In-Memory DB | EF Core InMemory 8.0.8           | `XenonClinic.Tests.csproj:19`    |
| Integration  | Microsoft.AspNetCore.Mvc.Testing | `Xenon.Platform.Tests.csproj:20` |

**Test Categories (from file names):**

- Unit: `PatientValidatorTests`, `AppointmentValidatorTests`, `FinancialValidatorTests`
- Service: `PatientServiceTests`, `AppointmentServiceTests`, `SalesServiceTests`
- Integration: `SalesIntegrationTests`
- E2E: `SalesE2ETests`
- API: `ValidationFilterTests`, `ExceptionHandlingTests`, `CorrelationIdMiddlewareTests`

### 5.2 Workflow Engine Tests (XenonClinic.WorkflowEngine.Tests)

| Attribute | Value                               | Evidence            |
| --------- | ----------------------------------- | ------------------- |
| Framework | xUnit                               | Project file        |
| Purpose   | Workflow definition/execution tests | Directory structure |

### 5.3 Platform Tests (Xenon.Platform.Tests)

| Attribute   | Value                 | Evidence                                     |
| ----------- | --------------------- | -------------------------------------------- |
| Framework   | xUnit                 | `Xenon.Platform.Tests.csproj`                |
| Integration | WebApplicationFactory | `Microsoft.AspNetCore.Mvc.Testing` reference |

### 5.4 Frontend Tests

| Attribute         | Value                      | Evidence                         |
| ----------------- | -------------------------- | -------------------------------- |
| Framework         | Vitest                     | `XenonClinic.React/package.json` |
| Component Testing | Testing Library React      | `@testing-library/react`         |
| User Events       | Testing Library User Event | `@testing-library/user-event`    |

**Existing Tests:**

- `ProtectedRoute.test.tsx`
- `StatusBadge.test.tsx`
- `Modal.test.tsx`
- `NotFound.test.tsx`
- `Forbidden.test.tsx`

### 5.5 End-to-End Tests (tests/e2e)

| Attribute | Value                  | Evidence                         |
| --------- | ---------------------- | -------------------------------- |
| Framework | Playwright 1.40.0      | `tests/e2e/package.json:18`      |
| Config    | `playwright.config.ts` | `tests/e2e/playwright.config.ts` |

**Test Suites (from file names):**

- Admin: dashboard, patients, appointments, clinical-visits, laboratory, inventory, financial, hr, payroll, marketing, analytics, workflow, sales, specialty-modules, security-audit, multi-tenancy, patient-portal
- Public: home

---

## 6. Build & DevOps

### 6.1 Package Management

| Tool           | Purpose             | Evidence                             |
| -------------- | ------------------- | ------------------------------------ |
| npm            | Node.js packages    | `package.json` (root + workspaces)   |
| npm workspaces | Monorepo management | `package.json:6-10`                  |
| NuGet          | .NET packages       | `.csproj` files                      |
| dotnet         | .NET CLI            | `dotnet restore/build/test` commands |

### 6.2 Build Tools

| Tool       | Purpose           | Evidence                 |
| ---------- | ----------------- | ------------------------ |
| Vite       | Frontend bundling | `vite.config.ts` files   |
| TypeScript | Type checking     | `tsconfig.json` files    |
| ESLint     | Linting           | `eslint.config.js` files |
| Prettier   | Formatting        | `lint-staged` config     |
| Husky      | Git hooks         | `.husky/` directory      |

### 6.3 Docker Configuration

| File                     | Purpose               | Evidence           |
| ------------------------ | --------------------- | ------------------ |
| `docker-compose.yml`     | Production setup      | 5 services defined |
| `docker-compose.dev.yml` | Development overrides | Exists in root     |
| `Dockerfile.api`         | API image             | .NET 8 SDK/Runtime |
| `Dockerfile.admin`       | Admin app image       | Node + nginx       |
| `Dockerfile.public`      | Public website image  | Node + nginx       |

### 6.4 CI/CD

| File                           | Purpose           | Evidence               |
| ------------------------------ | ----------------- | ---------------------- |
| `.github/workflows/ci.yml`     | Build, test, lint | All projects           |
| `.github/workflows/deploy.yml` | Deployment        | staging/production     |
| `.github/workflows/pr.yml`     | PR checks         | Size, breaking changes |

**CI Jobs:**

1. `backend` - .NET restore, build, test with coverage
2. `shared-ui` - Build shared library + Storybook
3. `admin-app` - Build admin React app
4. `public-website` - Build public React app
5. `security` - Security scanning + secret detection

---

## 7. Configuration

### 7.1 Environment Variables

| Variable                  | Purpose                     | Required |
| ------------------------- | --------------------------- | -------- |
| `DB_USER`                 | Database username           | Yes      |
| `DB_PASSWORD`             | Database password           | Yes      |
| `DB_NAME`                 | Database name               | Yes      |
| `JWT_SECRET`              | JWT signing key (32+ chars) | Yes      |
| `JWT_ISSUER`              | JWT issuer                  | Yes      |
| `JWT_AUDIENCE`            | JWT audience                | Yes      |
| `ASPNETCORE_ENVIRONMENT`  | Runtime environment         | Yes      |
| `CORS_ALLOWED_ORIGINS`    | CORS whitelist              | Yes      |
| `ADMIN_API_URL`           | API URL for admin app       | Yes      |
| `PUBLIC_API_URL`          | API URL for public site     | Yes      |
| `REDIS_CONNECTION_STRING` | Redis connection            | Optional |

### 7.2 Security Configuration

| File                        | Purpose           | Evidence           |
| --------------------------- | ----------------- | ------------------ |
| `appsettings.Security.json` | Security settings | `XenonClinic.Api/` |

---

## 8. Module Inventory

### 8.1 Core Modules (from `App.tsx` routes)

1. **Patient Management** - `/patients`
2. **Appointments** - `/appointments`
3. **Clinical Visits** - `/clinical-visits`
4. **Laboratory** - `/laboratory`
5. **Radiology** - `/radiology`
6. **Pharmacy** - `/pharmacy`
7. **Inventory** - `/inventory`
8. **Financial** - `/financial`
9. **HR** - `/hr`, `/hr/employees`, `/hr/payroll`, `/hr/salary-structures`
10. **Marketing** - `/marketing`
11. **Analytics** - `/analytics`, `/analytics/reports`
12. **Workflow** - `/workflow`, `/workflow/definitions`, `/workflow/instances`
13. **Admin** - `/admin`, `/admin/translations`
14. **Audiology** - `/audiology`

### 8.2 Patient Portal

- Registration: `/portal/register`
- Login: `/portal/login`
- Dashboard: `/portal/dashboard`
- Profile: `/portal/profile`
- Documents: `/portal/documents`
- Appointments: `/portal/appointments`

### 8.3 Specialty Modules (14 total)

| Module        | Routes             | Entities                                     |
| ------------- | ------------------ | -------------------------------------------- |
| Dental        | `/dental/*`        | ToothChart, DentalVisit, DentalProcedure     |
| Cardiology    | `/cardiology/*`    | ECGRecord, EchoResult, StressTest            |
| Ophthalmology | `/ophthalmology/*` | EyeExam, VisionTest, EyePrescription         |
| Orthopedics   | `/orthopedics/*`   | OrthoVisit, OrthoInjury, JointAssessment     |
| Dermatology   | `/dermatology/*`   | LesionRecord, SkinCondition, SkinProcedure   |
| Oncology      | `/oncology/*`      | CancerDiagnosis, ChemotherapySession         |
| Neurology     | `/neurology/*`     | NeuroExam, EEGRecord, NerveStudy             |
| Pediatrics    | `/pediatrics/*`    | GrowthRecord, VaccinationRecord              |
| OB/GYN        | `/obgyn/*`         | PregnancyRecord, PrenatalVisit, ObUltrasound |
| Physiotherapy | `/physiotherapy/*` | PhysioSession, ExerciseProgram               |
| ENT           | `/ent/*`           | HearingScreening, ENTProcedure               |
| Fertility     | `/fertility/*`     | IVFCycle, EmbryoRecord, SpermAnalysis        |
| Dialysis      | `/dialysis/*`      | DialysisSession, FluidBalance                |

---

## 9. Identified Risks & Gaps

### 9.1 Security Considerations

1. **RBAC Implementation** - Server-side enforcement via `BranchAuthorizationAttribute` is good, but needs verification that all endpoints are protected.
2. **JWT Secret** - Env var based, ensure min 32 chars in production.
3. **Multi-tenancy Isolation** - Verify tenant data isolation in all queries.

### 9.2 Testing Gaps (Preliminary)

1. Frontend tests exist but coverage appears limited (5 test files found).
2. E2E tests exist but need verification of coverage against all routes.
3. No visible API integration test suite for the main clinic API.

### 9.3 Configuration Risks

1. `.env.example` contains placeholder secrets - ensure production uses unique values.
2. CORS configuration needs verification for production.

---

## 10. Next Steps

1. **RUNBOOK.md** - Document exact steps to run locally
2. **Verify builds** - Ensure all projects compile without errors
3. **SYSTEM_MAP.md** - Map all UI routes to backend endpoints
4. **RBAC_MATRIX.md** - Map roles to permissions and endpoints
5. **JOURNEYS.md** - Define critical user journeys for testing
