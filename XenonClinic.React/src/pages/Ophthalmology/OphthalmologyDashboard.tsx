import { useQuery } from "@tanstack/react-query";
import {
  EyeIcon,
  BeakerIcon,
  ChartBarIcon,
  MagnifyingGlassIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import type { OphthalmologyStatistics } from "../../types/ophthalmology";
import { ophthalmologyApi } from "../../lib/api";

export const OphthalmologyDashboard = () => {
  const { data: stats, isLoading } = useQuery<OphthalmologyStatistics>({
    queryKey: ["ophthalmology-stats"],
    queryFn: async () => {
      const response = await ophthalmologyApi.getStatistics();
      return response.data;
    },
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Ophthalmology Dashboard
          </h1>
          <p className="text-gray-600 mt-1">
            Eye care and vision management overview
          </p>
        </div>
        <div className="flex gap-2">
          <button className="btn btn-outline">
            <ChartBarIcon className="h-5 w-5 mr-2" />
            View Reports
          </button>
          <button className="btn btn-primary">
            <DocumentTextIcon className="h-5 w-5 mr-2" />
            New Exam
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="text-gray-600 mt-4">Loading statistics...</p>
        </div>
      ) : (
        <>
          {/* Main Statistics */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-blue-100 rounded-lg">
                  <EyeIcon className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Total Patients</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {stats?.totalPatients || 0}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-green-100 rounded-lg">
                  <DocumentTextIcon className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Recent Exams</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {stats?.recentExams || 0}
                  </p>
                  <p className="text-xs text-gray-600 mt-1">This week</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-purple-100 rounded-lg">
                  <BeakerIcon className="h-6 w-6 text-purple-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Active Prescriptions</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {stats?.activePrescriptions || 0}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow p-4">
              <div className="flex items-center">
                <div className="p-3 bg-yellow-100 rounded-lg">
                  <ExclamationTriangleIcon className="h-6 w-6 text-yellow-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm text-gray-500">Expiring Soon</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {stats?.expiringPrescriptionsSoon || 0}
                  </p>
                  {stats && stats.highIOPCases && stats.highIOPCases > 0 && (
                    <p className="text-xs text-red-600 mt-1">
                      {stats.highIOPCases} high IOP
                    </p>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Monthly Activity */}
          <div className="card">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">
              This Month's Activity
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
              <div className="text-center">
                <p className="text-2xl font-bold text-blue-600">
                  {stats?.visualAcuityTestsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">
                  Visual Acuity Tests
                </p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-purple-600">
                  {stats?.refractionTestsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">Refraction Tests</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-indigo-600">
                  {stats?.iopMeasurementsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">IOP Measurements</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-teal-600">
                  {stats?.slitLampExamsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">Slit Lamp Exams</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-cyan-600">
                  {stats?.fundusExamsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">Fundus Exams</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-green-600">
                  {stats?.prescriptionsThisMonth || 0}
                </p>
                <p className="text-sm text-gray-600 mt-1">Prescriptions</p>
              </div>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">
              Quick Actions
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              <a
                href="/ophthalmology/visual-acuity"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <EyeIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  Visual Acuity Tests
                </span>
              </a>

              <a
                href="/ophthalmology/refraction"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <MagnifyingGlassIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  Refraction Tests
                </span>
              </a>

              <a
                href="/ophthalmology/iop"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <ChartBarIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  IOP Measurements
                </span>
              </a>

              <a
                href="/ophthalmology/slit-lamp"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <BeakerIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  Slit Lamp Exams
                </span>
              </a>

              <a
                href="/ophthalmology/fundus"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <EyeIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  Fundus Exams
                </span>
              </a>

              <a
                href="/ophthalmology/prescriptions"
                className="flex flex-col items-center justify-center p-4 border-2 border-gray-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
              >
                <DocumentTextIcon className="h-8 w-8 text-primary-600 mb-2" />
                <span className="text-sm font-medium text-gray-900">
                  Glasses Prescriptions
                </span>
              </a>
            </div>
          </div>

          {/* Alerts */}
          {stats &&
            (stats.expiringPrescriptionsSoon > 0 ||
              (stats.highIOPCases && stats.highIOPCases > 0)) && (
              <div className="card bg-yellow-50 border-yellow-200">
                <h2 className="text-lg font-semibold text-gray-900 mb-3">
                  Alerts & Notifications
                </h2>
                <div className="space-y-2">
                  {stats.expiringPrescriptionsSoon > 0 && (
                    <div className="flex items-center text-yellow-800">
                      <svg
                        className="h-5 w-5 mr-2"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fillRule="evenodd"
                          d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                          clipRule="evenodd"
                        />
                      </svg>
                      <span>
                        {stats.expiringPrescriptionsSoon} prescription(s)
                        expiring within 30 days
                      </span>
                    </div>
                  )}
                  {stats.highIOPCases && stats.highIOPCases > 0 && (
                    <div className="flex items-center text-red-800">
                      <svg
                        className="h-5 w-5 mr-2"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fillRule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                          clipRule="evenodd"
                        />
                      </svg>
                      <span>
                        {stats.highIOPCases} patient(s) with elevated IOP
                        (&gt;21 mmHg)
                      </span>
                    </div>
                  )}
                </div>
              </div>
            )}
        </>
      )}
    </div>
  );
};
