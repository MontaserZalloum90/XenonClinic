# XenonClinic - System Map

**Generated:** 2025-12-13
**Phase:** 1 - Auto-Discover Modules, Screens, APIs, Data Stores, Auth

---

## Executive Summary

XenonClinic is a comprehensive **multi-tenant healthcare management system** with:

- **Frontend:** 100+ screens across 18+ modules
- **Backend:** 35+ controllers with 400+ API endpoints
- **Data:** 252 entities across clinical, financial, and HR domains
- **Auth:** JWT-based RBAC with multi-tenancy isolation

---

## 1. Module Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     PLATFORM LAYER                               │
│  ┌─────────────┬─────────────┬─────────────┬─────────────────┐  │
│  │  Tenant Mgmt │   Auth      │  Licensing  │   Usage/Billing │  │
│  └─────────────┴─────────────┴─────────────┴─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                     CLINIC ERP LAYER                             │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    CORE MODULES                           │   │
│  │  ┌──────────┬──────────┬──────────┬──────────┬────────┐  │   │
│  │  │ Patients │ Appoint. │ Clinical │   Lab    │Pharmacy│  │   │
│  │  └──────────┴──────────┴──────────┴──────────┴────────┘  │   │
│  │  ┌──────────┬──────────┬──────────┬──────────┬────────┐  │   │
│  │  │Radiology │Financial │Inventory │    HR    │Marketing│  │   │
│  │  └──────────┴──────────┴──────────┴──────────┴────────┘  │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                  SPECIALTY MODULES                        │   │
│  │  Dental │ Cardio │ Ophthal │ Ortho │ Derma │ Oncology    │   │
│  │  Neuro  │ Peds   │ OB/GYN  │ Physio│  ENT  │ Fertility   │   │
│  │  Dialysis│ Audio │ Psych   │ Gastro│ Podiat│ Chiro       │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                  SUPPORTING MODULES                       │   │
│  │  Workflow Engine │ Analytics │ Security │ Patient Portal │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Frontend Routes & Screens

### 2.1 Core Modules

| Module        | Route    | Component       | API Endpoint         | Roles Required    |
| ------------- | -------- | --------------- | -------------------- | ----------------- |
| **Dashboard** | `/`      | `Dashboard.tsx` | Multiple stats APIs  | Any authenticated |
| **Login**     | `/login` | `Login.tsx`     | `/api/AuthApi/login` | Public            |

#### Patient Management

| Route       | Component          | API                  | Description         |
| ----------- | ------------------ | -------------------- | ------------------- |
| `/patients` | `PatientsList.tsx` | `/api/PatientsApi/*` | Patient list/search |

#### Appointments

| Route                     | Component                   | API                      | Description        |
| ------------------------- | --------------------------- | ------------------------ | ------------------ |
| `/appointments`           | `AppointmentsList.tsx`      | `/api/AppointmentsApi/*` | All appointments   |
| `/appointments/recurring` | `RecurringAppointments.tsx` | TBD                      | Recurring patterns |
| `/appointments/checkin`   | `AppointmentCheckin.tsx`    | TBD                      | Check-in flow      |
| `/appointments/waitlist`  | `WaitlistManagement.tsx`    | TBD                      | Waitlist mgmt      |

#### Clinical Visits

| Route                            | Component                | API                        | Description  |
| -------------------------------- | ------------------------ | -------------------------- | ------------ |
| `/clinical-visits`               | `ClinicalVisitsList.tsx` | `/api/ClinicalVisitsApi/*` | Visit list   |
| `/clinical-visits/vitals`        | `VitalsRecording.tsx`    | TBD                        | Vitals entry |
| `/clinical-visits/diagnosis`     | `DiagnosisEntry.tsx`     | TBD                        | Diagnosis    |
| `/clinical-visits/prescriptions` | `PrescriptionEntry.tsx`  | TBD                        | Rx           |
| `/clinical-visits/referrals`     | `ReferralManagement.tsx` | TBD                        | Referrals    |

#### Laboratory

| Route         | Component            | API                    | Description |
| ------------- | -------------------- | ---------------------- | ----------- |
| `/laboratory` | `LaboratoryList.tsx` | `/api/LaboratoryApi/*` | Lab orders  |

#### HR & Payroll

| Route                   | Component              | API                    | Description   |
| ----------------------- | ---------------------- | ---------------------- | ------------- |
| `/hr`                   | `HRList.tsx`           | `/api/HRApi/*`         | HR dashboard  |
| `/hr/employees`         | `EmployeesList.tsx`    | `/api/HRApi/employees` | Employees     |
| `/hr/payroll`           | `PayrollList.tsx`      | `/api/HRApi/payroll`   | Payroll       |
| `/hr/salary-structures` | `SalaryStructures.tsx` | TBD                    | Salary config |

#### Financial

| Route                    | Component                | API                   | Description  |
| ------------------------ | ------------------------ | --------------------- | ------------ |
| `/financial`             | `FinancialList.tsx`      | `/api/FinancialApi/*` | Finance dash |
| `/financial/receivables` | `AccountsReceivable.tsx` | TBD                   | A/R          |
| `/financial/expenses`    | `ExpenseManagement.tsx`  | TBD                   | Expenses     |
| `/financial/claims`      | `InsuranceClaims.tsx`    | TBD                   | Claims       |

#### Inventory & Pharmacy

| Route        | Component           | API                   | Description   |
| ------------ | ------------------- | --------------------- | ------------- |
| `/inventory` | `InventoryList.tsx` | `/api/InventoryApi/*` | Stock mgmt    |
| `/pharmacy`  | `PharmacyList.tsx`  | `/api/PharmacyApi/*`  | Rx dispensing |

#### Radiology

| Route        | Component           | API                   | Description    |
| ------------ | ------------------- | --------------------- | -------------- |
| `/radiology` | `RadiologyList.tsx` | `/api/RadiologyApi/*` | Imaging orders |

#### Marketing

| Route        | Component           | API                   | Description     |
| ------------ | ------------------- | --------------------- | --------------- |
| `/marketing` | `MarketingList.tsx` | `/api/MarketingApi/*` | Campaigns/Leads |

#### Analytics

| Route                | Component                | API                   | Description    |
| -------------------- | ------------------------ | --------------------- | -------------- |
| `/analytics`         | `AnalyticsDashboard.tsx` | `/api/AnalyticsApi/*` | KPIs           |
| `/analytics/reports` | `ReportsList.tsx`        | TBD                   | Custom reports |

#### Workflow

| Route                   | Component                 | API                  | Description   |
| ----------------------- | ------------------------- | -------------------- | ------------- |
| `/workflow`             | `WorkflowDefinitions.tsx` | `/api/v1/workflow/*` | Definitions   |
| `/workflow/definitions` | `WorkflowDefinitions.tsx` | Same                 | Definitions   |
| `/workflow/instances`   | `WorkflowInstances.tsx`   | Same                 | Instances     |
| `/workflow/editor/:id`  | `WorkflowEditor.tsx`      | Same                 | Visual editor |

#### Admin

| Route                 | Component                   | API     | Description |
| --------------------- | --------------------------- | ------- | ----------- |
| `/admin`              | `AdminDashboard.tsx`        | Various | Admin panel |
| `/admin/translations` | `TranslationManagement.tsx` | TBD     | i18n        |

---

### 2.2 Specialty Modules

| Module            | Base Route       | Sub-routes                                                                         | API Status        |
| ----------------- | ---------------- | ---------------------------------------------------------------------------------- | ----------------- |
| **Dental**        | `/dental`        | `/charts`, `/treatments`, `/periodontal`                                           | Mock              |
| **Cardiology**    | `/cardiology`    | `/ecg`, `/echo`, `/stress-tests`, `/cath-lab`, `/risk-calculator`                  | Mock              |
| **Ophthalmology** | `/ophthalmology` | `/visual-acuity`, `/refraction`, `/iop`, `/slit-lamp`, `/fundus`, `/prescriptions` | Mock              |
| **Orthopedics**   | `/orthopedics`   | `/exams`, `/fractures`, `/surgeries`                                               | Mock              |
| **Dermatology**   | `/dermatology`   | `/exams`, `/photos`, `/mole-mapping`, `/biopsies`                                  | Mock              |
| **Oncology**      | `/oncology`      | `/diagnoses`, `/chemotherapy`, `/treatment-plans`                                  | Mock              |
| **Neurology**     | `/neurology`     | `/exams`, `/eeg`, `/emg`                                                           | Mock              |
| **Pediatrics**    | `/pediatrics`    | `/growth`, `/vaccinations`, `/milestones`                                          | Mock              |
| **OB/GYN**        | `/obgyn`         | `/pregnancies`, `/prenatal`, `/ultrasounds`                                        | Mock              |
| **Physiotherapy** | `/physiotherapy` | `/sessions`, `/exercises`                                                          | Mock              |
| **ENT**           | `/ent`           | `/audiometry`, `/hearing-aids`                                                     | Mock              |
| **Fertility**     | `/fertility`     | `/ivf-cycles`, `/embryos`                                                          | Mock              |
| **Dialysis**      | `/dialysis`      | `/sessions`, `/vascular-access`                                                    | Mock              |
| **Audiology**     | `/audiology`     | Main list                                                                          | Backend Connected |

---

### 2.3 Patient Portal

| Route                  | Component                | API                        | Description |
| ---------------------- | ------------------------ | -------------------------- | ----------- |
| `/portal/register`     | `PortalRegistration.tsx` | `/api/portal/register`     | Public      |
| `/portal/login`        | `PortalLogin.tsx`        | `/api/portal/login`        | Public      |
| `/portal`              | `PortalDashboard.tsx`    | `/api/portal/dashboard`    | Patient     |
| `/portal/profile`      | `PortalProfile.tsx`      | `/api/portal/profile`      | Patient     |
| `/portal/documents`    | `PortalDocuments.tsx`    | `/api/portal/documents`    | Patient     |
| `/portal/appointments` | `PortalAppointments.tsx` | `/api/portal/appointments` | Patient     |

---

## 3. Backend API Structure

### 3.1 Platform API (Xenon.Platform.Api)

**Base:** `/api/`

| Controller         | Route Prefix                     | Endpoints | Auth        |
| ------------------ | -------------------------------- | --------- | ----------- |
| **Public Pricing** | `/api/public/pricing`            | 3         | None        |
| **Public Tenants** | `/api/public/tenants`            | 4         | None/Tenant |
| **Demo Request**   | `/api/public/demo-request`       | 1         | None        |
| **Platform Auth**  | `/api/platform-admin/auth`       | 2         | Admin       |
| **Tenant Admin**   | `/api/platform-admin/tenants`    | 6         | Admin       |
| **Monitoring**     | `/api/platform-admin/monitoring` | 3         | Admin       |
| **Reports**        | `/api/platform-admin/reports`    | 3         | Admin       |
| **Dashboard**      | `/api/platform-admin/dashboard`  | 1         | Admin       |
| **License**        | `/api/tenant/license`            | 2         | Tenant      |
| **Usage**          | `/api/tenant/usage`              | 2         | Tenant      |
| **Security**       | `/api/tenant/security`           | 9         | Tenant      |

### 3.2 Clinic ERP API (XenonClinic.Api)

**Base:** `/api/[controller]`

| Controller                  | Endpoint Count | Key Operations                         |
| --------------------------- | -------------- | -------------------------------------- |
| **Patient**                 | 10+            | CRUD, search, documents                |
| **Appointments**            | 15+            | CRUD, confirm, cancel, checkIn         |
| **ClinicalVisits**          | 30+            | CRUD per specialty                     |
| **Laboratory**              | 30+            | Tests, orders, results, external labs  |
| **HR**                      | 40+            | Employees, dept, attendance, leave     |
| **Financial**               | 15+            | Invoices, payments, expenses           |
| **Inventory**               | 10+            | CRUD, stock adjustment                 |
| **Radiology**               | 10+            | Studies, modalities, reports           |
| **Sales**                   | 20+            | Orders, payments, quotations           |
| **PatientPortal**           | 40+            | Auth, appointments, records, messaging |
| **ClinicalDecisionSupport** | 30+            | Drug checks, alerts, guidelines        |
| **Backup**                  | 10+            | Full/incremental, restore              |
| **Audit**                   | 5+             | Log retrieval                          |
| **Security**                | 10+            | User/role management                   |

### 3.3 Workflow Engine API

**Base:** `/api/v1/workflow/` and `/api/workflow/`

| Controller      | Route                        | Endpoints | Purpose                |
| --------------- | ---------------------------- | --------- | ---------------------- |
| **Designer**    | `/api/v1/workflow/designer`  | 16        | Visual workflow design |
| **Execution**   | `/api/v1/workflow/execution` | 12        | Instance management    |
| **Definitions** | `/api/workflow/definitions`  | 18        | BPMN definitions       |
| **Instances**   | `/api/workflow/instances`    | 12        | Process instances      |
| **Tasks**       | `/api/workflow/tasks`        | 18        | Human task inbox       |
| **Rules**       | `/api/workflow/rules`        | 13        | Business rules         |
| **BPMN**        | `/api/workflow/bpmn`         | 10        | BPMN import/export     |
| **Webhooks**    | `/api/workflow/webhooks`     | 12        | Event notifications    |
| **Documents**   | `/api/workflow/documents`    | 13        | Document generation    |
| **Monitoring**  | `/api/workflow/monitoring`   | 8         | Execution analytics    |
| **Admin**       | `/api/workflow/admin`        | 20+       | Cluster/cache/roles    |

---

## 4. Data Model Summary

### 4.1 Entity Categories

| Category          | Entity Count | Key Entities                            |
| ----------------- | ------------ | --------------------------------------- |
| **Core**          | 45+          | Patient, Appointment, Employee, Invoice |
| **Clinical**      | 20+          | LabOrder, LabResult, DicomStudy         |
| **Specialty**     | 150+         | Organized by specialty subfolder        |
| **Lookups**       | 27           | Status/type enumerations                |
| **Multi-tenancy** | 10+          | Tenant, Company, Branch                 |
| **Workflow**      | 7            | ProcessDefinition, ProcessInstance      |
| **Platform**      | 15+          | Plan, Subscription, AuditLog            |

### 4.2 Key Entities & Relationships

```
Tenant (Platform)
  └── Company
       └── Branch
            ├── Patient
            │    ├── Appointment
            │    ├── ClinicalVisit (specialty-specific)
            │    ├── LabOrder → LabResult
            │    └── Invoice → Payment
            ├── Employee
            │    ├── Attendance
            │    └── LeaveRequest
            └── InventoryItem
                 └── InventoryTransaction
```

### 4.3 Multi-Tenancy Model

| Level            | Entity  | Isolation          |
| ---------------- | ------- | ------------------ |
| **Platform**     | Tenant  | Database isolation |
| **Organization** | Company | Data filtering     |
| **Location**     | Branch  | Row-level security |

---

## 5. Authorization Matrix

### 5.1 Defined Roles (Frontend)

```typescript
export const Roles = {
  ADMIN: "admin",
  DOCTOR: "doctor",
  NURSE: "nurse",
  RECEPTIONIST: "receptionist",
  LAB_TECHNICIAN: "lab_technician",
  PHARMACIST: "pharmacist",
  RADIOLOGIST: "radiologist",
  HR_MANAGER: "hr_manager",
  ACCOUNTANT: "accountant",
  MARKETING_MANAGER: "marketing_manager",
};
```

### 5.2 Route Protection Summary

| Module            | Required Roles                 | UI Guard               | API Guard      |
| ----------------- | ------------------------------ | ---------------------- | -------------- |
| Dashboard         | Any authenticated              | ProtectedRoute         | Token          |
| Patients          | Any authenticated              | ProtectedRoute         | Token          |
| Appointments      | Any authenticated              | ProtectedRoute         | Token          |
| Laboratory        | Admin, Doctor, Nurse, Lab Tech | ProtectedRoute + roles | Token + Policy |
| HR                | Admin, HR Manager              | ProtectedRoute + roles | Token + Policy |
| Financial         | Admin, Accountant              | ProtectedRoute + roles | Token + Policy |
| Inventory         | Admin, Pharmacist, Nurse       | ProtectedRoute + roles | Token + Policy |
| Pharmacy          | Admin, Doctor, Pharmacist      | ProtectedRoute + roles | Token + Policy |
| Radiology         | Admin, Doctor, Radiologist     | ProtectedRoute + roles | Token + Policy |
| Marketing         | Admin, Marketing Manager       | ProtectedRoute + roles | Token + Policy |
| Analytics         | Admin                          | ProtectedRoute + roles | Token + Policy |
| Workflow          | Admin                          | ProtectedRoute + roles | Token + Policy |
| Admin             | Admin                          | ProtectedRoute + roles | Token + Policy |
| Specialty Modules | Any authenticated              | ProtectedRoute         | Token          |

### 5.3 Backend Authorization Mechanisms

| Mechanism                         | Location          | Purpose             |
| --------------------------------- | ----------------- | ------------------- |
| `[Authorize]`                     | Controller/Action | Basic auth required |
| `[Authorize(Policy=...)]`         | Controller/Action | Policy-based        |
| `[BranchAuthorization]`           | Controller/Action | Branch-level access |
| `ITenantContextAccessor`          | Service layer     | Tenant isolation    |
| `IRbacService.HasPermissionAsync` | Service layer     | Permission check    |

---

## 6. Real-Time Communication

### SignalR Hub: NotificationHub

**Connection:** `/hubs/notifications`

| Event                  | Direction     | Payload             |
| ---------------------- | ------------- | ------------------- |
| `AppointmentCreated`   | Server→Client | Appointment DTO     |
| `AppointmentUpdated`   | Server→Client | Appointment DTO     |
| `AppointmentCancelled` | Server→Client | Appointment ID      |
| `PatientCheckedIn`     | Server→Client | Patient/Appointment |
| `LabResultReady`       | Server→Client | Lab result DTO      |
| `MessageReceived`      | Server→Client | Message DTO         |
| `TaskAssigned`         | Server→Client | Task DTO            |
| `AlertCreated`         | Server→Client | Alert DTO           |

---

## 7. Integration Points

### 7.1 External Integrations (Configured)

| Service           | Interface            | Status            |
| ----------------- | -------------------- | ----------------- |
| **SMS**           | Twilio               | Configured (.env) |
| **Email**         | SMTP                 | Configured (.env) |
| **Storage**       | Azure Blob / AWS S3  | Configured (.env) |
| **DICOM/PACS**    | IDicomService        | Interface defined |
| **Drug DB**       | IDrugDatabaseService | Interface defined |
| **FHIR**          | IFhirService         | Interface defined |
| **Calendar Sync** | ICalendarSyncService | Interface defined |

### 7.2 Internal Service Communication

| From            | To       | Method         |
| --------------- | -------- | -------------- |
| Frontend        | API      | REST (Axios)   |
| API             | Database | EF Core        |
| API             | Cache    | Redis          |
| API             | Frontend | SignalR        |
| Workflow Engine | API      | Internal calls |

---

## 8. Module Status Summary

| Module                | UI Complete | API Connected | Tests   | Status     |
| --------------------- | ----------- | ------------- | ------- | ---------- |
| **Core Modules**      |
| Patient Management    | ✅          | ✅            | Partial | Production |
| Appointments          | ✅          | ✅            | Partial | Production |
| Clinical Visits       | ✅          | ✅            | Minimal | Production |
| Laboratory            | ✅          | ✅            | Minimal | Production |
| HR                    | ✅          | ✅            | Minimal | Production |
| Financial             | ✅          | ✅            | Partial | Production |
| Inventory             | ✅          | ✅            | Minimal | Production |
| Pharmacy              | ✅          | ✅            | Minimal | Production |
| Radiology             | ✅          | ✅            | Minimal | Production |
| Marketing             | ✅          | ✅            | Minimal | Production |
| Analytics             | ✅          | Partial       | Minimal | Beta       |
| Workflow              | ✅          | ✅            | Partial | Beta       |
| **Portal**            |
| Patient Portal        | ✅          | ✅            | Minimal | Production |
| **Specialty Modules** |
| Audiology             | ✅          | ✅            | Minimal | Production |
| All Others (13)       | ✅          | ❌ Mock       | None    | Prototype  |

---

## 9. Next Steps

1. **RBAC_MATRIX.md** - Detailed role-permission mapping
2. **JOURNEYS.md** - User journey definitions with test cases
3. **TEST_STRATEGY.md** - Testing approach and coverage plan
4. Implement P0 tests for core modules
