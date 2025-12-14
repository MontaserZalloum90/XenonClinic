// Documentation data and types for the Enterprise Documentation Portal

export interface DocSection {
  id: string;
  title: string;
  path: string;
  icon?: string;
  children?: DocSection[];
}

export interface Module {
  id: string;
  name: string;
  category: string;
  description: string;
  route: string;
  icon: string;
  businessValue: string;
  personas: string[];
  features: Feature[];
  permissions: string[];
  journeyCount: number;
}

export interface Feature {
  id: string;
  name: string;
  description: string;
}

export interface Persona {
  id: string;
  name: string;
  category: string;
  description: string;
  responsibilities: string[];
  accessScope: {
    level: string;
    dataAccess: string;
    roleType: string;
  };
  permissions: string[];
  primaryModules: string[];
  coreJourneys: string[];
  entryPoints: string[];
  commonTasks: string[];
  tips: string[];
}

export interface JourneyStep {
  number: number;
  action: string;
  owner: string;
  systemResponse: string;
  validations: string[];
  rbac: string | null;
  screenshot?: string;
  auditLog?: string;
  notifications?: string[];
}

export interface Journey {
  id: string;
  name: string;
  module: string;
  category: string;
  priority: string;
  description: string;
  goal: string;
  personas: string[];
  preconditions: string[];
  steps: JourneyStep[];
  edgeCases: string[];
  successCriteria: string[];
  estimatedDuration: string;
  relatedJourneys: string[];
}

// Navigation structure
export const docsNavigation: DocSection[] = [
  {
    id: 'getting-started',
    title: 'Getting Started',
    path: '/docs/getting-started',
    icon: 'Rocket',
  },
  {
    id: 'personas',
    title: 'User Personas',
    path: '/docs/personas',
    icon: 'Users',
    children: [
      { id: 'system-admin', title: 'System Administrator', path: '/docs/personas/system-admin' },
      { id: 'tenant-admin', title: 'Tenant Administrator', path: '/docs/personas/tenant-admin' },
      { id: 'doctor', title: 'Doctor / Physician', path: '/docs/personas/doctor' },
      { id: 'nurse', title: 'Nurse', path: '/docs/personas/nurse' },
      { id: 'receptionist', title: 'Receptionist', path: '/docs/personas/receptionist' },
      { id: 'lab-technician', title: 'Lab Technician', path: '/docs/personas/lab-technician' },
      { id: 'pharmacist', title: 'Pharmacist', path: '/docs/personas/pharmacist' },
      { id: 'radiologist', title: 'Radiologist', path: '/docs/personas/radiologist' },
      { id: 'audiologist', title: 'Audiologist', path: '/docs/personas/audiologist' },
      { id: 'hr-manager', title: 'HR Manager', path: '/docs/personas/hr-manager' },
      { id: 'accountant', title: 'Accountant', path: '/docs/personas/accountant' },
      { id: 'patient', title: 'Patient', path: '/docs/personas/patient' },
    ],
  },
  {
    id: 'modules',
    title: 'Product Modules',
    path: '/docs/modules',
    icon: 'LayoutGrid',
    children: [
      { id: 'patient-management', title: 'Patient Management', path: '/docs/modules/patient-management' },
      { id: 'appointments', title: 'Appointments', path: '/docs/modules/appointments' },
      { id: 'clinical-visits', title: 'Clinical Visits', path: '/docs/modules/clinical-visits' },
      { id: 'laboratory', title: 'Laboratory', path: '/docs/modules/laboratory' },
      { id: 'radiology', title: 'Radiology', path: '/docs/modules/radiology' },
      { id: 'pharmacy', title: 'Pharmacy', path: '/docs/modules/pharmacy' },
      { id: 'financial', title: 'Financial Management', path: '/docs/modules/financial' },
      { id: 'inventory', title: 'Inventory Management', path: '/docs/modules/inventory' },
      { id: 'hr-management', title: 'HR Management', path: '/docs/modules/hr-management' },
      { id: 'marketing', title: 'Marketing', path: '/docs/modules/marketing' },
      { id: 'workflow-engine', title: 'Workflow Engine', path: '/docs/modules/workflow-engine' },
      { id: 'patient-portal', title: 'Patient Portal', path: '/docs/modules/patient-portal' },
      { id: 'analytics', title: 'Analytics & Reporting', path: '/docs/modules/analytics' },
      { id: 'specialty-modules', title: 'Specialty Modules', path: '/docs/modules/specialty-modules' },
    ],
  },
  {
    id: 'journeys',
    title: 'User Journeys',
    path: '/docs/journeys',
    icon: 'Route',
    children: [
      { id: 'patient-registration', title: 'Patient Registration', path: '/docs/journeys/patient-registration' },
      { id: 'appointment-booking', title: 'Appointment Booking', path: '/docs/journeys/appointment-booking' },
      { id: 'patient-checkin', title: 'Patient Check-in', path: '/docs/journeys/patient-checkin' },
      { id: 'clinical-visit', title: 'Complete Clinical Visit', path: '/docs/journeys/clinical-visit' },
      { id: 'lab-order-to-results', title: 'Lab Order to Results', path: '/docs/journeys/lab-order-to-results' },
      { id: 'prescription-dispensing', title: 'Prescription Dispensing', path: '/docs/journeys/prescription-dispensing' },
      { id: 'invoice-payment', title: 'Invoice & Payment', path: '/docs/journeys/invoice-payment' },
      { id: 'employee-onboarding', title: 'Employee Onboarding', path: '/docs/journeys/employee-onboarding' },
      { id: 'insurance-claim-submission', title: 'Insurance Claims', path: '/docs/journeys/insurance-claim-submission' },
      { id: 'imaging-study', title: 'Imaging Study', path: '/docs/journeys/imaging-study' },
    ],
  },
  {
    id: 'admin-configuration',
    title: 'Admin & Configuration',
    path: '/docs/admin-configuration',
    icon: 'Settings',
  },
  {
    id: 'security-rbac',
    title: 'Security & RBAC',
    path: '/docs/security-rbac',
    icon: 'Shield',
  },
  {
    id: 'api-reference',
    title: 'API Reference',
    path: '/docs/api-reference',
    icon: 'Code',
  },
  {
    id: 'faq-troubleshooting',
    title: 'FAQ & Troubleshooting',
    path: '/docs/faq-troubleshooting',
    icon: 'HelpCircle',
  },
  {
    id: 'release-notes',
    title: 'Release Notes',
    path: '/docs/release-notes',
    icon: 'FileText',
  },
  {
    id: 'glossary',
    title: 'Glossary',
    path: '/docs/glossary',
    icon: 'Book',
  },
];

// Module data
export const modulesData: Module[] = [
  {
    id: 'patient-management',
    name: 'Patient Management',
    category: 'core',
    description: 'Comprehensive patient registration, profile management, medical history, and document handling for healthcare organizations.',
    route: '/patients',
    icon: 'Users',
    businessValue: 'Centralizes patient data for efficient care coordination, reduces duplicate records, and ensures regulatory compliance with healthcare data standards.',
    personas: ['receptionist', 'nurse', 'doctor', 'admin'],
    features: [
      { id: 'patient-registration', name: 'Patient Registration', description: 'Register new patients with personal, contact, and emergency information' },
      { id: 'patient-search', name: 'Patient Search & Lookup', description: 'Search patients by name, ID, phone, or Emirates ID' },
      { id: 'medical-history', name: 'Medical History Management', description: 'Track allergies, chronic conditions, surgeries, and family history' },
      { id: 'document-management', name: 'Document Management', description: 'Upload, categorize, and manage patient documents' },
      { id: 'insurance-management', name: 'Insurance Information', description: 'Manage insurance provider details and coverage verification' },
    ],
    permissions: ['patients:view', 'patients:create', 'patients:update', 'patients:delete', 'patients:export'],
    journeyCount: 5,
  },
  {
    id: 'appointments',
    name: 'Appointments',
    category: 'core',
    description: 'Complete appointment scheduling system with booking, rescheduling, cancellation, check-in, waitlist, and recurring appointment management.',
    route: '/appointments',
    icon: 'Calendar',
    businessValue: 'Optimizes provider utilization, reduces no-shows with automated reminders, and improves patient satisfaction through seamless scheduling.',
    personas: ['receptionist', 'nurse', 'doctor', 'patient', 'admin'],
    features: [
      { id: 'appointment-booking', name: 'Appointment Booking', description: 'Schedule appointments with provider, specialty, and time slot selection' },
      { id: 'appointment-rescheduling', name: 'Appointment Rescheduling', description: 'Modify existing appointment date and time' },
      { id: 'appointment-cancellation', name: 'Appointment Cancellation', description: 'Cancel appointments with reason tracking' },
      { id: 'patient-checkin', name: 'Patient Check-in', description: 'Check in patients on arrival and notify providers' },
      { id: 'waitlist-management', name: 'Waitlist Management', description: 'Manage waitlist for earlier appointments' },
      { id: 'recurring-appointments', name: 'Recurring Appointments', description: 'Set up recurring appointment schedules' },
    ],
    permissions: ['appointments:view', 'appointments:create', 'appointments:update', 'appointments:delete', 'appointments:reschedule'],
    journeyCount: 6,
  },
  {
    id: 'clinical-visits',
    name: 'Clinical Visits',
    category: 'clinical',
    description: 'Full clinical encounter management including patient intake, vitals recording, examinations, diagnosis, prescriptions, treatment plans, and referrals.',
    route: '/clinical-visits',
    icon: 'Stethoscope',
    businessValue: 'Streamlines clinical workflows, ensures complete documentation for billing and compliance, and supports evidence-based care decisions.',
    personas: ['nurse', 'doctor'],
    features: [
      { id: 'patient-intake', name: 'Patient Intake & Vitals', description: 'Record vital signs (BP, temperature, pulse, weight, height, BMI, O2 sat)' },
      { id: 'clinical-examination', name: 'Clinical Examination', description: 'Document physical examination findings' },
      { id: 'diagnosis-entry', name: 'Diagnosis Entry', description: 'ICD-10 coded diagnosis with primary/secondary classification' },
      { id: 'prescription-management', name: 'Prescription Management', description: 'E-prescribing with drug interaction checking' },
      { id: 'treatment-plan', name: 'Treatment Plan Creation', description: 'Comprehensive treatment planning with goals and follow-ups' },
      { id: 'referral-management', name: 'Referral Management', description: 'Create and track specialist referrals' },
    ],
    permissions: ['clinical_visits:view', 'clinical_visits:create', 'clinical_visits:update', 'prescriptions:create'],
    journeyCount: 7,
  },
  {
    id: 'laboratory',
    name: 'Laboratory',
    category: 'clinical',
    description: 'Complete laboratory management including order creation, specimen collection, processing, result entry, validation, and reporting with quality control.',
    route: '/laboratory',
    icon: 'FlaskConical',
    businessValue: 'Ensures accurate lab results with QC workflows, reduces turnaround time, and integrates seamlessly with clinical workflows for faster diagnosis.',
    personas: ['doctor', 'nurse', 'lab-technician'],
    features: [
      { id: 'lab-order-creation', name: 'Lab Order Creation', description: 'Order lab tests with clinical priority and indications' },
      { id: 'specimen-collection', name: 'Specimen Collection', description: 'Record specimen collection with barcode labeling' },
      { id: 'result-entry', name: 'Result Entry & Validation', description: 'Enter results with reference ranges and abnormal flagging' },
      { id: 'result-reporting', name: 'Result Reporting', description: 'Generate and distribute lab reports' },
      { id: 'quality-control', name: 'Quality Control', description: 'Daily QC testing and documentation' },
    ],
    permissions: ['lab:view', 'lab:create', 'lab:update', 'lab:approve'],
    journeyCount: 6,
  },
  {
    id: 'pharmacy',
    name: 'Pharmacy',
    category: 'clinical',
    description: 'Complete pharmacy operations including prescription receipt, verification, medication dispensing, patient counseling, and controlled substance management.',
    route: '/pharmacy',
    icon: 'Pill',
    businessValue: 'Ensures medication safety with interaction checking, manages controlled substances compliantly, and optimizes pharmacy inventory.',
    personas: ['pharmacist', 'doctor'],
    features: [
      { id: 'prescription-receipt', name: 'Prescription Receipt', description: 'Receive and validate electronic/paper prescriptions' },
      { id: 'prescription-verification', name: 'Prescription Verification', description: 'Check interactions, dosage, allergies' },
      { id: 'medication-dispensing', name: 'Medication Dispensing', description: 'Dispense with verification and labeling' },
      { id: 'patient-counseling', name: 'Patient Counseling', description: 'Medication education and instructions' },
      { id: 'controlled-substances', name: 'Controlled Substance Management', description: 'DEA-compliant controlled substance handling' },
    ],
    permissions: ['pharmacy:view', 'pharmacy:dispense', 'pharmacy:manage'],
    journeyCount: 6,
  },
  {
    id: 'financial',
    name: 'Financial Management',
    category: 'business',
    description: 'Complete financial operations including invoicing, payment processing, insurance claims, accounts receivable, expense management, and financial reporting.',
    route: '/financial',
    icon: 'DollarSign',
    businessValue: 'Maximizes revenue capture, accelerates payment collection, streamlines insurance billing, and provides financial visibility for decision making.',
    personas: ['billing-staff', 'accountant', 'admin'],
    features: [
      { id: 'invoice-generation', name: 'Invoice Generation', description: 'Create invoices with procedure codes and taxes' },
      { id: 'payment-processing', name: 'Payment Processing', description: 'Process cash, card, and insurance payments' },
      { id: 'insurance-claims', name: 'Insurance Claim Submission', description: 'Electronic claim submission with tracking' },
      { id: 'accounts-receivable', name: 'Accounts Receivable', description: 'Aging reports and collection management' },
      { id: 'financial-reporting', name: 'Financial Reporting', description: 'Generate financial statements and reports' },
    ],
    permissions: ['financial:view', 'financial:create', 'financial:update', 'financial:approve', 'financial:reports'],
    journeyCount: 6,
  },
];

// Personas data
export const personasData: Persona[] = [
  {
    id: 'doctor',
    name: 'Doctor / Physician',
    category: 'clinical',
    description: 'Licensed medical practitioner responsible for patient examinations, diagnoses, treatment plans, prescriptions, and clinical documentation.',
    responsibilities: [
      'Conduct patient examinations',
      'Diagnose conditions using clinical findings',
      'Prescribe medications and treatments',
      'Order laboratory and imaging tests',
      'Create and manage treatment plans',
      'Refer patients to specialists',
      'Document clinical encounters',
    ],
    accessScope: {
      level: 'clinical',
      dataAccess: 'Patient medical records, appointments',
      roleType: 'PHYSICIAN',
    },
    permissions: [
      'patients:view', 'patients:update', 'appointments:view', 'appointments:create',
      'clinical_visits:view', 'clinical_visits:create', 'clinical_visits:update',
      'prescriptions:create', 'lab:view', 'lab:create', 'radiology:view', 'radiology:create',
    ],
    primaryModules: ['clinical-visits', 'patient-management', 'appointments', 'laboratory', 'pharmacy'],
    coreJourneys: ['clinical-examination', 'diagnosis-entry', 'prescription-management', 'lab-order-creation', 'referral-management'],
    entryPoints: ['/', '/appointments', '/patients', '/clinical-visits'],
    commonTasks: [
      'Review patient history before visits',
      'Document examination findings',
      'Enter diagnoses with ICD-10 codes',
      'Prescribe medications with interaction checking',
      'Order labs and imaging studies',
      'Create follow-up appointments',
    ],
    tips: [
      'Use the clinical decision support alerts for drug interactions',
      'Review prior visit notes before each encounter',
      'Sign and lock visits promptly for billing',
    ],
  },
  {
    id: 'nurse',
    name: 'Nurse',
    category: 'clinical',
    description: 'Clinical staff member responsible for patient intake, vital signs recording, assisting physicians, medication administration, and patient education.',
    responsibilities: [
      'Record patient vital signs',
      'Assist with patient intake',
      'Administer medications',
      'Support clinical procedures',
      'Provide patient education',
      'Update patient records',
    ],
    accessScope: {
      level: 'clinical',
      dataAccess: 'Patient records (limited), vital signs, medications',
      roleType: 'NURSE',
    },
    permissions: [
      'patients:view', 'patients:update', 'appointments:view', 'appointments:create',
      'clinical_visits:view', 'clinical_visits:create', 'lab:view', 'lab:create', 'inventory:view',
    ],
    primaryModules: ['clinical-visits', 'appointments', 'patient-management', 'laboratory'],
    coreJourneys: ['patient-intake-vitals', 'medication-administration', 'specimen-collection'],
    entryPoints: ['/', '/appointments', '/patients'],
    commonTasks: [
      'Take and record vital signs',
      'Call patients from waiting room',
      'Document chief complaints',
      'Collect specimens for lab tests',
      'Administer vaccines and medications',
      'Prepare patients for examination',
    ],
    tips: [
      'Always verify patient identity before any procedure',
      'Double-check vital signs entries for accuracy',
      'Document any patient concerns in visit notes',
    ],
  },
  {
    id: 'receptionist',
    name: 'Receptionist',
    category: 'front-office',
    description: 'Front desk staff responsible for patient registration, appointment scheduling, check-in/check-out, and initial patient contact.',
    responsibilities: [
      'Register new patients',
      'Schedule appointments',
      'Check in arriving patients',
      'Verify patient information',
      'Collect co-payments',
      'Manage phone inquiries',
      'Handle appointment changes',
    ],
    accessScope: {
      level: 'front-office',
      dataAccess: 'Patient demographics, appointments, basic billing',
      roleType: 'RECEPTIONIST',
    },
    permissions: [
      'patients:view', 'patients:create', 'patients:update',
      'appointments:view', 'appointments:create', 'appointments:update', 'appointments:delete', 'appointments:reschedule',
      'financial:view',
    ],
    primaryModules: ['patient-management', 'appointments', 'financial'],
    coreJourneys: ['patient-registration', 'appointment-booking', 'appointment-rescheduling', 'patient-checkin'],
    entryPoints: ['/', '/appointments', '/patients'],
    commonTasks: [
      'Register new patients with complete information',
      'Book and confirm appointments',
      'Check in patients on arrival',
      'Update patient contact information',
      'Handle appointment cancellations',
      'Collect and record co-payments',
    ],
    tips: [
      'Verify insurance information at each visit',
      'Confirm contact details are current',
      'Use the waitlist for patients seeking earlier appointments',
    ],
  },
];

// Journeys data
export const journeysData: Journey[] = [
  {
    id: 'patient-registration',
    name: 'Patient Registration',
    module: 'patient-management',
    category: 'core',
    priority: 'high',
    description: 'Complete patient registration workflow from initial data entry to confirmation, including personal information, contact details, emergency contacts, and photo capture.',
    goal: 'Register a new patient in the system with all required demographic and contact information',
    personas: ['receptionist'],
    preconditions: [
      'User has receptionist role or higher',
      'Patient is not already registered in the system',
    ],
    steps: [
      {
        number: 1,
        action: 'Navigate to patient registration',
        owner: 'Receptionist',
        systemResponse: 'Displays registration form',
        validations: [],
        rbac: 'patients:create',
      },
      {
        number: 2,
        action: 'Enter personal information (name, DOB, gender)',
        owner: 'Receptionist',
        systemResponse: 'Validates required fields',
        validations: ['Name is required', 'Valid date of birth', 'Gender selection required'],
        rbac: 'patients:create',
      },
      {
        number: 3,
        action: 'Enter contact details (phone, email, address)',
        owner: 'Receptionist',
        systemResponse: 'Validates phone and email format',
        validations: ['Valid phone format', 'Valid email format', 'Address required'],
        rbac: 'patients:create',
      },
      {
        number: 4,
        action: 'Add emergency contact information',
        owner: 'Receptionist',
        systemResponse: 'Optional but recommended',
        validations: ['Emergency contact name', 'Relationship', 'Phone number'],
        rbac: 'patients:create',
      },
      {
        number: 5,
        action: 'Capture/upload patient photo',
        owner: 'Receptionist',
        systemResponse: 'Webcam capture or file upload',
        validations: ['Image format validation'],
        rbac: 'patients:create',
      },
      {
        number: 6,
        action: 'System assigns unique patient ID',
        owner: 'System',
        systemResponse: 'Generates MRN (Medical Record Number)',
        validations: ['Unique ID generated'],
        rbac: null,
        auditLog: 'Patient record created',
      },
      {
        number: 7,
        action: 'Save and confirm registration',
        owner: 'Receptionist',
        systemResponse: 'Displays confirmation with patient ID',
        validations: ['All required fields complete'],
        rbac: 'patients:create',
      },
    ],
    edgeCases: [
      'Duplicate patient detected - system prompts for verification',
      'Invalid Emirates ID format - validation error shown',
      'Missing required fields - cannot proceed to save',
    ],
    successCriteria: [
      'Patient record created successfully',
      'Unique patient ID assigned',
      'Confirmation displayed to user',
      'Audit log entry created',
    ],
    estimatedDuration: '5-10 minutes',
    relatedJourneys: ['appointment-booking', 'insurance-information'],
  },
  {
    id: 'appointment-booking',
    name: 'Appointment Booking',
    module: 'appointments',
    category: 'core',
    priority: 'high',
    description: 'Schedule a new appointment for a patient with provider selection, time slot availability, and confirmation notifications.',
    goal: 'Book an appointment for a patient with the appropriate provider and time slot',
    personas: ['receptionist', 'patient'],
    preconditions: [
      'Patient is registered in the system',
      'Provider schedule is configured',
      'User has appointment create permission',
    ],
    steps: [
      {
        number: 1,
        action: 'Search for patient',
        owner: 'Receptionist',
        systemResponse: 'Patient search results displayed',
        validations: ['Patient found in system'],
        rbac: 'patients:view',
      },
      {
        number: 2,
        action: 'Select appointment type',
        owner: 'Receptionist',
        systemResponse: 'Shows appointment types (consultation, follow-up, procedure)',
        validations: ['Appointment type selected'],
        rbac: 'appointments:create',
      },
      {
        number: 3,
        action: 'Choose department/specialty',
        owner: 'Receptionist',
        systemResponse: 'Filters providers by specialty',
        validations: ['Department selected'],
        rbac: 'appointments:create',
      },
      {
        number: 4,
        action: 'Select doctor/provider',
        owner: 'Receptionist',
        systemResponse: 'Shows available providers in specialty',
        validations: ['Provider selected'],
        rbac: 'appointments:create',
      },
      {
        number: 5,
        action: 'View available time slots',
        owner: 'System',
        systemResponse: 'Displays calendar with availability',
        validations: ['Slots loaded from provider schedule'],
        rbac: null,
      },
      {
        number: 6,
        action: 'Select preferred date and time',
        owner: 'Receptionist',
        systemResponse: 'Highlights selected slot',
        validations: ['Slot is available', 'Within booking window'],
        rbac: 'appointments:create',
      },
      {
        number: 7,
        action: 'Add appointment notes/reason',
        owner: 'Receptionist',
        systemResponse: 'Notes field for visit reason',
        validations: [],
        rbac: 'appointments:create',
      },
      {
        number: 8,
        action: 'Confirm booking',
        owner: 'Receptionist',
        systemResponse: 'Creates appointment record',
        validations: ['Slot still available'],
        rbac: 'appointments:create',
      },
      {
        number: 9,
        action: 'Send confirmation notification',
        owner: 'System',
        systemResponse: 'SMS/Email sent to patient',
        validations: ['Patient contact info available'],
        rbac: null,
        notifications: ['SMS confirmation', 'Email confirmation'],
      },
    ],
    edgeCases: [
      'Slot becomes unavailable during booking - prompt to select another',
      'Patient has conflicting appointment - warning displayed',
      'Provider schedule changes - real-time update',
    ],
    successCriteria: [
      'Appointment created successfully',
      'Confirmation sent to patient',
      'Appointment appears on provider calendar',
      'Audit log entry created',
    ],
    estimatedDuration: '3-5 minutes',
    relatedJourneys: ['patient-checkin', 'appointment-rescheduling'],
  },
  {
    id: 'clinical-visit',
    name: 'Complete Clinical Visit',
    module: 'clinical-visits',
    category: 'clinical',
    priority: 'high',
    description: 'Full clinical encounter from patient intake through examination, diagnosis, prescription, and visit completion with documentation.',
    goal: 'Complete a clinical encounter with proper documentation for patient care and billing',
    personas: ['nurse', 'doctor'],
    preconditions: [
      'Patient is checked in',
      'Provider is available',
      'Clinical visit record exists',
    ],
    steps: [
      {
        number: 1,
        action: 'Call patient from waiting area',
        owner: 'Nurse',
        systemResponse: 'Queue updates to show patient in exam',
        validations: [],
        rbac: 'clinical_visits:view',
      },
      {
        number: 2,
        action: 'Open patient visit record',
        owner: 'Nurse',
        systemResponse: 'Visit documentation opens',
        validations: ['Visit exists'],
        rbac: 'clinical_visits:view',
      },
      {
        number: 3,
        action: 'Measure and record vitals',
        owner: 'Nurse',
        systemResponse: 'Vitals form displayed',
        validations: ['All vital fields populated'],
        rbac: 'clinical_visits:create',
      },
      {
        number: 4,
        action: 'Document chief complaint',
        owner: 'Nurse',
        systemResponse: 'Notes saved to visit',
        validations: ['Complaint documented'],
        rbac: 'clinical_visits:create',
      },
      {
        number: 5,
        action: 'Review patient history',
        owner: 'Doctor',
        systemResponse: 'History displayed from record',
        validations: [],
        rbac: 'patients:view',
      },
      {
        number: 6,
        action: 'Conduct physical examination',
        owner: 'Doctor',
        systemResponse: 'Exam form for documentation',
        validations: [],
        rbac: 'clinical_visits:update',
      },
      {
        number: 7,
        action: 'Document examination findings',
        owner: 'Doctor',
        systemResponse: 'Findings saved',
        validations: ['Findings documented'],
        rbac: 'clinical_visits:update',
      },
      {
        number: 8,
        action: 'Enter diagnosis (ICD-10)',
        owner: 'Doctor',
        systemResponse: 'ICD-10 search and selection',
        validations: ['Valid ICD-10 code'],
        rbac: 'clinical_visits:update',
      },
      {
        number: 9,
        action: 'Create prescription',
        owner: 'Doctor',
        systemResponse: 'Medication selection with interaction check',
        validations: ['No drug interactions', 'No allergies'],
        rbac: 'prescriptions:create',
      },
      {
        number: 10,
        action: 'Order lab/imaging if needed',
        owner: 'Doctor',
        systemResponse: 'Order forms displayed',
        validations: [],
        rbac: 'lab:create',
      },
      {
        number: 11,
        action: 'Sign and complete visit',
        owner: 'Doctor',
        systemResponse: 'Visit locked for billing',
        validations: ['All required fields complete'],
        rbac: 'clinical_visits:update',
        auditLog: 'Visit completed and signed',
      },
      {
        number: 12,
        action: 'Generate after-visit summary',
        owner: 'System',
        systemResponse: 'Summary PDF generated',
        validations: [],
        rbac: null,
        notifications: ['Patient summary available'],
      },
    ],
    edgeCases: [
      'Drug interaction detected - requires acknowledgment to proceed',
      'Allergy alert - medication cannot be prescribed',
      'Critical vital signs - alert displayed',
    ],
    successCriteria: [
      'Visit documentation complete',
      'Diagnoses coded properly',
      'Prescriptions signed',
      'Orders placed',
      'Visit signed and locked',
    ],
    estimatedDuration: '15-30 minutes',
    relatedJourneys: ['lab-order-to-results', 'prescription-dispensing'],
  },
];

// Search index helper
export function buildSearchIndex() {
  const index: { title: string; path: string; content: string; type: string }[] = [];

  // Add modules to index
  modulesData.forEach((module) => {
    index.push({
      title: module.name,
      path: `/docs/modules/${module.id}`,
      content: `${module.description} ${module.businessValue} ${module.features.map((f) => f.name).join(' ')}`,
      type: 'module',
    });
  });

  // Add personas to index
  personasData.forEach((persona) => {
    index.push({
      title: persona.name,
      path: `/docs/personas/${persona.id}`,
      content: `${persona.description} ${persona.responsibilities.join(' ')} ${persona.commonTasks.join(' ')}`,
      type: 'persona',
    });
  });

  // Add journeys to index
  journeysData.forEach((journey) => {
    index.push({
      title: journey.name,
      path: `/docs/journeys/${journey.id}`,
      content: `${journey.description} ${journey.goal} ${journey.steps.map((s) => s.action).join(' ')}`,
      type: 'journey',
    });
  });

  return index;
}

// Helper functions
export function getModuleById(id: string): Module | undefined {
  return modulesData.find((m) => m.id === id);
}

export function getPersonaById(id: string): Persona | undefined {
  return personasData.find((p) => p.id === id);
}

export function getJourneyById(id: string): Journey | undefined {
  return journeysData.find((j) => j.id === id);
}

export function getModulesByCategory(category: string): Module[] {
  return modulesData.filter((m) => m.category === category);
}

export function getJourneysByModule(moduleId: string): Journey[] {
  return journeysData.filter((j) => j.module === moduleId);
}
