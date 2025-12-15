import { useState } from "react";
import { Link } from "react-router-dom";
import {
  Settings,
  Building,
  Users,
  Globe,
  Bell,
  Shield,
  Database,
  Clock,
  Palette,
  Key,
  ChevronDown,
  ChevronRight,
  CheckCircle,
  AlertTriangle,
  Info,
  Server,
  FileText,
  Printer,
  Lock,
  RefreshCw,
  HardDrive,
  Zap,
  Headphones,
  Languages,
  Stethoscope,
  Archive,
  Eye,
  Calculator,
} from "lucide-react";

interface ConfigSetting {
  name: string;
  type: string;
  description: string;
  defaultValue?: string;
  required?: boolean;
  example?: string;
}

interface ConfigCategory {
  id: string;
  name: string;
  icon: React.ComponentType<{ className?: string }>;
  description: string;
  settings: ConfigSetting[];
  setupSteps?: string[];
  warnings?: string[];
  tips?: string[];
}

const configCategories: ConfigCategory[] = [
  {
    id: "system",
    name: "System Settings",
    icon: Settings,
    description:
      "Core platform configuration affecting all users and operations",
    settings: [
      {
        name: "Application Name",
        type: "text",
        description: "Displayed in browser title and emails",
        defaultValue: "XenonClinic",
        required: true,
      },
      {
        name: "Default Language",
        type: "select",
        description: "Primary language for the platform",
        defaultValue: "English (US)",
        example: "English, Arabic, French",
      },
      {
        name: "Default Timezone",
        type: "select",
        description: "System-wide timezone setting",
        defaultValue: "Asia/Dubai (GMT+4)",
        required: true,
      },
      {
        name: "Date Format",
        type: "select",
        description: "How dates appear throughout the system",
        defaultValue: "DD/MM/YYYY",
        example: "MM/DD/YYYY, DD/MM/YYYY, YYYY-MM-DD",
      },
      {
        name: "Currency",
        type: "select",
        description: "Default currency for financial operations",
        defaultValue: "AED",
        required: true,
      },
      {
        name: "Decimal Separator",
        type: "select",
        description: "Number formatting preference",
        defaultValue: ".",
        example: ". or ,",
      },
      {
        name: "Week Start Day",
        type: "select",
        description: "First day of the week in calendars",
        defaultValue: "Sunday",
      },
    ],
    setupSteps: [
      "Navigate to Admin → System Settings",
      "Review each setting and adjust according to your organization's preferences",
      'Click "Save Changes" at the bottom of the page',
      "Settings take effect immediately for new sessions",
    ],
    tips: [
      "Set timezone before creating any appointments to avoid scheduling confusion",
      "Currency changes only affect new transactions, not historical data",
    ],
  },
  {
    id: "tenant",
    name: "Tenant Configuration",
    icon: Building,
    description: "Organization-level branding and business information",
    settings: [
      {
        name: "Organization Name",
        type: "text",
        description: "Legal name of the organization",
        required: true,
      },
      {
        name: "Trade Name",
        type: "text",
        description: "DBA or trading name if different from legal name",
      },
      {
        name: "Logo",
        type: "file",
        description: "Organization logo for branding (PNG/SVG, max 2MB)",
        example: "Recommended: 200x50px",
      },
      {
        name: "Favicon",
        type: "file",
        description: "Browser tab icon (ICO/PNG, 32x32px)",
      },
      {
        name: "Primary Color",
        type: "color",
        description: "Brand color used throughout the app",
        defaultValue: "#0066CC",
      },
      {
        name: "Secondary Color",
        type: "color",
        description: "Accent color for buttons and highlights",
        defaultValue: "#00A651",
      },
      {
        name: "Contact Email",
        type: "email",
        description: "Primary contact email for support",
        required: true,
      },
      {
        name: "Contact Phone",
        type: "tel",
        description: "Main contact phone number",
      },
      {
        name: "Business Address",
        type: "textarea",
        description: "Physical address for documents and invoices",
      },
      {
        name: "Tax Registration Number",
        type: "text",
        description: "VAT/TRN number for invoices",
      },
      {
        name: "License Number",
        type: "text",
        description: "Healthcare facility license number",
      },
      {
        name: "License Type",
        type: "readonly",
        description: "Current subscription tier",
      },
    ],
    setupSteps: [
      "Prepare your logo file (PNG/SVG format, transparent background recommended)",
      "Navigate to Admin → Organization Settings",
      "Upload logo and set brand colors",
      "Fill in all required business information",
      "Preview the branding in the live preview panel",
      "Save and verify changes appear across the application",
    ],
    warnings: [
      "Logo changes may take up to 15 minutes to propagate to all users",
      "Ensure your tax registration number is correct for invoice compliance",
    ],
  },
  {
    id: "branches",
    name: "Branch Setup",
    icon: Globe,
    description: "Multi-location configuration for clinics and departments",
    settings: [
      {
        name: "Branch Name",
        type: "text",
        description: "Display name for the branch",
        required: true,
      },
      {
        name: "Branch Code",
        type: "text",
        description: "Short code for identification (e.g., DXB01)",
        required: true,
      },
      {
        name: "Address",
        type: "textarea",
        description: "Physical location address",
      },
      {
        name: "Phone Number",
        type: "tel",
        description: "Branch contact number",
      },
      {
        name: "Email",
        type: "email",
        description: "Branch-specific email address",
      },
      {
        name: "Operating Hours",
        type: "schedule",
        description: "Weekly operating schedule",
      },
      {
        name: "Services Offered",
        type: "multiselect",
        description: "Available services at this branch",
      },
      {
        name: "Departments",
        type: "multiselect",
        description: "Departments operating at this branch",
      },
      {
        name: "Default Appointment Duration",
        type: "number",
        description: "Standard slot duration in minutes",
        defaultValue: "30",
      },
      {
        name: "Max Appointments Per Day",
        type: "number",
        description: "Capacity limit per provider",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Branch Management",
      'Click "Add New Branch" button',
      "Enter branch details and operating hours",
      "Select services and departments available at this location",
      "Configure appointment settings",
      "Assign staff members to the branch",
      "Activate the branch when ready to accept appointments",
    ],
    tips: [
      "Branch codes should be unique and consistent (e.g., city code + number)",
      "Set operating hours before configuring provider schedules",
      "Users can be assigned to multiple branches if needed",
    ],
  },
  {
    id: "users",
    name: "User Management",
    icon: Users,
    description: "User accounts, authentication methods, and access control",
    settings: [
      {
        name: "Auto-provision Users",
        type: "toggle",
        description: "Enable SSO auto-provisioning",
        defaultValue: "Off",
      },
      {
        name: "Default Role",
        type: "select",
        description: "Role assigned to new users",
        defaultValue: "Viewer",
      },
      {
        name: "Require Email Verification",
        type: "toggle",
        description: "Verify email before first login",
        defaultValue: "On",
      },
      {
        name: "Allow Self-Registration",
        type: "toggle",
        description: "Let users create their own accounts",
        defaultValue: "Off",
      },
      {
        name: "User Photo Required",
        type: "toggle",
        description: "Require profile photo for users",
        defaultValue: "Off",
      },
      {
        name: "Welcome Email Template",
        type: "select",
        description: "Email template for new users",
      },
      {
        name: "Account Approval Required",
        type: "toggle",
        description: "Admin must approve new accounts",
        defaultValue: "On",
      },
      {
        name: "Force Password Change",
        type: "toggle",
        description: "Require password change on first login",
        defaultValue: "On",
      },
    ],
    setupSteps: [
      "Define your user roles in Security Settings first",
      "Configure default role for new users",
      "Set up email verification if required",
      "Configure welcome email template",
      "Create initial admin and staff accounts",
      "Send invitations to users",
    ],
    warnings: [
      "Disabling email verification is not recommended for production environments",
      "Self-registration should only be enabled with account approval requirement",
    ],
  },
  {
    id: "security",
    name: "Security Settings",
    icon: Shield,
    description: "Authentication, access control, and data protection settings",
    settings: [
      {
        name: "Password Minimum Length",
        type: "number",
        description: "Minimum characters required",
        defaultValue: "12",
        required: true,
      },
      {
        name: "Password Complexity",
        type: "select",
        description: "Require uppercase, numbers, symbols",
        defaultValue: "Strong",
      },
      {
        name: "Password Expiry Days",
        type: "number",
        description: "Days until password reset required",
        defaultValue: "90",
      },
      {
        name: "Password History",
        type: "number",
        description: "Previous passwords that cannot be reused",
        defaultValue: "5",
      },
      {
        name: "Failed Login Attempts",
        type: "number",
        description: "Attempts before lockout",
        defaultValue: "5",
      },
      {
        name: "Lockout Duration",
        type: "number",
        description: "Minutes until auto-unlock",
        defaultValue: "15",
      },
      {
        name: "Session Timeout",
        type: "number",
        description: "Minutes of inactivity before logout",
        defaultValue: "30",
      },
      {
        name: "Require MFA",
        type: "toggle",
        description: "Mandatory multi-factor authentication",
        defaultValue: "Off",
      },
      {
        name: "MFA Methods",
        type: "multiselect",
        description: "Allowed MFA options",
        example: "Authenticator App, SMS, Email",
      },
      {
        name: "IP Whitelist",
        type: "textarea",
        description: "Allowed IP addresses/ranges (one per line)",
      },
      {
        name: "Audit Log Retention",
        type: "number",
        description: "Days to keep audit logs",
        defaultValue: "365",
      },
      {
        name: "Break the Glass",
        type: "toggle",
        description: "Allow emergency access to restricted records",
        defaultValue: "On",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Security Settings",
      "Set password policy according to your compliance requirements",
      "Configure session timeout based on clinical workflow needs",
      "Enable MFA for all users with access to PHI",
      "Set up IP whitelisting if accessing from known locations only",
      "Configure audit log retention per your data retention policy",
      "Test login with a non-admin account to verify settings",
    ],
    warnings: [
      "Changing password policy forces all users to update passwords on next login",
      "Session timeout affects clinical workflows - balance security with usability",
      "IP whitelisting will lock out users not on the list",
    ],
    tips: [
      "Healthcare environments typically require 30-minute session timeout",
      "Enable Break the Glass for clinical emergencies with mandatory justification",
      "Audit logs should be retained for minimum 7 years for healthcare compliance",
    ],
  },
  {
    id: "notifications",
    name: "Notification Settings",
    icon: Bell,
    description: "Email, SMS, and in-app notification configuration",
    settings: [
      {
        name: "SMTP Server",
        type: "text",
        description: "Email server hostname",
        example: "smtp.office365.com",
      },
      {
        name: "SMTP Port",
        type: "number",
        description: "Email server port",
        defaultValue: "587",
      },
      {
        name: "SMTP Security",
        type: "select",
        description: "Connection security",
        defaultValue: "STARTTLS",
      },
      {
        name: "SMTP Username",
        type: "text",
        description: "Email account username",
      },
      {
        name: "SMTP Password",
        type: "password",
        description: "Email account password",
      },
      {
        name: "From Email",
        type: "email",
        description: "Sender email address",
        required: true,
      },
      {
        name: "From Name",
        type: "text",
        description: "Sender display name",
        defaultValue: "XenonClinic",
      },
      {
        name: "SMS Provider",
        type: "select",
        description: "SMS gateway integration",
        example: "Twilio, MessageBird, AWS SNS",
      },
      {
        name: "SMS API Key",
        type: "password",
        description: "Provider API credentials",
      },
      {
        name: "SMS Sender ID",
        type: "text",
        description: "Sender name shown to recipients",
        example: "CLINIC",
      },
      {
        name: "Appointment Reminders",
        type: "toggle",
        description: "Send automated appointment reminders",
        defaultValue: "On",
      },
      {
        name: "Reminder Lead Time",
        type: "multiselect",
        description: "When to send reminders",
        example: "24h, 2h before",
      },
      {
        name: "Lab Results Notification",
        type: "toggle",
        description: "Notify patients when results ready",
        defaultValue: "On",
      },
      {
        name: "Invoice Notification",
        type: "toggle",
        description: "Email invoices to patients",
        defaultValue: "On",
      },
    ],
    setupSteps: [
      "Obtain SMTP credentials from your email provider (Office 365, Gmail, etc.)",
      "Navigate to Admin → Notification Settings",
      "Enter SMTP server details and test connection",
      "Configure SMS provider credentials if using SMS notifications",
      "Set up notification templates for each notification type",
      "Enable desired automated notifications",
      "Test each notification type with a test recipient",
    ],
    warnings: [
      "Test SMTP settings before enabling automated notifications",
      "SMS costs may apply based on your provider plan",
      "Ensure patient consent for communication preferences",
    ],
    tips: [
      "Office 365 requires app passwords if MFA is enabled",
      'Gmail requires "Less secure app access" or app-specific password',
      "Configure backup notification method (email if SMS fails)",
    ],
  },
  {
    id: "integrations",
    name: "Integrations",
    icon: Database,
    description: "External system connections and data exchange configuration",
    settings: [
      {
        name: "HL7 FHIR Endpoint",
        type: "url",
        description: "Healthcare data interoperability endpoint",
      },
      {
        name: "HL7 FHIR Version",
        type: "select",
        description: "FHIR version to use",
        defaultValue: "R4",
      },
      {
        name: "Lab Interface (LIS)",
        type: "select",
        description: "Laboratory information system",
        example: "LabCorp, Quest, Local LIS",
      },
      {
        name: "Lab Interface URL",
        type: "url",
        description: "LIS integration endpoint",
      },
      {
        name: "Payment Gateway",
        type: "select",
        description: "Payment processor integration",
        example: "Stripe, PayTabs, Network International",
      },
      {
        name: "Payment API Key",
        type: "password",
        description: "Payment gateway API credentials",
      },
      {
        name: "Insurance Clearinghouse",
        type: "select",
        description: "Claims submission service",
      },
      {
        name: "PACS Server URL",
        type: "url",
        description: "Medical imaging system endpoint",
      },
      {
        name: "PACS AE Title",
        type: "text",
        description: "DICOM Application Entity title",
      },
      {
        name: "SSO Provider",
        type: "select",
        description: "Single Sign-On integration",
        example: "Azure AD, Okta, SAML",
      },
      {
        name: "SSO Tenant ID",
        type: "text",
        description: "Identity provider tenant identifier",
      },
      {
        name: "Accounting System",
        type: "select",
        description: "Financial system integration",
        example: "QuickBooks, Xero, SAP",
      },
    ],
    setupSteps: [
      "Review integration requirements with your IT team",
      "Obtain API credentials from external service providers",
      "Navigate to Admin → Integrations",
      "Configure each integration in order of priority",
      "Test each integration in sandbox/test mode first",
      "Enable integrations for production use",
      "Set up monitoring and error alerts",
    ],
    warnings: [
      "Integration changes can affect live operations - test thoroughly",
      "Some integrations require network/firewall configuration",
      "Keep API credentials secure and rotate regularly",
    ],
    tips: [
      "Start with LIS integration for immediate clinical value",
      "Payment gateway integration enables online payments",
      "PACS integration requires DICOM networking expertise",
    ],
  },
  {
    id: "scheduling",
    name: "Scheduling Rules",
    icon: Clock,
    description: "Appointment booking, calendar, and availability settings",
    settings: [
      {
        name: "Default Slot Duration",
        type: "select",
        description: "Standard appointment length",
        defaultValue: "30 minutes",
      },
      {
        name: "Minimum Slot Duration",
        type: "number",
        description: "Shortest allowed appointment",
        defaultValue: "15",
      },
      {
        name: "Maximum Slot Duration",
        type: "number",
        description: "Longest allowed appointment",
        defaultValue: "120",
      },
      {
        name: "Booking Window",
        type: "number",
        description: "Days in advance patients can book",
        defaultValue: "60",
      },
      {
        name: "Minimum Notice",
        type: "number",
        description: "Hours required before appointment",
        defaultValue: "2",
      },
      {
        name: "Cancellation Window",
        type: "number",
        description: "Hours before appointment to cancel",
        defaultValue: "24",
      },
      {
        name: "Overbooking Limit",
        type: "number",
        description: "Maximum overlapping appointments",
        defaultValue: "0",
      },
      {
        name: "Buffer Between Appointments",
        type: "number",
        description: "Minutes between slots",
        defaultValue: "5",
      },
      {
        name: "Show Provider Photos",
        type: "toggle",
        description: "Display provider photos in booking",
        defaultValue: "On",
      },
      {
        name: "Allow Waitlist",
        type: "toggle",
        description: "Enable waitlist for fully booked slots",
        defaultValue: "On",
      },
      {
        name: "Auto-confirm Appointments",
        type: "toggle",
        description: "Skip manual confirmation step",
        defaultValue: "Off",
      },
      {
        name: "No-show Policy",
        type: "select",
        description: "Action for no-show patients",
        example: "None, Warning, Block future booking",
      },
    ],
    setupSteps: [
      "Analyze typical appointment durations by service type",
      "Navigate to Admin → Scheduling Settings",
      "Set default durations and booking windows",
      "Configure cancellation and no-show policies",
      "Set up provider-specific overrides if needed",
      "Test booking flow from patient portal",
    ],
    tips: [
      "Buffer time helps prevent schedule overruns",
      "Consider different durations for new vs. follow-up visits",
      "2-hour minimum notice helps reduce last-minute cancellations",
    ],
  },
  {
    id: "printing",
    name: "Printing & Documents",
    icon: Printer,
    description: "Document templates, printing, and report settings",
    settings: [
      {
        name: "Invoice Template",
        type: "select",
        description: "Template for patient invoices",
      },
      {
        name: "Prescription Template",
        type: "select",
        description: "Template for prescriptions",
      },
      {
        name: "Lab Report Template",
        type: "select",
        description: "Template for lab results",
      },
      {
        name: "Medical Certificate Template",
        type: "select",
        description: "Template for medical certificates",
      },
      {
        name: "Default Paper Size",
        type: "select",
        description: "Paper size for printing",
        defaultValue: "A4",
      },
      {
        name: "Print Header Logo",
        type: "toggle",
        description: "Include organization logo",
        defaultValue: "On",
      },
      {
        name: "Print Footer",
        type: "text",
        description: "Footer text for printed documents",
      },
      {
        name: "Digital Signature",
        type: "toggle",
        description: "Enable digital signatures",
        defaultValue: "Off",
      },
      {
        name: "QR Code on Documents",
        type: "toggle",
        description: "Add verification QR codes",
        defaultValue: "On",
      },
    ],
    setupSteps: [
      "Review and customize document templates",
      "Upload organization letterhead if required",
      "Configure header and footer content",
      "Enable digital signatures if required by regulations",
      "Test print each document type",
    ],
  },
  {
    id: "localization",
    name: "Localization & Languages",
    icon: Languages,
    description:
      "Multi-language support, regional settings, and cultural preferences",
    settings: [
      {
        name: "Available Languages",
        type: "multiselect",
        description: "Languages users can choose from",
        defaultValue: "English",
        example: "English, Arabic, French, Urdu, Hindi, Chinese",
      },
      {
        name: "Default Language",
        type: "select",
        description: "Default language for new users",
        defaultValue: "English (US)",
        required: true,
      },
      {
        name: "RTL Support",
        type: "toggle",
        description: "Enable right-to-left layout for Arabic/Hebrew",
        defaultValue: "Auto",
      },
      {
        name: "Date Format",
        type: "select",
        description: "Regional date display format",
        defaultValue: "DD/MM/YYYY",
        example: "DD/MM/YYYY, MM/DD/YYYY, YYYY-MM-DD",
      },
      {
        name: "Time Format",
        type: "select",
        description: "12-hour or 24-hour clock",
        defaultValue: "12-hour",
        example: "12-hour (AM/PM), 24-hour",
      },
      {
        name: "Number Format",
        type: "select",
        description: "Decimal and thousands separators",
        defaultValue: "1,234.56",
        example: "1,234.56 (US), 1.234,56 (EU)",
      },
      {
        name: "Currency Symbol Position",
        type: "select",
        description: "Currency symbol before or after amount",
        defaultValue: "Before",
        example: "Before ($100), After (100$)",
      },
      {
        name: "First Day of Week",
        type: "select",
        description: "Calendar week start day",
        defaultValue: "Sunday",
        example: "Sunday, Monday, Saturday",
      },
      {
        name: "Default Country Code",
        type: "select",
        description: "Default phone country code",
        defaultValue: "+971 (UAE)",
        required: true,
      },
      {
        name: "Address Format",
        type: "select",
        description: "Regional address field order",
        defaultValue: "UAE Format",
      },
      {
        name: "Emirates ID Validation",
        type: "toggle",
        description: "Enable UAE Emirates ID format validation",
        defaultValue: "On",
      },
      {
        name: "Translation Mode",
        type: "select",
        description: "How missing translations are handled",
        defaultValue: "Fall back to English",
      },
      {
        name: "Patient Portal Language",
        type: "select",
        description: "Default language for patient-facing pages",
        defaultValue: "Same as system",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Localization Settings",
      "Enable required languages in Available Languages",
      "Set the default language for your primary user base",
      "Configure regional date, time, and number formats",
      "Enable RTL support if using Arabic or Hebrew",
      "Set appropriate country code and address format",
      "Test the interface in each enabled language",
      "Configure patient portal language preferences",
    ],
    warnings: [
      "Not all custom content may be translated - verify important text manually",
      "RTL layout affects all UI elements and may require testing",
      "Changing date format does not convert existing stored dates",
    ],
    tips: [
      "Enable Arabic + English for UAE healthcare environments",
      "Allow users to override language in their profile settings",
      "Test print templates in each language to verify layout",
      "Use professional translation services for patient-facing content",
    ],
  },
  {
    id: "clinical",
    name: "Clinical Settings",
    icon: Stethoscope,
    description:
      "Clinical workflow, medical coding, vital signs, and healthcare-specific settings",
    settings: [
      {
        name: "ICD Version",
        type: "select",
        description: "Diagnosis coding system version",
        defaultValue: "ICD-10-CM",
        example: "ICD-10-CM, ICD-10-AM, ICD-11",
      },
      {
        name: "CPT Coding",
        type: "toggle",
        description: "Enable CPT procedure coding",
        defaultValue: "On",
      },
      {
        name: "Drug Database",
        type: "select",
        description: "Medication database source",
        defaultValue: "WHO Essential Medicines",
        example: "WHO, FDA NDC, Local formulary",
      },
      {
        name: "Drug Interaction Alerts",
        type: "select",
        description: "Severity level for drug interaction warnings",
        defaultValue: "Moderate and above",
        example: "All, Moderate+, Severe only",
      },
      {
        name: "Allergy Alert Level",
        type: "select",
        description: "When to show allergy warnings",
        defaultValue: "All allergies",
        example: "All, Severe only, Drug allergies only",
      },
      {
        name: "Vital Signs Units (Weight)",
        type: "select",
        description: "Unit for patient weight",
        defaultValue: "kg",
        example: "kg, lbs",
      },
      {
        name: "Vital Signs Units (Height)",
        type: "select",
        description: "Unit for patient height",
        defaultValue: "cm",
        example: "cm, ft/in",
      },
      {
        name: "Vital Signs Units (Temperature)",
        type: "select",
        description: "Temperature measurement unit",
        defaultValue: "Celsius",
        example: "Celsius, Fahrenheit",
      },
      {
        name: "Blood Pressure Format",
        type: "select",
        description: "BP display format",
        defaultValue: "Systolic/Diastolic",
      },
      {
        name: "BMI Calculation",
        type: "toggle",
        description: "Auto-calculate BMI from height/weight",
        defaultValue: "On",
      },
      {
        name: "Clinical Note Templates",
        type: "toggle",
        description: "Enable SOAP and specialty templates",
        defaultValue: "On",
      },
      {
        name: "Require Diagnosis for Billing",
        type: "toggle",
        description: "Mandate diagnosis before invoice",
        defaultValue: "On",
      },
      {
        name: "E-Prescribing",
        type: "toggle",
        description: "Enable electronic prescriptions",
        defaultValue: "On",
      },
      {
        name: "Prescription Validity Days",
        type: "number",
        description: "Default prescription expiry",
        defaultValue: "30",
      },
      {
        name: "Controlled Substance Rules",
        type: "select",
        description: "Controlled medication handling",
        defaultValue: "UAE MOH regulations",
      },
      {
        name: "Lab Reference Ranges",
        type: "select",
        description: "Source for normal value ranges",
        defaultValue: "WHO/CDC standards",
      },
      {
        name: "Critical Value Alerts",
        type: "toggle",
        description: "Alert for critical lab results",
        defaultValue: "On",
        required: true,
      },
      {
        name: "Critical Value Notification",
        type: "multiselect",
        description: "Who receives critical alerts",
        example: "Ordering physician, Nurse, Admin",
      },
      {
        name: "Visit Auto-lock Hours",
        type: "number",
        description: "Hours after which unsigned visits lock",
        defaultValue: "72",
      },
      {
        name: "Amendment Window Days",
        type: "number",
        description: "Days to amend signed records",
        defaultValue: "7",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Clinical Settings",
      "Select appropriate ICD and CPT coding versions for your jurisdiction",
      "Configure drug database and interaction alert levels",
      "Set vital signs units based on regional standards",
      "Enable clinical note templates for your specialties",
      "Configure prescription settings and controlled substance rules",
      "Set up critical value alerts and notification recipients",
      "Define visit locking and amendment policies",
      "Test clinical workflows with sample data",
    ],
    warnings: [
      "Drug interaction database updates require system restart",
      "Changing ICD version affects historical diagnosis lookups",
      "Critical value alerts are mandatory for patient safety - do not disable",
      "Controlled substance settings must comply with local regulations",
    ],
    tips: [
      "Enable BMI auto-calculation to save documentation time",
      "Configure specialty-specific note templates for efficient documentation",
      "Set visit auto-lock to ensure timely documentation completion",
      "Review critical value thresholds with your medical director",
    ],
  },
  {
    id: "data-management",
    name: "Data Management",
    icon: Archive,
    description:
      "Data retention, archiving, export, and storage management settings",
    settings: [
      {
        name: "Data Retention Period",
        type: "select",
        description: "How long to keep patient records",
        defaultValue: "10 years",
        example: "5, 7, 10, 15 years, Indefinite",
      },
      {
        name: "Inactive Patient Archive",
        type: "number",
        description: "Years until patients marked inactive",
        defaultValue: "3",
      },
      {
        name: "Auto-archive Documents",
        type: "toggle",
        description: "Move old documents to cold storage",
        defaultValue: "On",
      },
      {
        name: "Archive After Days",
        type: "number",
        description: "Days before documents auto-archive",
        defaultValue: "365",
      },
      {
        name: "Audit Log Retention",
        type: "select",
        description: "How long to keep audit logs",
        defaultValue: "7 years",
        example: "1, 3, 5, 7 years",
      },
      {
        name: "Deleted Record Retention",
        type: "number",
        description: "Days to keep soft-deleted records",
        defaultValue: "90",
      },
      {
        name: "Export Format",
        type: "select",
        description: "Default data export format",
        defaultValue: "Excel (XLSX)",
        example: "CSV, XLSX, PDF, JSON",
      },
      {
        name: "Export Encryption",
        type: "toggle",
        description: "Password-protect exported files",
        defaultValue: "On",
      },
      {
        name: "Bulk Export Limit",
        type: "number",
        description: "Maximum records per export",
        defaultValue: "10000",
      },
      {
        name: "FHIR Export",
        type: "toggle",
        description: "Enable FHIR format for interoperability",
        defaultValue: "On",
      },
      {
        name: "Storage Location",
        type: "select",
        description: "Primary data storage region",
        defaultValue: "UAE (Dubai)",
        example: "UAE, US, EU",
      },
      {
        name: "Storage Encryption",
        type: "readonly",
        description: "Data at rest encryption",
        defaultValue: "AES-256 (Always On)",
      },
      {
        name: "Media Storage Limit",
        type: "readonly",
        description: "Total storage for images/documents",
      },
      {
        name: "Database Size Alert",
        type: "number",
        description: "Alert when DB reaches % capacity",
        defaultValue: "80",
      },
    ],
    setupSteps: [
      "Review data retention requirements for your jurisdiction",
      "Navigate to Admin → Data Management",
      "Configure retention periods based on legal requirements",
      "Set up auto-archiving for efficient storage use",
      "Configure export formats and security settings",
      "Verify storage region complies with data residency laws",
      "Set capacity alerts to prevent storage issues",
    ],
    warnings: [
      "Healthcare records typically require 7-10 year retention minimum",
      "Deleted records may still be recoverable within retention period",
      "Changing storage region may require data migration approval",
      "Export encryption is recommended for all PHI exports",
    ],
    tips: [
      "UAE healthcare regulations typically require 10-year record retention",
      "Enable FHIR export for future interoperability requirements",
      "Monitor storage usage to plan capacity upgrades",
      "Create documented data retention policies for compliance",
    ],
  },
  {
    id: "backup-recovery",
    name: "Backup & Recovery",
    icon: HardDrive,
    description:
      "Data backup schedules, disaster recovery, and business continuity settings",
    settings: [
      {
        name: "Auto Backup",
        type: "toggle",
        description: "Enable automated backups",
        defaultValue: "On",
        required: true,
      },
      {
        name: "Backup Frequency",
        type: "select",
        description: "How often to create backups",
        defaultValue: "Daily",
        example: "Hourly, Daily, Weekly",
      },
      {
        name: "Backup Time",
        type: "time",
        description: "Scheduled backup time (off-peak recommended)",
        defaultValue: "02:00 AM",
      },
      {
        name: "Backup Retention",
        type: "select",
        description: "How long to keep backup copies",
        defaultValue: "30 days",
        example: "7, 14, 30, 90 days",
      },
      {
        name: "Backup Type",
        type: "select",
        description: "Full or incremental backups",
        defaultValue: "Daily incremental, Weekly full",
      },
      {
        name: "Backup Location",
        type: "multiselect",
        description: "Where backups are stored",
        defaultValue: "Cloud (Primary), Off-site (Secondary)",
      },
      {
        name: "Backup Encryption",
        type: "readonly",
        description: "Backup encryption status",
        defaultValue: "AES-256 (Always On)",
      },
      {
        name: "Point-in-Time Recovery",
        type: "toggle",
        description: "Enable granular recovery options",
        defaultValue: "On",
      },
      {
        name: "Recovery Point Objective",
        type: "select",
        description: "Maximum acceptable data loss",
        defaultValue: "1 hour",
        example: "15 min, 1 hour, 4 hours, 24 hours",
      },
      {
        name: "Recovery Time Objective",
        type: "select",
        description: "Maximum acceptable downtime",
        defaultValue: "4 hours",
        example: "1 hour, 4 hours, 8 hours, 24 hours",
      },
      {
        name: "Disaster Recovery Site",
        type: "toggle",
        description: "Enable secondary DR environment",
        defaultValue: "On",
      },
      {
        name: "DR Failover Mode",
        type: "select",
        description: "Automatic or manual failover",
        defaultValue: "Manual with notification",
      },
      {
        name: "Backup Verification",
        type: "toggle",
        description: "Test backup integrity automatically",
        defaultValue: "On",
      },
      {
        name: "Backup Notification",
        type: "multiselect",
        description: "Who receives backup status alerts",
        example: "System Admin, IT Team",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Backup & Recovery",
      "Ensure auto backup is enabled (mandatory)",
      "Set backup schedule during off-peak hours",
      "Configure backup retention based on your DR policy",
      "Enable backup verification for automatic integrity checks",
      "Set up DR site if high availability is required",
      "Configure backup notifications for IT team",
      "Document recovery procedures and test quarterly",
    ],
    warnings: [
      "Never disable auto backup in production environments",
      "Backup time should be during lowest system usage",
      "Test recovery procedures at least quarterly",
      "Ensure backup location complies with data residency requirements",
    ],
    tips: [
      "Healthcare systems typically require 1-hour RPO or better",
      "Keep at least one backup copy off-site/off-cloud",
      "Document and test disaster recovery procedures regularly",
      "Consider geo-redundant storage for critical data",
    ],
  },
  {
    id: "audit-compliance",
    name: "Audit & Compliance",
    icon: Eye,
    description: "Audit logging, compliance reporting, and regulatory settings",
    settings: [
      {
        name: "Audit Logging",
        type: "readonly",
        description: "Comprehensive audit trail",
        defaultValue: "Always On (Cannot disable)",
      },
      {
        name: "Audit Detail Level",
        type: "select",
        description: "Level of detail in audit logs",
        defaultValue: "Detailed",
        example: "Basic, Standard, Detailed",
      },
      {
        name: "PHI Access Logging",
        type: "toggle",
        description: "Log all patient record access",
        defaultValue: "On",
        required: true,
      },
      {
        name: "Failed Login Logging",
        type: "toggle",
        description: "Log failed authentication attempts",
        defaultValue: "On",
      },
      {
        name: "Export Activity Logging",
        type: "toggle",
        description: "Log all data exports",
        defaultValue: "On",
      },
      {
        name: "Audit Log Access",
        type: "select",
        description: "Who can view audit logs",
        defaultValue: "System Admin only",
      },
      {
        name: "Audit Log Export",
        type: "select",
        description: "Who can export audit logs",
        defaultValue: "System Admin with MFA",
      },
      {
        name: "Compliance Framework",
        type: "multiselect",
        description: "Applicable compliance standards",
        example: "HIPAA, DOH UAE, ISO 27001, GDPR",
      },
      {
        name: "HIPAA Mode",
        type: "toggle",
        description: "Enable HIPAA-specific controls",
        defaultValue: "On",
      },
      {
        name: "Break the Glass",
        type: "toggle",
        description: "Emergency access to restricted records",
        defaultValue: "On",
      },
      {
        name: "Break Glass Justification",
        type: "toggle",
        description: "Require reason for emergency access",
        defaultValue: "On",
      },
      {
        name: "Consent Tracking",
        type: "toggle",
        description: "Track patient consent records",
        defaultValue: "On",
      },
      {
        name: "Data Processing Agreement",
        type: "toggle",
        description: "Require DPA for data sharing",
        defaultValue: "On",
      },
      {
        name: "Compliance Report Schedule",
        type: "select",
        description: "Auto-generate compliance reports",
        defaultValue: "Monthly",
        example: "Weekly, Monthly, Quarterly",
      },
      {
        name: "Compliance Officer Email",
        type: "email",
        description: "Email for compliance notifications",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Audit & Compliance",
      "Review and enable applicable compliance frameworks",
      "Configure audit detail level based on requirements",
      "Set up compliance report schedule",
      "Configure Break the Glass access for emergencies",
      "Enable consent tracking for patient authorizations",
      "Set up compliance officer notifications",
      "Review audit logs regularly for anomalies",
    ],
    warnings: [
      "Audit logging cannot be disabled for healthcare compliance",
      "PHI access logging is mandatory for HIPAA compliance",
      "Break the Glass should require justification and creates permanent audit trail",
      "Audit logs may be required for legal proceedings - preserve carefully",
    ],
    tips: [
      "Review access logs monthly for suspicious patterns",
      "Set up alerts for unusual after-hours access",
      "Document all compliance policies and train staff",
      "Conduct annual compliance audits with external auditor",
    ],
  },
  {
    id: "financial-settings",
    name: "Financial Settings",
    icon: Calculator,
    description: "Billing, pricing, tax, and financial configuration",
    settings: [
      {
        name: "Default Currency",
        type: "select",
        description: "Primary currency for transactions",
        defaultValue: "AED",
        required: true,
      },
      {
        name: "Secondary Currencies",
        type: "multiselect",
        description: "Additional accepted currencies",
        example: "USD, EUR, GBP",
      },
      {
        name: "Exchange Rate Update",
        type: "select",
        description: "How exchange rates are updated",
        defaultValue: "Daily automatic",
      },
      {
        name: "Tax Type",
        type: "select",
        description: "Tax system type",
        defaultValue: "VAT",
        example: "VAT, GST, Sales Tax, None",
      },
      {
        name: "Tax Rate",
        type: "number",
        description: "Default tax percentage",
        defaultValue: "5",
        example: "5% UAE VAT",
      },
      {
        name: "Tax Registration Number",
        type: "text",
        description: "TRN for tax invoices",
        required: true,
      },
      {
        name: "Price Rounding",
        type: "select",
        description: "How prices are rounded",
        defaultValue: "Nearest 0.01",
      },
      {
        name: "Invoice Numbering",
        type: "select",
        description: "Invoice number format",
        defaultValue: "INV-YYYY-NNNNNN",
      },
      {
        name: "Invoice Due Days",
        type: "number",
        description: "Default payment terms",
        defaultValue: "30",
      },
      {
        name: "Late Payment Fee",
        type: "toggle",
        description: "Apply late payment charges",
        defaultValue: "Off",
      },
      {
        name: "Payment Methods",
        type: "multiselect",
        description: "Accepted payment types",
        defaultValue: "Cash, Card, Insurance",
        example: "Cash, Card, Bank Transfer, Insurance",
      },
      {
        name: "Advance Payment Required",
        type: "toggle",
        description: "Require deposit for appointments",
        defaultValue: "Off",
      },
      {
        name: "Insurance Auto-verification",
        type: "toggle",
        description: "Auto-check insurance eligibility",
        defaultValue: "On",
      },
      {
        name: "Claim Auto-submission",
        type: "toggle",
        description: "Auto-submit insurance claims",
        defaultValue: "Off",
      },
      {
        name: "Revenue Recognition",
        type: "select",
        description: "When revenue is recognized",
        defaultValue: "On service delivery",
      },
      {
        name: "Fiscal Year Start",
        type: "select",
        description: "Financial year start month",
        defaultValue: "January",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Financial Settings",
      "Set primary currency and tax configuration",
      "Enter tax registration number for compliant invoices",
      "Configure invoice numbering format",
      "Enable accepted payment methods",
      "Set up insurance verification if applicable",
      "Configure fiscal year for financial reporting",
      "Test billing workflow end-to-end",
    ],
    warnings: [
      "Tax registration number must be valid for tax compliance",
      "Currency changes affect future transactions only",
      "Insurance auto-submission requires payer contract setup",
    ],
    tips: [
      "UAE requires 5% VAT on healthcare services (some exemptions apply)",
      "Enable insurance auto-verification to reduce claim rejections",
      "Use sequential invoice numbers for audit compliance",
    ],
  },
  {
    id: "workflow-automation",
    name: "Workflow & Automation",
    icon: Zap,
    description:
      "Automated workflows, task assignments, and process automation",
    settings: [
      {
        name: "Workflow Engine",
        type: "toggle",
        description: "Enable visual workflow builder",
        defaultValue: "On",
      },
      {
        name: "Auto-assign Tasks",
        type: "toggle",
        description: "Automatically assign tasks based on rules",
        defaultValue: "On",
      },
      {
        name: "Escalation Rules",
        type: "toggle",
        description: "Enable automatic task escalation",
        defaultValue: "On",
      },
      {
        name: "Escalation Timeout",
        type: "number",
        description: "Hours before task escalates",
        defaultValue: "24",
      },
      {
        name: "Appointment Auto-confirm",
        type: "toggle",
        description: "Auto-confirm appointments",
        defaultValue: "Off",
      },
      {
        name: "Reminder Automation",
        type: "toggle",
        description: "Send automated reminders",
        defaultValue: "On",
      },
      {
        name: "No-show Follow-up",
        type: "toggle",
        description: "Auto-send no-show notifications",
        defaultValue: "On",
      },
      {
        name: "Follow-up Reminders",
        type: "toggle",
        description: "Remind patients about follow-ups",
        defaultValue: "On",
      },
      {
        name: "Lab Result Auto-release",
        type: "toggle",
        description: "Auto-release normal results",
        defaultValue: "Off",
      },
      {
        name: "Prescription Refill Alerts",
        type: "toggle",
        description: "Notify for medication refills",
        defaultValue: "On",
      },
      {
        name: "Pending Task Dashboard",
        type: "toggle",
        description: "Show pending tasks on login",
        defaultValue: "On",
      },
      {
        name: "Workflow Notifications",
        type: "multiselect",
        description: "How workflow events notify users",
        example: "In-app, Email, SMS, Push",
      },
      {
        name: "Daily Digest",
        type: "toggle",
        description: "Send daily task summary email",
        defaultValue: "On",
      },
      {
        name: "Workflow Audit Trail",
        type: "toggle",
        description: "Log all workflow actions",
        defaultValue: "On",
      },
    ],
    setupSteps: [
      "Navigate to Admin → Workflow & Automation",
      "Enable the workflow engine",
      "Configure task assignment rules",
      "Set up escalation timeouts based on priority",
      "Enable reminder automations for appointments",
      "Configure notification preferences",
      "Create custom workflows for your processes",
      "Test each automation with sample scenarios",
    ],
    tips: [
      "Start with simple automations and add complexity gradually",
      "Use escalation rules to ensure nothing falls through cracks",
      "Review workflow performance monthly for optimization",
      "Document all custom workflows for staff training",
    ],
  },
];

export default function AdminConfiguration() {
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(
    new Set(["system"]),
  );

  const toggleCategory = (id: string) => {
    const newExpanded = new Set(expandedCategories);
    if (newExpanded.has(id)) {
      newExpanded.delete(id);
    } else {
      newExpanded.add(id);
    }
    setExpandedCategories(newExpanded);
  };

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Admin Configuration
        </h1>
        <p className="text-lg text-gray-600">
          System administrators and tenant administrators can configure
          XenonClinic to match organizational requirements and policies. This
          guide covers all configuration areas with step-by-step setup
          instructions.
        </p>
      </div>

      {/* Quick Navigation */}
      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <h3 className="font-semibold text-gray-900 mb-3">Quick Navigation</h3>
        <div className="flex flex-wrap gap-2">
          {configCategories.map((category) => (
            <a
              key={category.id}
              href={`#${category.id}`}
              onClick={() => {
                const newExpanded = new Set(expandedCategories);
                newExpanded.add(category.id);
                setExpandedCategories(newExpanded);
              }}
              className="px-3 py-1.5 bg-gray-100 hover:bg-primary-100 hover:text-primary-700 rounded-full text-sm font-medium transition-colors"
            >
              {category.name}
            </a>
          ))}
        </div>
      </div>

      {/* Access Note */}
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-5">
        <div className="flex items-start gap-3">
          <Key className="h-6 w-6 text-blue-600 flex-shrink-0" />
          <div>
            <h3 className="font-semibold text-gray-900">
              Administrator Access Required
            </h3>
            <p className="text-gray-600 mt-1">
              These settings require System Admin or Tenant Admin role. Changes
              may affect all users in your organization and should be made
              carefully. We recommend testing in a sandbox environment first.
            </p>
            <div className="mt-3 flex flex-wrap gap-2">
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                <Lock className="h-3 w-3 mr-1" /> admin:settings:view
              </span>
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                <Lock className="h-3 w-3 mr-1" /> admin:settings:update
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Configuration Categories */}
      <div className="space-y-6">
        {configCategories.map((category) => {
          const Icon = category.icon;
          const isExpanded = expandedCategories.has(category.id);

          return (
            <section
              key={category.id}
              id={category.id}
              className="bg-white border border-gray-200 rounded-xl overflow-hidden"
            >
              <button
                onClick={() => toggleCategory(category.id)}
                className="w-full px-5 py-4 flex items-center justify-between hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-primary-100 rounded-lg">
                    <Icon className="h-6 w-6 text-primary-600" />
                  </div>
                  <div className="text-left">
                    <h2 className="text-xl font-semibold text-gray-900">
                      {category.name}
                    </h2>
                    <p className="text-sm text-gray-500">
                      {category.description}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm text-gray-400">
                    {category.settings.length} settings
                  </span>
                  {isExpanded ? (
                    <ChevronDown className="h-5 w-5 text-gray-400" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-gray-400" />
                  )}
                </div>
              </button>

              {isExpanded && (
                <div className="px-5 pb-5 space-y-6">
                  {/* Setup Steps */}
                  {category.setupSteps && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                      <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
                        <CheckCircle className="h-5 w-5 text-green-600" />
                        Setup Steps
                      </h4>
                      <ol className="space-y-2">
                        {category.setupSteps.map((step, index) => (
                          <li
                            key={index}
                            className="flex items-start gap-2 text-sm text-gray-700"
                          >
                            <span className="flex-shrink-0 w-5 h-5 bg-green-200 text-green-800 rounded-full flex items-center justify-center text-xs font-medium">
                              {index + 1}
                            </span>
                            {step}
                          </li>
                        ))}
                      </ol>
                    </div>
                  )}

                  {/* Settings Table */}
                  <div className="border border-gray-200 rounded-lg overflow-hidden">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                            Setting
                          </th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                            Type
                          </th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                            Description
                          </th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                            Default
                          </th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200">
                        {category.settings.map((setting) => (
                          <tr key={setting.name}>
                            <td className="px-4 py-3">
                              <div className="font-medium text-gray-900">
                                {setting.name}
                                {setting.required && (
                                  <span className="text-red-500 ml-1">*</span>
                                )}
                              </div>
                            </td>
                            <td className="px-4 py-3">
                              <code className="bg-gray-100 px-2 py-0.5 rounded text-sm text-gray-600">
                                {setting.type}
                              </code>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-600">
                              {setting.description}
                              {setting.example && (
                                <span className="block text-xs text-gray-400 mt-1">
                                  e.g., {setting.example}
                                </span>
                              )}
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-500">
                              {setting.defaultValue || "—"}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {/* Warnings */}
                  {category.warnings && category.warnings.length > 0 && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                      <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                        <AlertTriangle className="h-5 w-5 text-yellow-600" />
                        Important Warnings
                      </h4>
                      <ul className="space-y-1">
                        {category.warnings.map((warning, index) => (
                          <li
                            key={index}
                            className="text-sm text-yellow-800 flex items-start gap-2"
                          >
                            <span className="text-yellow-600">•</span>
                            {warning}
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}

                  {/* Tips */}
                  {category.tips && category.tips.length > 0 && (
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                      <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                        <Info className="h-5 w-5 text-blue-600" />
                        Tips & Best Practices
                      </h4>
                      <ul className="space-y-1">
                        {category.tips.map((tip, index) => (
                          <li
                            key={index}
                            className="text-sm text-blue-800 flex items-start gap-2"
                          >
                            <span className="text-blue-600">•</span>
                            {tip}
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              )}
            </section>
          );
        })}
      </div>

      {/* Configuration Best Practices */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Configuration Best Practices
        </h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Palette className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Customize Branding</h3>
            <p className="text-sm text-gray-600 mt-1">
              Upload your logo and set brand colors to make XenonClinic feel
              like your own. This improves user adoption and trust.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Shield className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Enable MFA</h3>
            <p className="text-sm text-gray-600 mt-1">
              Require multi-factor authentication for all users, especially
              those with access to PHI or financial data.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Bell className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Configure Reminders</h3>
            <p className="text-sm text-gray-600 mt-1">
              Set up appointment reminders to reduce no-shows. Consider both
              email and SMS for better reach.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Clock className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">
              Review Session Timeout
            </h3>
            <p className="text-sm text-gray-600 mt-1">
              Balance security and convenience. 30 minutes is recommended for
              clinical environments.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <RefreshCw className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">
              Test Before Deploying
            </h3>
            <p className="text-sm text-gray-600 mt-1">
              Always test configuration changes in a sandbox environment before
              applying to production.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <FileText className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Document Changes</h3>
            <p className="text-sm text-gray-600 mt-1">
              Keep a log of configuration changes with dates and reasons. This
              helps with troubleshooting and audits.
            </p>
          </div>
        </div>
      </section>

      {/* Configuration Checklist */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Initial Setup Checklist
        </h2>
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <p className="text-gray-600 mb-4">
            Use this checklist when setting up a new XenonClinic environment:
          </p>
          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <h4 className="font-medium text-gray-900 mb-2">
                Required Configuration
              </h4>
              <ul className="space-y-2">
                {[
                  "Set organization name and contact details",
                  "Configure timezone and date format",
                  "Upload organization logo",
                  "Set password policy",
                  "Configure session timeout",
                  "Set up at least one branch",
                  "Create initial admin accounts",
                ].map((item, index) => (
                  <li
                    key={index}
                    className="flex items-center gap-2 text-sm text-gray-600"
                  >
                    <input
                      type="checkbox"
                      className="h-4 w-4 text-primary-600 rounded"
                    />
                    {item}
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <h4 className="font-medium text-gray-900 mb-2">
                Recommended Configuration
              </h4>
              <ul className="space-y-2">
                {[
                  "Configure email (SMTP) settings",
                  "Enable appointment reminders",
                  "Set up SMS notifications",
                  "Configure payment gateway",
                  "Enable multi-factor authentication",
                  "Set up lab interface (LIS)",
                  "Configure backup schedule",
                ].map((item, index) => (
                  <li
                    key={index}
                    className="flex items-center gap-2 text-sm text-gray-600"
                  >
                    <input
                      type="checkbox"
                      className="h-4 w-4 text-primary-600 rounded"
                    />
                    {item}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* Navigation Links */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Related Documentation
        </h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <Link
            to="/docs/security-rbac"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Shield className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Security & RBAC
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Configure roles and permissions
            </p>
          </Link>
          <Link
            to="/docs/personas/tenant-admin"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Users className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Tenant Admin Guide
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Detailed admin responsibilities
            </p>
          </Link>
          <Link
            to="/docs/api-reference"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Server className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              API Reference
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              API configuration and webhooks
            </p>
          </Link>
        </div>
      </section>

      {/* Support Section */}
      <section className="bg-gray-50 border border-gray-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <Headphones className="h-6 w-6 text-gray-600 flex-shrink-0" />
          <div>
            <h3 className="font-semibold text-gray-900">
              Need Configuration Help?
            </h3>
            <p className="text-gray-600 mt-1">
              Our support team can assist with complex configurations,
              integrations, and migrations.
            </p>
            <ul className="mt-2 space-y-1 text-sm text-gray-600">
              <li>
                Email:{" "}
                <a
                  href="mailto:support@xenonclinic.com"
                  className="text-primary-600 hover:underline"
                >
                  support@xenonclinic.com
                </a>
              </li>
              <li>Phone: Available during business hours (see contract)</li>
              <li>In-app: Click the help icon in the application</li>
            </ul>
          </div>
        </div>
      </section>
    </div>
  );
}
