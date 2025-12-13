import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import { api } from '../../lib/api';
import type { PortalAppointment, BookAppointmentRequest } from '../../types/portal';
import { useToast } from '../../components/ui/Toast';

const STATUS_LABELS = ['Booked', 'Confirmed', 'Checked In', 'Completed', 'Cancelled', 'No Show'];
const TYPE_LABELS = ['Consultation', 'Follow Up', 'Procedure', 'Emergency'];

export const PortalAppointments = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isBookModalOpen, setIsBookModalOpen] = useState(false);
  const [filterStatus, setFilterStatus] = useState<number | null>(null);
  const [bookingData, setBookingData] = useState<BookAppointmentRequest>({
    startTime: '',
    endTime: '',
    type: 0,
    notes: '',
    providerId: undefined,
  });

  // Fetch appointments
  const { data: appointments, isLoading } = useQuery<PortalAppointment[]>({
    queryKey: ['portal-appointments'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/appointments');
      return response.data;
    },
  });

  // Book appointment mutation
  const bookMutation = useMutation({
    mutationFn: async (data: BookAppointmentRequest) => {
      const response = await api.post('/api/Portal/appointments/book', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-appointments'] });
      queryClient.invalidateQueries({ queryKey: ['portal-statistics'] });
      setIsBookModalOpen(false);
      resetBookingForm();
      showToast('success', 'Appointment booked successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Booking failed: ${error.message}`);
    },
  });

  // Cancel appointment mutation
  const cancelMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.post(`/api/Portal/appointments/${id}/cancel`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-appointments'] });
      queryClient.invalidateQueries({ queryKey: ['portal-statistics'] });
      showToast('success', 'Appointment cancelled successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Cancellation failed: ${error.message}`);
    },
  });

  const resetBookingForm = () => {
    setBookingData({
      startTime: '',
      endTime: '',
      type: 0,
      notes: '',
      providerId: undefined,
    });
  };

  const handleBookSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!bookingData.startTime || !bookingData.endTime) {
      showToast('error', 'Please select appointment date and time');
      return;
    }

    bookMutation.mutate(bookingData);
  };

  const handleCancel = (appointment: PortalAppointment) => {
    if (confirm('Are you sure you want to cancel this appointment?')) {
      cancelMutation.mutate(appointment.id);
    }
  };

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'bg-blue-100 text-blue-800',
      1: 'bg-green-100 text-green-800',
      2: 'bg-purple-100 text-purple-800',
      3: 'bg-gray-100 text-gray-800',
      4: 'bg-red-100 text-red-800',
      5: 'bg-orange-100 text-orange-800',
    };
    return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800';
  };

  const canCancel = (appointment: PortalAppointment) => {
    // Can only cancel if not already cancelled, completed, or no-show
    return ![3, 4, 5].includes(appointment.status) && new Date(appointment.startTime) > new Date();
  };

  const filteredAppointments = appointments?.filter(
    apt => filterStatus === null || apt.status === filterStatus
  );

  // Sort appointments by start time (upcoming first)
  const sortedAppointments = filteredAppointments?.sort((a, b) => {
    return new Date(a.startTime).getTime() - new Date(b.startTime).getTime();
  });

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">My Appointments</h1>
            <div className="flex items-center gap-3">
              <a href="/portal/dashboard" className="text-blue-600 hover:text-blue-700 text-sm font-medium">
                Back to Dashboard
              </a>
              <button
                onClick={() => setIsBookModalOpen(true)}
                className="btn btn-primary"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Book Appointment
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Filter */}
        <div className="card mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">Filter by Status</label>
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => setFilterStatus(null)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                filterStatus === null
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              All ({appointments?.length || 0})
            </button>
            {STATUS_LABELS.map((label, index) => (
              <button
                key={index}
                onClick={() => setFilterStatus(index)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  filterStatus === index
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {label} ({appointments?.filter(a => a.status === index).length || 0})
              </button>
            ))}
          </div>
        </div>

        {/* Appointments List */}
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        ) : sortedAppointments && sortedAppointments.length > 0 ? (
          <div className="space-y-4">
            {sortedAppointments.map((appointment) => (
              <div key={appointment.id} className="card hover:shadow-lg transition-shadow">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">
                          {format(new Date(appointment.startTime), 'EEEE, MMMM d, yyyy')}
                        </h3>
                        <p className="text-gray-600">
                          {format(new Date(appointment.startTime), 'h:mm a')} - {format(new Date(appointment.endTime), 'h:mm a')}
                        </p>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className={`px-3 py-1 text-sm font-medium rounded-full ${getStatusColor(appointment.status)}`}>
                          {STATUS_LABELS[appointment.status]}
                        </span>
                        <span className="px-3 py-1 text-sm font-medium bg-gray-100 text-gray-800 rounded-full">
                          {TYPE_LABELS[appointment.type]}
                        </span>
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                      {appointment.provider && (
                        <div>
                          <span className="text-gray-600 font-medium">Provider:</span>
                          <p className="text-gray-900">{appointment.provider.fullName}</p>
                        </div>
                      )}

                      {appointment.branch && (
                        <div>
                          <span className="text-gray-600 font-medium">Location:</span>
                          <p className="text-gray-900">{appointment.branch.name}</p>
                        </div>
                      )}

                      {appointment.notes && (
                        <div className="md:col-span-2">
                          <span className="text-gray-600 font-medium">Notes:</span>
                          <p className="text-gray-900">{appointment.notes}</p>
                        </div>
                      )}
                    </div>

                    {canCancel(appointment) && (
                      <div className="mt-4 pt-4 border-t border-gray-200">
                        <button
                          onClick={() => handleCancel(appointment)}
                          disabled={cancelMutation.isPending}
                          className="btn bg-red-600 hover:bg-red-700 text-white text-sm disabled:opacity-50"
                        >
                          <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                          </svg>
                          Cancel Appointment
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="card text-center py-12">
            <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              {filterStatus !== null ? 'No appointments with this status' : 'No appointments yet'}
            </h3>
            <p className="text-gray-600 mb-4">
              {filterStatus !== null
                ? 'Try selecting a different status or book a new appointment'
                : 'Book your first appointment to get started'}
            </p>
            <button onClick={() => setIsBookModalOpen(true)} className="btn btn-primary">
              Book Your First Appointment
            </button>
          </div>
        )}
      </div>

      {/* Book Appointment Modal */}
      <Dialog
        open={isBookModalOpen}
        onClose={() => {
          setIsBookModalOpen(false);
          resetBookingForm();
        }}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />

        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-lg w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-xl font-semibold text-gray-900 mb-4">
                Book Appointment
              </Dialog.Title>

              <form onSubmit={handleBookSubmit} className="space-y-4">
                {/* Appointment Type */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Appointment Type *
                  </label>
                  <select
                    value={bookingData.type}
                    onChange={(e) => setBookingData({ ...bookingData, type: parseInt(e.target.value) })}
                    className="input w-full"
                    required
                  >
                    {TYPE_LABELS.map((label, index) => (
                      <option key={index} value={index}>
                        {label}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Start Time */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Date and Start Time *
                  </label>
                  <input
                    type="datetime-local"
                    value={bookingData.startTime}
                    onChange={(e) => setBookingData({ ...bookingData, startTime: e.target.value })}
                    className="input w-full"
                    required
                    min={new Date().toISOString().slice(0, 16)}
                  />
                </div>

                {/* End Time */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    End Time *
                  </label>
                  <input
                    type="datetime-local"
                    value={bookingData.endTime}
                    onChange={(e) => setBookingData({ ...bookingData, endTime: e.target.value })}
                    className="input w-full"
                    required
                    min={bookingData.startTime || new Date().toISOString().slice(0, 16)}
                  />
                </div>

                {/* Notes */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Notes (Optional)
                  </label>
                  <textarea
                    value={bookingData.notes}
                    onChange={(e) => setBookingData({ ...bookingData, notes: e.target.value })}
                    className="input w-full"
                    rows={3}
                    placeholder="Add any notes or special requirements..."
                  />
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <p className="text-sm text-blue-800">
                    <strong>Note:</strong> Your appointment request will be reviewed and confirmed by our staff.
                    You will receive a confirmation email once approved.
                  </p>
                </div>

                {/* Actions */}
                <div className="flex justify-end gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setIsBookModalOpen(false);
                      resetBookingForm();
                    }}
                    className="btn btn-secondary"
                    disabled={bookMutation.isPending}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={bookMutation.isPending}
                  >
                    {bookMutation.isPending ? (
                      <div className="flex items-center">
                        <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                        Booking...
                      </div>
                    ) : (
                      'Book Appointment'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
