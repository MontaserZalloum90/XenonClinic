import { Tag, Calendar, ArrowUp, Bug, Zap, Shield, AlertTriangle } from 'lucide-react';

interface ReleaseNote {
  version: string;
  date: string;
  type: 'major' | 'minor' | 'patch';
  highlights: string[];
  changes: {
    category: 'feature' | 'improvement' | 'fix' | 'security';
    description: string;
    module?: string;
  }[];
  breakingChanges?: string[];
  upgradeNotes?: string[];
}

const releases: ReleaseNote[] = [
  {
    version: '2.5.0',
    date: '2024-12-01',
    type: 'minor',
    highlights: [
      'Audiology module with comprehensive hearing test support',
      'Enhanced workflow engine with conditional branching',
      'Patient portal mobile app integration',
    ],
    changes: [
      { category: 'feature', description: 'Added Audiology module with audiogram recording, hearing aid management, and follow-up tracking', module: 'Audiology' },
      { category: 'feature', description: 'Workflow engine now supports conditional branching and parallel task execution', module: 'Workflow' },
      { category: 'feature', description: 'Patient portal mobile app API endpoints for iOS and Android integration', module: 'Patient Portal' },
      { category: 'improvement', description: 'Dashboard load time reduced by 40% through query optimization', module: 'Analytics' },
      { category: 'improvement', description: 'Added bulk patient import from CSV with validation preview', module: 'Patients' },
      { category: 'fix', description: 'Fixed appointment overlap detection for recurring appointments', module: 'Appointments' },
      { category: 'fix', description: 'Corrected insurance eligibility check timeout handling', module: 'Financial' },
      { category: 'security', description: 'Enhanced audit logging for PHI access with detailed context', module: 'Security' },
    ],
    upgradeNotes: [
      'Run database migrations before deploying new version',
      'Clear browser cache after upgrade for new features',
    ],
  },
  {
    version: '2.4.2',
    date: '2024-11-15',
    type: 'patch',
    highlights: [
      'Critical security patch for session management',
      'Performance improvements for large patient lists',
    ],
    changes: [
      { category: 'security', description: 'Fixed session fixation vulnerability in authentication flow', module: 'Security' },
      { category: 'fix', description: 'Resolved memory leak in real-time notification service', module: 'System' },
      { category: 'fix', description: 'Fixed pagination issue with filtered patient search results', module: 'Patients' },
      { category: 'improvement', description: 'Improved lab result PDF rendering for long reports', module: 'Laboratory' },
    ],
    upgradeNotes: [
      'CRITICAL: Apply this update immediately for security fix',
      'All active sessions will be invalidated upon upgrade',
    ],
  },
  {
    version: '2.4.1',
    date: '2024-11-01',
    type: 'patch',
    highlights: [
      'Bug fixes for analytics dashboard',
      'Improved claim submission reliability',
    ],
    changes: [
      { category: 'fix', description: 'Fixed chart rendering issue in analytics dashboard on Safari', module: 'Analytics' },
      { category: 'fix', description: 'Resolved claim submission timeout for large batches', module: 'Financial' },
      { category: 'fix', description: 'Corrected date formatting issue in Arabic locale', module: 'System' },
      { category: 'improvement', description: 'Better error messages for failed API calls', module: 'System' },
    ],
  },
  {
    version: '2.4.0',
    date: '2024-10-20',
    type: 'minor',
    highlights: [
      'New analytics dashboard with custom report builder',
      'Insurance claim auto-submission feature',
      'Multi-language support (Spanish, Arabic, French)',
    ],
    changes: [
      { category: 'feature', description: 'Custom report builder with drag-and-drop interface', module: 'Analytics' },
      { category: 'feature', description: 'Automated insurance claim submission with status tracking', module: 'Financial' },
      { category: 'feature', description: 'Added Spanish, Arabic, and French language support', module: 'System' },
      { category: 'feature', description: 'Electronic prescription integration with major pharmacy networks', module: 'Pharmacy' },
      { category: 'improvement', description: 'Redesigned appointment calendar with better mobile support', module: 'Appointments' },
      { category: 'improvement', description: 'Enhanced search with phonetic matching for patient names', module: 'Patients' },
      { category: 'fix', description: 'Fixed duplicate notification issue for appointment reminders', module: 'Notifications' },
      { category: 'fix', description: 'Resolved timezone display issue in scheduled reports', module: 'Analytics' },
    ],
    breakingChanges: [
      'API endpoint /api/reports/generate now requires additional parameters',
      'Report export format changed from legacy XML to JSON',
    ],
    upgradeNotes: [
      'Review API integration code for reports endpoint changes',
      'Update any scheduled report configurations',
    ],
  },
  {
    version: '2.3.1',
    date: '2024-09-15',
    type: 'patch',
    highlights: [
      'Telehealth stability improvements',
      'Inventory calculation fixes',
    ],
    changes: [
      { category: 'fix', description: 'Fixed video call disconnection issue on slow networks', module: 'Telehealth' },
      { category: 'fix', description: 'Corrected inventory quantity calculation after transfers', module: 'Inventory' },
      { category: 'fix', description: 'Resolved issue with HIPAA report date range selection', module: 'Security' },
      { category: 'improvement', description: 'Added retry logic for failed video connections', module: 'Telehealth' },
    ],
  },
  {
    version: '2.3.0',
    date: '2024-09-01',
    type: 'minor',
    highlights: [
      'Telehealth video consultation integration',
      'Inventory alerts and auto-reorder system',
      'Enhanced HIPAA compliance reporting',
    ],
    changes: [
      { category: 'feature', description: 'Integrated video consultation with waiting room and recording', module: 'Telehealth' },
      { category: 'feature', description: 'Low stock alerts with configurable thresholds and auto-reorder', module: 'Inventory' },
      { category: 'feature', description: 'HIPAA compliance dashboard with audit report generation', module: 'Security' },
      { category: 'improvement', description: 'Faster lab result display with progressive loading', module: 'Laboratory' },
      { category: 'improvement', description: 'Added keyboard shortcuts for common actions', module: 'System' },
      { category: 'fix', description: 'Fixed PDF export issue for clinical notes with images', module: 'Clinical' },
    ],
  },
  {
    version: '2.2.1',
    date: '2024-08-01',
    type: 'patch',
    highlights: [
      'HR module bug fixes',
      'Inventory transfer improvements',
    ],
    changes: [
      { category: 'fix', description: 'Fixed shift overlap validation when editing schedules', module: 'HR' },
      { category: 'fix', description: 'Resolved inventory transfer notification not being sent', module: 'Inventory' },
      { category: 'fix', description: 'Corrected timezone issue in employee attendance reports', module: 'HR' },
      { category: 'improvement', description: 'Better validation messages for scheduling conflicts', module: 'HR' },
    ],
  },
  {
    version: '2.2.0',
    date: '2024-07-15',
    type: 'minor',
    highlights: [
      'Multi-branch inventory tracking',
      'Employee scheduling and shift management',
      'Patient communication preferences',
    ],
    changes: [
      { category: 'feature', description: 'Track inventory separately per branch with transfer requests', module: 'Inventory' },
      { category: 'feature', description: 'Employee shift scheduling with conflict detection', module: 'HR' },
      { category: 'feature', description: 'Patient communication preferences for email, SMS, and calls', module: 'Patients' },
      { category: 'improvement', description: 'Improved insurance eligibility response time', module: 'Financial' },
      { category: 'fix', description: 'Fixed appointment reminder not sent for rescheduled visits', module: 'Appointments' },
      { category: 'fix', description: 'Resolved duplicate entry issue in batch payment posting', module: 'Financial' },
    ],
  },
  {
    version: '2.1.2',
    date: '2024-06-20',
    type: 'patch',
    highlights: [
      'Critical fix for patient data export',
      'Performance improvements for large clinics',
    ],
    changes: [
      { category: 'fix', description: 'Fixed patient data export failing for records with special characters', module: 'Patients' },
      { category: 'fix', description: 'Resolved slow query performance for clinics with 50,000+ patients', module: 'System' },
      { category: 'security', description: 'Added additional validation for file upload endpoints', module: 'Security' },
      { category: 'improvement', description: 'Optimized database indexes for common search queries', module: 'System' },
    ],
  },
  {
    version: '2.1.1',
    date: '2024-06-01',
    type: 'patch',
    highlights: [
      'Radiology module fixes',
      'Insurance claim improvements',
    ],
    changes: [
      { category: 'fix', description: 'Fixed DICOM image viewer not loading on certain browsers', module: 'Radiology' },
      { category: 'fix', description: 'Resolved issue with ERA file parsing for certain payers', module: 'Financial' },
      { category: 'fix', description: 'Corrected radiology report template variable substitution', module: 'Radiology' },
      { category: 'improvement', description: 'Added support for additional ERA file formats', module: 'Financial' },
    ],
  },
  {
    version: '2.1.0',
    date: '2024-05-15',
    type: 'minor',
    highlights: [
      'Radiology module with PACS integration',
      'Enhanced billing with ERA auto-posting',
      'DOH UAE compliance features',
    ],
    changes: [
      { category: 'feature', description: 'Full radiology module with order management and DICOM viewer', module: 'Radiology' },
      { category: 'feature', description: 'PACS integration for medical image storage and retrieval', module: 'Radiology' },
      { category: 'feature', description: 'Electronic Remittance Advice (ERA) auto-posting', module: 'Financial' },
      { category: 'feature', description: 'DOH UAE compliance reporting and license tracking', module: 'Compliance' },
      { category: 'improvement', description: 'Faster claim submission with background processing', module: 'Financial' },
      { category: 'improvement', description: 'Added batch lab order entry for efficiency', module: 'Laboratory' },
      { category: 'fix', description: 'Fixed issue with recurring appointment series deletion', module: 'Appointments' },
    ],
    breakingChanges: [
      'Radiology orders now require modality specification',
      'Insurance payer codes updated to latest standards',
    ],
  },
  {
    version: '2.0.3',
    date: '2024-04-15',
    type: 'patch',
    highlights: [
      'Security patches and performance improvements',
    ],
    changes: [
      { category: 'security', description: 'Updated authentication tokens to use shorter expiration times', module: 'Security' },
      { category: 'security', description: 'Added rate limiting to API endpoints', module: 'Security' },
      { category: 'fix', description: 'Fixed session timeout not redirecting to login page', module: 'Security' },
      { category: 'improvement', description: 'Reduced memory usage in long-running background jobs', module: 'System' },
    ],
  },
  {
    version: '2.0.2',
    date: '2024-04-01',
    type: 'patch',
    highlights: [
      'Multi-tenant bug fixes',
      'Improved data isolation',
    ],
    changes: [
      { category: 'fix', description: 'Fixed tenant data isolation issue in shared report queries', module: 'Multi-Tenancy' },
      { category: 'fix', description: 'Resolved branch switching not updating user context properly', module: 'Multi-Tenancy' },
      { category: 'fix', description: 'Corrected permission check for cross-branch data access', module: 'Security' },
      { category: 'improvement', description: 'Added tenant ID validation to all API endpoints', module: 'Security' },
    ],
    upgradeNotes: [
      'Database migration required for data isolation fix',
      'Users may need to log out and back in after upgrade',
    ],
  },
  {
    version: '2.0.1',
    date: '2024-03-15',
    type: 'patch',
    highlights: [
      'Post-launch bug fixes',
      'UI improvements based on feedback',
    ],
    changes: [
      { category: 'fix', description: 'Fixed dashboard widgets not loading on first login', module: 'Dashboard' },
      { category: 'fix', description: 'Resolved calendar view not showing all-day appointments correctly', module: 'Appointments' },
      { category: 'fix', description: 'Corrected patient search not finding records with accented characters', module: 'Patients' },
      { category: 'improvement', description: 'Improved contrast for better accessibility', module: 'UI' },
      { category: 'improvement', description: 'Added loading indicators for slow operations', module: 'UI' },
    ],
  },
  {
    version: '2.0.0',
    date: '2024-03-01',
    type: 'major',
    highlights: [
      'Complete UI redesign with modern interface',
      'Multi-tenant architecture for enterprise deployments',
      'New workflow automation engine',
      'Enhanced security with MFA and SSO support',
    ],
    changes: [
      { category: 'feature', description: 'Completely redesigned user interface with responsive design', module: 'UI' },
      { category: 'feature', description: 'Multi-tenant architecture supporting isolated organizations', module: 'Multi-Tenancy' },
      { category: 'feature', description: 'Visual workflow builder for custom automation', module: 'Workflow' },
      { category: 'feature', description: 'Multi-factor authentication with TOTP and SMS options', module: 'Security' },
      { category: 'feature', description: 'Single sign-on integration with SAML and OAuth', module: 'Security' },
      { category: 'feature', description: 'Role-based access control with 50+ granular permissions', module: 'Security' },
      { category: 'feature', description: 'Real-time notifications and in-app messaging', module: 'System' },
      { category: 'improvement', description: 'Faster page load times with code splitting', module: 'System' },
      { category: 'improvement', description: 'Better mobile experience with touch-optimized controls', module: 'UI' },
      { category: 'security', description: 'End-to-end encryption for all PHI data', module: 'Security' },
    ],
    breakingChanges: [
      'Complete API overhaul - v1 API endpoints deprecated',
      'New authentication flow requires client updates',
      'Database schema changes require full migration',
      'Custom reports must be recreated in new format',
    ],
    upgradeNotes: [
      'CRITICAL: Full database backup required before upgrade',
      'Plan for 2-4 hours of downtime for migration',
      'Review API documentation for v2 endpoint changes',
      'All users will need to reset passwords',
    ],
  },
  {
    version: '1.9.5',
    date: '2024-02-15',
    type: 'patch',
    highlights: [
      'Final v1.x maintenance release',
      'Preparation for v2.0 migration',
    ],
    changes: [
      { category: 'fix', description: 'Fixed remaining v1.x bug reports', module: 'System' },
      { category: 'improvement', description: 'Added data export tools for v2.0 migration', module: 'System' },
      { category: 'security', description: 'Security patches for end-of-support preparation', module: 'Security' },
    ],
    upgradeNotes: [
      'This is the final v1.x release',
      'Begin planning migration to v2.0',
    ],
  },
  {
    version: '1.9.4',
    date: '2024-01-15',
    type: 'patch',
    highlights: [
      'Year-end billing improvements',
      'Insurance renewal processing',
    ],
    changes: [
      { category: 'fix', description: 'Fixed year-end date handling in billing reports', module: 'Financial' },
      { category: 'fix', description: 'Resolved insurance policy renewal date calculation', module: 'Financial' },
      { category: 'improvement', description: 'Added year-end summary reports', module: 'Analytics' },
    ],
  },
  {
    version: '1.9.3',
    date: '2023-12-01',
    type: 'patch',
    highlights: [
      'Holiday scheduling improvements',
      'Performance optimizations',
    ],
    changes: [
      { category: 'fix', description: 'Fixed appointment booking across holiday periods', module: 'Appointments' },
      { category: 'fix', description: 'Resolved lab result notification delays', module: 'Laboratory' },
      { category: 'improvement', description: 'Optimized database queries for end-of-year reports', module: 'System' },
    ],
  },
  {
    version: '1.9.2',
    date: '2023-11-01',
    type: 'patch',
    highlights: [
      'Pharmacy module improvements',
      'Lab interface stability',
    ],
    changes: [
      { category: 'fix', description: 'Fixed drug interaction alert not showing for certain combinations', module: 'Pharmacy' },
      { category: 'fix', description: 'Resolved HL7 message parsing errors for specific lab equipment', module: 'Laboratory' },
      { category: 'improvement', description: 'Better handling of discontinued medications in formulary', module: 'Pharmacy' },
    ],
  },
  {
    version: '1.9.1',
    date: '2023-10-15',
    type: 'patch',
    highlights: [
      'Clinical notes improvements',
      'Print functionality fixes',
    ],
    changes: [
      { category: 'fix', description: 'Fixed clinical note templates not saving custom fields', module: 'Clinical' },
      { category: 'fix', description: 'Resolved print preview showing incorrect page breaks', module: 'System' },
      { category: 'fix', description: 'Corrected patient statement printing with missing charges', module: 'Financial' },
    ],
  },
  {
    version: '1.9.0',
    date: '2023-10-01',
    type: 'minor',
    highlights: [
      'Laboratory module enhancements',
      'Insurance claim tracking improvements',
      'Patient self-scheduling',
    ],
    changes: [
      { category: 'feature', description: 'HL7 interface for automated lab equipment integration', module: 'Laboratory' },
      { category: 'feature', description: 'Real-time insurance claim status tracking', module: 'Financial' },
      { category: 'feature', description: 'Patient self-scheduling through online portal', module: 'Patient Portal' },
      { category: 'feature', description: 'Lab panel management with custom test groupings', module: 'Laboratory' },
      { category: 'improvement', description: 'Faster lab result entry with barcode scanning', module: 'Laboratory' },
      { category: 'improvement', description: 'Enhanced claim denial management workflow', module: 'Financial' },
      { category: 'fix', description: 'Fixed issue with lab critical value alerts', module: 'Laboratory' },
    ],
  },
  {
    version: '1.8.0',
    date: '2023-08-01',
    type: 'minor',
    highlights: [
      'Pharmacy module with e-prescribing',
      'Drug interaction checking',
      'Medication history tracking',
    ],
    changes: [
      { category: 'feature', description: 'Full pharmacy module with dispensing workflow', module: 'Pharmacy' },
      { category: 'feature', description: 'E-prescribing integration with pharmacy networks', module: 'Pharmacy' },
      { category: 'feature', description: 'Drug-drug and drug-allergy interaction checking', module: 'Pharmacy' },
      { category: 'feature', description: 'Controlled substance tracking with DEA compliance', module: 'Pharmacy' },
      { category: 'improvement', description: 'Medication history view in patient chart', module: 'Clinical' },
      { category: 'fix', description: 'Fixed prescription print format issues', module: 'Pharmacy' },
    ],
  },
  {
    version: '1.7.0',
    date: '2023-06-01',
    type: 'minor',
    highlights: [
      'Financial module with insurance billing',
      'Claims submission and tracking',
      'Patient statements and collections',
    ],
    changes: [
      { category: 'feature', description: 'Insurance claim generation and submission', module: 'Financial' },
      { category: 'feature', description: 'Electronic eligibility verification', module: 'Financial' },
      { category: 'feature', description: 'Patient statement generation and mailing', module: 'Financial' },
      { category: 'feature', description: 'Payment posting with automatic allocation', module: 'Financial' },
      { category: 'feature', description: 'Fee schedule management per insurance', module: 'Financial' },
      { category: 'improvement', description: 'Charge capture from clinical encounters', module: 'Financial' },
    ],
  },
  {
    version: '1.6.0',
    date: '2023-04-01',
    type: 'minor',
    highlights: [
      'Analytics and reporting module',
      'Custom report builder',
      'Scheduled report delivery',
    ],
    changes: [
      { category: 'feature', description: 'Comprehensive analytics dashboard with key metrics', module: 'Analytics' },
      { category: 'feature', description: 'Custom report builder with field selection', module: 'Analytics' },
      { category: 'feature', description: 'Scheduled reports with email delivery', module: 'Analytics' },
      { category: 'feature', description: 'Export reports to PDF, Excel, and CSV', module: 'Analytics' },
      { category: 'improvement', description: 'Role-based report access controls', module: 'Analytics' },
    ],
  },
  {
    version: '1.5.0',
    date: '2023-02-01',
    type: 'minor',
    highlights: [
      'Laboratory module launch',
      'Lab order management',
      'Result entry and reporting',
    ],
    changes: [
      { category: 'feature', description: 'Laboratory order entry and tracking', module: 'Laboratory' },
      { category: 'feature', description: 'Lab result entry with reference ranges', module: 'Laboratory' },
      { category: 'feature', description: 'Critical value flagging and alerts', module: 'Laboratory' },
      { category: 'feature', description: 'Lab report generation and printing', module: 'Laboratory' },
      { category: 'improvement', description: 'Provider notification for abnormal results', module: 'Laboratory' },
    ],
  },
  {
    version: '1.4.0',
    date: '2022-12-01',
    type: 'minor',
    highlights: [
      'Clinical documentation enhancements',
      'SOAP note templates',
      'Clinical decision support',
    ],
    changes: [
      { category: 'feature', description: 'Customizable SOAP note templates', module: 'Clinical' },
      { category: 'feature', description: 'ICD-10 diagnosis search and selection', module: 'Clinical' },
      { category: 'feature', description: 'Procedure documentation with CPT codes', module: 'Clinical' },
      { category: 'feature', description: 'Clinical alerts for allergies and conditions', module: 'Clinical' },
      { category: 'improvement', description: 'Voice dictation support for notes', module: 'Clinical' },
    ],
  },
  {
    version: '1.3.0',
    date: '2022-10-01',
    type: 'minor',
    highlights: [
      'Inventory management module',
      'Stock tracking and alerts',
      'Purchase order management',
    ],
    changes: [
      { category: 'feature', description: 'Inventory tracking with stock levels', module: 'Inventory' },
      { category: 'feature', description: 'Low stock alerts and reorder points', module: 'Inventory' },
      { category: 'feature', description: 'Purchase order creation and tracking', module: 'Inventory' },
      { category: 'feature', description: 'Lot and expiration date tracking', module: 'Inventory' },
      { category: 'improvement', description: 'Barcode scanning for inventory management', module: 'Inventory' },
    ],
  },
  {
    version: '1.2.0',
    date: '2022-08-01',
    type: 'minor',
    highlights: [
      'Patient portal launch',
      'Online appointment booking',
      'Secure messaging',
    ],
    changes: [
      { category: 'feature', description: 'Patient portal with secure login', module: 'Patient Portal' },
      { category: 'feature', description: 'Online appointment viewing and requests', module: 'Patient Portal' },
      { category: 'feature', description: 'Secure messaging with providers', module: 'Patient Portal' },
      { category: 'feature', description: 'Lab result viewing for patients', module: 'Patient Portal' },
      { category: 'improvement', description: 'Mobile-responsive patient portal design', module: 'Patient Portal' },
    ],
  },
  {
    version: '1.1.0',
    date: '2022-06-01',
    type: 'minor',
    highlights: [
      'Appointment scheduling module',
      'Provider calendar management',
      'Appointment reminders',
    ],
    changes: [
      { category: 'feature', description: 'Visual appointment calendar by provider', module: 'Appointments' },
      { category: 'feature', description: 'Appointment type configuration with durations', module: 'Appointments' },
      { category: 'feature', description: 'Automated appointment reminders via SMS and email', module: 'Appointments' },
      { category: 'feature', description: 'Recurring appointment scheduling', module: 'Appointments' },
      { category: 'improvement', description: 'Drag-and-drop appointment rescheduling', module: 'Appointments' },
    ],
  },
  {
    version: '1.0.0',
    date: '2022-04-01',
    type: 'major',
    highlights: [
      'Initial release of XenonClinic',
      'Patient management foundation',
      'Basic clinical workflow',
      'User and role management',
    ],
    changes: [
      { category: 'feature', description: 'Patient registration and demographics management', module: 'Patients' },
      { category: 'feature', description: 'Patient search with advanced filters', module: 'Patients' },
      { category: 'feature', description: 'Insurance information management', module: 'Patients' },
      { category: 'feature', description: 'Basic clinical visit documentation', module: 'Clinical' },
      { category: 'feature', description: 'Vital signs recording', module: 'Clinical' },
      { category: 'feature', description: 'Allergy and medication history', module: 'Clinical' },
      { category: 'feature', description: 'User account management', module: 'Security' },
      { category: 'feature', description: 'Role-based permissions', module: 'Security' },
      { category: 'feature', description: 'Audit logging for compliance', module: 'Security' },
    ],
    upgradeNotes: [
      'Initial installation requires database setup',
      'Default admin account created during installation',
    ],
  },
];

const categoryConfig = {
  feature: { icon: Zap, label: 'New Feature', color: 'bg-green-100 text-green-700' },
  improvement: { icon: ArrowUp, label: 'Improvement', color: 'bg-blue-100 text-blue-700' },
  fix: { icon: Bug, label: 'Bug Fix', color: 'bg-yellow-100 text-yellow-700' },
  security: { icon: Shield, label: 'Security', color: 'bg-red-100 text-red-700' },
};

const typeConfig = {
  major: { label: 'Major Release', color: 'bg-purple-600' },
  minor: { label: 'Feature Release', color: 'bg-primary-600' },
  patch: { label: 'Patch', color: 'bg-gray-600' },
};

export default function ReleaseNotes() {
  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Release Notes</h1>
        <p className="text-lg text-gray-600">
          Stay up to date with the latest features, improvements, and fixes in XenonClinic.
        </p>
      </div>

      {/* Version Legend */}
      <div className="flex flex-wrap gap-4">
        {Object.entries(typeConfig).map(([key, config]) => (
          <div key={key} className="flex items-center gap-2">
            <span className={`w-3 h-3 rounded-full ${config.color}`} />
            <span className="text-sm text-gray-600">{config.label}</span>
          </div>
        ))}
      </div>

      {/* Release List */}
      <div className="space-y-8">
        {releases.map((release) => (
          <article
            key={release.version}
            className="bg-white border border-gray-200 rounded-xl overflow-hidden"
            id={`v${release.version}`}
          >
            {/* Release Header */}
            <div className="border-b border-gray-200 px-6 py-4 bg-gray-50">
              <div className="flex flex-wrap items-center gap-4 justify-between">
                <div className="flex items-center gap-3">
                  <Tag className="h-5 w-5 text-gray-400" />
                  <h2 className="text-xl font-bold text-gray-900">
                    Version {release.version}
                  </h2>
                  <span className={`px-2.5 py-0.5 rounded-full text-xs font-medium text-white ${typeConfig[release.type].color}`}>
                    {typeConfig[release.type].label}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-gray-500">
                  <Calendar className="h-4 w-4" />
                  <span className="text-sm">{release.date}</span>
                </div>
              </div>
            </div>

            {/* Highlights */}
            <div className="px-6 py-4 bg-primary-50 border-b border-primary-100">
              <h3 className="text-sm font-medium text-primary-800 mb-2">Highlights</h3>
              <ul className="space-y-1">
                {release.highlights.map((highlight, i) => (
                  <li key={i} className="flex items-start gap-2 text-primary-700">
                    <span className="text-primary-500">•</span>
                    {highlight}
                  </li>
                ))}
              </ul>
            </div>

            {/* Breaking Changes */}
            {release.breakingChanges && release.breakingChanges.length > 0 && (
              <div className="px-6 py-4 bg-red-50 border-b border-red-100">
                <div className="flex items-center gap-2 mb-2">
                  <AlertTriangle className="h-4 w-4 text-red-600" />
                  <h3 className="text-sm font-medium text-red-800">Breaking Changes</h3>
                </div>
                <ul className="space-y-1">
                  {release.breakingChanges.map((change, i) => (
                    <li key={i} className="flex items-start gap-2 text-red-700 text-sm">
                      <span className="text-red-500">•</span>
                      {change}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Changes */}
            <div className="px-6 py-4">
              <h3 className="text-sm font-medium text-gray-700 mb-3">All Changes</h3>
              <div className="space-y-2">
                {release.changes.map((change, i) => {
                  const config = categoryConfig[change.category];
                  const Icon = config.icon;
                  return (
                    <div key={i} className="flex items-start gap-3">
                      <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium ${config.color}`}>
                        <Icon className="h-3 w-3" />
                        {config.label}
                      </span>
                      <span className="text-gray-600 text-sm flex-1">
                        {change.description}
                        {change.module && (
                          <span className="text-gray-400 ml-1">({change.module})</span>
                        )}
                      </span>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Upgrade Notes */}
            {release.upgradeNotes && release.upgradeNotes.length > 0 && (
              <div className="px-6 py-4 bg-yellow-50 border-t border-yellow-100">
                <h3 className="text-sm font-medium text-yellow-800 mb-2">Upgrade Notes</h3>
                <ul className="space-y-1">
                  {release.upgradeNotes.map((note, i) => (
                    <li key={i} className="flex items-start gap-2 text-yellow-700 text-sm">
                      <span className="text-yellow-500">•</span>
                      {note}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </article>
        ))}
      </div>

      {/* Archive Note */}
      <div className="text-center text-gray-500 py-6 border-t border-gray-200">
        <p>For earlier versions, please contact support or check the archived documentation.</p>
      </div>
    </div>
  );
}
