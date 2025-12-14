# XenonClinic Documentation - Information Architecture

## Overview

This document defines the information architecture for the XenonClinic Enterprise Documentation Portal. The structure is designed to support multiple user personas navigating documentation by module, role, or task.

---

## Primary Navigation Structure

```
/docs
├── /docs                          # Documentation Home
├── /docs/getting-started          # Quick Start Guide
│   ├── Overview
│   ├── First Login
│   ├── Basic Navigation
│   └── Key Concepts
│
├── /docs/personas                  # User Personas
│   ├── /docs/personas              # Personas Index
│   └── /docs/personas/:personaId   # Individual Persona Pages
│       ├── system-admin
│       ├── tenant-admin
│       ├── doctor
│       ├── nurse
│       ├── receptionist
│       ├── lab-technician
│       ├── pharmacist
│       ├── radiologist
│       ├── audiologist
│       ├── hr-manager
│       ├── accountant
│       ├── marketing-manager
│       ├── billing-staff
│       ├── warehouse-staff
│       ├── patient
│       └── security-admin
│
├── /docs/modules                   # Product Modules
│   ├── /docs/modules               # Modules Index
│   └── /docs/modules/:moduleId     # Individual Module Pages
│       ├── patient-management
│       ├── appointments
│       ├── clinical-visits
│       ├── laboratory
│       ├── radiology
│       ├── pharmacy
│       ├── financial
│       ├── inventory
│       ├── hr-management
│       ├── payroll
│       ├── marketing
│       ├── workflow-engine
│       ├── patient-portal
│       ├── multi-tenancy
│       ├── analytics
│       ├── security-audit
│       └── specialty-modules
│
├── /docs/journeys                  # User Journeys
│   ├── /docs/journeys              # Journeys Index
│   └── /docs/journeys/:journeyId   # Individual Journey Pages
│       ├── patient-registration
│       ├── appointment-booking
│       ├── patient-checkin
│       ├── clinical-visit
│       ├── lab-order-to-results
│       ├── prescription-dispensing
│       ├── invoice-payment
│       ├── employee-onboarding
│       ├── insurance-claim-submission
│       └── imaging-study
│
├── /docs/admin-configuration       # Admin Configuration
│   ├── System Settings
│   ├── Tenant Configuration
│   ├── Branch Setup
│   ├── User Management
│   └── Feature Configuration
│
├── /docs/security-rbac             # Security & RBAC
│   ├── Role Overview
│   ├── Permission Matrix
│   ├── Password Policies
│   ├── Audit Logging
│   └── Compliance
│
├── /docs/api-reference             # API Reference
│   ├── Authentication
│   ├── Endpoints by Module
│   ├── Request/Response Format
│   └── Error Codes
│
├── /docs/faq-troubleshooting       # FAQ & Troubleshooting
│   ├── Common Issues
│   ├── Error Messages
│   └── Support Contact
│
├── /docs/release-notes             # Release Notes
│   ├── Version History
│   └── Upgrade Guide
│
└── /docs/glossary                  # Glossary
    └── Terminology A-Z
```

---

## Page Components

### Every Documentation Page Contains:

1. **Header**
   - Page title
   - Last updated date
   - Version badge
   - Breadcrumb navigation

2. **Sidebar (Left)**
   - Tree navigation
   - Collapsible sections
   - Current location highlight
   - Search integration

3. **Main Content**
   - Structured content based on template
   - Screenshots with captions
   - Code/configuration blocks with copy button
   - Callout boxes (info, warning, tip)

4. **Table of Contents (Right)**
   - Auto-generated from headings
   - Scroll spy highlighting
   - Quick navigation

5. **Footer**
   - Related pages
   - Feedback link
   - Print/export options

---

## Content Templates

### Module Page Template
```
# [Module Name]

## Overview
Brief description of the module

## Business Value
Why this module matters

## Who Uses This
- Persona 1
- Persona 2

## Features
### Feature 1
Description with screenshot

### Feature 2
Description with screenshot

## Required Roles/Permissions
Table of permissions

## Configuration
Setup instructions

## Related Journeys
Links to journey pages

## Common Issues
FAQ for this module

## API Reference
Link to API docs
```

### Journey Page Template
```
# [Journey Name]

## Goal
What this journey accomplishes

## Personas Involved
- Primary: Role
- Supporting: Roles

## Preconditions
- Requirement 1
- Requirement 2

## Step-by-Step Flow

### Step 1: [Action]
**Owner:** Role
**System Response:** What happens
**Validations:** Business rules
**RBAC:** Required permission
[Screenshot]

### Step 2: [Action]
...

## Edge Cases
- Scenario 1
- Scenario 2

## Success Criteria
- Outcome 1
- Outcome 2
```

### Persona Page Template
```
# [Persona Name]

## Description
Role overview

## Responsibilities
- Responsibility 1
- Responsibility 2

## Access Scope
- Data access level
- System areas

## Core Journeys
Links to journeys

## Entry Points
- /route1
- /route2

## Common Tasks
Quick reference

## Tips
Best practices
```

---

## Navigation Categories

### By User Type
- Clinical Staff (Doctor, Nurse)
- Front Office (Receptionist)
- Back Office (Billing, Accountant, HR)
- Technical (Admin, IT)
- Patient (Portal)

### By Module Category
- Core (Patients, Appointments)
- Clinical (Visits, Lab, Radiology, Pharmacy)
- Business (Financial, Inventory, HR, Marketing)
- Platform (Analytics, Workflow, Multi-Tenancy)
- Security (Audit, RBAC)

### By Task Type
- Daily Operations
- Configuration
- Reporting
- Troubleshooting

---

## Search Functionality

### Indexed Content
- Page titles
- Headings (H1-H3)
- Body text
- Tags/metadata
- Code snippets

### Search Features
- Full-text search
- Filters by category
- Recent searches
- Popular searches
- Suggested results

---

## URL Structure

### Pattern
```
/docs/[category]/[item-id]
```

### Examples
```
/docs/modules/patient-management
/docs/personas/doctor
/docs/journeys/appointment-booking
/docs/security-rbac
```

---

## Metadata Schema

### Page Metadata
```json
{
  "title": "Page Title",
  "description": "SEO description",
  "category": "modules|personas|journeys|admin",
  "tags": ["tag1", "tag2"],
  "lastUpdated": "2024-12-14",
  "version": "1.0",
  "author": "Documentation Team",
  "relatedPages": ["page-id-1", "page-id-2"]
}
```

---

## Versioning Strategy

- Major version for breaking changes
- Minor version for feature additions
- Documentation version tracks product version
- Version badge displayed on all pages
- Version selector for archived docs (future)

---

## Accessibility

- WCAG 2.1 AA compliance
- Semantic HTML structure
- Keyboard navigation
- Screen reader support
- High contrast mode support
- Skip navigation links

---

## Mobile Responsiveness

- Collapsible sidebar on mobile
- Touch-friendly navigation
- Readable content at all sizes
- Optimized images
- Swipe gestures for navigation

---

*Last Updated: December 2024*
*Document Version: 1.0*
