import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { appointmentsApi } from '../../lib/api';
import { Appointment, AppointmentStatus } from '../../types/appointment';
import { StatusBadge } from '../../components/ui/StatusBadge';
import { format } from 'date-fns';

export const AppointmentsList = () => {
  const [selectedDate, setSelectedDate] = useState<string>('');
  const queryClient = useQueryClient();

  // Fetch appointments
  const { data: appointments, isLoading, error } = useQuery<Appointment[]>({
    queryKey: ['appointments', selectedDate],
    queryFn: async () => {
      if (selectedDate) {
        const response = await appointmentsApi.getByDate(new Date(selectedDate));
        return response.data;
      } else {
        const response = await appointmentsApi.getAll();
        return response.data;
      }
    },
  });

  // Confirm mutation
  const confirmMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.confirm(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
  });

  // Cancel mutation
  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      appointmentsApi.cancel(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
  });

  // Check-in mutation
  const checkInMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.checkIn(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
  });

  // Complete mutation
  const completeMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.complete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
  });

  const formatDateTime = (dateString: string) => {
    try {
      return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
    } catch {
      return dateString;
    }
  };

  const formatTime = (dateString: string) => {
    try {
      return format(new Date(dateString), 'HH:mm');
    } catch {
      return dateString;
    }
  };

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md">
        <p className="text-sm text-red-600">Error loading appointments: {(error as Error).message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Appointments</h1>
          <p className="text-gray-600 mt-1">Manage patient appointments and schedules</p>
        </div>
        <button className="btn btn-primary">
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          New Appointment
        </button>
      </div>

      {/* Filters */}
      <div className="card">
        <div className="flex items-center gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Filter by Date</label>
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="input"
            />
          </div>
          {selectedDate && (
            <button
              onClick={() => setSelectedDate('')}
              className="mt-6 text-sm text-primary-600 hover:text-primary-700"
            >
              Clear filter
            </button>
          )}
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{appointments?.length || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Confirmed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {appointments?.filter((a) => a.status === AppointmentStatus.Confirmed).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Checked In</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {appointments?.filter((a) => a.status === AppointmentStatus.CheckedIn).length || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed</p>
          <p className="text-2xl font-bold text-gray-600 mt-1">
            {appointments?.filter((a) => a.status === AppointmentStatus.Completed).length || 0}
          </p>
        </div>
      </div>

      {/* Table */}
      <div className="card overflow-hidden p-0">
        {isLoading ? (
          <div className="p-8 text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading appointments...</p>
          </div>
        ) : appointments && appointments.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date & Time
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Patient
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Provider
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {appointments.map((appointment) => (
                  <tr key={appointment.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {formatDateTime(appointment.startTime)}
                      </div>
                      <div className="text-sm text-gray-500">
                        {formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {appointment.patient?.fullNameEn || 'Unknown'}
                      </div>
                      <div className="text-sm text-gray-500">{appointment.patient?.phoneNumber}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {appointment.provider?.fullName || 'Not assigned'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <StatusBadge status={appointment.status} />
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div className="flex items-center gap-2">
                        {appointment.status === AppointmentStatus.Booked && (
                          <button
                            onClick={() => confirmMutation.mutate(appointment.id)}
                            disabled={confirmMutation.isPending}
                            className="text-green-600 hover:text-green-900 disabled:opacity-50"
                          >
                            Confirm
                          </button>
                        )}
                        {appointment.status === AppointmentStatus.Confirmed && (
                          <button
                            onClick={() => checkInMutation.mutate(appointment.id)}
                            disabled={checkInMutation.isPending}
                            className="text-purple-600 hover:text-purple-900 disabled:opacity-50"
                          >
                            Check In
                          </button>
                        )}
                        {appointment.status === AppointmentStatus.CheckedIn && (
                          <button
                            onClick={() => completeMutation.mutate(appointment.id)}
                            disabled={completeMutation.isPending}
                            className="text-blue-600 hover:text-blue-900 disabled:opacity-50"
                          >
                            Complete
                          </button>
                        )}
                        {(appointment.status === AppointmentStatus.Booked ||
                          appointment.status === AppointmentStatus.Confirmed) && (
                          <button
                            onClick={() => cancelMutation.mutate({ id: appointment.id })}
                            disabled={cancelMutation.isPending}
                            className="text-red-600 hover:text-red-900 disabled:opacity-50"
                          >
                            Cancel
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="p-8 text-center">
            <svg
              className="mx-auto h-12 w-12 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">No appointments</h3>
            <p className="mt-1 text-sm text-gray-500">Get started by creating a new appointment.</p>
          </div>
        )}
      </div>
    </div>
  );
};
