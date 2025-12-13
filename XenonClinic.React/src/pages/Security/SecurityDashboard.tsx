import { useQuery } from '@tanstack/react-query';
import {
  ShieldCheckIcon,
  UserGroupIcon,
  ExclamationTriangleIcon,
  KeyIcon,
  ClipboardDocumentCheckIcon,
  LockClosedIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';
import type { SecurityStatistics, SecurityIncident, AuditLog } from '../../types/security';

export function SecurityDashboard() {
  const { data: stats } = useQuery({
    queryKey: ['security-stats'],
    queryFn: () => api.get<SecurityStatistics>('/api/security/statistics'),
  });

  const { data: recentIncidents = [] } = useQuery({
    queryKey: ['security-incidents-recent'],
    queryFn: () => api.get<SecurityIncident[]>('/api/security/incidents?limit=5'),
  });

  const { data: recentAuditLogs = [] } = useQuery({
    queryKey: ['audit-logs-recent'],
    queryFn: () => api.get<AuditLog[]>('/api/security/audit-logs?limit=10'),
  });

  const severityColors: Record<number, string> = {
    0: 'bg-blue-100 text-blue-800',
    1: 'bg-yellow-100 text-yellow-800',
    2: 'bg-orange-100 text-orange-800',
    3: 'bg-red-100 text-red-800',
  };

  const severityLabels: Record<number, string> = {
    0: 'Low',
    1: 'Medium',
    2: 'High',
    3: 'Critical',
  };

  const actionLabels: Record<number, string> = {
    0: 'Create',
    1: 'Read',
    2: 'Update',
    3: 'Delete',
    4: 'Login',
    5: 'Logout',
    6: 'Export',
    7: 'Print',
    8: 'Permission Change',
    9: 'Password Change',
    10: 'Failed',
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Security Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">
          Monitor security metrics, incidents, and compliance status
        </p>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-500">Total Users</p>
              <p className="mt-1 text-3xl font-semibold text-gray-900">{stats?.totalUsers || 0}</p>
              <p className="mt-1 text-sm text-gray-500">
                {stats?.activeUsers || 0} active
              </p>
            </div>
            <div className="rounded-full bg-blue-100 p-3">
              <UserGroupIcon className="h-6 w-6 text-blue-600" />
            </div>
          </div>
        </div>

        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-500">Locked Accounts</p>
              <p className="mt-1 text-3xl font-semibold text-gray-900">{stats?.lockedAccounts || 0}</p>
              <p className="mt-1 text-sm text-red-500 flex items-center">
                <ExclamationTriangleIcon className="h-4 w-4 mr-1" />
                Requires attention
              </p>
            </div>
            <div className="rounded-full bg-red-100 p-3">
              <LockClosedIcon className="h-6 w-6 text-red-600" />
            </div>
          </div>
        </div>

        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-500">MFA Adoption</p>
              <p className="mt-1 text-3xl font-semibold text-gray-900">{stats?.mfaAdoption || 0}%</p>
              <p className={`mt-1 text-sm flex items-center ${(stats?.mfaAdoption || 0) > 80 ? 'text-green-500' : 'text-yellow-500'}`}>
                {(stats?.mfaAdoption || 0) > 80 ? (
                  <ArrowTrendingUpIcon className="h-4 w-4 mr-1" />
                ) : (
                  <ArrowTrendingDownIcon className="h-4 w-4 mr-1" />
                )}
                {(stats?.mfaAdoption || 0) > 80 ? 'Good adoption' : 'Needs improvement'}
              </p>
            </div>
            <div className="rounded-full bg-purple-100 p-3">
              <KeyIcon className="h-6 w-6 text-purple-600" />
            </div>
          </div>
        </div>

        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-500">Compliance Score</p>
              <p className="mt-1 text-3xl font-semibold text-gray-900">{stats?.complianceScore || 0}%</p>
              <p className={`mt-1 text-sm flex items-center ${(stats?.complianceScore || 0) >= 90 ? 'text-green-500' : 'text-yellow-500'}`}>
                {(stats?.complianceScore || 0) >= 90 ? 'Compliant' : 'Review needed'}
              </p>
            </div>
            <div className="rounded-full bg-green-100 p-3">
              <ClipboardDocumentCheckIcon className="h-6 w-6 text-green-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Security Status Cards */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-medium text-gray-900">Security Alerts</h3>
            <span className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
              (stats?.openIncidents || 0) > 0 ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
            }`}>
              {stats?.openIncidents || 0} Open
            </span>
          </div>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Open Incidents</span>
              <span className="font-medium">{stats?.openIncidents || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Critical Incidents</span>
              <span className="font-medium text-red-600">{stats?.criticalIncidents || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Failed Logins (24h)</span>
              <span className="font-medium">{stats?.failedLogins24h || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Pending Access Reviews</span>
              <span className="font-medium">{stats?.pendingAccessReviews || 0}</span>
            </div>
          </div>
        </div>

        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-medium text-gray-900">Account Health</h3>
            <ShieldCheckIcon className="h-5 w-5 text-gray-400" />
          </div>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Active Users</span>
              <span className="font-medium text-green-600">{stats?.activeUsers || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Locked Accounts</span>
              <span className="font-medium text-red-600">{stats?.lockedAccounts || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-500">Passwords Expiring Soon</span>
              <span className="font-medium text-yellow-600">{stats?.passwordExpiringUsers || 0}</span>
            </div>
          </div>
          <div className="mt-4 pt-4 border-t">
            <div className="flex justify-between items-center mb-2">
              <span className="text-sm text-gray-500">MFA Adoption Rate</span>
              <span className="font-medium">{stats?.mfaAdoption || 0}%</span>
            </div>
            <div className="h-2 bg-gray-200 rounded-full">
              <div
                className="h-2 bg-purple-500 rounded-full"
                style={{ width: `${stats?.mfaAdoption || 0}%` }}
              />
            </div>
          </div>
        </div>

        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-medium text-gray-900">Compliance Status</h3>
            <ClipboardDocumentCheckIcon className="h-5 w-5 text-gray-400" />
          </div>
          <div className="text-center py-4">
            <div className="relative inline-flex items-center justify-center">
              <svg className="w-32 h-32 transform -rotate-90">
                <circle
                  cx="64"
                  cy="64"
                  r="56"
                  fill="none"
                  stroke="#e5e7eb"
                  strokeWidth="12"
                />
                <circle
                  cx="64"
                  cy="64"
                  r="56"
                  fill="none"
                  stroke={(stats?.complianceScore || 0) >= 90 ? '#10b981' : (stats?.complianceScore || 0) >= 70 ? '#f59e0b' : '#ef4444'}
                  strokeWidth="12"
                  strokeDasharray={`${((stats?.complianceScore || 0) / 100) * 352} 352`}
                  strokeLinecap="round"
                />
              </svg>
              <span className="absolute text-3xl font-bold">{stats?.complianceScore || 0}%</span>
            </div>
            <p className="mt-2 text-sm text-gray-500">Overall Compliance Score</p>
          </div>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Recent Incidents */}
        <div className="rounded-lg bg-white shadow">
          <div className="border-b border-gray-200 px-6 py-4">
            <h3 className="text-lg font-medium text-gray-900">Recent Security Incidents</h3>
          </div>
          <div className="divide-y divide-gray-200">
            {recentIncidents.length === 0 ? (
              <div className="p-6 text-center text-gray-500">
                <ShieldCheckIcon className="mx-auto h-8 w-8 mb-2" />
                <p>No recent incidents</p>
              </div>
            ) : (
              recentIncidents.map((incident) => (
                <div key={incident.id} className="px-6 py-4 hover:bg-gray-50">
                  <div className="flex items-start justify-between">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${severityColors[incident.severity]}`}>
                          {severityLabels[incident.severity]}
                        </span>
                        <span className="text-sm font-medium text-gray-900">{incident.title}</span>
                      </div>
                      <p className="mt-1 text-sm text-gray-500 line-clamp-1">{incident.description}</p>
                    </div>
                    <span className="text-xs text-gray-400">{formatDate(incident.detectedAt)}</span>
                  </div>
                </div>
              ))
            )}
          </div>
          <div className="border-t border-gray-200 px-6 py-3">
            <a href="/security/incidents" className="text-sm text-blue-600 hover:text-blue-800">
              View all incidents →
            </a>
          </div>
        </div>

        {/* Recent Audit Logs */}
        <div className="rounded-lg bg-white shadow">
          <div className="border-b border-gray-200 px-6 py-4">
            <h3 className="text-lg font-medium text-gray-900">Recent Audit Activity</h3>
          </div>
          <div className="divide-y divide-gray-200">
            {recentAuditLogs.length === 0 ? (
              <div className="p-6 text-center text-gray-500">
                <ClipboardDocumentCheckIcon className="mx-auto h-8 w-8 mb-2" />
                <p>No recent activity</p>
              </div>
            ) : (
              recentAuditLogs.map((log) => (
                <div key={log.id} className="px-6 py-3 hover:bg-gray-50">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className={`h-2 w-2 rounded-full ${log.success ? 'bg-green-500' : 'bg-red-500'}`} />
                      <div>
                        <span className="text-sm font-medium text-gray-900">{log.username}</span>
                        <span className="text-sm text-gray-500 ml-2">{actionLabels[log.action]}</span>
                        <span className="text-sm text-gray-400 ml-1">in {log.module}</span>
                      </div>
                    </div>
                    <span className="text-xs text-gray-400">{formatDate(log.timestamp)}</span>
                  </div>
                </div>
              ))
            )}
          </div>
          <div className="border-t border-gray-200 px-6 py-3">
            <a href="/security/audit-logs" className="text-sm text-blue-600 hover:text-blue-800">
              View all audit logs →
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
