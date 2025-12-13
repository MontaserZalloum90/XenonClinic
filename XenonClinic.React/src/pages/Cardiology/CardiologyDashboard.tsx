import { useQuery } from "@tanstack/react-query";
import {
  HeartIcon,
  ClipboardDocumentCheckIcon,
  ChartBarIcon,
  BeakerIcon,
  CalendarIcon,
  DocumentTextIcon,
  ArrowTrendingUpIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { CardiologyStatistics } from "../../types/cardiology";
import { cardiologyApi } from "../../lib/api";

export const CardiologyDashboard = () => {
  const { data: statsResponse } = useQuery<CardiologyStatistics>({
    queryKey: ["cardiology-stats"],
    queryFn: () => cardiologyApi.getDashboard(),
  });

  const stats = statsResponse?.data;

  const { data: recentProceduresResponse } = useQuery<
    Array<{
      id: number;
      patientName: string;
      procedureType: string;
      date: string;
      status: string;
      performedBy: string;
    }>
  >({
    queryKey: ["recent-procedures"],
    queryFn: () => cardiologyApi.getStatistics(),
  });

  const recentProcedures = recentProceduresResponse?.data || [];

  const statistics = stats || {
    totalPatients: 0,
    ecgsToday: 0,
    echosThisWeek: 0,
    pendingResults: 0,
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Completed: "text-green-600 bg-green-100",
      "In Progress": "text-yellow-600 bg-yellow-100",
      Scheduled: "text-blue-600 bg-blue-100",
      Pending: "text-orange-600 bg-orange-100",
      Cancelled: "text-red-600 bg-red-100",
    };
    return colors[status] || "text-gray-600 bg-gray-100";
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Cardiology Dashboard
          </h1>
          <p className="text-gray-600 mt-1">
            Cardiac examinations and procedures overview
          </p>
        </div>
        <div className="flex gap-2">
          <button className="btn btn-outline">
            <CalendarIcon className="h-5 w-5 mr-2" />
            Schedule Procedure
          </button>
          <button className="btn btn-primary">
            <DocumentTextIcon className="h-5 w-5 mr-2" />
            New Record
          </button>
        </div>
      </div>

      {/* Statistics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <HeartIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Patients</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.totalPatients}
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
              <p className="text-sm text-gray-500">ECGs Today</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.ecgsToday}
              </p>
              {statistics.ecgsThisWeek && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.ecgsThisWeek} this week
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
              <p className="text-sm text-gray-500">Echos This Week</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.echosThisWeek}
              </p>
              {statistics.echosThisMonth && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.echosThisMonth} this month
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg">
              <ClipboardDocumentCheckIcon className="h-6 w-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pending Results</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.pendingResults}
              </p>
              {statistics.pendingReviews && statistics.pendingReviews > 0 && (
                <p className="text-xs text-red-600 mt-1">
                  {statistics.pendingReviews} need review
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Additional Stats Row */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Stress Tests</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.stressTestsThisMonth || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">This month</p>
            </div>
            <ArrowTrendingUpIcon className="h-8 w-8 text-blue-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Catheterizations</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.cathsThisMonth || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">This month</p>
            </div>
            <HeartIcon className="h-8 w-8 text-red-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Abnormal ECGs</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.abnormalECGs || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">Requiring follow-up</p>
            </div>
            <ExclamationTriangleIcon className="h-8 w-8 text-orange-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Reduced EF Cases</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.reducedEF || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">EF {"<"} 50%</p>
            </div>
            <ChartBarIcon className="h-8 w-8 text-purple-400" />
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">
          Quick Actions
        </h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <a
            href="/cardiology/ecg"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ChartBarIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              ECG Records
            </span>
          </a>

          <a
            href="/cardiology/echo"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <BeakerIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Echocardiograms
            </span>
          </a>

          <a
            href="/cardiology/stress-tests"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ArrowTrendingUpIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Stress Tests
            </span>
          </a>

          <a
            href="/cardiology/cath-lab"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <HeartIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Cath Lab</span>
          </a>

          <a
            href="/cardiology/risk-calculator"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ExclamationTriangleIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Risk Calculator
            </span>
          </a>
        </div>
      </div>

      {/* Recent Procedures */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">
            Recent Procedures
          </h2>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Procedure Type
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
              {recentProcedures && recentProcedures.length > 0 ? (
                recentProcedures.map((procedure) => (
                  <tr key={procedure.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {procedure.patientName}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {procedure.procedureType}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(procedure.date), "MMM d, yyyy h:mm a")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {procedure.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          procedure.status,
                        )}`}
                      >
                        {procedure.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-primary-600 hover:text-primary-900 mr-3">
                        View
                      </button>
                      {procedure.status === "Pending" && (
                        <button className="text-green-600 hover:text-green-900">
                          Review
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={6}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No recent procedures found
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
