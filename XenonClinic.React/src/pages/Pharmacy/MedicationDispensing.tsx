import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  PrinterIcon,
  QueueListIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface DispensingItem {
  id: number;
  prescriptionId: number;
  prescriptionNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  patientDOB: string;
  allergies?: string[];
  medicationId: number;
  medicationName: string;
  medicationStrength: string;
  dosageForm: string;
  quantity: number;
  quantityDispensed: number;
  daysSupply: number;
  directions: string;
  refillsRemaining: number;
  prescriberId: number;
  prescriberName: string;
  status: "pending" | "in-progress" | "ready" | "dispensed" | "on-hold";
  priority: "routine" | "urgent" | "stat";
  waitTime: number; // minutes
  insuranceVerified: boolean;
  copayAmount?: number;
  notes?: string;
  orderedAt: string;
  filledBy?: string;
  verifiedBy?: string;
  dispensedAt?: string;
}

const statusConfig = {
  pending: {
    label: "Pending",
    color: "bg-gray-100 text-gray-800",
    icon: ClockIcon,
  },
  "in-progress": {
    label: "In Progress",
    color: "bg-blue-100 text-blue-800",
    icon: ClockIcon,
  },
  ready: {
    label: "Ready",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  dispensed: {
    label: "Dispensed",
    color: "bg-purple-100 text-purple-800",
    icon: CheckCircleIcon,
  },
  "on-hold": {
    label: "On Hold",
    color: "bg-yellow-100 text-yellow-800",
    icon: ExclamationTriangleIcon,
  },
};

const priorityConfig = {
  routine: { label: "Routine", color: "bg-gray-100 text-gray-800" },
  urgent: { label: "Urgent", color: "bg-orange-100 text-orange-800" },
  stat: { label: "STAT", color: "bg-red-100 text-red-800" },
};

export function MedicationDispensing() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    DispensingItem["status"] | "all"
  >("all");
  const [selectedItem, setSelectedItem] = useState<DispensingItem | null>(null);
  const [isDispenseModalOpen, setIsDispenseModalOpen] = useState(false);
  const [quantityToDispense, setQuantityToDispense] = useState<number>(0);
  const queryClient = useQueryClient();

  const { data: items = [], isLoading } = useQuery({
    queryKey: ["dispensing-queue"],
    queryFn: () => api.get<DispensingItem[]>("/api/pharmacy/dispensing"),
  });

  const dispenseMutation = useMutation({
    mutationFn: ({
      id,
      quantityDispensed,
    }: {
      id: number;
      quantityDispensed: number;
    }) =>
      api.post(`/api/pharmacy/dispensing/${id}/dispense`, {
        quantityDispensed,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["dispensing-queue"] });
      setIsDispenseModalOpen(false);
      setSelectedItem(null);
    },
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      api.patch(`/api/pharmacy/dispensing/${id}/status`, { status }),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["dispensing-queue"] }),
  });

  const filteredItems = items.filter((item) => {
    const matchesSearch =
      item.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.prescriptionNumber
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      item.medicationName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || item.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const formatTime = (minutes: number) => {
    if (minutes < 60) return `${minutes}m`;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  };

  const pendingCount = items.filter((i) => i.status === "pending").length;
  const readyCount = items.filter((i) => i.status === "ready").length;
  const statCount = items.filter(
    (i) => i.priority === "stat" && i.status !== "dispensed",
  ).length;
  const avgWaitTime =
    items.length > 0
      ? Math.round(
          items
            .filter((i) => i.status !== "dispensed")
            .reduce((sum, i) => sum + i.waitTime, 0) /
            items.filter((i) => i.status !== "dispensed").length,
        )
      : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Medication Dispensing
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Process and dispense prescription medications
          </p>
        </div>
        <button className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
          <PrinterIcon className="mr-2 h-5 w-5" />
          Print Queue
        </button>
      </div>

      {statCount > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                STAT Orders Pending
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {statCount} STAT prescription(s) require immediate attention.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">{items.length}</div>
          <div className="text-sm text-gray-500">Total in Queue</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-600">{pendingCount}</div>
          <div className="text-sm text-gray-500">Pending</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-green-600">{readyCount}</div>
          <div className="text-sm text-gray-500">Ready for Pickup</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {formatTime(avgWaitTime)}
          </div>
          <div className="text-sm text-gray-500">Avg Wait Time</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by patient, Rx number, or medication..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as DispensingItem["status"] | "all")
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
        ) : filteredItems.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <QueueListIcon className="h-12 w-12 mb-2" />
            <p>No items in dispensing queue</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Medication
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Rx #
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Qty
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Priority
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Wait
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredItems.map((item) => {
                const status = statusConfig[item.status];
                const priority = priorityConfig[item.priority];
                const StatusIcon = status.icon;

                return (
                  <tr
                    key={item.id}
                    className={`hover:bg-gray-50 ${item.priority === "stat" ? "bg-red-50" : ""} ${item.allergies && item.allergies.length > 0 ? "border-l-4 border-l-orange-500" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {item.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {item.patientMRN}
                      </div>
                      {item.allergies && item.allergies.length > 0 && (
                        <div className="text-xs text-orange-600 font-medium mt-1">
                          Allergies: {item.allergies.join(", ")}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {item.medicationName}
                      </div>
                      <div className="text-xs text-gray-500">
                        {item.medicationStrength} {item.dosageForm}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm text-gray-900">
                        {item.prescriptionNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        Refills: {item.refillsRemaining}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {item.quantity} ({item.daysSupply} days)
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
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {formatTime(item.waitTime)}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        {item.status === "pending" && (
                          <button
                            onClick={() =>
                              updateStatusMutation.mutate({
                                id: item.id,
                                status: "in-progress",
                              })
                            }
                            className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Start Fill
                          </button>
                        )}
                        {item.status === "in-progress" && (
                          <button
                            onClick={() =>
                              updateStatusMutation.mutate({
                                id: item.id,
                                status: "ready",
                              })
                            }
                            className="rounded bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700"
                          >
                            Mark Ready
                          </button>
                        )}
                        {item.status === "ready" && (
                          <button
                            onClick={() => {
                              setSelectedItem(item);
                              setQuantityToDispense(item.quantity);
                              setIsDispenseModalOpen(true);
                            }}
                            className="rounded bg-purple-600 px-3 py-1 text-xs font-medium text-white hover:bg-purple-700"
                          >
                            Dispense
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

      {selectedItem && (
        <Dialog
          open={isDispenseModalOpen}
          onClose={() => setIsDispenseModalOpen(false)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white shadow-xl">
              <div className="border-b px-6 py-4">
                <Dialog.Title className="text-lg font-semibold text-gray-900">
                  Dispense Medication
                </Dialog.Title>
              </div>
              <div className="p-6 space-y-4">
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <p className="text-xs text-gray-500">Patient</p>
                      <p className="font-medium">{selectedItem.patientName}</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Rx Number</p>
                      <p className="font-mono">
                        {selectedItem.prescriptionNumber}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="bg-blue-50 rounded-lg p-4">
                  <p className="font-medium text-blue-900">
                    {selectedItem.medicationName}
                  </p>
                  <p className="text-sm text-blue-700">
                    {selectedItem.medicationStrength} {selectedItem.dosageForm}
                  </p>
                  <p className="text-sm text-blue-600 mt-2">
                    {selectedItem.directions}
                  </p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Quantity Ordered
                    </label>
                    <input
                      type="text"
                      value={selectedItem.quantity}
                      disabled
                      className="w-full rounded-md border border-gray-300 bg-gray-100 py-2 px-3"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Quantity to Dispense
                    </label>
                    <input
                      type="number"
                      value={quantityToDispense}
                      onChange={(e) => {
                        const val = parseInt(e.target.value, 10);
                        if (
                          !isNaN(val) &&
                          val >= 0 &&
                          val <= selectedItem.quantity
                        ) {
                          setQuantityToDispense(val);
                        }
                      }}
                      min={0}
                      max={selectedItem.quantity}
                      className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                    />
                    {quantityToDispense > selectedItem.quantity && (
                      <p className="text-red-600 text-xs mt-1">
                        Cannot exceed ordered quantity
                      </p>
                    )}
                  </div>
                </div>

                {selectedItem.copayAmount !== undefined && (
                  <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-yellow-800">
                        Copay Due:
                      </span>
                      <span className="text-lg font-bold text-yellow-900">
                        ${selectedItem.copayAmount.toFixed(2)}
                      </span>
                    </div>
                  </div>
                )}

                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <input
                    type="checkbox"
                    id="counseling"
                    className="rounded border-gray-300"
                  />
                  <label htmlFor="counseling">
                    Patient counseling completed
                  </label>
                </div>
              </div>
              <div className="border-t px-6 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setIsDispenseModalOpen(false)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Cancel
                </button>
                <button
                  onClick={() =>
                    dispenseMutation.mutate({
                      id: selectedItem.id,
                      quantityDispensed: quantityToDispense,
                    })
                  }
                  disabled={
                    quantityToDispense <= 0 ||
                    quantityToDispense > selectedItem.quantity
                  }
                  className="rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
                >
                  Complete Dispensing
                </button>
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}
    </div>
  );
}
