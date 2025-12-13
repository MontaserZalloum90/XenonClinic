# XenonClinic - Test Strategy

**Generated:** 2025-12-13
**Phase:** 3 - Test Strategy
**Status:** Production Readiness Audit

---

## 1. Testing Infrastructure Overview

### 1.1 Existing Test Frameworks (Evidence-Based)

| Layer               | Framework              | Version | Config Location                              |
| ------------------- | ---------------------- | ------- | -------------------------------------------- |
| **Frontend Unit**   | Vitest                 | 3.2.3   | `XenonClinic.React/package.json`             |
| **Frontend E2E**    | Playwright             | 1.52.0  | `tests/e2e/playwright.config.ts`             |
| **Backend Unit**    | xUnit                  | 2.6.2   | `XenonClinic.Tests/XenonClinic.Tests.csproj` |
| **Mocking**         | Moq                    | 4.20.70 | `XenonClinic.Tests/XenonClinic.Tests.csproj` |
| **Assertions**      | FluentAssertions       | 6.12.0  | `XenonClinic.Tests/XenonClinic.Tests.csproj` |
| **Testing Library** | @testing-library/react | 16.3.0  | `XenonClinic.React/package.json`             |

### 1.2 Current Test Coverage

| Category      | Files      | Tests          | Evidence                       |
| ------------- | ---------- | -------------- | ------------------------------ |
| Frontend Unit | 12         | 259 (257 pass) | `src/**/*.test.{ts,tsx}`       |
| E2E Specs     | 22         | ~300+          | `tests/e2e/tests/**/*.spec.ts` |
| Backend Unit  | 3 projects | TBD            | `*.Tests.csproj` files         |

### 1.3 Test Projects Structure

```
/tests/e2e/
├── playwright.config.ts          # Multi-project config (admin, public, api)
├── fixtures/
│   └── auth.ts                   # Auth helpers and test users
└── tests/
    ├── admin/                    # 20 spec files
    │   ├── auth.spec.ts
    │   ├── patients.spec.ts
    │   ├── appointments.spec.ts
    │   ├── clinical-visits.spec.ts
    │   ├── laboratory.spec.ts
    │   ├── financial.spec.ts
    │   └── ...
    ├── public/                   # 1 spec file
    │   └── home.spec.ts
    └── api/                      # 1 spec file
        └── health.spec.ts
```

---

## 2. Automation Strategy

### 2.1 What Gets Automated

| Category                 | Test Type   | Automation Level | Rationale                           |
| ------------------------ | ----------- | ---------------- | ----------------------------------- |
| **P0 Critical Paths**    | E2E         | 100%             | Must pass for production gate       |
| **API Contracts**        | Integration | 100%             | Backend stability critical          |
| **Component Rendering**  | Unit        | 100%             | Fast feedback, regression safety    |
| **Form Validation**      | Unit + E2E  | 100%             | User input is primary attack vector |
| **Authentication Flows** | E2E         | 100%             | Security-critical                   |
| **RBAC Enforcement**     | E2E + API   | 100%             | Security-critical                   |
| **Data CRUD Operations** | E2E + API   | 100%             | Core functionality                  |
| **Edge Cases**           | Unit        | 100%             | Prevent regressions                 |

### 2.2 What Stays Manual

| Category                    | Reason                              | Frequency         |
| --------------------------- | ----------------------------------- | ----------------- |
| **Visual Design Review**    | Subjective assessment               | Per release       |
| **Exploratory Testing**     | Human intuition finds edge cases    | Weekly            |
| **Performance Profiling**   | Requires context interpretation     | Monthly           |
| **Accessibility Audit**     | Some aspects require human judgment | Per feature       |
| **Usability Testing**       | UX feedback subjective              | Per major feature |
| **Third-party Integration** | External dependencies               | Per integration   |

### 2.3 Test Pyramid Distribution

```
                    ┌─────────────────┐
                    │   E2E Tests     │ ← 20% (Playwright)
                    │   P0 Journeys   │   Slow, comprehensive
                    ├─────────────────┤
                    │                 │
                    │  Integration    │ ← 30% (API tests)
                    │     Tests       │   Medium speed
                    │                 │
                    ├─────────────────┤
                    │                 │
                    │                 │
                    │   Unit Tests    │ ← 50% (Vitest/xUnit)
                    │   Components    │   Fast, isolated
                    │   Functions     │
                    │                 │
                    └─────────────────┘
```

---

## 3. CI/CD Gating Rules

### 3.1 Pull Request Gates

| Gate                 | Requirement      | Blocks Merge | Evidence                              |
| -------------------- | ---------------- | ------------ | ------------------------------------- |
| **TypeScript Build** | 0 errors         | Yes          | `npm run build`                       |
| **Unit Tests**       | 100% pass        | Yes          | `npm run test`                        |
| **Lint**             | 0 errors         | Yes          | `npm run lint`                        |
| **E2E Smoke**        | P0 journeys pass | Yes          | `npx playwright test --project=admin` |
| **Backend Build**    | Compiles         | Yes          | `dotnet build`                        |
| **Backend Tests**    | 100% pass        | Yes          | `dotnet test`                         |

### 3.2 Deployment Gates

| Environment     | Required Gates                | Additional           |
| --------------- | ----------------------------- | -------------------- |
| **Development** | PR gates pass                 | Auto-deploy on merge |
| **Staging**     | All PR gates + full E2E suite | Manual trigger       |
| **Production**  | Staging gates + smoke tests   | Manual approval      |

### 3.3 Proposed GitHub Actions Workflow

```yaml
# .github/workflows/ci.yml
name: CI

on:
  pull_request:
    branches: [main, develop]
  push:
    branches: [main, develop]

jobs:
  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: "20"
          cache: "npm"

      - name: Install dependencies
        run: npm ci

      - name: Type check
        run: npm run typecheck -w XenonClinic.React

      - name: Lint
        run: npm run lint -w XenonClinic.React

      - name: Unit tests
        run: npm run test -w XenonClinic.React -- --coverage

      - name: Build
        run: npm run build -w XenonClinic.React

  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

  e2e-tests:
    runs-on: ubuntu-latest
    needs: [frontend-tests, backend-tests]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: "20"
          cache: "npm"

      - name: Install dependencies
        run: npm ci

      - name: Install Playwright browsers
        run: npx playwright install --with-deps chromium

      - name: Run E2E tests
        run: npx playwright test --project=admin
        env:
          TEST_ADMIN_EMAIL: ${{ secrets.TEST_ADMIN_EMAIL }}
          TEST_ADMIN_PASSWORD: ${{ secrets.TEST_ADMIN_PASSWORD }}

      - name: Upload test results
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: tests/e2e/playwright-report/
```

---

## 4. Regression Policy

### 4.1 Bug-First Testing

**Rule:** Every bug gets a failing test BEFORE the fix is implemented.

| Step | Action             | Deliverable               |
| ---- | ------------------ | ------------------------- |
| 1    | Reproduce bug      | Steps documented in issue |
| 2    | Write failing test | Test file committed       |
| 3    | Verify test fails  | CI shows red              |
| 4    | Implement fix      | Code changes              |
| 5    | Verify test passes | CI shows green            |
| 6    | Peer review        | PR approved               |

### 4.2 Regression Test Categories

| Category                 | Trigger                  | Scope             |
| ------------------------ | ------------------------ | ----------------- |
| **Smoke Tests**          | Every PR                 | P0 journeys only  |
| **Full Regression**      | Release candidate        | All E2E + Unit    |
| **Security Regression**  | Security-related changes | Auth + RBAC tests |
| **Performance Baseline** | Performance changes      | Load tests        |

### 4.3 Test Stability Rules

| Issue                  | Action                       | Threshold              |
| ---------------------- | ---------------------------- | ---------------------- |
| Flaky test detected    | Quarantine + investigate     | 2 failures in 10 runs  |
| Test timeout           | Increase timeout or optimize | 3x average duration    |
| Environment dependency | Mock or skip in CI           | Documented skip reason |

---

## 5. Test Data Strategy

### 5.1 Data Sources

| Environment    | Data Source          | Evidence                                    |
| -------------- | -------------------- | ------------------------------------------- |
| **Unit Tests** | Mocked/In-memory     | `EF.InMemory` in `XenonClinic.Tests.csproj` |
| **E2E Local**  | Docker PostgreSQL    | `docker-compose.yml`                        |
| **E2E CI**     | Docker PostgreSQL    | CI workflow setup                           |
| **API Tests**  | Seeded test database | Migration + seed scripts                    |

### 5.2 Test User Management

From `tests/e2e/fixtures/auth.ts`:

```typescript
// Test credentials loaded from environment variables
const testUsers = {
  admin: {
    email: process.env.TEST_ADMIN_EMAIL || "admin@test.com",
    password: process.env.TEST_ADMIN_PASSWORD || "TestPassword123!",
  },
  doctor: {
    email: process.env.TEST_DOCTOR_EMAIL || "doctor@test.com",
    password: process.env.TEST_DOCTOR_PASSWORD || "TestPassword123!",
  },
  receptionist: {
    email: process.env.TEST_RECEPTIONIST_EMAIL || "receptionist@test.com",
    password: process.env.TEST_RECEPTIONIST_PASSWORD || "TestPassword123!",
  },
};
```

### 5.3 Test Data Isolation Strategy

| Approach                 | When to Use       | Implementation                        |
| ------------------------ | ----------------- | ------------------------------------- |
| **Fresh Database**       | E2E suite start   | `docker-compose down -v && up`        |
| **Transaction Rollback** | Unit tests        | `BeginTransaction()` + `Rollback()`   |
| **Unique Identifiers**   | Create operations | `Date.now()` or UUID suffix           |
| **Cleanup After**        | CRUD tests        | Delete created records in `afterEach` |

### 5.4 Test Data Factory Pattern

```typescript
// Recommended: Test data factories
const TestDataFactory = {
  patient: (overrides = {}) => ({
    firstName: `E2E_${Date.now()}`,
    lastName: "TestPatient",
    dateOfBirth: "1990-01-15",
    gender: "Male",
    phone: `+97150${Math.floor(Math.random() * 10000000)}`,
    email: `test_${Date.now()}@example.com`,
    ...overrides,
  }),

  appointment: (patientId: string, overrides = {}) => ({
    patientId,
    providerId: "test-provider-id",
    appointmentDate: new Date(Date.now() + 86400000).toISOString(),
    appointmentType: "Consultation",
    status: "Scheduled",
    ...overrides,
  }),
};
```

---

## 6. P0 Journey Test Implementation Plan

### 6.1 Current Coverage Analysis

| Journey ID   | E2E File                  | Coverage Status            |
| ------------ | ------------------------- | -------------------------- |
| AUTH-JNY-001 | `auth.spec.ts`            | ✅ Comprehensive           |
| AUTH-JNY-002 | `auth.spec.ts`            | ✅ Role-Based Access tests |
| PAT-JNY-001  | `patients.spec.ts`        | ✅ Create Patient tests    |
| PAT-JNY-002  | `patients.spec.ts`        | ✅ Search tests            |
| APT-JNY-001  | `appointments.spec.ts`    | ⚠️ Verify coverage         |
| APT-JNY-002  | `appointments.spec.ts`    | ⚠️ Verify check-in         |
| CLN-JNY-001  | `clinical-visits.spec.ts` | ⚠️ Verify vitals           |
| CLN-JNY-002  | `clinical-visits.spec.ts` | ⚠️ Verify diagnosis        |
| LAB-JNY-001  | `laboratory.spec.ts`      | ⚠️ Verify order creation   |
| FIN-JNY-001  | `financial.spec.ts`       | ⚠️ Verify invoice          |

### 6.2 Gap Analysis Required

Tests to verify/add:

1. **APT-JNY-002:** Patient check-in flow with status update
2. **CLN-JNY-001:** Vitals recording with validation
3. **CLN-JNY-002:** ICD-10 diagnosis selection and linking
4. **LAB-JNY-001:** Multi-test order with priority
5. **FIN-JNY-001:** Invoice line items with calculation

---

## 7. Test Execution Commands

### 7.1 Local Development

```bash
# Frontend unit tests
npm run test -w XenonClinic.React

# Frontend unit tests with coverage
npm run test -w XenonClinic.React -- --coverage

# E2E tests (requires running app)
cd tests/e2e && npx playwright test

# E2E with UI mode
cd tests/e2e && npx playwright test --ui

# Specific E2E project
cd tests/e2e && npx playwright test --project=admin

# Single E2E spec file
cd tests/e2e && npx playwright test tests/admin/patients.spec.ts

# Backend tests
dotnet test XenonClinic.Tests
```

### 7.2 CI Commands

```bash
# Full frontend pipeline
npm ci && npm run typecheck -w XenonClinic.React && npm run test -w XenonClinic.React && npm run build -w XenonClinic.React

# Full backend pipeline
dotnet restore && dotnet build && dotnet test

# E2E with CI flags
CI=true npx playwright test --project=admin --reporter=github
```

---

## 8. Test Environment Requirements

### 8.1 Playwright Configuration

From `tests/e2e/playwright.config.ts`:

```typescript
{
  testDir: "./tests",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,      // Prevent .only in CI
  retries: process.env.CI ? 2 : 0,   // Retry in CI
  workers: process.env.CI ? 1 : undefined,  // Single worker in CI

  use: {
    baseURL: process.env.BASE_URL || "http://localhost:5173",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "on-first-retry",
  },

  webServer: [
    {
      command: "npm run dev -w XenonClinic.React",
      url: "http://localhost:5173",
      reuseExistingServer: !process.env.CI,
    },
  ],
}
```

### 8.2 Required Environment Variables

| Variable               | Purpose         | Required In   |
| ---------------------- | --------------- | ------------- |
| `TEST_ADMIN_EMAIL`     | Admin login     | CI            |
| `TEST_ADMIN_PASSWORD`  | Admin password  | CI            |
| `TEST_DOCTOR_EMAIL`    | Doctor login    | CI (optional) |
| `TEST_DOCTOR_PASSWORD` | Doctor password | CI (optional) |
| `BASE_URL`             | Frontend URL    | Optional      |
| `API_URL`              | Backend URL     | Optional      |

---

## 9. Known Issues and Workarounds

### 9.1 TypeScript Build Errors

**Issue:** 610 TypeScript errors prevent production build
**Impact:** Tests can run in dev mode, but production build fails
**Workaround:** Run tests with `npm run dev` instead of build
**Fix Required:** Address TS errors categorized in DEFECTS.md

### 9.2 Test Failures

| Test                      | Issue                   | Fix Status               |
| ------------------------- | ----------------------- | ------------------------ |
| `ProtectedRoute.test.tsx` | Expects 9 roles, has 10 | Fix test assertion       |
| `api.test.ts`             | Header check undefined  | Fix axios defaults setup |

### 9.3 Flaky Test Patterns

| Pattern            | Cause           | Mitigation                      |
| ------------------ | --------------- | ------------------------------- |
| `waitForTimeout()` | Fixed waits     | Use `waitFor()` with conditions |
| Network timeouts   | Slow API        | Increase timeout, add retries   |
| Selector misses    | Dynamic content | Use data-testid attributes      |

---

## 10. Metrics and Reporting

### 10.1 Coverage Targets

| Metric                | Target | Current | Gap         |
| --------------------- | ------ | ------- | ----------- |
| Unit test coverage    | 80%    | TBD     | Measure     |
| E2E P0 coverage       | 100%   | ~80%    | Verify gaps |
| API endpoint coverage | 70%    | TBD     | Measure     |
| Branch coverage       | 70%    | TBD     | Measure     |

### 10.2 Reporting

| Report             | Format         | Location                       |
| ------------------ | -------------- | ------------------------------ |
| Unit test coverage | HTML           | `coverage/`                    |
| E2E test report    | HTML           | `tests/e2e/playwright-report/` |
| E2E trace files    | ZIP            | `tests/e2e/test-results/`      |
| CI summary         | GitHub Actions | PR checks                      |

---

## 11. Next Steps (Priority Order)

1. **Run existing E2E tests** to establish baseline
2. **Verify P0 journey coverage** in existing specs
3. **Add missing P0 tests** for gaps identified
4. **Fix the 2 failing unit tests**
5. **Set up CI pipeline** with gating rules
6. **Address TypeScript errors** blocking production build
7. **Add coverage reporting** to CI
8. **Document test patterns** for team adoption

---

## Appendix A: Test File Inventory

### Frontend Unit Tests (12 files)

| File                                 | Tests              | Status    |
| ------------------------------------ | ------------------ | --------- |
| `components/ProtectedRoute.test.tsx` | Role guards        | 1 failing |
| `components/ui/Modal.test.tsx`       | Modal behavior     | Passing   |
| `components/ui/StatusBadge.test.tsx` | Badge rendering    | Passing   |
| `lib/api.test.ts`                    | API client         | 1 failing |
| `pages/Error/Forbidden.test.tsx`     | 403 page           | Passing   |
| `pages/Error/NotFound.test.tsx`      | 404 page           | Passing   |
| `test/demo.test.ts`                  | Demo tests         | Passing   |
| `test/enterpriseSecurity.test.ts`    | Security tests     | Passing   |
| `test/multiTenant.test.ts`           | Multi-tenant tests | Passing   |
| `test/security.test.ts`              | Security tests     | Passing   |
| `types/audiology.test.ts`            | Type tests         | Passing   |
| `types/appointment.test.ts`          | Type tests         | Passing   |

### E2E Test Specs (22 files)

| File                        | Focus              | P0 Coverage         |
| --------------------------- | ------------------ | ------------------- |
| `auth.spec.ts`              | Authentication     | ✅ AUTH-JNY-001/002 |
| `patients.spec.ts`          | Patient management | ✅ PAT-JNY-001/002  |
| `appointments.spec.ts`      | Scheduling         | ⚠️ APT-JNY-001/002  |
| `clinical-visits.spec.ts`   | Clinical           | ⚠️ CLN-JNY-001/002  |
| `laboratory.spec.ts`        | Lab orders         | ⚠️ LAB-JNY-001      |
| `financial.spec.ts`         | Invoicing          | ⚠️ FIN-JNY-001      |
| `dashboard.spec.ts`         | Dashboard          | N/A                 |
| `inventory.spec.ts`         | Inventory          | P1                  |
| `pharmacy.spec.ts`          | Pharmacy           | P1                  |
| `radiology.spec.ts`         | Radiology          | P1                  |
| `hr.spec.ts`                | HR                 | P1                  |
| `payroll.spec.ts`           | Payroll            | P1                  |
| `workflow.spec.ts`          | Workflow           | P1                  |
| `analytics.spec.ts`         | Analytics          | P2                  |
| `marketing.spec.ts`         | Marketing          | P2                  |
| `sales.spec.ts`             | Sales              | P2                  |
| `specialty-modules.spec.ts` | Specialties        | P2                  |
| `multi-tenancy.spec.ts`     | Multi-tenant       | P1                  |
| `patient-portal.spec.ts`    | Portal             | P2                  |
| `security-audit.spec.ts`    | Security           | P0                  |
| `health.spec.ts`            | API health         | N/A                 |
| `home.spec.ts`              | Public home        | N/A                 |
