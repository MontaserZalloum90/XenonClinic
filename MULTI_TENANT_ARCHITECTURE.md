# Multi-Tenant Configuration-Driven Architecture

## 1. Database Schema

### 1.1 Types Tables

```sql
-- Company Types (CLINIC, TRADING, etc.)
CREATE TABLE CompanyTypes (
    Code VARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IconName VARCHAR(50),
    IsActive BIT DEFAULT 1,
    SortOrder INT DEFAULT 0
);

-- Clinic Types (only used when CompanyType = CLINIC)
CREATE TABLE ClinicTypes (
    Code VARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IconName VARCHAR(50),
    IsActive BIT DEFAULT 1,
    SortOrder INT DEFAULT 0
);
```

### 1.2 Feature Registry

```sql
-- Master feature list
CREATE TABLE Features (
    Code VARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Category VARCHAR(50), -- 'core', 'clinical', 'trading', 'admin'
    IconName VARCHAR(50),
    DefaultRoute VARCHAR(100),
    SortOrder INT DEFAULT 0
);

-- Features enabled per tenant (overrides templates)
CREATE TABLE TenantFeatures (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    FeatureCode VARCHAR(50) NOT NULL REFERENCES Features(Code),
    Enabled BIT NOT NULL DEFAULT 1,
    SettingsJson NVARCHAR(MAX), -- JSON for feature-specific settings
    UNIQUE(TenantId, FeatureCode)
);
```

### 1.3 Templates

```sql
-- Base templates for company types
CREATE TABLE CompanyTypeTemplates (
    Id INT IDENTITY PRIMARY KEY,
    CompanyTypeCode VARCHAR(50) NOT NULL REFERENCES CompanyTypes(Code),
    FeaturesJson NVARCHAR(MAX) NOT NULL, -- Array of feature codes
    TerminologyJson NVARCHAR(MAX) NOT NULL,
    NavigationJson NVARCHAR(MAX) NOT NULL,
    UISchemasJson NVARCHAR(MAX),
    FormLayoutsJson NVARCHAR(MAX),
    ListLayoutsJson NVARCHAR(MAX),
    IsDefault BIT DEFAULT 0,
    UNIQUE(CompanyTypeCode)
);

-- Override templates for clinic types
CREATE TABLE ClinicTypeTemplates (
    Id INT IDENTITY PRIMARY KEY,
    ClinicTypeCode VARCHAR(50) NOT NULL REFERENCES ClinicTypes(Code),
    FeaturesJson NVARCHAR(MAX), -- Additional features to enable
    TerminologyJson NVARCHAR(MAX), -- Overrides
    NavigationJson NVARCHAR(MAX), -- Additional nav items
    UISchemasJson NVARCHAR(MAX), -- Schema overrides
    FormLayoutsJson NVARCHAR(MAX), -- Layout overrides
    ListLayoutsJson NVARCHAR(MAX),
    IsDefault BIT DEFAULT 0,
    UNIQUE(ClinicTypeCode)
);
```

### 1.4 Tenant Configuration (Overrides)

```sql
-- Extend existing Companies table
ALTER TABLE Companies ADD
    CompanyTypeCode VARCHAR(50) REFERENCES CompanyTypes(Code),
    ClinicTypeCode VARCHAR(50) NULL REFERENCES ClinicTypes(Code);

-- Tenant-level terminology overrides
CREATE TABLE TenantTerminology (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    Key NVARCHAR(100) NOT NULL,
    Value NVARCHAR(500) NOT NULL,
    UNIQUE(TenantId, Key)
);

-- Tenant-level UI schema overrides
CREATE TABLE TenantUISchemas (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    EntityName VARCHAR(50) NOT NULL,
    SchemaJson NVARCHAR(MAX) NOT NULL,
    UNIQUE(TenantId, EntityName)
);

-- Tenant-level form layout overrides
CREATE TABLE TenantFormLayouts (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    EntityName VARCHAR(50) NOT NULL,
    LayoutJson NVARCHAR(MAX) NOT NULL,
    UNIQUE(TenantId, EntityName)
);

-- Tenant-level list layout overrides
CREATE TABLE TenantListLayouts (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    EntityName VARCHAR(50) NOT NULL,
    LayoutJson NVARCHAR(MAX) NOT NULL,
    UNIQUE(TenantId, EntityName)
);

-- Tenant navigation overrides
CREATE TABLE TenantNavigation (
    Id INT IDENTITY PRIMARY KEY,
    TenantId INT NOT NULL REFERENCES Tenants(Id),
    NavigationJson NVARCHAR(MAX) NOT NULL,
    UNIQUE(TenantId)
);
```

---

## 2. TypeScript Interfaces

```typescript
// ============================================
// Company & Clinic Types
// ============================================

export type CompanyTypeCode = 'CLINIC' | 'TRADING';
export type ClinicTypeCode = 'AUDIOLOGY' | 'DENTAL' | 'VET' | 'DERMATOLOGY' | 'OPHTHALMOLOGY' | 'GENERAL';

// ============================================
// Feature System
// ============================================

export interface Feature {
  code: string;
  name: string;
  description?: string;
  category: 'core' | 'clinical' | 'trading' | 'admin';
  iconName?: string;
  defaultRoute?: string;
  enabled: boolean;
  settings?: Record<string, any>;
}

export interface FeatureMap {
  [featureCode: string]: {
    enabled: boolean;
    settings?: Record<string, any>;
  };
}

// ============================================
// Navigation
// ============================================

export interface NavItem {
  id: string;
  label: string; // terminology key, e.g., "nav.patients"
  icon: string;
  route: string;
  featureCode: string; // Required feature to show this item
  requiredRoles?: string[];
  children?: NavItem[];
  badge?: {
    type: 'count' | 'dot';
    countKey?: string; // API key for count
  };
  sortOrder: number;
}

// ============================================
// UI Schema (Field Definitions)
// ============================================

export type FieldType =
  | 'text' | 'textarea' | 'number' | 'currency' | 'percentage'
  | 'date' | 'datetime' | 'time'
  | 'select' | 'multiselect' | 'radio' | 'checkbox' | 'toggle'
  | 'lookup' | 'phone' | 'email' | 'url'
  | 'file' | 'image'
  | 'emiratesId' | 'passport'
  | 'address' | 'coordinates'
  | 'richtext' | 'markdown'
  | 'json' | 'code';

export interface FieldValidation {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: string;
  patternMessage?: string;
  custom?: string; // Name of custom validator function
}

export interface ConditionalRule {
  field: string;
  operator: 'eq' | 'neq' | 'gt' | 'gte' | 'lt' | 'lte' | 'in' | 'notIn' | 'contains' | 'empty' | 'notEmpty';
  value?: any;
}

export interface FieldDefinition {
  name: string;
  type: FieldType;
  label?: string; // terminology key override
  placeholder?: string;
  helpText?: string;
  defaultValue?: any;
  validation?: FieldValidation;

  // For select/lookup
  options?: { value: any; label: string }[];
  lookupEndpoint?: string;
  lookupDisplayField?: string;
  lookupValueField?: string;

  // Visibility/behavior
  visible?: boolean | ConditionalRule[];
  disabled?: boolean | ConditionalRule[];
  readOnly?: boolean;

  // Display
  width?: 'full' | 'half' | 'third' | 'quarter';
  sortable?: boolean;
  filterable?: boolean;
  searchable?: boolean;

  // For currency/number
  currency?: string;
  decimals?: number;

  // For file upload
  accept?: string;
  maxSize?: number;
  multiple?: boolean;
}

export interface UISchema {
  entityName: string;
  displayName: string; // terminology key
  displayNamePlural: string;
  primaryField: string; // field used as display name
  fields: FieldDefinition[];
  defaultSort?: { field: string; direction: 'asc' | 'desc' };
}

// ============================================
// Form Layout
// ============================================

export interface FormSection {
  id: string;
  title: string; // terminology key
  description?: string;
  collapsible?: boolean;
  defaultCollapsed?: boolean;
  visible?: boolean | ConditionalRule[];
  columns?: 1 | 2 | 3 | 4;
  fields: string[]; // field names from UISchema
}

export interface FormLayout {
  entityName: string;
  sections: FormSection[];
  submitLabel?: string;
  cancelLabel?: string;
  showDelete?: boolean;
  deleteConfirmMessage?: string;
}

// ============================================
// List/Table Layout
// ============================================

export interface ListColumn {
  field: string;
  width?: number | string;
  align?: 'left' | 'center' | 'right';
  format?: 'currency' | 'date' | 'datetime' | 'percentage' | 'badge' | 'avatar';
  sortable?: boolean;
  hidden?: boolean;
}

export interface ListAction {
  id: string;
  label: string; // terminology key
  icon: string;
  type: 'primary' | 'secondary' | 'danger';
  requiresSelection?: boolean;
  confirmMessage?: string;
  featureCode?: string;
  requiredRoles?: string[];
}

export interface ListFilter {
  field: string;
  type: 'text' | 'select' | 'date' | 'dateRange' | 'boolean';
  options?: { value: any; label: string }[];
}

export interface ListLayout {
  entityName: string;
  columns: ListColumn[];
  actions: {
    row: ListAction[];
    bulk: ListAction[];
    header: ListAction[];
  };
  filters: ListFilter[];
  defaultPageSize: number;
  pageSizeOptions: number[];
  showSearch: boolean;
  searchFields: string[];
}

// ============================================
// Tenant Context (API Response)
// ============================================

export interface TenantContext {
  tenantId: number;
  tenantName: string;
  companyId: number;
  companyName: string;
  companyType: CompanyTypeCode;
  clinicType: ClinicTypeCode | null;
  branchId: number;
  branchName: string;

  // Branding
  logoUrl: string | null;
  primaryColor: string;
  secondaryColor: string;

  // User context
  userId: string;
  userName: string;
  userRoles: string[];
  userPermissions: string[];

  // Configuration (merged based on precedence)
  features: FeatureMap;
  terminology: Record<string, string>;
  navigation: NavItem[];

  // UI configuration
  uiSchemas: Record<string, UISchema>;
  formLayouts: Record<string, FormLayout>;
  listLayouts: Record<string, ListLayout>;

  // Settings
  settings: {
    currency: string;
    timezone: string;
    dateFormat: string;
    timeFormat: string;
    language: string;
  };
}
```

---

## 3. JSON Template Packs

### 3.1 TRADING Template (Base)

```json
{
  "companyType": "TRADING",
  "features": [
    "dashboard",
    "customers",
    "leads",
    "products",
    "orders",
    "quotations",
    "inventory",
    "billing",
    "payments",
    "reports",
    "settings"
  ],
  "terminology": {
    "entity.patient.singular": "Customer",
    "entity.patient.plural": "Customers",
    "entity.visit.singular": "Order",
    "entity.visit.plural": "Orders",
    "entity.appointment.singular": "Meeting",
    "entity.appointment.plural": "Meetings",
    "entity.invoice.singular": "Invoice",
    "entity.invoice.plural": "Invoices",
    "role.doctor": "Sales Representative",
    "role.nurse": "Sales Associate",
    "role.receptionist": "Customer Service",
    "nav.patients": "Customers",
    "nav.appointments": "Meetings",
    "nav.visits": "Orders",
    "nav.audiology": "Products",
    "page.patients.title": "Customer Management",
    "page.patients.subtitle": "Manage your customers and leads",
    "page.patients.addNew": "Add Customer",
    "page.patients.empty": "No customers found",
    "page.appointments.title": "Meeting Schedule",
    "page.visits.title": "Order Management",
    "form.patient.firstName": "First Name",
    "form.patient.lastName": "Last Name",
    "form.patient.phone": "Phone",
    "form.patient.email": "Email",
    "form.patient.company": "Company",
    "form.patient.emiratesId": "Trade License",
    "table.patient.name": "Customer Name",
    "table.patient.contact": "Contact",
    "action.addPatient": "Add Customer",
    "action.editPatient": "Edit Customer",
    "action.deletePatient": "Delete Customer"
  },
  "navigation": [
    {
      "id": "dashboard",
      "label": "nav.dashboard",
      "icon": "LayoutDashboard",
      "route": "/",
      "featureCode": "dashboard",
      "sortOrder": 0
    },
    {
      "id": "customers",
      "label": "nav.patients",
      "icon": "Users",
      "route": "/customers",
      "featureCode": "customers",
      "sortOrder": 10
    },
    {
      "id": "leads",
      "label": "nav.leads",
      "icon": "Target",
      "route": "/leads",
      "featureCode": "leads",
      "sortOrder": 20
    },
    {
      "id": "products",
      "label": "nav.products",
      "icon": "Package",
      "route": "/products",
      "featureCode": "products",
      "sortOrder": 30
    },
    {
      "id": "orders",
      "label": "nav.visits",
      "icon": "ShoppingCart",
      "route": "/orders",
      "featureCode": "orders",
      "sortOrder": 40
    },
    {
      "id": "quotations",
      "label": "nav.quotations",
      "icon": "FileText",
      "route": "/quotations",
      "featureCode": "quotations",
      "sortOrder": 50
    },
    {
      "id": "inventory",
      "label": "nav.inventory",
      "icon": "Warehouse",
      "route": "/inventory",
      "featureCode": "inventory",
      "sortOrder": 60
    },
    {
      "id": "billing",
      "label": "nav.billing",
      "icon": "Receipt",
      "route": "/billing",
      "featureCode": "billing",
      "sortOrder": 70
    },
    {
      "id": "reports",
      "label": "nav.reports",
      "icon": "BarChart",
      "route": "/reports",
      "featureCode": "reports",
      "sortOrder": 80
    },
    {
      "id": "settings",
      "label": "nav.settings",
      "icon": "Settings",
      "route": "/settings",
      "featureCode": "settings",
      "sortOrder": 100
    }
  ],
  "uiSchemas": {
    "customer": {
      "entityName": "customer",
      "displayName": "entity.patient.singular",
      "displayNamePlural": "entity.patient.plural",
      "primaryField": "fullName",
      "fields": [
        { "name": "firstName", "type": "text", "label": "form.patient.firstName", "validation": { "required": true, "maxLength": 100 } },
        { "name": "lastName", "type": "text", "label": "form.patient.lastName", "validation": { "required": true, "maxLength": 100 } },
        { "name": "email", "type": "email", "label": "form.patient.email", "validation": { "required": true } },
        { "name": "phone", "type": "phone", "label": "form.patient.phone", "validation": { "required": true } },
        { "name": "company", "type": "text", "label": "form.patient.company" },
        { "name": "tradeLicense", "type": "text", "label": "form.patient.emiratesId" },
        { "name": "address", "type": "textarea", "label": "form.address" },
        { "name": "notes", "type": "textarea", "label": "form.notes" },
        { "name": "status", "type": "select", "label": "form.status", "options": [
          { "value": "active", "label": "Active" },
          { "value": "inactive", "label": "Inactive" },
          { "value": "prospect", "label": "Prospect" }
        ]}
      ]
    }
  },
  "formLayouts": {
    "customer": {
      "entityName": "customer",
      "sections": [
        {
          "id": "basic",
          "title": "section.basicInfo",
          "columns": 2,
          "fields": ["firstName", "lastName", "email", "phone"]
        },
        {
          "id": "business",
          "title": "section.businessInfo",
          "columns": 2,
          "fields": ["company", "tradeLicense", "status"]
        },
        {
          "id": "additional",
          "title": "section.additional",
          "columns": 1,
          "fields": ["address", "notes"]
        }
      ]
    }
  },
  "listLayouts": {
    "customer": {
      "entityName": "customer",
      "columns": [
        { "field": "fullName", "sortable": true },
        { "field": "email" },
        { "field": "phone" },
        { "field": "company" },
        { "field": "status", "format": "badge" }
      ],
      "actions": {
        "row": [
          { "id": "view", "label": "action.view", "icon": "Eye", "type": "secondary" },
          { "id": "edit", "label": "action.edit", "icon": "Pencil", "type": "primary" },
          { "id": "delete", "label": "action.delete", "icon": "Trash", "type": "danger", "confirmMessage": "confirm.delete" }
        ],
        "bulk": [
          { "id": "export", "label": "action.export", "icon": "Download", "type": "secondary" },
          { "id": "delete", "label": "action.delete", "icon": "Trash", "type": "danger", "confirmMessage": "confirm.bulkDelete" }
        ],
        "header": [
          { "id": "create", "label": "action.addPatient", "icon": "Plus", "type": "primary" },
          { "id": "import", "label": "action.import", "icon": "Upload", "type": "secondary" }
        ]
      },
      "filters": [
        { "field": "status", "type": "select", "options": [
          { "value": "active", "label": "Active" },
          { "value": "inactive", "label": "Inactive" },
          { "value": "prospect", "label": "Prospect" }
        ]}
      ],
      "defaultPageSize": 25,
      "pageSizeOptions": [10, 25, 50, 100],
      "showSearch": true,
      "searchFields": ["firstName", "lastName", "email", "phone", "company"]
    }
  }
}
```

### 3.2 CLINIC Base Template

```json
{
  "companyType": "CLINIC",
  "features": [
    "dashboard",
    "patients",
    "appointments",
    "visits",
    "billing",
    "payments",
    "inventory",
    "laboratory",
    "reports",
    "hr",
    "settings"
  ],
  "terminology": {
    "entity.patient.singular": "Patient",
    "entity.patient.plural": "Patients",
    "entity.visit.singular": "Visit",
    "entity.visit.plural": "Visits",
    "entity.appointment.singular": "Appointment",
    "entity.appointment.plural": "Appointments",
    "entity.invoice.singular": "Invoice",
    "entity.invoice.plural": "Invoices",
    "entity.encounter.singular": "Encounter",
    "entity.encounter.plural": "Encounters",
    "role.doctor": "Doctor",
    "role.nurse": "Nurse",
    "role.receptionist": "Receptionist",
    "role.labTechnician": "Lab Technician",
    "nav.patients": "Patients",
    "nav.appointments": "Appointments",
    "nav.visits": "Visits",
    "nav.billing": "Billing",
    "nav.laboratory": "Laboratory",
    "nav.inventory": "Inventory",
    "nav.reports": "Reports",
    "nav.hr": "HR",
    "nav.settings": "Settings",
    "page.patients.title": "Patient Management",
    "page.patients.subtitle": "Manage patient records and medical history",
    "page.patients.addNew": "New Patient",
    "page.patients.empty": "No patients found",
    "page.appointments.title": "Appointment Schedule",
    "page.visits.title": "Patient Visits",
    "form.patient.firstName": "First Name",
    "form.patient.lastName": "Last Name",
    "form.patient.dateOfBirth": "Date of Birth",
    "form.patient.gender": "Gender",
    "form.patient.phone": "Phone",
    "form.patient.email": "Email",
    "form.patient.emiratesId": "Emirates ID",
    "form.patient.passport": "Passport Number",
    "form.patient.nationality": "Nationality",
    "form.patient.address": "Address",
    "form.patient.emergencyContact": "Emergency Contact",
    "form.patient.bloodType": "Blood Type",
    "form.patient.allergies": "Allergies",
    "form.patient.medicalHistory": "Medical History",
    "table.patient.name": "Patient Name",
    "table.patient.contact": "Contact",
    "table.patient.lastVisit": "Last Visit",
    "action.addPatient": "Add Patient",
    "action.editPatient": "Edit Patient",
    "action.deletePatient": "Delete Patient",
    "action.viewHistory": "View History",
    "action.newAppointment": "New Appointment",
    "section.basicInfo": "Basic Information",
    "section.contactInfo": "Contact Information",
    "section.medicalInfo": "Medical Information",
    "section.emergencyInfo": "Emergency Contact",
    "section.additional": "Additional Information"
  },
  "navigation": [
    {
      "id": "dashboard",
      "label": "nav.dashboard",
      "icon": "LayoutDashboard",
      "route": "/",
      "featureCode": "dashboard",
      "sortOrder": 0
    },
    {
      "id": "patients",
      "label": "nav.patients",
      "icon": "Users",
      "route": "/patients",
      "featureCode": "patients",
      "sortOrder": 10
    },
    {
      "id": "appointments",
      "label": "nav.appointments",
      "icon": "Calendar",
      "route": "/appointments",
      "featureCode": "appointments",
      "sortOrder": 20,
      "badge": { "type": "count", "countKey": "todayAppointments" }
    },
    {
      "id": "visits",
      "label": "nav.visits",
      "icon": "ClipboardList",
      "route": "/visits",
      "featureCode": "visits",
      "sortOrder": 30
    },
    {
      "id": "laboratory",
      "label": "nav.laboratory",
      "icon": "FlaskConical",
      "route": "/laboratory",
      "featureCode": "laboratory",
      "sortOrder": 40,
      "requiredRoles": ["Admin", "Doctor", "LabTechnician"]
    },
    {
      "id": "inventory",
      "label": "nav.inventory",
      "icon": "Package",
      "route": "/inventory",
      "featureCode": "inventory",
      "sortOrder": 50
    },
    {
      "id": "billing",
      "label": "nav.billing",
      "icon": "Receipt",
      "route": "/billing",
      "featureCode": "billing",
      "sortOrder": 60
    },
    {
      "id": "hr",
      "label": "nav.hr",
      "icon": "UserCog",
      "route": "/hr",
      "featureCode": "hr",
      "sortOrder": 70,
      "requiredRoles": ["Admin", "HRManager"]
    },
    {
      "id": "reports",
      "label": "nav.reports",
      "icon": "BarChart",
      "route": "/reports",
      "featureCode": "reports",
      "sortOrder": 80
    },
    {
      "id": "settings",
      "label": "nav.settings",
      "icon": "Settings",
      "route": "/settings",
      "featureCode": "settings",
      "sortOrder": 100
    }
  ],
  "uiSchemas": {
    "patient": {
      "entityName": "patient",
      "displayName": "entity.patient.singular",
      "displayNamePlural": "entity.patient.plural",
      "primaryField": "fullName",
      "fields": [
        { "name": "firstName", "type": "text", "label": "form.patient.firstName", "validation": { "required": true, "maxLength": 100 }, "width": "half" },
        { "name": "lastName", "type": "text", "label": "form.patient.lastName", "validation": { "required": true, "maxLength": 100 }, "width": "half" },
        { "name": "dateOfBirth", "type": "date", "label": "form.patient.dateOfBirth", "validation": { "required": true }, "width": "half" },
        { "name": "gender", "type": "select", "label": "form.patient.gender", "options": [
          { "value": "male", "label": "Male" },
          { "value": "female", "label": "Female" }
        ], "validation": { "required": true }, "width": "half" },
        { "name": "phone", "type": "phone", "label": "form.patient.phone", "validation": { "required": true }, "width": "half" },
        { "name": "email", "type": "email", "label": "form.patient.email", "width": "half" },
        { "name": "emiratesId", "type": "emiratesId", "label": "form.patient.emiratesId", "width": "half" },
        { "name": "passport", "type": "passport", "label": "form.patient.passport", "width": "half" },
        { "name": "nationality", "type": "select", "label": "form.patient.nationality", "lookupEndpoint": "/api/lookups/nationalities", "width": "half" },
        { "name": "bloodType", "type": "select", "label": "form.patient.bloodType", "options": [
          { "value": "A+", "label": "A+" }, { "value": "A-", "label": "A-" },
          { "value": "B+", "label": "B+" }, { "value": "B-", "label": "B-" },
          { "value": "AB+", "label": "AB+" }, { "value": "AB-", "label": "AB-" },
          { "value": "O+", "label": "O+" }, { "value": "O-", "label": "O-" }
        ], "width": "half" },
        { "name": "address", "type": "textarea", "label": "form.patient.address", "width": "full" },
        { "name": "allergies", "type": "multiselect", "label": "form.patient.allergies", "lookupEndpoint": "/api/lookups/allergies", "width": "full" },
        { "name": "medicalHistory", "type": "textarea", "label": "form.patient.medicalHistory", "width": "full" },
        { "name": "emergencyContactName", "type": "text", "label": "form.emergencyContactName", "width": "half" },
        { "name": "emergencyContactPhone", "type": "phone", "label": "form.emergencyContactPhone", "width": "half" },
        { "name": "emergencyContactRelation", "type": "select", "label": "form.emergencyContactRelation", "options": [
          { "value": "spouse", "label": "Spouse" },
          { "value": "parent", "label": "Parent" },
          { "value": "sibling", "label": "Sibling" },
          { "value": "child", "label": "Child" },
          { "value": "friend", "label": "Friend" },
          { "value": "other", "label": "Other" }
        ], "width": "half" }
      ]
    }
  },
  "formLayouts": {
    "patient": {
      "entityName": "patient",
      "sections": [
        {
          "id": "basic",
          "title": "section.basicInfo",
          "columns": 2,
          "fields": ["firstName", "lastName", "dateOfBirth", "gender"]
        },
        {
          "id": "contact",
          "title": "section.contactInfo",
          "columns": 2,
          "fields": ["phone", "email", "address"]
        },
        {
          "id": "identification",
          "title": "section.identification",
          "columns": 2,
          "fields": ["emiratesId", "passport", "nationality"]
        },
        {
          "id": "medical",
          "title": "section.medicalInfo",
          "columns": 2,
          "collapsible": true,
          "fields": ["bloodType", "allergies", "medicalHistory"]
        },
        {
          "id": "emergency",
          "title": "section.emergencyInfo",
          "columns": 2,
          "collapsible": true,
          "fields": ["emergencyContactName", "emergencyContactPhone", "emergencyContactRelation"]
        }
      ]
    }
  },
  "listLayouts": {
    "patient": {
      "entityName": "patient",
      "columns": [
        { "field": "fullName", "sortable": true },
        { "field": "emiratesId" },
        { "field": "phone" },
        { "field": "dateOfBirth", "format": "date" },
        { "field": "gender", "format": "badge" },
        { "field": "lastVisitDate", "format": "date" }
      ],
      "actions": {
        "row": [
          { "id": "view", "label": "action.view", "icon": "Eye", "type": "secondary" },
          { "id": "edit", "label": "action.edit", "icon": "Pencil", "type": "primary" },
          { "id": "newAppointment", "label": "action.newAppointment", "icon": "Calendar", "type": "secondary", "featureCode": "appointments" },
          { "id": "delete", "label": "action.delete", "icon": "Trash", "type": "danger", "confirmMessage": "confirm.delete" }
        ],
        "bulk": [
          { "id": "export", "label": "action.export", "icon": "Download", "type": "secondary" }
        ],
        "header": [
          { "id": "create", "label": "action.addPatient", "icon": "Plus", "type": "primary" },
          { "id": "import", "label": "action.import", "icon": "Upload", "type": "secondary" }
        ]
      },
      "filters": [
        { "field": "gender", "type": "select", "options": [
          { "value": "male", "label": "Male" },
          { "value": "female", "label": "Female" }
        ]},
        { "field": "dateOfBirth", "type": "dateRange" }
      ],
      "defaultPageSize": 25,
      "pageSizeOptions": [10, 25, 50, 100],
      "showSearch": true,
      "searchFields": ["firstName", "lastName", "emiratesId", "phone", "email"]
    }
  }
}
```

### 3.3 AUDIOLOGY Override Template

```json
{
  "clinicType": "AUDIOLOGY",
  "features": [
    "audiogram",
    "hearingDevices",
    "fittingSessions",
    "speechAudiometry"
  ],
  "terminology": {
    "nav.audiology": "Audiology",
    "nav.audiograms": "Audiograms",
    "nav.hearingDevices": "Hearing Devices",
    "nav.fittingSessions": "Fitting Sessions",
    "page.audiology.title": "Audiology Center",
    "entity.audiogram.singular": "Audiogram",
    "entity.audiogram.plural": "Audiograms",
    "entity.hearingDevice.singular": "Hearing Device",
    "entity.hearingDevice.plural": "Hearing Devices",
    "entity.fittingSession.singular": "Fitting Session",
    "entity.fittingSession.plural": "Fitting Sessions",
    "form.audiogram.testDate": "Test Date",
    "form.audiogram.testType": "Test Type",
    "form.audiogram.ear": "Ear",
    "form.audiogram.frequency": "Frequency (Hz)",
    "form.audiogram.threshold": "Threshold (dB)",
    "form.hearingDevice.manufacturer": "Manufacturer",
    "form.hearingDevice.model": "Model",
    "form.hearingDevice.serialNumber": "Serial Number",
    "form.hearingDevice.side": "Side",
    "form.hearingDevice.purchaseDate": "Purchase Date",
    "form.hearingDevice.warrantyExpiry": "Warranty Expiry",
    "section.audiogramResults": "Audiogram Results",
    "section.deviceDetails": "Device Details",
    "section.fittingNotes": "Fitting Notes"
  },
  "navigation": [
    {
      "id": "audiology",
      "label": "nav.audiology",
      "icon": "Ear",
      "route": "/audiology",
      "featureCode": "audiogram",
      "sortOrder": 35,
      "children": [
        {
          "id": "audiograms",
          "label": "nav.audiograms",
          "icon": "Activity",
          "route": "/audiology/audiograms",
          "featureCode": "audiogram",
          "sortOrder": 0
        },
        {
          "id": "hearingDevices",
          "label": "nav.hearingDevices",
          "icon": "Headphones",
          "route": "/audiology/devices",
          "featureCode": "hearingDevices",
          "sortOrder": 1
        },
        {
          "id": "fittingSessions",
          "label": "nav.fittingSessions",
          "icon": "Wrench",
          "route": "/audiology/fittings",
          "featureCode": "fittingSessions",
          "sortOrder": 2
        }
      ]
    }
  ],
  "uiSchemas": {
    "audiogram": {
      "entityName": "audiogram",
      "displayName": "entity.audiogram.singular",
      "displayNamePlural": "entity.audiogram.plural",
      "primaryField": "testDate",
      "fields": [
        { "name": "patientId", "type": "lookup", "label": "entity.patient.singular", "lookupEndpoint": "/api/patients", "lookupDisplayField": "fullName", "validation": { "required": true } },
        { "name": "testDate", "type": "datetime", "label": "form.audiogram.testDate", "validation": { "required": true } },
        { "name": "testType", "type": "select", "label": "form.audiogram.testType", "options": [
          { "value": "puretoneBoth", "label": "Pure Tone (Both Ears)" },
          { "value": "puretoneRight", "label": "Pure Tone (Right)" },
          { "value": "puretoneLeft", "label": "Pure Tone (Left)" },
          { "value": "speech", "label": "Speech Audiometry" },
          { "value": "tympanometry", "label": "Tympanometry" }
        ], "validation": { "required": true } },
        { "name": "rightEarData", "type": "json", "label": "form.audiogram.rightEar" },
        { "name": "leftEarData", "type": "json", "label": "form.audiogram.leftEar" },
        { "name": "interpretation", "type": "textarea", "label": "form.interpretation" },
        { "name": "recommendations", "type": "textarea", "label": "form.recommendations" },
        { "name": "audiologistId", "type": "lookup", "label": "role.audiologist", "lookupEndpoint": "/api/employees?role=audiologist" }
      ]
    },
    "hearingDevice": {
      "entityName": "hearingDevice",
      "displayName": "entity.hearingDevice.singular",
      "displayNamePlural": "entity.hearingDevice.plural",
      "primaryField": "model",
      "fields": [
        { "name": "patientId", "type": "lookup", "label": "entity.patient.singular", "lookupEndpoint": "/api/patients", "validation": { "required": true } },
        { "name": "manufacturer", "type": "select", "label": "form.hearingDevice.manufacturer", "options": [
          { "value": "phonak", "label": "Phonak" },
          { "value": "widex", "label": "Widex" },
          { "value": "resound", "label": "ReSound" },
          { "value": "signia", "label": "Signia" },
          { "value": "oticon", "label": "Oticon" },
          { "value": "starkey", "label": "Starkey" }
        ], "validation": { "required": true } },
        { "name": "model", "type": "text", "label": "form.hearingDevice.model", "validation": { "required": true } },
        { "name": "serialNumber", "type": "text", "label": "form.hearingDevice.serialNumber", "validation": { "required": true } },
        { "name": "side", "type": "select", "label": "form.hearingDevice.side", "options": [
          { "value": "left", "label": "Left" },
          { "value": "right", "label": "Right" },
          { "value": "both", "label": "Both (Binaural)" }
        ], "validation": { "required": true } },
        { "name": "purchaseDate", "type": "date", "label": "form.hearingDevice.purchaseDate" },
        { "name": "warrantyExpiry", "type": "date", "label": "form.hearingDevice.warrantyExpiry" },
        { "name": "price", "type": "currency", "label": "form.price", "currency": "AED" },
        { "name": "notes", "type": "textarea", "label": "form.notes" }
      ]
    }
  },
  "formLayouts": {
    "audiogram": {
      "entityName": "audiogram",
      "sections": [
        {
          "id": "testInfo",
          "title": "section.testInfo",
          "columns": 2,
          "fields": ["patientId", "testDate", "testType", "audiologistId"]
        },
        {
          "id": "results",
          "title": "section.audiogramResults",
          "columns": 2,
          "fields": ["rightEarData", "leftEarData"]
        },
        {
          "id": "assessment",
          "title": "section.assessment",
          "columns": 1,
          "fields": ["interpretation", "recommendations"]
        }
      ]
    },
    "hearingDevice": {
      "entityName": "hearingDevice",
      "sections": [
        {
          "id": "patient",
          "title": "entity.patient.singular",
          "columns": 1,
          "fields": ["patientId"]
        },
        {
          "id": "device",
          "title": "section.deviceDetails",
          "columns": 2,
          "fields": ["manufacturer", "model", "serialNumber", "side"]
        },
        {
          "id": "purchase",
          "title": "section.purchaseInfo",
          "columns": 2,
          "fields": ["purchaseDate", "warrantyExpiry", "price"]
        },
        {
          "id": "notes",
          "title": "section.additional",
          "columns": 1,
          "fields": ["notes"]
        }
      ]
    }
  }
}
```

### 3.4 DENTAL Override Template

```json
{
  "clinicType": "DENTAL",
  "features": [
    "toothChart",
    "treatmentPlan",
    "dentalProcedures",
    "xrays",
    "periodontalCharting"
  ],
  "terminology": {
    "nav.dental": "Dental",
    "nav.toothChart": "Tooth Chart",
    "nav.treatmentPlans": "Treatment Plans",
    "nav.procedures": "Procedures",
    "page.dental.title": "Dental Clinic",
    "entity.tooth.singular": "Tooth",
    "entity.tooth.plural": "Teeth",
    "entity.treatmentPlan.singular": "Treatment Plan",
    "entity.treatmentPlan.plural": "Treatment Plans",
    "entity.procedure.singular": "Procedure",
    "entity.procedure.plural": "Procedures",
    "form.tooth.number": "Tooth Number",
    "form.tooth.condition": "Condition",
    "form.tooth.surface": "Surface",
    "form.procedure.type": "Procedure Type",
    "form.procedure.tooth": "Tooth",
    "form.procedure.surface": "Surface(s)",
    "form.procedure.notes": "Clinical Notes",
    "form.treatmentPlan.priority": "Priority",
    "form.treatmentPlan.estimatedCost": "Estimated Cost",
    "section.toothConditions": "Tooth Conditions",
    "section.plannedTreatments": "Planned Treatments",
    "section.completedProcedures": "Completed Procedures"
  },
  "navigation": [
    {
      "id": "dental",
      "label": "nav.dental",
      "icon": "Smile",
      "route": "/dental",
      "featureCode": "toothChart",
      "sortOrder": 35,
      "children": [
        {
          "id": "toothChart",
          "label": "nav.toothChart",
          "icon": "Grid3x3",
          "route": "/dental/chart",
          "featureCode": "toothChart",
          "sortOrder": 0
        },
        {
          "id": "treatmentPlans",
          "label": "nav.treatmentPlans",
          "icon": "ListChecks",
          "route": "/dental/plans",
          "featureCode": "treatmentPlan",
          "sortOrder": 1
        },
        {
          "id": "procedures",
          "label": "nav.procedures",
          "icon": "Stethoscope",
          "route": "/dental/procedures",
          "featureCode": "dentalProcedures",
          "sortOrder": 2
        }
      ]
    }
  ],
  "uiSchemas": {
    "dentalProcedure": {
      "entityName": "dentalProcedure",
      "displayName": "entity.procedure.singular",
      "displayNamePlural": "entity.procedure.plural",
      "primaryField": "procedureType",
      "fields": [
        { "name": "patientId", "type": "lookup", "label": "entity.patient.singular", "lookupEndpoint": "/api/patients", "validation": { "required": true } },
        { "name": "procedureType", "type": "select", "label": "form.procedure.type", "options": [
          { "value": "filling", "label": "Filling" },
          { "value": "extraction", "label": "Extraction" },
          { "value": "rootCanal", "label": "Root Canal" },
          { "value": "crown", "label": "Crown" },
          { "value": "bridge", "label": "Bridge" },
          { "value": "denture", "label": "Denture" },
          { "value": "implant", "label": "Implant" },
          { "value": "cleaning", "label": "Cleaning/Scaling" },
          { "value": "whitening", "label": "Whitening" },
          { "value": "veneer", "label": "Veneer" },
          { "value": "orthodontics", "label": "Orthodontics" }
        ], "validation": { "required": true } },
        { "name": "toothNumber", "type": "select", "label": "form.tooth.number", "options": [
          { "value": "11", "label": "11 - Upper Right Central Incisor" },
          { "value": "12", "label": "12 - Upper Right Lateral Incisor" },
          { "value": "21", "label": "21 - Upper Left Central Incisor" },
          { "value": "22", "label": "22 - Upper Left Lateral Incisor" }
        ] },
        { "name": "surfaces", "type": "multiselect", "label": "form.procedure.surface", "options": [
          { "value": "M", "label": "Mesial" },
          { "value": "D", "label": "Distal" },
          { "value": "O", "label": "Occlusal" },
          { "value": "B", "label": "Buccal" },
          { "value": "L", "label": "Lingual" }
        ] },
        { "name": "procedureDate", "type": "datetime", "label": "form.date", "validation": { "required": true } },
        { "name": "dentistId", "type": "lookup", "label": "role.doctor", "lookupEndpoint": "/api/employees?role=dentist" },
        { "name": "cost", "type": "currency", "label": "form.cost", "currency": "AED" },
        { "name": "notes", "type": "textarea", "label": "form.procedure.notes" },
        { "name": "status", "type": "select", "label": "form.status", "options": [
          { "value": "planned", "label": "Planned" },
          { "value": "inProgress", "label": "In Progress" },
          { "value": "completed", "label": "Completed" },
          { "value": "cancelled", "label": "Cancelled" }
        ] }
      ]
    },
    "treatmentPlan": {
      "entityName": "treatmentPlan",
      "displayName": "entity.treatmentPlan.singular",
      "displayNamePlural": "entity.treatmentPlan.plural",
      "primaryField": "name",
      "fields": [
        { "name": "patientId", "type": "lookup", "label": "entity.patient.singular", "validation": { "required": true } },
        { "name": "name", "type": "text", "label": "form.name", "validation": { "required": true } },
        { "name": "priority", "type": "select", "label": "form.treatmentPlan.priority", "options": [
          { "value": "urgent", "label": "Urgent" },
          { "value": "high", "label": "High" },
          { "value": "medium", "label": "Medium" },
          { "value": "low", "label": "Low" }
        ] },
        { "name": "estimatedCost", "type": "currency", "label": "form.treatmentPlan.estimatedCost", "currency": "AED" },
        { "name": "procedures", "type": "json", "label": "section.plannedTreatments" },
        { "name": "notes", "type": "textarea", "label": "form.notes" },
        { "name": "status", "type": "select", "label": "form.status", "options": [
          { "value": "draft", "label": "Draft" },
          { "value": "presented", "label": "Presented to Patient" },
          { "value": "accepted", "label": "Accepted" },
          { "value": "inProgress", "label": "In Progress" },
          { "value": "completed", "label": "Completed" }
        ] }
      ]
    }
  }
}
```

### 3.5 VET Override Template

```json
{
  "clinicType": "VET",
  "features": [
    "petManagement",
    "vaccinations",
    "petOwners",
    "grooming",
    "boarding"
  ],
  "terminology": {
    "entity.patient.singular": "Pet",
    "entity.patient.plural": "Pets",
    "entity.owner.singular": "Owner",
    "entity.owner.plural": "Owners",
    "entity.vaccination.singular": "Vaccination",
    "entity.vaccination.plural": "Vaccinations",
    "nav.patients": "Pets",
    "nav.petOwners": "Pet Owners",
    "nav.vaccinations": "Vaccinations",
    "nav.grooming": "Grooming",
    "nav.boarding": "Boarding",
    "page.patients.title": "Pet Management",
    "page.patients.subtitle": "Manage pet records and medical history",
    "page.patients.addNew": "Register Pet",
    "page.patients.empty": "No pets registered",
    "form.patient.firstName": "Pet Name",
    "form.patient.lastName": "Owner Last Name",
    "form.patient.dateOfBirth": "Birth Date",
    "form.patient.gender": "Sex",
    "form.patient.species": "Species",
    "form.patient.breed": "Breed",
    "form.patient.color": "Color/Markings",
    "form.patient.microchip": "Microchip Number",
    "form.patient.weight": "Weight (kg)",
    "form.patient.emiratesId": "Owner Emirates ID",
    "form.vaccination.type": "Vaccine Type",
    "form.vaccination.date": "Vaccination Date",
    "form.vaccination.nextDue": "Next Due Date",
    "form.vaccination.batch": "Batch Number",
    "action.addPatient": "Register Pet",
    "action.editPatient": "Edit Pet",
    "action.viewVaccinations": "View Vaccinations",
    "action.scheduleVaccination": "Schedule Vaccination",
    "section.petInfo": "Pet Information",
    "section.ownerInfo": "Owner Information",
    "section.vaccinationHistory": "Vaccination History",
    "table.patient.name": "Pet Name",
    "table.patient.species": "Species",
    "table.patient.breed": "Breed",
    "table.patient.owner": "Owner"
  },
  "navigation": [
    {
      "id": "pets",
      "label": "nav.patients",
      "icon": "PawPrint",
      "route": "/pets",
      "featureCode": "petManagement",
      "sortOrder": 10
    },
    {
      "id": "petOwners",
      "label": "nav.petOwners",
      "icon": "Users",
      "route": "/owners",
      "featureCode": "petOwners",
      "sortOrder": 15
    },
    {
      "id": "vaccinations",
      "label": "nav.vaccinations",
      "icon": "Syringe",
      "route": "/vaccinations",
      "featureCode": "vaccinations",
      "sortOrder": 25,
      "badge": { "type": "count", "countKey": "dueVaccinations" }
    },
    {
      "id": "grooming",
      "label": "nav.grooming",
      "icon": "Scissors",
      "route": "/grooming",
      "featureCode": "grooming",
      "sortOrder": 45
    },
    {
      "id": "boarding",
      "label": "nav.boarding",
      "icon": "Home",
      "route": "/boarding",
      "featureCode": "boarding",
      "sortOrder": 46
    }
  ],
  "uiSchemas": {
    "patient": {
      "entityName": "patient",
      "displayName": "entity.patient.singular",
      "displayNamePlural": "entity.patient.plural",
      "primaryField": "petName",
      "fields": [
        { "name": "petName", "type": "text", "label": "form.patient.firstName", "validation": { "required": true, "maxLength": 100 } },
        { "name": "species", "type": "select", "label": "form.patient.species", "options": [
          { "value": "dog", "label": "Dog" },
          { "value": "cat", "label": "Cat" },
          { "value": "bird", "label": "Bird" },
          { "value": "rabbit", "label": "Rabbit" },
          { "value": "hamster", "label": "Hamster" },
          { "value": "fish", "label": "Fish" },
          { "value": "reptile", "label": "Reptile" },
          { "value": "other", "label": "Other" }
        ], "validation": { "required": true } },
        { "name": "breed", "type": "text", "label": "form.patient.breed" },
        { "name": "dateOfBirth", "type": "date", "label": "form.patient.dateOfBirth" },
        { "name": "gender", "type": "select", "label": "form.patient.gender", "options": [
          { "value": "male", "label": "Male" },
          { "value": "female", "label": "Female" },
          { "value": "neutered", "label": "Neutered Male" },
          { "value": "spayed", "label": "Spayed Female" }
        ] },
        { "name": "color", "type": "text", "label": "form.patient.color" },
        { "name": "weight", "type": "number", "label": "form.patient.weight", "decimals": 2 },
        { "name": "microchip", "type": "text", "label": "form.patient.microchip" },
        { "name": "ownerId", "type": "lookup", "label": "entity.owner.singular", "lookupEndpoint": "/api/petowners", "validation": { "required": true } },
        { "name": "allergies", "type": "multiselect", "label": "form.patient.allergies" },
        { "name": "medicalHistory", "type": "textarea", "label": "form.patient.medicalHistory" }
      ]
    },
    "petOwner": {
      "entityName": "petOwner",
      "displayName": "entity.owner.singular",
      "displayNamePlural": "entity.owner.plural",
      "primaryField": "fullName",
      "fields": [
        { "name": "firstName", "type": "text", "label": "form.firstName", "validation": { "required": true } },
        { "name": "lastName", "type": "text", "label": "form.lastName", "validation": { "required": true } },
        { "name": "phone", "type": "phone", "label": "form.phone", "validation": { "required": true } },
        { "name": "email", "type": "email", "label": "form.email" },
        { "name": "emiratesId", "type": "emiratesId", "label": "form.patient.emiratesId" },
        { "name": "address", "type": "textarea", "label": "form.address" }
      ]
    },
    "vaccination": {
      "entityName": "vaccination",
      "displayName": "entity.vaccination.singular",
      "displayNamePlural": "entity.vaccination.plural",
      "primaryField": "vaccineType",
      "fields": [
        { "name": "petId", "type": "lookup", "label": "entity.patient.singular", "lookupEndpoint": "/api/pets", "validation": { "required": true } },
        { "name": "vaccineType", "type": "select", "label": "form.vaccination.type", "options": [
          { "value": "rabies", "label": "Rabies" },
          { "value": "distemper", "label": "Distemper" },
          { "value": "parvovirus", "label": "Parvovirus" },
          { "value": "hepatitis", "label": "Hepatitis" },
          { "value": "leptospirosis", "label": "Leptospirosis" },
          { "value": "bordetella", "label": "Bordetella" },
          { "value": "fvrcp", "label": "FVRCP (Cat)" },
          { "value": "felv", "label": "FeLV (Cat)" }
        ], "validation": { "required": true } },
        { "name": "vaccinationDate", "type": "date", "label": "form.vaccination.date", "validation": { "required": true } },
        { "name": "nextDueDate", "type": "date", "label": "form.vaccination.nextDue" },
        { "name": "batchNumber", "type": "text", "label": "form.vaccination.batch" },
        { "name": "veterinarianId", "type": "lookup", "label": "role.doctor", "lookupEndpoint": "/api/employees?role=veterinarian" },
        { "name": "notes", "type": "textarea", "label": "form.notes" }
      ]
    }
  },
  "formLayouts": {
    "patient": {
      "entityName": "patient",
      "sections": [
        {
          "id": "pet",
          "title": "section.petInfo",
          "columns": 2,
          "fields": ["petName", "species", "breed", "dateOfBirth", "gender", "color", "weight", "microchip"]
        },
        {
          "id": "owner",
          "title": "section.ownerInfo",
          "columns": 1,
          "fields": ["ownerId"]
        },
        {
          "id": "medical",
          "title": "section.medicalInfo",
          "columns": 1,
          "collapsible": true,
          "fields": ["allergies", "medicalHistory"]
        }
      ]
    }
  }
}
```

---

## 4. Configuration Precedence

When building the final merged context:

```
1. System Defaults (hardcoded fallbacks)
   ↓ merged with
2. CompanyType Template (CLINIC or TRADING base)
   ↓ merged with
3. ClinicType Template (if companyType=CLINIC: AUDIOLOGY, DENTAL, VET, etc.)
   ↓ merged with
4. Tenant Overrides (TenantTerminology, TenantUISchemas, TenantFeatures, etc.)
```

**Merge Rules:**
- Features: Union of all enabled features, tenant can disable
- Terminology: Deep merge, later values override earlier
- Navigation: Merge by ID, respect sortOrder
- UISchemas: Deep merge by entityName, field-level overrides
- FormLayouts: Replace entire layout if overridden
- ListLayouts: Replace entire layout if overridden

---

## 5. API Endpoints

### GET /api/tenant/context
Returns fully merged tenant context based on current user's tenant/company.

### GET /api/admin/templates
Returns available company type and clinic type templates.

### PUT /api/admin/features
Update tenant feature toggles.

### PUT /api/admin/terminology
Update tenant terminology overrides.

### PUT /api/admin/schemas/{entityName}
Update tenant UI schema for entity.

### PUT /api/admin/layouts/form/{entityName}
Update tenant form layout for entity.

### PUT /api/admin/layouts/list/{entityName}
Update tenant list layout for entity.

### POST /api/admin/demo/load
Load demo dataset for tenant.

### POST /api/admin/demo/reset
Reset demo dataset for tenant.

### DELETE /api/admin/demo
Clear demo data for tenant.
