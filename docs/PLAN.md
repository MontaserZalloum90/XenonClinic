# XenonClinic Enterprise Documentation Portal - Implementation Plan

## Executive Summary

This plan outlines the implementation of an enterprise-grade documentation portal for XenonClinic, integrated into the existing public website. The documentation will cover all 17+ product modules, 16 personas, 98 user journeys, and 765 steps - comparable in quality to ServiceNow, Salesforce, and Microsoft documentation standards.

---

## Phase 1: Documentation Portal Infrastructure

### 1.1 Routes & Navigation Setup
- [x] Add `/docs` as documentation entry point
- [x] Create documentation layout with sidebar navigation
- [x] Implement route structure:
  - `/docs` - Documentation home
  - `/docs/getting-started` - Quick start guide
  - `/docs/personas` - User personas
  - `/docs/personas/:personaId` - Individual persona pages
  - `/docs/modules` - All modules
  - `/docs/modules/:moduleId` - Individual module pages
  - `/docs/journeys` - User journeys
  - `/docs/journeys/:journeyId` - Individual journey pages
  - `/docs/admin-configuration` - Admin settings
  - `/docs/security-rbac` - Security & RBAC
  - `/docs/api-reference` - API documentation
  - `/docs/faq-troubleshooting` - FAQ
  - `/docs/release-notes` - Release notes
  - `/docs/glossary` - Terminology

### 1.2 Documentation UI Components
- [x] DocsSidebar - Tree navigation with collapsible sections
- [x] DocsSearch - Full-text search across all docs
- [x] TableOfContents - Auto-generated TOC per page
- [x] Breadcrumbs - Navigation context
- [x] DocsMeta - Last updated, version badge
- [x] CodeBlock - Copy-to-clipboard for code/config
- [x] Screenshot - Responsive image with caption

### 1.3 Inventory Generation
- [x] `docs/inventory/modules.json` - 17+ modules
- [x] `docs/inventory/personas.json` - 16 personas
- [x] `docs/inventory/journeys.json` - 98 journeys
- [x] `docs/inventory/permissions.json` - 52+ permissions

---

## Phase 2: Content Creation

### 2.1 Persona Documentation (16 pages)
| Persona | Priority | Status |
|---------|----------|--------|
| System Admin | High | [x] |
| Tenant Admin | High | [x] |
| Doctor/Physician | High | [x] |
| Nurse | High | [x] |
| Receptionist | High | [x] |
| Lab Technician | Medium | [x] |
| Pharmacist | Medium | [x] |
| Radiologist | Medium | [x] |
| Audiologist | Medium | [x] |
| HR Manager | Medium | [x] |
| Accountant | Medium | [x] |
| Marketing Manager | Medium | [x] |
| Warehouse Staff | Low | [x] |
| Billing Staff | Low | [x] |
| Patient | Low | [x] |
| Security Admin | Low | [x] |

### 2.2 Module Documentation (17+ pages)
| Module | Priority | Status |
|--------|----------|--------|
| Patient Management | High | [x] |
| Appointments | High | [x] |
| Clinical Visits | High | [x] |
| Laboratory | High | [x] |
| Pharmacy | High | [x] |
| Radiology | Medium | [x] |
| Financial | Medium | [x] |
| Inventory | Medium | [x] |
| HR Management | Medium | [x] |
| Marketing | Medium | [x] |
| Workflow Engine | Low | [x] |
| Patient Portal | Low | [x] |
| Multi-Tenancy | Low | [x] |
| Analytics | Low | [x] |
| Security & Audit | Low | [x] |
| Specialty Modules | Low | [x] |

### 2.3 Journey Documentation (Top 10 Priority)
| Journey | Module | Priority | Status |
|---------|--------|----------|--------|
| Patient Registration | Patient Mgmt | High | [x] |
| Appointment Booking | Appointments | High | [x] |
| Patient Check-in | Appointments | High | [x] |
| Clinical Visit (Full) | Clinical Visits | High | [x] |
| Lab Order to Results | Laboratory | High | [x] |
| Prescription Dispensing | Pharmacy | Medium | [x] |
| Invoice & Payment | Financial | Medium | [x] |
| Employee Onboarding | HR | Medium | [x] |
| Insurance Claim | Financial | Medium | [x] |
| Imaging Study | Radiology | Medium | [x] |

---

## Phase 3: Screenshot Automation

### 3.1 Screenshot Pipeline Setup
- [x] Playwright-based screenshot automation
- [x] Stable viewport sizes (1280x720 desktop, 768x1024 tablet, 375x667 mobile)
- [x] Demo data seeding for consistency
- [x] PII/PHI masking utilities

### 3.2 Screenshot Coverage
- [ ] Module dashboards
- [ ] Key feature screens
- [ ] Journey step screenshots
- [ ] Mobile responsive views

### 3.3 Naming Convention
```
module__feature__journeyStep__state.png

Examples:
patients__registration__step1__form-empty.png
appointments__booking__step3__calendar-view.png
laboratory__results__final__approved.png
```

---

## Phase 4: Quality Assurance

### 4.1 Content Verification
- [ ] All modules have complete documentation
- [ ] All journeys have step-by-step instructions
- [ ] All personas have access scopes documented
- [ ] RBAC is documented for each feature

### 4.2 Technical Verification
- [ ] No broken internal links
- [ ] All screenshots present and loading
- [ ] Search indexing working
- [ ] Mobile responsive

### 4.3 Consistency Review
- [ ] Terminology consistency (glossary alignment)
- [ ] Template adherence
- [ ] Screenshot quality and naming

---

## Discovery Summary

### Modules Identified: 17+
1. Patient Management
2. Appointments
3. Clinical Visits
4. Laboratory
5. Radiology
6. Pharmacy
7. Financial
8. Inventory
9. HR Management
10. Payroll
11. Marketing
12. Workflow Engine
13. Patient Portal
14. Multi-Tenancy
15. Analytics & Reporting
16. Security & Audit
17. Specialty Modules (10 sub-modules)

### Personas Identified: 16
1. System Admin
2. Tenant Admin
3. Company Admin
4. Branch Admin
5. Doctor/Physician
6. Nurse
7. Receptionist
8. Lab Technician
9. Pharmacist
10. Radiologist
11. Audiologist
12. HR Manager
13. Accountant
14. Marketing Manager
15. Patient
16. Security Admin

### Journeys Documented: 98
Covering 765 steps across all modules

### API Endpoints: 18 Controllers
With 200+ endpoints documented

### Permissions: 52+
Across 9 permission categories

---

## File Structure

```
docs/
├── PLAN.md                    # This file
├── IA.md                      # Information Architecture
├── inventory/
│   ├── modules.json
│   ├── personas.json
│   ├── journeys.json
│   └── permissions.json
├── templates/
│   ├── module-template.md
│   ├── journey-template.md
│   └── persona-template.md
└── screenshots/
    └── (auto-generated)

Xenon.PublicWebsite/src/
├── pages/docs/
│   ├── DocsHome.tsx
│   ├── GettingStarted.tsx
│   ├── PersonaPage.tsx
│   ├── ModulePage.tsx
│   ├── JourneyPage.tsx
│   └── ...
├── components/docs/
│   ├── DocsLayout.tsx
│   ├── DocsSidebar.tsx
│   ├── DocsSearch.tsx
│   ├── TableOfContents.tsx
│   ├── Breadcrumbs.tsx
│   └── ...
└── lib/docs/
    ├── docsData.ts
    └── searchIndex.ts
```

---

## Progress Tracking

| Phase | Description | Status | Progress |
|-------|-------------|--------|----------|
| 1 | Infrastructure | Complete | 100% |
| 2 | Content | In Progress | 80% |
| 3 | Screenshots | Pending | 0% |
| 4 | Quality Gate | Pending | 0% |

---

*Last Updated: December 2024*
*Plan Version: 1.0*
