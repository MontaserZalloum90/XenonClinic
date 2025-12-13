# XenonClinic - Local Development Runbook

**Generated:** 2025-12-13
**Purpose:** Production Readiness Audit - Phase 0

---

## Prerequisites

### Required Software

| Software       | Version              | Verification Command     |
| -------------- | -------------------- | ------------------------ |
| .NET SDK       | 8.0.x                | `dotnet --version`       |
| Node.js        | 18+ (20 recommended) | `node --version`         |
| npm            | 9+                   | `npm --version`          |
| Docker         | 24+                  | `docker --version`       |
| Docker Compose | 2.x                  | `docker compose version` |

### Optional Software

| Software   | Purpose             | Verification          |
| ---------- | ------------------- | --------------------- |
| PostgreSQL | Direct DB access    | `psql --version`      |
| Redis CLI  | Direct cache access | `redis-cli --version` |

---

## Quick Start (Docker - Recommended)

### 1. Clone and Setup Environment

```bash
cd /home/user/XenonClinic

# Copy environment template
cp .env.example .env

# Edit .env and set secure values
# REQUIRED changes for production:
# - DB_PASSWORD (change from default)
# - JWT_SECRET (min 32 characters)
# - JWT_SECRET_KEY (min 32 characters)
```

### 2. Start All Services

```bash
# Start infrastructure (DB + Redis)
docker compose up -d db redis

# Wait for healthy status
docker compose ps

# Start API (after DB is healthy)
docker compose up -d api

# Start frontend apps
docker compose up -d admin public
```

### 3. Access Applications

| Application     | URL                           | Credentials                 |
| --------------- | ----------------------------- | --------------------------- |
| Admin Dashboard | http://localhost:3000         | admin@xenon.ae / Admin@123! |
| Public Website  | http://localhost:3001         | N/A                         |
| API             | http://localhost:5000         | N/A                         |
| Swagger Docs    | http://localhost:5000/swagger | N/A                         |
| Health Check    | http://localhost:5000/health  | N/A                         |

### 4. Stop Services

```bash
docker compose down        # Stop containers
docker compose down -v     # Stop and remove volumes (data loss!)
```

---

## Manual Development Setup

### Step 1: Backend Setup

#### 1.1 Restore .NET Dependencies

```bash
cd /home/user/XenonClinic

# Restore main solution
dotnet restore XenonClinic.sln

# Restore Platform solution
dotnet restore Xenon.Platform/Xenon.Platform.sln
```

#### 1.2 Build Backend

```bash
# Build main solution
dotnet build XenonClinic.sln --configuration Release

# Build Platform solution
dotnet build Xenon.Platform/Xenon.Platform.sln --configuration Release
```

#### 1.3 Run Backend Tests

```bash
# Run all .NET tests
dotnet test XenonClinic.sln --configuration Release --verbosity normal

# Run with coverage
dotnet test XenonClinic.sln --configuration Release --collect:"XPlat Code Coverage"
```

#### 1.4 Start Database (Docker)

```bash
# Start PostgreSQL only
docker compose up -d db

# Or start with Redis
docker compose up -d db redis

# Verify health
docker compose ps
```

#### 1.5 Run Database Migrations

```bash
# Apply migrations to Platform database
dotnet ef database update \
  --project Xenon.Platform/src/Xenon.Platform.Infrastructure \
  --startup-project Xenon.Platform/src/Xenon.Platform.Api
```

#### 1.6 Run API

```bash
# Run Platform API
dotnet run --project Xenon.Platform/src/Xenon.Platform.Api

# API will start at http://localhost:5000
```

### Step 2: Frontend Setup

#### 2.1 Install Root Dependencies

```bash
cd /home/user/XenonClinic

# Install root workspace dependencies (husky, lint-staged, etc.)
npm install
```

#### 2.2 Build Shared UI Library (Required First)

```bash
cd Shared.UI

# Install dependencies
npm install

# Build the library
npm run build

# Return to root
cd ..
```

#### 2.3 Admin Dashboard Setup

```bash
cd XenonClinic.React

# Install dependencies
npm install

# Start development server
npm run dev

# Admin app runs at http://localhost:5173
```

#### 2.4 Public Website Setup (Optional)

```bash
cd Xenon.PublicWebsite

# Install dependencies
npm install

# Start development server
npm run dev

# Public site runs at http://localhost:5174
```

### Step 3: Run Frontend Tests

```bash
# Run all frontend tests (from root)
npm test --workspaces

# Or run specific workspace tests
cd XenonClinic.React && npm test
cd Shared.UI && npm test
```

### Step 4: Run E2E Tests

```bash
cd tests/e2e

# Install dependencies
npm install

# Install Playwright browsers
npx playwright install

# Run all E2E tests (requires running app)
npm test

# Run specific project
npm run test:admin
npm run test:public

# Run in headed mode (visible browser)
npm run test:headed

# Run with UI
npm run test:ui
```

---

## Environment Configuration

### Required Environment Variables

Create `.env` file in root:

```bash
# Database Configuration
DB_USER=xenon
DB_PASSWORD=your_secure_password_here
DB_NAME=xenonclinic

# Platform Database (SQL Server format)
PLATFORM_DB_CONNECTION_STRING=Host=localhost;Database=XenonPlatform;Username=xenon;Password=your_secure_password_here

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development

# JWT Configuration (REQUIRED - min 32 characters)
JWT_SECRET_KEY=your-super-secret-jwt-key-that-must-be-at-least-32-characters-long
JWT_SECRET=your-super-secret-jwt-key-at-least-32-characters-long
JWT_ISSUER=XenonClinic
JWT_AUDIENCE=XenonClinicUsers

# CORS (comma-separated)
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173

# API URLs for frontend
ADMIN_API_URL=http://localhost:5000
PUBLIC_API_URL=http://localhost:5000
```

### Frontend Environment (.env files)

**XenonClinic.React/.env:**

```bash
VITE_API_URL=http://localhost:5000
```

**Xenon.PublicWebsite/.env:**

```bash
VITE_API_URL=http://localhost:5000
```

---

## Code Quality Commands

### Linting

```bash
# Lint all workspaces
npm run lint --workspaces

# Lint specific workspace
cd XenonClinic.React && npm run lint

# Format C# code
dotnet format XenonClinic.sln
```

### Type Checking

```bash
# TypeScript check
npm run typecheck --workspaces
```

### Storybook (Component Documentation)

```bash
cd Shared.UI
npm run storybook
# Opens at http://localhost:6006
```

---

## Troubleshooting

### Common Issues

#### 1. "Port already in use"

```bash
# Find process using port
lsof -i :5000
lsof -i :3000

# Kill process
kill -9 <PID>

# Or change port in config
```

#### 2. "Database connection failed"

```bash
# Check if DB container is running
docker compose ps

# Check DB logs
docker compose logs db

# Verify connection string in .env
```

#### 3. "npm install fails for @xenon/ui"

```bash
# Build shared library first
cd Shared.UI
npm install
npm run build

# Then install admin app
cd ../XenonClinic.React
npm install
```

#### 4. "EF Core migration errors"

```bash
# Install EF tools globally
dotnet tool install --global dotnet-ef

# Ensure correct startup project is specified
dotnet ef database update \
  --project Xenon.Platform/src/Xenon.Platform.Infrastructure \
  --startup-project Xenon.Platform/src/Xenon.Platform.Api
```

#### 5. "CORS errors in browser"

Ensure `CORS_ALLOWED_ORIGINS` includes your frontend URL:

```bash
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

---

## Verification Checklist

### Backend Verification

- [ ] `dotnet restore XenonClinic.sln` - No errors
- [ ] `dotnet build XenonClinic.sln` - No errors
- [ ] `dotnet test XenonClinic.sln` - All tests pass
- [ ] Database container healthy: `docker compose ps`
- [ ] API responds: `curl http://localhost:5000/health`
- [ ] Swagger loads: http://localhost:5000/swagger

### Frontend Verification

- [ ] `npm install` (root) - No errors
- [ ] `npm run build -w Shared.UI` - No errors
- [ ] `npm install -w XenonClinic.React` - No errors
- [ ] `npm run build -w XenonClinic.React` - No errors
- [ ] Admin app loads: http://localhost:5173 (dev) or http://localhost:3000 (docker)
- [ ] Login works with default credentials

### E2E Verification

- [ ] Playwright installed: `npx playwright install`
- [ ] E2E tests pass: `cd tests/e2e && npm test`

---

## Next Steps After Setup

1. Run full test suite to establish baseline
2. Review `TECH_DETECTION.md` for identified tech stack
3. Review `SYSTEM_MAP.md` for module documentation
4. Review `JOURNEYS.md` for test scenarios
5. Execute production readiness audit checklist
