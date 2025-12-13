import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { api } from '../../lib/api';
import type { PortalUser, PortalProfileUpdate } from '../../types/portal';
import { useToast } from '../../components/ui/Toast';

export const PortalProfile = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isEditing, setIsEditing] = useState(false);

  // Fetch user data
  const { data: user, isLoading } = useQuery<PortalUser>({
    queryKey: ['portal-user'],
    queryFn: async () => {
      const response = await api.get('/api/Portal/user');
      return response.data;
    },
  });

  const [formData, setFormData] = useState<PortalProfileUpdate>({
    fullNameEn: user?.patient?.fullNameEn || '',
    fullNameAr: user?.patient?.fullNameAr || '',
    phoneNumber: user?.patient?.phoneNumber || '',
    email: user?.patient?.email || '',
    dateOfBirth: user?.patient?.dateOfBirth || '',
    gender: user?.patient?.gender || 'M',
    medicalHistory: '',
    allergies: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: async (data: PortalProfileUpdate) => {
      const response = await api.put('/api/Portal/profile', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-user'] });
      setIsEditing(false);
      showToast('success', 'Profile updated successfully');
    },
    onError: (error: Error) => {
      showToast('error', `Update failed: ${error.message}`);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate(formData);
  };

  const handleCancel = () => {
    setIsEditing(false);
    if (user?.patient) {
      setFormData({
        fullNameEn: user.patient.fullNameEn || '',
        fullNameAr: user.patient.fullNameAr || '',
        phoneNumber: user.patient.phoneNumber || '',
        email: user.patient.email || '',
        dateOfBirth: user.patient.dateOfBirth || '',
        gender: user.patient.gender || 'M',
        medicalHistory: '',
        allergies: '',
        emergencyContactName: '',
        emergencyContactPhone: '',
      });
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">My Profile</h1>
            <a href="/portal/dashboard" className="text-blue-600 hover:text-blue-700 text-sm font-medium">
              Back to Dashboard
            </a>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Personal Information */}
          <div className="card">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900">Personal Information</h2>
              {!isEditing ? (
                <button
                  type="button"
                  onClick={() => setIsEditing(true)}
                  className="btn btn-secondary text-sm"
                >
                  Edit Profile
                </button>
              ) : (
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={handleCancel}
                    className="btn btn-secondary text-sm"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={updateMutation.isPending}
                    className="btn btn-primary text-sm"
                  >
                    {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Full Name (English) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Full Name (English)
                </label>
                {isEditing ? (
                  <input
                    type="text"
                    value={formData.fullNameEn}
                    onChange={(e) => setFormData({ ...formData, fullNameEn: e.target.value })}
                    className="input w-full"
                    required
                  />
                ) : (
                  <p className="text-gray-900">{user?.patient?.fullNameEn || '-'}</p>
                )}
              </div>

              {/* Full Name (Arabic) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Full Name (Arabic)
                </label>
                {isEditing ? (
                  <input
                    type="text"
                    value={formData.fullNameAr}
                    onChange={(e) => setFormData({ ...formData, fullNameAr: e.target.value })}
                    className="input w-full"
                    dir="rtl"
                  />
                ) : (
                  <p className="text-gray-900" dir="rtl">{user?.patient?.fullNameAr || '-'}</p>
                )}
              </div>

              {/* Email */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email Address
                </label>
                {isEditing ? (
                  <input
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    className="input w-full"
                    required
                  />
                ) : (
                  <p className="text-gray-900">{user?.email || '-'}</p>
                )}
              </div>

              {/* Phone */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Phone Number
                </label>
                {isEditing ? (
                  <input
                    type="tel"
                    value={formData.phoneNumber}
                    onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                    className="input w-full"
                    required
                  />
                ) : (
                  <p className="text-gray-900">{user?.patient?.phoneNumber || '-'}</p>
                )}
              </div>

              {/* Date of Birth */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Date of Birth
                </label>
                {isEditing ? (
                  <input
                    type="date"
                    value={formData.dateOfBirth ? format(new Date(formData.dateOfBirth), 'yyyy-MM-dd') : ''}
                    onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                    className="input w-full"
                    required
                  />
                ) : (
                  <p className="text-gray-900">
                    {user?.patient?.dateOfBirth ? format(new Date(user.patient.dateOfBirth), 'MMM d, yyyy') : '-'}
                  </p>
                )}
              </div>

              {/* Gender */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Gender
                </label>
                {isEditing ? (
                  <select
                    value={formData.gender}
                    onChange={(e) => setFormData({ ...formData, gender: e.target.value })}
                    className="input w-full"
                    required
                  >
                    <option value="M">Male</option>
                    <option value="F">Female</option>
                  </select>
                ) : (
                  <p className="text-gray-900">{user?.patient?.gender === 'M' ? 'Male' : 'Female'}</p>
                )}
              </div>
            </div>
          </div>

          {/* Medical History */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-6">Medical History</h2>

            <div className="space-y-6">
              {/* Medical History */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Medical History
                </label>
                {isEditing ? (
                  <textarea
                    value={formData.medicalHistory}
                    onChange={(e) => setFormData({ ...formData, medicalHistory: e.target.value })}
                    className="input w-full"
                    rows={4}
                    placeholder="List any past medical conditions, surgeries, or treatments..."
                  />
                ) : (
                  <p className="text-gray-900 whitespace-pre-wrap">{formData.medicalHistory || 'No medical history recorded'}</p>
                )}
              </div>

              {/* Allergies */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Allergies
                </label>
                {isEditing ? (
                  <textarea
                    value={formData.allergies}
                    onChange={(e) => setFormData({ ...formData, allergies: e.target.value })}
                    className="input w-full"
                    rows={3}
                    placeholder="List any known allergies to medications, foods, or other substances..."
                  />
                ) : (
                  <p className="text-gray-900 whitespace-pre-wrap">{formData.allergies || 'No allergies recorded'}</p>
                )}
              </div>
            </div>
          </div>

          {/* Emergency Contacts */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-6">Emergency Contact</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Emergency Contact Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Contact Name
                </label>
                {isEditing ? (
                  <input
                    type="text"
                    value={formData.emergencyContactName}
                    onChange={(e) => setFormData({ ...formData, emergencyContactName: e.target.value })}
                    className="input w-full"
                    placeholder="Full name of emergency contact"
                  />
                ) : (
                  <p className="text-gray-900">{formData.emergencyContactName || 'Not specified'}</p>
                )}
              </div>

              {/* Emergency Contact Phone */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Contact Phone
                </label>
                {isEditing ? (
                  <input
                    type="tel"
                    value={formData.emergencyContactPhone}
                    onChange={(e) => setFormData({ ...formData, emergencyContactPhone: e.target.value })}
                    className="input w-full"
                    placeholder="Emergency contact phone number"
                  />
                ) : (
                  <p className="text-gray-900">{formData.emergencyContactPhone || 'Not specified'}</p>
                )}
              </div>
            </div>
          </div>

          {/* Account Information */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-6">Account Information</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Account Status
                </label>
                <div className="flex items-center gap-2">
                  <span className={`px-3 py-1 text-sm font-medium rounded-full ${
                    user?.isVerified ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
                  }`}>
                    {user?.isVerified ? 'Verified' : 'Pending Verification'}
                  </span>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Registered Since
                </label>
                <p className="text-gray-900">
                  {user?.registeredAt ? format(new Date(user.registeredAt), 'MMM d, yyyy') : '-'}
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last Login
                </label>
                <p className="text-gray-900">
                  {user?.lastLoginAt ? format(new Date(user.lastLoginAt), 'MMM d, yyyy h:mm a') : 'Never'}
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Patient ID
                </label>
                <p className="text-gray-900 font-mono">#{user?.patientId || '-'}</p>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};
