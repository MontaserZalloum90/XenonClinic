import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  BoltIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { emgApi } from "../../lib/api";

// EMG Types
const EMGStatus = {
  Scheduled: "scheduled",
  InProgress: "in_progress",
  Completed: "completed",
  Interpreted: "interpreted",
} as const;

type EMGStatusType = (typeof EMGStatus)[keyof typeof EMGStatus];

const EMGFinding = {
  Normal: "normal",
  Neuropathy: "neuropathy",
  Myopathy: "myopathy",
  Radiculopathy: "radiculopathy",
  CarpalTunnel: "carpal_tunnel",
  MotorNeuronDisease: "motor_neuron_disease",
  Other: "other",
} as const;

type EMGFindingType = (typeof EMGFinding)[keyof typeof EMGFinding];

interface EMGStudy {
  id: number;
  patientId: number;
  patientName?: string;
  studyDate: string;
  referralReason: string;
  nervesStudied: string[];
  musclesStudied: string[];
  findings: EMGFindingType;
  interpretation: string;
  performedBy: string;
  interpretedBy?: string;
  status: EMGStatusType;
  notes?: string;
  createdAt?: string;
}

export const EMGStudies = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedStudy, setSelectedStudy] = useState<EMGStudy | undefined>(
    undefined,
  );

  const { data: response, isLoading } = useQuery<EMGStudy[]>({
    queryKey: ["emg-studies"],
    queryFn: () => emgApi.getAll(),
  });

  const studies = response?.data || [];

  const filteredStudies = studies?.filter((study) => {
    const matchesSearch =
      !searchTerm ||
      study.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      study.interpretation.toLowerCase().includes(searchTerm.toLowerCase()) ||
      study.referralReason.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || study.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleCreate = () => {
    setSelectedStudy(undefined);
    setIsModalOpen(true);
  };

  const handleView = (study: EMGStudy) => {
    setSelectedStudy(study);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedStudy(undefined);
  };

  const getStatusBadge = (status: EMGStatusType) => {
    const config: Record<string, { className: string; label: string }> = {
      [EMGStatus.Scheduled]: {
        className: "bg-gray-100 text-gray-800",
        label: "Scheduled",
      },
      [EMGStatus.InProgress]: {
        className: "bg-blue-100 text-blue-800",
        label: "In Progress",
      },
      [EMGStatus.Completed]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Completed",
      },
      [EMGStatus.Interpreted]: {
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

  const getFindingBadge = (finding: EMGFindingType) => {
    const config: Record<string, { className: string; label: string }> = {
      [EMGFinding.Normal]: {
        className: "bg-green-100 text-green-800",
        label: "Normal",
      },
      [EMGFinding.Neuropathy]: {
        className: "bg-orange-100 text-orange-800",
        label: "Neuropathy",
      },
      [EMGFinding.Myopathy]: {
        className: "bg-purple-100 text-purple-800",
        label: "Myopathy",
      },
      [EMGFinding.Radiculopathy]: {
        className: "bg-red-100 text-red-800",
        label: "Radiculopathy",
      },
      [EMGFinding.CarpalTunnel]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Carpal Tunnel",
      },
      [EMGFinding.MotorNeuronDisease]: {
        className: "bg-red-100 text-red-800",
        label: "Motor Neuron Disease",
      },
      [EMGFinding.Other]: {
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
    studies?.filter((s) => s.findings !== EMGFinding.Normal).length || 0;
  const pendingInterpretation =
    studies?.filter((s) => s.status === EMGStatus.Completed).length || 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">EMG Studies</h1>
          <p className="text-gray-600 mt-1">
            Electromyography and nerve conduction studies
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New EMG Study
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <BoltIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Studies</p>
              <p className="text-2xl font-bold text-gray-900">
                {studies?.length || 0}
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
              <p className="text-sm text-gray-500">Abnormal Studies</p>
              <p className="text-2xl font-bold text-red-600">{abnormalCount}</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">
                {studies?.filter((s) => s.status === EMGStatus.Interpreted)
                  .length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Interpreted</p>
              <p className="text-2xl font-bold text-green-600">
                {studies?.filter((s) => s.status === EMGStatus.Interpreted)
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
                placeholder="Search by patient name, interpretation, or referral reason..."
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
              {Object.entries(EMGStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Studies Table */}
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
                  Referral Reason
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Findings
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Performed By
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
                    <p className="mt-2">Loading studies...</p>
                  </td>
                </tr>
              ) : filteredStudies && filteredStudies.length > 0 ? (
                filteredStudies.map((study) => (
                  <tr key={study.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {study.patientName || `Patient #${study.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {format(new Date(study.studyDate), "MMM d, yyyy")}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {study.referralReason}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getFindingBadge(study.findings)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(study.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {study.performedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleView(study)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {study.status === EMGStatus.Completed && (
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
                      ? "No studies found matching your search."
                      : "No EMG studies found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <EMGStudyModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        study={selectedStudy}
      />
    </div>
  );
};

// EMG Study Form Modal
interface EMGStudyModalProps {
  isOpen: boolean;
  onClose: () => void;
  study?: EMGStudy;
}

const EMGStudyModal = ({ isOpen, onClose, study }: EMGStudyModalProps) => {
  const [formData, setFormData] = useState({
    patientId: study?.patientId || 0,
    studyDate: study?.studyDate || new Date().toISOString().split("T")[0],
    referralReason: study?.referralReason || "",
    nervesStudied: study?.nervesStudied?.join(", ") || "",
    musclesStudied: study?.musclesStudied?.join(", ") || "",
    findings: study?.findings || EMGFinding.Normal,
    interpretation: study?.interpretation || "",
    performedBy: study?.performedBy || "",
    notes: study?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Saving EMG study:", formData);
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
      [name]: name === "patientId" ? Number(value) : value,
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
                {study ? "View EMG Study" : "New EMG Study"}
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
                    Study Date
                  </label>
                  <input
                    type="date"
                    name="studyDate"
                    value={formData.studyDate?.split("T")[0]}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Referral Reason
                  </label>
                  <input
                    type="text"
                    name="referralReason"
                    value={formData.referralReason}
                    onChange={handleChange}
                    className="input w-full"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Nerves Studied
                  </label>
                  <input
                    type="text"
                    name="nervesStudied"
                    value={formData.nervesStudied}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., Median, Ulnar, Radial"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Muscles Studied
                  </label>
                  <input
                    type="text"
                    name="musclesStudied"
                    value={formData.musclesStudied}
                    onChange={handleChange}
                    className="input w-full"
                    placeholder="e.g., APB, ADM, FDI"
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
                    {Object.entries(EMGFinding).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key.replace(/([A-Z])/g, " $1").trim()}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
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
                  {study ? "Update" : "Create"} Study
                </button>
              </div>
            </form>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
