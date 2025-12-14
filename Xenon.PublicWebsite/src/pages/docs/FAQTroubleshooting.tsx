import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  HelpCircle, AlertTriangle, ChevronDown, ChevronRight, Search, CheckCircle,
  XCircle, Clock, Shield, Users, Calendar, DollarSign, FileText, Database,
  Wifi, RefreshCw, Lock, Settings, Printer, Mail, Monitor, Smartphone,
  Server, Key, AlertCircle, ArrowRight, Zap, MessageSquare, Headphones,
} from 'lucide-react';

interface FAQ {
  question: string;
  answer: string;
  category: string;
  tags?: string[];
}

interface ErrorCode {
  code: string;
  name: string;
  description: string;
  causes: string[];
  solutions: string[];
  severity: 'low' | 'medium' | 'high' | 'critical';
  category: string;
}

const faqs: FAQ[] = [
  // Account & Login FAQs
  {
    question: 'How do I reset my password?',
    answer: 'Click "Forgot Password" on the login page. Enter your registered email address and check your inbox for a password reset link. The link expires in 24 hours. If you don\'t receive the email, check your spam folder or contact your administrator.',
    category: 'Account & Login',
    tags: ['password', 'login', 'reset'],
  },
  {
    question: 'Why am I getting locked out of my account?',
    answer: 'Accounts are locked after 5 consecutive failed login attempts. Wait 15 minutes for automatic unlock, or contact your administrator to unlock immediately. Enable multi-factor authentication to add an extra layer of security while maintaining access.',
    category: 'Account & Login',
    tags: ['lockout', 'security', 'login'],
  },
  {
    question: 'How do I enable multi-factor authentication (MFA)?',
    answer: 'Go to your Profile → Security Settings → Enable MFA. Choose your preferred method (Authenticator App recommended). Scan the QR code with Google Authenticator or similar app. Enter the verification code to complete setup. Save backup codes in a secure location.',
    category: 'Account & Login',
    tags: ['mfa', '2fa', 'security'],
  },
  {
    question: 'Can I use XenonClinic on multiple devices?',
    answer: 'Yes, you can access XenonClinic from any device with a web browser. However, each device creates a separate session. You\'ll need to log in on each device. Concurrent sessions may be limited based on your organization\'s security policy.',
    category: 'Account & Login',
    tags: ['devices', 'sessions', 'mobile'],
  },
  {
    question: 'Why does my session keep timing out?',
    answer: 'Sessions time out after 30 minutes of inactivity for security. This is especially important in clinical environments where devices may be shared. Contact your administrator if you need a longer session timeout for your workflow.',
    category: 'Account & Login',
    tags: ['timeout', 'session', 'security'],
  },

  // Patient Management FAQs
  {
    question: 'How do I merge duplicate patient records?',
    answer: 'Go to Patients → Patient Management → Find Duplicates. The system will suggest potential matches based on name, Emirates ID, and phone number. Review each pair carefully, select the primary record, and click "Merge". This action cannot be undone, so verify before confirming.',
    category: 'Patients',
    tags: ['merge', 'duplicate', 'records'],
  },
  {
    question: 'Can patients update their own information?',
    answer: 'Yes, if enabled. Patients can update contact information and emergency contacts through the Patient Portal. Medical information and insurance details require staff verification. Changes are logged in the audit trail.',
    category: 'Patients',
    tags: ['patient portal', 'self-service', 'updates'],
  },
  {
    question: 'How do I handle patient consent forms?',
    answer: 'Create consent form templates in Admin → Document Templates. Assign required consents at the appointment type level. When patients check in, the system prompts for pending consents. Digital signatures are captured and stored with a timestamp.',
    category: 'Patients',
    tags: ['consent', 'forms', 'legal'],
  },
  {
    question: 'What happens when a patient requests data deletion?',
    answer: 'Under data protection regulations, patients may request data deletion. Go to Patient Profile → Privacy → Data Deletion Request. The system will check for legal retention requirements (billing, clinical records). Data may be anonymized rather than deleted if legally required to retain.',
    category: 'Patients',
    tags: ['privacy', 'gdpr', 'deletion'],
  },

  // Appointments FAQs
  {
    question: 'How do I set up recurring appointments?',
    answer: 'When creating an appointment, enable "Recurring" and set the frequency (weekly, bi-weekly, monthly). Define the end date or number of occurrences. The system will check provider availability for each date and flag conflicts.',
    category: 'Appointments',
    tags: ['recurring', 'scheduling', 'repeat'],
  },
  {
    question: 'Can I overbook a provider\'s schedule?',
    answer: 'Overbooking is configurable per provider and appointment type. Go to Admin → Scheduling → Overbooking Rules. Set the maximum number of overlapping appointments. The system will warn but allow overbooking up to the configured limit.',
    category: 'Appointments',
    tags: ['overbooking', 'scheduling', 'capacity'],
  },
  {
    question: 'How do appointment reminders work?',
    answer: 'Reminders are sent automatically 24 hours and 2 hours before appointments (configurable). Patients receive both email and SMS if contact information is available. Reminders include appointment details and cancellation instructions. Enable reminders in Admin → Notifications.',
    category: 'Appointments',
    tags: ['reminders', 'notifications', 'sms'],
  },
  {
    question: 'How do I handle walk-in patients?',
    answer: 'Use "Quick Add" on the schedule view or create an appointment with "Walk-in" type. Walk-ins are shown differently on the schedule. You can configure walk-in slots or allow walk-ins to fit into regular appointment gaps.',
    category: 'Appointments',
    tags: ['walk-in', 'scheduling', 'urgent'],
  },
  {
    question: 'What is the waitlist and how does it work?',
    answer: 'The waitlist allows patients to request earlier appointments if cancellations occur. Enable in Admin → Scheduling → Waitlist. When a slot opens, the system notifies waitlisted patients in order. They have a limited time to confirm before the slot goes to the next person.',
    category: 'Appointments',
    tags: ['waitlist', 'cancellation', 'scheduling'],
  },

  // Clinical FAQs
  {
    question: 'How do I prescribe controlled substances?',
    answer: 'Controlled substances require additional verification. The prescriber must have a valid DEA number registered in their profile. The system checks prescribing limits and warns about early refills. E-prescribing for controlled substances uses additional authentication.',
    category: 'Clinical',
    tags: ['controlled', 'prescription', 'dea'],
  },
  {
    question: 'Can I customize clinical templates?',
    answer: 'Yes. Go to Admin → Clinical Settings → Templates. Create specialty-specific templates for SOAP notes, examination forms, and procedure documentation. Templates can include custom fields, dropdowns, and default values. Changes apply to future encounters only.',
    category: 'Clinical',
    tags: ['templates', 'soap', 'documentation'],
  },
  {
    question: 'How do I handle critical lab results?',
    answer: 'Critical values trigger immediate alerts. When a lab result falls outside critical ranges, the ordering physician receives an alert. Acknowledgment is required within the configured timeframe. Unacknowledged alerts escalate to department head.',
    category: 'Clinical',
    tags: ['critical', 'lab', 'alerts'],
  },
  {
    question: 'How do referrals work?',
    answer: 'Create referrals from the clinical visit screen. Select the specialist, urgency level, and attach relevant clinical information. If the specialist is in-network, the system can automatically suggest appointment slots. External referrals generate printable referral letters.',
    category: 'Clinical',
    tags: ['referral', 'specialist', 'orders'],
  },

  // Financial FAQs
  {
    question: 'How do I apply a discount to an invoice?',
    answer: 'Open the invoice and click "Apply Discount". Choose percentage or fixed amount. Select a discount reason (requires appropriate permission). Discounts above certain thresholds may require manager approval. All discounts are logged in the audit trail.',
    category: 'Financial',
    tags: ['discount', 'invoice', 'billing'],
  },
  {
    question: 'How do I process insurance claims?',
    answer: 'After completing a visit, go to Billing → Insurance Claims. Select the visit and verify coding (ICD-10, CPT). Click "Submit Claim". Claims are sent to the configured clearinghouse. Track claim status in the Claims Dashboard. Appeal denied claims from the same screen.',
    category: 'Financial',
    tags: ['insurance', 'claims', 'billing'],
  },
  {
    question: 'Can patients pay online?',
    answer: 'Yes, if enabled. Configure payment gateway in Admin → Financial → Payment Settings. Patients receive email invoices with a "Pay Now" link. The Patient Portal also shows outstanding balances. Accepted methods depend on your payment gateway.',
    category: 'Financial',
    tags: ['online payment', 'portal', 'gateway'],
  },
  {
    question: 'How do I handle refunds?',
    answer: 'Go to the original payment and click "Refund". Enter the refund amount and reason. Full or partial refunds are supported. Refunds go back to the original payment method. Credit card refunds may take 5-10 business days to process.',
    category: 'Financial',
    tags: ['refund', 'payment', 'billing'],
  },

  // System & Technical FAQs
  {
    question: 'What browsers are supported?',
    answer: 'XenonClinic supports Chrome, Firefox, Safari, and Edge (latest 2 versions). Internet Explorer is not supported. For the best experience, use Chrome on desktop. Mobile browsers are supported but desktop is recommended for clinical workflows.',
    category: 'Technical',
    tags: ['browser', 'compatibility', 'chrome'],
  },
  {
    question: 'How often is data backed up?',
    answer: 'Data is backed up hourly with daily snapshots retained for 30 days. Point-in-time recovery is available for the last 7 days. Backup data is encrypted and stored in geographically separate locations. Disaster recovery tests are conducted quarterly.',
    category: 'Technical',
    tags: ['backup', 'data', 'recovery'],
  },
  {
    question: 'Is XenonClinic HIPAA compliant?',
    answer: 'Yes. XenonClinic is designed for HIPAA compliance with encryption at rest and in transit, audit logging, access controls, and break-the-glass procedures. We sign BAAs with healthcare organizations. Compliance documentation is available on request.',
    category: 'Technical',
    tags: ['hipaa', 'compliance', 'security'],
  },
  {
    question: 'Can I export my data?',
    answer: 'Yes. Export options vary by module. Patient data can be exported in HL7 FHIR format. Reports can be exported to Excel, PDF, or CSV. Full data exports for migration are available upon request with appropriate authorization.',
    category: 'Technical',
    tags: ['export', 'data', 'migration'],
  },
  {
    question: 'How do I connect to the API?',
    answer: 'Get API credentials from Admin → Integrations → API Keys. Authenticate using JWT tokens. See the API Reference documentation for endpoints, request formats, and response structures. A sandbox environment is available for development.',
    category: 'Technical',
    tags: ['api', 'integration', 'development'],
  },

  // Admin FAQs
  {
    question: 'How do I add a new user?',
    answer: 'Go to Admin → Users → Add User. Enter user details, assign role(s), and select branch access. An invitation email is sent automatically. The user sets their password on first login. Bulk user import is available via CSV.',
    category: 'Administration',
    tags: ['users', 'admin', 'onboarding'],
  },
  {
    question: 'How do I create custom roles?',
    answer: 'Go to Admin → Security → Roles. Click "Create Role" and define permissions by module. Roles can inherit from existing roles. Test the role by assigning it to a test user. Document the role\'s purpose for future reference.',
    category: 'Administration',
    tags: ['roles', 'permissions', 'rbac'],
  },
  {
    question: 'How do I view the audit log?',
    answer: 'Go to Admin → Audit Log. Filter by date range, user, action type, or resource. The log shows who did what, when, and what changed. For PHI access, additional details show the business justification. Export logs for compliance reporting.',
    category: 'Administration',
    tags: ['audit', 'log', 'compliance'],
  },
];

const errorCodes: ErrorCode[] = [
  // Authentication Errors (1000-1099)
  {
    code: 'AUTH_001',
    name: 'Invalid Credentials',
    description: 'The username or password provided is incorrect.',
    causes: ['Incorrect password', 'Wrong username/email', 'Caps lock enabled'],
    solutions: ['Verify username spelling', 'Try password reset', 'Check caps lock'],
    severity: 'low',
    category: 'Authentication',
  },
  {
    code: 'AUTH_002',
    name: 'Account Locked',
    description: 'Account has been temporarily locked due to multiple failed login attempts.',
    causes: ['5+ failed login attempts', 'Brute force protection triggered'],
    solutions: ['Wait 15 minutes for auto-unlock', 'Contact administrator to unlock', 'Reset password'],
    severity: 'medium',
    category: 'Authentication',
  },
  {
    code: 'AUTH_003',
    name: 'Session Expired',
    description: 'Your session has timed out due to inactivity.',
    causes: ['30+ minutes of inactivity', 'Browser closed without logout', 'Server restart'],
    solutions: ['Log in again', 'Save work before extended breaks'],
    severity: 'low',
    category: 'Authentication',
  },
  {
    code: 'AUTH_004',
    name: 'MFA Required',
    description: 'Multi-factor authentication code is required to complete login.',
    causes: ['MFA enabled for account', 'New device detected', 'Elevated permissions required'],
    solutions: ['Enter code from authenticator app', 'Check SMS for code', 'Use backup codes if available'],
    severity: 'low',
    category: 'Authentication',
  },
  {
    code: 'AUTH_005',
    name: 'Invalid MFA Code',
    description: 'The multi-factor authentication code provided is invalid or expired.',
    causes: ['Code expired (30 seconds)', 'Time sync issue on device', 'Wrong authenticator account'],
    solutions: ['Wait for new code', 'Sync device time', 'Verify correct account in app'],
    severity: 'low',
    category: 'Authentication',
  },
  {
    code: 'AUTH_006',
    name: 'Password Expired',
    description: 'Your password has expired and must be changed.',
    causes: ['90+ days since last change', 'Organization policy requires rotation'],
    solutions: ['Set a new password meeting complexity requirements', 'Cannot reuse last 5 passwords'],
    severity: 'medium',
    category: 'Authentication',
  },
  {
    code: 'AUTH_007',
    name: 'Account Disabled',
    description: 'This account has been disabled by an administrator.',
    causes: ['Terminated employment', 'Security concern', 'License expiration'],
    solutions: ['Contact your administrator', 'HR may need to reinstate access'],
    severity: 'high',
    category: 'Authentication',
  },

  // Authorization Errors (1100-1199)
  {
    code: 'AUTHZ_001',
    name: 'Permission Denied',
    description: 'You do not have permission to perform this action.',
    causes: ['Missing role/permission', 'Branch restriction', 'PHI access restriction'],
    solutions: ['Request access from administrator', 'Contact IT for role assignment'],
    severity: 'medium',
    category: 'Authorization',
  },
  {
    code: 'AUTHZ_002',
    name: 'PHI Access Denied',
    description: 'Access to protected health information requires explicit authorization.',
    causes: ['No treatment relationship', 'Record restricted', 'Patient opt-out'],
    solutions: ['Establish treatment relationship', 'Use Break the Glass for emergencies'],
    severity: 'high',
    category: 'Authorization',
  },
  {
    code: 'AUTHZ_003',
    name: 'Branch Access Denied',
    description: 'You do not have access to this branch location.',
    causes: ['Not assigned to branch', 'Branch access revoked'],
    solutions: ['Request branch assignment from administrator'],
    severity: 'medium',
    category: 'Authorization',
  },

  // Patient Errors (2000-2099)
  {
    code: 'PAT_001',
    name: 'Patient Not Found',
    description: 'The requested patient record does not exist or has been archived.',
    causes: ['Invalid patient ID', 'Record deleted/archived', 'Wrong branch'],
    solutions: ['Verify patient ID', 'Search by name/MRN', 'Check archived records'],
    severity: 'low',
    category: 'Patient',
  },
  {
    code: 'PAT_002',
    name: 'Duplicate Patient',
    description: 'A patient with similar identifying information already exists.',
    causes: ['Same Emirates ID', 'Similar name and DOB', 'Existing MRN'],
    solutions: ['Review existing patient', 'Merge if duplicate', 'Update existing record'],
    severity: 'medium',
    category: 'Patient',
  },
  {
    code: 'PAT_003',
    name: 'Invalid Emirates ID',
    description: 'The Emirates ID format is invalid or does not pass validation.',
    causes: ['Wrong format', 'Incorrect check digit', 'Expired ID'],
    solutions: ['Verify format: 784-YYYY-NNNNNNN-N', 'Check original ID document'],
    severity: 'low',
    category: 'Patient',
  },

  // Appointment Errors (3000-3099)
  {
    code: 'APT_001',
    name: 'Slot Not Available',
    description: 'The selected appointment slot is no longer available.',
    causes: ['Slot already booked', 'Provider schedule changed', 'Time passed'],
    solutions: ['Select different time slot', 'Refresh availability', 'Try another provider'],
    severity: 'low',
    category: 'Appointment',
  },
  {
    code: 'APT_002',
    name: 'Provider Unavailable',
    description: 'The provider is not available during the requested time.',
    causes: ['Outside working hours', 'Provider on leave', 'Schedule blocked'],
    solutions: ['Check provider schedule', 'Select different date', 'Try another provider'],
    severity: 'low',
    category: 'Appointment',
  },
  {
    code: 'APT_003',
    name: 'Booking Window Exceeded',
    description: 'Appointments cannot be booked this far in advance.',
    causes: ['Exceeds 60-day booking window', 'Organization policy restriction'],
    solutions: ['Select date within booking window', 'Contact administrator for exception'],
    severity: 'low',
    category: 'Appointment',
  },
  {
    code: 'APT_004',
    name: 'Insufficient Notice',
    description: 'Appointments require minimum advance notice.',
    causes: ['Less than 2 hours notice', 'Same-day booking disabled'],
    solutions: ['Book further in advance', 'Use walk-in option if available'],
    severity: 'low',
    category: 'Appointment',
  },
  {
    code: 'APT_005',
    name: 'Cancellation Not Allowed',
    description: 'This appointment can no longer be cancelled.',
    causes: ['Within 24-hour window', 'Appointment already started', 'Visit completed'],
    solutions: ['Contact clinic directly', 'Mark as no-show if applicable'],
    severity: 'medium',
    category: 'Appointment',
  },

  // Clinical Errors (4000-4099)
  {
    code: 'CLIN_001',
    name: 'Visit Already Signed',
    description: 'This clinical visit has been signed and cannot be modified.',
    causes: ['Visit locked after signing', 'Billing already processed'],
    solutions: ['Create addendum for corrections', 'Contact medical records'],
    severity: 'medium',
    category: 'Clinical',
  },
  {
    code: 'CLIN_002',
    name: 'Drug Interaction Detected',
    description: 'Potential drug interaction detected with current medications.',
    causes: ['Contraindicated combination', 'Duplicate therapy', 'Allergy alert'],
    solutions: ['Review interaction details', 'Modify prescription', 'Override with justification'],
    severity: 'high',
    category: 'Clinical',
  },
  {
    code: 'CLIN_003',
    name: 'Invalid ICD-10 Code',
    description: 'The diagnosis code entered is invalid or inactive.',
    causes: ['Outdated code', 'Typo in code', 'Code not billable'],
    solutions: ['Search for correct code', 'Verify in ICD-10 database', 'Select active code'],
    severity: 'low',
    category: 'Clinical',
  },
  {
    code: 'CLIN_004',
    name: 'Prescription Limit Exceeded',
    description: 'Controlled substance prescription exceeds allowed limits.',
    causes: ['Exceeds maximum quantity', 'Early refill requested', 'DEA limit reached'],
    solutions: ['Reduce quantity', 'Wait until eligible for refill', 'Document justification'],
    severity: 'high',
    category: 'Clinical',
  },

  // Financial Errors (5000-5099)
  {
    code: 'FIN_001',
    name: 'Payment Failed',
    description: 'The payment transaction could not be processed.',
    causes: ['Card declined', 'Insufficient funds', 'Network error', 'Invalid card'],
    solutions: ['Verify card details', 'Try different payment method', 'Contact bank'],
    severity: 'medium',
    category: 'Financial',
  },
  {
    code: 'FIN_002',
    name: 'Invoice Already Paid',
    description: 'This invoice has already been paid in full.',
    causes: ['Duplicate payment attempt', 'Payment recorded elsewhere'],
    solutions: ['Verify payment history', 'Check for duplicate payments', 'Process refund if needed'],
    severity: 'low',
    category: 'Financial',
  },
  {
    code: 'FIN_003',
    name: 'Claim Rejected',
    description: 'The insurance claim was rejected by the payer.',
    causes: ['Missing information', 'Invalid codes', 'Coverage issue', 'Pre-auth required'],
    solutions: ['Review rejection reason', 'Correct errors', 'Submit appeal if appropriate'],
    severity: 'medium',
    category: 'Financial',
  },
  {
    code: 'FIN_004',
    name: 'Discount Limit Exceeded',
    description: 'The requested discount exceeds your authorization limit.',
    causes: ['Discount above threshold', 'Daily limit reached'],
    solutions: ['Request manager approval', 'Apply smaller discount'],
    severity: 'low',
    category: 'Financial',
  },

  // System Errors (9000-9099)
  {
    code: 'SYS_001',
    name: 'Service Unavailable',
    description: 'The service is temporarily unavailable.',
    causes: ['Scheduled maintenance', 'High system load', 'Server issue'],
    solutions: ['Wait and retry', 'Check status page', 'Contact support if persistent'],
    severity: 'high',
    category: 'System',
  },
  {
    code: 'SYS_002',
    name: 'Network Error',
    description: 'Unable to connect to the server.',
    causes: ['Internet connection issue', 'VPN disconnected', 'Firewall blocking'],
    solutions: ['Check internet connection', 'Reconnect VPN', 'Try different network'],
    severity: 'medium',
    category: 'System',
  },
  {
    code: 'SYS_003',
    name: 'Timeout Error',
    description: 'The operation timed out before completing.',
    causes: ['Slow network', 'Large data processing', 'Server overload'],
    solutions: ['Retry the operation', 'Try during off-peak hours', 'Break into smaller operations'],
    severity: 'medium',
    category: 'System',
  },
  {
    code: 'SYS_004',
    name: 'Data Sync Error',
    description: 'Data synchronization failed between systems.',
    causes: ['Integration down', 'Data format mismatch', 'External system unavailable'],
    solutions: ['Retry sync', 'Check integration status', 'Contact IT support'],
    severity: 'medium',
    category: 'System',
  },
  {
    code: 'SYS_005',
    name: 'Database Error',
    description: 'An unexpected database error occurred.',
    causes: ['Query error', 'Connection pool exhausted', 'Database maintenance'],
    solutions: ['Refresh the page', 'Try again in a few minutes', 'Report to IT if persistent'],
    severity: 'high',
    category: 'System',
  },

  // Integration Errors (6000-6099)
  {
    code: 'INT_001',
    name: 'Lab Interface Error',
    description: 'Unable to communicate with the laboratory system.',
    causes: ['LIS offline', 'Authentication failed', 'Network issue'],
    solutions: ['Check LIS status', 'Verify credentials', 'Contact lab IT'],
    severity: 'high',
    category: 'Integration',
  },
  {
    code: 'INT_002',
    name: 'PACS Connection Failed',
    description: 'Unable to connect to the imaging system.',
    causes: ['PACS server down', 'DICOM network issue', 'Invalid AE title'],
    solutions: ['Verify PACS status', 'Check network settings', 'Contact radiology IT'],
    severity: 'high',
    category: 'Integration',
  },
  {
    code: 'INT_003',
    name: 'Payment Gateway Error',
    description: 'Unable to process payment through the gateway.',
    causes: ['Gateway maintenance', 'API credentials expired', 'Account issue'],
    solutions: ['Try alternative payment', 'Verify gateway status', 'Update credentials'],
    severity: 'medium',
    category: 'Integration',
  },
];

const severityColors: Record<string, string> = {
  low: 'bg-green-100 text-green-700',
  medium: 'bg-yellow-100 text-yellow-700',
  high: 'bg-orange-100 text-orange-700',
  critical: 'bg-red-100 text-red-700',
};

const categoryIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  'Account & Login': Lock,
  'Patients': Users,
  'Appointments': Calendar,
  'Clinical': FileText,
  'Financial': DollarSign,
  'Technical': Server,
  'Administration': Settings,
  'Authentication': Key,
  'Authorization': Shield,
  'Patient': Users,
  'Appointment': Calendar,
  'System': Database,
  'Integration': Wifi,
};

export default function FAQTroubleshooting() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFAQCategory, setSelectedFAQCategory] = useState<string>('all');
  const [selectedErrorCategory, setSelectedErrorCategory] = useState<string>('all');
  const [expandedFAQs, setExpandedFAQs] = useState<Set<number>>(new Set());
  const [expandedErrors, setExpandedErrors] = useState<Set<string>>(new Set());

  const faqCategories = ['all', ...Array.from(new Set(faqs.map((f) => f.category)))];
  const errorCategories = ['all', ...Array.from(new Set(errorCodes.map((e) => e.category)))];

  const filteredFAQs = faqs.filter((faq) => {
    const matchesSearch =
      searchQuery === '' ||
      faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.tags?.some((t) => t.toLowerCase().includes(searchQuery.toLowerCase()));
    const matchesCategory = selectedFAQCategory === 'all' || faq.category === selectedFAQCategory;
    return matchesSearch && matchesCategory;
  });

  const filteredErrors = errorCodes.filter((error) => {
    const matchesSearch =
      searchQuery === '' ||
      error.code.toLowerCase().includes(searchQuery.toLowerCase()) ||
      error.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      error.description.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedErrorCategory === 'all' || error.category === selectedErrorCategory;
    return matchesSearch && matchesCategory;
  });

  const toggleFAQ = (index: number) => {
    const newExpanded = new Set(expandedFAQs);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedFAQs(newExpanded);
  };

  const toggleError = (code: string) => {
    const newExpanded = new Set(expandedErrors);
    if (newExpanded.has(code)) {
      newExpanded.delete(code);
    } else {
      newExpanded.add(code);
    }
    setExpandedErrors(newExpanded);
  };

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">FAQ & Troubleshooting</h1>
        <p className="text-lg text-gray-600">
          Find answers to common questions and solutions to known issues. Can't find
          what you're looking for? Contact our support team.
        </p>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search FAQs and error codes..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="w-full pl-12 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        />
      </div>

      {/* Quick Links */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <a
          href="#faqs"
          className="p-4 bg-white border border-gray-200 rounded-xl hover:border-primary-300 transition-colors"
        >
          <HelpCircle className="h-6 w-6 text-primary-600 mb-2" />
          <h3 className="font-medium text-gray-900">FAQs</h3>
          <p className="text-sm text-gray-500">{faqs.length} questions answered</p>
        </a>
        <a
          href="#error-codes"
          className="p-4 bg-white border border-gray-200 rounded-xl hover:border-primary-300 transition-colors"
        >
          <AlertTriangle className="h-6 w-6 text-yellow-600 mb-2" />
          <h3 className="font-medium text-gray-900">Error Codes</h3>
          <p className="text-sm text-gray-500">{errorCodes.length} codes documented</p>
        </a>
        <Link
          to="/docs/admin-configuration"
          className="p-4 bg-white border border-gray-200 rounded-xl hover:border-primary-300 transition-colors"
        >
          <Settings className="h-6 w-6 text-gray-600 mb-2" />
          <h3 className="font-medium text-gray-900">Configuration</h3>
          <p className="text-sm text-gray-500">Admin settings guide</p>
        </Link>
        <Link
          to="/docs/api-reference"
          className="p-4 bg-white border border-gray-200 rounded-xl hover:border-primary-300 transition-colors"
        >
          <Zap className="h-6 w-6 text-green-600 mb-2" />
          <h3 className="font-medium text-gray-900">API Reference</h3>
          <p className="text-sm text-gray-500">Integration help</p>
        </Link>
      </div>

      {/* FAQs Section */}
      <section id="faqs">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-2xl font-semibold text-gray-900">Frequently Asked Questions</h2>
          <span className="text-sm text-gray-500">{filteredFAQs.length} questions</span>
        </div>

        {/* Category Filter */}
        <div className="flex flex-wrap gap-2 mb-6">
          {faqCategories.map((category) => (
            <button
              key={category}
              onClick={() => setSelectedFAQCategory(category)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                selectedFAQCategory === category
                  ? 'bg-primary-600 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              {category === 'all' ? 'All Categories' : category}
            </button>
          ))}
        </div>

        {/* FAQ List */}
        <div className="space-y-3">
          {filteredFAQs.map((faq, index) => {
            const Icon = categoryIcons[faq.category] || HelpCircle;
            return (
              <div
                key={index}
                className="bg-white border border-gray-200 rounded-lg overflow-hidden"
              >
                <button
                  onClick={() => toggleFAQ(index)}
                  className="w-full px-5 py-4 flex items-center justify-between hover:bg-gray-50 transition-colors text-left"
                >
                  <div className="flex items-center gap-3">
                    <Icon className="h-5 w-5 text-gray-400 flex-shrink-0" />
                    <span className="font-medium text-gray-900">{faq.question}</span>
                  </div>
                  {expandedFAQs.has(index) ? (
                    <ChevronDown className="h-5 w-5 text-gray-400 flex-shrink-0" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-gray-400 flex-shrink-0" />
                  )}
                </button>
                {expandedFAQs.has(index) && (
                  <div className="px-5 pb-4 pt-0">
                    <p className="text-gray-600 pl-8">{faq.answer}</p>
                    {faq.tags && (
                      <div className="flex flex-wrap gap-1 mt-3 pl-8">
                        {faq.tags.map((tag) => (
                          <span
                            key={tag}
                            className="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded"
                          >
                            #{tag}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </section>

      {/* Error Codes Section */}
      <section id="error-codes">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-2xl font-semibold text-gray-900">Error Codes & Solutions</h2>
          <span className="text-sm text-gray-500">{filteredErrors.length} codes</span>
        </div>

        {/* Category Filter */}
        <div className="flex flex-wrap gap-2 mb-6">
          {errorCategories.map((category) => (
            <button
              key={category}
              onClick={() => setSelectedErrorCategory(category)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                selectedErrorCategory === category
                  ? 'bg-primary-600 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              {category === 'all' ? 'All Categories' : category}
            </button>
          ))}
        </div>

        {/* Error List */}
        <div className="space-y-3">
          {filteredErrors.map((error) => {
            const Icon = categoryIcons[error.category] || AlertTriangle;
            return (
              <div
                key={error.code}
                className="bg-white border border-gray-200 rounded-lg overflow-hidden"
              >
                <button
                  onClick={() => toggleError(error.code)}
                  className="w-full px-5 py-4 flex items-center justify-between hover:bg-gray-50 transition-colors text-left"
                >
                  <div className="flex items-center gap-3">
                    <Icon className="h-5 w-5 text-gray-400 flex-shrink-0" />
                    <code className="font-mono text-sm bg-gray-100 px-2 py-0.5 rounded">
                      {error.code}
                    </code>
                    <span className="font-medium text-gray-900">{error.name}</span>
                    <span className={`px-2 py-0.5 rounded text-xs font-medium ${severityColors[error.severity]}`}>
                      {error.severity}
                    </span>
                  </div>
                  {expandedErrors.has(error.code) ? (
                    <ChevronDown className="h-5 w-5 text-gray-400 flex-shrink-0" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-gray-400 flex-shrink-0" />
                  )}
                </button>
                {expandedErrors.has(error.code) && (
                  <div className="px-5 pb-4 space-y-4">
                    <p className="text-gray-600">{error.description}</p>

                    <div className="grid md:grid-cols-2 gap-4">
                      <div className="bg-red-50 rounded-lg p-4">
                        <h4 className="font-medium text-red-800 mb-2 flex items-center gap-2">
                          <XCircle className="h-4 w-4" /> Possible Causes
                        </h4>
                        <ul className="space-y-1">
                          {error.causes.map((cause, i) => (
                            <li key={i} className="text-sm text-red-700 flex items-start gap-2">
                              <span>•</span> {cause}
                            </li>
                          ))}
                        </ul>
                      </div>

                      <div className="bg-green-50 rounded-lg p-4">
                        <h4 className="font-medium text-green-800 mb-2 flex items-center gap-2">
                          <CheckCircle className="h-4 w-4" /> Solutions
                        </h4>
                        <ul className="space-y-1">
                          {error.solutions.map((solution, i) => (
                            <li key={i} className="text-sm text-green-700 flex items-start gap-2">
                              <span>•</span> {solution}
                            </li>
                          ))}
                        </ul>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </section>

      {/* Common Issues Quick Reference */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Common Issues Quick Reference</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Issue</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Quick Fix</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Page not loading</td>
                <td className="px-4 py-3 text-gray-600">Clear browser cache, try incognito mode, check network</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Can't see patient data</td>
                <td className="px-4 py-3 text-gray-600">Check branch assignment, verify PHI permissions</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Reports showing wrong data</td>
                <td className="px-4 py-3 text-gray-600">Verify date range and filters, check timezone settings</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Email notifications not arriving</td>
                <td className="px-4 py-3 text-gray-600">Check spam folder, verify SMTP settings, test connection</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Printing issues</td>
                <td className="px-4 py-3 text-gray-600">Try different browser, check popup blocker, use Chrome</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Slow performance</td>
                <td className="px-4 py-3 text-gray-600">Close unused tabs, clear cache, check network speed</td>
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium text-gray-900">Mobile layout issues</td>
                <td className="px-4 py-3 text-gray-600">Update browser, try landscape mode, clear mobile cache</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      {/* Still Need Help */}
      <section className="bg-gradient-to-r from-primary-50 to-blue-50 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <div className="p-3 bg-white rounded-xl shadow-sm">
            <Headphones className="h-8 w-8 text-primary-600" />
          </div>
          <div>
            <h3 className="text-xl font-semibold text-gray-900">Still Need Help?</h3>
            <p className="text-gray-600 mt-1">
              Our support team is available to assist you with any issues not covered here.
            </p>
            <div className="mt-4 flex flex-wrap gap-3">
              <a
                href="mailto:support@xenonclinic.com"
                className="inline-flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg font-medium hover:bg-primary-700 transition-colors"
              >
                <Mail className="h-4 w-4" /> Email Support
              </a>
              <a
                href="#"
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg font-medium hover:bg-gray-50 transition-colors"
              >
                <MessageSquare className="h-4 w-4" /> Live Chat
              </a>
            </div>
            <div className="mt-4 text-sm text-gray-600">
              <p>
                <strong>Support Hours:</strong> Sunday - Thursday, 8:00 AM - 6:00 PM (GST)
              </p>
              <p>
                <strong>Emergency Support:</strong> 24/7 for critical issues (Enterprise plans)
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
