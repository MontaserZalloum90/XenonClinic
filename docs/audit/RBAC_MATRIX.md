# XenonClinic - RBAC Authorization Matrix

**Generated:** 2025-12-13
**Phase:** 1.4 - Authorization Discovery

---

## 1. Role Definitions

### 1.1 Frontend Roles (ProtectedRoute.tsx)

| Role Constant       | Display Value    | UI Guard Usage      |
| ------------------- | ---------------- | ------------------- |
| `ADMIN`             | Admin            | Full system access  |
| `DOCTOR`            | Doctor           | Clinical modules    |
| `NURSE`             | Nurse            | Clinical support    |
| `RECEPTIONIST`      | Receptionist     | Front desk          |
| `LAB_TECHNICIAN`    | LabTechnician    | Laboratory          |
| `PHARMACIST`        | Pharmacist       | Pharmacy, Inventory |
| `RADIOLOGIST`       | Radiologist      | Radiology           |
| `HR_MANAGER`        | HRManager        | HR module           |
| `ACCOUNTANT`        | Accountant       | Financial module    |
| `MARKETING_MANAGER` | MarketingManager | Marketing module    |

### 1.2 Backend Role Types (RbacDtos.cs)

| Role Type         | Code                | Description                  |
| ----------------- | ------------------- | ---------------------------- |
| System Admin      | `SYSTEM_ADMIN`      | Platform-level administrator |
| Clinic Admin      | `CLINIC_ADMIN`      | Tenant-level administrator   |
| Physician         | `PHYSICIAN`         | Licensed doctor              |
| Nurse             | `NURSE`             | Nursing staff                |
| Medical Assistant | `MEDICAL_ASSISTANT` | Clinical support             |
| Receptionist      | `RECEPTIONIST`      | Front desk                   |
| Billing Staff     | `BILLING_STAFF`     | Financial operations         |
| Lab Technician    | `LAB_TECHNICIAN`    | Laboratory                   |
| Pharmacist        | `PHARMACIST`        | Pharmacy                     |
| Patient           | `PATIENT`           | Patient portal               |
| Custom            | `CUSTOM`            | Tenant-defined               |

---

## 2. Permission Codes

### 2.1 Patient Management

| Permission Code  | Description             | PHI Related |
| ---------------- | ----------------------- | ----------- |
| `PATIENT_VIEW`   | View patient records    | Yes         |
| `PATIENT_CREATE` | Create new patients     | Yes         |
| `PATIENT_EDIT`   | Edit patient info       | Yes         |
| `PATIENT_DELETE` | Delete patients         | Yes         |
| `PATIENT_EXPORT` | Export patient data     | Yes         |
| `PATIENT_MERGE`  | Merge duplicate records | Yes         |

### 2.2 Medical Records

| Permission Code         | Description          | PHI Related |
| ----------------------- | -------------------- | ----------- |
| `MEDICAL_RECORD_VIEW`   | View medical records | Yes         |
| `MEDICAL_RECORD_CREATE` | Create records       | Yes         |
| `MEDICAL_RECORD_EDIT`   | Edit records         | Yes         |
| `MEDICAL_RECORD_DELETE` | Delete records       | Yes         |
| `MEDICAL_RECORD_PRINT`  | Print records        | Yes         |

### 2.3 Prescriptions

| Permission Code                  | Description                     | PHI Related |
| -------------------------------- | ------------------------------- | ----------- |
| `PRESCRIPTION_VIEW`              | View prescriptions              | Yes         |
| `PRESCRIPTION_CREATE`            | Create prescriptions            | Yes         |
| `PRESCRIPTION_EDIT`              | Edit prescriptions              | Yes         |
| `PRESCRIPTION_CANCEL`            | Cancel prescriptions            | Yes         |
| `CONTROLLED_SUBSTANCE_PRESCRIBE` | Prescribe controlled substances | Yes         |

### 2.4 Appointments

| Permission Code      | Description         | PHI Related |
| -------------------- | ------------------- | ----------- |
| `APPOINTMENT_VIEW`   | View appointments   | No          |
| `APPOINTMENT_CREATE` | Create appointments | No          |
| `APPOINTMENT_EDIT`   | Edit appointments   | No          |
| `APPOINTMENT_CANCEL` | Cancel appointments | No          |

### 2.5 Billing & Insurance

| Permission Code          | Description       | PHI Related |
| ------------------------ | ----------------- | ----------- |
| `BILLING_VIEW`           | View billing info | No          |
| `BILLING_CREATE`         | Create invoices   | No          |
| `BILLING_EDIT`           | Edit invoices     | No          |
| `BILLING_REFUND`         | Process refunds   | No          |
| `INSURANCE_CLAIM_SUBMIT` | Submit claims     | Yes         |
| `INSURANCE_CLAIM_VIEW`   | View claims       | Yes         |

### 2.6 Lab & Imaging

| Permission Code     | Description      | PHI Related |
| ------------------- | ---------------- | ----------- |
| `LAB_RESULT_VIEW`   | View lab results | Yes         |
| `LAB_RESULT_CREATE` | Enter results    | Yes         |
| `LAB_RESULT_EDIT`   | Edit results     | Yes         |
| `IMAGING_VIEW`      | View images      | Yes         |
| `IMAGING_UPLOAD`    | Upload images    | Yes         |

### 2.7 Reporting

| Permission Code         | Description       | PHI Related |
| ----------------------- | ----------------- | ----------- |
| `REPORT_VIEW`           | View reports      | No          |
| `REPORT_CREATE`         | Create reports    | No          |
| `REPORT_EXPORT`         | Export reports    | No          |
| `FINANCIAL_REPORT_VIEW` | Financial reports | No          |
| `CLINICAL_REPORT_VIEW`  | Clinical reports  | Yes         |

### 2.8 Administration

| Permission Code   | Description       | PHI Related |
| ----------------- | ----------------- | ----------- |
| `USER_MANAGE`     | Manage users      | No          |
| `ROLE_MANAGE`     | Manage roles      | No          |
| `SETTINGS_MANAGE` | System settings   | No          |
| `AUDIT_LOG_VIEW`  | View audit logs   | No          |
| `SYSTEM_ADMIN`    | Full admin access | No          |

### 2.9 Emergency Access

| Permission Code    | Description        | PHI Related |
| ------------------ | ------------------ | ----------- |
| `EMERGENCY_ACCESS` | Emergency override | Yes         |
| `BREAK_THE_GLASS`  | Break glass access | Yes         |

---

## 3. Role-Permission Matrix (Recommended)

### 3.1 Core Permissions by Role

| Permission             | Admin | Doctor | Nurse | Receptionist | Lab Tech | Pharmacist | Radiologist | Accountant | HR Mgr | Mktg Mgr |
| ---------------------- | :---: | :----: | :---: | :----------: | :------: | :--------: | :---------: | :--------: | :----: | :------: |
| **Patient Management** |
| PATIENT_VIEW           |  ✅   |   ✅   |  ✅   |      ✅      |    ⚠️    |     ⚠️     |     ⚠️      |     ❌     |   ❌   |    ❌    |
| PATIENT_CREATE         |  ✅   |   ✅   |  ✅   |      ✅      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| PATIENT_EDIT           |  ✅   |   ✅   |  ✅   |      ✅      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| PATIENT_DELETE         |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| PATIENT_EXPORT         |  ✅   |   ⚠️   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| **Medical Records**    |
| MEDICAL_RECORD_VIEW    |  ✅   |   ✅   |  ✅   |      ❌      |    ⚠️    |     ⚠️     |     ⚠️      |     ❌     |   ❌   |    ❌    |
| MEDICAL_RECORD_CREATE  |  ✅   |   ✅   |  ✅   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| MEDICAL_RECORD_EDIT    |  ✅   |   ✅   |  ⚠️   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| **Prescriptions**      |
| PRESCRIPTION_VIEW      |  ✅   |   ✅   |  ✅   |      ❌      |    ❌    |     ✅     |     ❌      |     ❌     |   ❌   |    ❌    |
| PRESCRIPTION_CREATE    |  ✅   |   ✅   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| CONTROLLED_SUBSTANCE   |  ✅   |   ✅   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| **Appointments**       |
| APPOINTMENT_VIEW       |  ✅   |   ✅   |  ✅   |      ✅      |    ⚠️    |     ⚠️     |     ⚠️      |     ❌     |   ❌   |    ❌    |
| APPOINTMENT_CREATE     |  ✅   |   ✅   |  ✅   |      ✅      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| APPOINTMENT_EDIT       |  ✅   |   ✅   |  ✅   |      ✅      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| **Billing**            |
| BILLING_VIEW           |  ✅   |   ⚠️   |  ❌   |      ✅      |    ❌    |     ❌     |     ❌      |     ✅     |   ❌   |    ❌    |
| BILLING_CREATE         |  ✅   |   ❌   |  ❌   |      ✅      |    ❌    |     ❌     |     ❌      |     ✅     |   ❌   |    ❌    |
| BILLING_REFUND         |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ✅     |   ❌   |    ❌    |
| **Lab**                |
| LAB_RESULT_VIEW        |  ✅   |   ✅   |  ✅   |      ❌      |    ✅    |     ⚠️     |     ❌      |     ❌     |   ❌   |    ❌    |
| LAB_RESULT_CREATE      |  ✅   |   ❌   |  ❌   |      ❌      |    ✅    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| LAB_RESULT_EDIT        |  ✅   |   ❌   |  ❌   |      ❌      |    ✅    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| **Imaging**            |
| IMAGING_VIEW           |  ✅   |   ✅   |  ⚠️   |      ❌      |    ❌    |     ❌     |     ✅      |     ❌     |   ❌   |    ❌    |
| IMAGING_UPLOAD         |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ✅      |     ❌     |   ❌   |    ❌    |
| **Admin**              |
| USER_MANAGE            |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ✅   |    ❌    |
| ROLE_MANAGE            |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| SETTINGS_MANAGE        |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |
| AUDIT_LOG_VIEW         |  ✅   |   ❌   |  ❌   |      ❌      |    ❌    |     ❌     |     ❌      |     ❌     |   ❌   |    ❌    |

Legend: ✅ Full Access | ⚠️ Limited/Contextual | ❌ No Access

---

## 4. Route Protection Matrix

### 4.1 Frontend Route Guards

| Route              | Component          | Required Roles                      | Guard Type             |
| ------------------ | ------------------ | ----------------------------------- | ---------------------- |
| `/`                | Dashboard          | Any authenticated                   | ProtectedRoute         |
| `/login`           | Login              | None (public)                       | None                   |
| `/patients`        | PatientsList       | Any authenticated                   | ProtectedRoute         |
| `/appointments`    | AppointmentsList   | Any authenticated                   | ProtectedRoute         |
| `/clinical-visits` | ClinicalVisitsList | Any authenticated                   | ProtectedRoute         |
| `/laboratory`      | LaboratoryList     | Admin, Doctor, Nurse, LabTechnician | ProtectedRoute + roles |
| `/hr`              | HRList             | Admin, HRManager                    | ProtectedRoute + roles |
| `/hr/employees`    | EmployeesList      | Admin, HRManager                    | ProtectedRoute + roles |
| `/hr/payroll`      | PayrollList        | Admin, HRManager                    | ProtectedRoute + roles |
| `/financial`       | FinancialList      | Admin, Accountant                   | ProtectedRoute + roles |
| `/inventory`       | InventoryList      | Admin, Pharmacist, Nurse            | ProtectedRoute + roles |
| `/pharmacy`        | PharmacyList       | Admin, Doctor, Pharmacist           | ProtectedRoute + roles |
| `/radiology`       | RadiologyList      | Admin, Doctor, Radiologist          | ProtectedRoute + roles |
| `/marketing`       | MarketingList      | Admin, MarketingManager             | ProtectedRoute + roles |
| `/analytics`       | AnalyticsDashboard | Admin                               | ProtectedRoute + roles |
| `/workflow/*`      | Workflow pages     | Admin                               | ProtectedRoute + roles |
| `/admin/*`         | Admin pages        | Admin                               | ProtectedRoute + roles |

### 4.2 Backend API Authorization

| Controller                  | Route Prefix      | Auth Attribute     | Policy               |
| --------------------------- | ----------------- | ------------------ | -------------------- |
| **PatientController**       | `/api/Patient`    | `[Authorize]`      | Default              |
| **SalesController**         | `/api/Sales`      | `[Authorize]`      | Default              |
| **RadiologyController**     | `/api/Radiology`  | `[Authorize]`      | Default              |
| **LaboratoryController**    | `/api/Laboratory` | `[Authorize]`      | Default              |
| **SecurityController**      | `/api/Security`   | `[Authorize]`      | Default + RoleManage |
| **PatientPortalController** | `/api/portal`     | Mixed (per-action) | Default              |
| **NotificationHub**         | SignalR           | `[Authorize]`      | Default              |

---

## 5. Multi-Level Access Control

### 5.1 Tenant Isolation

| Level         | Implementation                                     | Evidence                       |
| ------------- | -------------------------------------------------- | ------------------------------ |
| Super Admin   | `ITenantContextAccessor.IsSuperAdmin`              | Bypasses all checks            |
| Company Admin | `ITenantContextAccessor.IsCompanyAdmin`            | Access all branches in company |
| Branch User   | `ITenantContextAccessor.HasBranchAccess(branchId)` | Specific branch only           |

### 5.2 Branch Authorization Attribute

**File:** `XenonClinic.Infrastructure/Authorization/BranchAuthorizationAttribute.cs`

```csharp
// Checks performed in order:
1. IsSuperAdmin → Allow all
2. IsCompanyAdmin (if allowCompanyAdmin=true) → Allow all company branches
3. Extract branchId from route/query/header
4. HasBranchAccess(branchId) → Allow/Deny
```

### 5.3 Access Check Flow

```
Request
    │
    ▼
┌─────────────────────┐
│ JWT Token Present?  │──No──▶ 401 Unauthorized
└─────────────────────┘
    │Yes
    ▼
┌─────────────────────┐
│ Token Valid?        │──No──▶ 401 Unauthorized
└─────────────────────┘
    │Yes
    ▼
┌─────────────────────┐
│ [Authorize] Check   │──Fail──▶ 403 Forbidden
└─────────────────────┘
    │Pass
    ▼
┌─────────────────────┐
│ Policy/Role Check   │──Fail──▶ 403 Forbidden
└─────────────────────┘
    │Pass
    ▼
┌─────────────────────┐
│ Branch Auth Check   │──Fail──▶ 403 Forbidden
└─────────────────────┘
    │Pass
    ▼
┌─────────────────────┐
│ Service Permission  │──Fail──▶ Business error
│ Check (optional)    │
└─────────────────────┘
    │Pass
    ▼
   Success
```

---

## 6. Security Gaps & Risks

### 6.1 Bypass Risk Assessment

| Risk                        | UI Protected   | API Protected          | Gap               |
| --------------------------- | -------------- | ---------------------- | ----------------- |
| Unauthorized patient access | ⚠️ Any auth    | ✅ [Authorize]         | UI too permissive |
| Cross-tenant data access    | N/A            | ⚠️ Needs verification  | Test required     |
| Cross-branch data access    | N/A            | ✅ BranchAuthorization | Verify coverage   |
| Role elevation              | ❌ Client-side | ✅ [Authorize(Policy)] | API protected     |
| Direct API bypass           | N/A            | ✅ JWT required        | Good              |

### 6.2 Identified Issues

1. **Specialty Modules** - Most use `ProtectedRoute` without role checks (any authenticated user)
2. **Permission Enforcement** - `IRbacService.HasPermissionAsync` exists but unclear if used consistently
3. **PHI Access Logging** - `IsPHIRelated` flag defined but audit logging coverage unknown

### 6.3 Recommendations

1. **Add role requirements** to specialty module routes
2. **Implement consistent** service-layer permission checks
3. **Verify audit logging** for all PHI-related operations
4. **Test cross-tenant isolation** in integration tests
5. **Add E2E tests** for authorization bypass attempts

---

## 7. Testing Requirements

### 7.1 Authorization Test Cases

| Test ID  | Scenario                     | Expected Result  |
| -------- | ---------------------------- | ---------------- |
| AUTH-001 | Access without token         | 401 Unauthorized |
| AUTH-002 | Access with expired token    | 401 Unauthorized |
| AUTH-003 | Access with invalid role     | 403 Forbidden    |
| AUTH-004 | Doctor accessing HR module   | 403 Forbidden    |
| AUTH-005 | Lab tech accessing radiology | 403 Forbidden    |
| AUTH-006 | Cross-branch data access     | 403 Forbidden    |
| AUTH-007 | Cross-tenant data access     | 403 Forbidden    |
| AUTH-008 | Admin bypass attempts        | Proper logging   |

### 7.2 API Bypass Tests

```typescript
// Test cases for each protected endpoint:
1. Call without Authorization header
2. Call with malformed JWT
3. Call with valid JWT but wrong tenant
4. Call with valid JWT but wrong branch
5. Call with valid JWT but insufficient role
6. Call with valid JWT and correct permissions
```

---

## 8. Permission Categories Summary

| Category           | Code                 | Typical Roles                      |
| ------------------ | -------------------- | ---------------------------------- |
| Patient Management | `PATIENT_MANAGEMENT` | Admin, Doctor, Nurse, Receptionist |
| Clinical Care      | `CLINICAL_CARE`      | Admin, Doctor, Nurse               |
| Prescriptions      | `PRESCRIPTIONS`      | Admin, Doctor, Pharmacist          |
| Scheduling         | `SCHEDULING`         | Admin, Doctor, Nurse, Receptionist |
| Billing            | `BILLING`            | Admin, Accountant, Receptionist    |
| Laboratory         | `LABORATORY`         | Admin, Doctor, Lab Technician      |
| Imaging            | `IMAGING`            | Admin, Doctor, Radiologist         |
| Reporting          | `REPORTING`          | Admin (configurable)               |
| Administration     | `ADMINISTRATION`     | Admin, HR Manager                  |
| Emergency          | `EMERGENCY`          | Doctor (with justification)        |
