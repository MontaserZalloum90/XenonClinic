import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  HeartIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  ClockIcon,
  ArrowPathIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { vascularAccessApi } from "../../lib/api";

interface VascularAccessRecord {
  id: number;
  patientId: number;
  patientName: string;
  accessType: "avf" | "avg" | "cvc_tunneled" | "cvc_non_tunneled";
  location: string;
  creationDate: string;
  maturityDate?: string;
  status: "maturing" | "functioning" | "failing" | "failed" | "removed";
  lastAssessmentDate: string;
  thrill: "strong" | "weak" | "absent";
  bruit: "strong" | "weak" | "absent";
  complications?: string[];
  flowRate?: number;
  interventions?: AccessIntervention[];
  notes?: string;
  surgeon?: string;
}

interface AccessIntervention {
  date: string;
  type: string;
  outcome: string;
  performedBy: string;
}

const accessTypeConfig = {
  avf: {
    label: "Arteriovenous Fistula (AVF)",
    color: "bg-blue-100 text-blue-800",
  },
  avg: {
    label: "Arteriovenous Graft (AVG)",
    color: "bg-purple-100 text-purple-800",
  },
  cvc_tunneled: {
    label: "Tunneled CVC",
    color: "bg-orange-100 text-orange-800",
  },
  cvc_non_tunneled: {
    label: "Non-Tunneled CVC",
    color: "bg-red-100 text-red-800",
  },
};

const statusConfig = {
  maturing: {
    label: "Maturing",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  functioning: {
    label: "Functioning",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  failing: {
    label: "Failing",
    color: "bg-orange-100 text-orange-800",
    icon: ExclamationTriangleIcon,
  },
  failed: {
    label: "Failed",
    color: "bg-red-100 text-red-800",
    icon: ExclamationTriangleIcon,
  },
  removed: {
    label: "Removed",
    color: "bg-gray-100 text-gray-800",
    icon: ArrowPathIcon,
  },
};

export function VascularAccess() {
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] =
    useState<VascularAccessRecord | null>(null);

  const { data: records = [], isLoading } = useQuery({
    queryKey: ["vascular-access", searchTerm, typeFilter, statusFilter],
    queryFn: async () => {
      const response = await vascularAccessApi.getAll();
      return response.data;
    },
  });

  const filteredRecords = records.filter((record: VascularAccessRecord) => {
    const matchesSearch =
      record.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.location.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType =
      typeFilter === "all" || record.accessType === typeFilter;
    const matchesStatus =
      statusFilter === "all" || record.status === statusFilter;
    return matchesSearch && matchesType && matchesStatus;
  });

  const stats = {
    totalActive: records.filter(
      (r: VascularAccessRecord) =>
        r.status === "functioning" || r.status === "maturing",
    ).length,
    avf: records.filter(
      (r: VascularAccessRecord) =>
        r.accessType === "avf" && r.status !== "removed",
    ).length,
    failing: records.filter((r: VascularAccessRecord) => r.status === "failing")
      .length,
    cvc: records.filter(
      (r: VascularAccessRecord) =>
        (r.accessType === "cvc_tunneled" ||
          r.accessType === "cvc_non_tunneled") &&
        r.status === "functioning",
    ).length,
  };

  const handleViewDetails = (record: VascularAccessRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">
            Vascular Access Management
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track and monitor dialysis access sites, assessments, and
            interventions
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Access Record
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <HeartIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Active Access
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.totalActive}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <CheckCircleIcon className="h-6 w-6 text-blue-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    AVF/AVG
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.avf}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <ExclamationTriangleIcon className="h-6 w-6 text-orange-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Failing Access
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.failing}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <ClockIcon className="h-6 w-6 text-purple-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    CVC in Use
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.cvc}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white shadow rounded-lg p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search by patient or location..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              />
            </div>
          </div>
          <select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Types</option>
            <option value="avf">AVF</option>
            <option value="avg">AVG</option>
            <option value="cvc_tunneled">Tunneled CVC</option>
            <option value="cvc_non_tunneled">Non-Tunneled CVC</option>
          </select>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Status</option>
            <option value="maturing">Maturing</option>
            <option value="functioning">Functioning</option>
            <option value="failing">Failing</option>
            <option value="failed">Failed</option>
            <option value="removed">Removed</option>
          </select>
        </div>
      </div>

      {/* Access Records List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Access Type
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Location
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Assessment
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredRecords.map((record: VascularAccessRecord) => {
              const StatusIcon = statusConfig[record.status].icon;
              return (
                <tr key={record.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {record.patientName}
                    </div>
                    <div className="text-sm text-gray-500">
                      ID: {record.patientId}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${accessTypeConfig[record.accessType].color}`}
                    >
                      {accessTypeConfig[record.accessType].label.split(" ")[0]}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm text-gray-900">
                      {record.location}
                    </div>
                    <div className="text-sm text-gray-500">
                      Created:{" "}
                      {format(new Date(record.creationDate), "MMM d, yyyy")}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[record.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[record.status].label}
                    </span>
                    {record.complications &&
                      record.complications.length > 0 && (
                        <div className="mt-1">
                          <span className="text-xs text-red-600">
                            {record.complications.length} complication(s)
                          </span>
                        </div>
                      )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      Thrill:{" "}
                      <span
                        className={
                          record.thrill === "strong"
                            ? "text-green-600"
                            : record.thrill === "weak"
                              ? "text-yellow-600"
                              : "text-gray-400"
                        }
                      >
                        {record.thrill}
                      </span>
                    </div>
                    <div className="text-sm text-gray-900">
                      Bruit:{" "}
                      <span
                        className={
                          record.bruit === "strong"
                            ? "text-green-600"
                            : record.bruit === "weak"
                              ? "text-yellow-600"
                              : "text-gray-400"
                        }
                      >
                        {record.bruit}
                      </span>
                    </div>
                    {record.flowRate && (
                      <div className="text-xs text-gray-500">
                        Flow: {record.flowRate} mL/min
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(record)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      Details
                    </button>
                    <button className="text-gray-600 hover:text-gray-900">
                      Assess
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Detail Modal */}
      <Dialog
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {selectedRecord
                  ? "Vascular Access Details"
                  : "New Vascular Access"}
              </Dialog.Title>

              {selectedRecord && (
                <div className="mt-4 space-y-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Surgeon
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.surgeon || "N/A"}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Access Type
                      </label>
                      <span
                        className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${accessTypeConfig[selectedRecord.accessType].color}`}
                      >
                        {accessTypeConfig[selectedRecord.accessType].label}
                      </span>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Status
                      </label>
                      <span
                        className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[selectedRecord.status].color}`}
                      >
                        {statusConfig[selectedRecord.status].label}
                      </span>
                    </div>
                    <div className="col-span-2">
                      <label className="text-sm font-medium text-gray-500">
                        Location
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.location}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Creation Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {format(
                          new Date(selectedRecord.creationDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Maturity Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.maturityDate
                          ? format(
                              new Date(selectedRecord.maturityDate),
                              "MMMM d, yyyy",
                            )
                          : "Not yet mature"}
                      </p>
                    </div>
                  </div>

                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">
                      Physical Assessment
                    </h4>
                    <div className="grid grid-cols-3 gap-4">
                      <div className="text-center">
                        <p className="text-sm text-gray-500">Thrill</p>
                        <p
                          className={`text-lg font-medium ${selectedRecord.thrill === "strong" ? "text-green-600" : selectedRecord.thrill === "weak" ? "text-yellow-600" : "text-red-600"}`}
                        >
                          {selectedRecord.thrill.charAt(0).toUpperCase() +
                            selectedRecord.thrill.slice(1)}
                        </p>
                      </div>
                      <div className="text-center">
                        <p className="text-sm text-gray-500">Bruit</p>
                        <p
                          className={`text-lg font-medium ${selectedRecord.bruit === "strong" ? "text-green-600" : selectedRecord.bruit === "weak" ? "text-yellow-600" : "text-red-600"}`}
                        >
                          {selectedRecord.bruit.charAt(0).toUpperCase() +
                            selectedRecord.bruit.slice(1)}
                        </p>
                      </div>
                      <div className="text-center">
                        <p className="text-sm text-gray-500">Flow Rate</p>
                        <p className="text-lg font-medium text-gray-900">
                          {selectedRecord.flowRate
                            ? `${selectedRecord.flowRate} mL/min`
                            : "N/A"}
                        </p>
                      </div>
                    </div>
                  </div>

                  {selectedRecord.complications &&
                    selectedRecord.complications.length > 0 && (
                      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                        <h4 className="text-sm font-medium text-red-800">
                          Complications
                        </h4>
                        <ul className="mt-2 list-disc list-inside text-sm text-red-700">
                          {selectedRecord.complications.map((comp, index) => (
                            <li key={index}>{comp}</li>
                          ))}
                        </ul>
                      </div>
                    )}

                  {selectedRecord.interventions &&
                    selectedRecord.interventions.length > 0 && (
                      <div>
                        <h4 className="text-sm font-medium text-gray-900 mb-3">
                          Interventions
                        </h4>
                        <div className="space-y-2">
                          {selectedRecord.interventions.map(
                            (intervention, index) => (
                              <div
                                key={index}
                                className="bg-gray-50 rounded p-3 border"
                              >
                                <div className="flex justify-between">
                                  <span className="font-medium text-gray-900">
                                    {intervention.type}
                                  </span>
                                  <span className="text-sm text-gray-500">
                                    {format(
                                      new Date(intervention.date),
                                      "MMM d, yyyy",
                                    )}
                                  </span>
                                </div>
                                <p className="text-sm text-gray-600 mt-1">
                                  Outcome: {intervention.outcome}
                                </p>
                                <p className="text-xs text-gray-500">
                                  By: {intervention.performedBy}
                                </p>
                              </div>
                            ),
                          )}
                        </div>
                      </div>
                    )}

                  {selectedRecord.notes && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Notes
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.notes}
                      </p>
                    </div>
                  )}
                </div>
              )}

              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                >
                  Close
                </button>
                {selectedRecord && (
                  <button className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700">
                    Record Assessment
                  </button>
                )}
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
}
