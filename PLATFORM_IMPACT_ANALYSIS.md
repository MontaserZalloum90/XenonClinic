# Xenon Platform - Impact Analysis

## Executive Summary

This document outlines the changes required to implement the Xenon Platform Backend API and integrate it with the existing marketing website.

---

## 1. Files to Modify in Website (`XenonClinic.Website/`)

### API Routes (Replace demo implementations with real backend calls)
| File | Current State | Change |
|------|---------------|--------|
| `app/api/auth/login/route.ts` | Demo login with hardcoded users | Remove - use platform backend |
| `app/api/leads/route.ts` | In-memory storage | Remove - use platform backend |
| `app/api/tenants/route.ts` | In-memory storage | Remove - use platform backend |

### Pages to Update
| File | Change |
|------|--------|
| `app/(auth)/login/page.tsx` | Connect to `/api/public/tenants/login` |
| `app/(auth)/signup/page.tsx` | Connect to `/api/public/tenants/signup` |
| `app/(marketing)/pricing/page.tsx` | Add live pricing calculator via `/api/public/pricing/estimate` |
| `app/(marketing)/contact/page.tsx` | Connect form to `/api/public/demo-request` |
| `app/(marketing)/demo/page.tsx` | Connect to `/api/public/demo-request` |

### New Files to Add
| File | Purpose |
|------|---------|
| `lib/platform-api.ts` | Platform API client with axios/fetch |
| `app/(auth)/tenant-login/page.tsx` | Dedicated tenant login page |
| `app/(dashboard)/tenant/page.tsx` | Tenant dashboard after login |
| `components/signup/SignupWizard.tsx` | Multi-step signup wizard component |
| `components/pricing/PricingCalculator.tsx` | Live pricing calculator component |

### Configuration
| File | Change |
|------|--------|
| `.env.local` | Add `PLATFORM_API_URL` |
| `next.config.js` | Add API proxy configuration (optional) |

---

## 2. New Routes/Pages to Add

### Authentication Routes
```
/tenant-login         → Tenant realm login
/admin-login          → Platform admin login (optional link in footer)
/signup               → Multi-step tenant signup wizard
/signup/success       → Post-signup confirmation
```

### Dashboard Routes (Tenant Portal)
```
/tenant               → Tenant dashboard
/tenant/billing       → Subscription & billing management
/tenant/usage         → Usage metrics
/tenant/settings      → Tenant settings
```

### Admin Routes (Platform Admin Portal - Optional)
```
/platform-admin       → Platform admin login
/platform-admin/dashboard → KPIs dashboard
/platform-admin/tenants   → Tenant management
```

---

## 3. Backend Projects to Add

### New .NET 8 Solution Structure
```
Xenon.Platform/
├── Xenon.Platform.sln
├── src/
│   ├── Xenon.Platform.Api/           # Web API project
│   │   ├── Controllers/
│   │   │   ├── Public/               # Public endpoints
│   │   │   │   ├── PricingController.cs
│   │   │   │   ├── TenantsController.cs
│   │   │   │   └── DemoRequestController.cs
│   │   │   ├── PlatformAdmin/        # Admin endpoints
│   │   │   │   ├── AuthController.cs
│   │   │   │   ├── DashboardController.cs
│   │   │   │   ├── TenantsController.cs
│   │   │   │   ├── MonitoringController.cs
│   │   │   │   └── ReportsController.cs
│   │   │   └── Tenant/               # Tenant realm endpoints
│   │   │       ├── LicenseController.cs
│   │   │       └── UsageController.cs
│   │   ├── Middleware/
│   │   │   ├── TenantAuthenticationMiddleware.cs
│   │   │   └── PlatformAdminAuthenticationMiddleware.cs
│   │   ├── BackgroundServices/
│   │   │   ├── TenantHealthCheckService.cs
│   │   │   ├── UsageMeteringService.cs
│   │   │   └── SubscriptionExpiryService.cs
│   │   └── Program.cs
│   │
│   ├── Xenon.Platform.Application/   # Application layer
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   └── Models/
│   │   ├── Tenants/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTenantCommand.cs
│   │   │   │   ├── SuspendTenantCommand.cs
│   │   │   │   └── ExtendTrialCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetTenantQuery.cs
│   │   │   │   └── GetTenantsQuery.cs
│   │   │   └── Handlers/
│   │   ├── Subscriptions/
│   │   ├── Monitoring/
│   │   ├── Reports/
│   │   └── UsageMetering/
│   │
│   ├── Xenon.Platform.Domain/        # Domain layer
│   │   ├── Entities/
│   │   │   ├── Tenant.cs
│   │   │   ├── TenantAdmin.cs
│   │   │   ├── Plan.cs
│   │   │   ├── Subscription.cs
│   │   │   ├── PlatformAdmin.cs
│   │   │   ├── DemoRequest.cs
│   │   │   ├── UsageSnapshot.cs
│   │   │   ├── TenantHealthCheck.cs
│   │   │   └── AuditLog.cs
│   │   ├── ValueObjects/
│   │   │   ├── TenantSlug.cs
│   │   │   ├── LicenseGuardrails.cs
│   │   │   └── UsageMetrics.cs
│   │   ├── Enums/
│   │   │   ├── TenantStatus.cs
│   │   │   ├── SubscriptionStatus.cs
│   │   │   └── PlanCode.cs
│   │   └── Events/
│   │
│   └── Xenon.Platform.Infrastructure/ # Infrastructure layer
│       ├── Persistence/
│       │   ├── PlatformDbContext.cs
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Services/
│       │   ├── TenantProvisioningService.cs
│       │   ├── TenantDatabaseService.cs
│       │   ├── JwtTokenService.cs
│       │   ├── PasswordHashingService.cs
│       │   ├── PricingCalculatorService.cs
│       │   └── HealthCheckService.cs
│       └── Repositories/
│
└── tests/
    ├── Xenon.Platform.Api.Tests/
    ├── Xenon.Platform.Application.Tests/
    └── Xenon.Platform.Infrastructure.Tests/
```

---

## 4. Database Architecture

### Platform Database (Single DB)
Stores all platform-level data:

```sql
-- Core Tenant Management
Tenants                 -- Tenant master records
TenantAdmins           -- Admin users per tenant (for platform login)
TenantDatabases        -- Connection info for tenant DBs

-- Plans & Subscriptions
Plans                  -- STARTER, GROWTH, ENTERPRISE
Subscriptions          -- Active subscriptions
SubscriptionHistory    -- Historical subscription changes

-- Platform Administration
PlatformAdmins         -- Xenon internal admins
PlatformRoles          -- Admin roles (SuperAdmin, Support, etc.)

-- Leads & Demo Requests
DemoRequests           -- Contact/demo form submissions

-- Monitoring & Usage
TenantHealthChecks     -- Periodic health check results
UsageSnapshots         -- Daily/hourly usage metrics
ApiCallLogs            -- API usage tracking

-- Audit & Reports
AuditLogs              -- All critical actions
MonthlyReports         -- Pre-computed report data
```

### Tenant Database Strategy: DB-per-Tenant
Each tenant gets their own database, provisioned automatically:

```
XenonPlatform          -- Platform DB (single)
XenonTenant_acme       -- Tenant "acme" DB
XenonTenant_globex     -- Tenant "globex" DB
XenonTenant_{slug}     -- Pattern: XenonTenant_{tenant_slug}
```

**Provisioning Flow:**
1. Tenant signs up → Create record in Platform DB
2. Generate connection string → `XenonTenant_{slug}`
3. Run EF Core migrations → Create tenant DB schema
4. Seed default data → Lookups, default roles, admin user
5. Store connection info → TenantDatabases table

---

## 5. Authentication Architecture

### Dual Realm JWT Strategy

| Realm | Issuer | Audience | Token Lifetime | Use Case |
|-------|--------|----------|----------------|----------|
| Tenant | `xenon-platform` | `xenon-tenant` | 24h | Tenant admin/user login |
| Platform Admin | `xenon-platform` | `xenon-admin` | 8h | Internal admin access |

### Token Claims

**Tenant Token:**
```json
{
  "sub": "user-id",
  "email": "admin@acme.com",
  "tenant_id": "tenant-guid",
  "tenant_slug": "acme",
  "role": "TENANT_ADMIN",
  "iss": "xenon-platform",
  "aud": "xenon-tenant"
}
```

**Platform Admin Token:**
```json
{
  "sub": "admin-id",
  "email": "support@xenon.ae",
  "role": "PLATFORM_ADMIN",
  "permissions": ["tenants:read", "tenants:write", "reports:read"],
  "iss": "xenon-platform",
  "aud": "xenon-admin"
}
```

### Middleware Chain
```
Request → JWT Validation → Realm Detection → Context Setup → Controller
                              ↓
                    [Tenant Realm]  or  [Admin Realm]
                         ↓                    ↓
               TenantContext.Current   AdminContext.Current
```

---

## 6. API Endpoint Summary

### Public Endpoints (No Auth)
| Method | Path | Purpose |
|--------|------|---------|
| POST | `/api/public/demo-request` | Submit demo/contact request |
| GET | `/api/public/pricing/estimate` | Calculate pricing |
| POST | `/api/public/tenants/signup` | Create trial tenant |
| POST | `/api/public/tenants/login` | Tenant realm login |
| GET | `/api/public/tenants/me` | Get current tenant context |

### Platform Admin Endpoints (Admin Auth)
| Method | Path | Purpose |
|--------|------|---------|
| POST | `/api/platform-admin/auth/login` | Admin login |
| GET | `/api/platform-admin/dashboard/kpis` | Dashboard metrics |
| GET | `/api/platform-admin/tenants` | List tenants |
| GET | `/api/platform-admin/tenants/{id}` | Tenant details |
| POST | `/api/platform-admin/tenants/{id}/suspend` | Suspend tenant |
| POST | `/api/platform-admin/tenants/{id}/activate` | Activate tenant |
| POST | `/api/platform-admin/tenants/{id}/extend-trial` | Extend trial |
| GET | `/api/platform-admin/tenants/{id}/usage` | Tenant usage |
| GET | `/api/platform-admin/monitoring/health` | Platform health |
| GET | `/api/platform-admin/reports/monthly` | Monthly report |

### Tenant Endpoints (Tenant Auth)
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/tenant/license` | License limits & usage |
| GET | `/api/tenant/usage` | Consumption metrics |

---

## 7. Integration Points with Existing ERP

### Minimal Changes to ERP Backend (`XenonClinic.Web`)
| Change | Purpose |
|--------|---------|
| Add license check middleware | Verify tenant has valid subscription |
| Add usage reporting endpoint | Report API calls to platform |
| Add platform token validation | Accept platform-issued tokens |

### ERP License Check Flow
```
ERP API Request → License Middleware → Check Platform API → Allow/Deny
                                              ↓
                           GET /api/tenant/license (cached 5 min)
```

---

## 8. Monitoring & Health Check Architecture

### Background Services
1. **TenantHealthCheckService** (runs every 5 min)
   - Ping each tenant DB
   - Record connectivity + latency
   - Alert on failures

2. **UsageMeteringService** (runs hourly)
   - Aggregate API calls
   - Count active users
   - Calculate storage usage

3. **SubscriptionExpiryService** (runs daily)
   - Check trial expirations
   - Send expiry notifications
   - Auto-suspend expired tenants

### Health Endpoints
- `GET /health` - Platform API health
- `GET /api/platform-admin/monitoring/health` - Detailed health with tenant status

---

## 9. Risk Assessment

| Risk | Mitigation |
|------|------------|
| Tenant DB provisioning failures | Retry logic + manual provision endpoint |
| Token realm confusion | Strict audience validation |
| Cross-tenant data access | Middleware enforces tenant isolation |
| Platform DB overload | Read replicas, caching |
| Subscription check latency | Local cache with 5-min TTL |

---

## 10. Implementation Order

1. **Phase 1: Platform Backend Core** (Priority)
   - Solution structure
   - Domain entities
   - Platform DB + migrations
   - JWT auth (both realms)

2. **Phase 2: Public API**
   - Demo request endpoint
   - Pricing calculator
   - Tenant signup + provisioning
   - Tenant login

3. **Phase 3: Platform Admin API**
   - Admin auth
   - Dashboard KPIs
   - Tenant management
   - Reports

4. **Phase 4: Monitoring**
   - Health checks
   - Usage metering
   - Background services

5. **Phase 5: Website Integration**
   - API client
   - Signup wizard
   - Login page updates
   - Pricing calculator UI

---

## Appendix: Environment Variables

### Platform API (.NET)
```
ConnectionStrings__PlatformDb=Server=...;Database=XenonPlatform;...
Jwt__SecretKey=your-256-bit-secret
Jwt__Issuer=xenon-platform
Jwt__TenantAudience=xenon-tenant
Jwt__AdminAudience=xenon-admin
TenantDb__ConnectionStringTemplate=Server=...;Database=XenonTenant_{0};...
```

### Website (Next.js)
```
NEXT_PUBLIC_PLATFORM_API_URL=https://platform-api.xenon.ae
```
