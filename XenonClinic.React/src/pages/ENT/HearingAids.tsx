import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  SpeakerWaveIcon,
  WrenchScrewdriverIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  CalendarIcon,
} from "@heroicons/react/24/outline";
import { format, differenceInDays } from "date-fns";
import { entHearingAidsApi } from "../../lib/api";

interface HearingAidRecord {
  id: number;
  patientId: number;
  patientName: string;
  ear: "left" | "right" | "binaural";
  manufacturer: string;
  model: string;
  serialNumber: string;
  type: "bte" | "ite" | "itc" | "cic" | "ric" | "baha";
  fittingDate: string;
  warrantyExpiry: string;
  status: "active" | "repair" | "replaced" | "returned";
  programmingSettings: ProgrammingSettings;
  adjustments?: Adjustment[];
  nextAppointment?: string;
  notes?: string;
  fittedBy: string;
}

interface ProgrammingSettings {
  programCount: number;
  primaryProgram: string;
  feedbackManagement: "on" | "off";
  noiseReduction: "low" | "medium" | "high";
  directionality: "omnidirectional" | "adaptive" | "fixed";
}

interface Adjustment {
  date: string;
  type: string;
  changes: string;
  performedBy: string;
}

const typeConfig = {
  bte: { label: "Behind-The-Ear (BTE)", color: "bg-blue-100 text-blue-800" },
  ite: { label: "In-The-Ear (ITE)", color: "bg-green-100 text-green-800" },
  itc: { label: "In-The-Canal (ITC)", color: "bg-purple-100 text-purple-800" },
  cic: {
    label: "Completely-In-Canal (CIC)",
    color: "bg-orange-100 text-orange-800",
  },
  ric: { label: "Receiver-In-Canal (RIC)", color: "bg-teal-100 text-teal-800" },
  baha: { label: "Bone-Anchored (BAHA)", color: "bg-red-100 text-red-800" },
};

const statusConfig = {
  active: {
    label: "Active",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  repair: {
    label: "In Repair",
    color: "bg-yellow-100 text-yellow-800",
    icon: WrenchScrewdriverIcon,
  },
  replaced: {
    label: "Replaced",
    color: "bg-gray-100 text-gray-800",
    icon: SpeakerWaveIcon,
  },
  returned: {
    label: "Returned",
    color: "bg-red-100 text-red-800",
    icon: ExclamationTriangleIcon,
  },
};

export function HearingAids() {
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<HearingAidRecord | null>(
    null,
  );

  const { data: records = [], isLoading } = useQuery({
    queryKey: ["hearing-aids", searchTerm, typeFilter, statusFilter],
    queryFn: async () => {
      const response = await entHearingAidsApi.getAll();
      return response.data;
    },
  });

  const filteredRecords = records.filter((record: HearingAidRecord) => {
    const matchesSearch =
      record.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.manufacturer.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.model.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = typeFilter === "all" || record.type === typeFilter;
    const matchesStatus =
      statusFilter === "all" || record.status === statusFilter;
    return matchesSearch && matchesType && matchesStatus;
  });

  const stats = {
    active: records.filter((r: HearingAidRecord) => r.status === "active")
      .length,
    inRepair: records.filter((r: HearingAidRecord) => r.status === "repair")
      .length,
    binaural: records.filter((r: HearingAidRecord) => r.ear === "binaural")
      .length,
    expiringWarranty: records.filter((r: HearingAidRecord) => {
      const daysToExpiry = differenceInDays(
        new Date(r.warrantyExpiry),
        new Date(),
      );
      return daysToExpiry > 0 && daysToExpiry <= 90;
    }).length,
  };

  const handleViewDetails = (record: HearingAidRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  const getWarrantyStatus = (
    expiryDate: string,
  ): { label: string; color: string } => {
    const daysToExpiry = differenceInDays(new Date(expiryDate), new Date());
    if (daysToExpiry < 0) return { label: "Expired", color: "text-red-600" };
    if (daysToExpiry <= 90)
      return { label: `${daysToExpiry} days left`, color: "text-orange-600" };
    return { label: "Valid", color: "text-green-600" };
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
            Hearing Aid Management
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track fittings, adjustments, and device maintenance
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Fitting
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <SpeakerWaveIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Active Devices
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.active}
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
                <WrenchScrewdriverIcon className="h-6 w-6 text-yellow-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    In Repair
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.inRepair}
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
                    Binaural Fittings
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.binaural}
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
                <CalendarIcon className="h-6 w-6 text-orange-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Warranty Expiring
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.expiringWarranty}
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
                placeholder="Search by patient, manufacturer, or model..."
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
            <option value="bte">BTE</option>
            <option value="ite">ITE</option>
            <option value="itc">ITC</option>
            <option value="cic">CIC</option>
            <option value="ric">RIC</option>
            <option value="baha">BAHA</option>
          </select>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Status</option>
            <option value="active">Active</option>
            <option value="repair">In Repair</option>
            <option value="replaced">Replaced</option>
            <option value="returned">Returned</option>
          </select>
        </div>
      </div>

      {/* Records List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Device
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Type / Ear
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Warranty
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredRecords.map((record: HearingAidRecord) => {
              const StatusIcon = statusConfig[record.status].icon;
              const warrantyStatus = getWarrantyStatus(record.warrantyExpiry);
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
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">
                      {record.manufacturer} {record.model}
                    </div>
                    <div className="text-sm text-gray-500">
                      S/N: {record.serialNumber}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${typeConfig[record.type].color}`}
                    >
                      {record.type.toUpperCase()}
                    </span>
                    <div className="text-sm text-gray-500 mt-1 capitalize">
                      {record.ear}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[record.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[record.status].label}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div
                      className={`text-sm font-medium ${warrantyStatus.color}`}
                    >
                      {warrantyStatus.label}
                    </div>
                    <div className="text-xs text-gray-500">
                      Exp:{" "}
                      {format(new Date(record.warrantyExpiry), "MMM d, yyyy")}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(record)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      Details
                    </button>
                    <button className="text-gray-600 hover:text-gray-900">
                      Adjust
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
                  ? "Hearing Aid Details"
                  : "New Hearing Aid Fitting"}
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
                        Fitted By
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.fittedBy}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Device
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.manufacturer} {selectedRecord.model}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Serial Number
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedRecord.serialNumber}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Type
                      </label>
                      <span
                        className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${typeConfig[selectedRecord.type].color}`}
                      >
                        {typeConfig[selectedRecord.type].label}
                      </span>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Ear
                      </label>
                      <p className="text-sm text-gray-900 capitalize">
                        {selectedRecord.ear}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Fitting Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {format(
                          new Date(selectedRecord.fittingDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Warranty Expiry
                      </label>
                      <p
                        className={`text-sm font-medium ${getWarrantyStatus(selectedRecord.warrantyExpiry).color}`}
                      >
                        {format(
                          new Date(selectedRecord.warrantyExpiry),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                  </div>

                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">
                      Programming Settings
                    </h4>
                    <div className="grid grid-cols-2 gap-3 text-sm">
                      <div>
                        <span className="text-gray-500">Programs:</span>
                        <span className="ml-2">
                          {selectedRecord.programmingSettings.programCount}
                        </span>
                      </div>
                      <div>
                        <span className="text-gray-500">Primary:</span>
                        <span className="ml-2">
                          {selectedRecord.programmingSettings.primaryProgram}
                        </span>
                      </div>
                      <div>
                        <span className="text-gray-500">Noise Reduction:</span>
                        <span className="ml-2 capitalize">
                          {selectedRecord.programmingSettings.noiseReduction}
                        </span>
                      </div>
                      <div>
                        <span className="text-gray-500">Directionality:</span>
                        <span className="ml-2 capitalize">
                          {selectedRecord.programmingSettings.directionality}
                        </span>
                      </div>
                      <div>
                        <span className="text-gray-500">
                          Feedback Management:
                        </span>
                        <span className="ml-2 capitalize">
                          {
                            selectedRecord.programmingSettings
                              .feedbackManagement
                          }
                        </span>
                      </div>
                    </div>
                  </div>

                  {selectedRecord.adjustments &&
                    selectedRecord.adjustments.length > 0 && (
                      <div>
                        <h4 className="text-sm font-medium text-gray-900 mb-3">
                          Adjustment History
                        </h4>
                        <div className="space-y-2">
                          {selectedRecord.adjustments.map((adj, index) => (
                            <div
                              key={index}
                              className="bg-gray-50 rounded p-3 border"
                            >
                              <div className="flex justify-between">
                                <span className="font-medium text-gray-900">
                                  {adj.type}
                                </span>
                                <span className="text-sm text-gray-500">
                                  {format(new Date(adj.date), "MMM d, yyyy")}
                                </span>
                              </div>
                              <p className="text-sm text-gray-600 mt-1">
                                {adj.changes}
                              </p>
                              <p className="text-xs text-gray-500">
                                By: {adj.performedBy}
                              </p>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                  {selectedRecord.nextAppointment && (
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-blue-800">
                        Next Appointment
                      </h4>
                      <p className="text-sm text-blue-700 mt-1">
                        {format(
                          new Date(selectedRecord.nextAppointment),
                          "MMMM d, yyyy",
                        )}
                      </p>
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
                    Record Adjustment
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
