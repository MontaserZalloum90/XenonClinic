import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import type { DermatologyStatistics } from '../../types/dermatology';

// Mock API - Replace with actual dermatology API
const dermatologyApi = {
  getStatistics: async () => {
    // TODO: Implement actual API call
    return {
      data: {
        totalPatients: 0,
        newPatientsThisMonth: 0,
        examsThisWeek: 0,
        examsThisMonth: 0,
        totalPhotos: 0,
        photosThisMonth: 0,
        moleMappingsThisMonth: 0,
        highRiskPatients: 0,
        biopsiesThisMonth: 0,
        pendingBiopsies: 0,
        requiresFollowUp: 0,
      } as DermatologyStatistics,
    };
  },
};

export const DermatologyDashboard = () => {
  const { data: stats, isLoading } = useQuery<DermatologyStatistics>({
    queryKey: ['dermatology-stats'],
    queryFn: async () => {
      const response = await dermatologyApi.getStatistics();
      return response.data;
    },
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        <p className="ml-3 text-gray-600">Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dermatology Dashboard</h1>
        <p className="text-gray-600 mt-1">Overview of dermatology services and patient care</p>
      </div>

      {/* Statistics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Patient Stats */}
        <div className="card">
          <p className="text-sm text-gray-600">Total Patients</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalPatients || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            +{stats?.newPatientsThisMonth || 0} this month
          </p>
        </div>

        {/* Exam Stats */}
        <div className="card">
          <p className="text-sm text-gray-600">Exams This Week</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{stats?.examsThisWeek || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            {stats?.examsThisMonth || 0} this month
          </p>
        </div>

        {/* Biopsy Stats */}
        <div className="card">
          <p className="text-sm text-gray-600">Pending Biopsies</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats?.pendingBiopsies || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            {stats?.biopsiesThisMonth || 0} this month
          </p>
        </div>

        {/* Follow-up Stats */}
        <div className="card">
          <p className="text-sm text-gray-600">Requires Follow-up</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.requiresFollowUp || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            Action needed
          </p>
        </div>
      </div>

      {/* Additional Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Photos</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">{stats?.totalPhotos || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            +{stats?.photosThisMonth || 0} this month
          </p>
        </div>

        <div className="card">
          <p className="text-sm text-gray-600">Mole Mappings</p>
          <p className="text-2xl font-bold text-indigo-600 mt-1">{stats?.moleMappingsThisMonth || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            This month
          </p>
        </div>

        <div className="card">
          <p className="text-sm text-gray-600">High Risk Patients</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">{stats?.highRiskPatients || 0}</p>
          <p className="text-xs text-gray-500 mt-1">
            Monitoring required
          </p>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <Link
            to="/dermatology/skin-exams"
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <h3 className="font-medium text-gray-900">Skin Exams</h3>
            <p className="text-sm text-gray-600 mt-1">
              Record and manage skin examinations
            </p>
          </Link>

          <Link
            to="/dermatology/skin-photos"
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <h3 className="font-medium text-gray-900">Skin Photos</h3>
            <p className="text-sm text-gray-600 mt-1">
              Upload and manage patient photos
            </p>
          </Link>

          <Link
            to="/dermatology/mole-mappings"
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <h3 className="font-medium text-gray-900">Mole Mapping</h3>
            <p className="text-sm text-gray-600 mt-1">
              Track and document mole locations
            </p>
          </Link>

          <Link
            to="/dermatology/biopsies"
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <h3 className="font-medium text-gray-900">Biopsies</h3>
            <p className="text-sm text-gray-600 mt-1">
              Manage biopsy procedures and results
            </p>
          </Link>

          <div className="p-4 border border-gray-200 rounded-lg bg-gray-50">
            <h3 className="font-medium text-gray-900">Treatment Plans</h3>
            <p className="text-sm text-gray-600 mt-1">
              Coming soon
            </p>
          </div>

          <div className="p-4 border border-gray-200 rounded-lg bg-gray-50">
            <h3 className="font-medium text-gray-900">Reports</h3>
            <p className="text-sm text-gray-600 mt-1">
              Coming soon
            </p>
          </div>
        </div>
      </div>

      {/* Risk Assessment Summary */}
      {stats?.riskLevelDistribution && (
        <div className="card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Patient Risk Distribution</h2>
          <div className="space-y-3">
            {Object.entries(stats.riskLevelDistribution).map(([level, count]) => (
              <div key={level} className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div
                    className={`w-3 h-3 rounded-full ${
                      level === 'high'
                        ? 'bg-red-500'
                        : level === 'moderate'
                        ? 'bg-yellow-500'
                        : 'bg-green-500'
                    }`}
                  />
                  <span className="text-sm text-gray-700 capitalize">{level} Risk</span>
                </div>
                <span className="text-sm font-medium text-gray-900">{count} patients</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
