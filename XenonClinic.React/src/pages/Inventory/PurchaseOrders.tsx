import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ShoppingCartIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
  TruckIcon,
  DocumentTextIcon,
  PrinterIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface PurchaseOrderItem {
  id: number;
  itemCode: string;
  itemName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  receivedQuantity: number;
}

interface PurchaseOrder {
  id: number;
  poNumber: string;
  vendorId: number;
  vendorName: string;
  vendorContact?: string;
  vendorEmail?: string;
  status:
    | "draft"
    | "submitted"
    | "approved"
    | "ordered"
    | "partial"
    | "received"
    | "cancelled";
  orderDate: string;
  expectedDelivery?: string;
  actualDelivery?: string;
  items: PurchaseOrderItem[];
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  totalAmount: number;
  paymentTerms?: string;
  shippingAddress?: string;
  notes?: string;
  createdBy: string;
  approvedBy?: string;
  approvedAt?: string;
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
    icon: ClockIcon,
  },
  approved: {
    label: "Approved",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  ordered: {
    label: "Ordered",
    color: "bg-purple-100 text-purple-800",
    icon: ShoppingCartIcon,
  },
  partial: {
    label: "Partial",
    color: "bg-yellow-100 text-yellow-800",
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

export function PurchaseOrders() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    PurchaseOrder["status"] | "all"
  >("all");
  const [selectedPO, setSelectedPO] = useState<PurchaseOrder | null>(null);
  const queryClient = useQueryClient();

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ["purchase-orders"],
    queryFn: () => api.get<PurchaseOrder[]>("/api/inventory/purchase-orders"),
  });

  const approvePOMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/inventory/purchase-orders/${id}/approve`),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["purchase-orders"] }),
  });

  const submitPOMutation = useMutation({
    mutationFn: (id: number) =>
      api.post(`/api/inventory/purchase-orders/${id}/submit`),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["purchase-orders"] }),
  });

  const receiveItemsMutation = useMutation({
    mutationFn: ({
      id,
      items,
    }: {
      id: number;
      items: { itemId: number; quantity: number }[];
    }) => api.post(`/api/inventory/purchase-orders/${id}/receive`, { items }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["purchase-orders"] });
      setSelectedPO(null);
    },
  });

  const filteredOrders = orders.filter((order) => {
    const matchesSearch =
      order.poNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.vendorName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || order.status === statusFilter;
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

  const totalOpen = orders
    .filter((o) =>
      ["submitted", "approved", "ordered", "partial"].includes(o.status),
    )
    .reduce((sum, o) => sum + o.totalAmount, 0);
  const pendingApproval = orders.filter((o) => o.status === "submitted").length;
  const awaitingDelivery = orders.filter((o) =>
    ["ordered", "partial"].includes(o.status),
  ).length;
  const thisMonthTotal = orders
    .filter((o) => {
      const orderDate = new Date(o.orderDate);
      const now = new Date();
      return (
        orderDate.getMonth() === now.getMonth() &&
        orderDate.getFullYear() === now.getFullYear()
      );
    })
    .reduce((sum, o) => sum + o.totalAmount, 0);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Purchase Orders</h1>
          <p className="mt-1 text-sm text-gray-500">
            Create and manage vendor purchase orders
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New PO
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {formatCurrency(totalOpen)}
          </div>
          <div className="text-sm text-gray-500">Open Orders</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {pendingApproval}
          </div>
          <div className="text-sm text-gray-500">Pending Approval</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-purple-600">
            {awaitingDelivery}
          </div>
          <div className="text-sm text-gray-500">Awaiting Delivery</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">
            {formatCurrency(thisMonthTotal)}
          </div>
          <div className="text-sm text-gray-500">This Month</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by PO number or vendor..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as PurchaseOrder["status"] | "all")
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
        ) : filteredOrders.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ShoppingCartIcon className="h-12 w-12 mb-2" />
            <p>No purchase orders found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  PO Number
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Vendor
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Items
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Total
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Order Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Expected
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
              {filteredOrders.map((order) => {
                const status = statusConfig[order.status];
                const StatusIcon = status.icon;

                return (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm font-medium text-gray-900">
                        {order.poNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        By: {order.createdBy}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {order.vendorName}
                      </div>
                      {order.vendorContact && (
                        <div className="text-xs text-gray-500">
                          {order.vendorContact}
                        </div>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {order.items.length} item(s)
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                      {formatCurrency(order.totalAmount)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatDate(order.orderDate)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {order.expectedDelivery
                        ? formatDate(order.expectedDelivery)
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
                          onClick={() => setSelectedPO(order)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          View
                        </button>
                        {order.status === "draft" && (
                          <button
                            onClick={() => submitPOMutation.mutate(order.id)}
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Submit
                          </button>
                        )}
                        {order.status === "submitted" && (
                          <button
                            onClick={() => approvePOMutation.mutate(order.id)}
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Approve
                          </button>
                        )}
                        {["ordered", "partial"].includes(order.status) && (
                          <button
                            onClick={() => setSelectedPO(order)}
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

      {selectedPO && (
        <Dialog
          open={!!selectedPO}
          onClose={() => setSelectedPO(null)}
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
                        {selectedPO.poNumber}
                      </span>
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusConfig[selectedPO.status].color}`}
                      >
                        {statusConfig[selectedPO.status].label}
                      </span>
                    </div>
                    <Dialog.Title className="text-xl font-semibold text-gray-900 mt-1">
                      {selectedPO.vendorName}
                    </Dialog.Title>
                  </div>
                  <button className="text-gray-400 hover:text-gray-600">
                    <PrinterIcon className="h-5 w-5" />
                  </button>
                </div>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Order Date
                    </h4>
                    <p className="font-medium">
                      {formatDate(selectedPO.orderDate)}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Expected Delivery
                    </h4>
                    <p className="font-medium">
                      {selectedPO.expectedDelivery
                        ? formatDate(selectedPO.expectedDelivery)
                        : "Not specified"}
                    </p>
                  </div>
                  {selectedPO.paymentTerms && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Payment Terms
                      </h4>
                      <p className="font-medium">{selectedPO.paymentTerms}</p>
                    </div>
                  )}
                  {selectedPO.approvedBy && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Approved By
                      </h4>
                      <p className="font-medium">{selectedPO.approvedBy}</p>
                      {selectedPO.approvedAt && (
                        <p className="text-xs text-gray-500">
                          {formatDate(selectedPO.approvedAt)}
                        </p>
                      )}
                    </div>
                  )}
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-3">
                    Order Items
                  </h4>
                  <div className="border rounded-lg overflow-hidden">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                            Item
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Qty
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Unit Price
                          </th>
                          <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                            Total
                          </th>
                          {["ordered", "partial", "received"].includes(
                            selectedPO.status,
                          ) && (
                            <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">
                              Received
                            </th>
                          )}
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200">
                        {selectedPO.items.map((item) => (
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
                              {item.quantity}
                            </td>
                            <td className="px-4 py-2 text-right text-sm">
                              {formatCurrency(item.unitPrice)}
                            </td>
                            <td className="px-4 py-2 text-right text-sm font-medium">
                              {formatCurrency(item.totalPrice)}
                            </td>
                            {["ordered", "partial", "received"].includes(
                              selectedPO.status,
                            ) && (
                              <td className="px-4 py-2 text-right text-sm">
                                <span
                                  className={
                                    item.receivedQuantity < item.quantity
                                      ? "text-yellow-600"
                                      : "text-green-600"
                                  }
                                >
                                  {item.receivedQuantity}/{item.quantity}
                                </span>
                              </td>
                            )}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Subtotal:</span>
                      <span className="font-medium">
                        {formatCurrency(selectedPO.subtotal)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Tax:</span>
                      <span className="font-medium">
                        {formatCurrency(selectedPO.taxAmount)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Shipping:</span>
                      <span className="font-medium">
                        {formatCurrency(selectedPO.shippingCost)}
                      </span>
                    </div>
                    <div className="flex justify-between pt-2 border-t border-gray-200">
                      <span className="font-medium text-gray-900">Total:</span>
                      <span className="font-bold text-lg">
                        {formatCurrency(selectedPO.totalAmount)}
                      </span>
                    </div>
                  </div>
                </div>

                {selectedPO.notes && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-1">
                      Notes
                    </h4>
                    <p className="text-sm text-gray-700">{selectedPO.notes}</p>
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedPO(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
                {["ordered", "partial"].includes(selectedPO.status) && (
                  <button
                    onClick={() => {
                      const allItems = selectedPO.items.map((item) => ({
                        itemId: item.id,
                        quantity: item.quantity - item.receivedQuantity,
                      }));
                      receiveItemsMutation.mutate({
                        id: selectedPO.id,
                        items: allItems,
                      });
                    }}
                    className="rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700"
                  >
                    Receive All Items
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
