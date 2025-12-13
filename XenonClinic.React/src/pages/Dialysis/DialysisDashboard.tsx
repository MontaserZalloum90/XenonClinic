import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { dialysisApi } from "../../lib/api";

export const DialysisDashboard = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["dialysis-stats"],
    queryFn: async () => {
      const response = await dialysisApi.getStatistics();
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
          <h1 className="text-2xl font-bold text-gray-900">Dialysis Unit</h1>
          <p className="text-gray-600 mt-1">Hemodialysis and renal care</p>
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
          <p className="text-sm text-gray-600">Average Kt/V</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {stats?.averageKtV?.toFixed(2) || "0.00"}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Average URR (%)</p>
          <p className="text-2xl font-bold text-teal-600 mt-1">
            {stats?.averageURR || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Incomplete Sessions</p>
          <p className="text-2xl font-bold text-red-600 mt-1">
            {stats?.incompleteSessionsThisMonth || 0}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Link
          to="/dialysis/sessions"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Sessions</h3>
          <p className="text-sm text-gray-600 mt-1">Record dialysis sessions</p>
        </Link>
        <Link
          to="/dialysis/schedule"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Schedule</h3>
          <p className="text-sm text-gray-600 mt-1">Patient scheduling</p>
        </Link>
        <Link
          to="/dialysis/patients"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Patients</h3>
          <p className="text-sm text-gray-600 mt-1">
            Dialysis patient registry
          </p>
        </Link>
        <Link
          to="/dialysis/labs"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Lab Results</h3>
          <p className="text-sm text-gray-600 mt-1">Monthly lab tracking</p>
        </Link>
      </div>
    </div>
  );
};
