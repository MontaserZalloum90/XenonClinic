import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  UserGroupIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  ShieldCheckIcon,
  KeyIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';
import type { UserAccessReview, Permission } from '../../types/security';

const statusConfig = {
  pending: { label: 'Pending Review', color: 'bg-yellow-100 text-yellow-800', icon: ClockIcon },
  approved: { label: 'Approved', color: 'bg-green-100 text-green-800', icon: CheckCircleIcon },
  revoked: { label: 'Revoked', color: 'bg-red-100 text-red-800', icon: XCircleIcon },
  modified: { label: 'Modified', color: 'bg-blue-100 text-blue-800', icon: ShieldCheckIcon },
};

const accountStatusConfig = {
  active: { label: 'Active', color: 'bg-green-100 text-green-800' },
  inactive: { label: 'Inactive', color: 'bg-gray-100 text-gray-800' },
  locked: { label: 'Locked', color: 'bg-red-100 text-red-800' },
  expired: { label: 'Expired', color: 'bg-orange-100 text-orange-800' },
};

export function AccessReview() {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<UserAccessReview['reviewStatus'] | 'all'>('all');
  const [selectedUser, setSelectedUser] = useState<UserAccessReview | null>(null);
  const queryClient = useQueryClient();

  const { data: users = [], isLoading } = useQuery({
    queryKey: ['access-reviews'],
    queryFn: () => api.get<UserAccessReview[]>('/api/security/access-reviews'),
  });

  const reviewMutation = useMutation({
    mutationFn: ({ userId, status, notes }: { userId: number; status: string; notes?: string }) =>
      api.post(`/api/security/access-reviews/${userId}/review`, { status, notes }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['access-reviews'] });
      setSelectedUser(null);
    },
  });

  const filteredUsers = users.filter((user) => {
    const matchesSearch =
      user.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || user.reviewStatus === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const pendingCount = users.filter((u) => u.reviewStatus === 'pending').length;

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">User Access Review</h1>
          <p className="mt-1 text-sm text-gray-500">
            Review and manage user access permissions
          </p>
        </div>
        {pendingCount > 0 && (
          <div className="flex items-center bg-yellow-50 border border-yellow-200 rounded-md px-4 py-2">
            <ExclamationTriangleIcon className="h-5 w-5 text-yellow-600 mr-2" />
            <span className="text-sm text-yellow-800">
              {pendingCount} pending review{pendingCount !== 1 ? 's' : ''}
            </span>
          </div>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-blue-100 p-3">
              <UserGroupIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Total Users</p>
              <p className="text-2xl font-semibold text-gray-900">{users.length}</p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-yellow-100 p-3">
              <ClockIcon className="h-6 w-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Pending Review</p>
              <p className="text-2xl font-semibold text-gray-900">{pendingCount}</p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-green-100 p-3">
              <ShieldCheckIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">MFA Enabled</p>
              <p className="text-2xl font-semibold text-gray-900">
                {users.filter((u) => u.mfaEnabled).length}
              </p>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-6 shadow">
          <div className="flex items-center">
            <div className="rounded-full bg-red-100 p-3">
              <XCircleIcon className="h-6 w-6 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Locked Accounts</p>
              <p className="text-2xl font-semibold text-gray-900">
                {users.filter((u) => u.accountStatus === 'locked').length}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search users..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as UserAccessReview['reviewStatus'] | 'all')}
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value="pending">Pending Review</option>
            <option value="approved">Approved</option>
            <option value="revoked">Revoked</option>
            <option value="modified">Modified</option>
          </select>
        </div>
      </div>

      {/* Users Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <UserGroupIcon className="h-12 w-12 mb-2" />
            <p>No users found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  User
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Role / Department
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Account Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Security
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Last Activity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Review Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredUsers.map((user) => {
                const status = statusConfig[user.reviewStatus];
                const accountStatus = accountStatusConfig[user.accountStatus];
                const StatusIcon = status.icon;

                return (
                  <tr key={user.id} className="hover:bg-gray-50">
                    <td className="whitespace-nowrap px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">{user.fullName}</div>
                        <div className="text-sm text-gray-500">{user.username}</div>
                        <div className="text-xs text-gray-400">{user.email}</div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="text-sm text-gray-900">{user.role}</div>
                      <div className="text-xs text-gray-500">{user.department || 'No department'}</div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${accountStatus.color}`}>
                        {accountStatus.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="flex items-center gap-2">
                        <span
                          className={`inline-flex items-center rounded px-2 py-1 text-xs font-medium ${
                            user.mfaEnabled ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'
                          }`}
                        >
                          <KeyIcon className="h-3 w-3 mr-1" />
                          {user.mfaEnabled ? 'MFA On' : 'MFA Off'}
                        </span>
                      </div>
                      <div className="text-xs text-gray-500 mt-1">
                        {user.permissions.filter((p) => p.granted).length} permissions
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      <div>Last login: {formatDate(user.lastLogin)}</div>
                      <div className="text-xs text-gray-400">
                        Password: {formatDate(user.lastPasswordChange)}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}>
                        <StatusIcon className="mr-1 h-3.5 w-3.5" />
                        {status.label}
                      </span>
                      {user.reviewedBy && (
                        <div className="text-xs text-gray-400 mt-1">
                          by {user.reviewedBy}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <button
                        onClick={() => setSelectedUser(user)}
                        className="text-blue-600 hover:text-blue-900 font-medium"
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Review Modal */}
      <AccessReviewModal
        user={selectedUser}
        onClose={() => setSelectedUser(null)}
        onSubmit={(status, notes) => {
          if (selectedUser) {
            reviewMutation.mutate({ userId: selectedUser.id, status, notes });
          }
        }}
        isSubmitting={reviewMutation.isPending}
      />
    </div>
  );
}

function AccessReviewModal({
  user,
  onClose,
  onSubmit,
  isSubmitting,
}: {
  user: UserAccessReview | null;
  onClose: () => void;
  onSubmit: (status: string, notes?: string) => void;
  isSubmitting: boolean;
}) {
  const [notes, setNotes] = useState('');
  const [modifiedPermissions, setModifiedPermissions] = useState<Permission[]>([]);

  if (!user) return null;

  const handlePermissionToggle = (permissionId: number) => {
    setModifiedPermissions((prev) => {
      const existing = prev.find((p) => p.id === permissionId);
      if (existing) {
        return prev.filter((p) => p.id !== permissionId);
      }
      const permission = user.permissions.find((p) => p.id === permissionId);
      if (permission) {
        return [...prev, { ...permission, granted: !permission.granted }];
      }
      return prev;
    });
  };

  const getPermissionGranted = (permission: Permission) => {
    const modified = modifiedPermissions.find((p) => p.id === permission.id);
    return modified ? modified.granted : permission.granted;
  };

  const groupedPermissions = user.permissions.reduce((acc, permission) => {
    const module = permission.module;
    if (!acc[module]) {
      acc[module] = [];
    }
    acc[module].push(permission);
    return acc;
  }, {} as Record<string, Permission[]>);

  return (
    <Dialog open={!!user} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            Access Review: {user.fullName}
          </Dialog.Title>

          {/* User Info */}
          <div className="bg-gray-50 rounded-lg p-4 mb-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-500">Username:</span>
                <span className="ml-2 font-medium">{user.username}</span>
              </div>
              <div>
                <span className="text-gray-500">Email:</span>
                <span className="ml-2 font-medium">{user.email}</span>
              </div>
              <div>
                <span className="text-gray-500">Role:</span>
                <span className="ml-2 font-medium">{user.role}</span>
              </div>
              <div>
                <span className="text-gray-500">Department:</span>
                <span className="ml-2 font-medium">{user.department || 'N/A'}</span>
              </div>
              <div>
                <span className="text-gray-500">Account Status:</span>
                <span className={`ml-2 inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                  accountStatusConfig[user.accountStatus].color
                }`}>
                  {accountStatusConfig[user.accountStatus].label}
                </span>
              </div>
              <div>
                <span className="text-gray-500">MFA:</span>
                <span className={`ml-2 font-medium ${user.mfaEnabled ? 'text-green-600' : 'text-red-600'}`}>
                  {user.mfaEnabled ? 'Enabled' : 'Disabled'}
                </span>
              </div>
            </div>
          </div>

          {/* Permissions */}
          <div className="mb-4">
            <h4 className="text-sm font-medium text-gray-900 mb-2">Permissions</h4>
            <div className="border rounded-lg divide-y max-h-60 overflow-y-auto">
              {Object.entries(groupedPermissions).map(([module, permissions]) => (
                <div key={module} className="p-3">
                  <h5 className="text-xs font-semibold text-gray-700 uppercase mb-2">{module}</h5>
                  <div className="space-y-1">
                    {permissions.map((permission) => (
                      <label
                        key={permission.id}
                        className="flex items-center justify-between py-1 hover:bg-gray-50 rounded px-2 cursor-pointer"
                      >
                        <div>
                          <span className="text-sm text-gray-900">{permission.name}</span>
                          <p className="text-xs text-gray-500">{permission.description}</p>
                        </div>
                        <input
                          type="checkbox"
                          checked={getPermissionGranted(permission)}
                          onChange={() => handlePermissionToggle(permission.id)}
                          className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                      </label>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Notes */}
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Review Notes
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              placeholder="Add any notes about this review..."
              className="w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-between gap-3">
            <button
              type="button"
              onClick={() => onSubmit('revoked', notes)}
              disabled={isSubmitting}
              className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
            >
              Revoke Access
            </button>
            <div className="flex gap-3">
              <button
                type="button"
                onClick={onClose}
                className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              {modifiedPermissions.length > 0 && (
                <button
                  type="button"
                  onClick={() => onSubmit('modified', notes)}
                  disabled={isSubmitting}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                  Save Changes
                </button>
              )}
              <button
                type="button"
                onClick={() => onSubmit('approved', notes)}
                disabled={isSubmitting}
                className="rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
              >
                Approve Access
              </button>
            </div>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
