# XenonClinic

Healthcare Management System - A comprehensive clinic management platform built with .NET 8 and React.

## Architecture

```
XenonClinic/
├── XenonClinic.Core/           # Domain entities, interfaces, abstractions
├── XenonClinic.Infrastructure/ # EF Core, services, external integrations
├── Xenon.Platform/             # Platform API for tenant management
├── XenonClinic.React/          # Admin dashboard (React + TypeScript)
├── Xenon.PublicWebsite/        # Public website (React + TypeScript)
├── Shared.UI/                  # Shared component library (@xenon/ui)
└── tests/
    └── e2e/                    # Playwright E2E tests
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/) or Docker
- [Redis](https://redis.io/) (optional, for caching)

## Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/your-org/XenonClinic.git
cd XenonClinic

# Copy environment file
cp .env.example .env

# Start all services
docker-compose up -d

# Access the applications:
# - Admin App: http://localhost:3000
# - Public Website: http://localhost:3001
# - API: http://localhost:5000
# - API Docs: http://localhost:5000/swagger
```

### Manual Setup

#### 1. Backend Setup

```bash
# Restore dependencies
dotnet restore XenonClinic.sln

# Update database
dotnet ef database update --project XenonClinic.Infrastructure --startup-project Xenon.Platform/src/Xenon.Platform.Api

# Run the API
dotnet run --project Xenon.Platform/src/Xenon.Platform.Api
```

#### 2. Frontend Setup

```bash
# Install root dependencies (husky, lint-staged)
npm install

# Build shared library first
cd Shared.UI
npm install
npm run build

# Run Admin App
cd ../XenonClinic.React
npm install
npm run dev

# In another terminal - Run Public Website
cd ../Xenon.PublicWebsite
npm install
npm run dev
```

## Development

### Running Tests

```bash
# Backend tests
dotnet test XenonClinic.sln

# Frontend unit tests
npm test --workspaces

# E2E tests
cd tests/e2e
npm install
npx playwright install
npm test
```

### Code Quality

Pre-commit hooks are configured with husky:
- TypeScript/JavaScript: ESLint + Prettier
- C#: dotnet format

```bash
# Run linting manually
npm run lint --workspaces

# Format C# code
dotnet format XenonClinic.sln
```

### Storybook (Component Documentation)

```bash
cd Shared.UI
npm run storybook
# Opens at http://localhost:6006
```

## Project Structure

### Backend Services

| Service | Description |
|---------|-------------|
| `ICurrentUserContext` | Centralized user context access |
| `ISequenceGenerator` | Sequential number generation (invoices, orders) |
| `IRepository<T>` | Generic repository pattern |
| `ICacheService` | Caching abstraction |

### Shared UI Components

| Component | Description |
|-----------|-------------|
| `Toast` | Notification toasts |
| `Modal` | Modal dialogs |
| `DataTable` | Sortable, selectable data tables |
| `Pagination` | Page navigation |
| `FormField` | Form inputs with validation |
| `Badge` | Status badges |
| `LoadingSkeleton` | Loading placeholders |

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DB_USER` | Database username | xenon |
| `DB_PASSWORD` | Database password | - |
| `DB_NAME` | Database name | xenonclinic |
| `JWT_SECRET` | JWT signing key | - |
| `VITE_API_URL` | API URL for frontend | http://localhost:5000 |

### API Configuration

The API supports:
- **Versioning**: URL (`/api/v1/`), Header (`X-Api-Version`), Query (`?api-version=1.0`)
- **Rate Limiting**: 1000 requests/minute per user
- **Health Checks**: `/health` endpoint

## Deployment

### Docker

```bash
# Build images
docker-compose build

# Push to registry
docker-compose push

# Deploy
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### CI/CD

GitHub Actions workflows:
- `ci.yml` - Build, test, lint on PR/push
- `deploy.yml` - Deploy to staging/production
- `pr.yml` - PR checks (size, breaking changes)

## API Documentation

Swagger UI available at `/swagger` when running in Development mode.

## Default Credentials

| User | Email | Password |
|------|-------|----------|
| Admin | admin@xenon.ae | Admin@123! |

## License

Proprietary - All rights reserved.
