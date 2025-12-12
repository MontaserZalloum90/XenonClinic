import { Link } from 'react-router-dom';
import {
  ArrowRight,
  Users,
  Calendar,
  Stethoscope,
  Beaker,
  Radio,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Wallet,
  Megaphone,
  GitBranch,
  Globe,
  Building2,
  BarChart3,
  Shield,
  Heart,
  Eye,
  Bone,
  Baby,
  Activity,
  Ear,
  Scissors,
  Brain,
  Droplet,
  ChevronDown,
  ChevronUp,
} from 'lucide-react';
import { useState } from 'react';

interface Journey {
  name: string;
  steps: { step: string; owner: string }[];
}

interface Module {
  icon: React.ElementType;
  name: string;
  description: string;
  journeyCount: number;
  stepCount: number;
  journeys: Journey[];
}

const coreModules: Module[] = [
  {
    icon: Users,
    name: 'Patient Management',
    description: 'Complete patient records, medical history, documents, and demographics.',
    journeyCount: 5,
    stepCount: 35,
    journeys: [
      {
        name: 'Patient Registration',
        steps: [
          { step: 'Navigate to patient registration', owner: 'Receptionist' },
          { step: 'Enter personal information (name, DOB, gender)', owner: 'Receptionist' },
          { step: 'Enter contact details (phone, email, address)', owner: 'Receptionist' },
          { step: 'Add emergency contact information', owner: 'Receptionist' },
          { step: 'Capture/upload patient photo', owner: 'Receptionist' },
          { step: 'Assign unique patient ID', owner: 'System' },
          { step: 'Save and confirm registration', owner: 'Receptionist' },
        ],
      },
      {
        name: 'Patient Search & Lookup',
        steps: [
          { step: 'Access patient search interface', owner: 'Staff' },
          { step: 'Enter search criteria (name, ID, phone)', owner: 'Staff' },
          { step: 'Review search results', owner: 'Staff' },
          { step: 'Select patient from results', owner: 'Staff' },
          { step: 'View patient profile', owner: 'Staff' },
        ],
      },
      {
        name: 'Medical History Management',
        steps: [
          { step: 'Open patient record', owner: 'Nurse/Doctor' },
          { step: 'Navigate to medical history section', owner: 'Nurse/Doctor' },
          { step: 'Add allergies and reactions', owner: 'Nurse/Doctor' },
          { step: 'Record chronic conditions', owner: 'Doctor' },
          { step: 'Enter past surgeries/procedures', owner: 'Doctor' },
          { step: 'Document family medical history', owner: 'Doctor' },
          { step: 'Save medical history', owner: 'Nurse/Doctor' },
        ],
      },
    ],
  },
  {
    icon: Calendar,
    name: 'Appointments',
    description: 'Scheduling, calendar views, status tracking, and automated reminders.',
    journeyCount: 6,
    stepCount: 45,
    journeys: [
      {
        name: 'Appointment Booking',
        steps: [
          { step: 'Search for patient', owner: 'Receptionist' },
          { step: 'Select appointment type', owner: 'Receptionist' },
          { step: 'Choose department/specialty', owner: 'Receptionist' },
          { step: 'Select doctor/provider', owner: 'Receptionist' },
          { step: 'View available time slots', owner: 'System' },
          { step: 'Select preferred date and time', owner: 'Receptionist' },
          { step: 'Add appointment notes/reason', owner: 'Receptionist' },
          { step: 'Confirm booking', owner: 'Receptionist' },
          { step: 'Send confirmation notification', owner: 'System' },
        ],
      },
      {
        name: 'Patient Check-in',
        steps: [
          { step: 'Identify patient arrival', owner: 'Receptionist' },
          { step: 'Verify appointment details', owner: 'Receptionist' },
          { step: 'Confirm patient identity', owner: 'Receptionist' },
          { step: 'Update appointment status to "Checked-in"', owner: 'Receptionist' },
          { step: 'Collect co-payment if applicable', owner: 'Receptionist' },
          { step: 'Direct patient to waiting area', owner: 'Receptionist' },
          { step: 'Notify provider of arrival', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: Stethoscope,
    name: 'Clinical Visits',
    description: 'Vitals, examination, diagnosis, prescriptions, and treatment plans.',
    journeyCount: 7,
    stepCount: 55,
    journeys: [
      {
        name: 'Patient Intake & Vitals',
        steps: [
          { step: 'Call patient from waiting area', owner: 'Nurse' },
          { step: 'Open patient visit record', owner: 'Nurse' },
          { step: 'Measure and record blood pressure', owner: 'Nurse' },
          { step: 'Record temperature', owner: 'Nurse' },
          { step: 'Measure pulse/heart rate', owner: 'Nurse' },
          { step: 'Record respiratory rate', owner: 'Nurse' },
          { step: 'Measure height and weight', owner: 'Nurse' },
          { step: 'Calculate BMI', owner: 'System' },
          { step: 'Record oxygen saturation', owner: 'Nurse' },
          { step: 'Document chief complaint', owner: 'Nurse' },
          { step: 'Save vitals to visit record', owner: 'Nurse' },
        ],
      },
      {
        name: 'Prescription Management',
        steps: [
          { step: 'Open prescription module', owner: 'Doctor' },
          { step: 'Search medication database', owner: 'Doctor' },
          { step: 'Select medication', owner: 'Doctor' },
          { step: 'Set dosage and frequency', owner: 'Doctor' },
          { step: 'Define duration of treatment', owner: 'Doctor' },
          { step: 'Check drug interactions', owner: 'System' },
          { step: 'Review allergy alerts', owner: 'System' },
          { step: 'Add prescription instructions', owner: 'Doctor' },
          { step: 'Sign prescription', owner: 'Doctor' },
          { step: 'Print/send to pharmacy', owner: 'Doctor' },
        ],
      },
    ],
  },
  {
    icon: Beaker,
    name: 'Laboratory',
    description: 'Lab orders, specimen tracking, results entry, and external lab integration.',
    journeyCount: 6,
    stepCount: 40,
    journeys: [
      {
        name: 'Lab Order Creation',
        steps: [
          { step: 'Access lab order module', owner: 'Doctor/Nurse' },
          { step: 'Select patient', owner: 'Doctor/Nurse' },
          { step: 'Choose test panel or individual tests', owner: 'Doctor' },
          { step: 'Set clinical priority', owner: 'Doctor' },
          { step: 'Add clinical notes/indications', owner: 'Doctor' },
          { step: 'Specify fasting requirements', owner: 'Doctor' },
          { step: 'Submit lab order', owner: 'Doctor' },
          { step: 'Print requisition/labels', owner: 'System' },
        ],
      },
      {
        name: 'Result Entry & Validation',
        steps: [
          { step: 'Review analyzer output', owner: 'Lab Technician' },
          { step: 'Enter manual results if needed', owner: 'Lab Technician' },
          { step: 'Flag abnormal values', owner: 'System' },
          { step: 'Apply reference ranges', owner: 'System' },
          { step: 'Technical validation', owner: 'Lab Technician' },
          { step: 'Medical validation/review', owner: 'Pathologist' },
          { step: 'Approve and release results', owner: 'Pathologist' },
        ],
      },
    ],
  },
  {
    icon: Radio,
    name: 'Radiology',
    description: 'Imaging orders, DICOM viewer, PACS integration, and reporting.',
    journeyCount: 6,
    stepCount: 40,
    journeys: [
      {
        name: 'Image Acquisition',
        steps: [
          { step: 'Configure imaging equipment', owner: 'Radiology Technician' },
          { step: 'Acquire images', owner: 'Radiology Technician' },
          { step: 'Review image quality', owner: 'Radiology Technician' },
          { step: 'Retake if necessary', owner: 'Radiology Technician' },
          { step: 'Process images', owner: 'System' },
          { step: 'Upload to PACS', owner: 'System' },
          { step: 'Document acquisition details', owner: 'Radiology Technician' },
        ],
      },
      {
        name: 'Image Interpretation',
        steps: [
          { step: 'Open study in DICOM viewer', owner: 'Radiologist' },
          { step: 'Review clinical history', owner: 'Radiologist' },
          { step: 'Analyze images systematically', owner: 'Radiologist' },
          { step: 'Compare with prior studies', owner: 'Radiologist' },
          { step: 'Identify findings', owner: 'Radiologist' },
          { step: 'Measure lesions if present', owner: 'Radiologist' },
          { step: 'Dictate/type report', owner: 'Radiologist' },
        ],
      },
    ],
  },
  {
    icon: Pill,
    name: 'Pharmacy',
    description: 'Prescription management, dispensing, and inventory tracking.',
    journeyCount: 6,
    stepCount: 50,
    journeys: [
      {
        name: 'Medication Dispensing',
        steps: [
          { step: 'Select medication from inventory', owner: 'Pharmacy Technician' },
          { step: 'Verify drug/strength/form', owner: 'Pharmacy Technician' },
          { step: 'Count/measure quantity', owner: 'Pharmacy Technician' },
          { step: 'Label container', owner: 'Pharmacy Technician' },
          { step: 'Pharmacist final check', owner: 'Pharmacist' },
          { step: 'Package medication', owner: 'Pharmacy Technician' },
          { step: 'Record dispensing', owner: 'System' },
        ],
      },
      {
        name: 'Patient Counseling',
        steps: [
          { step: 'Call patient for pickup', owner: 'Pharmacy Staff' },
          { step: 'Verify patient identity', owner: 'Pharmacist' },
          { step: 'Explain medication usage', owner: 'Pharmacist' },
          { step: 'Discuss side effects', owner: 'Pharmacist' },
          { step: 'Answer patient questions', owner: 'Pharmacist' },
          { step: 'Provide written instructions', owner: 'Pharmacist' },
          { step: 'Obtain signature', owner: 'Pharmacy Staff' },
          { step: 'Complete transaction', owner: 'Pharmacy Staff' },
        ],
      },
    ],
  },
  {
    icon: DollarSign,
    name: 'Financial',
    description: 'Invoicing, payments, accounts, expenses, and financial reporting.',
    journeyCount: 6,
    stepCount: 45,
    journeys: [
      {
        name: 'Invoice Generation',
        steps: [
          { step: 'Select patient/visit', owner: 'Billing Staff' },
          { step: 'Review charges', owner: 'Billing Staff' },
          { step: 'Apply procedure/service codes', owner: 'Billing Staff' },
          { step: 'Calculate taxes if applicable', owner: 'System' },
          { step: 'Apply discounts if authorized', owner: 'Billing Staff' },
          { step: 'Generate invoice', owner: 'System' },
          { step: 'Preview and verify', owner: 'Billing Staff' },
          { step: 'Finalize invoice', owner: 'Billing Staff' },
          { step: 'Print/email invoice', owner: 'Billing Staff' },
        ],
      },
      {
        name: 'Insurance Claim Submission',
        steps: [
          { step: 'Verify insurance eligibility', owner: 'Billing Staff' },
          { step: 'Prepare claim with CPT/ICD codes', owner: 'Billing Staff' },
          { step: 'Attach supporting documents', owner: 'Billing Staff' },
          { step: 'Submit claim electronically', owner: 'System' },
          { step: 'Track claim status', owner: 'Billing Staff' },
          { step: 'Receive ERA/EOB', owner: 'System' },
          { step: 'Post insurance payment', owner: 'Billing Staff' },
          { step: 'Handle denials/appeals', owner: 'Billing Staff' },
        ],
      },
    ],
  },
  {
    icon: Package,
    name: 'Inventory',
    description: 'Stock management, goods receipt, transactions, and reorder alerts.',
    journeyCount: 5,
    stepCount: 40,
    journeys: [
      {
        name: 'Stock Receipt',
        steps: [
          { step: 'Receive delivery at warehouse', owner: 'Warehouse Staff' },
          { step: 'Verify against purchase order', owner: 'Warehouse Staff' },
          { step: 'Inspect item quality', owner: 'Warehouse Staff' },
          { step: 'Record received quantities', owner: 'Warehouse Staff' },
          { step: 'Check expiration dates', owner: 'Warehouse Staff' },
          { step: 'Update inventory system', owner: 'System' },
          { step: 'Store items in designated locations', owner: 'Warehouse Staff' },
          { step: 'Complete goods receipt note', owner: 'Warehouse Staff' },
        ],
      },
      {
        name: 'Purchase Order Creation',
        steps: [
          { step: 'Review stock levels', owner: 'Procurement Staff' },
          { step: 'Identify items to reorder', owner: 'Procurement Staff' },
          { step: 'Select supplier', owner: 'Procurement Staff' },
          { step: 'Add items to purchase order', owner: 'Procurement Staff' },
          { step: 'Specify quantities and prices', owner: 'Procurement Staff' },
          { step: 'Set delivery terms', owner: 'Procurement Staff' },
          { step: 'Submit for approval', owner: 'Procurement Staff' },
          { step: 'Approve purchase order', owner: 'Procurement Manager' },
          { step: 'Send PO to supplier', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: UserCog,
    name: 'HR Management',
    description: 'Employees, attendance, leave management, and performance.',
    journeyCount: 5,
    stepCount: 45,
    journeys: [
      {
        name: 'Employee Onboarding',
        steps: [
          { step: 'Create employee record', owner: 'HR Staff' },
          { step: 'Enter personal information', owner: 'HR Staff' },
          { step: 'Upload documents (ID, certificates)', owner: 'HR Staff' },
          { step: 'Assign department and position', owner: 'HR Staff' },
          { step: 'Set reporting manager', owner: 'HR Staff' },
          { step: 'Create system credentials', owner: 'IT Admin' },
          { step: 'Assign roles and permissions', owner: 'IT Admin' },
          { step: 'Schedule orientation', owner: 'HR Staff' },
          { step: 'Complete onboarding checklist', owner: 'HR Staff' },
        ],
      },
      {
        name: 'Leave Request',
        steps: [
          { step: 'Employee submits leave request', owner: 'Employee' },
          { step: 'Select leave type', owner: 'Employee' },
          { step: 'Specify dates', owner: 'Employee' },
          { step: 'Add reason/notes', owner: 'Employee' },
          { step: 'Submit for approval', owner: 'Employee' },
          { step: 'Manager reviews request', owner: 'Manager' },
          { step: 'Check leave balance', owner: 'System' },
          { step: 'Approve or reject request', owner: 'Manager' },
          { step: 'Notify employee of decision', owner: 'System' },
          { step: 'Update leave balance', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: Wallet,
    name: 'Payroll',
    description: 'Salary processing, payslips, overtime, loans, and tax calculations.',
    journeyCount: 5,
    stepCount: 40,
    journeys: [
      {
        name: 'Payroll Processing',
        steps: [
          { step: 'Select pay period', owner: 'Payroll Staff' },
          { step: 'Import attendance data', owner: 'System' },
          { step: 'Calculate work hours', owner: 'System' },
          { step: 'Apply overtime calculations', owner: 'System' },
          { step: 'Add bonuses/commissions', owner: 'Payroll Staff' },
          { step: 'Apply deductions', owner: 'System' },
          { step: 'Calculate tax withholdings', owner: 'System' },
          { step: 'Generate payroll summary', owner: 'System' },
          { step: 'Review for accuracy', owner: 'Payroll Manager' },
          { step: 'Approve payroll', owner: 'Finance Manager' },
          { step: 'Process payments', owner: 'Payroll Staff' },
        ],
      },
    ],
  },
  {
    icon: Megaphone,
    name: 'Marketing',
    description: 'Campaign management, lead tracking, patient outreach, and analytics.',
    journeyCount: 6,
    stepCount: 55,
    journeys: [
      {
        name: 'Campaign Creation',
        steps: [
          { step: 'Access marketing module', owner: 'Marketing Manager' },
          { step: 'Create new campaign', owner: 'Marketing Manager' },
          { step: 'Define campaign name and type', owner: 'Marketing Manager' },
          { step: 'Set target audience', owner: 'Marketing Manager' },
          { step: 'Define campaign objectives', owner: 'Marketing Manager' },
          { step: 'Set budget allocation', owner: 'Marketing Manager' },
          { step: 'Set start and end dates', owner: 'Marketing Manager' },
          { step: 'Design campaign materials', owner: 'Marketing Staff' },
          { step: 'Configure distribution channels', owner: 'Marketing Staff' },
          { step: 'Submit for approval', owner: 'Marketing Staff' },
          { step: 'Approve and launch campaign', owner: 'Marketing Manager' },
        ],
      },
      {
        name: 'Lead Management',
        steps: [
          { step: 'Capture lead (web form, call, referral)', owner: 'System/Staff' },
          { step: 'Create lead record', owner: 'Marketing Staff' },
          { step: 'Assign lead source', owner: 'Marketing Staff' },
          { step: 'Qualify lead', owner: 'Sales Staff' },
          { step: 'Assign to sales representative', owner: 'Marketing Manager' },
          { step: 'Track lead interactions', owner: 'Sales Staff' },
          { step: 'Update lead status', owner: 'Sales Staff' },
          { step: 'Convert to patient/customer', owner: 'Sales Staff' },
        ],
      },
    ],
  },
  {
    icon: GitBranch,
    name: 'Workflow Engine',
    description: 'Process automation, task management, approvals, and escalations.',
    journeyCount: 4,
    stepCount: 30,
    journeys: [
      {
        name: 'Process Definition',
        steps: [
          { step: 'Access workflow designer', owner: 'System Admin' },
          { step: 'Create new process definition', owner: 'System Admin' },
          { step: 'Define start event/trigger', owner: 'System Admin' },
          { step: 'Add workflow steps/tasks', owner: 'System Admin' },
          { step: 'Configure task assignments', owner: 'System Admin' },
          { step: 'Add decision gateways', owner: 'System Admin' },
          { step: 'Define conditions/rules', owner: 'System Admin' },
          { step: 'Set timeouts and escalations', owner: 'System Admin' },
          { step: 'Test workflow', owner: 'System Admin' },
          { step: 'Deploy to production', owner: 'System Admin' },
        ],
      },
    ],
  },
  {
    icon: Globe,
    name: 'Patient Portal',
    description: 'Self-service booking, records access, messaging, and payments.',
    journeyCount: 6,
    stepCount: 45,
    journeys: [
      {
        name: 'Online Appointment Booking',
        steps: [
          { step: 'Log in to patient portal', owner: 'Patient' },
          { step: 'Navigate to appointments', owner: 'Patient' },
          { step: 'Select specialty/doctor', owner: 'Patient' },
          { step: 'View available slots', owner: 'Patient' },
          { step: 'Select preferred time', owner: 'Patient' },
          { step: 'Enter visit reason', owner: 'Patient' },
          { step: 'Confirm booking', owner: 'Patient' },
          { step: 'Receive confirmation', owner: 'System' },
        ],
      },
      {
        name: 'Secure Messaging',
        steps: [
          { step: 'Log in to patient portal', owner: 'Patient' },
          { step: 'Navigate to messages', owner: 'Patient' },
          { step: 'Compose new message', owner: 'Patient' },
          { step: 'Select recipient', owner: 'Patient' },
          { step: 'Write message', owner: 'Patient' },
          { step: 'Attach documents if needed', owner: 'Patient' },
          { step: 'Send message', owner: 'Patient' },
          { step: 'Provider receives notification', owner: 'System' },
          { step: 'Provider responds', owner: 'Provider' },
          { step: 'Patient receives notification', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: Building2,
    name: 'Multi-Tenancy',
    description: 'Tenant, company, branch management, and subscriptions.',
    journeyCount: 5,
    stepCount: 40,
    journeys: [
      {
        name: 'Tenant Provisioning',
        steps: [
          { step: 'Receive new tenant request', owner: 'Platform Admin' },
          { step: 'Create tenant record', owner: 'Platform Admin' },
          { step: 'Configure tenant settings', owner: 'Platform Admin' },
          { step: 'Set up database schema', owner: 'System' },
          { step: 'Create admin user', owner: 'Platform Admin' },
          { step: 'Configure branding', owner: 'Platform Admin' },
          { step: 'Enable modules', owner: 'Platform Admin' },
          { step: 'Activate tenant', owner: 'Platform Admin' },
          { step: 'Send welcome email', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: BarChart3,
    name: 'Analytics & Reporting',
    description: 'Dashboards, KPIs, custom reports, and data visualization.',
    journeyCount: 5,
    stepCount: 35,
    journeys: [
      {
        name: 'Custom Report Creation',
        steps: [
          { step: 'Open report builder', owner: 'Analyst' },
          { step: 'Select data source', owner: 'Analyst' },
          { step: 'Add report columns', owner: 'Analyst' },
          { step: 'Configure filters', owner: 'Analyst' },
          { step: 'Add grouping/sorting', owner: 'Analyst' },
          { step: 'Add calculations', owner: 'Analyst' },
          { step: 'Preview report', owner: 'Analyst' },
          { step: 'Save report', owner: 'Analyst' },
          { step: 'Share with users', owner: 'Analyst' },
        ],
      },
    ],
  },
  {
    icon: Shield,
    name: 'Security & Audit',
    description: 'Access control, audit logging, compliance, and security policies.',
    journeyCount: 5,
    stepCount: 40,
    journeys: [
      {
        name: 'Audit Log Review',
        steps: [
          { step: 'Access audit log module', owner: 'Auditor' },
          { step: 'Set search criteria', owner: 'Auditor' },
          { step: 'Filter by date range', owner: 'Auditor' },
          { step: 'Filter by user/action type', owner: 'Auditor' },
          { step: 'Review log entries', owner: 'Auditor' },
          { step: 'Investigate anomalies', owner: 'Auditor' },
          { step: 'Document findings', owner: 'Auditor' },
          { step: 'Report to management', owner: 'Auditor' },
        ],
      },
    ],
  },
];

const specialtyModules: Module[] = [
  {
    icon: Ear,
    name: 'Audiology',
    description: 'Audiometry, audiograms, hearing aids, and fittings.',
    journeyCount: 4,
    stepCount: 30,
    journeys: [
      {
        name: 'Audiometry Testing',
        steps: [
          { step: 'Prepare audiometry room', owner: 'Audiologist' },
          { step: 'Explain test to patient', owner: 'Audiologist' },
          { step: 'Position patient with headphones', owner: 'Audiologist' },
          { step: 'Conduct pure tone audiometry', owner: 'Audiologist' },
          { step: 'Record thresholds for each frequency', owner: 'Audiologist' },
          { step: 'Perform speech audiometry', owner: 'Audiologist' },
          { step: 'Complete tympanometry', owner: 'Audiologist' },
          { step: 'Plot audiogram', owner: 'System' },
          { step: 'Interpret results', owner: 'Audiologist' },
          { step: 'Recommend hearing aids if needed', owner: 'Audiologist' },
        ],
      },
    ],
  },
  {
    icon: Scissors,
    name: 'Dental',
    description: 'Dental charts, treatments, periodontal exams, and imaging.',
    journeyCount: 5,
    stepCount: 35,
    journeys: [
      {
        name: 'Dental Examination',
        steps: [
          { step: 'Review patient dental history', owner: 'Dentist' },
          { step: 'Perform visual examination', owner: 'Dentist' },
          { step: 'Use explorer for detailed check', owner: 'Dentist' },
          { step: 'Update dental chart', owner: 'Dentist' },
          { step: 'Take X-rays if needed', owner: 'Dental Assistant' },
          { step: 'Document findings per tooth', owner: 'Dentist' },
          { step: 'Create treatment plan', owner: 'Dentist' },
          { step: 'Discuss with patient', owner: 'Dentist' },
        ],
      },
    ],
  },
  {
    icon: Heart,
    name: 'Cardiology',
    description: 'ECG, echocardiogram, stress tests, and catheterization.',
    journeyCount: 5,
    stepCount: 35,
    journeys: [
      {
        name: 'ECG Recording',
        steps: [
          { step: 'Prepare patient', owner: 'Cardiac Technician' },
          { step: 'Apply electrodes', owner: 'Cardiac Technician' },
          { step: 'Record 12-lead ECG', owner: 'Cardiac Technician' },
          { step: 'Check signal quality', owner: 'Cardiac Technician' },
          { step: 'Save ECG recording', owner: 'System' },
          { step: 'Cardiologist reviews ECG', owner: 'Cardiologist' },
          { step: 'Document interpretation', owner: 'Cardiologist' },
          { step: 'Add to patient record', owner: 'System' },
        ],
      },
    ],
  },
  {
    icon: Eye,
    name: 'Ophthalmology',
    description: 'Visual acuity, refraction, IOP, slit lamp, and fundus exams.',
    journeyCount: 5,
    stepCount: 35,
    journeys: [
      {
        name: 'Eye Examination',
        steps: [
          { step: 'Test visual acuity', owner: 'Ophthalmologist/Technician' },
          { step: 'Perform refraction', owner: 'Ophthalmologist' },
          { step: 'Measure intraocular pressure', owner: 'Ophthalmologist' },
          { step: 'Conduct slit lamp examination', owner: 'Ophthalmologist' },
          { step: 'Dilate pupils if needed', owner: 'Ophthalmologist' },
          { step: 'Examine fundus', owner: 'Ophthalmologist' },
          { step: 'Document findings', owner: 'Ophthalmologist' },
          { step: 'Prescribe glasses/treatment', owner: 'Ophthalmologist' },
        ],
      },
    ],
  },
  {
    icon: Bone,
    name: 'Orthopedics',
    description: 'MSK exams, fractures, surgery tracking, and rehabilitation.',
    journeyCount: 4,
    stepCount: 30,
    journeys: [
      {
        name: 'Fracture Management',
        steps: [
          { step: 'Review X-rays', owner: 'Orthopedic Surgeon' },
          { step: 'Assess fracture type', owner: 'Orthopedic Surgeon' },
          { step: 'Determine treatment approach', owner: 'Orthopedic Surgeon' },
          { step: 'Explain to patient', owner: 'Orthopedic Surgeon' },
          { step: 'Apply cast/splint or schedule surgery', owner: 'Orthopedic Surgeon' },
          { step: 'Document treatment', owner: 'Orthopedic Surgeon' },
          { step: 'Schedule follow-up', owner: 'Staff' },
          { step: 'Monitor healing progress', owner: 'Orthopedic Surgeon' },
        ],
      },
    ],
  },
  {
    icon: Baby,
    name: 'Pediatrics',
    description: 'Growth charts, milestones, vaccinations, and dosing calculators.',
    journeyCount: 4,
    stepCount: 30,
    journeys: [
      {
        name: 'Well-Child Visit',
        steps: [
          { step: 'Measure growth parameters', owner: 'Nurse' },
          { step: 'Plot on growth charts', owner: 'System' },
          { step: 'Assess developmental milestones', owner: 'Pediatrician' },
          { step: 'Conduct physical examination', owner: 'Pediatrician' },
          { step: 'Review vaccination schedule', owner: 'Pediatrician' },
          { step: 'Administer vaccines', owner: 'Nurse' },
          { step: 'Provide anticipatory guidance', owner: 'Pediatrician' },
          { step: 'Schedule next visit', owner: 'Staff' },
        ],
      },
    ],
  },
  {
    icon: Activity,
    name: 'OB/GYN',
    description: 'Pregnancy tracking, prenatal visits, ultrasound, and Pap smears.',
    journeyCount: 5,
    stepCount: 40,
    journeys: [
      {
        name: 'Prenatal Visit',
        steps: [
          { step: 'Record weight and blood pressure', owner: 'Nurse' },
          { step: 'Measure fundal height', owner: 'OB/GYN' },
          { step: 'Listen to fetal heart tones', owner: 'OB/GYN' },
          { step: 'Review lab results', owner: 'OB/GYN' },
          { step: 'Perform ultrasound if scheduled', owner: 'OB/GYN' },
          { step: 'Assess fetal movement', owner: 'OB/GYN' },
          { step: 'Discuss concerns', owner: 'OB/GYN' },
          { step: 'Update pregnancy record', owner: 'OB/GYN' },
          { step: 'Schedule next appointment', owner: 'Staff' },
        ],
      },
    ],
  },
  {
    icon: Brain,
    name: 'Oncology',
    description: 'Cancer staging, chemotherapy, radiation, and tumor markers.',
    journeyCount: 5,
    stepCount: 45,
    journeys: [
      {
        name: 'Chemotherapy Administration',
        steps: [
          { step: 'Verify patient identity', owner: 'Oncology Nurse' },
          { step: 'Review treatment protocol', owner: 'Oncology Nurse' },
          { step: 'Check lab values', owner: 'Oncologist' },
          { step: 'Prepare chemotherapy agents', owner: 'Pharmacist' },
          { step: 'Verify drug, dose, route', owner: 'Oncology Nurse' },
          { step: 'Establish IV access', owner: 'Oncology Nurse' },
          { step: 'Administer pre-medications', owner: 'Oncology Nurse' },
          { step: 'Infuse chemotherapy', owner: 'Oncology Nurse' },
          { step: 'Monitor for reactions', owner: 'Oncology Nurse' },
          { step: 'Document administration', owner: 'Oncology Nurse' },
        ],
      },
    ],
  },
  {
    icon: Droplet,
    name: 'Dialysis',
    description: 'Hemodialysis sessions, intradialytic monitoring, and scheduling.',
    journeyCount: 4,
    stepCount: 35,
    journeys: [
      {
        name: 'Hemodialysis Session',
        steps: [
          { step: 'Weigh patient (pre-dialysis)', owner: 'Dialysis Nurse' },
          { step: 'Record vital signs', owner: 'Dialysis Nurse' },
          { step: 'Assess vascular access', owner: 'Dialysis Nurse' },
          { step: 'Cannulate access', owner: 'Dialysis Nurse' },
          { step: 'Connect to dialysis machine', owner: 'Dialysis Nurse' },
          { step: 'Set dialysis parameters', owner: 'Dialysis Nurse' },
          { step: 'Monitor during treatment', owner: 'Dialysis Nurse' },
          { step: 'Record intradialytic vitals', owner: 'Dialysis Nurse' },
          { step: 'Complete dialysis', owner: 'Dialysis Nurse' },
          { step: 'Weigh patient (post-dialysis)', owner: 'Dialysis Nurse' },
          { step: 'Document session', owner: 'Dialysis Nurse' },
        ],
      },
    ],
  },
];

function ModuleCard({ module, expanded, onToggle }: { module: Module; expanded: boolean; onToggle: () => void }) {
  return (
    <div className="card">
      <div
        className="flex items-start gap-4 cursor-pointer"
        onClick={onToggle}
      >
        <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
          <module.icon className="h-6 w-6" />
        </div>
        <div className="flex-1">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-gray-900">{module.name}</h3>
            {expanded ? (
              <ChevronUp className="h-5 w-5 text-gray-400" />
            ) : (
              <ChevronDown className="h-5 w-5 text-gray-400" />
            )}
          </div>
          <p className="text-sm text-gray-600 mt-1">{module.description}</p>
          <div className="flex gap-4 mt-2">
            <span className="text-xs bg-primary-50 text-primary-700 px-2 py-1 rounded">
              {module.journeyCount} journeys
            </span>
            <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">
              {module.stepCount} steps
            </span>
          </div>
        </div>
      </div>

      {expanded && module.journeys.length > 0 && (
        <div className="mt-6 border-t pt-4">
          {module.journeys.map((journey, idx) => (
            <div key={idx} className="mb-4 last:mb-0">
              <h4 className="font-medium text-gray-800 mb-2">{journey.name}</h4>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-gray-50">
                      <th className="text-left px-3 py-2 font-medium text-gray-600">Step</th>
                      <th className="text-left px-3 py-2 font-medium text-gray-600">Description</th>
                      <th className="text-left px-3 py-2 font-medium text-gray-600">Owner</th>
                    </tr>
                  </thead>
                  <tbody>
                    {journey.steps.map((step, stepIdx) => (
                      <tr key={stepIdx} className="border-b border-gray-100">
                        <td className="px-3 py-2 text-gray-500">{stepIdx + 1}</td>
                        <td className="px-3 py-2 text-gray-700">{step.step}</td>
                        <td className="px-3 py-2">
                          <span className="inline-block bg-blue-50 text-blue-700 px-2 py-0.5 rounded text-xs">
                            {step.owner}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default function ModuleJourneysPage() {
  const [expandedCore, setExpandedCore] = useState<number | null>(null);
  const [expandedSpecialty, setExpandedSpecialty] = useState<number | null>(null);

  const totalJourneys = [...coreModules, ...specialtyModules].reduce((acc, m) => acc + m.journeyCount, 0);
  const totalSteps = [...coreModules, ...specialtyModules].reduce((acc, m) => acc + m.stepCount, 0);

  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Documentation</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Module User Journeys
            </h1>
            <p className="text-lg md:text-xl text-gray-600">
              Comprehensive documentation of all user journeys across XenonClinic modules,
              including detailed steps and step owners.
            </p>
            <div className="flex justify-center gap-6 mt-8">
              <div className="text-center">
                <div className="text-4xl font-bold text-primary-600">{coreModules.length + specialtyModules.length}</div>
                <div className="text-sm text-gray-500">Modules</div>
              </div>
              <div className="text-center">
                <div className="text-4xl font-bold text-primary-600">{totalJourneys}</div>
                <div className="text-sm text-gray-500">User Journeys</div>
              </div>
              <div className="text-center">
                <div className="text-4xl font-bold text-primary-600">{totalSteps}</div>
                <div className="text-sm text-gray-500">Total Steps</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Core Modules */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Core Modules</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Essential modules for complete healthcare and business operations management.
            </p>
          </div>

          <div className="grid lg:grid-cols-2 gap-6">
            {coreModules.map((module, idx) => (
              <ModuleCard
                key={module.name}
                module={module}
                expanded={expandedCore === idx}
                onToggle={() => setExpandedCore(expandedCore === idx ? null : idx)}
              />
            ))}
          </div>
        </div>
      </section>

      {/* Specialty Modules */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Specialty Modules</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Specialized modules for different medical departments and specialties.
            </p>
          </div>

          <div className="grid lg:grid-cols-2 gap-6">
            {specialtyModules.map((module, idx) => (
              <ModuleCard
                key={module.name}
                module={module}
                expanded={expandedSpecialty === idx}
                onToggle={() => setExpandedSpecialty(expandedSpecialty === idx ? null : idx)}
              />
            ))}
          </div>
        </div>
      </section>

      {/* Role Reference */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Role Reference</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Understanding the different roles and their responsibilities in the system.
            </p>
          </div>

          <div className="max-w-4xl mx-auto">
            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
              {[
                { role: 'Receptionist', desc: 'Front desk staff handling patient registration and appointments' },
                { role: 'Nurse', desc: 'Clinical staff assisting with patient care' },
                { role: 'Doctor', desc: 'Physician providing medical care' },
                { role: 'Lab Technician', desc: 'Staff performing laboratory tests' },
                { role: 'Pharmacist', desc: 'Licensed professional dispensing medications' },
                { role: 'Billing Staff', desc: 'Staff handling invoicing and payments' },
                { role: 'HR Staff', desc: 'Human resources personnel' },
                { role: 'Manager', desc: 'Department or team manager' },
                { role: 'System Admin', desc: 'Technical administrator' },
                { role: 'Patient', desc: 'End user of patient portal' },
                { role: 'Auditor', desc: 'Security and compliance reviewer' },
                { role: 'System', desc: 'Automated system actions' },
              ].map((item) => (
                <div key={item.role} className="bg-gray-50 rounded-lg p-4">
                  <div className="font-medium text-gray-900">{item.role}</div>
                  <div className="text-sm text-gray-600 mt-1">{item.desc}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">Ready to streamline your workflows?</h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your free trial and experience all these modules with your own data.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link to="/features" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              View All Features
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
