import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  KeyIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  CheckCircleIcon,
  ShieldCheckIcon,
  ClockIcon,
  LockClosedIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';
import type { PasswordPolicy } from '../../types/security';

export function PasswordPolicies() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPolicy, setSelectedPolicy] = useState<PasswordPolicy | null>(null);
  const queryClient = useQueryClient();

  const { data: policies = [], isLoading } = useQuery({
    queryKey: ['password-policies'],
    queryFn: () => api.get<PasswordPolicy[]>('/api/security/password-policies'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/security/password-policies/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['password-policies'] });
    },
  });

  const setDefaultMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/security/password-policies/${id}/set-default`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['password-policies'] });
    },
  });

  const handleEdit = (policy: PasswordPolicy) => {
    setSelectedPolicy(policy);
    setIsModalOpen(true);
  };

  const handleDelete = (policy: PasswordPolicy) => {
    if (policy.isDefault) {
      alert('Cannot delete the default policy. Set another policy as default first.');
      return;
    }
    if (confirm(`Are you sure you want to delete the policy "${policy.name}"?`)) {
      deleteMutation.mutate(policy.id);
    }
  };

  const handleSetDefault = (policy: PasswordPolicy) => {
    if (confirm(`Set "${policy.name}" as the default password policy?`)) {
      setDefaultMutation.mutate(policy.id);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Password Policies</h1>
          <p className="mt-1 text-sm text-gray-500">
            Configure password requirements and security settings
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedPolicy(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Add Policy
        </button>
      </div>

      {/* Policies Grid */}
      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      ) : policies.length === 0 ? (
        <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
          <KeyIcon className="h-12 w-12 mb-2" />
          <p>No password policies configured</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {policies.map((policy) => (
            <div
              key={policy.id}
              className={`rounded-lg bg-white shadow ${
                policy.isDefault ? 'ring-2 ring-blue-500' : ''
              }`}
            >
              <div className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex items-center">
                    <div className={`rounded-full p-3 ${policy.isDefault ? 'bg-blue-100' : 'bg-gray-100'}`}>
                      <KeyIcon className={`h-6 w-6 ${policy.isDefault ? 'text-blue-600' : 'text-gray-600'}`} />
                    </div>
                    <div className="ml-4">
                      <h3 className="font-semibold text-gray-900 flex items-center">
                        {policy.name}
                        {policy.isDefault && (
                          <span className="ml-2 inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-800">
                            Default
                          </span>
                        )}
                      </h3>
                      <span
                        className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                          policy.isActive
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}
                      >
                        {policy.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="mt-6 grid grid-cols-2 gap-4">
                  <div className="flex items-center gap-2">
                    <LockClosedIcon className="h-5 w-5 text-gray-400" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">Min Length</div>
                      <div className="text-sm text-gray-500">{policy.minLength} characters</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <ClockIcon className="h-5 w-5 text-gray-400" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">Max Age</div>
                      <div className="text-sm text-gray-500">{policy.maxAge} days</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <ShieldCheckIcon className="h-5 w-5 text-gray-400" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">Lockout</div>
                      <div className="text-sm text-gray-500">
                        {policy.lockoutThreshold} attempts / {policy.lockoutDuration} min
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <KeyIcon className="h-5 w-5 text-gray-400" />
                    <div>
                      <div className="text-sm font-medium text-gray-900">MFA</div>
                      <div className="text-sm text-gray-500">
                        {policy.mfaRequired ? 'Required' : 'Optional'}
                      </div>
                    </div>
                  </div>
                </div>

                <div className="mt-4 border-t pt-4">
                  <h4 className="text-xs font-medium text-gray-500 uppercase mb-2">Requirements</h4>
                  <div className="flex flex-wrap gap-2">
                    {policy.requireUppercase && (
                      <span className="inline-flex items-center rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-700">
                        <CheckCircleIcon className="h-3 w-3 mr-1 text-green-500" />
                        Uppercase
                      </span>
                    )}
                    {policy.requireLowercase && (
                      <span className="inline-flex items-center rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-700">
                        <CheckCircleIcon className="h-3 w-3 mr-1 text-green-500" />
                        Lowercase
                      </span>
                    )}
                    {policy.requireNumbers && (
                      <span className="inline-flex items-center rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-700">
                        <CheckCircleIcon className="h-3 w-3 mr-1 text-green-500" />
                        Numbers
                      </span>
                    )}
                    {policy.requireSpecialChars && (
                      <span className="inline-flex items-center rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-700">
                        <CheckCircleIcon className="h-3 w-3 mr-1 text-green-500" />
                        Special chars
                      </span>
                    )}
                    {policy.preventReuse > 0 && (
                      <span className="inline-flex items-center rounded bg-gray-100 px-2 py-1 text-xs font-medium text-gray-700">
                        <CheckCircleIcon className="h-3 w-3 mr-1 text-green-500" />
                        No reuse ({policy.preventReuse})
                      </span>
                    )}
                  </div>
                </div>

                <div className="mt-4 pt-4 border-t flex justify-between">
                  {!policy.isDefault && (
                    <button
                      onClick={() => handleSetDefault(policy)}
                      className="text-sm text-blue-600 hover:text-blue-800"
                    >
                      Set as Default
                    </button>
                  )}
                  <div className="flex gap-2 ml-auto">
                    <button
                      onClick={() => handleEdit(policy)}
                      className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50"
                    >
                      <PencilIcon className="h-4 w-4 mr-1" />
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(policy)}
                      disabled={policy.isDefault}
                      className="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-1.5 text-sm font-medium text-red-700 hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <TrashIcon className="h-4 w-4 mr-1" />
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Policy Modal */}
      <PasswordPolicyModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        policy={selectedPolicy}
      />
    </div>
  );
}

function PasswordPolicyModal({
  isOpen,
  onClose,
  policy,
}: {
  isOpen: boolean;
  onClose: () => void;
  policy: PasswordPolicy | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    name: policy?.name || '',
    minLength: policy?.minLength || 8,
    requireUppercase: policy?.requireUppercase ?? true,
    requireLowercase: policy?.requireLowercase ?? true,
    requireNumbers: policy?.requireNumbers ?? true,
    requireSpecialChars: policy?.requireSpecialChars ?? true,
    preventReuse: policy?.preventReuse || 5,
    maxAge: policy?.maxAge || 90,
    lockoutThreshold: policy?.lockoutThreshold || 5,
    lockoutDuration: policy?.lockoutDuration || 30,
    mfaRequired: policy?.mfaRequired ?? false,
    sessionTimeout: policy?.sessionTimeout || 30,
    isActive: policy?.isActive ?? true,
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      policy
        ? api.put(`/api/security/password-policies/${policy.id}`, data)
        : api.post('/api/security/password-policies', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['password-policies'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {policy ? 'Edit Password Policy' : 'Create Password Policy'}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Policy Name</label>
              <input
                type="text"
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Minimum Length</label>
                <input
                  type="number"
                  min="6"
                  max="128"
                  required
                  value={formData.minLength}
                  onChange={(e) => setFormData({ ...formData, minLength: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Max Age (days)</label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.maxAge}
                  onChange={(e) => setFormData({ ...formData, maxAge: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Lockout Threshold</label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.lockoutThreshold}
                  onChange={(e) => setFormData({ ...formData, lockoutThreshold: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Lockout Duration (min)</label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.lockoutDuration}
                  onChange={(e) => setFormData({ ...formData, lockoutDuration: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Prevent Reuse (count)</label>
                <input
                  type="number"
                  min="0"
                  required
                  value={formData.preventReuse}
                  onChange={(e) => setFormData({ ...formData, preventReuse: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Session Timeout (min)</label>
                <input
                  type="number"
                  min="5"
                  required
                  value={formData.sessionTimeout}
                  onChange={(e) => setFormData({ ...formData, sessionTimeout: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>

            <div className="border-t pt-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Character Requirements</h4>
              <div className="grid grid-cols-2 gap-3">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.requireUppercase}
                    onChange={(e) => setFormData({ ...formData, requireUppercase: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Uppercase letters (A-Z)</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.requireLowercase}
                    onChange={(e) => setFormData({ ...formData, requireLowercase: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Lowercase letters (a-z)</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.requireNumbers}
                    onChange={(e) => setFormData({ ...formData, requireNumbers: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Numbers (0-9)</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.requireSpecialChars}
                    onChange={(e) => setFormData({ ...formData, requireSpecialChars: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Special characters (!@#$)</span>
                </label>
              </div>
            </div>

            <div className="border-t pt-4">
              <div className="flex gap-6">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.mfaRequired}
                    onChange={(e) => setFormData({ ...formData, mfaRequired: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Require MFA</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Active</span>
                </label>
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={onClose}
                className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={mutation.isPending}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {mutation.isPending ? 'Saving...' : policy ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
