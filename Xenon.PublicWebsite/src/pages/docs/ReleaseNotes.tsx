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
