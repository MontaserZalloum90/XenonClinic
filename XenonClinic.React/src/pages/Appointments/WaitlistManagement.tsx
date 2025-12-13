import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  ClockIcon,
  PlusIcon,
  TrashIcon,
  PhoneIcon,
  EnvelopeIcon,
  CheckCircleIcon,
  XCircleIcon,
  ArrowUpIcon,
  ArrowDownIcon,
  BellIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';

interface WaitlistEntry {
  id: number;
  patientId: number;
  patientName: string;
  patientPhone: string;
  patientEmail: string;
  doctorId?: number;
  doctorName?: string;
  department: string;
  appointmentType: string;
  preferredDays: string[];
  preferredTimeSlots: string[];
  urgency: 'low' | 'medium' | 'high' | 'urgent';
  reason: string;
  status: 'waiting' | 'contacted' | 'scheduled' | 'cancelled' | 'expired';
  position: number;
  addedAt: string;
  lastContactedAt?: string;
  notes?: string;
  notificationsSent: number;
}

const urgencyConfig = {
  low: { label: 'Low', color: 'bg-gray-100 text-gray-800' },
  medium: { label: 'Medium', color: 'bg-yellow-100 text-yellow-800' },
  high: { label: 'High', color: 'bg-orange-100 text-orange-800' },
  urgent: { label: 'Urgent', color: 'bg-red-100 text-red-800' },
};

const statusConfig = {
  waiting: { label: 'Waiting', color: 'bg-blue-100 text-blue-800' },
  contacted: { label: 'Contacted', color: 'bg-purple-100 text-purple-800' },
  scheduled: { label: 'Scheduled', color: 'bg-green-100 text-green-800' },
  cancelled: { label: 'Cancelled', color: 'bg-gray-100 text-gray-800' },
  expired: { label: 'Expired', color: 'bg-red-100 text-red-800' },
};

export function WaitlistManagement() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<WaitlistEntry | null>(null);
  const [departmentFilter, setDepartmentFilter] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<WaitlistEntry['status'] | 'all'>('waiting');
  const queryClient = useQueryClient();

  const { data: waitlist = [], isLoading } = useQuery({
    queryKey: ['waitlist', departmentFilter, statusFilter],
    queryFn: () => api.get<WaitlistEntry[]>('/api/appointments/waitlist'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/appointments/waitlist/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['waitlist'] });
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      api.put(`/api/appointments/waitlist/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['waitlist'] });
    },
  });

  const notifyPatientMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/appointments/waitlist/${id}/notify`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['waitlist'] });
    },
  });

  const movePositionMutation = useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: 'up' | 'down' }) =>
      api.post(`/api/appointments/waitlist/${id}/move`, { direction }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['waitlist'] });
    },
  });

  const departments = [...new Set(waitlist.map((w) => w.department))];

  const filteredWaitlist = waitlist.filter((entry) => {
    const matchesDepartment = departmentFilter === 'all' || entry.department === departmentFilter;
    const matchesStatus = statusFilter === 'all' || entry.status === statusFilter;
    return matchesDepartment && matchesStatus;
  }).sort((a, b) => a.position - b.position);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const getDaysSinceAdded = (dateString: string) => {
    const added = new Date(dateString);
    const now = new Date();
    return Math.floor((now.getTime() - added.getTime()) / (1000 * 60 * 60 * 24));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Waitlist Management</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage patient waiting lists for appointments
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedEntry(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Add to Waitlist
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {waitlist.filter((w) => w.status === 'waiting').length}
          </div>
          <div className="text-sm text-gray-500">Waiting</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-purple-600">
            {waitlist.filter((w) => w.status === 'contacted').length}
          </div>
          <div className="text-sm text-gray-500">Contacted</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {waitlist.filter((w) => w.urgency === 'urgent').length}
          </div>
          <div className="text-sm text-gray-500">Urgent</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {waitlist.filter((w) => w.status === 'scheduled').length}
          </div>
          <div className="text-sm text-gray-500">Scheduled</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-600">
            {Math.round(waitlist.reduce((acc, w) => acc + getDaysSinceAdded(w.addedAt), 0) / (waitlist.length || 1))}
          </div>
          <div className="text-sm text-gray-500">Avg. Wait (days)</div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <select
          value={departmentFilter}
          onChange={(e) => setDepartmentFilter(e.target.value)}
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Departments</option>
          {departments.map((dept) => (
            <option key={dept} value={dept}>
              {dept}
            </option>
          ))}
        </select>
        <div className="flex gap-2 flex-wrap">
          {(['all', 'waiting', 'contacted', 'scheduled'] as const).map((status) => (
            <button
              key={status}
              onClick={() => setStatusFilter(status)}
              className={`rounded-md px-3 py-1.5 text-sm font-medium ${
                statusFilter === status
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {status === 'all' ? 'All' : statusConfig[status].label}
            </button>
          ))}
        </div>
      </div>

      {/* Waitlist Table */}
      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredWaitlist.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ClockIcon className="h-12 w-12 mb-2" />
            <p>No patients on waitlist</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Position
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Department / Doctor
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Preferences
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Urgency
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Wait Time
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
              {filteredWaitlist.map((entry, index) => {
                const urgency = urgencyConfig[entry.urgency];
                const status = statusConfig[entry.status];
                const daysSince = getDaysSinceAdded(entry.addedAt);

                return (
                  <tr key={entry.id} className="hover:bg-gray-50">
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className="flex items-center gap-1">
                        <span className="text-lg font-bold text-gray-900">#{entry.position}</span>
                        {entry.status === 'waiting' && (
                          <div className="flex flex-col">
                            <button
                              onClick={() => movePositionMutation.mutate({ id: entry.id, direction: 'up' })}
                              disabled={index === 0}
                              className="text-gray-400 hover:text-gray-600 disabled:opacity-30"
                            >
                              <ArrowUpIcon className="h-4 w-4" />
                            </button>
                            <button
                              onClick={() => movePositionMutation.mutate({ id: entry.id, direction: 'down' })}
                              disabled={index === filteredWaitlist.length - 1}
                              className="text-gray-400 hover:text-gray-600 disabled:opacity-30"
                            >
                              <ArrowDownIcon className="h-4 w-4" />
                            </button>
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">{entry.patientName}</div>
                        <div className="flex items-center gap-3 mt-1">
                          <a href={`tel:${entry.patientPhone}`} className="text-sm text-blue-600 flex items-center">
                            <PhoneIcon className="h-3 w-3 mr-1" />
                            {entry.patientPhone}
                          </a>
                          <a href={`mailto:${entry.patientEmail}`} className="text-sm text-blue-600 flex items-center">
                            <EnvelopeIcon className="h-3 w-3 mr-1" />
                            Email
                          </a>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">{entry.department}</div>
                      {entry.doctorName && (
                        <div className="text-sm text-gray-500">{entry.doctorName}</div>
                      )}
                      <div className="text-xs text-gray-400">{entry.appointmentType}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-500">
                        <div>{entry.preferredDays.join(', ')}</div>
                        <div className="text-xs">{entry.preferredTimeSlots.join(', ')}</div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${urgency.color}`}>
                        {urgency.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <div className={`text-sm font-medium ${daysSince > 14 ? 'text-red-600' : daysSince > 7 ? 'text-yellow-600' : 'text-gray-900'}`}>
                        {daysSince} days
                      </div>
                      <div className="text-xs text-gray-500">
                        Added: {formatDate(entry.addedAt)}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}>
                        {status.label}
                      </span>
                      {entry.notificationsSent > 0 && (
                        <div className="text-xs text-gray-400 mt-1">
                          {entry.notificationsSent} notification{entry.notificationsSent !== 1 ? 's' : ''} sent
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        {entry.status === 'waiting' && (
                          <>
                            <button
                              onClick={() => notifyPatientMutation.mutate(entry.id)}
                              className="text-blue-600 hover:text-blue-900"
                              title="Send Notification"
                            >
                              <BellIcon className="h-5 w-5" />
                            </button>
                            <button
                              onClick={() => updateStatusMutation.mutate({ id: entry.id, status: 'contacted' })}
                              className="text-purple-600 hover:text-purple-900"
                              title="Mark as Contacted"
                            >
                              <PhoneIcon className="h-5 w-5" />
                            </button>
                          </>
                        )}
                        {entry.status === 'contacted' && (
                          <button
                            onClick={() => updateStatusMutation.mutate({ id: entry.id, status: 'scheduled' })}
                            className="text-green-600 hover:text-green-900"
                            title="Mark as Scheduled"
                          >
                            <CheckCircleIcon className="h-5 w-5" />
                          </button>
                        )}
                        {['waiting', 'contacted'].includes(entry.status) && (
                          <button
                            onClick={() => updateStatusMutation.mutate({ id: entry.id, status: 'cancelled' })}
                            className="text-gray-600 hover:text-gray-900"
                            title="Cancel"
                          >
                            <XCircleIcon className="h-5 w-5" />
                          </button>
                        )}
                        <button
                          onClick={() => deleteMutation.mutate(entry.id)}
                          className="text-red-600 hover:text-red-900"
                          title="Remove"
                        >
                          <TrashIcon className="h-5 w-5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Add to Waitlist Modal */}
      <WaitlistModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        entry={selectedEntry}
      />
    </div>
  );
}

function WaitlistModal({
  isOpen,
  onClose,
  entry,
}: {
  isOpen: boolean;
  onClose: () => void;
  entry: WaitlistEntry | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    patientId: entry?.patientId || 0,
    patientName: entry?.patientName || '',
    patientPhone: entry?.patientPhone || '',
    patientEmail: entry?.patientEmail || '',
    department: entry?.department || '',
    doctorId: entry?.doctorId || undefined,
    appointmentType: entry?.appointmentType || '',
    preferredDays: entry?.preferredDays || [],
    preferredTimeSlots: entry?.preferredTimeSlots || [],
    urgency: entry?.urgency || 'medium',
    reason: entry?.reason || '',
    notes: entry?.notes || '',
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      entry
        ? api.put(`/api/appointments/waitlist/${entry.id}`, data)
        : api.post('/api/appointments/waitlist', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['waitlist'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(formData);
  };

  const toggleDay = (day: string) => {
    setFormData((prev) => ({
      ...prev,
      preferredDays: prev.preferredDays.includes(day)
        ? prev.preferredDays.filter((d) => d !== day)
        : [...prev.preferredDays, day],
    }));
  };

  const toggleTimeSlot = (slot: string) => {
    setFormData((prev) => ({
      ...prev,
      preferredTimeSlots: prev.preferredTimeSlots.includes(slot)
        ? prev.preferredTimeSlots.filter((s) => s !== slot)
        : [...prev.preferredTimeSlots, slot],
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
          <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
            {entry ? 'Edit Waitlist Entry' : 'Add to Waitlist'}
          </Dialog.Title>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700">Patient Name</label>
                <input
                  type="text"
                  required
                  value={formData.patientName}
                  onChange={(e) => setFormData({ ...formData, patientName: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Phone</label>
                <input
                  type="tel"
                  required
                  value={formData.patientPhone}
                  onChange={(e) => setFormData({ ...formData, patientPhone: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Email</label>
                <input
                  type="email"
                  required
                  value={formData.patientEmail}
                  onChange={(e) => setFormData({ ...formData, patientEmail: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Department</label>
                <input
                  type="text"
                  required
                  value={formData.department}
                  onChange={(e) => setFormData({ ...formData, department: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Appointment Type</label>
                <input
                  type="text"
                  required
                  value={formData.appointmentType}
                  onChange={(e) => setFormData({ ...formData, appointmentType: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Urgency</label>
                <select
                  value={formData.urgency}
                  onChange={(e) => setFormData({ ...formData, urgency: e.target.value as WaitlistEntry['urgency'] })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="low">Low</option>
                  <option value="medium">Medium</option>
                  <option value="high">High</option>
                  <option value="urgent">Urgent</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Preferred Days</label>
              <div className="flex flex-wrap gap-2">
                {['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'].map((day) => (
                  <button
                    key={day}
                    type="button"
                    onClick={() => toggleDay(day)}
                    className={`rounded-md px-3 py-1 text-sm ${
                      formData.preferredDays.includes(day)
                        ? 'bg-blue-600 text-white'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    {day.substring(0, 3)}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Preferred Time Slots</label>
              <div className="flex flex-wrap gap-2">
                {['Morning', 'Afternoon', 'Evening'].map((slot) => (
                  <button
                    key={slot}
                    type="button"
                    onClick={() => toggleTimeSlot(slot)}
                    className={`rounded-md px-3 py-1 text-sm ${
                      formData.preferredTimeSlots.includes(slot)
                        ? 'bg-blue-600 text-white'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    {slot}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Reason for Visit</label>
              <textarea
                rows={2}
                required
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Notes</label>
              <textarea
                rows={2}
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
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
                {mutation.isPending ? 'Saving...' : entry ? 'Update' : 'Add to Waitlist'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
