import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { format } from 'date-fns';
import { api } from '../../lib/api';
import type { PortalUser, PortalStatistics, PortalAppointment, PortalDocument } from '../../types/portal';

export const PortalDashboard = () => {
  // Fetch user info
  const { data: user } = useQuery<PortalUser>({
    queryKey: ['portal-user'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/user');
      return response.data;
    },
  });

  // Fetch statistics
  const { data: stats } = useQuery<PortalStatistics>({
    queryKey: ['portal-statistics'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/statistics');
      return response.data;
    },
  });

  // Fetch upcoming appointments
  const { data: upcomingAppointments } = useQuery<PortalAppointment[]>({
    queryKey: ['portal-upcoming-appointments'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/appointments/upcoming');
      return response.data;
    },
  });

  // Fetch recent documents
  const { data: recentDocuments } = useQuery<PortalDocument[]>({
    queryKey: ['portal-recent-documents'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/documents/recent');
      return response.data;
    },
  });

  const getStatusColor = (status: number) => {
    const colors = {
      0: 'bg-blue-100 text-blue-800',    // Booked
      1: 'bg-green-100 text-green-800',  // Confirmed
      2: 'bg-purple-100 text-purple-800', // CheckedIn
      3: 'bg-gray-100 text-gray-800',    // Completed
      4: 'bg-red-100 text-red-800',      // Cancelled
      5: 'bg-orange-100 text-orange-800', // NoShow
    };
    return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800';
  };

  const getStatusLabel = (status: number) => {
    const labels = ['Booked', 'Confirmed', 'Checked In', 'Completed', 'Cancelled', 'No Show'];
    return labels[status] || 'Unknown';
  };

  const getDocumentTypeLabel = (type: number) => {
    const labels = ['Medical Record', 'Lab Result', 'Prescription', 'Imaging Report', 'Insurance', 'Other'];
    return labels[type] || 'Document';
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const profileCompletionPercentage = user?.isProfileComplete ? 100 : 60;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              <div>
                <h1 className="text-xl font-bold text-gray-900">Patient Portal</h1>
                <p className="text-sm text-gray-600">XenonClinic</p>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <Link to="/portal/profile" className="btn btn-secondary text-sm">
                My Profile
              </Link>
              <button className="text-gray-600 hover:text-gray-900">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
        {/* Welcome Section */}
        <div className="animate-fade-in">
          <h2 className="text-2xl font-bold text-gray-900">
            Welcome back, {user?.patient?.fullNameEn || 'Patient'}!
          </h2>
          <p className="text-gray-600 mt-1">Here's your health dashboard overview</p>
        </div>

        {/* Profile Completion */}
        {!user?.isProfileComplete && (
          <div className="card bg-blue-50 border border-blue-200 animate-fade-in">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <h3 className="font-semibold text-blue-900 mb-2">Complete Your Profile</h3>
                <p className="text-sm text-blue-700 mb-3">
                  Add more information to help us provide better care
                </p>
                <div className="w-full bg-blue-200 rounded-full h-2 mb-3">
                  <div
                    className="bg-blue-600 h-2 rounded-full transition-all"
                    style={{ width: `${profileCompletionPercentage}%` }}
                  ></div>
                </div>
                <p className="text-xs text-blue-600 font-medium">{profileCompletionPercentage}% Complete</p>
              </div>
              <Link to="/portal/profile" className="btn btn-primary ml-4">
                Complete Profile
              </Link>
            </div>
          </div>
        )}

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="card bg-gradient-to-br from-blue-50 to-blue-100 animate-fade-in">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-blue-900">Total Appointments</p>
                <p className="text-3xl font-bold text-blue-900 mt-2">{stats?.totalAppointments || 0}</p>
              </div>
              <div className="w-12 h-12 bg-blue-200 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-blue-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="card bg-gradient-to-br from-green-50 to-green-100 animate-fade-in" style={{ animationDelay: '50ms' }}>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-green-900">Upcoming</p>
                <p className="text-3xl font-bold text-green-900 mt-2">{stats?.upcomingAppointments || 0}</p>
              </div>
              <div className="w-12 h-12 bg-green-200 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-green-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="card bg-gradient-to-br from-purple-50 to-purple-100 animate-fade-in" style={{ animationDelay: '100ms' }}>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-purple-900">Documents</p>
                <p className="text-3xl font-bold text-purple-900 mt-2">{stats?.totalDocuments || 0}</p>
              </div>
              <div className="w-12 h-12 bg-purple-200 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-purple-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
            </div>
          </div>

          <div className="card bg-gradient-to-br from-orange-50 to-orange-100 animate-fade-in" style={{ animationDelay: '150ms' }}>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-orange-900">Member Since</p>
                <p className="text-lg font-bold text-orange-900 mt-2">
                  {stats?.memberSince ? format(new Date(stats.memberSince), 'MMM yyyy') : 'N/A'}
                </p>
              </div>
              <div className="w-12 h-12 bg-orange-200 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-orange-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        {/* Main Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Upcoming Appointments */}
          <div className="card animate-fade-in">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Upcoming Appointments</h3>
              <Link to="/portal/appointments" className="text-sm text-blue-600 hover:text-blue-700 font-medium">
                View All
              </Link>
            </div>

            {upcomingAppointments && upcomingAppointments.length > 0 ? (
              <div className="space-y-3">
                {upcomingAppointments.slice(0, 3).map((appointment) => (
                  <div key={appointment.id} className="p-4 bg-gray-50 rounded-lg border border-gray-200">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <p className="font-medium text-gray-900">
                          {format(new Date(appointment.startTime), 'EEEE, MMM d, yyyy')}
                        </p>
                        <p className="text-sm text-gray-600">
                          {format(new Date(appointment.startTime), 'h:mm a')} - {format(new Date(appointment.endTime), 'h:mm a')}
                        </p>
                      </div>
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(appointment.status)}`}>
                        {getStatusLabel(appointment.status)}
                      </span>
                    </div>
                    {appointment.provider && (
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Provider:</span> {appointment.provider.fullName}
                      </p>
                    )}
                    {appointment.branch && (
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Location:</span> {appointment.branch.name}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8">
                <svg className="w-12 h-12 text-gray-400 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <p className="text-gray-600 mb-4">No upcoming appointments</p>
                <Link to="/portal/appointments" className="btn btn-primary">
                  Book Appointment
                </Link>
              </div>
            )}
          </div>

          {/* Recent Documents */}
          <div className="card animate-fade-in">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Recent Documents</h3>
              <Link to="/portal/documents" className="text-sm text-blue-600 hover:text-blue-700 font-medium">
                View All
              </Link>
            </div>

            {recentDocuments && recentDocuments.length > 0 ? (
              <div className="space-y-3">
                {recentDocuments.slice(0, 3).map((document) => (
                  <div key={document.id} className="p-4 bg-gray-50 rounded-lg border border-gray-200">
                    <div className="flex items-start justify-between">
                      <div className="flex items-start gap-3 flex-1">
                        <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                          <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                          </svg>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-900 truncate">{document.name}</p>
                          <p className="text-xs text-gray-600">{getDocumentTypeLabel(document.type)}</p>
                          <p className="text-xs text-gray-500 mt-1">
                            {format(new Date(document.uploadedAt), 'MMM d, yyyy')} • {formatFileSize(document.size)}
                          </p>
                        </div>
                      </div>
                      <a
                        href={document.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 hover:text-blue-700 ml-2"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                        </svg>
                      </a>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8">
                <svg className="w-12 h-12 text-gray-400 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <p className="text-gray-600 mb-4">No documents yet</p>
                <Link to="/portal/documents" className="btn btn-primary">
                  Upload Document
                </Link>
              </div>
            )}
          </div>
        </div>

        {/* Quick Actions */}
        <div className="card animate-fade-in">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Link
              to="/portal/appointments"
              className="p-4 border-2 border-gray-200 rounded-lg hover:border-blue-500 hover:bg-blue-50 transition-all"
            >
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                  <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <div>
                  <p className="font-semibold text-gray-900">Book Appointment</p>
                  <p className="text-sm text-gray-600">Schedule a visit</p>
                </div>
              </div>
            </Link>

            <Link
              to="/portal/documents"
              className="p-4 border-2 border-gray-200 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all"
            >
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
                  <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                  </svg>
                </div>
                <div>
                  <p className="font-semibold text-gray-900">Upload Document</p>
                  <p className="text-sm text-gray-600">Share medical records</p>
                </div>
              </div>
            </Link>

            <Link
              to="/portal/profile"
              className="p-4 border-2 border-gray-200 rounded-lg hover:border-green-500 hover:bg-green-50 transition-all"
            >
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                  <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </div>
                <div>
                  <p className="font-semibold text-gray-900">View Records</p>
                  <p className="text-sm text-gray-600">Access health info</p>
                </div>
              </div>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};
