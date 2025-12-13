import { useQuery } from "@tanstack/react-query";
import {
  BeakerIcon,
  ClipboardDocumentCheckIcon,
  ChartBarIcon,
  BoltIcon,
  CalendarIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon,
  HeartIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { NeurologyStatistics } from "../../types/neurology";
import { neurologyApi } from "../../lib/api";

export const NeurologyDashboard = () => {
  const { data: statsResponse } = useQuery<NeurologyStatistics>({
    queryKey: ["neurology-stats"],
    queryFn: () => neurologyApi.getDashboard(),
  });

  const stats = statsResponse?.data;

  const { data: recentExamsResponse } = useQuery<
    Array<{
      id: number;
      patientName: string;
      examType: string;
      date: string;
      status: string;
      performedBy: string;
    }>
  >({
    queryKey: ["recent-neuro-exams"],
    queryFn: () => neurologyApi.getStatistics(),
  });

  const recentExams = recentExamsResponse?.data || [];

  const statistics = stats || {
    totalPatients: 0,
    examsToday: 0,
    eegsThisWeek: 0,
    pendingReviews: 0,
    newPatientsThisMonth: 0,
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Completed: "text-green-600 bg-green-100",
      "In Progress": "text-yellow-600 bg-yellow-100",
      Scheduled: "text-blue-600 bg-blue-100",
      "Pending Review": "text-orange-600 bg-orange-100",
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
            Neurology Dashboard
          </h1>
          <p className="text-gray-600 mt-1">
            Neurological examinations and assessments overview
          </p>
        </div>
        <div className="flex gap-2">
          <button className="btn btn-outline">
            <CalendarIcon className="h-5 w-5 mr-2" />
            Schedule Exam
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
              <ClipboardDocumentCheckIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Exams Today</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.examsToday}
              </p>
              {statistics.examsThisWeek && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.examsThisWeek} this week
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-purple-100 rounded-lg">
              <ChartBarIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">EEGs This Week</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.eegsThisWeek}
              </p>
              {statistics.eegsThisMonth && (
                <p className="text-xs text-gray-600 mt-1">
                  {statistics.eegsThisMonth} this month
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg">
              <ExclamationTriangleIcon className="h-6 w-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pending Reviews</p>
              <p className="text-2xl font-bold text-gray-900">
                {statistics.pendingReviews}
              </p>
              {statistics.pendingReviews > 0 && (
                <p className="text-xs text-red-600 mt-1">Requires attention</p>
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
              <p className="text-sm text-gray-500">Seizures Logged</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.seizuresThisMonth || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">This month</p>
            </div>
            <BoltIcon className="h-8 w-8 text-orange-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Epilepsy Patients</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.epilepsyPatients || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">Active monitoring</p>
            </div>
            <BeakerIcon className="h-8 w-8 text-purple-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Stroke Assessments</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.strokeAssessmentsThisMonth || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">This month</p>
            </div>
            <HeartIcon className="h-8 w-8 text-red-400" />
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Abnormal EEGs</p>
              <p className="text-xl font-bold text-gray-900">
                {statistics.abnormalEEGs || 0}
              </p>
              <p className="text-xs text-gray-600 mt-1">Requiring follow-up</p>
            </div>
            <ExclamationTriangleIcon className="h-8 w-8 text-yellow-400" />
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">
          Quick Actions
        </h2>
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
          <a
            href="/neurology/exams"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ClipboardDocumentCheckIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Neuro Exams
            </span>
          </a>

          <a
            href="/neurology/eeg"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <ChartBarIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              EEG Records
            </span>
          </a>

          <a
            href="/neurology/epilepsy"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <BoltIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Epilepsy Diary
            </span>
          </a>

          <a
            href="/neurology/stroke"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <HeartIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">
              Stroke Assessment
            </span>
          </a>

          <a
            href="/neurology/reports"
            className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <DocumentTextIcon className="h-8 w-8 text-primary-600 mb-2" />
            <span className="text-sm font-medium text-gray-900">Reports</span>
          </a>
        </div>
      </div>

      {/* Recent Examinations */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">
            Recent Examinations
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
                  Exam Type
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
              {recentExams && recentExams.length > 0 ? (
                recentExams.map((exam) => (
                  <tr key={exam.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {exam.patientName}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {exam.examType}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(exam.date), "MMM d, yyyy h:mm a")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {exam.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                          exam.status,
                        )}`}
                      >
                        {exam.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-primary-600 hover:text-primary-900 mr-3">
                        View
                      </button>
                      {exam.status === "Pending Review" && (
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
                    No recent examinations found
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
