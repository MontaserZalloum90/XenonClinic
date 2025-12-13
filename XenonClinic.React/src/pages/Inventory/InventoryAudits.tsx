import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ClipboardDocumentCheckIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  CalendarIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface AuditItem {
  id: number;
  itemCode: string;
  itemName: string;
  category: string;
  location: string;
  expectedQuantity: number;
  countedQuantity?: number;
  variance?: number;
  varianceValue?: number;
  notes?: string;
  countedBy?: string;
  countedAt?: string;
}

interface InventoryAudit {
  id: number;
  auditNumber: string;
  auditType: "full" | "cycle" | "spot" | "annual";
  status:
    | "scheduled"
    | "in-progress"
    | "pending-review"
    | "completed"
    | "cancelled";
  scheduledDate: string;
  startedAt?: string;
  completedAt?: string;
  location?: string;
  category?: string;
  items: AuditItem[];
  totalItems: number;
  countedItems: number;
  discrepancies: number;
  totalVarianceValue: number;
  conductedBy?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  notes?: string;
  createdBy: string;
  createdAt: string;
}

const auditTypeConfig = {
  full: { label: "Full Audit", color: "bg-purple-100 text-purple-800" },
  cycle: { label: "Cycle Count", color: "bg-blue-100 text-blue-800" },
  spot: { label: "Spot Check", color: "bg-yellow-100 text-yellow-800" },
  annual: { label: "Annual Audit", color: "bg-red-100 text-red-800" },
};

const statusConfig = {
  scheduled: {
    label: "Scheduled",
    color: "bg-gray-100 text-gray-800",
    icon: CalendarIcon,
  },
  "in-progress": {
    label: "In Progress",
    color: "bg-blue-100 text-blue-800",
    icon: ClockIcon,
  },
  "pending-review": {
    label: "Pending Review",
    color: "bg-yellow-100 text-yellow-800",
    icon: ExclamationTriangleIcon,
  },
  completed: {
    label: "Completed",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-red-100 text-red-800",
    icon: ExclamationTriangleIcon,
  },
};

export function InventoryAudits() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    InventoryAudit["status"] | "all"
  >("all");
  const [selectedAudit, setSelectedAudit] = useState<InventoryAudit | null>(
    null,
  );
  const queryClient = useQueryClient();

  const { data: audits = [], isLoading } = useQuery({
    queryKey: ["inventory-audits"],
    queryFn: () => api.get<InventoryAudit[]>("/api/inventory/audits"),
  });

  const startAuditMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/inventory/audits/${id}/start`),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["inventory-audits"] }),
  });

  const submitCountMutation = useMutation({
    mutationFn: ({
      auditId,
      itemId,
      count,
      notes,
    }: {
      auditId: number;
      itemId: number;
      count: number;
      notes?: string;
    }) =>
      api.post(`/api/inventory/audits/${auditId}/items/${itemId}/count`, {
        count,
        notes,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inventory-audits"] });
    },
  });

  const completeAuditMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/inventory/audits/${id}/complete`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inventory-audits"] });
      setSelectedAudit(null);
    },
  });

  const approveAuditMutation = useMutation({
    mutationFn: (id: number) => api.post(`/api/inventory/audits/${id}/approve`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inventory-audits"] });
      setSelectedAudit(null);
    },
  });

  const filteredAudits = audits.filter((audit) => {
    const matchesSearch =
      audit.auditNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (audit.location &&
        audit.location.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesStatus =
      statusFilter === "all" || audit.status === statusFilter;
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

  const scheduledCount = audits.filter((a) => a.status === "scheduled").length;
  const inProgressCount = audits.filter(
    (a) => a.status === "in-progress",
  ).length;
  const pendingReviewCount = audits.filter(
    (a) => a.status === "pending-review",
  ).length;
  const totalDiscrepancies = audits
    .filter((a) => a.status === "completed")
    .reduce((sum, a) => sum + a.discrepancies, 0);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Inventory Audits</h1>
          <p className="mt-1 text-sm text-gray-500">
            Schedule and conduct inventory counts and audits
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          Schedule Audit
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {scheduledCount}
          </div>
          <div className="text-sm text-gray-500">Scheduled</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {inProgressCount}
          </div>
          <div className="text-sm text-gray-500">In Progress</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingReviewCount}
          </div>
          <div className="text-sm text-gray-500">Pending Review</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {totalDiscrepancies}
          </div>
          <div className="text-sm text-gray-500">Total Discrepancies</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search audits..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as InventoryAudit["status"] | "all")
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
        ) : filteredAudits.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ClipboardDocumentCheckIcon className="h-12 w-12 mb-2" />
            <p>No audits found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Audit
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Scope
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Progress
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Variance
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
              {filteredAudits.map((audit) => {
                const auditType = auditTypeConfig[audit.auditType];
                const status = statusConfig[audit.status];
                const StatusIcon = status.icon;
                const progress =
                  audit.totalItems > 0
                    ? Math.round((audit.countedItems / audit.totalItems) * 100)
                    : 0;

                return (
                  <tr key={audit.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm font-medium text-gray-900">
                        {audit.auditNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        {formatDate(audit.scheduledDate)}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${auditType.color}`}
                      >
                        {auditType.label}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {audit.location && <div>Location: {audit.location}</div>}
                      {audit.category && <div>Category: {audit.category}</div>}
                      {!audit.location && !audit.category && (
                        <span>All items</span>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                          <div
                            className="h-full bg-blue-600"
                            style={{ width: `${progress}%` }}
                          />
                        </div>
                        <span className="text-xs text-gray-500">
                          {audit.countedItems}/{audit.totalItems}
                        </span>
                      </div>
                      {audit.discrepancies > 0 && (
                        <div className="text-xs text-red-600 mt-1">
                          {audit.discrepancies} discrepancy(ies)
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm">
                      {audit.totalVarianceValue !== 0 ? (
                        <span
                          className={
                            audit.totalVarianceValue < 0
                              ? "text-red-600"
                              : "text-green-600"
                          }
                        >
                          {formatCurrency(audit.totalVarianceValue)}
                        </span>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
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
                          onClick={() => setSelectedAudit(audit)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          View
                        </button>
                        {audit.status === "scheduled" && (
                          <button
                            onClick={() => startAuditMutation.mutate(audit.id)}
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Start
                          </button>
                        )}
                        {audit.status === "in-progress" && (
                          <button
                            onClick={() => setSelectedAudit(audit)}
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Count
                          </button>
                        )}
                        {audit.status === "pending-review" && (
                          <button
                            onClick={() =>
                              approveAuditMutation.mutate(audit.id)
                            }
                            className="rounded bg-purple-600 px-3 py-1 text-xs font-medium text-white hover:bg-purple-700"
                          >
                            Approve
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

      {selectedAudit && (
        <Dialog
          open={!!selectedAudit}
          onClose={() => setSelectedAudit(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-4xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${auditTypeConfig[selectedAudit.auditType].color}`}
                  >
                    {auditTypeConfig[selectedAudit.auditType].label}
                  </span>
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[selectedAudit.status].color}`}
                  >
                    {statusConfig[selectedAudit.status].label}
                  </span>
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  Audit {selectedAudit.auditNumber}
                </Dialog.Title>
                <p className="text-sm text-gray-500">
                  Scheduled: {formatDate(selectedAudit.scheduledDate)}
                  {selectedAudit.conductedBy &&
                    ` | Conducted by: ${selectedAudit.conductedBy}`}
                </p>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-4 gap-4 text-center">
                  <div className="bg-gray-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-gray-900">
                      {selectedAudit.totalItems}
                    </div>
                    <div className="text-xs text-gray-600">Total Items</div>
                  </div>
                  <div className="bg-blue-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-blue-700">
                      {selectedAudit.countedItems}
                    </div>
                    <div className="text-xs text-blue-600">Counted</div>
                  </div>
                  <div className="bg-red-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-red-700">
                      {selectedAudit.discrepancies}
                    </div>
                    <div className="text-xs text-red-600">Discrepancies</div>
                  </div>
                  <div className="bg-yellow-50 rounded-lg p-3">
                    <div className="text-lg font-bold text-yellow-700">
                      {formatCurrency(
                        Math.abs(selectedAudit.totalVarianceValue),
                      )}
                    </div>
                    <div className="text-xs text-yellow-600">
                      Variance Value
                    </div>
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-3">
                    Audit Items
                  </h4>
                  <div className="border rounded-lg overflow-hidden max-h-96 overflow-y-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50 sticky top-0">
                        <tr>
                          <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                            Item
                          </th>
                          <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                            Location
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Expected
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Counted
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Variance
                          </th>
                          {selectedAudit.status === "in-progress" && (
                            <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                              Action
                            </th>
                          )}
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200">
                        {selectedAudit.items.map((item) => (
                          <tr
                            key={item.id}
                            className={
                              item.variance !== undefined && item.variance !== 0
                                ? "bg-red-50"
                                : ""
                            }
                          >
                            <td className="px-4 py-2">
                              <div className="text-sm font-medium text-gray-900">
                                {item.itemName}
                              </div>
                              <div className="text-xs text-gray-500">
                                {item.itemCode}
                              </div>
                            </td>
                            <td className="px-4 py-2 text-sm text-gray-500">
                              {item.location}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.expectedQuantity}
                            </td>
                            <td className="px-4 py-2 text-right text-sm font-medium">
                              {item.countedQuantity !== undefined
                                ? item.countedQuantity
                                : "-"}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.variance !== undefined ? (
                                <span
                                  className={
                                    item.variance !== 0
                                      ? "text-red-600 font-medium"
                                      : "text-green-600"
                                  }
                                >
                                  {item.variance > 0 ? "+" : ""}
                                  {item.variance}
                                </span>
                              ) : (
                                "-"
                              )}
                            </td>
                            {selectedAudit.status === "in-progress" && (
                              <td className="px-4 py-2 text-right">
                                {item.countedQuantity === undefined && (
                                  <button
                                    onClick={() => {
                                      const count = prompt(
                                        `Enter count for ${item.itemName}:`,
                                        item.expectedQuantity.toString(),
                                      );
                                      if (count !== null) {
                                        submitCountMutation.mutate({
                                          auditId: selectedAudit.id,
                                          itemId: item.id,
                                          count: parseInt(count),
                                        });
                                      }
                                    }}
                                    className="text-blue-600 hover:text-blue-900 text-xs"
                                  >
                                    Count
                                  </button>
                                )}
                              </td>
                            )}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {selectedAudit.notes && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-1">
                      Notes
                    </h4>
                    <p className="text-sm text-gray-700">
                      {selectedAudit.notes}
                    </p>
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedAudit(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
                {selectedAudit.status === "in-progress" &&
                  selectedAudit.countedItems === selectedAudit.totalItems && (
                    <button
                      onClick={() =>
                        completeAuditMutation.mutate(selectedAudit.id)
                      }
                      className="rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
                    >
                      Complete Audit
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
