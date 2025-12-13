import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  UserCircleIcon,
  CheckCircleIcon,
  ClockIcon,
  MagnifyingGlassIcon,
  PrinterIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/24/outline';
import { api } from '../../lib/api';

interface CheckinAppointment {
  id: number;
  patientId: number;
  patientName: string;
  patientMRN: string;
  dateOfBirth: string;
  appointmentTime: string;
  doctorName: string;
  department: string;
  appointmentType: string;
  status: 'scheduled' | 'checked-in' | 'with-nurse' | 'with-doctor' | 'completed' | 'no-show';
  insuranceVerified: boolean;
  copayAmount?: number;
  copayCollected: boolean;
  notes?: string;
  arrivalTime?: string;
  waitTime?: number;
}

const statusConfig = {
  scheduled: { label: 'Scheduled', color: 'bg-gray-100 text-gray-800', next: 'Check In' },
  'checked-in': { label: 'Checked In', color: 'bg-blue-100 text-blue-800', next: 'Send to Nurse' },
  'with-nurse': { label: 'With Nurse', color: 'bg-purple-100 text-purple-800', next: 'Send to Doctor' },
  'with-doctor': { label: 'With Doctor', color: 'bg-indigo-100 text-indigo-800', next: 'Complete' },
  completed: { label: 'Completed', color: 'bg-green-100 text-green-800', next: null },
  'no-show': { label: 'No Show', color: 'bg-red-100 text-red-800', next: null },
};

export function AppointmentCheckin() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [statusFilter, setStatusFilter] = useState<CheckinAppointment['status'] | 'all'>('all');
  const queryClient = useQueryClient();

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['checkin-appointments', selectedDate],
    queryFn: () => api.get<CheckinAppointment[]>(`/api/appointments/checkin?date=${selectedDate}`),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      api.put(`/api/appointments/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['checkin-appointments'] });
    },
  });

  const collectCopayMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/appointments/${id}/collect-copay`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['checkin-appointments'] });
    },
  });

  const filteredAppointments = appointments.filter((apt) => {
    const matchesSearch =
      apt.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      apt.patientMRN.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || apt.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatTime = (timeString: string) => {
    return new Date(timeString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getNextStatus = (currentStatus: CheckinAppointment['status']): CheckinAppointment['status'] | null => {
    const statusOrder: CheckinAppointment['status'][] = ['scheduled', 'checked-in', 'with-nurse', 'with-doctor', 'completed'];
    const currentIndex = statusOrder.indexOf(currentStatus);
    if (currentIndex >= 0 && currentIndex < statusOrder.length - 1) {
      return statusOrder[currentIndex + 1];
    }
    return null;
  };

  const handleStatusUpdate = (apt: CheckinAppointment) => {
    const nextStatus = getNextStatus(apt.status);
    if (nextStatus) {
      updateStatusMutation.mutate({ id: apt.id, status: nextStatus });
    }
  };

  const stats = {
    total: appointments.length,
    checkedIn: appointments.filter((a) => a.status !== 'scheduled').length,
    waiting: appointments.filter((a) => a.status === 'checked-in').length,
    withProvider: appointments.filter((a) => ['with-nurse', 'with-doctor'].includes(a.status)).length,
    completed: appointments.filter((a) => a.status === 'completed').length,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Patient Check-In</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage patient arrivals and appointment flow
          </p>
        </div>
        <div className="flex items-center gap-4">
          <input
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">{stats.total}</div>
          <div className="text-sm text-gray-500">Total Today</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">{stats.checkedIn}</div>
          <div className="text-sm text-gray-500">Checked In</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">{stats.waiting}</div>
          <div className="text-sm text-gray-500">In Waiting</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-purple-600">{stats.withProvider}</div>
          <div className="text-sm text-gray-500">With Provider</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">{stats.completed}</div>
          <div className="text-sm text-gray-500">Completed</div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name or MRN..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div className="flex gap-2 flex-wrap">
          {['all', 'scheduled', 'checked-in', 'with-nurse', 'with-doctor'].map((status) => (
            <button
              key={status}
              onClick={() => setStatusFilter(status as CheckinAppointment['status'] | 'all')}
              className={`rounded-md px-3 py-1.5 text-sm font-medium ${
                statusFilter === status
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {status === 'all' ? 'All' : statusConfig[status as CheckinAppointment['status']]?.label}
            </button>
          ))}
        </div>
      </div>

      {/* Appointments List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredAppointments.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center rounded-lg bg-white shadow text-gray-500">
            <UserCircleIcon className="h-12 w-12 mb-2" />
            <p>No appointments found</p>
          </div>
        ) : (
          filteredAppointments.map((apt) => {
            const status = statusConfig[apt.status];
            const nextStatus = getNextStatus(apt.status);

            return (
              <div
                key={apt.id}
                className="rounded-lg bg-white shadow hover:shadow-md transition-shadow"
              >
                <div className="p-4 sm:p-6">
                  <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                    {/* Patient Info */}
                    <div className="flex items-start gap-4">
                      <div className="h-12 w-12 rounded-full bg-gray-200 flex items-center justify-center">
                        <UserCircleIcon className="h-8 w-8 text-gray-500" />
                      </div>
                      <div>
                        <h3 className="font-semibold text-gray-900">{apt.patientName}</h3>
                        <p className="text-sm text-gray-500">MRN: {apt.patientMRN}</p>
                        <p className="text-sm text-gray-500">DOB: {apt.dateOfBirth}</p>
                      </div>
                    </div>

                    {/* Appointment Info */}
                    <div className="flex-1 grid grid-cols-2 sm:grid-cols-4 gap-4">
                      <div>
                        <p className="text-xs text-gray-500">Time</p>
                        <p className="text-sm font-medium text-gray-900 flex items-center">
                          <ClockIcon className="h-4 w-4 mr-1 text-gray-400" />
                          {formatTime(apt.appointmentTime)}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Doctor</p>
                        <p className="text-sm font-medium text-gray-900">{apt.doctorName}</p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Department</p>
                        <p className="text-sm font-medium text-gray-900">{apt.department}</p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Type</p>
                        <p className="text-sm font-medium text-gray-900">{apt.appointmentType}</p>
                      </div>
                    </div>

                    {/* Status & Actions */}
                    <div className="flex flex-col items-end gap-2">
                      <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${status.color}`}>
                        {status.label}
                      </span>
                      {apt.waitTime !== undefined && apt.status !== 'completed' && (
                        <span className="text-xs text-gray-500">
                          Wait: {apt.waitTime} min
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Additional Info & Actions Bar */}
                  <div className="mt-4 pt-4 border-t flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                    <div className="flex items-center gap-4">
                      {/* Insurance Status */}
                      <div className="flex items-center gap-1">
                        {apt.insuranceVerified ? (
                          <CheckCircleIcon className="h-5 w-5 text-green-500" />
                        ) : (
                          <ExclamationTriangleIcon className="h-5 w-5 text-yellow-500" />
                        )}
                        <span className={`text-sm ${apt.insuranceVerified ? 'text-green-700' : 'text-yellow-700'}`}>
                          {apt.insuranceVerified ? 'Insurance Verified' : 'Verify Insurance'}
                        </span>
                      </div>

                      {/* Copay Status */}
                      {apt.copayAmount !== undefined && apt.copayAmount > 0 && (
                        <div className="flex items-center gap-2">
                          <span className="text-sm text-gray-600">
                            Copay: ${apt.copayAmount.toFixed(2)}
                          </span>
                          {apt.copayCollected ? (
                            <span className="inline-flex items-center rounded bg-green-100 px-2 py-0.5 text-xs text-green-800">
                              Collected
                            </span>
                          ) : (
                            <button
                              onClick={() => collectCopayMutation.mutate(apt.id)}
                              className="inline-flex items-center rounded bg-yellow-100 px-2 py-0.5 text-xs text-yellow-800 hover:bg-yellow-200"
                            >
                              Collect
                            </button>
                          )}
                        </div>
                      )}
                    </div>

                    {/* Action Buttons */}
                    <div className="flex items-center gap-2">
                      <button className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">
                        <PrinterIcon className="h-4 w-4 mr-1" />
                        Print
                      </button>
                      <button className="inline-flex items-center rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">
                        <DocumentTextIcon className="h-4 w-4 mr-1" />
                        Forms
                      </button>
                      {nextStatus && (
                        <button
                          onClick={() => handleStatusUpdate(apt)}
                          disabled={updateStatusMutation.isPending}
                          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                        >
                          {status.next}
                        </button>
                      )}
                      {apt.status === 'scheduled' && (
                        <button
                          onClick={() => updateStatusMutation.mutate({ id: apt.id, status: 'no-show' })}
                          className="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-1.5 text-sm text-red-700 hover:bg-red-50"
                        >
                          No Show
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
    </div>
  );
}
