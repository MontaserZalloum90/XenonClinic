import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  DocumentTextIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
  ExclamationTriangleIcon,
  ArrowPathIcon,
  PaperAirplaneIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface InsuranceClaim {
  id: number;
  claimNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  insurerId: number;
  insurerName: string;
  policyNumber: string;
  authorizationNumber?: string;
  serviceDate: string;
  submissionDate?: string;
  claimType: "professional" | "institutional" | "dental" | "pharmacy";
  status:
    | "draft"
    | "submitted"
    | "pending"
    | "approved"
    | "partially-approved"
    | "denied"
    | "appealed"
    | "paid";
  totalBilled: number;
  allowedAmount?: number;
  paidAmount?: number;
  patientResponsibility?: number;
  adjustments?: number;
  denialReason?: string;
  denialCode?: string;
  diagnosisCodes: string[];
  procedureCodes: string[];
  providerNPI: string;
  providerName: string;
  facilityName?: string;
  notes?: string;
  remittanceDate?: string;
  checkNumber?: string;
  createdAt: string;
}

const statusConfig = {
  draft: {
    label: "Draft",
    color: "bg-gray-100 text-gray-800",
    icon: DocumentTextIcon,
  },
  submitted: {
    label: "Submitted",
    color: "bg-blue-100 text-blue-800",
    icon: PaperAirplaneIcon,
  },
  pending: {
    label: "Pending",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  approved: {
    label: "Approved",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  "partially-approved": {
    label: "Partial",
    color: "bg-orange-100 text-orange-800",
    icon: ExclamationTriangleIcon,
  },
  denied: {
    label: "Denied",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
  appealed: {
    label: "Appealed",
    color: "bg-purple-100 text-purple-800",
    icon: ArrowPathIcon,
  },
  paid: {
    label: "Paid",
    color: "bg-emerald-100 text-emerald-800",
    icon: CheckCircleIcon,
  },
};

const claimTypeConfig = {
  professional: { label: "Professional", color: "bg-blue-100 text-blue-800" },
  institutional: {
    label: "Institutional",
    color: "bg-purple-100 text-purple-800",
  },
  dental: { label: "Dental", color: "bg-green-100 text-green-800" },
  pharmacy: { label: "Pharmacy", color: "bg-orange-100 text-orange-800" },
};

export function InsuranceClaims() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    InsuranceClaim["status"] | "all"
  >("all");
  const [selectedClaim, setSelectedClaim] = useState<InsuranceClaim | null>(
    null,
  );
  const queryClient = useQueryClient();

  const { data: claims = [], isLoading } = useQuery({
    queryKey: ["insurance-claims"],
    queryFn: () => api.get<InsuranceClaim[]>("/api/financial/claims"),
  });

  const submitClaimMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/financial/claims/${id}/submit`),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["insurance-claims"] }),
  });

  const appealClaimMutation = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason: string }) =>
      api.post(`/api/financial/claims/${id}/appeal`, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["insurance-claims"] });
      setSelectedClaim(null);
    },
  });

  const filteredClaims = claims.filter((claim) => {
    const matchesSearch =
      claim.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      claim.claimNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      claim.insurerName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || claim.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(amount);
  };

  const totalBilled = claims.reduce((sum, c) => sum + c.totalBilled, 0);
  const totalPaid = claims.reduce((sum, c) => sum + (c.paidAmount || 0), 0);
  const pendingCount = claims.filter((c) =>
    ["submitted", "pending"].includes(c.status),
  ).length;
  const deniedCount = claims.filter((c) => c.status === "denied").length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Insurance Claims</h1>
          <p className="mt-1 text-sm text-gray-500">
            Submit and track insurance claims and reimbursements
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Claim
        </button>
      </div>

      {deniedCount > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex items-center">
            <XCircleIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                Claims Requiring Attention
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {deniedCount} claim(s) have been denied. Review and consider
                appealing.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {claims.length}
          </div>
          <div className="text-sm text-gray-500">Total Claims</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {formatCurrency(totalBilled)}
          </div>
          <div className="text-sm text-gray-500">Total Billed</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {formatCurrency(totalPaid)}
          </div>
          <div className="text-sm text-gray-500">Total Paid</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingCount}
          </div>
          <div className="text-sm text-gray-500">Pending Review</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search claims..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as InsuranceClaim["status"] | "all")
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
        ) : filteredClaims.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <DocumentTextIcon className="h-12 w-12 mb-2" />
            <p>No insurance claims found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Claim
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Insurer
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Billed
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Paid
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
              {filteredClaims.map((claim) => {
                const status = statusConfig[claim.status];
                const claimType = claimTypeConfig[claim.claimType];
                const StatusIcon = status.icon;

                return (
                  <tr
                    key={claim.id}
                    className={`hover:bg-gray-50 ${claim.status === "denied" ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm font-medium text-gray-900">
                        {claim.claimNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        Service: {formatDate(claim.serviceDate)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {claim.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {claim.patientMRN}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">
                        {claim.insurerName}
                      </div>
                      <div className="text-xs text-gray-500">
                        {claim.policyNumber}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${claimType.color}`}
                      >
                        {claimType.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                      {formatCurrency(claim.totalBilled)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {claim.paidAmount
                        ? formatCurrency(claim.paidAmount)
                        : "-"}
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
                        <button
                          onClick={() => setSelectedClaim(claim)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          View
                        </button>
                        {claim.status === "draft" && (
                          <button
                            onClick={() => submitClaimMutation.mutate(claim.id)}
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Submit
                          </button>
                        )}
                        {claim.status === "denied" && (
                          <button
                            onClick={() => setSelectedClaim(claim)}
                            className="rounded bg-purple-600 px-3 py-1 text-xs font-medium text-white hover:bg-purple-700"
                          >
                            Appeal
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

      {selectedClaim && (
        <Dialog
          open={!!selectedClaim}
          onClose={() => setSelectedClaim(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-3xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[selectedClaim.status].color}`}
                  >
                    {statusConfig[selectedClaim.status].label}
                  </span>
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${claimTypeConfig[selectedClaim.claimType].color}`}
                  >
                    {claimTypeConfig[selectedClaim.claimType].label}
                  </span>
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  Claim {selectedClaim.claimNumber}
                </Dialog.Title>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Patient
                    </h4>
                    <p className="font-medium">{selectedClaim.patientName}</p>
                    <p className="text-sm text-gray-500">
                      MRN: {selectedClaim.patientMRN}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Insurance
                    </h4>
                    <p className="font-medium">{selectedClaim.insurerName}</p>
                    <p className="text-sm text-gray-500">
                      Policy: {selectedClaim.policyNumber}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Provider
                    </h4>
                    <p className="font-medium">{selectedClaim.providerName}</p>
                    <p className="text-sm text-gray-500">
                      NPI: {selectedClaim.providerNPI}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Service Date
                    </h4>
                    <p className="font-medium">
                      {formatDate(selectedClaim.serviceDate)}
                    </p>
                  </div>
                </div>

                <div className="bg-gray-50 rounded-lg p-4">
                  <h4 className="text-sm font-medium text-gray-900 mb-3">
                    Financial Summary
                  </h4>
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Total Billed:</span>
                      <span className="font-medium">
                        {formatCurrency(selectedClaim.totalBilled)}
                      </span>
                    </div>
                    {selectedClaim.allowedAmount !== undefined && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Allowed Amount:</span>
                        <span className="font-medium">
                          {formatCurrency(selectedClaim.allowedAmount)}
                        </span>
                      </div>
                    )}
                    {selectedClaim.paidAmount !== undefined && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Paid Amount:</span>
                        <span className="font-medium text-green-600">
                          {formatCurrency(selectedClaim.paidAmount)}
                        </span>
                      </div>
                    )}
                    {selectedClaim.adjustments !== undefined && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Adjustments:</span>
                        <span className="font-medium text-red-600">
                          -{formatCurrency(selectedClaim.adjustments)}
                        </span>
                      </div>
                    )}
                    {selectedClaim.patientResponsibility !== undefined && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">
                          Patient Responsibility:
                        </span>
                        <span className="font-medium">
                          {formatCurrency(selectedClaim.patientResponsibility)}
                        </span>
                      </div>
                    )}
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">
                      Diagnosis Codes (ICD-10)
                    </h4>
                    <div className="flex flex-wrap gap-1">
                      {selectedClaim.diagnosisCodes.map((code, idx) => (
                        <span
                          key={idx}
                          className="inline-flex rounded bg-gray-100 px-2 py-0.5 text-xs font-mono"
                        >
                          {code}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">
                      Procedure Codes (CPT)
                    </h4>
                    <div className="flex flex-wrap gap-1">
                      {selectedClaim.procedureCodes.map((code, idx) => (
                        <span
                          key={idx}
                          className="inline-flex rounded bg-blue-100 px-2 py-0.5 text-xs font-mono"
                        >
                          {code}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>

                {selectedClaim.status === "denied" &&
                  selectedClaim.denialReason && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-red-900 mb-2">
                        Denial Information
                      </h4>
                      <p className="text-sm text-red-700">
                        <span className="font-mono">
                          {selectedClaim.denialCode}
                        </span>
                        : {selectedClaim.denialReason}
                      </p>
                    </div>
                  )}

                {selectedClaim.remittanceDate && (
                  <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-green-900 mb-2">
                      Payment Information
                    </h4>
                    <div className="text-sm text-green-700">
                      <p>
                        Remittance Date:{" "}
                        {formatDate(selectedClaim.remittanceDate)}
                      </p>
                      {selectedClaim.checkNumber && (
                        <p>Check Number: {selectedClaim.checkNumber}</p>
                      )}
                    </div>
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedClaim(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
                {selectedClaim.status === "denied" && (
                  <button
                    onClick={() =>
                      appealClaimMutation.mutate({
                        id: selectedClaim.id,
                        reason: "Appeal submitted",
                      })
                    }
                    className="rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700"
                  >
                    File Appeal
                  </button>
                )}
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}
    </div>
  );
}
