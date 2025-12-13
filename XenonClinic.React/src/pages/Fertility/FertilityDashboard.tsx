import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { fertilityApi } from "../../lib/api";

export const FertilityDashboard = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["fertility-stats"],
    queryFn: async () => {
      const response = await fertilityApi.getStatistics();
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
          <h1 className="text-2xl font-bold text-gray-900">Fertility Center</h1>
          <p className="text-gray-600 mt-1">IVF and reproductive medicine</p>
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
          <p className="text-sm text-gray-600">Active Cycles</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {stats?.activeCycles || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Retrievals This Month</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {stats?.retrievalsThisMonth || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Transfers This Month</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {stats?.transfersThisMonth || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Positive Results</p>
          <p className="text-2xl font-bold text-pink-600 mt-1">
            {stats?.positiveResults || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Frozen Embryos</p>
          <p className="text-2xl font-bold text-cyan-600 mt-1">
            {stats?.frozenEmbryos || 0}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <Link
          to="/fertility/assessments"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Assessments</h3>
          <p className="text-sm text-gray-600 mt-1">
            Initial fertility assessments
          </p>
        </Link>
        <Link
          to="/fertility/cycles"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">IVF Cycles</h3>
          <p className="text-sm text-gray-600 mt-1">
            Manage IVF treatment cycles
          </p>
        </Link>
        <Link
          to="/fertility/monitoring"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Monitoring</h3>
          <p className="text-sm text-gray-600 mt-1">Stimulation monitoring</p>
        </Link>
        <Link
          to="/fertility/retrievals"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Retrievals</h3>
          <p className="text-sm text-gray-600 mt-1">Oocyte retrieval records</p>
        </Link>
        <Link
          to="/fertility/embryos"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Embryo Lab</h3>
          <p className="text-sm text-gray-600 mt-1">
            Embryo tracking and grading
          </p>
        </Link>
      </div>
    </div>
  );
};
