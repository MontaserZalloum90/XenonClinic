import { useState } from 'react';
import { BookOpen, Search } from 'lucide-react';

interface GlossaryTerm {
  term: string;
  definition: string;
  category: string;
  relatedTerms?: string[];
}

const glossaryTerms: GlossaryTerm[] = [
  // A
  {
    term: 'Access Control List (ACL)',
    definition: 'A list of permissions attached to a resource specifying which users or system processes can access it and what operations they can perform.',
    category: 'Security',
    relatedTerms: ['RBAC', 'Permission'],
  },
  {
    term: 'Accession Number',
    definition: 'A unique identifier assigned to a laboratory sample when it is received. Used to track the sample through testing and reporting.',
    category: 'Laboratory',
    relatedTerms: ['Specimen', 'Lab Order'],
  },
  {
    term: 'Active Directory (AD)',
    definition: 'Microsoft directory service used for single sign-on (SSO) integration, allowing users to authenticate with corporate credentials.',
    category: 'Security',
    relatedTerms: ['SSO', 'LDAP'],
  },
  {
    term: 'Adjudication',
    definition: 'The process by which an insurance company reviews a claim and decides whether to pay, deny, or request more information.',
    category: 'Billing',
    relatedTerms: ['Insurance Claim', 'ERA'],
  },
  {
    term: 'Adverse Event',
    definition: 'An unexpected medical problem that occurs during treatment. Must be documented and reported per regulatory requirements.',
    category: 'Clinical',
    relatedTerms: ['Incident Report', 'Patient Safety'],
  },
  {
    term: 'Allergy',
    definition: 'A documented patient sensitivity to medications, foods, or environmental factors. Triggers alerts during prescribing and treatment.',
    category: 'Clinical',
    relatedTerms: ['Drug Interaction', 'Alert'],
  },
  {
    term: 'Appointment Slot',
    definition: 'A specific time block reserved for patient consultations. Slots are configured per provider and can have different durations based on appointment type.',
    category: 'Appointments',
    relatedTerms: ['Provider Schedule', 'Booking'],
  },
  {
    term: 'Appointment Status',
    definition: 'The current state of an appointment: Scheduled, Confirmed, Checked-In, In Progress, Completed, Cancelled, or No-Show.',
    category: 'Appointments',
    relatedTerms: ['Appointment Slot', 'Check-In'],
  },
  {
    term: 'Audit Log',
    definition: 'A chronological record of system activities including user actions, data access, and changes. Audit logs are retained for 7 years for compliance.',
    category: 'Security',
    relatedTerms: ['Compliance', 'HIPAA'],
  },
  {
    term: 'Audit Trail',
    definition: 'Complete history of all changes made to a record, including who made the change, when, and what was modified.',
    category: 'Security',
    relatedTerms: ['Audit Log', 'Compliance'],
  },
  {
    term: 'Audiogram',
    definition: 'A graph showing hearing test results at different frequencies. Records air conduction, bone conduction, and speech recognition thresholds.',
    category: 'Audiology',
  },
  {
    term: 'Audiometry',
    definition: 'The science of measuring hearing acuity. Includes pure tone audiometry, speech audiometry, and impedance audiometry.',
    category: 'Audiology',
    relatedTerms: ['Audiogram', 'Hearing Test'],
  },
  {
    term: 'Authorization',
    definition: 'Pre-approval from an insurance company required before certain procedures, tests, or specialist visits can be performed.',
    category: 'Billing',
    relatedTerms: ['Prior Authorization', 'Insurance'],
  },
  {
    term: 'Auto-Posting',
    definition: 'Automatic application of payments to patient accounts based on remittance advice (ERA) from insurance companies.',
    category: 'Billing',
    relatedTerms: ['ERA', 'Payment Posting'],
  },
  // B
  {
    term: 'Backorder',
    definition: 'An inventory item that is currently out of stock but has been ordered from a supplier and is awaiting delivery.',
    category: 'Inventory',
    relatedTerms: ['Stock Level', 'Reorder Point'],
  },
  {
    term: 'Batch Processing',
    definition: 'Processing multiple records or transactions together as a group, such as batch claim submission or batch payment posting.',
    category: 'System',
    relatedTerms: ['Auto-Posting', 'Claim Submission'],
  },
  {
    term: 'Biometric Authentication',
    definition: 'Identity verification using physical characteristics such as fingerprints or facial recognition for secure login.',
    category: 'Security',
    relatedTerms: ['MFA', 'Authentication'],
  },
  {
    term: 'Branch',
    definition: 'A physical location or clinic within a tenant organization. Each branch can have its own staff, schedules, and inventory.',
    category: 'Multi-Tenancy',
    relatedTerms: ['Tenant', 'Location'],
  },
  {
    term: 'Break the Glass',
    definition: 'Emergency access protocol allowing authorized users to bypass normal access restrictions to view patient data. Requires documented justification and triggers compliance review.',
    category: 'Security',
    relatedTerms: ['Emergency Access', 'PHI'],
  },
  {
    term: 'Business Associate Agreement (BAA)',
    definition: 'A contract between a healthcare provider and a vendor that handles PHI, ensuring HIPAA compliance.',
    category: 'Compliance',
    relatedTerms: ['HIPAA', 'PHI'],
  },
  // C
  {
    term: 'Care Plan',
    definition: 'A documented treatment plan outlining goals, interventions, and expected outcomes for a patient\'s care over time.',
    category: 'Clinical',
    relatedTerms: ['Treatment Plan', 'Care Coordination'],
  },
  {
    term: 'Chart',
    definition: 'The complete medical record for a patient, including all encounters, diagnoses, medications, and test results.',
    category: 'Clinical',
    relatedTerms: ['EHR', 'Medical Record'],
  },
  {
    term: 'Check-In',
    definition: 'The process of registering a patient\'s arrival for their appointment, verifying demographics and insurance information.',
    category: 'Appointments',
    relatedTerms: ['Registration', 'Appointment Status'],
  },
  {
    term: 'Chief Complaint',
    definition: 'The primary reason a patient seeks medical attention, documented in their own words or as described to the provider.',
    category: 'Clinical',
    relatedTerms: ['SOAP Note', 'Clinical Visit'],
  },
  {
    term: 'Claim Denial',
    definition: 'Rejection of an insurance claim by the payer, requiring correction and resubmission or appeal.',
    category: 'Billing',
    relatedTerms: ['Adjudication', 'Appeal'],
  },
  {
    term: 'Claim Scrubbing',
    definition: 'Automated validation of claims before submission to identify errors and improve first-pass acceptance rate.',
    category: 'Billing',
    relatedTerms: ['Claim Submission', 'Clearinghouse'],
  },
  {
    term: 'Clearinghouse',
    definition: 'A third-party service that receives claims from providers, reformats them to payer specifications, and transmits them electronically.',
    category: 'Billing',
    relatedTerms: ['Claim Submission', 'EDI'],
  },
  {
    term: 'Clinical Decision Support (CDS)',
    definition: 'System features that provide alerts, reminders, and recommendations to help clinicians make informed treatment decisions.',
    category: 'Clinical',
    relatedTerms: ['Drug Interaction', 'Alert'],
  },
  {
    term: 'Clinical Visit',
    definition: 'A documented patient encounter including vitals, examination, diagnosis, and treatment plan. Forms the core of the patient medical record.',
    category: 'Clinical',
  },
  {
    term: 'CMS-1500',
    definition: 'The standard paper claim form used by healthcare providers to bill insurance companies for professional services.',
    category: 'Billing',
    relatedTerms: ['UB-04', 'Claim'],
  },
  {
    term: 'Co-Insurance',
    definition: 'The percentage of costs a patient pays after meeting their deductible, typically 20% of allowed charges.',
    category: 'Billing',
    relatedTerms: ['Co-Pay', 'Deductible'],
  },
  {
    term: 'Co-Pay',
    definition: 'A fixed amount a patient pays at the time of service, as specified by their insurance plan.',
    category: 'Billing',
    relatedTerms: ['Co-Insurance', 'Patient Responsibility'],
  },
  {
    term: 'Controlled Substance',
    definition: 'Medications regulated by the DEA due to potential for abuse. Require special prescribing protocols and tracking.',
    category: 'Pharmacy',
    relatedTerms: ['DEA Number', 'EPCS'],
  },
  {
    term: 'CPT Code',
    definition: 'Current Procedural Terminology code used to describe medical, surgical, and diagnostic services for billing purposes.',
    category: 'Billing',
    relatedTerms: ['ICD Code', 'Claim'],
  },
  {
    term: 'Critical Value',
    definition: 'A lab result that indicates a potentially life-threatening condition requiring immediate notification of the ordering provider.',
    category: 'Laboratory',
    relatedTerms: ['Lab Result', 'Alert'],
  },
  // D
  {
    term: 'Dashboard',
    definition: 'A personalized home screen showing key metrics, pending tasks, and quick actions relevant to the user\'s role.',
    category: 'General',
  },
  {
    term: 'Data Classification',
    definition: 'Categorization of data by sensitivity level (Public, Internal, Confidential, PHI/Restricted) to apply appropriate security controls.',
    category: 'Security',
    relatedTerms: ['PHI', 'Encryption'],
  },
  {
    term: 'Data Retention',
    definition: 'Policies defining how long different types of data must be stored before archival or deletion. Medical records typically require 7+ years.',
    category: 'Compliance',
    relatedTerms: ['Archive', 'Audit Log'],
  },
  {
    term: 'Day Sheet',
    definition: 'A daily summary report showing all scheduled appointments, completed visits, and financial transactions for a provider or branch.',
    category: 'Reports',
    relatedTerms: ['Dashboard', 'Revenue Report'],
  },
  {
    term: 'DEA Number',
    definition: 'Drug Enforcement Administration registration number required for prescribing controlled substances. Verified during provider setup.',
    category: 'Pharmacy',
    relatedTerms: ['Controlled Substance', 'Prescription'],
  },
  {
    term: 'Deductible',
    definition: 'The amount a patient must pay out-of-pocket before insurance coverage begins for the plan year.',
    category: 'Billing',
    relatedTerms: ['Co-Pay', 'Co-Insurance'],
  },
  {
    term: 'Demographics',
    definition: 'Patient personal information including name, date of birth, address, phone, email, and emergency contacts.',
    category: 'Patient',
    relatedTerms: ['Registration', 'Profile'],
  },
  {
    term: 'Diagnosis',
    definition: 'The identification of a patient\'s medical condition, documented using ICD codes for billing and reporting.',
    category: 'Clinical',
    relatedTerms: ['ICD Code', 'Assessment'],
  },
  {
    term: 'Discharge Summary',
    definition: 'Documentation provided when a patient is released from care, summarizing treatment, medications, and follow-up instructions.',
    category: 'Clinical',
    relatedTerms: ['Clinical Visit', 'Care Plan'],
  },
  {
    term: 'Dispensing',
    definition: 'The act of preparing and providing medications to patients, including labeling and counseling.',
    category: 'Pharmacy',
    relatedTerms: ['Prescription', 'Medication'],
  },
  {
    term: 'DOH',
    definition: 'Department of Health - regulatory body overseeing healthcare facilities and licensing in a jurisdiction (e.g., DOH UAE).',
    category: 'Compliance',
    relatedTerms: ['License', 'Accreditation'],
  },
  {
    term: 'Drug Interaction',
    definition: 'A situation where one medication affects the activity of another. The system alerts prescribers to potential interactions.',
    category: 'Pharmacy',
    relatedTerms: ['Allergy', 'Alert'],
  },
  // E
  {
    term: 'E-Prescribing',
    definition: 'Electronic transmission of prescriptions directly from the provider to the pharmacy, reducing errors and improving efficiency.',
    category: 'Pharmacy',
    relatedTerms: ['EPCS', 'Prescription'],
  },
  {
    term: 'EDI',
    definition: 'Electronic Data Interchange - standardized format for exchanging healthcare transactions like claims and eligibility checks.',
    category: 'System',
    relatedTerms: ['Clearinghouse', 'HL7'],
  },
  {
    term: 'EHR',
    definition: 'Electronic Health Record - digital version of a patient\'s medical history maintained by the healthcare provider.',
    category: 'General',
  },
  {
    term: 'Eligibility Verification',
    definition: 'Real-time check with insurance companies to confirm a patient\'s coverage status, benefits, and co-pay amounts.',
    category: 'Billing',
    relatedTerms: ['Insurance', 'Benefits'],
  },
  {
    term: 'Encounter',
    definition: 'Any interaction between a patient and healthcare provider, including visits, phone calls, and telehealth sessions.',
    category: 'Clinical',
  },
  {
    term: 'Encryption',
    definition: 'The process of encoding data so only authorized parties can access it. XenonClinic uses AES-256 encryption for data at rest.',
    category: 'Security',
    relatedTerms: ['PHI', 'Data Protection'],
  },
  {
    term: 'EOB',
    definition: 'Explanation of Benefits - statement from insurance explaining how a claim was processed, what was paid, and patient responsibility.',
    category: 'Billing',
    relatedTerms: ['ERA', 'Adjudication'],
  },
  {
    term: 'EPCS',
    definition: 'Electronic Prescribing for Controlled Substances - DEA-compliant system for e-prescribing medications with abuse potential.',
    category: 'Pharmacy',
    relatedTerms: ['E-Prescribing', 'Controlled Substance'],
  },
  {
    term: 'ERA',
    definition: 'Electronic Remittance Advice - electronic version of an EOB containing payment information for automatic posting.',
    category: 'Billing',
    relatedTerms: ['EOB', 'Auto-Posting'],
  },
  // F
  {
    term: 'Face Sheet',
    definition: 'A summary document containing key patient information including demographics, insurance, allergies, and current medications.',
    category: 'Patient',
    relatedTerms: ['Demographics', 'Chart'],
  },
  {
    term: 'Fee Schedule',
    definition: 'A list of charges for medical services. Multiple fee schedules can be configured for different payers or service types.',
    category: 'Billing',
    relatedTerms: ['Charge', 'Contract Rate'],
  },
  {
    term: 'FIFO',
    definition: 'First In, First Out - inventory management method where oldest stock is used first to prevent expiration.',
    category: 'Inventory',
    relatedTerms: ['Lot Number', 'Expiration Date'],
  },
  {
    term: 'Fiscal Year',
    definition: 'The 12-month period used for financial reporting and budgeting, which may differ from the calendar year.',
    category: 'Financial',
    relatedTerms: ['Budget', 'Financial Report'],
  },
  {
    term: 'Formulary',
    definition: 'A list of medications covered by an insurance plan or preferred by the healthcare organization.',
    category: 'Pharmacy',
    relatedTerms: ['Medication', 'Insurance'],
  },
  // G
  {
    term: 'GDPR',
    definition: 'General Data Protection Regulation - European Union privacy law governing the collection and processing of personal data.',
    category: 'Compliance',
    relatedTerms: ['Privacy', 'Data Protection'],
  },
  {
    term: 'General Ledger',
    definition: 'The main accounting record containing all financial transactions, organized by account codes.',
    category: 'Financial',
    relatedTerms: ['Chart of Accounts', 'Journal Entry'],
  },
  {
    term: 'Guarantor',
    definition: 'The person responsible for paying a patient\'s medical bills, which may be the patient or a parent/guardian.',
    category: 'Billing',
    relatedTerms: ['Patient Responsibility', 'Statement'],
  },
  // H
  {
    term: 'HCPCS',
    definition: 'Healthcare Common Procedure Coding System - codes used for billing Medicare and Medicaid for supplies, equipment, and services.',
    category: 'Billing',
    relatedTerms: ['CPT Code', 'Billing Code'],
  },
  {
    term: 'Health Information Exchange (HIE)',
    definition: 'The electronic sharing of health information between different healthcare organizations to coordinate care.',
    category: 'System',
    relatedTerms: ['Interoperability', 'FHIR'],
  },
  {
    term: 'Hearing Aid',
    definition: 'An electronic device worn to amplify sound for patients with hearing loss. Tracked in audiology module with fitting and adjustment records.',
    category: 'Audiology',
    relatedTerms: ['Audiogram', 'Fitting'],
  },
  {
    term: 'HIPAA',
    definition: 'Health Insurance Portability and Accountability Act - US legislation providing data privacy and security provisions for safeguarding medical information.',
    category: 'Compliance',
    relatedTerms: ['PHI', 'Audit Log'],
  },
  {
    term: 'HL7',
    definition: 'Health Level Seven - international standards for exchanging clinical and administrative data between healthcare systems.',
    category: 'System',
    relatedTerms: ['FHIR', 'Integration'],
  },
  {
    term: 'Hold',
    definition: 'A temporary restriction on a patient account preventing certain actions, such as appointment scheduling or prescription refills.',
    category: 'Patient',
    relatedTerms: ['Account Status', 'Alert'],
  },
  // I
  {
    term: 'ICD Code',
    definition: 'International Classification of Diseases code used to classify diagnoses and health conditions for billing and statistics.',
    category: 'Billing',
    relatedTerms: ['CPT Code', 'Diagnosis'],
  },
  {
    term: 'Immunization',
    definition: 'Vaccination records including vaccine type, date administered, lot number, and site. Can be reported to registries.',
    category: 'Clinical',
    relatedTerms: ['Vaccine', 'Registry'],
  },
  {
    term: 'Incident Report',
    definition: 'Documentation of unexpected events affecting patient safety, staff safety, or operations. Triggers review and follow-up.',
    category: 'Clinical',
    relatedTerms: ['Adverse Event', 'Quality'],
  },
  {
    term: 'Insurance Claim',
    definition: 'A request submitted to an insurance company for payment of healthcare services rendered to a patient.',
    category: 'Billing',
    relatedTerms: ['ERA', 'Adjudication'],
  },
  {
    term: 'Integration Engine',
    definition: 'Software component that connects XenonClinic with external systems like labs, pharmacies, and billing clearinghouses.',
    category: 'System',
    relatedTerms: ['API', 'HL7'],
  },
  {
    term: 'Inventory Adjustment',
    definition: 'A manual change to inventory quantities to correct discrepancies found during counts or audits.',
    category: 'Inventory',
    relatedTerms: ['Stock Count', 'Variance'],
  },
  {
    term: 'ISO 27001',
    definition: 'International standard for information security management systems, specifying requirements for data protection.',
    category: 'Compliance',
    relatedTerms: ['Security', 'Certification'],
  },
  // J
  {
    term: 'Journal Entry',
    definition: 'A record of a financial transaction in the accounting system, including date, accounts, and amounts.',
    category: 'Financial',
    relatedTerms: ['General Ledger', 'Transaction'],
  },
  {
    term: 'Journey',
    definition: 'A documented workflow showing the step-by-step process for completing a common task, from start to finish.',
    category: 'General',
  },
  // K
  {
    term: 'KPI',
    definition: 'Key Performance Indicator - a measurable value that demonstrates how effectively objectives are being achieved.',
    category: 'Reports',
    relatedTerms: ['Dashboard', 'Analytics'],
  },
  // L
  {
    term: 'Lab Interface',
    definition: 'Electronic connection between XenonClinic and laboratory equipment or external lab systems for automated result delivery.',
    category: 'Laboratory',
    relatedTerms: ['Integration', 'HL7'],
  },
  {
    term: 'Lab Order',
    definition: 'A request for laboratory tests on patient samples. Includes specimen requirements, priority level, and ordering provider.',
    category: 'Laboratory',
    relatedTerms: ['Lab Result', 'Sample'],
  },
  {
    term: 'Lab Result',
    definition: 'The outcome of laboratory analysis, including values, reference ranges, and interpretation notes.',
    category: 'Laboratory',
  },
  {
    term: 'LDAP',
    definition: 'Lightweight Directory Access Protocol - used for integrating with corporate directories for user authentication.',
    category: 'Security',
    relatedTerms: ['Active Directory', 'SSO'],
  },
  {
    term: 'License',
    definition: 'Professional credentials required for healthcare providers, including medical licenses, DEA registration, and specialty certifications.',
    category: 'HR',
    relatedTerms: ['Credentialing', 'Provider'],
  },
  {
    term: 'Localization',
    definition: 'Adapting the system for different languages, regions, and cultural preferences including date formats and RTL support.',
    category: 'System',
    relatedTerms: ['Language', 'RTL'],
  },
  {
    term: 'Lot Number',
    definition: 'A unique identifier assigned by manufacturers to track batches of medications or supplies for recall purposes.',
    category: 'Inventory',
    relatedTerms: ['FIFO', 'Expiration Date'],
  },
  // M
  {
    term: 'Medication List',
    definition: 'A comprehensive list of all medications a patient is currently taking, including prescriptions and over-the-counter drugs.',
    category: 'Clinical',
    relatedTerms: ['Prescription', 'Reconciliation'],
  },
  {
    term: 'Medication Reconciliation',
    definition: 'The process of comparing a patient\'s medication list with new prescriptions to identify and resolve discrepancies.',
    category: 'Clinical',
    relatedTerms: ['Medication List', 'Safety'],
  },
  {
    term: 'MFA',
    definition: 'Multi-Factor Authentication - security mechanism requiring two or more verification factors to access an account.',
    category: 'Security',
  },
  {
    term: 'Modifier',
    definition: 'A two-character code added to CPT codes to provide additional information about the service performed.',
    category: 'Billing',
    relatedTerms: ['CPT Code', 'Claim'],
  },
  {
    term: 'Module',
    definition: 'A functional component of XenonClinic that handles a specific area of clinic operations (e.g., Appointments, Laboratory, Billing).',
    category: 'General',
  },
  // N
  {
    term: 'NPI',
    definition: 'National Provider Identifier - a unique 10-digit number assigned to healthcare providers for billing and identification.',
    category: 'Billing',
    relatedTerms: ['Provider', 'Credentialing'],
  },
  {
    term: 'Notification',
    definition: 'System alerts delivered via in-app messages, email, or SMS to inform users of important events or required actions.',
    category: 'System',
    relatedTerms: ['Alert', 'Workflow'],
  },
  {
    term: 'Nurse Station',
    definition: 'A specialized dashboard for nursing staff showing patient queues, vitals collection tasks, and medication schedules.',
    category: 'Clinical',
    relatedTerms: ['Dashboard', 'Workflow'],
  },
  // O
  {
    term: 'Order Set',
    definition: 'A predefined group of orders (labs, medications, procedures) that can be placed together for common clinical scenarios.',
    category: 'Clinical',
    relatedTerms: ['Protocol', 'Lab Order'],
  },
  {
    term: 'Out-of-Network',
    definition: 'Healthcare services provided by providers not contracted with the patient\'s insurance plan, typically resulting in higher costs.',
    category: 'Billing',
    relatedTerms: ['In-Network', 'Insurance'],
  },
  {
    term: 'Outbound Referral',
    definition: 'A referral from your clinic to an external specialist or facility, including clinical notes and reason for referral.',
    category: 'Clinical',
    relatedTerms: ['Referral', 'Care Coordination'],
  },
  // P
  {
    term: 'PACS',
    definition: 'Picture Archiving and Communication System - medical imaging storage and retrieval system integrated with radiology module.',
    category: 'Radiology',
    relatedTerms: ['DICOM', 'Imaging'],
  },
  {
    term: 'Patient Balance',
    definition: 'The total amount owed by a patient after insurance payments, including co-pays, deductibles, and non-covered services.',
    category: 'Billing',
    relatedTerms: ['Statement', 'Patient Responsibility'],
  },
  {
    term: 'Patient Portal',
    definition: 'Secure online platform where patients can view their health information, schedule appointments, and communicate with providers.',
    category: 'Patient',
  },
  {
    term: 'Payment Plan',
    definition: 'An arrangement allowing patients to pay their balance over time with scheduled installments.',
    category: 'Billing',
    relatedTerms: ['Patient Balance', 'Collection'],
  },
  {
    term: 'Permission',
    definition: 'A specific capability granted to a role, such as "patients:view" or "appointments:create". Controls access to features and data.',
    category: 'Security',
    relatedTerms: ['Role', 'RBAC'],
  },
  {
    term: 'PHI',
    definition: 'Protected Health Information - any individually identifiable health information that is transmitted or maintained in any form.',
    category: 'Compliance',
    relatedTerms: ['HIPAA', 'Privacy'],
  },
  {
    term: 'Point of Service (POS)',
    definition: 'Payment collection at the time of patient visit, typically for co-pays, deductibles, or self-pay services.',
    category: 'Billing',
    relatedTerms: ['Co-Pay', 'Payment'],
  },
  {
    term: 'Practice Management',
    definition: 'Administrative functions including scheduling, billing, and reporting that support clinical operations.',
    category: 'General',
    relatedTerms: ['EHR', 'Module'],
  },
  {
    term: 'Prior Authorization',
    definition: 'Pre-approval required from insurance before certain services can be performed. Failure to obtain may result in claim denial.',
    category: 'Billing',
    relatedTerms: ['Authorization', 'Referral'],
  },
  {
    term: 'Problem List',
    definition: 'A maintained list of a patient\'s active and resolved medical conditions, serving as a summary of their health history.',
    category: 'Clinical',
    relatedTerms: ['Diagnosis', 'Chart'],
  },
  {
    term: 'Procedure',
    definition: 'A medical treatment or intervention performed on a patient, documented with CPT codes for billing.',
    category: 'Clinical',
    relatedTerms: ['CPT Code', 'Treatment'],
  },
  {
    term: 'Provider',
    definition: 'A healthcare professional who delivers clinical services, including physicians, nurses, and specialists.',
    category: 'Clinical',
  },
  {
    term: 'Provider Schedule',
    definition: 'The calendar showing a provider\'s available appointment slots, blocked times, and scheduled patients.',
    category: 'Appointments',
    relatedTerms: ['Appointment Slot', 'Availability'],
  },
  {
    term: 'Purchase Order',
    definition: 'A formal document authorizing the purchase of inventory items from a supplier at specified prices and quantities.',
    category: 'Inventory',
    relatedTerms: ['Supplier', 'Procurement'],
  },
  // Q
  {
    term: 'Queue',
    definition: 'A waiting list for patients at various stages of their visit, such as check-in queue, triage queue, or provider queue.',
    category: 'Appointments',
    relatedTerms: ['Check-In', 'Workflow'],
  },
  {
    term: 'Quick Text',
    definition: 'Predefined text snippets that can be inserted into clinical notes or messages to save time and ensure consistency.',
    category: 'Clinical',
    relatedTerms: ['Template', 'SOAP Note'],
  },
  // R
  {
    term: 'Radiology Order',
    definition: 'A request for imaging studies such as X-rays, CT scans, MRIs, or ultrasounds.',
    category: 'Radiology',
    relatedTerms: ['PACS', 'Imaging'],
  },
  {
    term: 'RBAC',
    definition: 'Role-Based Access Control - security approach where permissions are assigned to roles, and users are assigned to roles.',
    category: 'Security',
    relatedTerms: ['Permission', 'Role'],
  },
  {
    term: 'Recall',
    definition: 'A system for tracking patients who need follow-up appointments and sending reminders to schedule.',
    category: 'Patient',
    relatedTerms: ['Follow-Up', 'Reminder'],
  },
  {
    term: 'Reconciliation',
    definition: 'The process of comparing records to identify and resolve discrepancies, such as payment reconciliation or inventory reconciliation.',
    category: 'Financial',
    relatedTerms: ['Audit', 'Verification'],
  },
  {
    term: 'Referral',
    definition: 'A recommendation from one provider to another for specialized care or services for a patient.',
    category: 'Clinical',
  },
  {
    term: 'Refill',
    definition: 'A request to renew an existing prescription. Can be initiated by patients through the portal or by calling the clinic.',
    category: 'Pharmacy',
    relatedTerms: ['Prescription', 'Medication'],
  },
  {
    term: 'Registration',
    definition: 'The process of creating a new patient record with demographics, insurance, and consent information.',
    category: 'Patient',
    relatedTerms: ['Demographics', 'Check-In'],
  },
  {
    term: 'Reorder Point',
    definition: 'The inventory level at which a new purchase order should be placed to avoid stockouts.',
    category: 'Inventory',
    relatedTerms: ['Stock Level', 'Backorder'],
  },
  {
    term: 'Report Builder',
    definition: 'A tool for creating custom reports by selecting data fields, filters, and formatting options.',
    category: 'Reports',
    relatedTerms: ['Analytics', 'Dashboard'],
  },
  {
    term: 'RIS',
    definition: 'Radiology Information System - manages radiology workflow including scheduling, reporting, and image distribution.',
    category: 'Radiology',
    relatedTerms: ['PACS', 'Radiology Order'],
  },
  {
    term: 'Role',
    definition: 'A defined set of permissions assigned to users based on their job function (e.g., Doctor, Nurse, Receptionist).',
    category: 'Security',
    relatedTerms: ['Permission', 'RBAC'],
  },
  {
    term: 'RTL',
    definition: 'Right-to-Left - text direction support for languages like Arabic and Hebrew, affecting interface layout and data entry.',
    category: 'System',
    relatedTerms: ['Localization', 'Language'],
  },
  // S
  {
    term: 'SAML',
    definition: 'Security Assertion Markup Language - XML-based standard for exchanging authentication data between identity providers.',
    category: 'Security',
    relatedTerms: ['SSO', 'Authentication'],
  },
  {
    term: 'Scheduling Template',
    definition: 'A predefined pattern of appointment slots that can be applied to provider schedules to standardize availability.',
    category: 'Appointments',
    relatedTerms: ['Provider Schedule', 'Appointment Slot'],
  },
  {
    term: 'Session Timeout',
    definition: 'Automatic logout after a period of inactivity to protect patient data. Configurable per security policy.',
    category: 'Security',
    relatedTerms: ['Authentication', 'Security'],
  },
  {
    term: 'SOAP Note',
    definition: 'Documentation format: Subjective (patient symptoms), Objective (exam findings), Assessment (diagnosis), Plan (treatment).',
    category: 'Clinical',
  },
  {
    term: 'Specimen',
    definition: 'A biological sample collected from a patient for laboratory analysis (blood, urine, tissue, etc.).',
    category: 'Laboratory',
    relatedTerms: ['Lab Order', 'Sample Collection'],
  },
  {
    term: 'SSO',
    definition: 'Single Sign-On - allows users to access multiple systems with one set of login credentials.',
    category: 'Security',
    relatedTerms: ['Active Directory', 'SAML'],
  },
  {
    term: 'Statement',
    definition: 'A document sent to patients showing their account balance, recent charges, payments, and amount due.',
    category: 'Billing',
    relatedTerms: ['Patient Balance', 'Invoice'],
  },
  {
    term: 'Stock Count',
    definition: 'Physical verification of inventory quantities, comparing actual counts to system records.',
    category: 'Inventory',
    relatedTerms: ['Inventory Adjustment', 'Variance'],
  },
  {
    term: 'Superbill',
    definition: 'An itemized form listing services provided during a patient visit, used to generate claims.',
    category: 'Billing',
    relatedTerms: ['Charge Capture', 'Claim'],
  },
  {
    term: 'Supplier',
    definition: 'A vendor who provides medications, supplies, or equipment to the clinic. Managed in procurement module.',
    category: 'Inventory',
    relatedTerms: ['Purchase Order', 'Procurement'],
  },
  // T
  {
    term: 'Task',
    definition: 'A work item assigned to a user or team, such as follow-up calls, document review, or prior authorization requests.',
    category: 'Workflow',
    relatedTerms: ['Workflow', 'Notification'],
  },
  {
    term: 'Telehealth',
    definition: 'Remote healthcare services delivered via video consultation, including virtual visits and remote monitoring.',
    category: 'Clinical',
    relatedTerms: ['Video Visit', 'Virtual Care'],
  },
  {
    term: 'Template',
    definition: 'A predefined document structure that can be used to create consistent clinical notes, letters, or forms.',
    category: 'Clinical',
    relatedTerms: ['Quick Text', 'SOAP Note'],
  },
  {
    term: 'Tenant',
    definition: 'An organization (clinic or hospital group) using XenonClinic. Each tenant has isolated data and can have multiple branches.',
    category: 'Multi-Tenancy',
    relatedTerms: ['Branch', 'Organization'],
  },
  {
    term: 'TLS',
    definition: 'Transport Layer Security - cryptographic protocol ensuring secure data transmission over networks. XenonClinic requires TLS 1.3.',
    category: 'Security',
    relatedTerms: ['Encryption', 'HTTPS'],
  },
  {
    term: 'TOTP',
    definition: 'Time-based One-Time Password - algorithm generating temporary codes for two-factor authentication.',
    category: 'Security',
    relatedTerms: ['MFA', 'Authenticator App'],
  },
  {
    term: 'Treatment Plan',
    definition: 'A documented approach for managing a patient\'s condition, including medications, therapies, and follow-up schedules.',
    category: 'Clinical',
    relatedTerms: ['Care Plan', 'Orders'],
  },
  {
    term: 'Triage',
    definition: 'The process of determining the priority of patient treatment based on the severity of their condition.',
    category: 'Clinical',
  },
  {
    term: 'Trigger',
    definition: 'An event that initiates a workflow, such as a new appointment booking or lab result arrival.',
    category: 'Workflow',
    relatedTerms: ['Workflow', 'Automation'],
  },
  // U
  {
    term: 'UB-04',
    definition: 'The standard paper claim form used by institutional providers (hospitals) to bill for facility services.',
    category: 'Billing',
    relatedTerms: ['CMS-1500', 'Claim'],
  },
  {
    term: 'Unbilled Charges',
    definition: 'Services that have been rendered but not yet submitted to insurance or billed to patients.',
    category: 'Billing',
    relatedTerms: ['Charge Capture', 'Revenue Cycle'],
  },
  {
    term: 'Unapplied Payment',
    definition: 'Money received that has not been allocated to specific charges, requiring manual posting.',
    category: 'Billing',
    relatedTerms: ['Payment Posting', 'Reconciliation'],
  },
  // V
  {
    term: 'Verification',
    definition: 'The process of confirming information accuracy, such as insurance eligibility verification or identity verification.',
    category: 'General',
    relatedTerms: ['Eligibility', 'Authentication'],
  },
  {
    term: 'Vitals',
    definition: 'Basic physiological measurements including blood pressure, heart rate, temperature, respiratory rate, and oxygen saturation.',
    category: 'Clinical',
  },
  {
    term: 'Void',
    definition: 'To cancel or nullify a transaction, such as voiding a charge or payment that was entered in error.',
    category: 'Billing',
    relatedTerms: ['Adjustment', 'Correction'],
  },
  // W
  {
    term: 'Waiting Room',
    definition: 'Virtual queue in telehealth module where patients wait before being connected to their provider for video visits.',
    category: 'Clinical',
    relatedTerms: ['Telehealth', 'Queue'],
  },
  {
    term: 'Worklist',
    definition: 'A filtered list of items requiring action, such as lab results to review or claims to process.',
    category: 'Workflow',
    relatedTerms: ['Task', 'Queue'],
  },
  {
    term: 'Workflow',
    definition: 'An automated sequence of tasks triggered by events, with conditional logic and notifications.',
    category: 'Workflow',
    relatedTerms: ['Automation', 'Trigger'],
  },
  {
    term: 'Workflow Engine',
    definition: 'The system component that executes automated workflows, evaluating conditions and triggering actions.',
    category: 'System',
    relatedTerms: ['Workflow', 'Automation'],
  },
  {
    term: 'Write-off',
    definition: 'The process of removing uncollectible amounts from patient accounts, typically requiring supervisor approval.',
    category: 'Billing',
  },
  // X
  {
    term: 'X12',
    definition: 'The standard format for electronic healthcare transactions including claims (837), eligibility (270/271), and payments (835).',
    category: 'System',
    relatedTerms: ['EDI', 'Clearinghouse'],
  },
  // Z
  {
    term: 'Zero Balance',
    definition: 'An account state where all charges have been paid or adjusted, with no remaining patient responsibility.',
    category: 'Billing',
    relatedTerms: ['Patient Balance', 'Statement'],
  },
];

const categories = Array.from(new Set(glossaryTerms.map((t) => t.category))).sort();

export default function Glossary() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const sortedTerms = [...glossaryTerms].sort((a, b) => a.term.localeCompare(b.term));

  const filteredTerms = sortedTerms.filter((term) => {
    const matchesSearch = searchQuery === '' ||
      term.term.toLowerCase().includes(searchQuery.toLowerCase()) ||
      term.definition.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === 'all' || term.category === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  // Group by first letter
  const groupedTerms = filteredTerms.reduce((acc, term) => {
    const letter = term.term[0].toUpperCase();
    if (!acc[letter]) {
      acc[letter] = [];
    }
    acc[letter].push(term);
    return acc;
  }, {} as Record<string, GlossaryTerm[]>);

  const letters = Object.keys(groupedTerms).sort();

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Glossary</h1>
        <p className="text-lg text-gray-600">
          Definitions of terms and concepts used throughout XenonClinic.
        </p>
      </div>

      {/* Search and Filter */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search terms..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="all">All Categories</option>
          {categories.map((category) => (
            <option key={category} value={category}>
              {category}
            </option>
          ))}
        </select>
      </div>

      {/* Letter Navigation */}
      <div className="flex flex-wrap gap-1">
        {Array.from('ABCDEFGHIJKLMNOPQRSTUVWXYZ').map((letter) => {
          const hasTerms = groupedTerms[letter]?.length > 0;
          return (
            <a
              key={letter}
              href={hasTerms ? `#letter-${letter}` : undefined}
              className={`w-8 h-8 flex items-center justify-center rounded text-sm font-medium ${
                hasTerms
                  ? 'bg-primary-100 text-primary-700 hover:bg-primary-200'
                  : 'bg-gray-100 text-gray-400 cursor-not-allowed'
              }`}
            >
              {letter}
            </a>
          );
        })}
      </div>

      {/* Terms List */}
      {letters.length === 0 ? (
        <div className="text-center py-12">
          <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-600">No terms found matching your search.</p>
        </div>
      ) : (
        <div className="space-y-8">
          {letters.map((letter) => (
            <section key={letter} id={`letter-${letter}`}>
              <h2 className="text-2xl font-bold text-primary-600 mb-4 border-b border-gray-200 pb-2">
                {letter}
              </h2>
              <div className="space-y-4">
                {groupedTerms[letter].map((item) => (
                  <div
                    key={item.term}
                    className="bg-white border border-gray-200 rounded-xl p-5"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h3 className="font-semibold text-gray-900 text-lg">
                          {item.term}
                        </h3>
                        <p className="text-gray-600 mt-1">{item.definition}</p>
                        {item.relatedTerms && item.relatedTerms.length > 0 && (
                          <div className="mt-3 flex items-center gap-2">
                            <span className="text-sm text-gray-500">Related:</span>
                            <div className="flex flex-wrap gap-1">
                              {item.relatedTerms.map((related) => (
                                <span
                                  key={related}
                                  className="px-2 py-0.5 bg-gray-100 text-gray-600 rounded text-sm"
                                >
                                  {related}
                                </span>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                      <span className="px-2.5 py-0.5 bg-primary-100 text-primary-700 rounded-full text-xs font-medium whitespace-nowrap">
                        {item.category}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </section>
          ))}
        </div>
      )}

      {/* Stats */}
      <div className="bg-gray-50 rounded-xl p-5 text-center">
        <p className="text-gray-600">
          <span className="font-semibold text-gray-900">{glossaryTerms.length}</span> terms across{' '}
          <span className="font-semibold text-gray-900">{categories.length}</span> categories
        </p>
      </div>
    </div>
  );
}
