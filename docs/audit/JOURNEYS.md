# XenonClinic - User Journeys for Testing

**Generated:** 2025-12-13
**Phase:** 2 - Derive Journeys & Prioritize
**Reference:** Full journeys documented in `/docs/MODULE_JOURNEYS.md`

---

## 1. Journey Prioritization

### P0 - Critical (Must Pass for Production)

| Journey ID       | Module       | Journey              | Entry Point              | Risk     |
| ---------------- | ------------ | -------------------- | ------------------------ | -------- |
| **PAT-JNY-001**  | Patient      | Patient Registration | `/patients` + Create     | High     |
| **PAT-JNY-002**  | Patient      | Patient Search       | `/patients`              | High     |
| **APT-JNY-001**  | Appointments | Appointment Booking  | `/appointments` + Create | High     |
| **APT-JNY-002**  | Appointments | Patient Check-in     | `/appointments` + Status | High     |
| **CLN-JNY-001**  | Clinical     | Vitals Recording     | `/clinical-visits`       | High     |
| **CLN-JNY-002**  | Clinical     | Diagnosis Entry      | `/clinical-visits`       | High     |
| **LAB-JNY-001**  | Laboratory   | Lab Order Creation   | `/laboratory`            | High     |
| **FIN-JNY-001**  | Financial    | Invoice Creation     | `/financial`             | High     |
| **AUTH-JNY-001** | Auth         | User Login           | `/login`                 | Critical |
| **AUTH-JNY-002** | Auth         | Role-based Access    | All protected routes     | Critical |

### P1 - Important (Should Pass)

| Journey ID      | Module       | Journey                 | Entry Point         | Risk   |
| --------------- | ------------ | ----------------------- | ------------------- | ------ |
| **PAT-JNY-003** | Patient      | Medical History Update  | `/patients/:id`     | Medium |
| **APT-JNY-003** | Appointments | Appointment Reschedule  | `/appointments/:id` | Medium |
| **APT-JNY-004** | Appointments | Appointment Cancel      | `/appointments/:id` | Medium |
| **CLN-JNY-003** | Clinical     | Prescription Entry      | `/clinical-visits`  | High   |
| **LAB-JNY-002** | Laboratory   | Lab Result Entry        | `/laboratory`       | Medium |
| **PHM-JNY-001** | Pharmacy     | Prescription Dispensing | `/pharmacy`         | Medium |
| **INV-JNY-001** | Inventory    | Stock Management        | `/inventory`        | Medium |
| **FIN-JNY-002** | Financial    | Payment Recording       | `/financial`        | Medium |
| **HR-JNY-001**  | HR           | Employee Management     | `/hr/employees`     | Medium |
| **WFL-JNY-001** | Workflow     | Workflow Definition     | `/workflow`         | Medium |

### P2 - Nice to Have

| Journey ID        | Module    | Journey                | Entry Point                    | Risk   |
| ----------------- | --------- | ---------------------- | ------------------------------ | ------ |
| Specialty modules | Various   | All specialty journeys | `/dental`, `/cardiology`, etc. | Low    |
| Marketing         | Marketing | Campaign Management    | `/marketing`                   | Low    |
| Analytics         | Analytics | Report Generation      | `/analytics`                   | Low    |
| Portal            | Portal    | Patient Self-Service   | `/portal/*`                    | Medium |

---

## 2. P0 Journey Specifications

### PAT-JNY-001: Patient Registration

**Entry Point:** `/patients` → "Add Patient" button
**Roles:** Receptionist, Nurse, Admin
**Priority:** P0
**Risk:** High (core functionality)

#### Steps Sequence

| Step | UI Action               | API Call                   | Data Change           |
| ---- | ----------------------- | -------------------------- | --------------------- |
| 1    | Navigate to `/patients` | `GET /api/PatientsApi`     | None                  |
| 2    | Click "Add Patient"     | None                       | None                  |
| 3    | Fill patient form       | None                       | None                  |
| 4    | Submit form             | `POST /api/PatientsApi`    | Insert Patient record |
| 5    | View confirmation       | `GET /api/PatientsApi/:id` | None                  |

#### Business Rules (Evidence-Based)

| Rule                        | Evidence               | Location                                |
| --------------------------- | ---------------------- | --------------------------------------- |
| Required fields: name, DOB  | PatientForm validation | `components/PatientForm.tsx`            |
| Unique patient ID generated | ISequenceGenerator     | `Core/Interfaces/ISequenceGenerator.cs` |
| Branch assignment           | IBranchEntity          | `Core/Interfaces/IBranchEntity.cs`      |

#### Test Data

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "phone": "+971501234567",
  "email": "john.doe@example.com",
  "address": "123 Test Street, Dubai"
}
```

#### Expected Results

- **UX:** Loading indicator during save, success toast, redirect to patient list
- **API:** 201 Created with patient ID
- **Persistence:** Patient record in database with assigned ID
- **Audit:** AuditLog entry for patient creation

---

### APT-JNY-001: Appointment Booking

**Entry Point:** `/appointments` → "New Appointment" button
**Roles:** Receptionist, Admin
**Priority:** P0
**Risk:** High (core functionality)

#### Steps Sequence

| Step | UI Action                   | API Call                         | Data Change        |
| ---- | --------------------------- | -------------------------------- | ------------------ |
| 1    | Navigate to `/appointments` | `GET /api/AppointmentsApi`       | None               |
| 2    | Click "New Appointment"     | None                             | None               |
| 3    | Search/select patient       | `GET /api/PatientsApi/search`    | None               |
| 4    | Select provider             | `GET /api/providers`             | None               |
| 5    | Select date/time            | `GET /api/AppointmentsApi/slots` | None               |
| 6    | Submit                      | `POST /api/AppointmentsApi`      | Insert Appointment |
| 7    | View confirmation           | Response                         | None               |

#### Business Rules

| Rule                           | Evidence            | Location                                 |
| ------------------------------ | ------------------- | ---------------------------------------- |
| No double-booking              | AppointmentService  | Service implementation                   |
| Provider availability check    | IAppointmentService | `Core/Interfaces/IAppointmentService.cs` |
| Appointment status = Scheduled | Entity default      | `Core/Entities/Appointment.cs`           |

---

### AUTH-JNY-001: User Login

**Entry Point:** `/login`
**Roles:** All
**Priority:** P0 Critical
**Risk:** Critical (security)

#### Steps Sequence

| Step | UI Action             | API Call                          | Data Change     |
| ---- | --------------------- | --------------------------------- | --------------- |
| 1    | Navigate to `/login`  | None                              | None            |
| 2    | Enter email           | None                              | None            |
| 3    | Enter password        | None                              | None            |
| 4    | Click Login           | `POST /api/AuthApi/login`         | Session created |
| 5    | Redirect to dashboard | `GET /api/AuthApi/getCurrentUser` | None            |

#### Security Rules

| Rule                  | Evidence             | Test                   |
| --------------------- | -------------------- | ---------------------- |
| Password hashed       | Identity framework   | Not plain text in logs |
| JWT issued on success | JwtBearer config     | Token in response      |
| Failed login logged   | SecurityEvent entity | Audit log entry        |
| Rate limiting         | AspNetCoreRateLimit  | 429 after limit        |

---

### AUTH-JNY-002: Role-based Access Control

**Entry Point:** All protected routes
**Priority:** P0 Critical
**Risk:** Critical (security)

#### Test Matrix

| Role    | Route        | Expected             | Evidence            |
| ------- | ------------ | -------------------- | ------------------- |
| None    | `/`          | Redirect to `/login` | ProtectedRoute      |
| Any     | `/patients`  | Allow                | No role check       |
| Doctor  | `/hr`        | 403 Forbidden        | Role: HRManager     |
| LabTech | `/radiology` | 403 Forbidden        | Role: Radiologist   |
| Admin   | All routes   | Allow                | Admin in all checks |

---

### LAB-JNY-001: Lab Order Creation

**Entry Point:** `/laboratory` → "New Order"
**Roles:** Doctor, Nurse, Admin
**Priority:** P0
**Risk:** High (clinical)

#### Steps Sequence

| Step | UI Action                 | API Call                         | Data Change     |
| ---- | ------------------------- | -------------------------------- | --------------- |
| 1    | Navigate to `/laboratory` | `GET /api/LaboratoryApi/orders`  | None            |
| 2    | Click "New Order"         | None                             | None            |
| 3    | Select patient            | `GET /api/PatientsApi/search`    | None            |
| 4    | Select tests              | `GET /api/LaboratoryApi/tests`   | None            |
| 5    | Set priority              | None                             | None            |
| 6    | Submit                    | `POST /api/LaboratoryApi/orders` | Insert LabOrder |
| 7    | View confirmation         | Response                         | None            |

---

### FIN-JNY-001: Invoice Creation

**Entry Point:** `/financial` → "New Invoice"
**Roles:** Accountant, Admin
**Priority:** P0
**Risk:** High (financial)

#### Steps Sequence

| Step | UI Action                | API Call                          | Data Change    |
| ---- | ------------------------ | --------------------------------- | -------------- |
| 1    | Navigate to `/financial` | `GET /api/FinancialApi/invoices`  | None           |
| 2    | Click "New Invoice"      | None                              | None           |
| 3    | Select patient           | Patient search                    | None           |
| 4    | Add line items           | None                              | None           |
| 5    | Set amounts              | None                              | None           |
| 6    | Submit                   | `POST /api/FinancialApi/invoices` | Insert Invoice |

#### Business Rules

| Rule                       | Evidence           | Test                    |
| -------------------------- | ------------------ | ----------------------- |
| Invoice number generated   | ISequenceGenerator | Unique INV-XXXXX        |
| Total calculated correctly | Business logic     | Sum of line items       |
| Audit trail                | IAuditService      | Invoice creation logged |

---

## 3. Negative Test Cases (Per Journey)

### All P0 Journeys Must Include

| Category           | Test Case             | Expected                          |
| ------------------ | --------------------- | --------------------------------- |
| **Validation**     | Empty required fields | Error message, no submit          |
| **Validation**     | Invalid data format   | Error message                     |
| **Auth**           | No token              | 401 Unauthorized                  |
| **Auth**           | Wrong role            | 403 Forbidden                     |
| **Concurrency**    | Simultaneous edits    | Last write wins or conflict error |
| **Data Integrity** | Invalid foreign key   | 400 Bad Request                   |
| **Error Handling** | Network failure       | Graceful error, retry option      |

---

## 4. Edge Cases

### PAT-JNY-001 Edge Cases

| Case               | Input             | Expected                      |
| ------------------ | ----------------- | ----------------------------- |
| Duplicate patient  | Same name + DOB   | Warning or merge suggestion   |
| Special characters | Name with accents | Accepted, displayed correctly |
| Future DOB         | DOB > today       | Validation error              |
| Very old patient   | DOB < 1900        | Accept (historical records)   |

### APT-JNY-001 Edge Cases

| Case                 | Input                | Expected                       |
| -------------------- | -------------------- | ------------------------------ |
| Past date            | Date < today         | Validation error               |
| Provider unavailable | Selected slot taken  | Error, suggest alternatives    |
| Weekend booking      | Saturday appointment | Accept or block (configurable) |
| Same-day booking     | Today's date         | Accept with warning            |

---

## 5. Data Verification Points

### Database Verification (E2E Tests)

For each P0 journey, verify:

1. **Record Creation:** New record exists with correct ID
2. **Data Accuracy:** All submitted fields stored correctly
3. **Relationships:** Foreign keys valid (patient, branch, user)
4. **Audit Trail:** AuditLog entry created
5. **Timestamps:** CreatedAt, UpdatedAt set correctly
6. **Soft Delete:** IsDeleted = false for new records

### API Response Verification

1. **Status Code:** Matches expected (200, 201, 400, 401, 403)
2. **Response Body:** Contains expected fields
3. **Headers:** Content-Type = application/json
4. **Errors:** Structured error format with details

---

## 6. Journey Dependencies

```
                    ┌──────────────────┐
                    │  AUTH-JNY-001    │
                    │  (Login)         │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
    │PAT-JNY-001  │  │APT-JNY-001  │  │FIN-JNY-001  │
    │(Register)   │  │(Book Appt)  │  │(Invoice)    │
    └──────┬──────┘  └──────┬──────┘  └─────────────┘
           │                │
           └───────┬────────┘
                   │
           ┌───────▼───────┐
           │  CLN-JNY-001  │
           │  (Vitals)     │
           └───────┬───────┘
                   │
           ┌───────▼───────┐
           │  CLN-JNY-002  │
           │  (Diagnosis)  │
           └───────┬───────┘
                   │
           ┌───────▼───────┐
           │  LAB-JNY-001  │
           │  (Lab Order)  │
           └───────────────┘
```

---

## 7. Test Execution Order

### Phase 1: Authentication (Must Pass First)

1. AUTH-JNY-001 - Login
2. AUTH-JNY-002 - RBAC

### Phase 2: Core Data (Patient First)

3. PAT-JNY-001 - Registration
4. PAT-JNY-002 - Search

### Phase 3: Scheduling

5. APT-JNY-001 - Booking
6. APT-JNY-002 - Check-in

### Phase 4: Clinical

7. CLN-JNY-001 - Vitals
8. CLN-JNY-002 - Diagnosis

### Phase 5: Ancillary

9. LAB-JNY-001 - Lab Orders
10. FIN-JNY-001 - Invoicing

---

## 8. Acceptance Criteria Summary

| Journey      | Pass Criteria                                   |
| ------------ | ----------------------------------------------- |
| AUTH-JNY-001 | Valid credentials → JWT + redirect to dashboard |
| AUTH-JNY-002 | Role mismatch → 403 on UI and API               |
| PAT-JNY-001  | Valid data → Patient created with unique ID     |
| PAT-JNY-002  | Search term → Matching patients returned        |
| APT-JNY-001  | Valid selection → Appointment created           |
| APT-JNY-002  | Check-in click → Status updated to CheckedIn    |
| CLN-JNY-001  | Vitals entered → Saved to visit record          |
| CLN-JNY-002  | ICD-10 selected → Diagnosis linked to visit     |
| LAB-JNY-001  | Tests selected → LabOrder created               |
| FIN-JNY-001  | Items added → Invoice with correct total        |
