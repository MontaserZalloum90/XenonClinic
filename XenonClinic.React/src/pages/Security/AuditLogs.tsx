import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  ClipboardDocumentListIcon,
  MagnifyingGlassIcon,
  FunnelIcon,
  ArrowDownTrayIcon,
  CheckCircleIcon,
  XCircleIcon,
  EyeIcon,
} from '@heroicons/react/24/outline';
import { Dialog } from '@headlessui/react';
import { api } from '../../lib/api';
import type { AuditLog, AuditActionType } from '../../types/security';

const actionConfig: Record<number, { label: string; color: string }> = {
  0: { label: 'Create', color: 'bg-green-100 text-green-800' },
  1: { label: 'Read', color: 'bg-gray-100 text-gray-800' },
  2: { label: 'Update', color: 'bg-blue-100 text-blue-800' },
  3: { label: 'Delete', color: 'bg-red-100 text-red-800' },
  4: { label: 'Login', color: 'bg-purple-100 text-purple-800' },
  5: { label: 'Logout', color: 'bg-purple-100 text-purple-800' },
  6: { label: 'Export', color: 'bg-yellow-100 text-yellow-800' },
  7: { label: 'Print', color: 'bg-yellow-100 text-yellow-800' },
  8: { label: 'Permission Change', color: 'bg-orange-100 text-orange-800' },
  9: { label: 'Password Change', color: 'bg-indigo-100 text-indigo-800' },
  10: { label: 'Failed', color: 'bg-red-100 text-red-800' },
};

export function AuditLogs() {
  const [searchTerm, setSearchTerm] = useState('');
  const [actionFilter, setActionFilter] = useState<AuditActionType | 'all'>('all');
  const [moduleFilter, setModuleFilter] = useState<string>('all');
  const [dateRange, setDateRange] = useState({ start: '', end: '' });
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [showFilters, setShowFilters] = useState(false);

  const { data: logs = [], isLoading } = useQuery({
    queryKey: ['audit-logs', actionFilter, moduleFilter, dateRange],
    queryFn: () => api.get<AuditLog[]>('/api/security/audit-logs'),
  });

  const modules = [...new Set(logs.map((log) => log.module))];

  const filteredLogs = logs.filter((log) => {
    const matchesSearch =
      log.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.ipAddress.includes(searchTerm);
    const matchesAction = actionFilter === 'all' || log.action === actionFilter;
    const matchesModule = moduleFilter === 'all' || log.module === moduleFilter;
    const matchesDateRange =
      (!dateRange.start || new Date(log.timestamp) >= new Date(dateRange.start)) &&
      (!dateRange.end || new Date(log.timestamp) <= new Date(dateRange.end));
    return matchesSearch && matchesAction && matchesModule && matchesDateRange;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  };

  const handleExport = () => {
    const csvContent = [
      ['Timestamp', 'User', 'Role', 'Action', 'Module', 'Description', 'IP Address', 'Success'].join(','),
      ...filteredLogs.map((log) =>
        [
          log.timestamp,
          log.username,
          log.userRole,
          actionConfig[log.action].label,
          log.module,
          `"${log.description.replace(/"/g, '""')}"`,
          log.ipAddress,
          log.success ? 'Yes' : 'No',
        ].join(',')
      ),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `audit-logs-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Audit Logs</h1>
          <p className="mt-1 text-sm text-gray-500">
            Track all system activities and user actions
          </p>
        </div>
        <button
          onClick={handleExport}
          className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          <ArrowDownTrayIcon className="mr-2 h-5 w-5" />
          Export CSV
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="text-sm font-medium text-gray-500">Total Logs</div>
          <div className="mt-1 text-3xl font-semibold text-gray-900">{logs.length}</div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="text-sm font-medium text-gray-500">Successful Actions</div>
          <div className="mt-1 text-3xl font-semibold text-green-600">
            {logs.filter((l) => l.success).length}
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="text-sm font-medium text-gray-500">Failed Actions</div>
          <div className="mt-1 text-3xl font-semibold text-red-600">
            {logs.filter((l) => !l.success).length}
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="text-sm font-medium text-gray-500">Active Users</div>
          <div className="mt-1 text-3xl font-semibold text-blue-600">
            {new Set(logs.map((l) => l.userId)).size}
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="rounded-lg bg-white p-4 shadow">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="relative flex-1 max-w-md">
            <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search by user, description, or IP..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => setShowFilters(!showFilters)}
              className={`inline-flex items-center rounded-md border px-3 py-2 text-sm font-medium ${
                showFilters
                  ? 'border-blue-500 bg-blue-50 text-blue-700'
                  : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
              }`}
            >
              <FunnelIcon className="mr-2 h-4 w-4" />
              Filters
            </button>
          </div>
        </div>

        {showFilters && (
          <div className="mt-4 pt-4 border-t grid grid-cols-1 gap-4 sm:grid-cols-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Action Type</label>
              <select
                value={actionFilter}
                onChange={(e) => setActionFilter(e.target.value === 'all' ? 'all' : Number(e.target.value) as AuditActionType)}
                className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
              >
                <option value="all">All Actions</option>
                {Object.entries(actionConfig).map(([key, value]) => (
                  <option key={key} value={key}>
                    {value.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Module</label>
              <select
                value={moduleFilter}
                onChange={(e) => setModuleFilter(e.target.value)}
                className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
              >
                <option value="all">All Modules</option>
                {modules.map((module) => (
                  <option key={module} value={module}>
                    {module}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">From Date</label>
              <input
                type="date"
                value={dateRange.start}
                onChange={(e) => setDateRange({ ...dateRange, start: e.target.value })}
                className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">To Date</label>
              <input
                type="date"
                value={dateRange.end}
                onChange={(e) => setDateRange({ ...dateRange, end: e.target.value })}
                className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
              />
            </div>
          </div>
        )}
      </div>

      {/* Logs Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredLogs.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ClipboardDocumentListIcon className="h-12 w-12 mb-2" />
            <p>No audit logs found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Timestamp
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  User
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Action
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Module / Entity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Description
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredLogs.map((log) => {
                const action = actionConfig[log.action];
                return (
                  <tr key={log.id} className="hover:bg-gray-50">
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDate(log.timestamp)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">{log.username}</div>
                      <div className="text-xs text-gray-500">{log.userRole}</div>
                      <div className="text-xs text-gray-400">{log.ipAddress}</div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${action.color}`}>
                        {action.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="text-sm text-gray-900">{log.module}</div>
                      <div className="text-xs text-gray-500">
                        {log.entityType}
                        {log.entityId && ` #${log.entityId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-500 max-w-xs truncate" title={log.description}>
                        {log.description}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      {log.success ? (
                        <span className="inline-flex items-center text-green-600">
                          <CheckCircleIcon className="h-5 w-5" />
                        </span>
                      ) : (
                        <span className="inline-flex items-center text-red-600">
                          <XCircleIcon className="h-5 w-5" />
                        </span>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <button
                        onClick={() => setSelectedLog(log)}
                        className="text-blue-600 hover:text-blue-900"
                      >
                        <EyeIcon className="h-5 w-5" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Log Detail Modal */}
      <AuditLogDetailModal
        log={selectedLog}
        onClose={() => setSelectedLog(null)}
      />
    </div>
  );
}

function AuditLogDetailModal({
  log,
  onClose,
}: {
  log: AuditLog | null;
  onClose: () => void;
}) {
  if (!log) return null;

  const action = actionConfig[log.action];

  return (
    <Dialog open={!!log} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            Audit Log Details
          </Dialog.Title>

          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-gray-500">Timestamp</label>
                <p className="text-sm text-gray-900">
                  {new Date(log.timestamp).toLocaleString()}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Status</label>
                <p className={`text-sm font-medium ${log.success ? 'text-green-600' : 'text-red-600'}`}>
                  {log.success ? 'Success' : 'Failed'}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">User</label>
                <p className="text-sm text-gray-900">{log.username}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Role</label>
                <p className="text-sm text-gray-900">{log.userRole}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Action</label>
                <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${action.color}`}>
                  {action.label}
                </span>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Module</label>
                <p className="text-sm text-gray-900">{log.module}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Entity Type</label>
                <p className="text-sm text-gray-900">{log.entityType}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Entity ID</label>
                <p className="text-sm text-gray-900">{log.entityId || '-'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">IP Address</label>
                <p className="text-sm text-gray-900">{log.ipAddress}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">User Agent</label>
                <p className="text-sm text-gray-900 truncate" title={log.userAgent}>
                  {log.userAgent || '-'}
                </p>
              </div>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-500">Description</label>
              <p className="text-sm text-gray-900 mt-1">{log.description}</p>
            </div>

            {log.errorMessage && (
              <div className="bg-red-50 p-3 rounded-md">
                <label className="text-sm font-medium text-red-800">Error Message</label>
                <p className="text-sm text-red-700 mt-1">{log.errorMessage}</p>
              </div>
            )}

            {(log.oldValue || log.newValue) && (
              <div className="grid grid-cols-2 gap-4">
                {log.oldValue && (
                  <div className="bg-red-50 p-3 rounded-md">
                    <label className="text-sm font-medium text-red-800">Old Value</label>
                    <pre className="text-xs text-red-700 mt-1 overflow-auto max-h-32">
                      {log.oldValue}
                    </pre>
                  </div>
                )}
                {log.newValue && (
                  <div className="bg-green-50 p-3 rounded-md">
                    <label className="text-sm font-medium text-green-800">New Value</label>
                    <pre className="text-xs text-green-700 mt-1 overflow-auto max-h-32">
                      {log.newValue}
                    </pre>
                  </div>
                )}
              </div>
            )}
          </div>

          <div className="mt-6 flex justify-end">
            <button
              onClick={onClose}
              className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
            >
              Close
            </button>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
