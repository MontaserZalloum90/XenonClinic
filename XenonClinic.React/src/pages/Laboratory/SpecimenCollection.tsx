import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  BeakerIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  PrinterIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";

interface Specimen {
  id: number;
  specimenNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  orderId: number;
  orderNumber: string;
  specimenType: string;
  collectionSite: string;
  collectedBy: string;
  collectedAt?: string;
  receivedAt?: string;
  status:
    | "pending"
    | "collected"
    | "received"
    | "processing"
    | "completed"
    | "rejected";
  priority: "routine" | "urgent" | "stat";
  tests: string[];
  notes?: string;
  rejectionReason?: string;
  storageLocation?: string;
  volume?: string;
}

const statusConfig = {
  pending: {
    label: "Pending",
    color: "bg-gray-100 text-gray-800",
    icon: ClockIcon,
  },
  collected: {
    label: "Collected",
    color: "bg-blue-100 text-blue-800",
    icon: CheckCircleIcon,
  },
  received: {
    label: "Received",
    color: "bg-purple-100 text-purple-800",
    icon: CheckCircleIcon,
  },
  processing: {
    label: "Processing",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  completed: {
    label: "Completed",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  rejected: {
    label: "Rejected",
    color: "bg-red-100 text-red-800",
    icon: ExclamationTriangleIcon,
  },
};

const priorityConfig = {
  routine: { label: "Routine", color: "bg-gray-100 text-gray-800" },
  urgent: { label: "Urgent", color: "bg-orange-100 text-orange-800" },
  stat: { label: "STAT", color: "bg-red-100 text-red-800" },
};

export function SpecimenCollection() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<Specimen["status"] | "all">(
    "all",
  );
  const queryClient = useQueryClient();

  const { data: specimens = [], isLoading } = useQuery({
    queryKey: ["specimens"],
    queryFn: () => api.get<Specimen[]>("/api/laboratory/specimens"),
  });

  const collectMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/laboratory/specimens/${id}/collect`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["specimens"] });
    },
  });

  const receiveMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/laboratory/specimens/${id}/receive`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["specimens"] });
    },
  });

  const filteredSpecimens = specimens.filter((spec) => {
    const matchesSearch =
      spec.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      spec.specimenNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      spec.orderNumber.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || spec.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Specimen Collection
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Track and manage specimen collection workflow
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Collection
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {specimens.filter((s) => s.status === "pending").length}
          </div>
          <div className="text-sm text-gray-500">Pending Collection</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {specimens.filter((s) => s.status === "collected").length}
          </div>
          <div className="text-sm text-gray-500">Awaiting Receipt</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {specimens.filter((s) => s.status === "processing").length}
          </div>
          <div className="text-sm text-gray-500">Processing</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {specimens.filter((s) => s.priority === "stat").length}
          </div>
          <div className="text-sm text-gray-500">STAT Priority</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search specimens..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as Specimen["status"] | "all")
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Status</option>
          {Object.entries(statusConfig).map(([key, value]) => (
            <option key={key} value={key}>
              {value.label}
            </option>
          ))}
        </select>
      </div>

      <div className="overflow-hidden rounded-lg bg-white shadow">
        {isLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredSpecimens.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <BeakerIcon className="h-12 w-12 mb-2" />
            <p>No specimens found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Specimen
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type / Site
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Tests
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Priority
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredSpecimens.map((specimen) => {
                const status = statusConfig[specimen.status];
                const priority = priorityConfig[specimen.priority];
                const StatusIcon = status.icon;

                return (
                  <tr key={specimen.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm font-medium text-gray-900">
                        {specimen.specimenNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        Order: {specimen.orderNumber}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {specimen.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {specimen.patientMRN}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">
                        {specimen.specimenType}
                      </div>
                      <div className="text-xs text-gray-500">
                        {specimen.collectionSite}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-1">
                        {specimen.tests.slice(0, 2).map((test, i) => (
                          <span
                            key={i}
                            className="inline-flex rounded bg-gray-100 px-2 py-0.5 text-xs text-gray-700"
                          >
                            {test}
                          </span>
                        ))}
                        {specimen.tests.length > 2 && (
                          <span className="text-xs text-gray-500">
                            +{specimen.tests.length - 2} more
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${priority.color}`}
                      >
                        {priority.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        <StatusIcon className="h-3.5 w-3.5 mr-1" />
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button className="text-gray-600 hover:text-gray-900">
                          <PrinterIcon className="h-5 w-5" />
                        </button>
                        {specimen.status === "pending" && (
                          <button
                            onClick={() => collectMutation.mutate(specimen.id)}
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Collect
                          </button>
                        )}
                        {specimen.status === "collected" && (
                          <button
                            onClick={() => receiveMutation.mutate(specimen.id)}
                            className="rounded bg-purple-600 px-3 py-1 text-xs font-medium text-white hover:bg-purple-700"
                          >
                            Receive
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
