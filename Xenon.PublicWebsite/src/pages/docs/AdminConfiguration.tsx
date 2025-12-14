import { Link } from 'react-router-dom';
import { Settings, Building, Users, Globe, Bell, Shield, Database, Clock, Palette, Key } from 'lucide-react';

const configCategories = [
  {
    id: 'system',
    name: 'System Settings',
    icon: Settings,
    description: 'Core platform configuration',
    settings: [
      { name: 'Application Name', type: 'text', description: 'Displayed in browser title and emails' },
      { name: 'Default Language', type: 'select', description: 'Primary language for the platform' },
      { name: 'Default Timezone', type: 'select', description: 'System-wide timezone setting' },
      { name: 'Date Format', type: 'select', description: 'MM/DD/YYYY, DD/MM/YYYY, or YYYY-MM-DD' },
      { name: 'Currency', type: 'select', description: 'Default currency for financial operations' },
    ],
  },
  {
    id: 'tenant',
    name: 'Tenant Configuration',
    icon: Building,
    description: 'Organization-level settings',
    settings: [
      { name: 'Organization Name', type: 'text', description: 'Legal name of the organization' },
      { name: 'Logo', type: 'file', description: 'Organization logo for branding' },
      { name: 'Primary Color', type: 'color', description: 'Brand color used throughout the app' },
      { name: 'Contact Email', type: 'email', description: 'Primary contact email' },
      { name: 'License Type', type: 'readonly', description: 'Current subscription tier' },
    ],
  },
  {
    id: 'branches',
    name: 'Branch Setup',
    icon: Globe,
    description: 'Location and branch management',
    settings: [
      { name: 'Branch Name', type: 'text', description: 'Display name for the branch' },
      { name: 'Address', type: 'textarea', description: 'Physical location address' },
      { name: 'Phone Number', type: 'tel', description: 'Branch contact number' },
      { name: 'Operating Hours', type: 'schedule', description: 'Weekly operating schedule' },
      { name: 'Services Offered', type: 'multiselect', description: 'Available services at this branch' },
    ],
  },
  {
    id: 'users',
    name: 'User Management',
    icon: Users,
    description: 'User accounts and access',
    settings: [
      { name: 'Auto-provision Users', type: 'toggle', description: 'Enable SSO auto-provisioning' },
      { name: 'Default Role', type: 'select', description: 'Role assigned to new users' },
      { name: 'Require Email Verification', type: 'toggle', description: 'Verify email before first login' },
      { name: 'Allow Self-Registration', type: 'toggle', description: 'Let users create their own accounts' },
      { name: 'User Photo Required', type: 'toggle', description: 'Require profile photo for users' },
    ],
  },
  {
    id: 'security',
    name: 'Security Settings',
    icon: Shield,
    description: 'Authentication and access control',
    settings: [
      { name: 'Password Minimum Length', type: 'number', description: 'Minimum characters (default: 12)' },
      { name: 'Password Expiry Days', type: 'number', description: 'Days until password reset required' },
      { name: 'Failed Login Attempts', type: 'number', description: 'Attempts before lockout (default: 5)' },
      { name: 'Lockout Duration', type: 'number', description: 'Minutes until auto-unlock (default: 15)' },
      { name: 'Session Timeout', type: 'number', description: 'Minutes of inactivity (default: 30)' },
      { name: 'Require MFA', type: 'toggle', description: 'Mandatory multi-factor authentication' },
      { name: 'IP Whitelist', type: 'textarea', description: 'Allowed IP addresses/ranges' },
    ],
  },
  {
    id: 'notifications',
    name: 'Notification Settings',
    icon: Bell,
    description: 'Email and alert configuration',
    settings: [
      { name: 'SMTP Server', type: 'text', description: 'Email server hostname' },
      { name: 'SMTP Port', type: 'number', description: 'Email server port' },
      { name: 'From Email', type: 'email', description: 'Sender email address' },
      { name: 'SMS Provider', type: 'select', description: 'SMS gateway integration' },
      { name: 'Appointment Reminders', type: 'toggle', description: 'Send automated reminders' },
      { name: 'Reminder Lead Time', type: 'select', description: 'Hours before appointment' },
    ],
  },
  {
    id: 'integrations',
    name: 'Integrations',
    icon: Database,
    description: 'External system connections',
    settings: [
      { name: 'HL7 FHIR Endpoint', type: 'url', description: 'Healthcare data interoperability' },
      { name: 'Lab Interface', type: 'select', description: 'Laboratory information system' },
      { name: 'Payment Gateway', type: 'select', description: 'Payment processor integration' },
      { name: 'Insurance Clearinghouse', type: 'select', description: 'Claims submission service' },
      { name: 'PACS Server', type: 'url', description: 'Medical imaging system' },
    ],
  },
  {
    id: 'scheduling',
    name: 'Scheduling Rules',
    icon: Clock,
    description: 'Appointment and calendar settings',
    settings: [
      { name: 'Default Slot Duration', type: 'select', description: 'Standard appointment length' },
      { name: 'Booking Window', type: 'number', description: 'Days in advance patients can book' },
      { name: 'Cancellation Window', type: 'number', description: 'Hours before appointment to cancel' },
      { name: 'Overbooking Limit', type: 'number', description: 'Maximum overlapping appointments' },
      { name: 'Buffer Between Appointments', type: 'number', description: 'Minutes between slots' },
    ],
  },
];

export default function AdminConfiguration() {
  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Admin Configuration</h1>
        <p className="text-lg text-gray-600">
          System administrators and tenant administrators can configure XenonClinic
          to match organizational requirements and policies.
        </p>
      </div>

      {/* Access Note */}
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-5">
        <div className="flex items-start gap-3">
          <Key className="h-6 w-6 text-blue-600 flex-shrink-0" />
          <div>
            <h3 className="font-semibold text-gray-900">Administrator Access Required</h3>
            <p className="text-gray-600 mt-1">
              These settings require System Admin or Tenant Admin role. Changes may affect
              all users in your organization and should be made carefully.
            </p>
          </div>
        </div>
      </div>

      {/* Configuration Categories */}
      <div className="space-y-8">
        {configCategories.map((category) => {
          const Icon = category.icon;
          return (
            <section key={category.id} id={category.id}>
              <div className="flex items-center gap-3 mb-4">
                <div className="p-2 bg-primary-100 rounded-lg">
                  <Icon className="h-6 w-6 text-primary-600" />
                </div>
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">{category.name}</h2>
                  <p className="text-sm text-gray-500">{category.description}</p>
                </div>
              </div>

              <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Setting</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {category.settings.map((setting) => (
                      <tr key={setting.name}>
                        <td className="px-4 py-3 font-medium text-gray-900">{setting.name}</td>
                        <td className="px-4 py-3">
                          <code className="bg-gray-100 px-2 py-0.5 rounded text-sm text-gray-600">
                            {setting.type}
                          </code>
                        </td>
                        <td className="px-4 py-3 text-gray-600">{setting.description}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          );
        })}
      </div>

      {/* Configuration Best Practices */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Best Practices</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Palette className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Customize Branding</h3>
            <p className="text-sm text-gray-600 mt-1">
              Upload your logo and set brand colors to make XenonClinic feel like your own.
              This improves user adoption and trust.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Shield className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Enable MFA</h3>
            <p className="text-sm text-gray-600 mt-1">
              Require multi-factor authentication for all users, especially those with
              access to PHI or financial data.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Bell className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Configure Reminders</h3>
            <p className="text-sm text-gray-600 mt-1">
              Set up appointment reminders to reduce no-shows. Consider both email
              and SMS for better reach.
            </p>
          </div>
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <Clock className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-semibold text-gray-900">Review Session Timeout</h3>
            <p className="text-sm text-gray-600 mt-1">
              Balance security and convenience. 30 minutes is recommended for
              clinical environments.
            </p>
          </div>
        </div>
      </section>

      {/* Navigation Links */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Related Documentation</h2>
        <div className="grid sm:grid-cols-2 gap-4">
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
        </div>
      </section>
    </div>
  );
}
