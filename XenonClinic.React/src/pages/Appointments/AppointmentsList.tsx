import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { appointmentsApi } from '../../lib/api';
import type { Appointment } from '../../types/appointment';
import { AppointmentStatus } from '../../types/appointment';
import { StatusBadge } from '../../components/ui/StatusBadge';
import { Modal } from '../../components/ui/Modal';
import { AppointmentForm } from '../../components/AppointmentForm';
import { Calendar } from '../../components/Calendar';
import { format } from 'date-fns';
import { SkeletonTable } from '../../components/ui/LoadingSkeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog, useConfirmDialog } from '../../components/ui/ConfirmDialog';
import { useToast } from '../../components/ui/Toast';
import { useKeyboardShortcuts, createCommonShortcuts } from '../../hooks/useKeyboardShortcuts';
import { exportToCSV, exportToExcel, printTable, formatters, type ExportColumn } from '../../utils/export';

type ViewMode = 'list' | 'calendar';

export const AppointmentsList = () => {
  const [selectedDate, setSelectedDate] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | undefined>(undefined);
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [showExportMenu, setShowExportMenu] = useState(false);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { dialogState, showConfirm, hideConfirm, handleConfirm } = useConfirmDialog();

  // Fetch appointments
  const { data: appointments, isLoading, error, refetch } = useQuery<Appointment[]>({
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

  // Keyboard shortcuts
  useKeyboardShortcuts(
    createCommonShortcuts({
      onNew: handleOpenCreateModal,
      onRefresh: () => refetch(),
      onClose: () => {
        if (isModalOpen) handleCloseModal();
      },
    })
  );

  // Confirm mutation
  const confirmMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.confirm(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      showToast('success', 'Appointment confirmed successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to confirm appointment: ${error.message}`);
    },
  });

  // Cancel mutation
  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      appointmentsApi.cancel(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      showToast('success', 'Appointment cancelled');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to cancel appointment: ${error.message}`);
    },
  });

  // Check-in mutation
  const checkInMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.checkIn(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      showToast('success', 'Patient checked in successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to check in patient: ${error.message}`);
    },
  });

  // Complete mutation
  const completeMutation = useMutation({
    mutationFn: (id: number) => appointmentsApi.complete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      showToast('success', 'Appointment completed');
    },
    onError: (error: Error) => {
      showToast('error', `Failed to complete appointment: ${error.message}`);
    },
  });

  // Modal handlers
  const handleOpenCreateModal = () => {
    setSelectedAppointment(undefined);
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(undefined);
  };

  const handleFormSuccess = () => {
    handleCloseModal();
    showToast('success', selectedAppointment ? 'Appointment updated successfully' : 'Appointment created successfully');
  };

  const handleCancelWithConfirm = (appointment: Appointment) => {
    showConfirm(
      'Cancel Appointment',
      `Are you sure you want to cancel the appointment for "${appointment.patient?.fullNameEn}"?`,
      () => cancelMutation.mutate({ id: appointment.id }),
      'warning'
    );
  };

  // Export handlers
  const exportColumns: ExportColumn[] = [
    { key: 'startTime', label: 'Date & Time', format: formatters.datetime },
    { key: 'patientName', label: 'Patient' },
    { key: 'providerName', label: 'Provider' },
    { key: 'status', label: 'Status' },
    { key: 'appointmentType', label: 'Type' },
  ];

  const prepareExportData = () => {
    return (appointments || []).map((a) => ({
      ...a,
      patientName: a.patient?.fullNameEn || 'Unknown',
      providerName: a.provider?.fullName || 'Not assigned',
    }));
  };

  const handleExportCSV = () => {
    exportToCSV(prepareExportData(), exportColumns, 'appointments.csv');
    setShowExportMenu(false);
    showToast('success', 'Appointments exported to CSV');
  };

  const handleExportExcel = () => {
    exportToExcel(prepareExportData(), exportColumns, 'appointments.xls');
    setShowExportMenu(false);
    showToast('success', 'Appointments exported to Excel');
  };

  const handlePrint = () => {
    printTable(prepareExportData(), exportColumns, 'Appointments List');
    setShowExportMenu(false);
  };

  const handleAppointmentClick = (appointment: Appointment) => {
    handleOpenEditModal(appointment);
  };

  const handleDateClick = (date: Date) => {
    // When clicking a date in calendar, switch to list view filtered by that date
    setSelectedDate(format(date, 'yyyy-MM-dd'));
    setViewMode('list');
  };

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
      <div className="p-4 bg-red-50 border border-red-200 rounded-md animate-fade-in">
        <p className="text-sm text-red-600">Error loading appointments: {(error as Error).message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between animate-fade-in">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Appointments</h1>
          <p className="text-gray-600 mt-1">Manage patient appointments and schedules</p>
        </div>
        <div className="flex items-center gap-3">
          {/* Export Button */}
          <div className="relative">
            <button
              onClick={() => setShowExportMenu(!showExportMenu)}
              className="btn btn-secondary"
              disabled={!appointments || appointments.length === 0}
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              Export
            </button>
            {showExportMenu && (
              <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-10 border border-gray-200 animate-scale-in">
                <div className="py-1">
                  <button
                    onClick={handleExportCSV}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Export as CSV
                  </button>
                  <button
                    onClick={handleExportExcel}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Export as Excel
                  </button>
                  <button
                    onClick={handlePrint}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Print
                  </button>
                </div>
              </div>
            )}
          </div>
          {/* View Toggle */}
          <div className="flex bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode('list')}
              className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                viewMode === 'list'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              <div className="flex items-center">
                <svg className="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
                </svg>
                List
              </div>
            </button>
            <button
              onClick={() => setViewMode('calendar')}
              className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                viewMode === 'calendar'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              <div className="flex items-center">
                <svg className="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                Calendar
              </div>
            </button>
          </div>
          <button onClick={handleOpenCreateModal} className="btn btn-primary">
            <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            New Appointment
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="card animate-fade-in">
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
        {[
          { label: 'Total', value: appointments?.length || 0, color: 'text-gray-900' },
          { label: 'Confirmed', value: appointments?.filter((a) => a.status === AppointmentStatus.Confirmed).length || 0, color: 'text-green-600' },
          { label: 'Checked In', value: appointments?.filter((a) => a.status === AppointmentStatus.CheckedIn).length || 0, color: 'text-purple-600' },
          { label: 'Completed', value: appointments?.filter((a) => a.status === AppointmentStatus.Completed).length || 0, color: 'text-gray-600' },
        ].map((stat, index) => (
          <div key={stat.label} className="card animate-fade-in" style={{ animationDelay: `${index * 50}ms` }}>
            <p className="text-sm text-gray-600">{stat.label}</p>
            <p className={`text-2xl font-bold ${stat.color} mt-1`}>{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Calendar or Table View */}
      {viewMode === 'calendar' ? (
        <Calendar
          appointments={appointments || []}
          onAppointmentClick={handleAppointmentClick}
          onDateClick={handleDateClick}
        />
      ) : (
        <div className="card overflow-hidden p-0 animate-fade-in">
          {isLoading ? (
            <div className="p-6">
              <SkeletonTable rows={8} columns={5} />
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
                  <tr key={appointment.id} className="hover:bg-gray-50 transition-colors">
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
                        {/* Edit button - available for non-completed/cancelled appointments */}
                        {(appointment.status !== AppointmentStatus.Completed &&
                          appointment.status !== AppointmentStatus.Cancelled &&
                          appointment.status !== AppointmentStatus.NoShow) && (
                          <button
                            onClick={() => handleOpenEditModal(appointment)}
                            className="text-gray-600 hover:text-gray-900"
                            title="Edit appointment"
                          >
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                              />
                            </svg>
                          </button>
                        )}
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
                            onClick={() => handleCancelWithConfirm(appointment)}
                            disabled={cancelMutation.isPending}
                            className="text-red-600 hover:text-red-900 disabled:opacity-50 transition-colors"
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
          <EmptyState
            icon={
              <svg className="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            }
            title={selectedDate ? 'No appointments on this date' : 'No appointments yet'}
            description={selectedDate ? 'Try selecting a different date' : 'Get started by scheduling your first appointment'}
            action={
              selectedDate
                ? undefined
                : {
                    label: 'Create Appointment',
                    onClick: handleOpenCreateModal,
                  }
            }
          />
        )}
        </div>
      )}

      {/* Keyboard Shortcuts Help */}
      <div className="text-xs text-gray-500 animate-fade-in">
        <strong>Keyboard shortcuts:</strong> <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Ctrl+N</kbd> New,{' '}
        <kbd className="px-1.5 py-0.5 bg-gray-100 border border-gray-300 rounded">Ctrl+R</kbd> Refresh
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={selectedAppointment ? 'Edit Appointment' : 'New Appointment'}
        size="lg"
      >
        <AppointmentForm
          appointment={selectedAppointment}
          onSuccess={handleFormSuccess}
          onCancel={handleCloseModal}
        />
      </Modal>

      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={dialogState.isOpen}
        onClose={hideConfirm}
        onConfirm={handleConfirm}
        title={dialogState.title}
        message={dialogState.message}
        variant={dialogState.variant}
        isLoading={cancelMutation.isPending}
      />
    </div>
  );
};
