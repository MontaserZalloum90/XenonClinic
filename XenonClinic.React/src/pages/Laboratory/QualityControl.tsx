import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ChartBarIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  XCircleIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";

interface QCResult {
  id: number;
  testName: string;
  testCode: string;
  instrumentId: number;
  instrumentName: string;
  controlLevel: "low" | "normal" | "high";
  lotNumber: string;
  expirationDate: string;
  targetValue: number;
  targetSD: number;
  measuredValue: number;
  sdFromTarget: number;
  status: "accepted" | "warning" | "rejected" | "pending-review";
  performedBy: string;
  performedAt: string;
  reviewedBy?: string;
  reviewedAt?: string;
  comments?: string;
  correctiveAction?: string;
}

const statusConfig = {
  accepted: {
    label: "Accepted",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  warning: {
    label: "Warning",
    color: "bg-yellow-100 text-yellow-800",
    icon: ExclamationTriangleIcon,
  },
  rejected: {
    label: "Rejected",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
  "pending-review": {
    label: "Pending Review",
    color: "bg-gray-100 text-gray-800",
    icon: ExclamationTriangleIcon,
  },
};

const controlLevelConfig = {
  low: { label: "Low", color: "bg-blue-100 text-blue-800" },
  normal: { label: "Normal", color: "bg-green-100 text-green-800" },
  high: { label: "High", color: "bg-orange-100 text-orange-800" },
};

export function QualityControl() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<QCResult["status"] | "all">(
    "all",
  );
  const [dateFilter, setDateFilter] = useState(
    new Date().toISOString().split("T")[0],
  );
  const queryClient = useQueryClient();

  const { data: qcResults = [], isLoading } = useQuery({
    queryKey: ["qc-results", dateFilter],
    queryFn: () => api.get<QCResult[]>(`/api/laboratory/qc?date=${dateFilter}`),
  });

  const reviewMutation = useMutation({
    mutationFn: ({
      id,
      status,
      comments,
    }: {
      id: number;
      status: string;
      comments?: string;
    }) => api.post(`/api/laboratory/qc/${id}/review`, { status, comments }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["qc-results"] });
    },
  });

  const filteredResults = qcResults.filter((result) => {
    const matchesSearch =
      result.testName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      result.instrumentName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || result.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getSDColor = (sd: number) => {
    const absSD = Math.abs(sd);
    if (absSD <= 1) return "text-green-600";
    if (absSD <= 2) return "text-yellow-600";
    return "text-red-600";
  };

  const acceptedCount = qcResults.filter((q) => q.status === "accepted").length;
  const warningCount = qcResults.filter((q) => q.status === "warning").length;
  const rejectedCount = qcResults.filter((q) => q.status === "rejected").length;
  const pendingCount = qcResults.filter(
    (q) => q.status === "pending-review",
  ).length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quality Control</h1>
          <p className="mt-1 text-sm text-gray-500">
            Monitor and manage laboratory quality control results
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Record QC
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {qcResults.length}
          </div>
          <div className="text-sm text-gray-500">Total QC Runs</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {acceptedCount}
          </div>
          <div className="text-sm text-gray-500">Accepted</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {warningCount}
          </div>
          <div className="text-sm text-gray-500">Warnings</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">{rejectedCount}</div>
          <div className="text-sm text-gray-500">Rejected</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-600">{pendingCount}</div>
          <div className="text-sm text-gray-500">Pending Review</div>
        </div>
      </div>

      {(rejectedCount > 0 || pendingCount > 0) && (
        <div
          className={`rounded-lg p-4 ${rejectedCount > 0 ? "bg-red-50 border border-red-200" : "bg-yellow-50 border border-yellow-200"}`}
        >
          <div className="flex items-center">
            <ExclamationTriangleIcon
              className={`h-5 w-5 mr-3 ${rejectedCount > 0 ? "text-red-600" : "text-yellow-600"}`}
            />
            <div>
              <h3
                className={`text-sm font-medium ${rejectedCount > 0 ? "text-red-800" : "text-yellow-800"}`}
              >
                QC Action Required
              </h3>
              <p
                className={`text-sm mt-1 ${rejectedCount > 0 ? "text-red-700" : "text-yellow-700"}`}
              >
                {rejectedCount > 0 &&
                  `${rejectedCount} rejected result(s) require corrective action. `}
                {pendingCount > 0 &&
                  `${pendingCount} result(s) pending review.`}
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by test or instrument..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <input
          type="date"
          value={dateFilter}
          onChange={(e) => setDateFilter(e.target.value)}
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        />
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as QCResult["status"] | "all")
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
        ) : filteredResults.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ChartBarIcon className="h-12 w-12 mb-2" />
            <p>No QC results found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Test / Instrument
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Control Level
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Target
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Measured
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  SD
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Time
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredResults.map((result) => {
                const status = statusConfig[result.status];
                const controlLevel = controlLevelConfig[result.controlLevel];
                const StatusIcon = status.icon;

                return (
                  <tr
                    key={result.id}
                    className={`hover:bg-gray-50 ${result.status === "rejected" ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {result.testName}
                      </div>
                      <div className="text-xs text-gray-500">
                        {result.instrumentName}
                      </div>
                      <div className="text-xs text-gray-400">
                        Lot: {result.lotNumber}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${controlLevel.color}`}
                      >
                        {controlLevel.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {result.targetValue} ± {result.targetSD}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                      {result.measuredValue}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`text-sm font-bold ${getSDColor(result.sdFromTarget)}`}
                      >
                        {result.sdFromTarget > 0 ? "+" : ""}
                        {result.sdFromTarget.toFixed(2)} SD
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
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDateTime(result.performedAt)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      {result.status === "pending-review" && (
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() =>
                              reviewMutation.mutate({
                                id: result.id,
                                status: "accepted",
                              })
                            }
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Accept
                          </button>
                          <button
                            onClick={() =>
                              reviewMutation.mutate({
                                id: result.id,
                                status: "rejected",
                              })
                            }
                            className="rounded bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-700"
                          >
                            Reject
                          </button>
                        </div>
                      )}
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
