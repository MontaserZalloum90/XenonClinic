import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  DocumentChartBarIcon,
  MagnifyingGlassIcon,
  EyeIcon,
  ExclamationTriangleIcon,
  ArrowDownTrayIcon,
  PrinterIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";

interface LabResult {
  id: number;
  resultNumber: string;
  orderId: number;
  orderNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  testName: string;
  testCode: string;
  category: string;
  value: string;
  unit: string;
  referenceRange: string;
  interpretation:
    | "normal"
    | "low"
    | "high"
    | "critical-low"
    | "critical-high"
    | "abnormal";
  status: "pending" | "preliminary" | "final" | "corrected" | "cancelled";
  performedBy: string;
  performedAt: string;
  verifiedBy?: string;
  verifiedAt?: string;
  comments?: string;
  flags?: string[];
}

const interpretationConfig = {
  normal: { label: "Normal", color: "text-green-600 bg-green-50" },
  low: { label: "Low", color: "text-blue-600 bg-blue-50" },
  high: { label: "High", color: "text-orange-600 bg-orange-50" },
  "critical-low": {
    label: "Critical Low",
    color: "text-red-600 bg-red-50 font-bold",
  },
  "critical-high": {
    label: "Critical High",
    color: "text-red-600 bg-red-50 font-bold",
  },
  abnormal: { label: "Abnormal", color: "text-yellow-600 bg-yellow-50" },
};

const statusConfig = {
  pending: { label: "Pending", color: "bg-gray-100 text-gray-800" },
  preliminary: { label: "Preliminary", color: "bg-yellow-100 text-yellow-800" },
  final: { label: "Final", color: "bg-green-100 text-green-800" },
  corrected: { label: "Corrected", color: "bg-blue-100 text-blue-800" },
  cancelled: { label: "Cancelled", color: "bg-red-100 text-red-800" },
};

export function LabResults() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<LabResult["status"] | "all">(
    "all",
  );
  const [interpretationFilter, setInterpretationFilter] = useState<
    LabResult["interpretation"] | "all"
  >("all");
  const [selectedResult, setSelectedResult] = useState<LabResult | null>(null);
  const queryClient = useQueryClient();

  const { data: results = [], isLoading } = useQuery({
    queryKey: ["lab-results"],
    queryFn: () => api.get<LabResult[]>("/api/laboratory/results"),
  });

  const verifyMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/laboratory/results/${id}/verify`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["lab-results"] });
    },
  });

  const filteredResults = results.filter((result) => {
    const matchesSearch =
      result.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      result.testName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      result.orderNumber.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || result.status === statusFilter;
    const matchesInterpretation =
      interpretationFilter === "all" ||
      result.interpretation === interpretationFilter;
    return matchesSearch && matchesStatus && matchesInterpretation;
  });

  const criticalResults = results.filter(
    (r) =>
      r.interpretation === "critical-low" ||
      r.interpretation === "critical-high",
  );

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Lab Results</h1>
          <p className="mt-1 text-sm text-gray-500">
            Review and verify laboratory test results
          </p>
        </div>
        <button className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
          <ArrowDownTrayIcon className="mr-2 h-5 w-5" />
          Export Results
        </button>
      </div>

      {criticalResults.length > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                {criticalResults.length} Critical Result
                {criticalResults.length !== 1 ? "s" : ""} Require Attention
              </h3>
              <p className="text-sm text-red-700 mt-1">
                Please review and take appropriate clinical action.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {results.length}
          </div>
          <div className="text-sm text-gray-500">Total Results</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {results.filter((r) => r.status === "preliminary").length}
          </div>
          <div className="text-sm text-gray-500">Pending Verification</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {criticalResults.length}
          </div>
          <div className="text-sm text-gray-500">Critical</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {results.filter((r) => r.status === "final").length}
          </div>
          <div className="text-sm text-gray-500">Finalized</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search results..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as LabResult["status"] | "all")
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
        <select
          value={interpretationFilter}
          onChange={(e) =>
            setInterpretationFilter(
              e.target.value as LabResult["interpretation"] | "all",
            )
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Interpretations</option>
          {Object.entries(interpretationConfig).map(([key, value]) => (
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
            <DocumentChartBarIcon className="h-12 w-12 mb-2" />
            <p>No results found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Test
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Result
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Reference
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Interpretation
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
              {filteredResults.map((result) => {
                const interpretation =
                  interpretationConfig[result.interpretation];
                const status = statusConfig[result.status];
                const isCritical = result.interpretation.includes("critical");

                return (
                  <tr
                    key={result.id}
                    className={`hover:bg-gray-50 ${isCritical ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {result.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {result.patientMRN}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {result.testName}
                      </div>
                      <div className="text-xs text-gray-500">
                        {result.testCode} • {result.category}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`text-sm font-bold ${interpretation.color} px-2 py-1 rounded`}
                      >
                        {result.value} {result.unit}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {result.referenceRange}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${interpretation.color}`}
                      >
                        {interpretation.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setSelectedResult(result)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          <EyeIcon className="h-5 w-5" />
                        </button>
                        <button className="text-gray-600 hover:text-gray-900">
                          <PrinterIcon className="h-5 w-5" />
                        </button>
                        {result.status === "preliminary" && (
                          <button
                            onClick={() => verifyMutation.mutate(result.id)}
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Verify
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

      {selectedResult && (
        <Dialog
          open={!!selectedResult}
          onClose={() => setSelectedResult(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white p-6 shadow-xl">
              <Dialog.Title className="text-lg font-semibold text-gray-900 mb-4">
                Result Details
              </Dialog.Title>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-gray-500">Patient</label>
                    <p className="font-medium">{selectedResult.patientName}</p>
                    <p className="text-sm text-gray-500">
                      MRN: {selectedResult.patientMRN}
                    </p>
                  </div>
                  <div>
                    <label className="text-sm text-gray-500">Test</label>
                    <p className="font-medium">{selectedResult.testName}</p>
                    <p className="text-sm text-gray-500">
                      {selectedResult.testCode}
                    </p>
                  </div>
                </div>
                <div
                  className={`p-4 rounded-lg ${interpretationConfig[selectedResult.interpretation].color}`}
                >
                  <label className="text-sm font-medium">Result</label>
                  <p className="text-2xl font-bold">
                    {selectedResult.value} {selectedResult.unit}
                  </p>
                  <p className="text-sm">
                    Reference: {selectedResult.referenceRange}
                  </p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-gray-500">
                      Performed By
                    </label>
                    <p className="font-medium">{selectedResult.performedBy}</p>
                    <p className="text-sm text-gray-500">
                      {formatDateTime(selectedResult.performedAt)}
                    </p>
                  </div>
                  {selectedResult.verifiedBy && (
                    <div>
                      <label className="text-sm text-gray-500">
                        Verified By
                      </label>
                      <p className="font-medium">{selectedResult.verifiedBy}</p>
                      <p className="text-sm text-gray-500">
                        {formatDateTime(selectedResult.verifiedAt!)}
                      </p>
                    </div>
                  )}
                </div>
                {selectedResult.comments && (
                  <div>
                    <label className="text-sm text-gray-500">Comments</label>
                    <p className="text-sm">{selectedResult.comments}</p>
                  </div>
                )}
              </div>
              <div className="mt-6 flex justify-end">
                <button
                  onClick={() => setSelectedResult(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}
    </div>
  );
}
