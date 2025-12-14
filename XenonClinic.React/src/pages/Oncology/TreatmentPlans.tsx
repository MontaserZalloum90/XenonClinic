import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  ClipboardDocumentListIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import {
  ChemoProtocolStatus,
  CancerType,
  CancerStage,
} from "../../types/oncology";
import { oncologyApi } from "../../lib/api";

interface TreatmentPlan {
  id: number;
  patientId: number;
  patientName: string;
  diagnosisId: number;
  cancerType: string;
  stage: string;
  planName: string;
  treatmentType:
    | "chemotherapy"
    | "radiation"
    | "surgery"
    | "immunotherapy"
    | "combination";
  protocol?: string;
  drugs?: string[];
  totalCycles?: number;
  completedCycles: number;
  startDate: string;
  expectedEndDate?: string;
  status: string;
  oncologist: string;
  notes?: string;
  createdAt: string;
}

interface TreatmentPlansProps {
  patientId?: number;
}

export const TreatmentPlans = ({ patientId }: TreatmentPlansProps = {}) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [typeFilter, setTypeFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState<TreatmentPlan | undefined>(
    undefined,
  );

  const { data: plans, isLoading } = useQuery<TreatmentPlan[]>({
    queryKey: ["treatment-plans", patientId],
    queryFn: async () => {
      if (patientId) {
        const response = await oncologyApi.getTreatmentPlansByPatient(patientId);
        return response.data?.data ?? response.data ?? [];
      }
      const response = await oncologyApi.getAllTreatmentPlans();
      return response.data?.data ?? response.data ?? [];
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => oncologyApi.createTreatmentPlan(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["treatment-plans"] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Record<string, unknown> }) =>
      oncologyApi.updateTreatmentPlan(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["treatment-plans"] }),
  });

  void createMutation;
  void updateMutation;

  const filteredPlans = plans?.filter((plan) => {
    const matchesSearch =
      !searchTerm ||
      plan.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      plan.planName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      plan.protocol?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || plan.status === statusFilter;
    const matchesType = !typeFilter || plan.treatmentType === typeFilter;
    return matchesSearch && matchesStatus && matchesType;
  });

  const handleCreate = () => {
    setSelectedPlan(undefined);
    setIsModalOpen(true);
  };

  const handleView = (plan: TreatmentPlan) => {
    setSelectedPlan(plan);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedPlan(undefined);
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { className: string; label: string }> = {
      [ChemoProtocolStatus.Planned]: {
        className: "bg-gray-100 text-gray-800",
        label: "Planned",
      },
      [ChemoProtocolStatus.Active]: {
        className: "bg-green-100 text-green-800",
        label: "Active",
      },
      [ChemoProtocolStatus.Completed]: {
        className: "bg-blue-100 text-blue-800",
        label: "Completed",
      },
      [ChemoProtocolStatus.OnHold]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "On Hold",
      },
      [ChemoProtocolStatus.Discontinued]: {
        className: "bg-red-100 text-red-800",
        label: "Discontinued",
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

  const getTreatmentTypeBadge = (type: string) => {
    const config: Record<string, { className: string; label: string }> = {
      chemotherapy: {
        className: "bg-purple-100 text-purple-800",
        label: "Chemotherapy",
      },
      radiation: {
        className: "bg-orange-100 text-orange-800",
        label: "Radiation",
      },
      surgery: { className: "bg-red-100 text-red-800", label: "Surgery" },
      immunotherapy: {
        className: "bg-blue-100 text-blue-800",
        label: "Immunotherapy",
      },
      combination: {
        className: "bg-indigo-100 text-indigo-800",
        label: "Combination",
      },
    };
    const c = config[type] || {
      className: "bg-gray-100 text-gray-800",
      label: type,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const activePlans =
    plans?.filter((p) => p.status === ChemoProtocolStatus.Active).length || 0;
  const completedPlans =
    plans?.filter((p) => p.status === ChemoProtocolStatus.Completed).length ||
    0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Treatment Plans</h1>
          <p className="text-gray-600 mt-1">
            Manage oncology treatment protocols and progress
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          New Treatment Plan
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ClipboardDocumentListIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Plans</p>
              <p className="text-2xl font-bold text-gray-900">
                {plans?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-green-600 font-bold">{activePlans}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Active</p>
              <p className="text-2xl font-bold text-green-600">{activePlans}</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-blue-600 font-bold">{completedPlans}</span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Completed</p>
              <p className="text-2xl font-bold text-blue-600">
                {completedPlans}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center">
              <span className="text-gray-600 font-bold">
                {plans?.filter((p) => p.status === ChemoProtocolStatus.Planned)
                  .length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Planned</p>
              <p className="text-2xl font-bold text-gray-600">
                {plans?.filter((p) => p.status === ChemoProtocolStatus.Planned)
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
                placeholder="Search by patient, plan name, or protocol..."
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
              {Object.entries(ChemoProtocolStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key}
                </option>
              ))}
            </select>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Types</option>
              <option value="chemotherapy">Chemotherapy</option>
              <option value="radiation">Radiation</option>
              <option value="surgery">Surgery</option>
              <option value="immunotherapy">Immunotherapy</option>
              <option value="combination">Combination</option>
            </select>
          </div>
        </div>
      </div>

      {/* Plans Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient / Diagnosis
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Treatment Plan
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Progress
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Oncologist
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
                    <p className="mt-2">Loading plans...</p>
                  </td>
                </tr>
              ) : filteredPlans && filteredPlans.length > 0 ? (
                filteredPlans.map((plan) => {
                  const progress = plan.totalCycles
                    ? Math.round(
                        (plan.completedCycles / plan.totalCycles) * 100,
                      )
                    : 0;
                  return (
                    <tr key={plan.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4">
                        <div className="font-medium text-gray-900">
                          {plan.patientName}
                        </div>
                        <div className="text-xs text-gray-500">
                          {plan.cancerType} - {plan.stage}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="font-medium text-gray-900">
                          {plan.planName}
                        </div>
                        {plan.protocol && (
                          <div className="text-xs text-gray-500">
                            Protocol: {plan.protocol}
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getTreatmentTypeBadge(plan.treatmentType)}
                      </td>
                      <td className="px-6 py-4">
                        {plan.totalCycles ? (
                          <div>
                            <div className="flex items-center">
                              <div className="flex-1 bg-gray-200 rounded-full h-2 mr-2">
                                <div
                                  className="bg-green-600 h-2 rounded-full"
                                  style={{ width: `${progress}%` }}
                                />
                              </div>
                              <span className="text-xs text-gray-600">
                                {progress}%
                              </span>
                            </div>
                            <div className="text-xs text-gray-500 mt-1">
                              {plan.completedCycles}/{plan.totalCycles} cycles
                            </div>
                          </div>
                        ) : (
                          <span className="text-gray-400 text-sm">-</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getStatusBadge(plan.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {plan.oncologist}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          onClick={() => handleView(plan)}
                          className="text-primary-600 hover:text-primary-900 mr-3"
                        >
                          View
                        </button>
                        <button className="text-gray-600 hover:text-gray-900">
                          Edit
                        </button>
                      </td>
                    </tr>
                  );
                })
              ) : (
                <tr>
                  <td
                    colSpan={7}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    {searchTerm
                      ? "No plans found matching your search."
                      : "No treatment plans found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <TreatmentPlanModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        plan={selectedPlan}
      />
    </div>
  );
};

// Treatment Plan Modal
interface TreatmentPlanModalProps {
  isOpen: boolean;
  onClose: () => void;
  plan?: TreatmentPlan;
}

const TreatmentPlanModal = ({
  isOpen,
  onClose,
  plan,
}: TreatmentPlanModalProps) => {
  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-lg shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="p-6">
            <div className="flex justify-between items-center mb-4">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                {plan ? "Treatment Plan Details" : "Create Treatment Plan"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            {plan ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Patient
                    </label>
                    <p className="text-gray-900">{plan.patientName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Cancer Type
                    </label>
                    <p className="text-gray-900">
                      {plan.cancerType} - {plan.stage}
                    </p>
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-500">
                      Plan Name
                    </label>
                    <p className="text-gray-900 font-medium">{plan.planName}</p>
                  </div>
                  {plan.protocol && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Protocol
                      </label>
                      <p className="text-gray-900">{plan.protocol}</p>
                    </div>
                  )}
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Oncologist
                    </label>
                    <p className="text-gray-900">{plan.oncologist}</p>
                  </div>
                </div>

                {plan.drugs && plan.drugs.length > 0 && (
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-2">
                      Drugs
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {plan.drugs.map((drug, i) => (
                        <span
                          key={i}
                          className="px-2 py-1 bg-purple-50 text-purple-700 rounded text-sm"
                        >
                          {drug}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Start Date
                    </label>
                    <p className="text-gray-900">
                      {format(new Date(plan.startDate), "MMM d, yyyy")}
                    </p>
                  </div>
                  {plan.expectedEndDate && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Expected End
                      </label>
                      <p className="text-gray-900">
                        {format(new Date(plan.expectedEndDate), "MMM d, yyyy")}
                      </p>
                    </div>
                  )}
                </div>

                {plan.totalCycles && (
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-2">
                      Progress
                    </label>
                    <div className="bg-gray-50 p-4 rounded">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm text-gray-600">
                          {plan.completedCycles} of {plan.totalCycles} cycles
                          completed
                        </span>
                        <span className="text-sm font-medium">
                          {Math.round(
                            (plan.completedCycles / plan.totalCycles) * 100,
                          )}
                          %
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-3">
                        <div
                          className="bg-green-600 h-3 rounded-full"
                          style={{
                            width: `${(plan.completedCycles / plan.totalCycles) * 100}%`,
                          }}
                        />
                      </div>
                    </div>
                  </div>
                )}

                {plan.notes && (
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Notes
                    </label>
                    <p className="text-gray-700 mt-1">{plan.notes}</p>
                  </div>
                )}
              </div>
            ) : (
              <p className="text-gray-500">Create form would go here</p>
            )}

            <div className="flex justify-end gap-3 pt-4 mt-4 border-t">
              <button onClick={onClose} className="btn btn-outline">
                Close
              </button>
            </div>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
};
