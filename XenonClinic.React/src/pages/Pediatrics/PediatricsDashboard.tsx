import { useQuery } from '@tanstack/react-query';
import {
  UserGroupIcon,
  ChartBarIcon,
  BeakerIcon,
  ClipboardDocumentCheckIcon,
  CalendarIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon,
  PlusIcon,
} from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import type { PediatricStatistics } from '../../types/pediatrics';

export const PediatricsDashboard = () => {
  // Mock data - In production, replace with actual API calls
  const { data: stats } = useQuery<PediatricStatistics>({
    queryKey: ['pediatric-stats'],
    queryFn: async () => {
      // Mock implementation
      return {
        totalPediatricPatients: 567,
        newPatientsThisMonth: 42,
        growthMeasurementsThisMonth: 89,
        growthMeasurementsToday: 8,
        milestonesTracked: 234,
        milestonesDelayed: 12,
        milestonesAchievedThisMonth: 45,
        vaccinationsThisMonth: 156,
        vaccinationsToday: 15,
        overdueVaccinations: 23,
        upcomingVaccinations: 67,
        ageDistribution: {
          infants: 123,
          toddlers: 145,
          preschool: 98,
          schoolAge: 156,
          adolescents: 45,
        },
        growthConcerns: 8,
        developmentConcerns: 5,
      };
    },
  });

  const { data: recentActivities } = useQuery<any[]>({
    queryKey: ['recent-activities'],
    queryFn: async () => {
      // Mock implementation
      return [
        {
          id: 1,
          patientName: 'Emma Johnson',
          activity: 'Growth Measurement',
          date: new Date().toISOString(),
          details: 'Height: 110cm, Weight: 18kg',
          status: 'Completed',
        },
        {
          id: 2,
          patientName: 'Noah Smith',
          activity: 'Vaccination',
          date: new Date().toISOString(),
          details: 'MMR Dose 2',
          status: 'Completed',
        },
        {
          id: 3,
          patientName: 'Olivia Davis',
          activity: 'Milestone Assessment',
          date: new Date().toISOString(),
          details: 'Language development review',
          status: 'In Progress',
        },
      ];
    },
  });

  const statistics = stats || {
    totalPediatricPatients: 0,
    growthMeasurementsThisMonth: 0,
    vaccinationsThisMonth: 0,
    overdueVaccinations: 0,
    milestonesTracked: 0,
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Completed: 'text-green-600 bg-green-100',
      'In Progress': 'text-yellow-600 bg-yellow-100',
      Scheduled: 'text-blue-600 bg-blue-100',
      Overdue: 'text-red-600 bg-red-100',
      Pending: 'text-orange-600 bg-orange-100',
    };
    return colors[status] || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Pediatrics Dashboard</h1>
          <p className="text-gray-600 mt-1">
            Growth tracking, milestones, vaccinations, and pediatric care
          </p>
        </div>
        <div className="flex gap-2">
          <button className="btn btn-outline">
            <CalendarIcon className="h-5 w-5 mr-2" />
            Schedule Visit
          </button>
          <button className="btn btn-primary">
            <DocumentTextIcon className="h-5 w-5 mr-2" />
            New Record
          </button>
        </div>
      </div>

      {/* Main Statistics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <UserGroupIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pediatric Patients</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.totalPediatricPatients}
              </p>
              {statistics.newPatientsThisMonth && (
                <p className="text-xs text-green-600 mt-1">
                  +{statistics.newPatientsThisMonth} this month
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg">
              <ChartBarIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Growth Measurements</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.growthMeasurementsThisMonth}
              </p>
              {statistics.growthMeasurementsToday && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.growthMeasurementsToday} today
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-purple-100 rounded-lg">
              <BeakerIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Vaccinations</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.vaccinationsThisMonth}
              </p>
              {statistics.vaccinationsToday && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.vaccinationsToday} today
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-red-100 rounded-lg">
              <ExclamationTriangleIcon className="h-6 w-6 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Overdue Vaccines</p>
              <p className="text-2xl font-bold text-red-900">{statistics.overdueVaccinations}</p>
              {statistics.upcomingVaccinations && statistics.upcomingVaccinations > 0 && (
                <p className="text-xs text-orange-600 mt-1">
                  {statistics.upcomingVaccinations} upcoming
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Secondary Statistics Row */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Milestones Tracked</p>
              <p className="text-xl font-bold text-gray-900">{statistics.milestonesTracked}</p>
              <p className="text-xs text-gray-600 mt-1">
                {statistics.milestonesAchievedThisMonth || 0} achieved this month
              </p>
            </div>
            <ClipboardDocumentCheckIcon className="h-8 w-8 text-blue-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Delayed Milestones</p>
              <p className="text-xl font-bold text-orange-900">
                {statistics.milestonesDelayed || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">Requiring attention</p>
            </div>
            <ExclamationTriangleIcon className="h-8 w-8 text-orange-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Growth Concerns</p>
              <p className="text-xl font-bold text-red-900">{statistics.growthConcerns || 0}</p>
              <p className="text-xs text-gray-600 mt-1">Below percentiles</p>
            </div>
            <ChartBarIcon className="h-8 w-8 text-red-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Development Concerns</p>
              <p className="text-xl font-bold text-red-900">
                {statistics.developmentConcerns || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">Need evaluation</p>
            </div>
            <ClipboardDocumentCheckIcon className="h-8 w-8 text-red-400" />
          </div>
        </div>
      </div>

      {/* Age Distribution */}
      {statistics.ageDistribution && (
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Patient Age Distribution</h2>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <p className="text-2xl font-bold text-blue-900">
                {statistics.ageDistribution.infants || 0}
              </p>
              <p className="text-sm text-gray-600 mt-1">Infants</p>
              <p className="text-xs text-gray-500">0-12 months</p>
            </div>
            <div className="text-center p-4 bg-green-50 rounded-lg">
              <p className="text-2xl font-bold text-green-900">
                {statistics.ageDistribution.toddlers || 0}
              </p>
              <p className="text-sm text-gray-600 mt-1">Toddlers</p>
              <p className="text-xs text-gray-500">1-3 years</p>
            </div>
            <div className="text-center p-4 bg-yellow-50 rounded-lg">
              <p className="text-2xl font-bold text-yellow-900">
                {statistics.ageDistribution.preschool || 0}
              </p>
              <p className="text-sm text-gray-600 mt-1">Preschool</p>
              <p className="text-xs text-gray-500">3-5 years</p>
            </div>
            <div className="text-center p-4 bg-purple-50 rounded-lg">
              <p className="text-2xl font-bold text-purple-900">
                {statistics.ageDistribution.schoolAge || 0}
              </p>
              <p className="text-sm text-gray-600 mt-1">School Age</p>
              <p className="text-xs text-gray-500">5-12 years</p>
            </div>
            <div className="text-center p-4 bg-pink-50 rounded-lg">
              <p className="text-2xl font-bold text-pink-900">
                {statistics.ageDistribution.adolescents || 0}
              </p>
              <p className="text-sm text-gray-600 mt-1">Adolescents</p>
              <p className="text-xs text-gray-500">12-18 years</p>
            </div>
          </div>
        </div>
      )}

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <a
            href="/pediatrics/growth-charts"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ChartBarIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Growth Charts</span>
          </a>

          <a
            href="/pediatrics/milestones"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ClipboardDocumentCheckIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Milestones</span>
          </a>

          <a
            href="/pediatrics/vaccinations"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <BeakerIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Vaccinations</span>
          </a>

          <a
            href="/pediatrics/dosing-calculator"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <DocumentTextIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Dosing Calculator</span>
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
                  Details
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
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
                    <td className="px-6 py-4 text-sm text-gray-600">{activity.details}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(activity.date), 'MMM d, yyyy h:mm a')}
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
                      <button className="text-primary-600 hover:text-primary-900">View</button>
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
