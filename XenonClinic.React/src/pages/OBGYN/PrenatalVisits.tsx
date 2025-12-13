import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  PlusIcon,
  XMarkIcon,
  HeartIcon,
  CalendarIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { prenatalVisitsApi } from "../../lib/api";

interface PrenatalVisit {
  id: number;
  patientId: number;
  patientName: string;
  visitDate: string;
  gestationalAge: number;
  weight: number;
  bloodPressure: string;
  fundalHeight?: number;
  fetalHeartRate?: number;
  fetalMovement: string;
  urineProtein: string;
  urineGlucose: string;
  edema: string;
  complaints?: string;
  assessment: string;
  plan: string;
  nextVisitDate?: string;
  performedBy: string;
  riskLevel: "low" | "moderate" | "high";
  createdAt: string;
}

export const PrenatalVisits = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [riskFilter, setRiskFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedVisit, setSelectedVisit] = useState<PrenatalVisit | undefined>(
    undefined,
  );

  const { data: visitsResponse, isLoading } = useQuery<PrenatalVisit[]>({
    queryKey: ["prenatal-visits"],
    queryFn: () => prenatalVisitsApi.getAll(),
  });

  const visits = visitsResponse?.data || [];

  const filteredVisits = visits?.filter((visit) => {
    const matchesSearch =
      !searchTerm ||
      visit.patientName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRisk = !riskFilter || visit.riskLevel === riskFilter;
    return matchesSearch && matchesRisk;
  });

  const handleCreate = () => {
    setSelectedVisit(undefined);
    setIsModalOpen(true);
  };

  const handleView = (visit: PrenatalVisit) => {
    setSelectedVisit(visit);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedVisit(undefined);
  };

  const getRiskBadge = (risk: string) => {
    const config: Record<string, { className: string; label: string }> = {
      low: { className: "bg-green-100 text-green-800", label: "Low Risk" },
      moderate: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Moderate Risk",
      },
      high: { className: "bg-red-100 text-red-800", label: "High Risk" },
    };
    const c = config[risk] || {
      className: "bg-gray-100 text-gray-800",
      label: risk,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const highRiskCount =
    visits?.filter((v) => v.riskLevel === "high").length || 0;
  const todaysVisits =
    visits?.filter(
      (v) => new Date(v.visitDate).toDateString() === new Date().toDateString(),
    ).length || 0;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Prenatal Visits</h1>
          <p className="text-gray-600 mt-1">
            Track and manage pregnancy care visits
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Visit
        </button>
      </div>

      {highRiskCount > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <div className="flex items-center">
            <HeartIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                High Risk Pregnancies
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {highRiskCount} patient(s) require close monitoring.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <CalendarIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Visits</p>
              <p className="text-2xl font-bold text-gray-900">
                {visits?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-blue-600 font-bold">{todaysVisits}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Today</p>
              <p className="text-2xl font-bold text-blue-600">{todaysVisits}</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {visits?.filter((v) => v.riskLevel === "low").length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Low Risk</p>
              <p className="text-2xl font-bold text-green-600">
                {visits?.filter((v) => v.riskLevel === "low").length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-red-100 flex items-center justify-center">
              <span className="text-red-600 font-bold">{highRiskCount}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">High Risk</p>
              <p className="text-2xl font-bold text-red-600">{highRiskCount}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md"
              />
            </div>
          </div>
          <select
            value={riskFilter}
            onChange={(e) => setRiskFilter(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md"
          >
            <option value="">All Risk Levels</option>
            <option value="low">Low Risk</option>
            <option value="moderate">Moderate Risk</option>
            <option value="high">High Risk</option>
          </select>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  GA (weeks)
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  BP
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  FHR
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Risk
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Provider
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading visits...</p>
                  </td>
                </tr>
              ) : filteredVisits && filteredVisits.length > 0 ? (
                filteredVisits.map((visit) => (
                  <tr
                    key={visit.id}
                    className={`hover:bg-gray-50 ${visit.riskLevel === "high" ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap font-medium text-gray-900">
                      {visit.patientName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(visit.visitDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {visit.gestationalAge}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {visit.bloodPressure}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {visit.fetalHeartRate
                        ? `${visit.fetalHeartRate} bpm`
                        : "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getRiskBadge(visit.riskLevel)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {visit.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <button
                        onClick={() => handleView(visit)}
                        className="text-primary-600 hover:text-primary-900"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={8}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No prenatal visits found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Dialog
        open={isModalOpen}
        onClose={handleModalClose}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-4">
                <Dialog.Title className="text-lg font-medium text-gray-900">
                  {selectedVisit
                    ? "Prenatal Visit Details"
                    : "New Prenatal Visit"}
                </Dialog.Title>
                <button
                  onClick={handleModalClose}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              {selectedVisit && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-gray-900">
                        {selectedVisit.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Gestational Age
                      </label>
                      <p className="text-gray-900">
                        {selectedVisit.gestationalAge} weeks
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Blood Pressure
                      </label>
                      <p className="text-gray-900">
                        {selectedVisit.bloodPressure}
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Weight
                      </label>
                      <p className="text-gray-900">{selectedVisit.weight} kg</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Fetal Heart Rate
                      </label>
                      <p className="text-gray-900">
                        {selectedVisit.fetalHeartRate} bpm
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Fundal Height
                      </label>
                      <p className="text-gray-900">
                        {selectedVisit.fundalHeight} cm
                      </p>
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Assessment
                    </label>
                    <p className="text-gray-700 mt-1">
                      {selectedVisit.assessment}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Plan
                    </label>
                    <p className="text-gray-700 mt-1">{selectedVisit.plan}</p>
                  </div>
                </div>
              )}
              <div className="flex justify-end gap-3 pt-4 mt-4 border-t">
                <button onClick={handleModalClose} className="btn btn-outline">
                  Close
                </button>
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
