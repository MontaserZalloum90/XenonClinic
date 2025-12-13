import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  SignalIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { EEGRecord, CreateEEGRequest } from "../../types/neurology";
import { EEGStatus, EEGFinding } from "../../types/neurology";
import { eegApi } from "../../lib/api";

export const EEGRecords = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<EEGRecord | undefined>(
    undefined,
  );

  const { data: response, isLoading } = useQuery<EEGRecord[]>({
    queryKey: ["eeg-records"],
    queryFn: () => eegApi.getAll(),
  });

  const records = response?.data || [];

  const filteredRecords = records?.filter((record) => {
    const matchesSearch =
      !searchTerm ||
      record.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.interpretation.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || record.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedRecord(undefined);
    setIsModalOpen(true);
  };

  const handleView = (record: EEGRecord) => {
    setSelectedRecord(record);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedRecord(undefined);
  };

  const getStatusBadge = (status: EEGStatus) => {
    const config: Record<string, { className: string; label: string }> = {
      [EEGStatus.Scheduled]: {
        className: "bg-gray-100 text-gray-800",
        label: "Scheduled",
      },
      [EEGStatus.InProgress]: {
        className: "bg-blue-100 text-blue-800",
        label: "In Progress",
      },
      [EEGStatus.Completed]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Completed",
      },
      [EEGStatus.Interpreted]: {
        className: "bg-green-100 text-green-800",
        label: "Interpreted",
      },
    };
    const c = config[status] || {
      className: "bg-gray-100 text-gray-800",
      label: status,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const getFindingBadge = (finding: EEGFinding) => {
    const config: Record<string, { className: string; label: string }> = {
      [EEGFinding.Normal]: {
        className: "bg-green-100 text-green-800",
        label: "Normal",
      },
      [EEGFinding.Abnormal]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Abnormal",
      },
      [EEGFinding.EpilepticActivity]: {
        className: "bg-red-100 text-red-800",
        label: "Epileptic Activity",
      },
      [EEGFinding.Slowing]: {
        className: "bg-orange-100 text-orange-800",
        label: "Slowing",
      },
      [EEGFinding.AsymmetricActivity]: {
        className: "bg-purple-100 text-purple-800",
        label: "Asymmetric",
      },
      [EEGFinding.Other]: {
        className: "bg-gray-100 text-gray-800",
        label: "Other",
      },
    };
    const c = config[finding] || {
      className: "bg-gray-100 text-gray-800",
      label: finding,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const abnormalCount =
    records?.filter((r) => r.findings !== EEGFinding.Normal).length || 0;
  const pendingInterpretation =
    records?.filter((r) => r.status === EEGStatus.Completed).length || 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">EEG Records</h1>
          <p className="text-gray-600 mt-1">
            Electroencephalogram recordings and interpretations
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New EEG Record
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <SignalIcon className="h-8 w-8 text-blue-500" />
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
            <div className="h-8 w-8 rounded-full bg-yellow-100 flex items-center justify-center">
              <span className="text-yellow-600 font-bold">
                {pendingInterpretation}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Pending Interpretation</p>
              <p className="text-2xl font-bold text-yellow-600">
                {pendingInterpretation}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-red-100 flex items-center justify-center">
              <span className="text-red-600 font-bold">{abnormalCount}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Abnormal EEGs</p>
              <p className="text-2xl font-bold text-red-600">{abnormalCount}</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {records?.filter((r) => r.status === EEGStatus.Interpreted)
                  .length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Interpreted</p>
              <p className="text-2xl font-bold text-green-600">
                {records?.filter((r) => r.status === EEGStatus.Interpreted)
                  .length || 0}
              </p>
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
                placeholder="Search by patient name or interpretation..."
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
              {Object.entries(EEGStatus).map(([key, value]) => (
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
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Duration
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Findings
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Interpretation
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
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
                  <tr key={record.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {record.patientName || `Patient #${record.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(record.recordDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {record.duration} min
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getFindingBadge(record.findings)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {record.interpretation}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(record.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleView(record)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {record.status === EEGStatus.Completed && (
                        <button className="text-green-600 hover:text-green-900">
                          Interpret
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
                      : "No EEG records found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <EEGRecordModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        record={selectedRecord}
      />
    </div>
  );
};

// EEG Record Form Modal
interface EEGRecordModalProps {
  isOpen: boolean;
  onClose: () => void;
  record?: EEGRecord;
}

const EEGRecordModal = ({ isOpen, onClose, record }: EEGRecordModalProps) => {
  const [formData, setFormData] = useState<Partial<CreateEEGRequest>>({
    patientId: record?.patientId || 0,
    recordDate: record?.recordDate || new Date().toISOString().split("T")[0],
    duration: record?.duration || 30,
    findings: record?.findings || EEGFinding.Normal,
    interpretation: record?.interpretation || "",
    abnormalities: record?.abnormalities || [],
    performedBy: record?.performedBy || "",
    notes: record?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Saving EEG record:", formData);
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
        name === "patientId" || name === "duration" ? Number(value) : value,
    }));
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {record ? "View EEG Record" : "New EEG Record"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

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
                    Record Date
                  </label>
                  <input
                    type="date"
                    name="recordDate"
                    value={formData.recordDate?.split("T")[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Duration (min)
                  </label>
                  <input
                    type="number"
                    name="duration"
                    value={formData.duration}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Findings
                  </label>
                  <select
                    name="findings"
                    value={formData.findings}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  >
                    {Object.entries(EEGFinding).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key.replace(/([A-Z])/g, " $1").trim()}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Performed By
                  </label>
                  <input
                    type="text"
                    name="performedBy"
                    value={formData.performedBy}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Interpretation
                  </label>
                  <textarea
                    name="interpretation"
                    value={formData.interpretation}
                    onChange={handleChange}
                    rows={3}
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
                  {record ? "Update" : "Create"} Record
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
