import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  PlusIcon,
  MagnifyingGlassIcon,
  HeartIcon,
  ClockIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
  BeakerIcon,
  CalendarIcon,
} from "@heroicons/react/24/outline";
import { format, differenceInDays } from "date-fns";
import { ivfCyclesApi } from "../../lib/api";

interface IVFCycle {
  id: number;
  patientId: number;
  patientName: string;
  cycleNumber: number;
  startDate: string;
  protocol: string;
  status:
    | "planning"
    | "stimulation"
    | "retrieval"
    | "fertilization"
    | "transfer"
    | "waiting_result"
    | "positive"
    | "negative"
    | "cancelled";
  stimulationDays?: number;
  triggerDate?: string;
  retrievalDate?: string;
  oocytesRetrieved?: number;
  matureOocytes?: number;
  fertilized?: number;
  embryosTransferred?: number;
  transferDate?: string;
  betaHCGDate?: string;
  betaHCGResult?: number;
  managedBy: string;
  notes?: string;
  medications?: Medication[];
}

interface Medication {
  name: string;
  dose: string;
  startDay: number;
  endDay?: number;
}

const statusConfig = {
  planning: {
    label: "Planning",
    color: "bg-gray-100 text-gray-800",
    icon: CalendarIcon,
  },
  stimulation: {
    label: "Stimulation",
    color: "bg-blue-100 text-blue-800",
    icon: BeakerIcon,
  },
  retrieval: {
    label: "Retrieval",
    color: "bg-purple-100 text-purple-800",
    icon: BeakerIcon,
  },
  fertilization: {
    label: "Fertilization",
    color: "bg-indigo-100 text-indigo-800",
    icon: HeartIcon,
  },
  transfer: {
    label: "Transfer",
    color: "bg-pink-100 text-pink-800",
    icon: HeartIcon,
  },
  waiting_result: {
    label: "Waiting Result",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  positive: {
    label: "Positive",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  negative: {
    label: "Negative",
    color: "bg-red-100 text-red-800",
    icon: ExclamationCircleIcon,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-gray-100 text-gray-800",
    icon: ExclamationCircleIcon,
  },
};

const protocolConfig: { [key: string]: string } = {
  long_agonist: "Long Agonist Protocol",
  short_antagonist: "Short Antagonist Protocol",
  mini_ivf: "Mini IVF",
  natural_cycle: "Natural Cycle IVF",
  freeze_all: "Freeze All",
  donor_egg: "Donor Egg",
};

export function IVFCycles() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCycle, setSelectedCycle] = useState<IVFCycle | null>(null);

  const { data: cycles = [], isLoading } = useQuery({
    queryKey: ["ivf-cycles", searchTerm, statusFilter],
    queryFn: async () => {
      const response = await ivfCyclesApi.getAll();
      return response.data;
    },
  });

  const filteredCycles = cycles.filter((cycle: IVFCycle) => {
    const matchesSearch = cycle.patientName
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || cycle.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const stats = {
    active: cycles.filter((c: IVFCycle) =>
      [
        "planning",
        "stimulation",
        "retrieval",
        "fertilization",
        "transfer",
        "waiting_result",
      ].includes(c.status),
    ).length,
    positive: cycles.filter((c: IVFCycle) => c.status === "positive").length,
    retrievalsThisMonth: cycles.filter(
      (c: IVFCycle) =>
        c.retrievalDate &&
        differenceInDays(new Date(), new Date(c.retrievalDate)) <= 30,
    ).length,
    transfersThisMonth: cycles.filter(
      (c: IVFCycle) =>
        c.transferDate &&
        differenceInDays(new Date(), new Date(c.transferDate)) <= 30,
    ).length,
  };

  const handleViewDetails = (cycle: IVFCycle) => {
    setSelectedCycle(cycle);
    setIsModalOpen(true);
  };

  const getCycleDay = (startDate: string): number => {
    return differenceInDays(new Date(), new Date(startDate)) + 1;
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
          <h1 className="text-2xl font-semibold text-gray-900">IVF Cycles</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage IVF treatment cycles and monitor progress
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Cycle
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <HeartIcon className="h-6 w-6 text-pink-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Active Cycles
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
                <CheckCircleIcon className="h-6 w-6 text-green-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Positive Results
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.positive}
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
                    Retrievals (30d)
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.retrievalsThisMonth}
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
                <CalendarIcon className="h-6 w-6 text-blue-600" />
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Transfers (30d)
                  </dt>
                  <dd className="text-lg font-semibold text-gray-900">
                    {stats.transfersThisMonth}
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
            <option value="planning">Planning</option>
            <option value="stimulation">Stimulation</option>
            <option value="retrieval">Retrieval</option>
            <option value="fertilization">Fertilization</option>
            <option value="transfer">Transfer</option>
            <option value="waiting_result">Waiting Result</option>
            <option value="positive">Positive</option>
            <option value="negative">Negative</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      {/* Cycles List */}
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Cycle / Protocol
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Progress
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Outcome
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredCycles.map((cycle: IVFCycle) => {
              const StatusIcon = statusConfig[cycle.status].icon;
              return (
                <tr key={cycle.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {cycle.patientName}
                    </div>
                    <div className="text-sm text-gray-500">
                      {cycle.managedBy}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">
                      Cycle #{cycle.cycleNumber}
                    </div>
                    <div className="text-sm text-gray-500">
                      {protocolConfig[cycle.protocol] || cycle.protocol}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig[cycle.status].color}`}
                    >
                      <StatusIcon className="h-3 w-3 mr-1" />
                      {statusConfig[cycle.status].label}
                    </span>
                    <div className="text-xs text-gray-500 mt-1">
                      Day {getCycleDay(cycle.startDate)}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm">
                      {cycle.oocytesRetrieved !== undefined && (
                        <div className="text-gray-600">
                          Oocytes:{" "}
                          <span className="font-medium">
                            {cycle.oocytesRetrieved}
                          </span>
                          {cycle.matureOocytes !== undefined &&
                            ` (${cycle.matureOocytes} mature)`}
                        </div>
                      )}
                      {cycle.fertilized !== undefined && (
                        <div className="text-gray-600">
                          Fertilized:{" "}
                          <span className="font-medium">
                            {cycle.fertilized}
                          </span>
                        </div>
                      )}
                      {cycle.embryosTransferred !== undefined && (
                        <div className="text-gray-600">
                          Transferred:{" "}
                          <span className="font-medium">
                            {cycle.embryosTransferred}
                          </span>
                        </div>
                      )}
                      {!cycle.oocytesRetrieved && !cycle.fertilized && (
                        <span className="text-gray-400">In progress</span>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {cycle.betaHCGResult !== undefined ? (
                      <div>
                        <div
                          className={`text-sm font-medium ${cycle.status === "positive" ? "text-green-600" : "text-red-600"}`}
                        >
                          {cycle.status === "positive"
                            ? "Positive"
                            : "Negative"}
                        </div>
                        <div className="text-xs text-gray-500">
                          Beta: {cycle.betaHCGResult} mIU/mL
                        </div>
                      </div>
                    ) : cycle.betaHCGDate ? (
                      <div className="text-sm text-gray-500">
                        Beta: {format(new Date(cycle.betaHCGDate), "MMM d")}
                      </div>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => handleViewDetails(cycle)}
                      className="text-indigo-600 hover:text-indigo-900 mr-3"
                    >
                      View
                    </button>
                    <button className="text-gray-600 hover:text-gray-900">
                      Update
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
                {selectedCycle ? "IVF Cycle Details" : "New IVF Cycle"}
              </Dialog.Title>

              {selectedCycle && (
                <div className="mt-4 space-y-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Patient
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedCycle.patientName}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Managed By
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedCycle.managedBy}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Cycle Number
                      </label>
                      <p className="text-sm text-gray-900">
                        #{selectedCycle.cycleNumber}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Protocol
                      </label>
                      <p className="text-sm text-gray-900">
                        {protocolConfig[selectedCycle.protocol] ||
                          selectedCycle.protocol}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Start Date
                      </label>
                      <p className="text-sm text-gray-900">
                        {format(
                          new Date(selectedCycle.startDate),
                          "MMMM d, yyyy",
                        )}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Current Day
                      </label>
                      <p className="text-sm text-gray-900">
                        Day {getCycleDay(selectedCycle.startDate)}
                      </p>
                    </div>
                  </div>

                  {/* Timeline */}
                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">
                      Cycle Timeline
                    </h4>
                    <div className="grid grid-cols-2 gap-3 text-sm">
                      {selectedCycle.stimulationDays && (
                        <div>
                          <span className="text-gray-500">
                            Stimulation Days:
                          </span>
                          <span className="ml-2 font-medium">
                            {selectedCycle.stimulationDays}
                          </span>
                        </div>
                      )}
                      {selectedCycle.triggerDate && (
                        <div>
                          <span className="text-gray-500">Trigger:</span>
                          <span className="ml-2">
                            {format(
                              new Date(selectedCycle.triggerDate),
                              "MMM d",
                            )}
                          </span>
                        </div>
                      )}
                      {selectedCycle.retrievalDate && (
                        <div>
                          <span className="text-gray-500">Retrieval:</span>
                          <span className="ml-2">
                            {format(
                              new Date(selectedCycle.retrievalDate),
                              "MMM d",
                            )}
                          </span>
                        </div>
                      )}
                      {selectedCycle.transferDate && (
                        <div>
                          <span className="text-gray-500">Transfer:</span>
                          <span className="ml-2">
                            {format(
                              new Date(selectedCycle.transferDate),
                              "MMM d",
                            )}
                          </span>
                        </div>
                      )}
                      {selectedCycle.betaHCGDate && (
                        <div>
                          <span className="text-gray-500">Beta HCG:</span>
                          <span className="ml-2">
                            {format(
                              new Date(selectedCycle.betaHCGDate),
                              "MMM d",
                            )}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Outcome Summary */}
                  {(selectedCycle.oocytesRetrieved !== undefined ||
                    selectedCycle.fertilized !== undefined) && (
                    <div className="grid grid-cols-4 gap-4 text-center">
                      {selectedCycle.oocytesRetrieved !== undefined && (
                        <div className="bg-purple-50 rounded-lg p-3">
                          <p className="text-2xl font-bold text-purple-600">
                            {selectedCycle.oocytesRetrieved}
                          </p>
                          <p className="text-xs text-gray-500">Oocytes</p>
                        </div>
                      )}
                      {selectedCycle.matureOocytes !== undefined && (
                        <div className="bg-blue-50 rounded-lg p-3">
                          <p className="text-2xl font-bold text-blue-600">
                            {selectedCycle.matureOocytes}
                          </p>
                          <p className="text-xs text-gray-500">Mature</p>
                        </div>
                      )}
                      {selectedCycle.fertilized !== undefined && (
                        <div className="bg-indigo-50 rounded-lg p-3">
                          <p className="text-2xl font-bold text-indigo-600">
                            {selectedCycle.fertilized}
                          </p>
                          <p className="text-xs text-gray-500">Fertilized</p>
                        </div>
                      )}
                      {selectedCycle.embryosTransferred !== undefined && (
                        <div className="bg-pink-50 rounded-lg p-3">
                          <p className="text-2xl font-bold text-pink-600">
                            {selectedCycle.embryosTransferred}
                          </p>
                          <p className="text-xs text-gray-500">Transferred</p>
                        </div>
                      )}
                    </div>
                  )}

                  {selectedCycle.betaHCGResult !== undefined && (
                    <div
                      className={`rounded-lg p-4 ${selectedCycle.status === "positive" ? "bg-green-50 border border-green-200" : "bg-red-50 border border-red-200"}`}
                    >
                      <h4
                        className={`text-sm font-medium ${selectedCycle.status === "positive" ? "text-green-800" : "text-red-800"}`}
                      >
                        Beta HCG Result
                      </h4>
                      <p
                        className={`text-2xl font-bold mt-1 ${selectedCycle.status === "positive" ? "text-green-600" : "text-red-600"}`}
                      >
                        {selectedCycle.betaHCGResult} mIU/mL
                      </p>
                    </div>
                  )}

                  {selectedCycle.medications &&
                    selectedCycle.medications.length > 0 && (
                      <div>
                        <h4 className="text-sm font-medium text-gray-900 mb-3">
                          Medications
                        </h4>
                        <div className="space-y-2">
                          {selectedCycle.medications.map((med, index) => (
                            <div
                              key={index}
                              className="flex justify-between bg-gray-50 p-2 rounded"
                            >
                              <span className="font-medium">{med.name}</span>
                              <span className="text-gray-600">
                                {med.dose} (Day {med.startDay}
                                {med.endDay ? ` - ${med.endDay}` : "+"})
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                  {selectedCycle.notes && (
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Notes
                      </label>
                      <p className="text-sm text-gray-900">
                        {selectedCycle.notes}
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
                {selectedCycle && (
                  <button className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700">
                    Add Monitoring
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
