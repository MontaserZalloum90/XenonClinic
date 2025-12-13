import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { physiotherapyApi } from "../../lib/api";

export const PhysiotherapyDashboard = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["physio-stats"],
    queryFn: async () => {
      const response = await physiotherapyApi.getStatistics();
      return response.data;
    },
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Physiotherapy</h1>
          <p className="text-gray-600 mt-1">
            Physical therapy and rehabilitation
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Active Patients</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">
            {stats?.activePatients || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Sessions Today</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {stats?.sessionsToday || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Sessions This Week</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {stats?.sessionsThisWeek || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active Plans</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {stats?.activePlans || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed This Month</p>
          <p className="text-2xl font-bold text-teal-600 mt-1">
            {stats?.completedPlansThisMonth || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Avg Session (min)</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">
            {stats?.averageSessionDuration || 0}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Link
          to="/physiotherapy/plans"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Treatment Plans</h3>
          <p className="text-sm text-gray-600 mt-1">
            Create and manage treatment plans
          </p>
        </Link>
        <Link
          to="/physiotherapy/sessions"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Sessions</h3>
          <p className="text-sm text-gray-600 mt-1">Record therapy sessions</p>
        </Link>
        <Link
          to="/physiotherapy/rom"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">ROM Assessments</h3>
          <p className="text-sm text-gray-600 mt-1">
            Range of motion evaluations
          </p>
        </Link>
        <Link
          to="/physiotherapy/exercises"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Exercise Programs</h3>
          <p className="text-sm text-gray-600 mt-1">
            Assign home exercise programs
          </p>
        </Link>
      </div>
    </div>
  );
};
