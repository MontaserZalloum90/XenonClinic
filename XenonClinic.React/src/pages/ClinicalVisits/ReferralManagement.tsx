import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  ArrowRightOnRectangleIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  PrinterIcon,
  DocumentDuplicateIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';

interface Referral {
  id: number;
  referralNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  patientPhone: string;
  referringDoctorId: number;
  referringDoctorName: string;
  referringDepartment: string;
  referredToType: 'internal' | 'external';
  referredToDoctorId?: number;
  referredToDoctorName?: string;
  referredToDepartment: string;
  referredToFacility?: string;
  priority: 'routine' | 'urgent' | 'emergent';
  status: 'pending' | 'accepted' | 'scheduled' | 'completed' | 'declined' | 'cancelled';
  reason: string;
  clinicalSummary?: string;
  attachments?: string[];
  appointmentDate?: string;
  createdAt: string;
  updatedAt?: string;
  responseNotes?: string;
}

const priorityConfig = {
  routine: { label: 'Routine', color: 'bg-gray-100 text-gray-800' },
  urgent: { label: 'Urgent', color: 'bg-orange-100 text-orange-800' },
  emergent: { label: 'Emergent', color: 'bg-red-100 text-red-800' },
};

const statusConfig = {
  pending: { label: 'Pending', color: 'bg-yellow-100 text-yellow-800', icon: ClockIcon },
  accepted: { label: 'Accepted', color: 'bg-blue-100 text-blue-800', icon: CheckCircleIcon },
  scheduled: { label: 'Scheduled', color: 'bg-purple-100 text-purple-800', icon: CheckCircleIcon },
  completed: { label: 'Completed', color: 'bg-green-100 text-green-800', icon: CheckCircleIcon },
  declined: { label: 'Declined', color: 'bg-red-100 text-red-800', icon: XCircleIcon },
  cancelled: { label: 'Cancelled', color: 'bg-gray-100 text-gray-800', icon: XCircleIcon },
};

export function ReferralManagement() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedReferral, setSelectedReferral] = useState<Referral | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<Referral['status'] | 'all'>('all');
  const [typeFilter, setTypeFilter] = useState<Referral['referredToType'] | 'all'>('all');
  const queryClient = useQueryClient();

  const { data: referrals = [], isLoading } = useQuery({
    queryKey: ['referrals'],
    queryFn: () => api.get<Referral[]>('/api/clinical/referrals'),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      api.put(`/api/clinical/referrals/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['referrals'] });
    },
  });

  const filteredReferrals = referrals.filter((ref) => {
    const matchesSearch =
      ref.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ref.referralNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ref.referredToDepartment.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || ref.status === statusFilter;
    const matchesType = typeFilter === 'all' || ref.referredToType === typeFilter;
    return matchesSearch && matchesStatus && matchesType;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const handleEdit = (referral: Referral) => {
    setSelectedReferral(referral);
    setIsModalOpen(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Referral Management</h1>
          <p className="mt-1 text-sm text-gray-500">
            Create and track patient referrals
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedReferral(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Referral
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">{referrals.length}</div>
          <div className="text-sm text-gray-500">Total Referrals</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {referrals.filter((r) => r.status === 'pending').length}
          </div>
          <div className="text-sm text-gray-500">Pending</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-purple-600">
            {referrals.filter((r) => r.status === 'scheduled').length}
          </div>
          <div className="text-sm text-gray-500">Scheduled</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {referrals.filter((r) => r.status === 'completed').length}
          </div>
          <div className="text-sm text-gray-500">Completed</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {referrals.filter((r) => r.priority === 'urgent' || r.priority === 'emergent').length}
          </div>
          <div className="text-sm text-gray-500">Urgent/Emergent</div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search referrals..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2 flex-wrap">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as Referral['status'] | 'all')}
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
          >
            <option value="all">All Status</option>
            {Object.entries(statusConfig).map(([key, value]) => (
              <option key={key} value={key}>
                {value.label}
              </option>
            ))}
          </select>
          <select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value as Referral['referredToType'] | 'all')}
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
          >
            <option value="all">All Types</option>
            <option value="internal">Internal</option>
            <option value="external">External</option>
          </select>
        </div>
      </div>

      {/* Referrals List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredReferrals.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
            <ArrowRightOnRectangleIcon className="h-12 w-12 mb-2" />
            <p>No referrals found</p>
          </div>
        ) : (
          filteredReferrals.map((referral) => {
            const status = statusConfig[referral.status];
            const priority = priorityConfig[referral.priority];
            const StatusIcon = status.icon;

            return (
              <div
                key={referral.id}
                className={`rounded-lg bg-white shadow ${
                  referral.priority === 'emergent'
                    ? 'ring-2 ring-red-500'
                    : referral.priority === 'urgent'
                    ? 'ring-2 ring-orange-500'
                    : ''
                }`}
              >
                <div className="p-4 sm:p-6">
                  {/* Header Row */}
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center gap-3">
                      <div className={`h-10 w-10 rounded-full flex items-center justify-center ${
                        referral.referredToType === 'internal' ? 'bg-blue-100' : 'bg-purple-100'
                      }`}>
                        <ArrowRightOnRectangleIcon className={`h-5 w-5 ${
                          referral.referredToType === 'internal' ? 'text-blue-600' : 'text-purple-600'
                        }`} />
                      </div>
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="font-mono text-sm text-gray-500">{referral.referralNumber}</span>
                          <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${priority.color}`}>
                            {priority.label}
                          </span>
                        </div>
                        <h3 className="font-semibold text-gray-900">{referral.patientName}</h3>
                        <p className="text-sm text-gray-500">MRN: {referral.patientMRN}</p>
                      </div>
                    </div>
                    <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${status.color}`}>
                      <StatusIcon className="h-4 w-4 mr-1" />
                      {status.label}
                    </span>
                  </div>

                  {/* Referral Details */}
                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div className="bg-gray-50 rounded-lg p-3">
                      <h4 className="text-xs font-medium text-gray-500 uppercase mb-2">From</h4>
                      <p className="text-sm font-medium text-gray-900">{referral.referringDoctorName}</p>
                      <p className="text-sm text-gray-500">{referral.referringDepartment}</p>
                    </div>
                    <div className="bg-gray-50 rounded-lg p-3">
                      <h4 className="text-xs font-medium text-gray-500 uppercase mb-2">To</h4>
                      <p className="text-sm font-medium text-gray-900">
                        {referral.referredToDoctorName || referral.referredToDepartment}
                      </p>
                      {referral.referredToFacility && (
                        <p className="text-sm text-gray-500">{referral.referredToFacility}</p>
                      )}
                      <span className={`inline-flex rounded px-1.5 py-0.5 text-xs ${
                        referral.referredToType === 'internal' ? 'bg-blue-100 text-blue-700' : 'bg-purple-100 text-purple-700'
                      }`}>
                        {referral.referredToType}
                      </span>
                    </div>
                  </div>

                  {/* Reason */}
                  <div className="mt-4">
                    <h4 className="text-xs font-medium text-gray-500 uppercase mb-1">Reason for Referral</h4>
                    <p className="text-sm text-gray-900">{referral.reason}</p>
                  </div>

                  {/* Appointment Date if scheduled */}
                  {referral.appointmentDate && (
                    <div className="mt-3 inline-flex items-center rounded-md bg-green-50 px-3 py-2">
                      <CheckCircleIcon className="h-4 w-4 text-green-600 mr-2" />
                      <span className="text-sm text-green-800">
                        Appointment scheduled: {formatDate(referral.appointmentDate)}
                      </span>
                    </div>
                  )}

                  {/* Footer */}
                  <div className="mt-4 pt-4 border-t flex items-center justify-between">
                    <div className="text-sm text-gray-500">
                      Created: {formatDate(referral.createdAt)}
                    </div>
                    <div className="flex items-center gap-2">
                      <button className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">
                        <PrinterIcon className="h-4 w-4 mr-1" />
                        Print
                      </button>
                      <button className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">
                        <DocumentDuplicateIcon className="h-4 w-4 mr-1" />
                        Copy
                      </button>
                      <button
                        onClick={() => handleEdit(referral)}
                        className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50"
                      >
                        <PencilIcon className="h-4 w-4 mr-1" />
                        Edit
                      </button>
                      {referral.status === 'pending' && (
                        <button
                          onClick={() => updateStatusMutation.mutate({ id: referral.id, status: 'accepted' })}
                          className="inline-flex items-center rounded-md bg-green-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-green-700"
                        >
                          Accept
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Referral Modal */}
      <ReferralModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        referral={selectedReferral}
      />
    </div>
  );
}

function ReferralModal({
  isOpen,
  onClose,
  referral,
}: {
  isOpen: boolean;
  onClose: () => void;
  referral: Referral | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    patientId: referral?.patientId || 0,
    patientName: referral?.patientName || '',
    patientMRN: referral?.patientMRN || '',
    patientPhone: referral?.patientPhone || '',
    referringDoctorName: referral?.referringDoctorName || '',
    referringDepartment: referral?.referringDepartment || '',
    referredToType: referral?.referredToType || 'internal',
    referredToDoctorName: referral?.referredToDoctorName || '',
    referredToDepartment: referral?.referredToDepartment || '',
    referredToFacility: referral?.referredToFacility || '',
    priority: referral?.priority || 'routine',
    reason: referral?.reason || '',
    clinicalSummary: referral?.clinicalSummary || '',
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      referral
        ? api.put(`/api/clinical/referrals/${referral.id}`, data)
        : api.post('/api/clinical/referrals', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['referrals'] });
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
        <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {referral ? 'Edit Referral' : 'New Referral'}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Patient Info */}
            <div className="border-b pb-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Patient Information</h4>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Patient Name</label>
                  <input
                    type="text"
                    required
                    value={formData.patientName}
                    onChange={(e) => setFormData({ ...formData, patientName: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">MRN</label>
                  <input
                    type="text"
                    required
                    value={formData.patientMRN}
                    onChange={(e) => setFormData({ ...formData, patientMRN: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Phone</label>
                  <input
                    type="tel"
                    required
                    value={formData.patientPhone}
                    onChange={(e) => setFormData({ ...formData, patientPhone: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>
            </div>

            {/* Referring Provider */}
            <div className="border-b pb-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Referring Provider</h4>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Doctor Name</label>
                  <input
                    type="text"
                    required
                    value={formData.referringDoctorName}
                    onChange={(e) => setFormData({ ...formData, referringDoctorName: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Department</label>
                  <input
                    type="text"
                    required
                    value={formData.referringDepartment}
                    onChange={(e) => setFormData({ ...formData, referringDepartment: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>
            </div>

            {/* Referred To */}
            <div className="border-b pb-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Referred To</h4>
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Type</label>
                  <select
                    value={formData.referredToType}
                    onChange={(e) => setFormData({ ...formData, referredToType: e.target.value as 'internal' | 'external' })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="internal">Internal</option>
                    <option value="external">External</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Priority</label>
                  <select
                    value={formData.priority}
                    onChange={(e) => setFormData({ ...formData, priority: e.target.value as Referral['priority'] })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="routine">Routine</option>
                    <option value="urgent">Urgent</option>
                    <option value="emergent">Emergent</option>
                  </select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Specialist / Doctor</label>
                  <input
                    type="text"
                    value={formData.referredToDoctorName}
                    onChange={(e) => setFormData({ ...formData, referredToDoctorName: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Department / Specialty</label>
                  <input
                    type="text"
                    required
                    value={formData.referredToDepartment}
                    onChange={(e) => setFormData({ ...formData, referredToDepartment: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>
              {formData.referredToType === 'external' && (
                <div className="mt-4">
                  <label className="block text-sm font-medium text-gray-700">Facility Name</label>
                  <input
                    type="text"
                    value={formData.referredToFacility}
                    onChange={(e) => setFormData({ ...formData, referredToFacility: e.target.value })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                    placeholder="External facility name and address"
                  />
                </div>
              )}
            </div>

            {/* Clinical Information */}
            <div>
              <label className="block text-sm font-medium text-gray-700">Reason for Referral</label>
              <textarea
                rows={2}
                required
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                placeholder="Primary reason for this referral..."
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Clinical Summary</label>
              <textarea
                rows={3}
                value={formData.clinicalSummary}
                onChange={(e) => setFormData({ ...formData, clinicalSummary: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
                placeholder="Relevant clinical history, findings, and current treatment..."
              />
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
                {mutation.isPending ? 'Saving...' : referral ? 'Update' : 'Create Referral'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
