import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  ShieldCheckIcon,
  ExclamationTriangleIcon,
  ClockIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { VaccinationRecord, CreateVaccinationRequest } from "../../types/pediatrics";
import { VaccinationStatus, VaccineName } from "../../types/pediatrics";
import { pediatricsApi } from "../../lib/api";

interface VaccinationsProps {
  patientId?: number;
}

export const Vaccinations = ({ patientId }: VaccinationsProps = {}) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<
    VaccinationRecord | undefined
  >(undefined);

  // Fetch vaccination records from API
  const { data: records, isLoading } = useQuery<VaccinationRecord[]>({
    queryKey: ["vaccination-records", patientId],
    queryFn: async () => {
      if (patientId) {
        const response = await pediatricsApi.getVaccinationsByPatient(patientId);
        return response.data?.data ?? response.data ?? [];
      }
      return [];
    },
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: CreateVaccinationRequest) => pediatricsApi.createVaccination(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["vaccination-records"] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateVaccinationRequest> }) =>
      pediatricsApi.updateVaccination(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["vaccination-records"] }),
  });

  void createMutation;
  void updateMutation;
  void VaccinationStatus;
  void VaccineName;

  const filteredRecords = records?.filter((record) => {
    const matchesSearch =
      !searchTerm ||
      record.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.vaccineName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || record.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedRecord(undefined);
    setIsModalOpen(true);
  };

  const handleView = (record: VaccinationRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedRecord(undefined);
  };

  const getStatusBadge = (status?: VaccinationStatus) => {
    const config: Record<string, { className: string; label: string }> = {
      [VaccinationStatus.Scheduled]: {
        className: "bg-blue-100 text-blue-800",
        label: "Scheduled",
      },
      [VaccinationStatus.Administered]: {
        className: "bg-green-100 text-green-800",
        label: "Administered",
      },
      [VaccinationStatus.Missed]: {
        className: "bg-red-100 text-red-800",
        label: "Missed",
      },
      [VaccinationStatus.Declined]: {
        className: "bg-gray-100 text-gray-800",
        label: "Declined",
      },
      [VaccinationStatus.Delayed]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Delayed",
      },
    };
    const c = status
      ? config[status]
      : { className: "bg-gray-100 text-gray-800", label: "Unknown" };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c?.className || "bg-gray-100 text-gray-800"}`}
      >
        {c?.label || status}
      </span>
    );
  };

  const overdueCount =
    records?.filter((r) => r.status === VaccinationStatus.Missed).length || 0;
  const scheduledCount =
    records?.filter((r) => r.status === VaccinationStatus.Scheduled).length ||
    0;
  const administeredToday =
    records?.filter((r) => {
      if (r.status !== VaccinationStatus.Administered || !r.administeredDate)
        return false;
      return (
        new Date(r.administeredDate).toDateString() ===
        new Date().toDateString()
      );
    }).length || 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Vaccination Records
          </h1>
          <p className="text-gray-600 mt-1">
            Track and manage pediatric immunizations
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          Record Vaccination
        </button>
      </div>

      {/* Alert for overdue vaccinations */}
      {overdueCount > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-red-800">
                Overdue Vaccinations
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {overdueCount} vaccination(s) are overdue and need attention.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ShieldCheckIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Records</p>
              <p className="text-2xl font-bold text-gray-900">
                {records?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {administeredToday}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Today</p>
              <p className="text-2xl font-bold text-green-600">
                {administeredToday}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ClockIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Scheduled</p>
              <p className="text-2xl font-bold text-blue-600">
                {scheduledCount}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-8 w-8 text-red-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Overdue</p>
              <p className="text-2xl font-bold text-red-600">{overdueCount}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
              <input
                type="text"
                placeholder="Search by patient name or vaccine..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Status</option>
              {Object.entries(VaccinationStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Records Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Vaccine
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Dose
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Administered By
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
                    <p className="mt-2">Loading records...</p>
                  </td>
                </tr>
              ) : filteredRecords && filteredRecords.length > 0 ? (
                filteredRecords.map((record) => (
                  <tr
                    key={record.id}
                    className={`hover:bg-gray-50 ${record.status === VaccinationStatus.Missed ? "bg-red-50" : ""}`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {record.patientName || `Patient #${record.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="px-2 py-1 rounded bg-blue-50 text-blue-700 text-sm font-medium">
                        {record.vaccineName}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      Dose {record.doseNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {record.administeredDate
                        ? format(
                            new Date(record.administeredDate),
                            "MMM d, yyyy",
                          )
                        : record.scheduledDate
                          ? `Scheduled: ${format(new Date(record.scheduledDate), "MMM d, yyyy")}`
                          : "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(record.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {record.administeredBy || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleView(record)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {record.status === VaccinationStatus.Scheduled && (
                        <button className="text-green-600 hover:text-green-900">
                          Administer
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    {searchTerm
                      ? "No records found matching your search."
                      : "No vaccination records found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <VaccinationModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        record={selectedRecord}
      />
    </div>
  );
};

// Vaccination Modal
interface VaccinationModalProps {
  isOpen: boolean;
  onClose: () => void;
  record?: VaccinationRecord;
}

const VaccinationModal = ({
  isOpen,
  onClose,
  record,
}: VaccinationModalProps) => {
  const [formData, setFormData] = useState({
    patientId: record?.patientId || 0,
    vaccineName: record?.vaccineName || "",
    doseNumber: record?.doseNumber || 1,
    administeredDate:
      record?.administeredDate || new Date().toISOString().split("T")[0],
    batchNumber: record?.batchNumber || "",
    site: record?.site || "",
    administeredBy: record?.administeredBy || "",
    notes: record?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Implement API call to save vaccination record
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]:
        name === "patientId" || name === "doseNumber" ? Number(value) : value,
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-lg w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {record ? "Vaccination Details" : "Record Vaccination"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            {record ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Patient
                    </label>
                    <p className="text-gray-900">{record.patientName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Vaccine
                    </label>
                    <p className="text-gray-900">{record.vaccineName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Dose
                    </label>
                    <p className="text-gray-900">Dose {record.doseNumber}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Status
                    </label>
                    <p className="text-gray-900">{record.status}</p>
                  </div>
                  {record.batchNumber && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Batch Number
                      </label>
                      <p className="text-gray-900">{record.batchNumber}</p>
                    </div>
                  )}
                  {record.site && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Site
                      </label>
                      <p className="text-gray-900">{record.site}</p>
                    </div>
                  )}
                  {record.administeredBy && (
                    <div className="col-span-2">
                      <label className="block text-sm font-medium text-gray-500">
                        Administered By
                      </label>
                      <p className="text-gray-900">{record.administeredBy}</p>
                    </div>
                  )}
                  {record.nextDueDate && (
                    <div className="col-span-2">
                      <label className="block text-sm font-medium text-gray-500">
                        Next Dose Due
                      </label>
                      <p className="text-gray-900">
                        {format(new Date(record.nextDueDate), "MMM d, yyyy")}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Patient ID
                    </label>
                    <input
                      type="number"
                      name="patientId"
                      value={formData.patientId}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Vaccine
                    </label>
                    <select
                      name="vaccineName"
                      value={formData.vaccineName}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    >
                      <option value="">Select vaccine</option>
                      {Object.entries(VaccineName).map(([key, value]) => (
                        <option key={value} value={value}>
                          {key}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Dose Number
                    </label>
                    <input
                      type="number"
                      name="doseNumber"
                      value={formData.doseNumber}
                      onChange={handleChange}
                      min={1}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Date
                    </label>
                    <input
                      type="date"
                      name="administeredDate"
                      value={formData.administeredDate}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Batch Number
                    </label>
                    <input
                      type="text"
                      name="batchNumber"
                      value={formData.batchNumber}
                      onChange={handleChange}
                      className="input w-full"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Injection Site
                    </label>
                    <input
                      type="text"
                      name="site"
                      value={formData.site}
                      onChange={handleChange}
                      className="input w-full"
                      placeholder="e.g., Left deltoid"
                    />
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Administered By
                    </label>
                    <input
                      type="text"
                      name="administeredBy"
                      value={formData.administeredBy}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Notes
                    </label>
                    <textarea
                      name="notes"
                      value={formData.notes}
                      onChange={handleChange}
                      rows={2}
                      className="input w-full"
                    />
                  </div>
                </div>

                <div className="flex justify-end gap-3 pt-4">
                  <button
                    type="button"
                    onClick={onClose}
                    className="btn btn-outline"
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Save Record
                  </button>
                </div>
              </form>
            )}

            {record && (
              <div className="flex justify-end gap-3 pt-4 mt-4 border-t">
                <button onClick={onClose} className="btn btn-outline">
                  Close
                </button>
              </div>
            )}
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
