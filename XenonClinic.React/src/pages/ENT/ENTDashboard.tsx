import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { entApi } from "../../lib/api";

export const ENTDashboard = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["ent-stats"],
    queryFn: async () => {
      const response = await entApi.getStatistics();
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
          <h1 className="text-2xl font-bold text-gray-900">ENT Department</h1>
          <p className="text-gray-600 mt-1">
            Ear, Nose, and Throat specialists
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Exams Today</p>
          <p className="text-2xl font-bold text-primary-600 mt-1">
            {stats?.examsToday || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Endoscopies This Week</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {stats?.endoscopiesThisWeek || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Laryngoscopies This Month</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {stats?.laryngoscopiesThisMonth || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Tympanometries This Week</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {stats?.tympanometriesThisWeek || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Pending Follow-ups</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">
            {stats?.pendingFollowUps || 0}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Link
          to="/ent/ear-exams"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Ear Exams</h3>
          <p className="text-sm text-gray-600 mt-1">
            Otoscopy and ear examinations
          </p>
        </Link>
        <Link
          to="/ent/nasal-endoscopy"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Nasal Endoscopy</h3>
          <p className="text-sm text-gray-600 mt-1">
            Nasal and sinus evaluations
          </p>
        </Link>
        <Link
          to="/ent/laryngoscopy"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Laryngoscopy</h3>
          <p className="text-sm text-gray-600 mt-1">
            Vocal cord and throat exams
          </p>
        </Link>
        <Link
          to="/ent/tympanometry"
          className="card hover:shadow-lg transition-shadow"
        >
          <h3 className="font-medium text-gray-900">Tympanometry</h3>
          <p className="text-sm text-gray-600 mt-1">
            Middle ear function testing
          </p>
        </Link>
      </div>
    </div>
  );
};
