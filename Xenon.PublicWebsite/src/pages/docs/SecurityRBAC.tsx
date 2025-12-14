import { Link } from 'react-router-dom';
import { Shield, Users, Key, Lock, AlertTriangle, CheckCircle } from 'lucide-react';

const roles = [
  { name: 'System Admin', type: 'SYSTEM_ADMIN', permissions: 52, description: 'Full platform access' },
  { name: 'Tenant Admin', type: 'CLINIC_ADMIN', permissions: 35, description: 'Tenant-level administration' },
  { name: 'Doctor', type: 'PHYSICIAN', permissions: 25, description: 'Clinical care access' },
  { name: 'Nurse', type: 'NURSE', permissions: 18, description: 'Clinical support' },
  { name: 'Receptionist', type: 'RECEPTIONIST', permissions: 12, description: 'Front desk operations' },
  { name: 'Lab Technician', type: 'LAB_TECHNICIAN', permissions: 8, description: 'Laboratory operations' },
  { name: 'Pharmacist', type: 'PHARMACIST', permissions: 12, description: 'Pharmacy operations' },
  { name: 'Radiologist', type: 'RADIOLOGIST', permissions: 8, description: 'Imaging interpretation' },
  { name: 'HR Manager', type: 'HR_MANAGER', permissions: 10, description: 'HR administration' },
  { name: 'Accountant', type: 'ACCOUNTANT', permissions: 8, description: 'Financial operations' },
];

const permissionCategories = [
  {
    category: 'Patient Management',
    permissions: ['patients:view', 'patients:create', 'patients:update', 'patients:delete', 'patients:export'],
    isPHI: true,
  },
  {
    category: 'Appointments',
    permissions: ['appointments:view', 'appointments:create', 'appointments:update', 'appointments:delete', 'appointments:reschedule'],
    isPHI: false,
  },
  {
    category: 'Laboratory',
    permissions: ['lab:view', 'lab:create', 'lab:update', 'lab:approve', 'lab:delete'],
    isPHI: true,
  },
  {
    category: 'Financial',
    permissions: ['financial:view', 'financial:create', 'financial:update', 'financial:approve', 'financial:reports'],
    isPHI: false,
  },
  {
    category: 'Administration',
    permissions: ['admin:users', 'admin:roles', 'admin:settings', 'admin:audit_logs', 'admin:system'],
    isPHI: false,
  },
];

export default function SecurityRBAC() {
  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Security & RBAC</h1>
        <p className="text-lg text-gray-600">
          XenonClinic implements comprehensive Role-Based Access Control (RBAC) to ensure
          data security, regulatory compliance, and proper separation of duties.
        </p>
      </div>

      {/* Security Overview */}
      <div className="grid sm:grid-cols-3 gap-4">
        <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-5 border border-blue-200">
          <Shield className="h-8 w-8 text-blue-600 mb-3" />
          <h3 className="font-semibold text-gray-900">HIPAA Ready</h3>
          <p className="text-sm text-gray-600 mt-1">
            PHI encryption, access logging, and consent management
          </p>
        </div>
        <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-5 border border-green-200">
          <Lock className="h-8 w-8 text-green-600 mb-3" />
          <h3 className="font-semibold text-gray-900">52+ Permissions</h3>
          <p className="text-sm text-gray-600 mt-1">
            Granular control over every feature and action
          </p>
        </div>
        <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl p-5 border border-purple-200">
          <Key className="h-8 w-8 text-purple-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Multi-Factor Auth</h3>
          <p className="text-sm text-gray-600 mt-1">
            Optional MFA for enhanced account security
          </p>
        </div>
      </div>

      {/* Role Hierarchy */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Role Overview</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Role</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Permissions</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {roles.map((role) => (
                <tr key={role.type}>
                  <td className="px-4 py-3 font-medium text-gray-900">{role.name}</td>
                  <td className="px-4 py-3">
                    <code className="bg-gray-100 px-2 py-0.5 rounded text-sm">{role.type}</code>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{role.permissions}</td>
                  <td className="px-4 py-3 text-gray-600">{role.description}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {/* Permission Categories */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Permission Categories</h2>
        <div className="space-y-4">
          {permissionCategories.map((cat) => (
            <div key={cat.category} className="bg-white border border-gray-200 rounded-xl p-5">
              <div className="flex items-center justify-between mb-3">
                <h3 className="font-semibold text-gray-900">{cat.category}</h3>
                {cat.isPHI && (
                  <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">
                    <Shield className="h-3 w-3" />
                    PHI Related
                  </span>
                )}
              </div>
              <div className="flex flex-wrap gap-2">
                {cat.permissions.map((perm) => (
                  <code key={perm} className="bg-gray-100 px-2 py-1 rounded text-sm">
                    {perm}
                  </code>
                ))}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Password Policy */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Password Policy</h2>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <ul className="space-y-2">
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Minimum 12 characters required</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Must include uppercase, lowercase, numbers, and special characters</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Password history: Last 12 passwords cannot be reused</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Account lockout after 5 failed attempts (15 min lockout)</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Session timeout after 30 minutes of inactivity</span>
            </li>
          </ul>
        </div>
      </section>

      {/* Audit Logging */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Audit Logging</h2>
        <div className="bg-gray-900 text-gray-100 rounded-xl p-5 font-mono text-sm overflow-x-auto">
          <pre>
{`# Sample Audit Log Entry
{
  "timestamp": "2024-12-14T10:30:45.123Z",
  "userId": "user_12345",
  "action": "PATIENT_VIEW",
  "resource": "patient/67890",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "branchId": "branch_001",
  "success": true,
  "details": {
    "patientId": "67890",
    "accessReason": "Scheduled appointment"
  }
}`}
          </pre>
        </div>
        <p className="text-sm text-gray-500 mt-2">
          Audit logs are retained for 7 years (2555 days) for compliance purposes.
        </p>
      </section>

      {/* Emergency Access */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Emergency Access</h2>
        <div className="bg-red-50 border border-red-200 rounded-xl p-5">
          <div className="flex items-start gap-3">
            <AlertTriangle className="h-6 w-6 text-red-500 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-gray-900">Break the Glass</h3>
              <p className="text-gray-600 mt-1">
                In emergency situations, authorized users (Physicians, Nurses) can access
                patient records beyond their normal scope. This requires a documented
                justification that is logged and reviewed. All emergency access is audited
                and may trigger alerts to compliance officers.
              </p>
              <ul className="mt-3 space-y-1 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-red-500" />
                  Requires minimum 10-character justification
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-red-500" />
                  Logged with full audit trail
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-red-500" />
                  Subject to compliance review
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* Learn More */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Learn More</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <Link
            to="/docs/personas"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Users className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              User Personas
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              See permissions for each role
            </p>
          </Link>
          <Link
            to="/docs/admin-configuration"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Key className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Admin Configuration
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Configure security settings
            </p>
          </Link>
        </div>
      </section>
    </div>
  );
}
