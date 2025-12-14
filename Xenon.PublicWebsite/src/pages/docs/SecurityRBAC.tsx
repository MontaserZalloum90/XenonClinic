import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Shield, Users, Key, Lock, AlertTriangle, CheckCircle, Eye, FileText,
  Database, Globe, Clock, UserCheck, Activity, ChevronDown, ChevronRight,
  Fingerprint, Server, AlertCircle, FileSearch, ClipboardCheck,
} from 'lucide-react';

const roles = [
  { name: 'System Admin', type: 'SYSTEM_ADMIN', level: 'System', permissions: 52, description: 'Full platform access, system configuration, all tenants', dataAccess: 'All data across all tenants', canAccessPHI: true },
  { name: 'Tenant Admin', type: 'TENANT_ADMIN', level: 'Tenant', permissions: 35, description: 'Tenant-level administration, user management', dataAccess: 'All data within assigned tenant', canAccessPHI: true },
  { name: 'Doctor', type: 'PHYSICIAN', level: 'Clinical', permissions: 25, description: 'Patient care, diagnosis, prescriptions, orders', dataAccess: 'Assigned patients, clinical records', canAccessPHI: true },
  { name: 'Nurse', type: 'NURSE', level: 'Clinical', permissions: 18, description: 'Patient intake, vitals, clinical support', dataAccess: 'Assigned patients, limited clinical', canAccessPHI: true },
  { name: 'Receptionist', type: 'RECEPTIONIST', level: 'Front Office', permissions: 12, description: 'Scheduling, check-in, basic patient info', dataAccess: 'Demographics, appointments only', canAccessPHI: false },
  { name: 'Lab Technician', type: 'LAB_TECHNICIAN', level: 'Clinical', permissions: 8, description: 'Lab orders, specimens, results entry', dataAccess: 'Lab orders and results only', canAccessPHI: true },
  { name: 'Pharmacist', type: 'PHARMACIST', level: 'Clinical', permissions: 12, description: 'Prescription verification, dispensing', dataAccess: 'Prescriptions, medication history', canAccessPHI: true },
  { name: 'Radiologist', type: 'RADIOLOGIST', level: 'Clinical', permissions: 8, description: 'Image interpretation, reporting', dataAccess: 'Imaging orders and studies', canAccessPHI: true },
  { name: 'HR Manager', type: 'HR_MANAGER', level: 'Business', permissions: 10, description: 'Employee management, payroll', dataAccess: 'Employee records, no patient data', canAccessPHI: false },
  { name: 'Accountant', type: 'ACCOUNTANT', level: 'Business', permissions: 8, description: 'Billing, claims, financial reporting', dataAccess: 'Financial records, limited patient', canAccessPHI: false },
  { name: 'Audiologist', type: 'AUDIOLOGIST', level: 'Clinical', permissions: 8, description: 'Hearing assessments, device fitting', dataAccess: 'Audiology records only', canAccessPHI: true },
  { name: 'Patient', type: 'PATIENT', level: 'Portal', permissions: 5, description: 'Self-service portal access', dataAccess: 'Own records only', canAccessPHI: false },
];

const permissionMatrix = [
  { module: 'Patients', sysAdmin: 'CRUD+E', tenantAdmin: 'CRUD+E', doctor: 'CRU', nurse: 'CRU', receptionist: 'CRU', labTech: 'R', pharmacist: 'R', accountant: 'R' },
  { module: 'Appointments', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'CRU', nurse: 'CRU', receptionist: 'CRUD', labTech: '-', pharmacist: '-', accountant: 'R' },
  { module: 'Clinical Visits', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'CRUD', nurse: 'CRU', receptionist: '-', labTech: 'R', pharmacist: 'R', accountant: '-' },
  { module: 'Prescriptions', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'CRUD', nurse: 'R', receptionist: '-', labTech: '-', pharmacist: 'RU', accountant: '-' },
  { module: 'Lab Orders', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'CRUD', nurse: 'CR', receptionist: '-', labTech: 'CRUD', pharmacist: '-', accountant: '-' },
  { module: 'Radiology', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'CRU', nurse: 'R', receptionist: '-', labTech: '-', pharmacist: '-', accountant: '-' },
  { module: 'Pharmacy', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: 'R', nurse: '-', receptionist: '-', labTech: '-', pharmacist: 'CRUD', accountant: 'R' },
  { module: 'Financial', sysAdmin: 'CRUD+A', tenantAdmin: 'CRUD+A', doctor: 'R', nurse: '-', receptionist: 'CR', labTech: '-', pharmacist: 'R', accountant: 'CRUD+A' },
  { module: 'Inventory', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: '-', nurse: 'R', receptionist: '-', labTech: 'RU', pharmacist: 'RU', accountant: 'R' },
  { module: 'HR', sysAdmin: 'CRUD', tenantAdmin: 'CRUD', doctor: '-', nurse: '-', receptionist: '-', labTech: '-', pharmacist: '-', accountant: '-' },
  { module: 'Reports', sysAdmin: 'Full', tenantAdmin: 'Full', doctor: 'Clinical', nurse: 'Limited', receptionist: 'Basic', labTech: 'Lab', pharmacist: 'Pharmacy', accountant: 'Financial' },
  { module: 'Settings', sysAdmin: 'Full', tenantAdmin: 'Tenant', doctor: 'Personal', nurse: 'Personal', receptionist: 'Personal', labTech: 'Personal', pharmacist: 'Personal', accountant: 'Personal' },
];

const allPermissions = [
  // Patient Management
  { permission: 'patients:view', description: 'View patient demographics and basic info', category: 'Patient Management', isPHI: true },
  { permission: 'patients:create', description: 'Register new patients', category: 'Patient Management', isPHI: true },
  { permission: 'patients:update', description: 'Update patient information', category: 'Patient Management', isPHI: true },
  { permission: 'patients:delete', description: 'Delete/deactivate patients', category: 'Patient Management', isPHI: true },
  { permission: 'patients:export', description: 'Export patient data', category: 'Patient Management', isPHI: true },
  { permission: 'patients:merge', description: 'Merge duplicate patients', category: 'Patient Management', isPHI: true },
  { permission: 'patients:medical_history', description: 'Access full medical history', category: 'Patient Management', isPHI: true },
  // Appointments
  { permission: 'appointments:view', description: 'View appointments', category: 'Appointments', isPHI: false },
  { permission: 'appointments:create', description: 'Book new appointments', category: 'Appointments', isPHI: false },
  { permission: 'appointments:update', description: 'Modify appointments', category: 'Appointments', isPHI: false },
  { permission: 'appointments:delete', description: 'Cancel appointments', category: 'Appointments', isPHI: false },
  { permission: 'appointments:reschedule', description: 'Reschedule appointments', category: 'Appointments', isPHI: false },
  { permission: 'appointments:checkin', description: 'Check-in patients', category: 'Appointments', isPHI: false },
  // Clinical
  { permission: 'clinical_visits:view', description: 'View clinical encounters', category: 'Clinical', isPHI: true },
  { permission: 'clinical_visits:create', description: 'Create clinical encounters', category: 'Clinical', isPHI: true },
  { permission: 'clinical_visits:update', description: 'Update clinical documentation', category: 'Clinical', isPHI: true },
  { permission: 'clinical_visits:sign', description: 'Sign and finalize visits', category: 'Clinical', isPHI: true },
  { permission: 'clinical_visits:amend', description: 'Amend signed records', category: 'Clinical', isPHI: true },
  { permission: 'prescriptions:create', description: 'Create prescriptions', category: 'Clinical', isPHI: true },
  { permission: 'prescriptions:view', description: 'View prescriptions', category: 'Clinical', isPHI: true },
  { permission: 'prescriptions:controlled', description: 'Prescribe controlled substances', category: 'Clinical', isPHI: true },
  // Laboratory
  { permission: 'lab:view', description: 'View lab orders and results', category: 'Laboratory', isPHI: true },
  { permission: 'lab:create', description: 'Create lab orders', category: 'Laboratory', isPHI: true },
  { permission: 'lab:update', description: 'Update lab orders/results', category: 'Laboratory', isPHI: true },
  { permission: 'lab:approve', description: 'Approve and release results', category: 'Laboratory', isPHI: true },
  { permission: 'lab:qc', description: 'Access quality control', category: 'Laboratory', isPHI: false },
  // Radiology
  { permission: 'radiology:view', description: 'View imaging orders', category: 'Radiology', isPHI: true },
  { permission: 'radiology:create', description: 'Create imaging orders', category: 'Radiology', isPHI: true },
  { permission: 'radiology:report', description: 'Create radiology reports', category: 'Radiology', isPHI: true },
  { permission: 'radiology:approve', description: 'Sign radiology reports', category: 'Radiology', isPHI: true },
  // Pharmacy
  { permission: 'pharmacy:view', description: 'View pharmacy queue', category: 'Pharmacy', isPHI: true },
  { permission: 'pharmacy:dispense', description: 'Dispense medications', category: 'Pharmacy', isPHI: true },
  { permission: 'pharmacy:manage', description: 'Manage pharmacy inventory', category: 'Pharmacy', isPHI: false },
  { permission: 'pharmacy:controlled', description: 'Handle controlled substances', category: 'Pharmacy', isPHI: true },
  // Financial
  { permission: 'financial:view', description: 'View invoices and payments', category: 'Financial', isPHI: false },
  { permission: 'financial:create', description: 'Create invoices', category: 'Financial', isPHI: false },
  { permission: 'financial:update', description: 'Update financial records', category: 'Financial', isPHI: false },
  { permission: 'financial:approve', description: 'Approve refunds/adjustments', category: 'Financial', isPHI: false },
  { permission: 'financial:reports', description: 'Access financial reports', category: 'Financial', isPHI: false },
  { permission: 'financial:collect', description: 'Collect payments', category: 'Financial', isPHI: false },
  // Administration
  { permission: 'admin:users', description: 'Manage user accounts', category: 'Administration', isPHI: false },
  { permission: 'admin:roles', description: 'Manage roles and permissions', category: 'Administration', isPHI: false },
  { permission: 'admin:settings', description: 'Modify system settings', category: 'Administration', isPHI: false },
  { permission: 'admin:audit', description: 'View audit logs', category: 'Administration', isPHI: true },
  { permission: 'admin:integrations', description: 'Configure integrations', category: 'Administration', isPHI: false },
  { permission: 'admin:backup', description: 'Manage backups', category: 'Administration', isPHI: true },
  { permission: 'admin:security', description: 'Security configuration', category: 'Administration', isPHI: false },
  // HR
  { permission: 'hr:view', description: 'View employee records', category: 'HR', isPHI: false },
  { permission: 'hr:create', description: 'Create employee records', category: 'HR', isPHI: false },
  { permission: 'hr:update', description: 'Update employee info', category: 'HR', isPHI: false },
  { permission: 'hr:payroll', description: 'Access payroll', category: 'HR', isPHI: false },
  { permission: 'hr:approve', description: 'Approve leave/requests', category: 'HR', isPHI: false },
];

const dataClassifications = [
  { level: 'Public', color: 'bg-green-100 text-green-800', description: 'Non-sensitive information', examples: 'Clinic hours, general policies, public announcements', handling: 'Can be freely shared' },
  { level: 'Internal', color: 'bg-blue-100 text-blue-800', description: 'Internal business information', examples: 'Staff schedules, internal memos, operational procedures', handling: 'Share within organization only' },
  { level: 'Confidential', color: 'bg-yellow-100 text-yellow-800', description: 'Sensitive business data', examples: 'Financial reports, vendor contracts, employee records', handling: 'Need-to-know basis, encrypted storage' },
  { level: 'PHI/Restricted', color: 'bg-red-100 text-red-800', description: 'Protected Health Information', examples: 'Patient records, diagnoses, lab results, prescriptions', handling: 'HIPAA compliant, encrypted, audit logged' },
];

const complianceFrameworks = [
  {
    name: 'HIPAA',
    fullName: 'Health Insurance Portability and Accountability Act',
    region: 'USA',
    requirements: [
      'PHI encryption at rest and in transit (AES-256)',
      'Access controls and unique user identification',
      'Audit trails for all PHI access',
      'Business Associate Agreements (BAAs)',
      'Breach notification procedures',
      'Minimum necessary access principle',
      'Patient rights to access records',
    ],
    status: 'Compliant',
  },
  {
    name: 'DOH UAE',
    fullName: 'Department of Health - UAE Healthcare Standards',
    region: 'UAE',
    requirements: [
      'Emirates ID validation for patient registration',
      'Arabic language support',
      'Local data residency (UAE servers)',
      '10-year medical record retention',
      'MOH reporting requirements',
      'Controlled substance tracking per UAE law',
      'Health facility licensing compliance',
    ],
    status: 'Compliant',
  },
  {
    name: 'ISO 27001',
    fullName: 'Information Security Management System',
    region: 'International',
    requirements: [
      'Information security policy documentation',
      'Risk assessment and treatment',
      'Asset management and classification',
      'Access control policies',
      'Cryptographic controls',
      'Incident management procedures',
      'Business continuity planning',
    ],
    status: 'Certified',
  },
  {
    name: 'GDPR',
    fullName: 'General Data Protection Regulation',
    region: 'EU',
    requirements: [
      'Lawful basis for data processing',
      'Data subject rights (access, erasure, portability)',
      'Data Protection Impact Assessments',
      'Data breach notification (72 hours)',
      'Consent management',
      'Right to be forgotten',
      'Data Processing Agreements',
    ],
    status: 'Compliant',
  },
];

export default function SecurityRBAC() {
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(['overview']));
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const toggleSection = (section: string) => {
    const newExpanded = new Set(expandedSections);
    if (newExpanded.has(section)) {
      newExpanded.delete(section);
    } else {
      newExpanded.add(section);
    }
    setExpandedSections(newExpanded);
  };

  const filteredPermissions = selectedCategory === 'all'
    ? allPermissions
    : allPermissions.filter(p => p.category === selectedCategory);

  const categories = ['all', ...new Set(allPermissions.map(p => p.category))];

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Security & RBAC</h1>
        <p className="text-lg text-gray-600">
          XenonClinic implements comprehensive Role-Based Access Control (RBAC) with
          enterprise-grade security features for healthcare compliance, PHI protection,
          and regulatory requirements across multiple jurisdictions.
        </p>
      </div>

      {/* Security Overview Stats */}
      <div className="grid sm:grid-cols-4 gap-4">
        <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-5 border border-blue-200">
          <Shield className="h-8 w-8 text-blue-600 mb-3" />
          <h3 className="text-2xl font-bold text-gray-900">4</h3>
          <p className="text-sm text-gray-600">Compliance Frameworks</p>
        </div>
        <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-5 border border-green-200">
          <Lock className="h-8 w-8 text-green-600 mb-3" />
          <h3 className="text-2xl font-bold text-gray-900">52+</h3>
          <p className="text-sm text-gray-600">Granular Permissions</p>
        </div>
        <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl p-5 border border-purple-200">
          <Users className="h-8 w-8 text-purple-600 mb-3" />
          <h3 className="text-2xl font-bold text-gray-900">12</h3>
          <p className="text-sm text-gray-600">Pre-defined Roles</p>
        </div>
        <div className="bg-gradient-to-br from-orange-50 to-orange-100 rounded-xl p-5 border border-orange-200">
          <Activity className="h-8 w-8 text-orange-600 mb-3" />
          <h3 className="text-2xl font-bold text-gray-900">7 Years</h3>
          <p className="text-sm text-gray-600">Audit Log Retention</p>
        </div>
      </div>

      {/* Quick Navigation */}
      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <h3 className="font-semibold text-gray-900 mb-3">Quick Navigation</h3>
        <div className="flex flex-wrap gap-2">
          {['roles', 'permissions', 'data-classification', 'compliance', 'authentication', 'audit', 'emergency', 'encryption'].map(section => (
            <a
              key={section}
              href={`#${section}`}
              onClick={() => {
                const newExpanded = new Set(expandedSections);
                newExpanded.add(section);
                setExpandedSections(newExpanded);
              }}
              className="px-3 py-1.5 bg-gray-100 hover:bg-primary-100 hover:text-primary-700 rounded-full text-sm font-medium transition-colors capitalize"
            >
              {section.replace('-', ' ')}
            </a>
          ))}
        </div>
      </div>

      {/* Role Hierarchy */}
      <section id="roles">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('roles')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Role Hierarchy & Data Access</h2>
          {expandedSections.has('roles') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('roles') && (
          <div className="mt-4 space-y-4">
            <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Role</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Level</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Permissions</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Data Access Scope</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">PHI Access</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {roles.map((role) => (
                      <tr key={role.type} className="hover:bg-gray-50">
                        <td className="px-4 py-3">
                          <div className="font-medium text-gray-900">{role.name}</div>
                          <code className="text-xs text-gray-500">{role.type}</code>
                        </td>
                        <td className="px-4 py-3">
                          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                            role.level === 'System' ? 'bg-red-100 text-red-700' :
                            role.level === 'Tenant' ? 'bg-orange-100 text-orange-700' :
                            role.level === 'Clinical' ? 'bg-blue-100 text-blue-700' :
                            role.level === 'Business' ? 'bg-green-100 text-green-700' :
                            'bg-gray-100 text-gray-700'
                          }`}>
                            {role.level}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-600">{role.permissions}</td>
                        <td className="px-4 py-3 text-sm text-gray-600">{role.dataAccess}</td>
                        <td className="px-4 py-3">
                          {role.canAccessPHI ? (
                            <span className="inline-flex items-center gap-1 text-red-600">
                              <Shield className="h-4 w-4" /> Yes
                            </span>
                          ) : (
                            <span className="text-gray-400">No</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Permission Matrix */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4">Permission Matrix by Module</h3>
              <p className="text-sm text-gray-600 mb-4">
                <strong>Legend:</strong> C=Create, R=Read, U=Update, D=Delete, E=Export, A=Approve, -=No Access
              </p>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200 text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">Module</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Sys Admin</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Tenant Admin</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Doctor</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Nurse</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Reception</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Lab Tech</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Pharmacist</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-gray-500 uppercase">Accountant</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {permissionMatrix.map((row) => (
                      <tr key={row.module}>
                        <td className="px-3 py-2 font-medium text-gray-900">{row.module}</td>
                        <td className="px-3 py-2 text-center"><code className="bg-green-50 px-1 rounded">{row.sysAdmin}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-green-50 px-1 rounded">{row.tenantAdmin}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-blue-50 px-1 rounded">{row.doctor}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-blue-50 px-1 rounded">{row.nurse}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-gray-50 px-1 rounded">{row.receptionist}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-purple-50 px-1 rounded">{row.labTech}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-purple-50 px-1 rounded">{row.pharmacist}</code></td>
                        <td className="px-3 py-2 text-center"><code className="bg-yellow-50 px-1 rounded">{row.accountant}</code></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </section>

      {/* All Permissions */}
      <section id="permissions">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('permissions')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Complete Permission Reference</h2>
          {expandedSections.has('permissions') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('permissions') && (
          <div className="mt-4 space-y-4">
            <div className="flex flex-wrap gap-2 mb-4">
              {categories.map(cat => (
                <button
                  key={cat}
                  onClick={() => setSelectedCategory(cat)}
                  className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                    selectedCategory === cat
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  {cat === 'all' ? 'All Categories' : cat}
                </button>
              ))}
            </div>
            <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Permission</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">PHI</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {filteredPermissions.map((perm) => (
                    <tr key={perm.permission} className="hover:bg-gray-50">
                      <td className="px-4 py-3">
                        <code className="bg-gray-100 px-2 py-0.5 rounded text-sm">{perm.permission}</code>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">{perm.description}</td>
                      <td className="px-4 py-3 text-sm text-gray-500">{perm.category}</td>
                      <td className="px-4 py-3">
                        {perm.isPHI && (
                          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">
                            <Shield className="h-3 w-3" /> PHI
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </section>

      {/* Data Classification */}
      <section id="data-classification">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('data-classification')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Data Classification Levels</h2>
          {expandedSections.has('data-classification') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('data-classification') && (
          <div className="mt-4 grid sm:grid-cols-2 gap-4">
            {dataClassifications.map((level) => (
              <div key={level.level} className="bg-white border border-gray-200 rounded-xl p-5">
                <div className="flex items-center gap-3 mb-3">
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${level.color}`}>
                    {level.level}
                  </span>
                </div>
                <p className="font-medium text-gray-900">{level.description}</p>
                <div className="mt-3 space-y-2 text-sm">
                  <div>
                    <span className="text-gray-500">Examples:</span>
                    <span className="text-gray-700 ml-2">{level.examples}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Handling:</span>
                    <span className="text-gray-700 ml-2">{level.handling}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Compliance Frameworks */}
      <section id="compliance">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('compliance')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Compliance Frameworks</h2>
          {expandedSections.has('compliance') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('compliance') && (
          <div className="mt-4 space-y-4">
            {complianceFrameworks.map((framework) => (
              <div key={framework.name} className="bg-white border border-gray-200 rounded-xl p-5">
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{framework.name}</h3>
                    <p className="text-sm text-gray-500">{framework.fullName}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-gray-500">{framework.region}</span>
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                      framework.status === 'Certified' ? 'bg-green-100 text-green-700' : 'bg-blue-100 text-blue-700'
                    }`}>
                      {framework.status}
                    </span>
                  </div>
                </div>
                <div className="grid sm:grid-cols-2 gap-2">
                  {framework.requirements.map((req, idx) => (
                    <div key={idx} className="flex items-start gap-2 text-sm">
                      <CheckCircle className="h-4 w-4 text-green-500 flex-shrink-0 mt-0.5" />
                      <span className="text-gray-600">{req}</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Authentication & Session Security */}
      <section id="authentication">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('authentication')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Authentication & Session Security</h2>
          {expandedSections.has('authentication') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('authentication') && (
          <div className="mt-4 grid sm:grid-cols-2 gap-6">
            {/* Password Policy */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Key className="h-5 w-5 text-primary-600" />
                Password Policy
              </h3>
              <ul className="space-y-2">
                {[
                  'Minimum 12 characters required',
                  'Must include uppercase, lowercase, numbers',
                  'At least one special character (!@#$%^&*)',
                  'Cannot contain username or email',
                  'Password history: Last 12 cannot be reused',
                  'Maximum age: 90 days (configurable)',
                  'Complexity check against common passwords',
                ].map((item, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-sm">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    <span className="text-gray-600">{item}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* MFA Options */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Fingerprint className="h-5 w-5 text-primary-600" />
                Multi-Factor Authentication
              </h3>
              <ul className="space-y-2">
                {[
                  'TOTP Authenticator Apps (Google, Microsoft, Authy)',
                  'SMS One-Time Passwords',
                  'Email Verification Codes',
                  'Hardware Security Keys (FIDO2/WebAuthn)',
                  'Backup Recovery Codes (10 single-use)',
                  'Required for PHI access roles',
                  'Enforced for admin accounts',
                ].map((item, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-sm">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    <span className="text-gray-600">{item}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Session Security */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Clock className="h-5 w-5 text-primary-600" />
                Session Security
              </h3>
              <ul className="space-y-2">
                {[
                  'Idle timeout: 30 minutes (configurable)',
                  'Absolute timeout: 12 hours maximum',
                  'Secure session cookies (HttpOnly, Secure)',
                  'Session binding to IP address (optional)',
                  'Concurrent session limit per user',
                  'Force logout on password change',
                  'Activity-based session extension',
                ].map((item, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-sm">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    <span className="text-gray-600">{item}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Account Protection */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Shield className="h-5 w-5 text-primary-600" />
                Account Protection
              </h3>
              <ul className="space-y-2">
                {[
                  'Account lockout after 5 failed attempts',
                  'Lockout duration: 15 minutes (progressive)',
                  'CAPTCHA after 3 failed attempts',
                  'Suspicious login alerts via email',
                  'Login from new device notifications',
                  'Geographic anomaly detection',
                  'Brute force protection with rate limiting',
                ].map((item, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-sm">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    <span className="text-gray-600">{item}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}
      </section>

      {/* Audit Logging */}
      <section id="audit">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('audit')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Comprehensive Audit Logging</h2>
          {expandedSections.has('audit') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('audit') && (
          <div className="mt-4 space-y-4">
            <div className="grid sm:grid-cols-3 gap-4">
              <div className="bg-white border border-gray-200 rounded-xl p-5">
                <Eye className="h-6 w-6 text-primary-600 mb-3" />
                <h3 className="font-semibold text-gray-900">What's Logged</h3>
                <ul className="mt-2 space-y-1 text-sm text-gray-600">
                  <li>• All PHI access events</li>
                  <li>• Authentication attempts</li>
                  <li>• Permission changes</li>
                  <li>• Data exports</li>
                  <li>• Configuration changes</li>
                  <li>• Clinical documentation</li>
                </ul>
              </div>
              <div className="bg-white border border-gray-200 rounded-xl p-5">
                <Database className="h-6 w-6 text-primary-600 mb-3" />
                <h3 className="font-semibold text-gray-900">Log Details</h3>
                <ul className="mt-2 space-y-1 text-sm text-gray-600">
                  <li>• Timestamp (UTC)</li>
                  <li>• User ID & session</li>
                  <li>• Action performed</li>
                  <li>• Resource affected</li>
                  <li>• IP address & device</li>
                  <li>• Before/after values</li>
                </ul>
              </div>
              <div className="bg-white border border-gray-200 rounded-xl p-5">
                <FileSearch className="h-6 w-6 text-primary-600 mb-3" />
                <h3 className="font-semibold text-gray-900">Retention & Access</h3>
                <ul className="mt-2 space-y-1 text-sm text-gray-600">
                  <li>• 7-year retention (2555 days)</li>
                  <li>• Immutable storage</li>
                  <li>• Encrypted at rest</li>
                  <li>• Admin-only access</li>
                  <li>• Export capabilities</li>
                  <li>• Search & filtering</li>
                </ul>
              </div>
            </div>

            {/* Sample Audit Log */}
            <div className="bg-gray-900 text-gray-100 rounded-xl p-5 font-mono text-sm overflow-x-auto">
              <div className="text-gray-400 mb-2"># Sample Audit Log Entry</div>
              <pre className="text-green-400">
{`{
  "id": "audit_7f8e9d0c1b2a",
  "timestamp": "2024-12-14T10:30:45.123Z",
  "eventType": "PHI_ACCESS",
  "userId": "user_dr_smith_12345",
  "userName": "Dr. Sarah Smith",
  "role": "PHYSICIAN",
  "action": "PATIENT_RECORD_VIEW",
  "resource": {
    "type": "Patient",
    "id": "patient_67890",
    "mrn": "MRN-2024-00123"
  },
  "context": {
    "sessionId": "sess_abc123def456",
    "ipAddress": "192.168.1.100",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64)",
    "branchId": "branch_dubai_main",
    "branchName": "Dubai Main Clinic"
  },
  "accessReason": "Scheduled appointment - APT-2024-12345",
  "accessJustification": null,
  "breakTheGlass": false,
  "success": true,
  "sensitivityLevel": "PHI",
  "dataFieldsAccessed": [
    "demographics", "vitals", "diagnoses", "medications"
  ]
}`}
              </pre>
            </div>

            {/* Audit Events Table */}
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-900 mb-4">Common Audit Events</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200 text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Event Type</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Alert Level</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {[
                      { event: 'LOGIN_SUCCESS', desc: 'Successful user authentication', level: 'Info' },
                      { event: 'LOGIN_FAILED', desc: 'Failed authentication attempt', level: 'Warning' },
                      { event: 'PHI_ACCESS', desc: 'Patient health information accessed', level: 'Info' },
                      { event: 'PHI_EXPORT', desc: 'Patient data exported', level: 'Warning' },
                      { event: 'BREAK_GLASS', desc: 'Emergency access to restricted records', level: 'Critical' },
                      { event: 'PERMISSION_CHANGE', desc: 'User permissions modified', level: 'Warning' },
                      { event: 'ACCOUNT_LOCKED', desc: 'Account locked due to failed attempts', level: 'Warning' },
                      { event: 'CONFIG_CHANGE', desc: 'System configuration modified', level: 'Info' },
                      { event: 'DATA_DELETE', desc: 'Record deletion event', level: 'Warning' },
                      { event: 'BULK_EXPORT', desc: 'Large data export initiated', level: 'Critical' },
                    ].map((row) => (
                      <tr key={row.event}>
                        <td className="px-4 py-2"><code className="bg-gray-100 px-2 py-0.5 rounded">{row.event}</code></td>
                        <td className="px-4 py-2 text-gray-600">{row.desc}</td>
                        <td className="px-4 py-2">
                          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                            row.level === 'Critical' ? 'bg-red-100 text-red-700' :
                            row.level === 'Warning' ? 'bg-yellow-100 text-yellow-700' :
                            'bg-blue-100 text-blue-700'
                          }`}>{row.level}</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </section>

      {/* Emergency Access */}
      <section id="emergency">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('emergency')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Emergency Access (Break the Glass)</h2>
          {expandedSections.has('emergency') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('emergency') && (
          <div className="mt-4 space-y-4">
            <div className="bg-red-50 border border-red-200 rounded-xl p-6">
              <div className="flex items-start gap-4">
                <AlertTriangle className="h-8 w-8 text-red-500 flex-shrink-0" />
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Break the Glass Procedure</h3>
                  <p className="text-gray-600 mt-2">
                    In emergency situations, authorized clinical staff can access patient records
                    beyond their normal scope. This feature ensures patient safety while maintaining
                    accountability and compliance.
                  </p>
                </div>
              </div>
            </div>

            <div className="grid sm:grid-cols-2 gap-4">
              <div className="bg-white border border-gray-200 rounded-xl p-5">
                <h4 className="font-semibold text-gray-900 mb-3">Who Can Use</h4>
                <ul className="space-y-2 text-sm text-gray-600">
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    Physicians (all specialties)
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    Nurses (RN, LPN with clinical duties)
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    Emergency Department staff
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />
                    Pharmacists (medication emergencies)
                  </li>
                </ul>
              </div>

              <div className="bg-white border border-gray-200 rounded-xl p-5">
                <h4 className="font-semibold text-gray-900 mb-3">Requirements</h4>
                <ul className="space-y-2 text-sm text-gray-600">
                  <li className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4 text-red-500" />
                    Written justification (min 20 characters)
                  </li>
                  <li className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4 text-red-500" />
                    Select emergency reason from list
                  </li>
                  <li className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4 text-red-500" />
                    Re-authenticate with password/MFA
                  </li>
                  <li className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4 text-red-500" />
                    Acknowledge audit warning
                  </li>
                </ul>
              </div>
            </div>

            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <h4 className="font-semibold text-gray-900 mb-3">Break the Glass Workflow</h4>
              <div className="flex flex-wrap items-center gap-2 text-sm">
                <span className="px-3 py-2 bg-blue-100 text-blue-700 rounded-lg">1. Access Blocked</span>
                <span className="text-gray-400">→</span>
                <span className="px-3 py-2 bg-yellow-100 text-yellow-700 rounded-lg">2. Click "Emergency Access"</span>
                <span className="text-gray-400">→</span>
                <span className="px-3 py-2 bg-orange-100 text-orange-700 rounded-lg">3. Select Reason</span>
                <span className="text-gray-400">→</span>
                <span className="px-3 py-2 bg-red-100 text-red-700 rounded-lg">4. Enter Justification</span>
                <span className="text-gray-400">→</span>
                <span className="px-3 py-2 bg-purple-100 text-purple-700 rounded-lg">5. Re-authenticate</span>
                <span className="text-gray-400">→</span>
                <span className="px-3 py-2 bg-green-100 text-green-700 rounded-lg">6. Access Granted (30 min)</span>
              </div>
            </div>

            <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-5">
              <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                <ClipboardCheck className="h-5 w-5 text-yellow-600" />
                Compliance Review Process
              </h4>
              <ul className="space-y-1 text-sm text-gray-700">
                <li>• All Break the Glass events are flagged for mandatory review</li>
                <li>• Compliance officer receives immediate notification</li>
                <li>• Review must be completed within 72 hours</li>
                <li>• Unjustified access triggers disciplinary process</li>
                <li>• Monthly Break the Glass reports generated automatically</li>
              </ul>
            </div>
          </div>
        )}
      </section>

      {/* Encryption */}
      <section id="encryption">
        <div
          className="flex items-center justify-between cursor-pointer"
          onClick={() => toggleSection('encryption')}
        >
          <h2 className="text-2xl font-semibold text-gray-900">Encryption & Data Protection</h2>
          {expandedSections.has('encryption') ? <ChevronDown className="h-5 w-5" /> : <ChevronRight className="h-5 w-5" />}
        </div>
        {expandedSections.has('encryption') && (
          <div className="mt-4 grid sm:grid-cols-2 gap-4">
            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <Server className="h-6 w-6 text-primary-600 mb-3" />
              <h3 className="font-semibold text-gray-900">Data at Rest</h3>
              <ul className="mt-3 space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  AES-256 encryption for all databases
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Encrypted file storage for documents
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Encrypted backups with separate keys
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Hardware Security Modules (HSM) for keys
                </li>
              </ul>
            </div>

            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <Globe className="h-6 w-6 text-primary-600 mb-3" />
              <h3 className="font-semibold text-gray-900">Data in Transit</h3>
              <ul className="mt-3 space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  TLS 1.3 for all connections
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Certificate pinning for mobile apps
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  HSTS headers enforced
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Perfect Forward Secrecy (PFS)
                </li>
              </ul>
            </div>

            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <Lock className="h-6 w-6 text-primary-600 mb-3" />
              <h3 className="font-semibold text-gray-900">Key Management</h3>
              <ul className="mt-3 space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Automated key rotation (90 days)
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Separate keys per tenant
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Bring Your Own Key (BYOK) option
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Key escrow for disaster recovery
                </li>
              </ul>
            </div>

            <div className="bg-white border border-gray-200 rounded-xl p-5">
              <FileText className="h-6 w-6 text-primary-600 mb-3" />
              <h3 className="font-semibold text-gray-900">PHI-Specific Protection</h3>
              <ul className="mt-3 space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Field-level encryption for SSN/ID
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Tokenization for payment data
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Data masking in non-prod environments
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  Secure deletion (crypto-shredding)
                </li>
              </ul>
            </div>
          </div>
        )}
      </section>

      {/* Learn More */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Related Documentation</h2>
        <div className="grid sm:grid-cols-3 gap-4">
          <Link
            to="/docs/personas"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Users className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">User Personas</h3>
            <p className="text-sm text-gray-500 mt-1">Role-specific guides and permissions</p>
          </Link>
          <Link
            to="/docs/admin-configuration"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Key className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">Admin Configuration</h3>
            <p className="text-sm text-gray-500 mt-1">Security and compliance settings</p>
          </Link>
          <Link
            to="/docs/api-reference"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <Server className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">API Security</h3>
            <p className="text-sm text-gray-500 mt-1">API authentication and authorization</p>
          </Link>
        </div>
      </section>
    </div>
  );
}
