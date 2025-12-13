import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  UserGroupIcon,
  ClipboardDocumentListIcon,
  ClockIcon,
  CalendarDaysIcon,
  DocumentChartBarIcon,
  BeakerIcon,
  PlusIcon,
} from '@heroicons/react/24/outline';
import type { DentalStatistics, DentalTreatment } from '../../types/dental';
import { format } from 'date-fns';

// Mock API functions - Replace with actual API calls
const dentalApi = {
  getStatistics: async () => ({
    data: {
      totalPatients: 245,
      newPatientsThisMonth: 18,
      treatmentsToday: 12,
      treatmentsThisWeek: 47,
      treatmentsThisMonth: 186,
      pendingTreatments: 23,
      completedTreatments: 163,
      monthlyRevenue: 125400,
      outstandingPayments: 8500,
      chartsThisMonth: 34,
      periodontalExamsThisMonth: 15,
      treatmentTypeDistribution: {
        cleaning: 45,
        filling: 38,
        extraction: 12,
        crown: 18,
        root_canal: 9,
      },
      statusDistribution: {
        planned: 23,
        in_progress: 8,
        completed: 163,
      },
    } as DentalStatistics,
  }),
  getRecentTreatments: async () => ({
    data: [] as DentalTreatment[],
  }),
};

export const DentalDashboard = () => {
  const [dateRange, setDateRange] = useState('week');

  // Fetch statistics
  const { data: statsData, isLoading: statsLoading } = useQuery({
    queryKey: ['dental-stats'],
    queryFn: () => dentalApi.getStatistics(),
  });

  // Fetch recent treatments
  const { data: treatmentsData, isLoading: treatmentsLoading } = useQuery({
    queryKey: ['recent-treatments'],
    queryFn: () => dentalApi.getRecentTreatments(),
  });

  const stats = statsData?.data;
  const recentTreatments = treatmentsData?.data || [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dental Dashboard</h1>
          <p className="text-gray-600">Overview of dental practice metrics</p>
        </div>
        <div className="flex gap-2">
          <button className="inline-flex items-center px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50">
            <DocumentChartBarIcon className="h-5 w-5 mr-2" />
            New Chart
          </button>
          <button className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700">
            <PlusIcon className="h-5 w-5 mr-2" />
            New Treatment
          </button>
        </div>
      </div>

      {/* Statistics Cards */}
      {statsLoading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
          <p className="text-gray-600 mt-2">Loading statistics...</p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-blue-100 rounded-lg">
                  <UserGroupIcon className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Total Patients</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.totalPatients || 0}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    +{stats?.newPatientsThisMonth || 0} this month
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-green-100 rounded-lg">
                  <ClipboardDocumentListIcon className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Treatments Today</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.treatmentsToday || 0}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    {stats?.treatmentsThisWeek || 0} this week
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-yellow-100 rounded-lg">
                  <ClockIcon className="h-6 w-6 text-yellow-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Pending Treatments</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.pendingTreatments || 0}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    {stats?.completedTreatments || 0} completed
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-purple-100 rounded-lg">
                  <CalendarDaysIcon className="h-6 w-6 text-purple-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Monthly Revenue</p>
                  <p className="text-2xl font-bold text-gray-900">
                    AED {stats?.monthlyRevenue?.toLocaleString() || 0}
                  </p>
                  <p className="text-xs text-red-500 mt-1">
                    AED {stats?.outstandingPayments?.toLocaleString() || 0} outstanding
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Additional Stats Row */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Dental Charts This Month</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.chartsThisMonth || 0}</p>
                </div>
                <DocumentChartBarIcon className="h-8 w-8 text-blue-500" />
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Periodontal Exams</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.periodontalExamsThisMonth || 0}</p>
                </div>
                <BeakerIcon className="h-8 w-8 text-green-500" />
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Treatments This Month</p>
                  <p className="text-2xl font-bold text-gray-900">{stats?.treatmentsThisMonth || 0}</p>
                </div>
                <ClipboardDocumentListIcon className="h-8 w-8 text-purple-500" />
              </div>
            </div>
          </div>
        </>
      )}

      {/* Recent Treatments Table */}
      <div className="bg-white rounded-lg shadow">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">Recent Treatments</h2>
        </div>
        <div className="overflow-x-auto">
          {treatmentsLoading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
              <p className="text-gray-600 mt-2">Loading treatments...</p>
            </div>
          ) : recentTreatments.length > 0 ? (
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Patient
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Treatment
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tooth
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Cost
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {recentTreatments.map((treatment) => (
                  <tr key={treatment.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {treatment.patientName || `Patient #${treatment.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {treatment.treatmentType}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {treatment.toothNumber || '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {treatment.performedDate
                        ? format(new Date(treatment.performedDate), 'MMM d, yyyy')
                        : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                          treatment.status === 'completed'
                            ? 'bg-green-100 text-green-800'
                            : treatment.status === 'in_progress'
                            ? 'bg-yellow-100 text-yellow-800'
                            : treatment.status === 'planned'
                            ? 'bg-blue-100 text-blue-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}
                      >
                        {treatment.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {treatment.cost ? `AED ${treatment.cost.toFixed(2)}` : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-primary-600 hover:text-primary-900 mr-3">
                        View
                      </button>
                      <button className="text-green-600 hover:text-green-900">Edit</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="text-center py-12 text-gray-500">
              No recent treatments found
            </div>
          )}
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button className="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors">
            <DocumentChartBarIcon className="h-8 w-8 text-gray-400 mx-auto mb-2" />
            <p className="text-sm font-medium text-gray-700">Create Dental Chart</p>
          </button>
          <button className="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors">
            <ClipboardDocumentListIcon className="h-8 w-8 text-gray-400 mx-auto mb-2" />
            <p className="text-sm font-medium text-gray-700">Schedule Treatment</p>
          </button>
          <button className="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors">
            <BeakerIcon className="h-8 w-8 text-gray-400 mx-auto mb-2" />
            <p className="text-sm font-medium text-gray-700">Periodontal Exam</p>
          </button>
        </div>
      </div>
    </div>
  );
};
