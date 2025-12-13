import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ArrowsRightLeftIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
  TruckIcon,
  BuildingOffice2Icon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface TransferItem {
  id: number;
  itemCode: string;
  itemName: string;
  category: string;
  requestedQuantity: number;
  approvedQuantity?: number;
  shippedQuantity?: number;
  receivedQuantity?: number;
  unitCost: number;
  notes?: string;
}

interface StockTransfer {
  id: number;
  transferNumber: string;
  sourceLocationId: number;
  sourceLocationName: string;
  destinationLocationId: number;
  destinationLocationName: string;
  transferType: "internal" | "branch" | "return" | "emergency";
  status:
    | "draft"
    | "pending-approval"
    | "approved"
    | "in-transit"
    | "partial"
    | "received"
    | "cancelled";
  priority: "low" | "normal" | "high" | "urgent";
  requestedDate: string;
  approvedDate?: string;
  shippedDate?: string;
  expectedArrival?: string;
  receivedDate?: string;
  items: TransferItem[];
  totalItems: number;
  totalValue: number;
  requestedBy: string;
  approvedBy?: string;
  shippedBy?: string;
  receivedBy?: string;
  reason?: string;
  notes?: string;
  trackingNumber?: string;
  createdAt: string;
}

const transferTypeConfig = {
  internal: { label: "Internal", color: "bg-blue-100 text-blue-800" },
  branch: { label: "Branch", color: "bg-purple-100 text-purple-800" },
  return: { label: "Return", color: "bg-orange-100 text-orange-800" },
  emergency: { label: "Emergency", color: "bg-red-100 text-red-800" },
};

const statusConfig = {
  draft: {
    label: "Draft",
    color: "bg-gray-100 text-gray-800",
    icon: ClockIcon,
  },
  "pending-approval": {
    label: "Pending",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  approved: {
    label: "Approved",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  "in-transit": {
    label: "In Transit",
    color: "bg-blue-100 text-blue-800",
    icon: TruckIcon,
  },
  partial: {
    label: "Partial",
    color: "bg-orange-100 text-orange-800",
    icon: TruckIcon,
  },
  received: {
    label: "Received",
    color: "bg-emerald-100 text-emerald-800",
    icon: CheckCircleIcon,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
};

const priorityConfig = {
  low: { label: "Low", color: "bg-gray-100 text-gray-800" },
  normal: { label: "Normal", color: "bg-blue-100 text-blue-800" },
  high: { label: "High", color: "bg-orange-100 text-orange-800" },
  urgent: { label: "Urgent", color: "bg-red-100 text-red-800" },
};

export function StockTransfers() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    StockTransfer["status"] | "all"
  >("all");
  const [selectedTransfer, setSelectedTransfer] =
    useState<StockTransfer | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: transfers = [], isLoading } = useQuery({
    queryKey: ["stock-transfers"],
    queryFn: () => api.get<StockTransfer[]>("/api/inventory/transfers"),
  });

  const approveTransferMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/inventory/transfers/${id}/approve`),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["stock-transfers"] }),
  });

  const shipTransferMutation = useMutation({
    mutationFn: ({
      id,
      trackingNumber,
    }: {
      id: number;
      trackingNumber?: string;
    }) => api.post(`/api/inventory/transfers/${id}/ship`, { trackingNumber }),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["stock-transfers"] }),
  });

  const receiveTransferMutation = useMutation({
    mutationFn: ({
      id,
      items,
    }: {
      id: number;
      items: { itemId: number; receivedQuantity: number }[];
    }) => api.post(`/api/inventory/transfers/${id}/receive`, { items }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["stock-transfers"] });
      setSelectedTransfer(null);
    },
  });

  const createTransferMutation = useMutation({
    mutationFn: (data: {
      sourceLocationName: string;
      destinationLocationName: string;
      transferType: string;
      priority: string;
      reason?: string;
      notes?: string;
      items: {
        itemCode: string;
        itemName: string;
        quantity: number;
        unitCost: number;
      }[];
    }) => api.post("/api/inventory/transfers", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["stock-transfers"] });
      setIsModalOpen(false);
    },
  });

  const filteredTransfers = transfers.filter((transfer) => {
    const matchesSearch =
      transfer.transferNumber
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      transfer.sourceLocationName
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      transfer.destinationLocationName
        .toLowerCase()
        .includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || transfer.status === statusFilter;
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

  const pendingApproval = transfers.filter(
    (t) => t.status === "pending-approval",
  ).length;
  const inTransit = transfers.filter((t) => t.status === "in-transit").length;
  const urgentTransfers = transfers.filter(
    (t) =>
      t.priority === "urgent" && !["received", "cancelled"].includes(t.status),
  ).length;
  const totalValue = transfers
    .filter((t) =>
      ["pending-approval", "approved", "in-transit"].includes(t.status),
    )
    .reduce((sum, t) => sum + t.totalValue, 0);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Stock Transfers</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage inventory transfers between locations
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Transfer
        </button>
      </div>

      {urgentTransfers > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex items-center">
            <TruckIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                Urgent Transfers
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {urgentTransfers} urgent transfer(s) require immediate
                attention.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {transfers.length}
          </div>
          <div className="text-sm text-gray-500">Total Transfers</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingApproval}
          </div>
          <div className="text-sm text-gray-500">Pending Approval</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">{inTransit}</div>
          <div className="text-sm text-gray-500">In Transit</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {formatCurrency(totalValue)}
          </div>
          <div className="text-sm text-gray-500">Value in Transit</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search transfers..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as StockTransfer["status"] | "all")
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
        ) : filteredTransfers.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ArrowsRightLeftIcon className="h-12 w-12 mb-2" />
            <p>No stock transfers found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Transfer
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  From / To
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Items
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Value
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
              {filteredTransfers.map((transfer) => {
                const transferType = transferTypeConfig[transfer.transferType];
                const status = statusConfig[transfer.status];
                const priority = priorityConfig[transfer.priority];
                const StatusIcon = status.icon;

                return (
                  <tr
                    key={transfer.id}
                    className={`hover:bg-gray-50 ${transfer.priority === "urgent" ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <div className="font-mono text-sm font-medium text-gray-900">
                          {transfer.transferNumber}
                        </div>
                        {transfer.priority !== "normal" && (
                          <span
                            className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${priority.color}`}
                          >
                            {priority.label}
                          </span>
                        )}
                      </div>
                      <div className="text-xs text-gray-500">
                        {formatDate(transfer.requestedDate)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center text-sm">
                        <BuildingOffice2Icon className="h-4 w-4 text-gray-400 mr-1" />
                        <span className="font-medium">
                          {transfer.sourceLocationName}
                        </span>
                      </div>
                      <div className="flex items-center text-sm text-gray-500 mt-1">
                        <ArrowsRightLeftIcon className="h-3 w-3 mr-1" />
                        {transfer.destinationLocationName}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${transferType.color}`}
                      >
                        {transferType.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {transfer.totalItems} item(s)
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                      {formatCurrency(transfer.totalValue)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${status.color}`}
                      >
                        <StatusIcon className="h-3.5 w-3.5 mr-1" />
                        {status.label}
                      </span>
                      {transfer.trackingNumber && (
                        <div className="text-xs text-gray-500 mt-1">
                          Track: {transfer.trackingNumber}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setSelectedTransfer(transfer)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          View
                        </button>
                        {transfer.status === "pending-approval" && (
                          <button
                            onClick={() =>
                              approveTransferMutation.mutate(transfer.id)
                            }
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Approve
                          </button>
                        )}
                        {transfer.status === "approved" && (
                          <button
                            onClick={() =>
                              shipTransferMutation.mutate({ id: transfer.id })
                            }
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Ship
                          </button>
                        )}
                        {transfer.status === "in-transit" && (
                          <button
                            onClick={() => setSelectedTransfer(transfer)}
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

      {selectedTransfer && (
        <Dialog
          open={!!selectedTransfer}
          onClose={() => setSelectedTransfer(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-3xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${transferTypeConfig[selectedTransfer.transferType].color}`}
                  >
                    {transferTypeConfig[selectedTransfer.transferType].label}
                  </span>
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[selectedTransfer.status].color}`}
                  >
                    {statusConfig[selectedTransfer.status].label}
                  </span>
                  {selectedTransfer.priority !== "normal" && (
                    <span
                      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${priorityConfig[selectedTransfer.priority].color}`}
                    >
                      {priorityConfig[selectedTransfer.priority].label}
                    </span>
                  )}
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  Transfer {selectedTransfer.transferNumber}
                </Dialog.Title>
              </div>
              <div className="p-6 space-y-6">
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="flex items-center justify-between">
                    <div className="text-center">
                      <div className="text-sm font-medium text-gray-500">
                        From
                      </div>
                      <div className="font-medium text-gray-900">
                        {selectedTransfer.sourceLocationName}
                      </div>
                    </div>
                    <ArrowsRightLeftIcon className="h-8 w-8 text-gray-400" />
                    <div className="text-center">
                      <div className="text-sm font-medium text-gray-500">
                        To
                      </div>
                      <div className="font-medium text-gray-900">
                        {selectedTransfer.destinationLocationName}
                      </div>
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Requested By
                    </h4>
                    <p className="font-medium">
                      {selectedTransfer.requestedBy}
                    </p>
                    <p className="text-xs text-gray-500">
                      {formatDate(selectedTransfer.requestedDate)}
                    </p>
                  </div>
                  {selectedTransfer.approvedBy && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Approved By
                      </h4>
                      <p className="font-medium">
                        {selectedTransfer.approvedBy}
                      </p>
                      {selectedTransfer.approvedDate && (
                        <p className="text-xs text-gray-500">
                          {formatDate(selectedTransfer.approvedDate)}
                        </p>
                      )}
                    </div>
                  )}
                  {selectedTransfer.expectedArrival && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Expected Arrival
                      </h4>
                      <p className="font-medium">
                        {formatDate(selectedTransfer.expectedArrival)}
                      </p>
                    </div>
                  )}
                  {selectedTransfer.trackingNumber && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Tracking Number
                      </h4>
                      <p className="font-mono font-medium">
                        {selectedTransfer.trackingNumber}
                      </p>
                    </div>
                  )}
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-3">
                    Transfer Items
                  </h4>
                  <div className="border rounded-lg overflow-hidden">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                            Item
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Requested
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Approved
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Shipped
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Received
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Value
                          </th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200">
                        {selectedTransfer.items.map((item) => (
                          <tr key={item.id}>
                            <td className="px-4 py-2">
                              <div className="text-sm font-medium text-gray-900">
                                {item.itemName}
                              </div>
                              <div className="text-xs text-gray-500">
                                {item.itemCode}
                              </div>
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.requestedQuantity}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.approvedQuantity !== undefined
                                ? item.approvedQuantity
                                : "-"}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.shippedQuantity !== undefined
                                ? item.shippedQuantity
                                : "-"}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {item.receivedQuantity !== undefined
                                ? item.receivedQuantity
                                : "-"}
                            </td>
                            <td className="px-4 py-2 text-right text-sm font-medium">
                              {formatCurrency(
                                item.requestedQuantity * item.unitCost,
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                <div className="bg-gray-50 rounded-lg p-4 text-right">
                  <span className="text-sm text-gray-600">Total Value: </span>
                  <span className="text-lg font-bold text-gray-900">
                    {formatCurrency(selectedTransfer.totalValue)}
                  </span>
                </div>

                {selectedTransfer.reason && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-1">
                      Reason
                    </h4>
                    <p className="text-sm text-gray-700">
                      {selectedTransfer.reason}
                    </p>
                  </div>
                )}

                {selectedTransfer.notes && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-1">
                      Notes
                    </h4>
                    <p className="text-sm text-gray-700">
                      {selectedTransfer.notes}
                    </p>
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedTransfer(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
                {selectedTransfer.status === "in-transit" && (
                  <button
                    onClick={() => {
                      const allItems = selectedTransfer.items.map((item) => ({
                        itemId: item.id,
                        receivedQuantity:
                          item.shippedQuantity ||
                          item.approvedQuantity ||
                          item.requestedQuantity,
                      }));
                      receiveTransferMutation.mutate({
                        id: selectedTransfer.id,
                        items: allItems,
                      });
                    }}
                    className="rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700"
                  >
                    Confirm Receipt
                  </button>
                )}
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}

      {/* Create Transfer Modal */}
      <Dialog
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
            <div className="border-b px-6 py-4">
              <Dialog.Title className="text-lg font-semibold text-gray-900">
                Create New Stock Transfer
              </Dialog.Title>
            </div>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                createTransferMutation.mutate({
                  sourceLocationName: formData.get("sourceLocation") as string,
                  destinationLocationName: formData.get(
                    "destinationLocation",
                  ) as string,
                  transferType: formData.get("transferType") as string,
                  priority: formData.get("priority") as string,
                  reason: (formData.get("reason") as string) || undefined,
                  notes: (formData.get("notes") as string) || undefined,
                  items: [
                    {
                      itemCode: formData.get("itemCode") as string,
                      itemName: formData.get("itemName") as string,
                      quantity:
                        parseInt(formData.get("quantity") as string) || 1,
                      unitCost:
                        parseFloat(formData.get("unitCost") as string) || 0,
                    },
                  ],
                });
              }}
              className="p-6 space-y-4"
            >
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Source Location *
                  </label>
                  <input
                    type="text"
                    name="sourceLocation"
                    required
                    placeholder="e.g., Main Warehouse"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Destination Location *
                  </label>
                  <input
                    type="text"
                    name="destinationLocation"
                    required
                    placeholder="e.g., Branch A"
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Transfer Type *
                  </label>
                  <select
                    name="transferType"
                    required
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="internal">Internal</option>
                    <option value="branch">Branch</option>
                    <option value="return">Return</option>
                    <option value="emergency">Emergency</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Priority *
                  </label>
                  <select
                    name="priority"
                    required
                    className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="low">Low</option>
                    <option value="normal">Normal</option>
                    <option value="high">High</option>
                    <option value="urgent">Urgent</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Reason
                </label>
                <input
                  type="text"
                  name="reason"
                  placeholder="Reason for transfer"
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                />
              </div>

              <div className="border-t pt-4 mt-4">
                <h4 className="text-sm font-medium text-gray-900 mb-3">
                  Transfer Item
                </h4>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Item Code *
                    </label>
                    <input
                      type="text"
                      name="itemCode"
                      required
                      className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Item Name *
                    </label>
                    <input
                      type="text"
                      name="itemName"
                      required
                      className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Quantity *
                    </label>
                    <input
                      type="number"
                      name="quantity"
                      min="1"
                      defaultValue="1"
                      required
                      className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Unit Cost
                    </label>
                    <input
                      type="number"
                      name="unitCost"
                      min="0"
                      step="0.01"
                      defaultValue="0"
                      className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Notes
                </label>
                <textarea
                  name="notes"
                  rows={2}
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                />
              </div>

              <div className="flex justify-end gap-3 pt-4 border-t">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createTransferMutation.isPending}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                  {createTransferMutation.isPending
                    ? "Creating..."
                    : "Create Transfer"}
                </button>
              </div>
            </form>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
}
