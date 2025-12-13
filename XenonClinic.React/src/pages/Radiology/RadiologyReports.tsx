import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  DocumentTextIcon,
  MagnifyingGlassIcon,
  EyeIcon,
  ClockIcon,
  PrinterIcon,
  ArrowDownTrayIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface RadiologyReport {
  id: number;
  reportNumber: string;
  orderId: number;
  orderNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  examType: string;
  modality: string;
  examDate: string;
  radiologistId: number;
  radiologistName: string;
  status: "draft" | "preliminary" | "final" | "addendum" | "amended";
  findings: string;
  impression: string;
  recommendations?: string;
  criticalFindings?: boolean;
  criticalFindingsCommunicated?: boolean;
  communicatedTo?: string;
  communicatedAt?: string;
  dictatedAt?: string;
  signedAt?: string;
  transcribedBy?: string;
}

const statusConfig = {
  draft: { label: "Draft", color: "bg-gray-100 text-gray-800" },
  preliminary: { label: "Preliminary", color: "bg-yellow-100 text-yellow-800" },
  final: { label: "Final", color: "bg-green-100 text-green-800" },
  addendum: { label: "Addendum", color: "bg-blue-100 text-blue-800" },
  amended: { label: "Amended", color: "bg-purple-100 text-purple-800" },
};

export function RadiologyReports() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    RadiologyReport["status"] | "all"
  >("all");
  const [selectedReport, setSelectedReport] = useState<RadiologyReport | null>(
    null,
  );
  const queryClient = useQueryClient();

  const { data: reports = [], isLoading } = useQuery({
    queryKey: ["radiology-reports"],
    queryFn: () => api.get<RadiologyReport[]>("/api/radiology/reports"),
  });

  const signReportMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/radiology/reports/${id}/sign`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["radiology-reports"] });
      setSelectedReport(null);
    },
  });

  const filteredReports = reports.filter((report) => {
    const matchesSearch =
      report.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      report.reportNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      report.examType.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || report.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const pendingSignature = reports.filter(
    (r) => r.status === "preliminary",
  ).length;
  const criticalPending = reports.filter(
    (r) => r.criticalFindings && !r.criticalFindingsCommunicated,
  ).length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Radiology Reports
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            View and manage radiology interpretations and reports
          </p>
        </div>
        <button className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
          <ArrowDownTrayIcon className="mr-2 h-5 w-5" />
          Export Reports
        </button>
      </div>

      {(pendingSignature > 0 || criticalPending > 0) && (
        <div
          className={`rounded-lg p-4 ${criticalPending > 0 ? "bg-red-50 border border-red-200" : "bg-yellow-50 border border-yellow-200"}`}
        >
          <div className="flex items-center">
            <ClockIcon
              className={`h-5 w-5 mr-3 ${criticalPending > 0 ? "text-red-600" : "text-yellow-600"}`}
            />
            <div>
              <h3
                className={`text-sm font-medium ${criticalPending > 0 ? "text-red-800" : "text-yellow-800"}`}
              >
                Action Required
              </h3>
              <p
                className={`text-sm mt-1 ${criticalPending > 0 ? "text-red-700" : "text-yellow-700"}`}
              >
                {criticalPending > 0 &&
                  `${criticalPending} critical finding(s) need communication. `}
                {pendingSignature > 0 &&
                  `${pendingSignature} report(s) pending signature.`}
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {reports.length}
          </div>
          <div className="text-sm text-gray-500">Total Reports</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingSignature}
          </div>
          <div className="text-sm text-gray-500">Pending Signature</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {reports.filter((r) => r.status === "final").length}
          </div>
          <div className="text-sm text-gray-500">Finalized</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {reports.filter((r) => r.criticalFindings).length}
          </div>
          <div className="text-sm text-gray-500">Critical Findings</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search reports..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as RadiologyReport["status"] | "all")
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
        ) : filteredReports.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <DocumentTextIcon className="h-12 w-12 mb-2" />
            <p>No reports found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Report
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Exam
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Radiologist
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Date
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredReports.map((report) => {
                const status = statusConfig[report.status];

                return (
                  <tr
                    key={report.id}
                    className={`hover:bg-gray-50 ${
                      report.criticalFindings &&
                      !report.criticalFindingsCommunicated
                        ? "bg-red-50"
                        : ""
                    }`}
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        <div className="font-mono text-sm font-medium text-gray-900">
                          {report.reportNumber}
                        </div>
                        {report.criticalFindings && (
                          <span className="ml-2 inline-flex rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-800">
                            CRITICAL
                          </span>
                        )}
                      </div>
                      <div className="text-xs text-gray-500">
                        Order: {report.orderNumber}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {report.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {report.patientMRN}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {report.examType}
                      </div>
                      <div className="text-xs text-gray-500">
                        {report.modality}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {report.radiologistName}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        {status.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDate(report.examDate)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setSelectedReport(report)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          <EyeIcon className="h-5 w-5" />
                        </button>
                        <button className="text-gray-600 hover:text-gray-900">
                          <PrinterIcon className="h-5 w-5" />
                        </button>
                        {report.status === "preliminary" && (
                          <button
                            onClick={() => signReportMutation.mutate(report.id)}
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Sign
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

      {selectedReport && (
        <Dialog
          open={!!selectedReport}
          onClose={() => setSelectedReport(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-3xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center justify-between">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-sm text-gray-500">
                        {selectedReport.reportNumber}
                      </span>
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[selectedReport.status].color}`}
                      >
                        {statusConfig[selectedReport.status].label}
                      </span>
                      {selectedReport.criticalFindings && (
                        <span className="inline-flex rounded bg-red-100 px-2 py-0.5 text-xs font-bold text-red-800">
                          CRITICAL FINDINGS
                        </span>
                      )}
                    </div>
                    <Dialog.Title className="text-xl font-semibold text-gray-900 mt-1">
                      {selectedReport.examType}
                    </Dialog.Title>
                  </div>
                </div>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Patient
                    </h4>
                    <p className="font-medium">{selectedReport.patientName}</p>
                    <p className="text-sm text-gray-500">
                      MRN: {selectedReport.patientMRN}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Radiologist
                    </h4>
                    <p className="font-medium">
                      {selectedReport.radiologistName}
                    </p>
                    <p className="text-sm text-gray-500">
                      Exam Date: {formatDate(selectedReport.examDate)}
                    </p>
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-2">
                    FINDINGS
                  </h4>
                  <p className="text-sm text-gray-700 whitespace-pre-wrap">
                    {selectedReport.findings}
                  </p>
                </div>

                <div className="bg-gray-50 -mx-6 px-6 py-4">
                  <h4 className="text-sm font-medium text-gray-900 mb-2">
                    IMPRESSION
                  </h4>
                  <p className="text-sm text-gray-700 whitespace-pre-wrap">
                    {selectedReport.impression}
                  </p>
                </div>

                {selectedReport.recommendations && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-900 mb-2">
                      RECOMMENDATIONS
                    </h4>
                    <p className="text-sm text-gray-700 whitespace-pre-wrap">
                      {selectedReport.recommendations}
                    </p>
                  </div>
                )}

                {selectedReport.signedAt && (
                  <div className="text-sm text-gray-500 border-t pt-4">
                    Electronically signed by {selectedReport.radiologistName} on{" "}
                    {formatDate(selectedReport.signedAt)}
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedReport(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
                <button className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
                  <PrinterIcon className="h-4 w-4 mr-1 inline" />
                  Print
                </button>
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}
    </div>
  );
}
