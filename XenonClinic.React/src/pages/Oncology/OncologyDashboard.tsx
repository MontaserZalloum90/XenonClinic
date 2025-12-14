import { useQuery } from '@tanstack/react-query';
import {
  UserGroupIcon,
  BeakerIcon,
  HeartIcon,
  ChartBarIcon,
  ClipboardDocumentCheckIcon,
  ExclamationTriangleIcon,
  ArrowTrendingUpIcon,
  DocumentTextIcon,
} from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import type { OncologyStatistics } from '../../types/oncology';
import { oncologyApi } from '../../lib/api';

interface RecentActivity {
  id: number;
  patientName: string;
  activity: string;
  date: string;
  status: string;
  performedBy: string;
}

export const OncologyDashboard = () => {
  const { data: stats } = useQuery<OncologyStatistics>({
    queryKey: ['oncology-stats'],
    queryFn: async () => {
      const response = await oncologyApi.getStatistics();
      return response.data?.data ?? response.data ?? {
        totalPatients: 0,
        activePatients: 0,
        newPatientsThisMonth: 0,
        newDiagnosesThisMonth: 0,
        activeTreatments: 0,
        activeChemoProtocols: 0,
        activeRadiationTreatments: 0,
        completedTreatmentsThisMonth: 0,
        chemoAdministrationsToday: 0,
        chemoAdministrationsThisWeek: 0,
        radiationSessionsThisWeek: 0,
        tumorMarkersThisMonth: 0,
        abnormalMarkers: 0,
        remissionRate: 0,
        adverseReactionRate: 0,
      };
    },
  });

  const { data: recentActivities } = useQuery<RecentActivity[]>({
    queryKey: ['oncology-recent-activities'],
    queryFn: async () => {
      const response = await oncologyApi.getStatistics();
      return response.data?.recentActivities ?? [];
    },
  });

  const statistics = stats || {
    totalPatients: 0,
    activeTreatments: 0,
    newDiagnosesThisMonth: 0,
    activePatients: 0,
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Completed: 'text-green-600 bg-green-100',
      'In Progress': 'text-yellow-600 bg-yellow-100',
      Scheduled: 'text-blue-600 bg-blue-100',
      Pending: 'text-orange-600 bg-orange-100',
      Cancelled: 'text-red-600 bg-red-100',
    };
    return colors[status] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Oncology Dashboard</h1>
          <p className="text-gray-600 mt-1">
            Cancer care and treatment management overview
          </p>
        </div>
        <div className="flex gap-2">
          <button className="btn btn-outline">
            <ChartBarIcon className="h-5 w-5 mr-2" />
            View Reports
          </button>
          <button className="btn btn-primary">
            <DocumentTextIcon className="h-5 w-5 mr-2" />
            New Record
          </button>
        </div>
      </div>

      {/* Primary Statistics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <UserGroupIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Patients</p>
              <p className="text-2xl font-bold text-gray-900">{statistics.totalPatients}</p>
              {statistics.activePatients && (
                <p className="text-xs text-green-600 mt-1">
                  {statistics.activePatients} active
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-purple-100 rounded-lg">
              <HeartIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Active Treatments</p>
              <p className="text-2xl font-bold text-gray-900">{statistics.activeTreatments}</p>
              {statistics.activeChemoProtocols && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.activeChemoProtocols} chemo protocols
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-red-100 rounded-lg">
              <ClipboardDocumentCheckIcon className="h-6 w-6 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">New Diagnoses</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.newDiagnosesThisMonth}
              </p>
              <p className="text-xs text-gray-600 mt-1">This month</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg">
              <BeakerIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Tumor Markers</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.tumorMarkersThisMonth || 0}
              </p>
              {statistics.abnormalMarkers && statistics.abnormalMarkers > 0 && (
                <p className="text-xs text-red-600 mt-1">
                  {statistics.abnormalMarkers} abnormal
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Treatment Activity Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Chemo Today</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.chemoAdministrationsToday || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">
                {statistics.chemoAdministrationsThisWeek || 0} this week
              </p>
            </div>
            <BeakerIcon className="h-8 w-8 text-purple-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Radiation Sessions</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.radiationSessionsThisWeek || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">This week</p>
            </div>
            <ArrowTrendingUpIcon className="h-8 w-8 text-blue-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Remission Rate</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.remissionRate?.toFixed(1) || 0}%
              </p>
              <p className="text-xs text-green-600 mt-1">Clinical outcome</p>
            </div>
            <ChartBarIcon className="h-8 w-8 text-green-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Adverse Reactions</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.adverseReactionRate?.toFixed(1) || 0}%
              </p>
              <p className="text-xs text-gray-600 mt-1">Monitoring rate</p>
            </div>
            <ExclamationTriangleIcon className="h-8 w-8 text-orange-400" />
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <a
            href="/oncology/diagnoses"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ClipboardDocumentCheckIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Diagnoses</span>
          </a>

          <a
            href="/oncology/chemotherapy-protocols"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <BeakerIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Chemo Protocols</span>
          </a>

          <a
            href="/oncology/chemotherapy-administration"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <HeartIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Administration</span>
          </a>

          <a
            href="/oncology/radiation-therapy"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ArrowTrendingUpIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Radiation</span>
          </a>

          <a
            href="/oncology/tumor-markers"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ChartBarIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Tumor Markers</span>
          </a>
        </div>
      </div>

      {/* Recent Activities */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">Recent Activities</h2>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Activity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Performed By
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {recentActivities && recentActivities.length > 0 ? (
                recentActivities.map((activity) => (
                  <tr key={activity.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">{activity.patientName}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {activity.activity}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(activity.date), 'MMM d, yyyy h:mm a')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {activity.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          activity.status
                        )}`}
                      >
                        {activity.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-primary-600 hover:text-primary-900">
                        View
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                    No recent activities found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
