import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  PhotoIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  XCircleIcon,
  PrinterIcon,
} from "@heroicons/react/24/outline";
import { api } from "../../lib/api";

interface ImagingOrder {
  id: number;
  orderNumber: string;
  patientId: number;
  patientName: string;
  patientMRN: string;
  orderingDoctorId: number;
  orderingDoctorName: string;
  department: string;
  modality: string;
  examType: string;
  bodyPart: string;
  laterality?: "left" | "right" | "bilateral";
  priority: "routine" | "urgent" | "stat";
  status: "ordered" | "scheduled" | "in-progress" | "completed" | "cancelled";
  clinicalIndication: string;
  patientHistory?: string;
  scheduledDate?: string;
  scheduledTime?: string;
  room?: string;
  technologist?: string;
  notes?: string;
  orderedAt: string;
}

const statusConfig = {
  ordered: {
    label: "Ordered",
    color: "bg-gray-100 text-gray-800",
    icon: ClockIcon,
  },
  scheduled: {
    label: "Scheduled",
    color: "bg-blue-100 text-blue-800",
    icon: ClockIcon,
  },
  "in-progress": {
    label: "In Progress",
    color: "bg-yellow-100 text-yellow-800",
    icon: ClockIcon,
  },
  completed: {
    label: "Completed",
    color: "bg-green-100 text-green-800",
    icon: CheckCircleIcon,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-red-100 text-red-800",
    icon: XCircleIcon,
  },
};

const priorityConfig = {
  routine: { label: "Routine", color: "bg-gray-100 text-gray-800" },
  urgent: { label: "Urgent", color: "bg-orange-100 text-orange-800" },
  stat: { label: "STAT", color: "bg-red-100 text-red-800" },
};

const modalities = [
  "X-Ray",
  "CT",
  "MRI",
  "Ultrasound",
  "Mammography",
  "PET",
  "Fluoroscopy",
  "Nuclear Medicine",
];

export function ImagingOrders() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    ImagingOrder["status"] | "all"
  >("all");
  const [modalityFilter, setModalityFilter] = useState<string>("all");

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ["imaging-orders"],
    queryFn: () => api.get<ImagingOrder[]>("/api/radiology/orders"),
  });

  const filteredOrders = orders.filter((order) => {
    const matchesSearch =
      order.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.examType.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "all" || order.status === statusFilter;
    const matchesModality =
      modalityFilter === "all" || order.modality === modalityFilter;
    return matchesSearch && matchesStatus && matchesModality;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Imaging Orders</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage radiology imaging orders and scheduling
          </p>
        </div>
        <button
          onClick={() => setIsModalOpen(true)}
          className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <PlusIcon className="mr-2 h-5 w-5" />
          New Order
        </button>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-900">
            {orders.length}
          </div>
          <div className="text-sm text-gray-500">Total Orders</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-gray-600">
            {orders.filter((o) => o.status === "ordered").length}
          </div>
          <div className="text-sm text-gray-500">Pending Schedule</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-blue-600">
            {orders.filter((o) => o.status === "scheduled").length}
          </div>
          <div className="text-sm text-gray-500">Scheduled</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {orders.filter((o) => o.status === "in-progress").length}
          </div>
          <div className="text-sm text-gray-500">In Progress</div>
        </div>
        <div className="rounded-lg bg-white p-4 shadow">
          <div className="text-2xl font-bold text-red-600">
            {orders.filter((o) => o.priority === "stat").length}
          </div>
          <div className="text-sm text-gray-500">STAT</div>
        </div>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-md">
          <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search orders..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border border-gray-300 py-2 pl-10 pr-4 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as ImagingOrder["status"] | "all")
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
          value={modalityFilter}
          onChange={(e) => setModalityFilter(e.target.value)}
          className="rounded-md border border-gray-300 py-2 px-3 focus:border-blue-500 focus:outline-none"
        >
          <option value="all">All Modalities</option>
          {modalities.map((mod) => (
            <option key={mod} value={mod}>
              {mod}
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
            <PhotoIcon className="h-12 w-12 mb-2" />
            <p>No imaging orders found</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Order
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Exam
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Modality
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Priority
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Scheduled
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredOrders.map((order) => {
                const status = statusConfig[order.status];
                const priority = priorityConfig[order.priority];
                const StatusIcon = status.icon;

                return (
                  <tr
                    key={order.id}
                    className={`hover:bg-gray-50 ${order.priority === "stat" ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4">
                      <div className="font-mono text-sm font-medium text-gray-900">
                        {order.orderNumber}
                      </div>
                      <div className="text-xs text-gray-500">
                        {formatDate(order.orderedAt)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {order.patientName}
                      </div>
                      <div className="text-xs text-gray-500">
                        MRN: {order.patientMRN}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">
                        {order.examType}
                      </div>
                      <div className="text-xs text-gray-500">
                        {order.bodyPart}
                        {order.laterality && ` (${order.laterality})`}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                      {order.modality}
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
                      {order.scheduledDate ? (
                        <div>
                          <div>{formatDate(order.scheduledDate)}</div>
                          <div className="text-xs">
                            {order.scheduledTime} • Room {order.room}
                          </div>
                        </div>
                      ) : (
                        <span className="text-gray-400">Not scheduled</span>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm">
                      <div className="flex items-center justify-end gap-2">
                        <button className="text-gray-600 hover:text-gray-900">
                          <PrinterIcon className="h-5 w-5" />
                        </button>
                        {order.status === "ordered" && (
                          <button className="rounded bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700">
                            Schedule
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
