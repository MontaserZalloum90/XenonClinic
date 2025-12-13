import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ShieldExclamationIcon,
  MagnifyingGlassIcon,
  DocumentTextIcon,
  ExclamationTriangleIcon,
  ArrowDownTrayIcon,
  EyeIcon,
  LockClosedIcon,
} from "@heroicons/react/24/outline";
import { Dialog } from "@headlessui/react";
import { api } from "../../lib/api";

interface ControlledSubstanceRecord {
  id: number;
  transactionType:
    | "received"
    | "dispensed"
    | "destroyed"
    | "transferred"
    | "adjusted";
  drugName: string;
  drugNDC: string;
  schedule: "II" | "III" | "IV" | "V";
  lotNumber: string;
  expirationDate: string;
  quantity: number;
  quantityUnit: string;
  runningBalance: number;
  prescriptionNumber?: string;
  patientName?: string;
  patientId?: number;
  prescriberName?: string;
  prescriberDEA?: string;
  supplierName?: string;
  invoiceNumber?: string;
  witnessName?: string;
  witnessId?: number;
  reason?: string;
  notes?: string;
  recordedBy: string;
  recordedAt: string;
  verifiedBy?: string;
  verifiedAt?: string;
}

interface InventoryCount {
  drugName: string;
  drugNDC: string;
  schedule: "II" | "III" | "IV" | "V";
  currentBalance: number;
  quantityUnit: string;
  lastCountDate: string;
  lastCountBy: string;
  discrepancy: boolean;
}

const transactionTypeConfig = {
  received: { label: "Received", color: "bg-green-100 text-green-800" },
  dispensed: { label: "Dispensed", color: "bg-blue-100 text-blue-800" },
  destroyed: { label: "Destroyed", color: "bg-red-100 text-red-800" },
  transferred: { label: "Transferred", color: "bg-purple-100 text-purple-800" },
  adjusted: { label: "Adjusted", color: "bg-yellow-100 text-yellow-800" },
};

const scheduleConfig = {
  II: { label: "Schedule II", color: "bg-red-100 text-red-800" },
  III: { label: "Schedule III", color: "bg-orange-100 text-orange-800" },
  IV: { label: "Schedule IV", color: "bg-yellow-100 text-yellow-800" },
  V: { label: "Schedule V", color: "bg-green-100 text-green-800" },
};

export function ControlledSubstances() {
  const [searchTerm, setSearchTerm] = useState("");
  const [scheduleFilter, setScheduleFilter] = useState<
    ControlledSubstanceRecord["schedule"] | "all"
  >("all");
  const [transactionFilter, setTransactionFilter] = useState<
    ControlledSubstanceRecord["transactionType"] | "all"
  >("all");
  const [activeTab, setActiveTab] = useState<"transactions" | "inventory">(
    "transactions",
  );
  const [selectedRecord, setSelectedRecord] =
    useState<ControlledSubstanceRecord | null>(null);
  const [isInventoryCountOpen, setIsInventoryCountOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: records = [], isLoading: recordsLoading } = useQuery({
    queryKey: ["controlled-substances"],
    queryFn: () =>
      api.get<ControlledSubstanceRecord[]>(
        "/api/pharmacy/controlled-substances",
      ),
  });

  const { data: inventory = [], isLoading: inventoryLoading } = useQuery({
    queryKey: ["controlled-inventory"],
    queryFn: () =>
      api.get<InventoryCount[]>(
        "/api/pharmacy/controlled-substances/inventory",
      ),
  });

  const recordCountMutation = useMutation({
    mutationFn: (data: {
      drugNDC: string;
      countedQuantity: number;
      notes?: string;
    }) => api.post("/api/pharmacy/controlled-substances/count", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["controlled-inventory"] });
      queryClient.invalidateQueries({ queryKey: ["controlled-substances"] });
      setIsInventoryCountOpen(false);
    },
  });

  const filteredRecords = records.filter((record) => {
    const matchesSearch =
      record.drugName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.drugNDC.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (record.patientName &&
        record.patientName.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesSchedule =
      scheduleFilter === "all" || record.schedule === scheduleFilter;
    const matchesTransaction =
      transactionFilter === "all" ||
      record.transactionType === transactionFilter;
    return matchesSearch && matchesSchedule && matchesTransaction;
  });

  const filteredInventory = inventory.filter((item) => {
    const matchesSearch =
      item.drugName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.drugNDC.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesSchedule =
      scheduleFilter === "all" || item.schedule === scheduleFilter;
    return matchesSearch && matchesSchedule;
  });

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const scheduleIICount = inventory.filter((i) => i.schedule === "II").length;
  const discrepancyCount = inventory.filter((i) => i.discrepancy).length;
  const todayTransactions = records.filter(
    (r) => new Date(r.recordedAt).toDateString() === new Date().toDateString(),
  ).length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Controlled Substances
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            DEA-compliant controlled substance tracking and inventory
          </p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setIsInventoryCountOpen(true)}
            className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
          >
            <LockClosedIcon className="mr-2 h-5 w-5" />
            Record Count
          </button>
          <button className="inline-flex items-center rounded-md bg-white border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
            <ArrowDownTrayIcon className="mr-2 h-5 w-5" />
            Export Log
          </button>
        </div>
      </div>

      {discrepancyCount > 0 && (
        <div className="rounded-lg bg-red-50 border border-red-200 p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                Inventory Discrepancies Detected
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {discrepancyCount} controlled substance(s) have count
                discrepancies. Immediate investigation required.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {inventory.length}
          </div>
          <div className="text-sm text-gray-500">Total Items</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {scheduleIICount}
          </div>
          <div className="text-sm text-gray-500">Schedule II</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {todayTransactions}
          </div>
          <div className="text-sm text-gray-500">Today's Transactions</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div
            className={`text-2xl font-bold ${discrepancyCount > 0 ? "text-red-600" : "text-green-600"}`}
          >
            {discrepancyCount > 0 ? discrepancyCount : "None"}
          </div>
          <div className="text-sm text-gray-500">Discrepancies</div>
        </div>
      </div>

      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab("transactions")}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === "transactions"
                ? "border-blue-500 text-blue-600"
                : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
            }`}
          >
            Transaction Log
          </button>
          <button
            onClick={() => setActiveTab("inventory")}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === "inventory"
                ? "border-blue-500 text-blue-600"
                : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
            }`}
          >
            Inventory Counts
          </button>
        </nav>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by drug name, NDC, or patient..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={scheduleFilter}
          onChange={(e) =>
            setScheduleFilter(
              e.target.value as ControlledSubstanceRecord["schedule"] | "all",
            )
          }
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Schedules</option>
          {Object.entries(scheduleConfig).map(([key, value]) => (
            <option key={key} value={key}>
              {value.label}
            </option>
          ))}
        </select>
        {activeTab === "transactions" && (
          <select
            value={transactionFilter}
            onChange={(e) =>
              setTransactionFilter(
                e.target.value as
                  | ControlledSubstanceRecord["transactionType"]
                  | "all",
              )
            }
            className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
          >
            <option value="all">All Transactions</option>
            {Object.entries(transactionTypeConfig).map(([key, value]) => (
              <option key={key} value={key}>
                {value.label}
              </option>
            ))}
          </select>
        )}
      </div>

      <div className="overflow-hidden rounded-lg bg-white shadow">
        {activeTab === "transactions" ? (
          recordsLoading ? (
            <div className="flex h-64 items-center justify-center">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
            </div>
          ) : filteredRecords.length === 0 ? (
            <div className="flex h-64 flex-col items-center justify-center text-gray-500">
              <DocumentTextIcon className="h-12 w-12 mb-2" />
              <p>No transaction records found</p>
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Date/Time
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Drug
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Schedule
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Qty
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Balance
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    Reference
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {filteredRecords.map((record) => {
                  const transaction =
                    transactionTypeConfig[record.transactionType];
                  const schedule = scheduleConfig[record.schedule];

                  return (
                    <tr key={record.id} className="hover:bg-gray-50">
                      <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                        {formatDateTime(record.recordedAt)}
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm font-medium text-gray-900">
                          {record.drugName}
                        </div>
                        <div className="text-xs text-gray-500">
                          NDC: {record.drugNDC}
                        </div>
                        <div className="text-xs text-gray-400">
                          Lot: {record.lotNumber}
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${schedule.color}`}
                        >
                          {record.schedule}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${transaction.color}`}
                        >
                          {transaction.label}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm">
                        <span
                          className={
                            record.transactionType === "received"
                              ? "text-green-600"
                              : "text-red-600"
                          }
                        >
                          {record.transactionType === "received" ? "+" : "-"}
                          {record.quantity} {record.quantityUnit}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                        {record.runningBalance} {record.quantityUnit}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-500">
                        {record.prescriptionNumber && (
                          <div>Rx: {record.prescriptionNumber}</div>
                        )}
                        {record.patientName && (
                          <div className="text-xs">{record.patientName}</div>
                        )}
                        {record.invoiceNumber && (
                          <div>Invoice: {record.invoiceNumber}</div>
                        )}
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedRecord(record)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          <EyeIcon className="h-5 w-5" />
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )
        ) : inventoryLoading ? (
          <div className="flex h-64 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
          </div>
        ) : filteredInventory.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center text-gray-500">
            <ShieldExclamationIcon className="h-12 w-12 mb-2" />
            <p>No inventory records found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Drug
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  NDC
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Schedule
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Balance
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Last Count
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredInventory.map((item, index) => {
                const schedule = scheduleConfig[item.schedule];

                return (
                  <tr
                    key={index}
                    className={`hover:bg-gray-50 ${item.discrepancy ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {item.drugName}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-mono text-gray-500">
                      {item.drugNDC}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${schedule.color}`}
                      >
                        {schedule.label}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                      {item.currentBalance} {item.quantityUnit}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      <div>{formatDate(item.lastCountDate)}</div>
                      <div className="text-xs">{item.lastCountBy}</div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      {item.discrepancy ? (
                        <span className="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                          <ExclamationTriangleIcon className="h-3.5 w-3.5 mr-1" />
                          Discrepancy
                        </span>
                      ) : (
                        <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                          Verified
                        </span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {selectedRecord && (
        <Dialog
          open={!!selectedRecord}
          onClose={() => setSelectedRecord(null)}
          className="relative z-50"
        >
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4">
            <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-lg bg-white shadow-xl max-h-[90vh] overflow-y-auto">
              <div className="border-b px-6 py-4">
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${scheduleConfig[selectedRecord.schedule].color}`}
                  >
                    {scheduleConfig[selectedRecord.schedule].label}
                  </span>
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${transactionTypeConfig[selectedRecord.transactionType].color}`}
                  >
                    {
                      transactionTypeConfig[selectedRecord.transactionType]
                        .label
                    }
                  </span>
                </div>
                <Dialog.Title className="text-xl font-semibold text-gray-900 mt-2">
                  {selectedRecord.drugName}
                </Dialog.Title>
                <p className="text-sm text-gray-500">
                  NDC: {selectedRecord.drugNDC} | Lot:{" "}
                  {selectedRecord.lotNumber}
                </p>
              </div>
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Quantity
                    </h4>
                    <p className="font-medium">
                      <span
                        className={
                          selectedRecord.transactionType === "received"
                            ? "text-green-600"
                            : "text-red-600"
                        }
                      >
                        {selectedRecord.transactionType === "received"
                          ? "+"
                          : "-"}
                        {selectedRecord.quantity} {selectedRecord.quantityUnit}
                      </span>
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Running Balance
                    </h4>
                    <p className="font-medium">
                      {selectedRecord.runningBalance}{" "}
                      {selectedRecord.quantityUnit}
                    </p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500">
                      Recorded By
                    </h4>
                    <p className="font-medium">{selectedRecord.recordedBy}</p>
                    <p className="text-sm text-gray-500">
                      {formatDateTime(selectedRecord.recordedAt)}
                    </p>
                  </div>
                  {selectedRecord.verifiedBy && (
                    <div>
                      <h4 className="text-sm font-medium text-gray-500">
                        Verified By
                      </h4>
                      <p className="font-medium">{selectedRecord.verifiedBy}</p>
                      <p className="text-sm text-gray-500">
                        {selectedRecord.verifiedAt &&
                          formatDateTime(selectedRecord.verifiedAt)}
                      </p>
                    </div>
                  )}
                </div>

                {selectedRecord.transactionType === "dispensed" && (
                  <div className="bg-blue-50 rounded-lg p-4 space-y-3">
                    <h4 className="text-sm font-medium text-blue-900">
                      Dispensing Details
                    </h4>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-blue-700">Rx Number:</span>
                        <span className="ml-2 font-medium">
                          {selectedRecord.prescriptionNumber}
                        </span>
                      </div>
                      <div>
                        <span className="text-blue-700">Patient:</span>
                        <span className="ml-2 font-medium">
                          {selectedRecord.patientName}
                        </span>
                      </div>
                      <div>
                        <span className="text-blue-700">Prescriber:</span>
                        <span className="ml-2 font-medium">
                          {selectedRecord.prescriberName}
                        </span>
                      </div>
                      <div>
                        <span className="text-blue-700">DEA:</span>
                        <span className="ml-2 font-mono">
                          {selectedRecord.prescriberDEA}
                        </span>
                      </div>
                    </div>
                  </div>
                )}

                {selectedRecord.transactionType === "received" && (
                  <div className="bg-green-50 rounded-lg p-4 space-y-3">
                    <h4 className="text-sm font-medium text-green-900">
                      Receipt Details
                    </h4>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-green-700">Supplier:</span>
                        <span className="ml-2 font-medium">
                          {selectedRecord.supplierName}
                        </span>
                      </div>
                      <div>
                        <span className="text-green-700">Invoice:</span>
                        <span className="ml-2 font-medium">
                          {selectedRecord.invoiceNumber}
                        </span>
                      </div>
                      <div>
                        <span className="text-green-700">Expiration:</span>
                        <span className="ml-2 font-medium">
                          {formatDate(selectedRecord.expirationDate)}
                        </span>
                      </div>
                    </div>
                  </div>
                )}

                {selectedRecord.witnessName && (
                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900">
                      Witness
                    </h4>
                    <p className="text-sm text-gray-700">
                      {selectedRecord.witnessName}
                    </p>
                  </div>
                )}

                {selectedRecord.notes && (
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-1">
                      Notes
                    </h4>
                    <p className="text-sm text-gray-700">
                      {selectedRecord.notes}
                    </p>
                  </div>
                )}
              </div>
              <div className="border-t px-6 py-4 flex justify-end">
                <button
                  onClick={() => setSelectedRecord(null)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Close
                </button>
              </div>
            </Dialog.Panel>
          </div>
        </Dialog>
      )}

      <Dialog
        open={isInventoryCountOpen}
        onClose={() => setIsInventoryCountOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-lg w-full rounded-lg bg-white shadow-xl">
            <div className="border-b px-6 py-4">
              <Dialog.Title className="text-lg font-semibold text-gray-900">
                Record Inventory Count
              </Dialog.Title>
            </div>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                recordCountMutation.mutate({
                  drugNDC: formData.get("drugNDC") as string,
                  countedQuantity: parseInt(
                    formData.get("countedQuantity") as string,
                  ),
                  notes: formData.get("notes") as string,
                });
              }}
              className="p-6 space-y-4"
            >
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Select Drug
                </label>
                <select
                  name="drugNDC"
                  required
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                >
                  <option value="">Select a controlled substance...</option>
                  {inventory.map((item, index) => (
                    <option key={index} value={item.drugNDC}>
                      {item.drugName} ({item.schedule}) - Current:{" "}
                      {item.currentBalance}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Counted Quantity
                </label>
                <input
                  type="number"
                  name="countedQuantity"
                  required
                  min="0"
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Notes (Optional)
                </label>
                <textarea
                  name="notes"
                  rows={3}
                  className="w-full rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
                />
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <input
                  type="checkbox"
                  id="witness"
                  required
                  className="rounded border-gray-300"
                />
                <label htmlFor="witness">
                  Witness present for count verification
                </label>
              </div>
              <div className="flex justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsInventoryCountOpen(false)}
                  className="rounded-md bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
                >
                  Record Count
                </button>
              </div>
            </form>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
}
