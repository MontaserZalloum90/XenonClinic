import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import {
  ArrowPathIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  CalendarDaysIcon,
  ClockIcon,
  CheckCircleIcon,
  PauseCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';

interface RecurringAppointment {
  id: number;
  patientId: number;
  patientName: string;
  doctorId: number;
  doctorName: string;
  department: string;
  appointmentType: string;
  frequency: 'daily' | 'weekly' | 'biweekly' | 'monthly' | 'quarterly';
  dayOfWeek?: number;
  dayOfMonth?: number;
  preferredTime: string;
  duration: number;
  startDate: string;
  endDate?: string;
  occurrencesGenerated: number;
  maxOccurrences?: number;
  status: 'active' | 'paused' | 'completed' | 'cancelled';
  notes?: string;
  lastGeneratedDate?: string;
  nextOccurrence?: string;
  createdAt: string;
}

const frequencyConfig = {
  daily: { label: 'Daily', description: 'Every day' },
  weekly: { label: 'Weekly', description: 'Once a week' },
  biweekly: { label: 'Bi-weekly', description: 'Every two weeks' },
  monthly: { label: 'Monthly', description: 'Once a month' },
  quarterly: { label: 'Quarterly', description: 'Every three months' },
};

const statusConfig = {
  active: { label: 'Active', color: 'bg-green-100 text-green-800', icon: CheckCircleIcon },
  paused: { label: 'Paused', color: 'bg-yellow-100 text-yellow-800', icon: PauseCircleIcon },
  completed: { label: 'Completed', color: 'bg-gray-100 text-gray-800', icon: CheckCircleIcon },
  cancelled: { label: 'Cancelled', color: 'bg-red-100 text-red-800', icon: XCircleIcon },
};

const daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

export function RecurringAppointments() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecurring, setSelectedRecurring] = useState<RecurringAppointment | null>(null);
  const [statusFilter, setStatusFilter] = useState<RecurringAppointment['status'] | 'all'>('active');
  const queryClient = useQueryClient();

  const { data: recurringList = [], isLoading } = useQuery({
    queryKey: ['recurring-appointments'],
    queryFn: () => api.get<RecurringAppointment[]>('/api/appointments/recurring'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/api/appointments/recurring/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recurring-appointments'] });
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      api.put(`/api/appointments/recurring/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recurring-appointments'] });
    },
  });

  const filteredList = recurringList.filter((apt) => {
    return statusFilter === 'all' || apt.status === statusFilter;
  });

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const formatTime = (timeString: string) => {
    const [hours, minutes] = timeString.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
  };

  const handleEdit = (recurring: RecurringAppointment) => {
    setSelectedRecurring(recurring);
    setIsModalOpen(true);
  };

  const handleDelete = (recurring: RecurringAppointment) => {
    if (confirm(`Delete recurring appointment for ${recurring.patientName}?`)) {
      deleteMutation.mutate(recurring.id);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Recurring Appointments</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage repeating appointment schedules
          </p>
        </div>
        <button
          onClick={() => {
            setSelectedRecurring(null);
            setIsModalOpen(true);
          }}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Create Recurring
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <ArrowPathIcon className="h-8 w-8 text-blue-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">{recurringList.length}</div>
              <div className="text-sm text-gray-500">Total Recurring</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <CheckCircleIcon className="h-8 w-8 text-green-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {recurringList.filter((r) => r.status === 'active').length}
              </div>
              <div className="text-sm text-gray-500">Active</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <PauseCircleIcon className="h-8 w-8 text-yellow-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {recurringList.filter((r) => r.status === 'paused').length}
              </div>
              <div className="text-sm text-gray-500">Paused</div>
            </div>
          </div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="flex items-center">
            <CalendarDaysIcon className="h-8 w-8 text-purple-500 mr-3" />
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {recurringList.reduce((acc, r) => acc + r.occurrencesGenerated, 0)}
              </div>
              <div className="text-sm text-gray-500">Total Occurrences</div>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex gap-2">
        {(['all', 'active', 'paused', 'completed', 'cancelled'] as const).map((status) => (
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

      {/* Recurring List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredList.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
            <ArrowPathIcon className="h-12 w-12 mb-2" />
            <p>No recurring appointments found</p>
          </div>
        ) : (
          filteredList.map((recurring) => {
            const status = statusConfig[recurring.status];
            const frequency = frequencyConfig[recurring.frequency];
            const StatusIcon = status.icon;

            return (
              <div
                key={recurring.id}
                className="rounded-lg bg-white shadow hover:shadow-md transition-shadow"
              >
                <div className="p-6">
                  <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
                    {/* Patient & Doctor Info */}
                    <div className="flex-1">
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                          <ArrowPathIcon className="h-5 w-5 text-blue-600" />
                        </div>
                        <div>
                          <h3 className="font-semibold text-gray-900">{recurring.patientName}</h3>
                          <p className="text-sm text-gray-500">
                            with {recurring.doctorName} • {recurring.department}
                          </p>
                        </div>
                      </div>
                    </div>

                    {/* Schedule Info */}
                    <div className="flex-1 grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-xs text-gray-500">Frequency</p>
                        <p className="text-sm font-medium text-gray-900">{frequency.label}</p>
                        {recurring.dayOfWeek !== undefined && (
                          <p className="text-xs text-gray-500">
                            Every {daysOfWeek[recurring.dayOfWeek]}
                          </p>
                        )}
                        {recurring.dayOfMonth !== undefined && (
                          <p className="text-xs text-gray-500">
                            Day {recurring.dayOfMonth} of each month
                          </p>
                        )}
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Time</p>
                        <p className="text-sm font-medium text-gray-900 flex items-center">
                          <ClockIcon className="h-4 w-4 mr-1 text-gray-400" />
                          {formatTime(recurring.preferredTime)}
                        </p>
                        <p className="text-xs text-gray-500">{recurring.duration} min</p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Next Occurrence</p>
                        <p className="text-sm font-medium text-gray-900">
                          {formatDate(recurring.nextOccurrence)}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Generated</p>
                        <p className="text-sm font-medium text-gray-900">
                          {recurring.occurrencesGenerated}
                          {recurring.maxOccurrences && ` / ${recurring.maxOccurrences}`}
                        </p>
                      </div>
                    </div>

                    {/* Status & Actions */}
                    <div className="flex flex-col items-end gap-2">
                      <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${status.color}`}>
                        <StatusIcon className="h-4 w-4 mr-1" />
                        {status.label}
                      </span>
                      <div className="text-xs text-gray-500">
                        {formatDate(recurring.startDate)} - {recurring.endDate ? formatDate(recurring.endDate) : 'Ongoing'}
                      </div>
                    </div>
                  </div>

                  {/* Actions Bar */}
                  <div className="mt-4 pt-4 border-t flex items-center justify-between">
                    <div className="text-sm text-gray-500">
                      {recurring.appointmentType}
                      {recurring.notes && ` • ${recurring.notes}`}
                    </div>
                    <div className="flex items-center gap-2">
                      {recurring.status === 'active' && (
                        <button
                          onClick={() => updateStatusMutation.mutate({ id: recurring.id, status: 'paused' })}
                          className="inline-flex items-center rounded-md border border-yellow-300 bg-white px-3 py-1.5 text-sm text-yellow-700 hover:bg-yellow-50"
                        >
                          <PauseCircleIcon className="h-4 w-4 mr-1" />
                          Pause
                        </button>
                      )}
                      {recurring.status === 'paused' && (
                        <button
                          onClick={() => updateStatusMutation.mutate({ id: recurring.id, status: 'active' })}
                          className="inline-flex items-center rounded-md border border-green-300 bg-white px-3 py-1.5 text-sm text-green-700 hover:bg-green-50"
                        >
                          <CheckCircleIcon className="h-4 w-4 mr-1" />
                          Resume
                        </button>
                      )}
                      <button
                        onClick={() => handleEdit(recurring)}
                        className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50"
                      >
                        <PencilIcon className="h-4 w-4 mr-1" />
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(recurring)}
                        className="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-1.5 text-sm text-red-700 hover:bg-red-50"
                      >
                        <TrashIcon className="h-4 w-4 mr-1" />
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Create/Edit Modal */}
      <RecurringModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        recurring={selectedRecurring}
      />
    </div>
  );
}

function RecurringModal({
  isOpen,
  onClose,
  recurring,
}: {
  isOpen: boolean;
  onClose: () => void;
  recurring: RecurringAppointment | null;
}) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState({
    patientId: recurring?.patientId || 0,
    patientName: recurring?.patientName || '',
    doctorId: recurring?.doctorId || 0,
    doctorName: recurring?.doctorName || '',
    department: recurring?.department || '',
    appointmentType: recurring?.appointmentType || '',
    frequency: recurring?.frequency || 'weekly',
    dayOfWeek: recurring?.dayOfWeek ?? 1,
    dayOfMonth: recurring?.dayOfMonth ?? 1,
    preferredTime: recurring?.preferredTime || '09:00',
    duration: recurring?.duration || 30,
    startDate: recurring?.startDate?.split('T')[0] || new Date().toISOString().split('T')[0],
    endDate: recurring?.endDate?.split('T')[0] || '',
    maxOccurrences: recurring?.maxOccurrences || undefined,
    notes: recurring?.notes || '',
  });

  const mutation = useMutation({
    mutationFn: (data: typeof formData) =>
      recurring
        ? api.put(`/api/appointments/recurring/${recurring.id}`, data)
        : api.post('/api/appointments/recurring', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recurring-appointments'] });
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
            {recurring ? 'Edit Recurring Appointment' : 'Create Recurring Appointment'}
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
                <label className="block text-sm font-medium text-gray-700">Doctor Name</label>
                <input
                  type="text"
                  required
                  value={formData.doctorName}
                  onChange={(e) => setFormData({ ...formData, doctorName: e.target.value })}
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
                <label className="block text-sm font-medium text-gray-700">Frequency</label>
                <select
                  value={formData.frequency}
                  onChange={(e) => setFormData({ ...formData, frequency: e.target.value as RecurringAppointment['frequency'] })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  {Object.entries(frequencyConfig).map(([key, value]) => (
                    <option key={key} value={key}>
                      {value.label}
                    </option>
                  ))}
                </select>
              </div>

              {formData.frequency === 'weekly' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700">Day of Week</label>
                  <select
                    value={formData.dayOfWeek}
                    onChange={(e) => setFormData({ ...formData, dayOfWeek: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  >
                    {daysOfWeek.map((day, index) => (
                      <option key={day} value={index}>
                        {day}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              {formData.frequency === 'monthly' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700">Day of Month</label>
                  <input
                    type="number"
                    min="1"
                    max="28"
                    value={formData.dayOfMonth}
                    onChange={(e) => setFormData({ ...formData, dayOfMonth: Number(e.target.value) })}
                    className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700">Preferred Time</label>
                <input
                  type="time"
                  required
                  value={formData.preferredTime}
                  onChange={(e) => setFormData({ ...formData, preferredTime: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Duration (min)</label>
                <input
                  type="number"
                  min="5"
                  step="5"
                  required
                  value={formData.duration}
                  onChange={(e) => setFormData({ ...formData, duration: Number(e.target.value) })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Start Date</label>
                <input
                  type="date"
                  required
                  value={formData.startDate}
                  onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">End Date (optional)</label>
                <input
                  type="date"
                  value={formData.endDate}
                  onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Max Occurrences (optional)</label>
                <input
                  type="number"
                  min="1"
                  value={formData.maxOccurrences || ''}
                  onChange={(e) => setFormData({ ...formData, maxOccurrences: e.target.value ? Number(e.target.value) : undefined })}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
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
                {mutation.isPending ? 'Saving...' : recurring ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
