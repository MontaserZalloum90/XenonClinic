import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  BeakerIcon,
  CheckCircleIcon,
  ArchiveBoxIcon,
  ArrowPathIcon,
  ExclamationCircleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import api from "../../lib/api";

interface EmbryoRecord {
  id: number;
  cycleId: number;
  patientId: number;
  patientName: string;
  embryoNumber: number;
  fertilizationDate: string;
  day: number;
  grade: string;
  cellCount?: number;
  fragmentation?: string;
  status: "developing" | "transferred" | "frozen" | "discarded" | "thawed";
  cryopreservationDate?: string;
  tankLocation?: string;
  canisterPosition?: string;
  thawDate?: string;
  survivalPostThaw?: boolean;
  transferDate?: string;
  notes?: string;
  embryologist: string;
}

const statusConfig = {
  developing: {
    label: "Developing",
    color: "bg-blue-100 text-blue-800",
    icon: ArrowPathIcon,
  },
  transferred: {
    label: "Transferred",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  frozen: {
    label: "Frozen",
    color: "bg-cyan-100 text-cyan-800",
    icon: ArchiveBoxIcon,
  },
  discarded: {
    label: "Discarded",
    color: "bg-gray-100 text-gray-800",
    icon: ExclamationCircleIcon,
  },
  thawed: {
    label: "Thawed",
    color: "bg-yellow-100 text-yellow-800",
    icon: BeakerIcon,
  },
};

const gradeDescriptions: { [key: string]: string } = {
  AA: "Excellent - Top quality blastocyst",
  AB: "Good - High quality blastocyst",
  BA: "Good - High quality blastocyst",
  BB: "Fair - Average quality blastocyst",
  BC: "Poor - Below average quality",
  CB: "Poor - Below average quality",
  CC: "Poor - Low quality blastocyst",
};

const mockRecords: EmbryoRecord[] = [
  {
    id: 1,
    cycleId: 1,
    patientId: 101,
    patientName: "Jennifer Davis",
    embryoNumber: 1,
    fertilizationDate: "2024-01-15",
    day: 5,
    grade: "AA",
    cellCount: undefined,
    fragmentation: "none",
    status: "developing",
    embryologist: "Dr. Lisa Wang",
    notes: "Excellent development. Candidate for transfer.",
  },
  {
    id: 2,
    cycleId: 1,
    patientId: 101,
    patientName: "Jennifer Davis",
    embryoNumber: 2,
    fertilizationDate: "2024-01-15",
    day: 5,
    grade: "AB",
    fragmentation: "<10%",
    status: "developing",
    embryologist: "Dr. Lisa Wang",
  },
  {
    id: 3,
    cycleId: 1,
    patientId: 101,
    patientName: "Jennifer Davis",
    embryoNumber: 3,
    fertilizationDate: "2024-01-15",
    day: 5,
    grade: "BB",
    fragmentation: "10-15%",
    status: "frozen",
    cryopreservationDate: "2024-01-20",
    tankLocation: "Tank A",
    canisterPosition: "C1-S3",
    embryologist: "Dr. Lisa Wang",
  },
  {
    id: 4,
    cycleId: 2,
    patientId: 102,
    patientName: "Amanda Wilson",
    embryoNumber: 1,
    fertilizationDate: "2023-12-29",
    day: 5,
    grade: "AA",
    status: "transferred",
    transferDate: "2024-01-03",
    embryologist: "Dr. Lisa Wang",
    notes: "Single embryo transfer performed.",
  },
  {
    id: 5,
    cycleId: 2,
    patientId: 102,
    patientName: "Amanda Wilson",
    embryoNumber: 2,
    fertilizationDate: "2023-12-29",
    day: 5,
    grade: "AB",
    status: "transferred",
    transferDate: "2024-01-03",
    embryologist: "Dr. Lisa Wang",
  },
  {
    id: 6,
    cycleId: 2,
    patientId: 102,
    patientName: "Amanda Wilson",
    embryoNumber: 3,
    fertilizationDate: "2023-12-29",
    day: 5,
    grade: "BA",
    status: "frozen",
    cryopreservationDate: "2024-01-03",
    tankLocation: "Tank B",
    canisterPosition: "C2-S1",
    embryologist: "Dr. Lisa Wang",
  },
  {
    id: 7,
    cycleId: 3,
    patientId: 103,
    patientName: "Michelle Thompson",
    embryoNumber: 1,
    fertilizationDate: "2023-12-03",
    day: 6,
    grade: "AA",
    status: "thawed",
    cryopreservationDate: "2023-12-08",
    thawDate: "2024-01-08",
    survivalPostThaw: true,
    transferDate: "2024-01-08",
    embryologist: "Dr. Lisa Wang",
    notes:
      "Successfully thawed and transferred. Resulted in positive pregnancy.",
  },
  {
    id: 8,
    cycleId: 3,
    patientId: 103,
    patientName: "Michelle Thompson",
    embryoNumber: 2,
    fertilizationDate: "2023-12-03",
    day: 3,
    grade: "CC",
    cellCount: 4,
    fragmentation: ">25%",
    status: "discarded",
    embryologist: "Dr. Lisa Wang",
    notes: "Poor development. Arrested at 4-cell stage.",
  },
];

export function EmbryoRecords() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedEmbryo, setSelectedEmbryo] = useState<EmbryoRecord | null>(
    null,
  );

  const { data: embryos = mockRecords, isLoading } = useQuery({
    queryKey: ["embryo-records", searchTerm, statusFilter],
    queryFn: async () => {
      const response = await api.get("/api/fertility/embryo-records", {
        params: {
          search: searchTerm,
          status: statusFilter !== "all" ? statusFilter : undefined,
        },
      });
      return response.data;
    },
    placeholderData: mockRecords,
  });

  const filteredEmbryos = embryos.filter((embryo: EmbryoRecord) => {
    const matchesSearch = embryo.patientName
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || embryo.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const stats = {
    developing: embryos.filter((e: EmbryoRecord) => e.status === "developing")
      .length,
    frozen: embryos.filter((e: EmbryoRecord) => e.status === "frozen").length,
    transferred: embryos.filter((e: EmbryoRecord) => e.status === "transferred")
      .length,
    highQuality: embryos.filter(
      (e: EmbryoRecord) =>
        e.grade.includes("A") &&
        (e.status === "developing" || e.status === "frozen"),
    ).length,
  };

  const handleViewDetails = (embryo: EmbryoRecord) => {
    setSelectedEmbryo(embryo);
    setIsModalOpen(true);
  };

  const getGradeColor = (grade: string): string => {
    if (grade === "AA") return "bg-green-100 text-green-800";
    if (grade.includes("A")) return "bg-blue-100 text-blue-800";
    if (grade === "BB") return "bg-yellow-100 text-yellow-800";
    return "bg-gray-100 text-gray-800";
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
            Embryo Records
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track embryo development, grading, and cryopreservation
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Record
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <ArrowPathIcon className="h-6 w-6 text-blue-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Developing
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.developing}
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
                <ArchiveBoxIcon className="h-6 w-6 text-cyan-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Frozen
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.frozen}
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
                <CheckCircleIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Transferred
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.transferred}
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
                <BeakerIcon className="h-6 w-6 text-purple-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    High Quality
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.highQuality}
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
                placeholder="Search by patient name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              />
            </div>
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="all">All Status</option>
            <option value="developing">Developing</option>
            <option value="frozen">Frozen</option>
            <option value="transferred">Transferred</option>
            <option value="thawed">Thawed</option>
            <option value="discarded">Discarded</option>
          </select>
        </div>
      </div>

      {/* Embryos List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient / Cycle
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Embryo #
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Day / Grade
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Location
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredEmbryos.map((embryo: EmbryoRecord) => {
              const StatusIcon = statusConfig[embryo.status].icon;
              return (
                <tr key={embryo.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {embryo.patientName}
                    </div>
                    <div className="text-sm text-gray-500">
                      Cycle #{embryo.cycleId}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      Embryo #{embryo.embryoNumber}
                    </div>
                    <div className="text-sm text-gray-500">
                      {format(
                        new Date(embryo.fertilizationDate),
                        "MMM d, yyyy",
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      Day {embryo.day}
                    </div>
                    <span
                      className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${getGradeColor(embryo.grade)}`}
                    >
                      Grade {embryo.grade}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[embryo.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[embryo.status].label}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {embryo.status === "frozen" && embryo.tankLocation ? (
                      <div className="text-sm">
                        <div className="text-gray-900">
                          {embryo.tankLocation}
                        </div>
                        <div className="text-gray-500">
                          {embryo.canisterPosition}
                        </div>
                      </div>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(embryo)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      View
                    </button>
                    {embryo.status === "developing" && (
                      <button className="text-gray-600 hover:text-gray-900">
                        Update
                      </button>
                    )}
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
          <Dialog.Panel className="mx-auto max-w-xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {selectedEmbryo ? "Embryo Details" : "New Embryo Record"}
              </Dialog.Title>

              {selectedEmbryo && (
                <div className="mt-4 space-y-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedEmbryo.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Cycle
                      </label>
                      <p className="text-sm text-gray-900">
                        #{selectedEmbryo.cycleId}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Embryo Number
                      </label>
                      <p className="text-sm text-gray-900">
                        #{selectedEmbryo.embryoNumber}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Embryologist
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedEmbryo.embryologist}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Fertilization Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {format(
                          new Date(selectedEmbryo.fertilizationDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Day of Development
                      </label>
                      <p className="text-sm text-gray-900">
                        Day {selectedEmbryo.day}
                      </p>
                    </div>
                  </div>

                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">
                      Embryo Assessment
                    </h4>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="text-sm font-medium text-gray-500">
                          Grade
                        </label>
                        <div className="mt-1">
                          <span
                            className={`inline-flex px-3 py-1 rounded text-sm font-medium ${getGradeColor(selectedEmbryo.grade)}`}
                          >
                            {selectedEmbryo.grade}
                          </span>
                          <p className="text-xs text-gray-500 mt-1">
                            {gradeDescriptions[selectedEmbryo.grade] ||
                              "Standard grade"}
                          </p>
                        </div>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">
                          Status
                        </label>
                        <div className="mt-1">
                          <span
                            className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[selectedEmbryo.status].color}`}
                          >
                            {statusConfig[selectedEmbryo.status].label}
                          </span>
                        </div>
                      </div>
                      {selectedEmbryo.cellCount && (
                        <div>
                          <label className="text-sm font-medium text-gray-500">
                            Cell Count
                          </label>
                          <p className="text-sm text-gray-900">
                            {selectedEmbryo.cellCount} cells
                          </p>
                        </div>
                      )}
                      {selectedEmbryo.fragmentation && (
                        <div>
                          <label className="text-sm font-medium text-gray-500">
                            Fragmentation
                          </label>
                          <p className="text-sm text-gray-900">
                            {selectedEmbryo.fragmentation}
                          </p>
                        </div>
                      )}
                    </div>
                  </div>

                  {selectedEmbryo.status === "frozen" && (
                    <div className="bg-cyan-50 border border-cyan-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-cyan-800 mb-3">
                        Cryopreservation Details
                      </h4>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-gray-500">Freeze Date:</span>
                          <span className="ml-2">
                            {selectedEmbryo.cryopreservationDate &&
                              format(
                                new Date(selectedEmbryo.cryopreservationDate),
                                "MMM d, yyyy",
                              )}
                          </span>
                        </div>
                        <div>
                          <span className="text-gray-500">Tank:</span>
                          <span className="ml-2">
                            {selectedEmbryo.tankLocation}
                          </span>
                        </div>
                        <div>
                          <span className="text-gray-500">Position:</span>
                          <span className="ml-2">
                            {selectedEmbryo.canisterPosition}
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  {selectedEmbryo.status === "thawed" && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-yellow-800 mb-3">
                        Thaw Details
                      </h4>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-gray-500">Thaw Date:</span>
                          <span className="ml-2">
                            {selectedEmbryo.thawDate &&
                              format(
                                new Date(selectedEmbryo.thawDate),
                                "MMM d, yyyy",
                              )}
                          </span>
                        </div>
                        <div>
                          <span className="text-gray-500">Survival:</span>
                          <span
                            className={`ml-2 font-medium ${selectedEmbryo.survivalPostThaw ? "text-green-600" : "text-red-600"}`}
                          >
                            {selectedEmbryo.survivalPostThaw
                              ? "Survived"
                              : "Did not survive"}
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  {selectedEmbryo.transferDate && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-green-800">
                        Transfer
                      </h4>
                      <p className="text-sm text-green-700 mt-1">
                        Transferred on{" "}
                        {format(
                          new Date(selectedEmbryo.transferDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                  )}

                  {selectedEmbryo.notes && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Notes
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedEmbryo.notes}
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
                {selectedEmbryo && selectedEmbryo.status === "developing" && (
                  <button className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700">
                    Update Grade
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
