import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  PlusIcon,
  XMarkIcon,
  AcademicCapIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline";
import { format } from "date-fns";
import type { DevelopmentMilestone } from "../../types/pediatrics";
import { MilestoneCategory, MilestoneStatus } from "../../types/pediatrics";
import { developmentMilestonesApi } from "../../lib/api";

export const DevelopmentMilestones = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [categoryFilter, setCategoryFilter] = useState<string>("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedMilestone, setSelectedMilestone] = useState<
    DevelopmentMilestone | undefined
  >(undefined);

  const { data: milestonesResponse, isLoading } = useQuery<
    DevelopmentMilestone[]
  >({
    queryKey: ["development-milestones"],
    queryFn: () => developmentMilestonesApi.getAll(),
  });

  const milestones = milestonesResponse?.data || [];

  const filteredMilestones = milestones?.filter((milestone) => {
    const matchesSearch =
      !searchTerm ||
      milestone.patientName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      milestone.milestoneName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || milestone.status === statusFilter;
    const matchesCategory =
      !categoryFilter || milestone.milestoneCategory === categoryFilter;
    return matchesSearch && matchesStatus && matchesCategory;
  });

  const handleCreate = () => {
    setSelectedMilestone(undefined);
    setIsModalOpen(true);
  };

  const handleView = (milestone: DevelopmentMilestone) => {
    setSelectedMilestone(milestone);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedMilestone(undefined);
  };

  const getStatusBadge = (status: MilestoneStatus) => {
    const config: Record<
      string,
      { className: string; label: string; icon: typeof CheckCircleIcon }
    > = {
      [MilestoneStatus.NotYetAchieved]: {
        className: "bg-gray-100 text-gray-800",
        label: "Not Yet",
        icon: AcademicCapIcon,
      },
      [MilestoneStatus.Achieved]: {
        className: "bg-green-100 text-green-800",
        label: "Achieved",
        icon: CheckCircleIcon,
      },
      [MilestoneStatus.Delayed]: {
        className: "bg-yellow-100 text-yellow-800",
        label: "Delayed",
        icon: ExclamationTriangleIcon,
      },
      [MilestoneStatus.Concerning]: {
        className: "bg-red-100 text-red-800",
        label: "Concerning",
        icon: ExclamationTriangleIcon,
      },
    };
    const c = config[status] || {
      className: "bg-gray-100 text-gray-800",
      label: status,
      icon: AcademicCapIcon,
    };
    const Icon = c.icon;
    return (
      <span
        className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        <Icon className="h-3 w-3 mr-1" />
        {c.label}
      </span>
    );
  };

  const getCategoryBadge = (category: MilestoneCategory) => {
    const config: Record<string, { className: string; label: string }> = {
      [MilestoneCategory.Social]: {
        className: "bg-pink-100 text-pink-800",
        label: "Social",
      },
      [MilestoneCategory.Language]: {
        className: "bg-blue-100 text-blue-800",
        label: "Language",
      },
      [MilestoneCategory.Cognitive]: {
        className: "bg-purple-100 text-purple-800",
        label: "Cognitive",
      },
      [MilestoneCategory.Motor]: {
        className: "bg-orange-100 text-orange-800",
        label: "Motor",
      },
      [MilestoneCategory.Fine_Motor]: {
        className: "bg-teal-100 text-teal-800",
        label: "Fine Motor",
      },
      [MilestoneCategory.Gross_Motor]: {
        className: "bg-green-100 text-green-800",
        label: "Gross Motor",
      },
      [MilestoneCategory.Emotional]: {
        className: "bg-red-100 text-red-800",
        label: "Emotional",
      },
    };
    const c = config[category] || {
      className: "bg-gray-100 text-gray-800",
      label: category,
    };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {c.label}
      </span>
    );
  };

  const concerningCount =
    milestones?.filter(
      (m) =>
        m.status === MilestoneStatus.Delayed ||
        m.status === MilestoneStatus.Concerning,
    ).length || 0;
  const achievedCount =
    milestones?.filter((m) => m.status === MilestoneStatus.Achieved).length ||
    0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Development Milestones
          </h1>
          <p className="text-gray-600 mt-1">
            Track and assess pediatric developmental progress
          </p>
        </div>
        <button onClick={handleCreate} className="btn btn-primary">
          <PlusIcon className="h-5 w-5 mr-2" />
          Record Milestone
        </button>
      </div>

      {/* Alert for concerning milestones */}
      {concerningCount > 0 && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-5 w-5 text-yellow-600 mr-3" />
            <div>
              <h3 className="text-sm font-medium text-yellow-800">
                Development Concerns
              </h3>
              <p className="text-sm text-yellow-700 mt-1">
                {concerningCount} milestone(s) are delayed or concerning and may
                require follow-up.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <AcademicCapIcon className="h-8 w-8 text-blue-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Total Tracked</p>
              <p className="text-2xl font-bold text-gray-900">
                {milestones?.length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <CheckCircleIcon className="h-8 w-8 text-green-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Achieved</p>
              <p className="text-2xl font-bold text-green-600">
                {achievedCount}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center">
              <span className="text-gray-600 font-bold">
                {milestones?.filter(
                  (m) => m.status === MilestoneStatus.NotYetAchieved,
                ).length || 0}
              </span>
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">In Progress</p>
              <p className="text-2xl font-bold text-gray-600">
                {milestones?.filter(
                  (m) => m.status === MilestoneStatus.NotYetAchieved,
                ).length || 0}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <ExclamationTriangleIcon className="h-8 w-8 text-yellow-500" />
            <div className="ml-4">
              <p className="text-sm text-gray-500">Concerns</p>
              <p className="text-2xl font-bold text-yellow-600">
                {concerningCount}
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
                placeholder="Search by patient name or milestone..."
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
              {Object.entries(MilestoneStatus).map(([key, value]) => (
                <option key={value} value={value}>
                  {key.replace(/_/g, " ")}
                </option>
              ))}
            </select>
            <select
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md"
            >
              <option value="">All Categories</option>
              {Object.entries(MilestoneCategory).map(([key, value]) => (
                <option key={value} value={value}>
                  {key.replace(/_/g, " ")}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Milestones Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Milestone
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Expected Age
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Assessed By
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
                    <p className="mt-2">Loading milestones...</p>
                  </td>
                </tr>
              ) : filteredMilestones && filteredMilestones.length > 0 ? (
                filteredMilestones.map((milestone) => (
                  <tr
                    key={milestone.id}
                    className={`hover:bg-gray-50 ${
                      milestone.status === MilestoneStatus.Concerning
                        ? "bg-red-50"
                        : milestone.status === MilestoneStatus.Delayed
                          ? "bg-yellow-50"
                          : ""
                    }`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="font-medium text-gray-900">
                        {milestone.patientName ||
                          `Patient #${milestone.patientId}`}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">
                        {milestone.milestoneName}
                      </div>
                      {milestone.notes && (
                        <div className="text-xs text-gray-500 truncate max-w-xs">
                          {milestone.notes}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getCategoryBadge(milestone.milestoneCategory)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {milestone.expectedAge} months
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(milestone.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {milestone.assessedBy}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleView(milestone)}
                        className="text-primary-600 hover:text-primary-900 mr-3"
                      >
                        View
                      </button>
                      {milestone.status !== MilestoneStatus.Achieved && (
                        <button className="text-green-600 hover:text-green-900">
                          Mark Achieved
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
                      ? "No milestones found matching your search."
                      : "No development milestones found."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal */}
      <MilestoneModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        milestone={selectedMilestone}
      />
    </div>
  );
};

// Milestone Modal
interface MilestoneModalProps {
  isOpen: boolean;
  onClose: () => void;
  milestone?: DevelopmentMilestone;
}

const MilestoneModal = ({
  isOpen,
  onClose,
  milestone,
}: MilestoneModalProps) => {
  const [formData, setFormData] = useState({
    patientId: milestone?.patientId || 0,
    milestoneCategory: milestone?.milestoneCategory || "",
    milestoneName: milestone?.milestoneName || "",
    expectedAge: milestone?.expectedAge || 0,
    status: milestone?.status || MilestoneStatus.NotYetAchieved,
    assessedBy: milestone?.assessedBy || "",
    notes: milestone?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Saving milestone:", formData);
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
        name === "patientId" || name === "expectedAge" ? Number(value) : value,
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
                {milestone ? "Milestone Details" : "Record Milestone"}
              </Dialog.Title>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>

            {milestone ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Patient
                    </label>
                    <p className="text-gray-900">{milestone.patientName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Category
                    </label>
                    <p className="text-gray-900">
                      {milestone.milestoneCategory}
                    </p>
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-500">
                      Milestone
                    </label>
                    <p className="text-gray-900 font-medium">
                      {milestone.milestoneName}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Expected Age
                    </label>
                    <p className="text-gray-900">
                      {milestone.expectedAge} months
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Status
                    </label>
                    <p className="text-gray-900">{milestone.status}</p>
                  </div>
                  {milestone.achievedDate && (
                    <div>
                      <label className="block text-sm font-medium text-gray-500">
                        Achieved Date
                      </label>
                      <p className="text-gray-900">
                        {format(
                          new Date(milestone.achievedDate),
                          "MMM d, yyyy",
                        )}
                      </p>
                    </div>
                  )}
                  <div>
                    <label className="block text-sm font-medium text-gray-500">
                      Assessed By
                    </label>
                    <p className="text-gray-900">{milestone.assessedBy}</p>
                  </div>
                  {milestone.notes && (
                    <div className="col-span-2">
                      <label className="block text-sm font-medium text-gray-500">
                        Notes
                      </label>
                      <p className="text-gray-700 mt-1">{milestone.notes}</p>
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
                      Category
                    </label>
                    <select
                      name="milestoneCategory"
                      value={formData.milestoneCategory}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    >
                      <option value="">Select category</option>
                      {Object.entries(MilestoneCategory).map(([key, value]) => (
                        <option key={value} value={value}>
                          {key.replace(/_/g, " ")}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Milestone Name
                    </label>
                    <input
                      type="text"
                      name="milestoneName"
                      value={formData.milestoneName}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Expected Age (months)
                    </label>
                    <input
                      type="number"
                      name="expectedAge"
                      value={formData.expectedAge}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Status
                    </label>
                    <select
                      name="status"
                      value={formData.status}
                      onChange={handleChange}
                      className="input w-full"
                      required
                    >
                      {Object.entries(MilestoneStatus).map(([key, value]) => (
                        <option key={value} value={value}>
                          {key.replace(/_/g, " ")}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Assessed By
                    </label>
                    <input
                      type="text"
                      name="assessedBy"
                      value={formData.assessedBy}
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
                    Save Milestone
                  </button>
                </div>
              </form>
            )}

            {milestone && (
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
