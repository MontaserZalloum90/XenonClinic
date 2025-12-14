import { useState } from 'react';
import { Code, Lock, Zap, FileJson, ExternalLink, Copy, Check, ChevronDown, ChevronRight, Server, Database, Shield, Clock, AlertCircle } from 'lucide-react';

interface Endpoint {
  method: string;
  path: string;
  description: string;
  parameters?: { name: string; type: string; required: boolean; description: string }[];
  requestBody?: string;
  response?: string;
  permissions?: string[];
}

interface EndpointGroup {
  module: string;
  base: string;
  description: string;
  endpoints: Endpoint[];
}

const endpointGroups: EndpointGroup[] = [
  {
    module: 'Authentication',
    base: '/api/auth',
    description: 'User authentication and session management',
    endpoints: [
      {
        method: 'POST',
        path: '/login',
        description: 'Authenticate user and receive JWT token',
        requestBody: `{
  "username": "user@example.com",
  "password": "string",
  "rememberMe": false
}`,
        response: `{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "abc123...",
    "expiresIn": 3600,
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "fullName": "John Doe",
      "roles": ["PHYSICIAN"]
    }
  }
}`,
      },
      {
        method: 'POST',
        path: '/refresh',
        description: 'Refresh an expired JWT token',
        requestBody: `{ "refreshToken": "abc123..." }`,
        response: `{ "token": "newToken...", "expiresIn": 3600 }`,
      },
      {
        method: 'POST',
        path: '/logout',
        description: 'Invalidate current session',
        permissions: ['authenticated'],
      },
      {
        method: 'POST',
        path: '/forgot-password',
        description: 'Request password reset email',
        requestBody: `{ "email": "user@example.com" }`,
      },
      {
        method: 'POST',
        path: '/reset-password',
        description: 'Reset password using token',
        requestBody: `{ "token": "resetToken", "newPassword": "string" }`,
      },
      {
        method: 'POST',
        path: '/mfa/setup',
        description: 'Initialize MFA setup',
        permissions: ['authenticated'],
      },
      {
        method: 'POST',
        path: '/mfa/verify',
        description: 'Verify MFA code during login',
        requestBody: `{ "code": "123456", "sessionToken": "string" }`,
      },
    ],
  },
  {
    module: 'Patients',
    base: '/api/patients',
    description: 'Patient registration, profile management, and medical records',
    endpoints: [
      {
        method: 'GET',
        path: '/',
        description: 'List patients with pagination and filtering',
        parameters: [
          { name: 'page', type: 'number', required: false, description: 'Page number (default: 1)' },
          { name: 'pageSize', type: 'number', required: false, description: 'Items per page (default: 20, max: 100)' },
          { name: 'search', type: 'string', required: false, description: 'Search by name, MRN, phone, or Emirates ID' },
          { name: 'status', type: 'string', required: false, description: 'Filter by status: active, inactive, deceased' },
          { name: 'sortBy', type: 'string', required: false, description: 'Sort field: name, createdAt, lastVisit' },
          { name: 'sortOrder', type: 'string', required: false, description: 'asc or desc' },
        ],
        permissions: ['patients:view'],
        response: `{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "mrn": "MRN-001234",
        "firstName": "Ahmed",
        "lastName": "Hassan",
        "dateOfBirth": "1985-03-15",
        "gender": "male",
        "phone": "+971501234567",
        "email": "ahmed@email.com",
        "emiratesId": "784-1985-1234567-1",
        "status": "active",
        "lastVisit": "2024-12-10T10:30:00Z"
      }
    ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}`,
      },
      {
        method: 'GET',
        path: '/:id',
        description: 'Get patient details by ID',
        permissions: ['patients:view'],
      },
      {
        method: 'POST',
        path: '/',
        description: 'Register a new patient',
        permissions: ['patients:create'],
        requestBody: `{
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "dateOfBirth": "1985-03-15",
  "gender": "male",
  "phone": "+971501234567",
  "email": "ahmed@email.com",
  "emiratesId": "784-1985-1234567-1",
  "address": {
    "line1": "123 Main St",
    "city": "Dubai",
    "emirate": "Dubai",
    "country": "UAE"
  },
  "emergencyContact": {
    "name": "Sara Hassan",
    "relationship": "spouse",
    "phone": "+971507654321"
  }
}`,
      },
      {
        method: 'PUT',
        path: '/:id',
        description: 'Update patient information',
        permissions: ['patients:update'],
      },
      {
        method: 'DELETE',
        path: '/:id',
        description: 'Soft delete a patient record',
        permissions: ['patients:delete'],
      },
      {
        method: 'GET',
        path: '/:id/medical-history',
        description: 'Get patient medical history (allergies, conditions, surgeries)',
        permissions: ['patients:view', 'phi:access'],
      },
      {
        method: 'POST',
        path: '/:id/medical-history',
        description: 'Add medical history entry',
        permissions: ['patients:update', 'phi:access'],
      },
      {
        method: 'GET',
        path: '/:id/documents',
        description: 'List patient documents',
        permissions: ['patients:view'],
      },
      {
        method: 'POST',
        path: '/:id/documents',
        description: 'Upload patient document',
        permissions: ['patients:update'],
      },
      {
        method: 'GET',
        path: '/:id/insurance',
        description: 'Get patient insurance information',
        permissions: ['patients:view', 'insurance:view'],
      },
      {
        method: 'POST',
        path: '/merge',
        description: 'Merge duplicate patient records',
        permissions: ['patients:merge', 'admin'],
      },
    ],
  },
  {
    module: 'Appointments',
    base: '/api/appointments',
    description: 'Appointment scheduling, management, and calendar operations',
    endpoints: [
      {
        method: 'GET',
        path: '/',
        description: 'List appointments with filters',
        parameters: [
          { name: 'startDate', type: 'date', required: true, description: 'Start of date range (ISO 8601)' },
          { name: 'endDate', type: 'date', required: true, description: 'End of date range (ISO 8601)' },
          { name: 'providerId', type: 'uuid', required: false, description: 'Filter by provider' },
          { name: 'patientId', type: 'uuid', required: false, description: 'Filter by patient' },
          { name: 'status', type: 'string', required: false, description: 'scheduled, checked-in, completed, cancelled, no-show' },
          { name: 'departmentId', type: 'uuid', required: false, description: 'Filter by department' },
        ],
        permissions: ['appointments:view'],
      },
      {
        method: 'GET',
        path: '/:id',
        description: 'Get appointment details',
        permissions: ['appointments:view'],
      },
      {
        method: 'POST',
        path: '/',
        description: 'Create a new appointment',
        permissions: ['appointments:create'],
        requestBody: `{
  "patientId": "uuid",
  "providerId": "uuid",
  "appointmentType": "consultation",
  "dateTime": "2024-12-20T10:00:00Z",
  "duration": 30,
  "departmentId": "uuid",
  "reason": "Follow-up visit",
  "notes": "Patient requested morning slot"
}`,
      },
      {
        method: 'PUT',
        path: '/:id',
        description: 'Update appointment details',
        permissions: ['appointments:update'],
      },
      {
        method: 'POST',
        path: '/:id/reschedule',
        description: 'Reschedule an appointment',
        permissions: ['appointments:reschedule'],
        requestBody: `{
  "newDateTime": "2024-12-21T14:00:00Z",
  "reason": "Patient request"
}`,
      },
      {
        method: 'POST',
        path: '/:id/cancel',
        description: 'Cancel an appointment',
        permissions: ['appointments:delete'],
        requestBody: `{
  "reason": "Patient cancelled",
  "notifyPatient": true
}`,
      },
      {
        method: 'POST',
        path: '/:id/check-in',
        description: 'Check in a patient for their appointment',
        permissions: ['appointments:update'],
      },
      {
        method: 'POST',
        path: '/:id/no-show',
        description: 'Mark appointment as no-show',
        permissions: ['appointments:update'],
      },
      {
        method: 'GET',
        path: '/availability',
        description: 'Get available time slots',
        parameters: [
          { name: 'providerId', type: 'uuid', required: true, description: 'Provider to check' },
          { name: 'date', type: 'date', required: true, description: 'Date to check' },
          { name: 'duration', type: 'number', required: false, description: 'Appointment duration in minutes' },
        ],
        permissions: ['appointments:view'],
      },
      {
        method: 'GET',
        path: '/waitlist',
        description: 'Get waitlist for earlier appointments',
        permissions: ['appointments:view'],
      },
      {
        method: 'POST',
        path: '/waitlist',
        description: 'Add patient to waitlist',
        permissions: ['appointments:create'],
      },
    ],
  },
  {
    module: 'Clinical Visits',
    base: '/api/clinical-visits',
    description: 'Clinical encounter documentation, vitals, diagnoses, and prescriptions',
    endpoints: [
      {
        method: 'GET',
        path: '/',
        description: 'List clinical visits',
        permissions: ['clinical_visits:view'],
      },
      {
        method: 'GET',
        path: '/:id',
        description: 'Get visit details including documentation',
        permissions: ['clinical_visits:view'],
      },
      {
        method: 'POST',
        path: '/',
        description: 'Create a new clinical visit',
        permissions: ['clinical_visits:create'],
        requestBody: `{
  "patientId": "uuid",
  "appointmentId": "uuid",
  "providerId": "uuid",
  "visitType": "consultation",
  "chiefComplaint": "Fever and headache for 3 days"
}`,
      },
      {
        method: 'PUT',
        path: '/:id/vitals',
        description: 'Record patient vital signs',
        permissions: ['clinical_visits:update'],
        requestBody: `{
  "bloodPressureSystolic": 120,
  "bloodPressureDiastolic": 80,
  "heartRate": 72,
  "temperature": 37.2,
  "respiratoryRate": 16,
  "oxygenSaturation": 98,
  "weight": 75.5,
  "height": 175
}`,
      },
      {
        method: 'PUT',
        path: '/:id/examination',
        description: 'Document physical examination findings',
        permissions: ['clinical_visits:update'],
      },
      {
        method: 'POST',
        path: '/:id/diagnoses',
        description: 'Add diagnosis to visit',
        permissions: ['clinical_visits:update'],
        requestBody: `{
  "icd10Code": "J06.9",
  "description": "Acute upper respiratory infection, unspecified",
  "type": "primary",
  "notes": "Viral etiology suspected"
}`,
      },
      {
        method: 'POST',
        path: '/:id/prescriptions',
        description: 'Create prescription',
        permissions: ['prescriptions:create'],
        requestBody: `{
  "medications": [
    {
      "drugId": "uuid",
      "drugName": "Paracetamol 500mg",
      "dosage": "1 tablet",
      "frequency": "every 6 hours",
      "duration": "5 days",
      "quantity": 20,
      "instructions": "Take with food"
    }
  ]
}`,
      },
      {
        method: 'POST',
        path: '/:id/sign',
        description: 'Sign and complete the visit',
        permissions: ['clinical_visits:update'],
      },
      {
        method: 'GET',
        path: '/:id/summary',
        description: 'Generate after-visit summary PDF',
        permissions: ['clinical_visits:view'],
      },
    ],
  },
  {
    module: 'Laboratory',
    base: '/api/laboratory',
    description: 'Lab order management, specimen tracking, and result reporting',
    endpoints: [
      {
        method: 'GET',
        path: '/orders',
        description: 'List lab orders',
        permissions: ['lab:view'],
      },
      {
        method: 'POST',
        path: '/orders',
        description: 'Create lab order',
        permissions: ['lab:create'],
        requestBody: `{
  "patientId": "uuid",
  "visitId": "uuid",
  "orderingProviderId": "uuid",
  "priority": "routine",
  "tests": [
    { "testId": "uuid", "testCode": "CBC" },
    { "testId": "uuid", "testCode": "BMP" }
  ],
  "clinicalIndication": "Annual checkup",
  "fastingRequired": true
}`,
      },
      {
        method: 'GET',
        path: '/orders/:id',
        description: 'Get lab order details',
        permissions: ['lab:view'],
      },
      {
        method: 'POST',
        path: '/specimens',
        description: 'Register specimen collection',
        permissions: ['lab:create'],
        requestBody: `{
  "orderId": "uuid",
  "specimenType": "blood",
  "collectedAt": "2024-12-14T10:30:00Z",
  "collectedBy": "uuid",
  "barcode": "LAB-2024-001234"
}`,
      },
      {
        method: 'POST',
        path: '/results',
        description: 'Enter lab results',
        permissions: ['lab:update'],
        requestBody: `{
  "orderId": "uuid",
  "results": [
    {
      "testCode": "WBC",
      "value": 7.5,
      "unit": "10^9/L",
      "referenceRange": "4.0-11.0",
      "flag": "normal"
    }
  ]
}`,
      },
      {
        method: 'POST',
        path: '/results/:id/approve',
        description: 'Approve/validate lab results',
        permissions: ['lab:approve'],
      },
      {
        method: 'GET',
        path: '/results/:id/report',
        description: 'Generate lab report PDF',
        permissions: ['lab:view'],
      },
      {
        method: 'GET',
        path: '/queue',
        description: 'Get lab processing queue',
        permissions: ['lab:view'],
      },
      {
        method: 'GET',
        path: '/tests',
        description: 'List available lab tests',
        permissions: ['lab:view'],
      },
    ],
  },
  {
    module: 'Radiology',
    base: '/api/radiology',
    description: 'Imaging orders, PACS integration, and radiology reporting',
    endpoints: [
      {
        method: 'GET',
        path: '/orders',
        description: 'List radiology orders',
        permissions: ['radiology:view'],
      },
      {
        method: 'POST',
        path: '/orders',
        description: 'Create imaging order',
        permissions: ['radiology:create'],
        requestBody: `{
  "patientId": "uuid",
  "orderingProviderId": "uuid",
  "modality": "X-RAY",
  "bodyPart": "chest",
  "clinicalIndication": "Suspected pneumonia",
  "priority": "routine"
}`,
      },
      {
        method: 'GET',
        path: '/studies/:id',
        description: 'Get study details and images',
        permissions: ['radiology:view'],
      },
      {
        method: 'POST',
        path: '/studies/:id/report',
        description: 'Create radiology report',
        permissions: ['radiology:update'],
      },
      {
        method: 'GET',
        path: '/worklist',
        description: 'Get radiologist worklist',
        permissions: ['radiology:view'],
      },
    ],
  },
  {
    module: 'Pharmacy',
    base: '/api/pharmacy',
    description: 'Prescription management, medication dispensing, and inventory',
    endpoints: [
      {
        method: 'GET',
        path: '/prescriptions',
        description: 'List prescriptions pending dispensing',
        permissions: ['pharmacy:view'],
      },
      {
        method: 'GET',
        path: '/prescriptions/:id',
        description: 'Get prescription details',
        permissions: ['pharmacy:view'],
      },
      {
        method: 'POST',
        path: '/prescriptions/:id/verify',
        description: 'Pharmacist verification of prescription',
        permissions: ['pharmacy:dispense'],
      },
      {
        method: 'POST',
        path: '/prescriptions/:id/dispense',
        description: 'Dispense medication',
        permissions: ['pharmacy:dispense'],
        requestBody: `{
  "dispensedItems": [
    {
      "medicationId": "uuid",
      "quantityDispensed": 20,
      "lotNumber": "LOT123",
      "expiryDate": "2025-12-31"
    }
  ],
  "patientCounseled": true,
  "counselingNotes": "Discussed side effects and interactions"
}`,
      },
      {
        method: 'GET',
        path: '/drugs',
        description: 'Search drug database',
        permissions: ['pharmacy:view'],
      },
      {
        method: 'GET',
        path: '/drugs/:id/interactions',
        description: 'Check drug interactions',
        permissions: ['pharmacy:view'],
      },
      {
        method: 'GET',
        path: '/inventory',
        description: 'Get pharmacy inventory',
        permissions: ['pharmacy:manage'],
      },
    ],
  },
  {
    module: 'Financial',
    base: '/api/financial',
    description: 'Billing, invoicing, payments, and insurance claims',
    endpoints: [
      {
        method: 'GET',
        path: '/invoices',
        description: 'List invoices',
        permissions: ['financial:view'],
      },
      {
        method: 'POST',
        path: '/invoices',
        description: 'Create invoice',
        permissions: ['financial:create'],
        requestBody: `{
  "patientId": "uuid",
  "visitId": "uuid",
  "items": [
    {
      "serviceCode": "CONS001",
      "description": "Consultation - General",
      "quantity": 1,
      "unitPrice": 300.00
    }
  ],
  "discountPercent": 0,
  "taxPercent": 5
}`,
      },
      {
        method: 'POST',
        path: '/payments',
        description: 'Record payment',
        permissions: ['financial:create'],
        requestBody: `{
  "invoiceId": "uuid",
  "amount": 315.00,
  "paymentMethod": "card",
  "referenceNumber": "TXN123456"
}`,
      },
      {
        method: 'POST',
        path: '/payments/:id/refund',
        description: 'Process refund',
        permissions: ['financial:approve'],
      },
      {
        method: 'GET',
        path: '/insurance-claims',
        description: 'List insurance claims',
        permissions: ['financial:view', 'insurance:view'],
      },
      {
        method: 'POST',
        path: '/insurance-claims',
        description: 'Submit insurance claim',
        permissions: ['financial:create', 'insurance:create'],
      },
      {
        method: 'GET',
        path: '/reports/revenue',
        description: 'Generate revenue report',
        permissions: ['financial:reports'],
      },
      {
        method: 'GET',
        path: '/reports/aging',
        description: 'Get accounts receivable aging',
        permissions: ['financial:reports'],
      },
    ],
  },
  {
    module: 'HR Management',
    base: '/api/hr',
    description: 'Employee management, attendance, and leave tracking',
    endpoints: [
      {
        method: 'GET',
        path: '/employees',
        description: 'List employees',
        permissions: ['hr:view'],
      },
      {
        method: 'POST',
        path: '/employees',
        description: 'Create employee record',
        permissions: ['hr:create'],
      },
      {
        method: 'GET',
        path: '/employees/:id',
        description: 'Get employee details',
        permissions: ['hr:view'],
      },
      {
        method: 'PUT',
        path: '/employees/:id',
        description: 'Update employee information',
        permissions: ['hr:update'],
      },
      {
        method: 'GET',
        path: '/attendance',
        description: 'Get attendance records',
        permissions: ['hr:view'],
      },
      {
        method: 'POST',
        path: '/attendance/clock-in',
        description: 'Clock in employee',
        permissions: ['authenticated'],
      },
      {
        method: 'POST',
        path: '/attendance/clock-out',
        description: 'Clock out employee',
        permissions: ['authenticated'],
      },
      {
        method: 'GET',
        path: '/leave-requests',
        description: 'List leave requests',
        permissions: ['hr:view'],
      },
      {
        method: 'POST',
        path: '/leave-requests',
        description: 'Submit leave request',
        permissions: ['authenticated'],
      },
      {
        method: 'PUT',
        path: '/leave-requests/:id/approve',
        description: 'Approve/reject leave request',
        permissions: ['hr:approve'],
      },
    ],
  },
  {
    module: 'Inventory',
    base: '/api/inventory',
    description: 'Stock management, purchase orders, and supplier management',
    endpoints: [
      {
        method: 'GET',
        path: '/items',
        description: 'List inventory items',
        permissions: ['inventory:view'],
      },
      {
        method: 'POST',
        path: '/items',
        description: 'Add inventory item',
        permissions: ['inventory:create'],
      },
      {
        method: 'PUT',
        path: '/items/:id',
        description: 'Update inventory item',
        permissions: ['inventory:update'],
      },
      {
        method: 'POST',
        path: '/stock-adjustments',
        description: 'Record stock adjustment',
        permissions: ['inventory:update'],
      },
      {
        method: 'GET',
        path: '/purchase-orders',
        description: 'List purchase orders',
        permissions: ['inventory:view'],
      },
      {
        method: 'POST',
        path: '/purchase-orders',
        description: 'Create purchase order',
        permissions: ['inventory:create'],
      },
      {
        method: 'GET',
        path: '/suppliers',
        description: 'List suppliers',
        permissions: ['inventory:view'],
      },
      {
        method: 'GET',
        path: '/reports/low-stock',
        description: 'Get low stock alerts',
        permissions: ['inventory:view'],
      },
    ],
  },
  {
    module: 'Analytics',
    base: '/api/analytics',
    description: 'Reporting, dashboards, and business intelligence',
    endpoints: [
      {
        method: 'GET',
        path: '/dashboard',
        description: 'Get dashboard metrics',
        permissions: ['analytics:view'],
      },
      {
        method: 'GET',
        path: '/reports/patient-volume',
        description: 'Patient volume report',
        permissions: ['analytics:view'],
      },
      {
        method: 'GET',
        path: '/reports/provider-productivity',
        description: 'Provider productivity report',
        permissions: ['analytics:view'],
      },
      {
        method: 'GET',
        path: '/reports/revenue-analysis',
        description: 'Revenue analysis report',
        permissions: ['analytics:view', 'financial:reports'],
      },
      {
        method: 'POST',
        path: '/reports/custom',
        description: 'Generate custom report',
        permissions: ['analytics:create'],
      },
      {
        method: 'GET',
        path: '/exports/:id',
        description: 'Download exported report',
        permissions: ['analytics:view'],
      },
    ],
  },
  {
    module: 'Security & Admin',
    base: '/api/admin',
    description: 'User management, roles, permissions, and system configuration',
    endpoints: [
      {
        method: 'GET',
        path: '/users',
        description: 'List system users',
        permissions: ['admin:users:view'],
      },
      {
        method: 'POST',
        path: '/users',
        description: 'Create user account',
        permissions: ['admin:users:create'],
      },
      {
        method: 'PUT',
        path: '/users/:id',
        description: 'Update user',
        permissions: ['admin:users:update'],
      },
      {
        method: 'PUT',
        path: '/users/:id/roles',
        description: 'Assign roles to user',
        permissions: ['admin:users:update'],
      },
      {
        method: 'POST',
        path: '/users/:id/unlock',
        description: 'Unlock locked user account',
        permissions: ['admin:users:update'],
      },
      {
        method: 'GET',
        path: '/roles',
        description: 'List available roles',
        permissions: ['admin:roles:view'],
      },
      {
        method: 'GET',
        path: '/audit-logs',
        description: 'Get system audit logs',
        permissions: ['admin:audit:view'],
      },
      {
        method: 'GET',
        path: '/settings',
        description: 'Get system settings',
        permissions: ['admin:settings:view'],
      },
      {
        method: 'PUT',
        path: '/settings',
        description: 'Update system settings',
        permissions: ['admin:settings:update'],
      },
    ],
  },
  {
    module: 'Workflows',
    base: '/api/workflows',
    description: 'Automated workflow and business process management',
    endpoints: [
      {
        method: 'GET',
        path: '/definitions',
        description: 'List workflow definitions',
        permissions: ['workflows:view'],
      },
      {
        method: 'POST',
        path: '/definitions',
        description: 'Create workflow definition',
        permissions: ['workflows:create'],
      },
      {
        method: 'GET',
        path: '/instances',
        description: 'List active workflow instances',
        permissions: ['workflows:view'],
      },
      {
        method: 'POST',
        path: '/instances/:id/trigger',
        description: 'Trigger workflow step',
        permissions: ['workflows:execute'],
      },
      {
        method: 'GET',
        path: '/tasks',
        description: 'Get pending workflow tasks',
        permissions: ['workflows:view'],
      },
      {
        method: 'POST',
        path: '/tasks/:id/complete',
        description: 'Complete workflow task',
        permissions: ['workflows:execute'],
      },
    ],
  },
];

const methodColors: Record<string, string> = {
  GET: 'bg-green-100 text-green-700',
  POST: 'bg-blue-100 text-blue-700',
  PUT: 'bg-yellow-100 text-yellow-700',
  DELETE: 'bg-red-100 text-red-700',
  PATCH: 'bg-purple-100 text-purple-700',
};

function CodeBlock({ code, language = 'json' }: { code: string; language?: string }) {
  const [copied, setCopied] = useState(false);

  const copyToClipboard = () => {
    navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="relative bg-gray-900 text-gray-100 rounded-lg overflow-hidden">
      <button
        onClick={copyToClipboard}
        className="absolute top-2 right-2 p-1.5 rounded bg-gray-700 hover:bg-gray-600 transition-colors"
        title="Copy to clipboard"
      >
        {copied ? <Check className="h-4 w-4 text-green-400" /> : <Copy className="h-4 w-4" />}
      </button>
      <pre className="p-4 text-sm overflow-x-auto">
        <code>{code}</code>
      </pre>
    </div>
  );
}

function EndpointCard({ endpoint, basePath }: { endpoint: Endpoint; basePath: string }) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full px-4 py-3 flex items-center gap-3 hover:bg-gray-50 transition-colors text-left"
      >
        <span className={`px-2 py-0.5 rounded text-xs font-medium ${methodColors[endpoint.method]}`}>
          {endpoint.method}
        </span>
        <code className="text-sm text-gray-700 flex-1">{basePath}{endpoint.path}</code>
        {expanded ? <ChevronDown className="h-4 w-4 text-gray-400" /> : <ChevronRight className="h-4 w-4 text-gray-400" />}
      </button>
      {expanded && (
        <div className="px-4 py-3 border-t border-gray-200 bg-gray-50 space-y-4">
          <p className="text-gray-600">{endpoint.description}</p>

          {endpoint.permissions && endpoint.permissions.length > 0 && (
            <div>
              <h5 className="text-sm font-medium text-gray-900 mb-1">Required Permissions</h5>
              <div className="flex flex-wrap gap-1">
                {endpoint.permissions.map((perm) => (
                  <code key={perm} className="bg-purple-100 text-purple-700 px-2 py-0.5 rounded text-xs">
                    {perm}
                  </code>
                ))}
              </div>
            </div>
          )}

          {endpoint.parameters && endpoint.parameters.length > 0 && (
            <div>
              <h5 className="text-sm font-medium text-gray-900 mb-2">Parameters</h5>
              <div className="bg-white rounded border border-gray-200 overflow-hidden">
                <table className="min-w-full text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Name</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Type</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Required</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Description</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {endpoint.parameters.map((param) => (
                      <tr key={param.name}>
                        <td className="px-3 py-2"><code className="text-xs">{param.name}</code></td>
                        <td className="px-3 py-2 text-gray-500">{param.type}</td>
                        <td className="px-3 py-2">
                          {param.required ? (
                            <span className="text-red-600 text-xs">required</span>
                          ) : (
                            <span className="text-gray-400 text-xs">optional</span>
                          )}
                        </td>
                        <td className="px-3 py-2 text-gray-600">{param.description}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {endpoint.requestBody && (
            <div>
              <h5 className="text-sm font-medium text-gray-900 mb-2">Request Body</h5>
              <CodeBlock code={endpoint.requestBody} />
            </div>
          )}

          {endpoint.response && (
            <div>
              <h5 className="text-sm font-medium text-gray-900 mb-2">Response</h5>
              <CodeBlock code={endpoint.response} />
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default function APIReference() {
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(new Set(['Authentication']));

  const toggleGroup = (module: string) => {
    const newExpanded = new Set(expandedGroups);
    if (newExpanded.has(module)) {
      newExpanded.delete(module);
    } else {
      newExpanded.add(module);
    }
    setExpandedGroups(newExpanded);
  };

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">API Reference</h1>
        <p className="text-lg text-gray-600">
          XenonClinic provides a comprehensive REST API for integration with external
          systems. This documentation covers all available endpoints, authentication,
          and response formats.
        </p>
      </div>

      {/* Quick Info Cards */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Code className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">REST API</h3>
          <p className="text-sm text-gray-600 mt-1">
            Standard REST endpoints with JSON payloads
          </p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Lock className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">JWT Authentication</h3>
          <p className="text-sm text-gray-600 mt-1">
            Bearer token authentication required
          </p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Zap className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Rate Limited</h3>
          <p className="text-sm text-gray-600 mt-1">
            100 requests/minute per API key
          </p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Server className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Versioned</h3>
          <p className="text-sm text-gray-600 mt-1">
            API version included in base URL
          </p>
        </div>
      </div>

      {/* Base URL */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Base URL</h2>
        <CodeBlock code={`# Production
https://api.yourcompany.xenonclinic.com/v1

# Sandbox (for development)
https://sandbox-api.yourcompany.xenonclinic.com/v1`} />
        <p className="text-sm text-gray-500 mt-2">
          Replace <code className="bg-gray-100 px-1 rounded">yourcompany</code> with your organization's subdomain.
        </p>
      </section>

      {/* Authentication */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Authentication</h2>
        <div className="space-y-4">
          <p className="text-gray-600">
            All API requests require a valid JWT token in the Authorization header. Tokens are obtained
            by authenticating with user credentials.
          </p>
          <CodeBlock code={`# 1. Obtain Token
POST /api/auth/login
Content-Type: application/json

{
  "username": "user@example.com",
  "password": "your-password"
}

# Response
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresIn": 3600,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "fullName": "John Doe",
      "roles": ["PHYSICIAN"]
    }
  }
}

# 2. Use Token in Subsequent Requests
GET /api/patients
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

# 3. Refresh Token Before Expiry
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}`} />
        </div>
      </section>

      {/* Response Format */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Response Format</h2>
        <p className="text-gray-600 mb-4">
          All API responses follow a consistent JSON structure for easy parsing and error handling.
        </p>
        <div className="grid md:grid-cols-2 gap-4">
          <div>
            <h4 className="font-medium text-gray-900 mb-2">Success Response</h4>
            <CodeBlock code={`{
  "success": true,
  "data": { ... },
  "message": null,
  "timestamp": "2024-12-14T10:30:00Z",
  "traceId": "abc-123-def-456"
}`} />
          </div>
          <div>
            <h4 className="font-medium text-gray-900 mb-2">Error Response</h4>
            <CodeBlock code={`{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {
      "email": ["Invalid email format"],
      "phone": ["Phone number required"]
    }
  },
  "timestamp": "2024-12-14T10:30:00Z",
  "traceId": "abc-123-def-456"
}`} />
          </div>
        </div>
        <div className="mt-4">
          <h4 className="font-medium text-gray-900 mb-2">Paginated Response</h4>
          <CodeBlock code={`{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}`} />
        </div>
      </section>

      {/* HTTP Status Codes */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">HTTP Status Codes</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              <tr><td className="px-4 py-3 font-mono text-green-600">200</td><td className="px-4 py-3 font-medium">OK</td><td className="px-4 py-3 text-gray-600">Request successful</td></tr>
              <tr><td className="px-4 py-3 font-mono text-green-600">201</td><td className="px-4 py-3 font-medium">Created</td><td className="px-4 py-3 text-gray-600">Resource created successfully</td></tr>
              <tr><td className="px-4 py-3 font-mono text-green-600">204</td><td className="px-4 py-3 font-medium">No Content</td><td className="px-4 py-3 text-gray-600">Request successful, no content returned (DELETE)</td></tr>
              <tr><td className="px-4 py-3 font-mono text-yellow-600">400</td><td className="px-4 py-3 font-medium">Bad Request</td><td className="px-4 py-3 text-gray-600">Invalid request parameters or body</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">401</td><td className="px-4 py-3 font-medium">Unauthorized</td><td className="px-4 py-3 text-gray-600">Missing or invalid authentication token</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">403</td><td className="px-4 py-3 font-medium">Forbidden</td><td className="px-4 py-3 text-gray-600">Insufficient permissions for this action</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">404</td><td className="px-4 py-3 font-medium">Not Found</td><td className="px-4 py-3 text-gray-600">Requested resource does not exist</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">409</td><td className="px-4 py-3 font-medium">Conflict</td><td className="px-4 py-3 text-gray-600">Resource conflict (duplicate, version mismatch)</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">422</td><td className="px-4 py-3 font-medium">Unprocessable</td><td className="px-4 py-3 text-gray-600">Business logic validation failed</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">429</td><td className="px-4 py-3 font-medium">Too Many Requests</td><td className="px-4 py-3 text-gray-600">Rate limit exceeded, retry after delay</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">500</td><td className="px-4 py-3 font-medium">Server Error</td><td className="px-4 py-3 text-gray-600">Unexpected server error, contact support</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">503</td><td className="px-4 py-3 font-medium">Service Unavailable</td><td className="px-4 py-3 text-gray-600">System maintenance or overload</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      {/* Rate Limiting */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Rate Limiting</h2>
        <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-5">
          <div className="flex items-start gap-3">
            <Clock className="h-6 w-6 text-yellow-600 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-gray-900">Rate Limit Headers</h3>
              <p className="text-gray-600 mt-1">
                API responses include rate limit information in the headers:
              </p>
              <ul className="mt-2 space-y-1 text-sm text-gray-600">
                <li><code className="bg-white px-1 rounded">X-RateLimit-Limit</code>: Maximum requests per minute</li>
                <li><code className="bg-white px-1 rounded">X-RateLimit-Remaining</code>: Requests remaining in window</li>
                <li><code className="bg-white px-1 rounded">X-RateLimit-Reset</code>: Unix timestamp when limit resets</li>
              </ul>
              <p className="text-sm text-gray-500 mt-2">
                Default limit: 100 requests/minute. Contact support for higher limits.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* API Endpoints */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">API Endpoints</h2>
        <p className="text-gray-600 mb-6">
          Click on any endpoint group to expand and view detailed documentation for each endpoint.
          Total endpoints: <strong>{endpointGroups.reduce((acc, g) => acc + g.endpoints.length, 0)}+</strong>
        </p>

        <div className="space-y-4">
          {endpointGroups.map((group) => (
            <div key={group.module} className="bg-white border border-gray-200 rounded-xl overflow-hidden">
              <button
                onClick={() => toggleGroup(group.module)}
                className="w-full px-5 py-4 flex items-center justify-between hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <Database className="h-5 w-5 text-primary-600" />
                  <div className="text-left">
                    <h3 className="font-semibold text-gray-900">{group.module}</h3>
                    <p className="text-sm text-gray-500">{group.description}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-sm text-gray-400">{group.endpoints.length} endpoints</span>
                  <code className="bg-gray-100 px-2 py-0.5 rounded text-sm">{group.base}</code>
                  {expandedGroups.has(group.module) ? (
                    <ChevronDown className="h-5 w-5 text-gray-400" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-gray-400" />
                  )}
                </div>
              </button>
              {expandedGroups.has(group.module) && (
                <div className="px-5 pb-5 space-y-2">
                  {group.endpoints.map((endpoint, idx) => (
                    <EndpointCard key={idx} endpoint={endpoint} basePath={group.base} />
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      </section>

      {/* Webhooks */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Webhooks</h2>
        <p className="text-gray-600 mb-4">
          XenonClinic can send real-time notifications to your systems when events occur.
          Configure webhook endpoints in Admin Settings.
        </p>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h4 className="font-medium text-gray-900 mb-3">Available Events</h4>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-2">
            {[
              'patient.created', 'patient.updated',
              'appointment.created', 'appointment.cancelled', 'appointment.checked_in',
              'visit.completed', 'visit.signed',
              'lab.results_ready', 'lab.critical_value',
              'prescription.created', 'prescription.dispensed',
              'invoice.created', 'payment.received',
              'claim.submitted', 'claim.adjudicated',
            ].map((event) => (
              <code key={event} className="bg-gray-100 px-2 py-1 rounded text-sm text-gray-700">
                {event}
              </code>
            ))}
          </div>
        </div>
        <div className="mt-4">
          <h4 className="font-medium text-gray-900 mb-2">Webhook Payload Example</h4>
          <CodeBlock code={`{
  "event": "appointment.created",
  "timestamp": "2024-12-14T10:30:00Z",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "patientId": "uuid",
    "providerId": "uuid",
    "dateTime": "2024-12-20T10:00:00Z",
    "status": "scheduled"
  },
  "signature": "sha256=..."
}`} />
        </div>
      </section>

      {/* SDKs */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">SDKs & Libraries</h2>
        <div className="grid sm:grid-cols-3 gap-4">
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <h4 className="font-medium text-gray-900">JavaScript/TypeScript</h4>
            <code className="text-sm text-gray-600 block mt-2">npm install @xenonclinic/sdk</code>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <h4 className="font-medium text-gray-900">Python</h4>
            <code className="text-sm text-gray-600 block mt-2">pip install xenonclinic</code>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <h4 className="font-medium text-gray-900">C# / .NET</h4>
            <code className="text-sm text-gray-600 block mt-2">dotnet add package XenonClinic.SDK</code>
          </div>
        </div>
      </section>

      {/* Swagger Link */}
      <section className="bg-primary-50 border border-primary-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <FileJson className="h-8 w-8 text-primary-600 flex-shrink-0" />
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Interactive API Documentation</h3>
            <p className="text-gray-600 mt-1">
              Access the full Swagger/OpenAPI documentation for testing endpoints directly in your browser.
              The interactive documentation includes request builders and live API testing.
            </p>
            <div className="flex flex-wrap gap-3 mt-4">
              <a
                href="https://api.yourcompany.xenonclinic.com/swagger"
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg font-medium hover:bg-primary-700 transition-colors"
              >
                Open Swagger UI <ExternalLink className="h-4 w-4" />
              </a>
              <a
                href="https://api.yourcompany.xenonclinic.com/swagger/v1/swagger.json"
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg font-medium hover:bg-gray-50 transition-colors"
              >
                Download OpenAPI Spec <ExternalLink className="h-4 w-4" />
              </a>
            </div>
            <p className="text-sm text-gray-500 mt-3">
              Replace <code className="bg-white px-1 rounded">yourcompany</code> with your organization's subdomain.
            </p>
          </div>
        </div>
      </section>

      {/* Support */}
      <section className="bg-gray-50 border border-gray-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <AlertCircle className="h-6 w-6 text-gray-600 flex-shrink-0" />
          <div>
            <h3 className="font-semibold text-gray-900">Need Help?</h3>
            <p className="text-gray-600 mt-1">
              For API support, integration assistance, or to report issues:
            </p>
            <ul className="mt-2 space-y-1 text-sm text-gray-600">
              <li>Email: <a href="mailto:api-support@xenonclinic.com" className="text-primary-600 hover:underline">api-support@xenonclinic.com</a></li>
              <li>Developer Portal: <a href="https://developers.xenonclinic.com" className="text-primary-600 hover:underline">developers.xenonclinic.com</a></li>
              <li>Status Page: <a href="https://status.xenonclinic.com" className="text-primary-600 hover:underline">status.xenonclinic.com</a></li>
            </ul>
          </div>
        </div>
      </section>
    </div>
  );
}
