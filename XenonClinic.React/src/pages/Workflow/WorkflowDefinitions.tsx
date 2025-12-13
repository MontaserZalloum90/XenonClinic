import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Dialog } from "@headlessui/react";
import { format } from "date-fns";
import { workflowDefinitionsApi } from "../../lib/api";
import type {
  WorkflowDefinition,
  WorkflowStatistics,
} from "../../types/workflow";
import { WorkflowStepType } from "../../types/workflow";

export const WorkflowDefinitions = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isViewStepsModalOpen, setIsViewStepsModalOpen] = useState(false);
  const [selectedDefinition, setSelectedDefinition] =
    useState<WorkflowDefinition | null>(null);
  const [newWorkflowName, setNewWorkflowName] = useState("");
  const [newWorkflowDescription, setNewWorkflowDescription] = useState("");
  const [newWorkflowCategory, setNewWorkflowCategory] = useState("");

  const { data: definitionsData, isLoading } = useQuery<{
    data: WorkflowDefinition[];
  }>({
    queryKey: ["workflow-definitions"],
    queryFn: workflowDefinitionsApi.getAll,
  });

  const { data: statsData } = useQuery<{ data: WorkflowStatistics }>({
    queryKey: ["workflow-statistics"],
    queryFn: workflowDefinitionsApi.getStatistics,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => workflowDefinitionsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workflow-definitions"] });
      queryClient.invalidateQueries({ queryKey: ["workflow-statistics"] });
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: number; isActive: boolean }) =>
      workflowDefinitionsApi.toggleActive(id, isActive),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workflow-definitions"] });
      queryClient.invalidateQueries({ queryKey: ["workflow-statistics"] });
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: Partial<WorkflowDefinition>) =>
      workflowDefinitionsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workflow-definitions"] });
      queryClient.invalidateQueries({ queryKey: ["workflow-statistics"] });
      setIsCreateModalOpen(false);
      setNewWorkflowName("");
      setNewWorkflowDescription("");
      setNewWorkflowCategory("");
    },
  });

  const definitions = definitionsData?.data || [];
  const stats = statsData?.data;

  const filteredDefinitions = definitions.filter((def) => {
    const matchesSearch =
      !searchTerm ||
      def.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      def.description?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus =
      !statusFilter ||
      (statusFilter === "active" && def.isActive) ||
      (statusFilter === "inactive" && !def.isActive);
    return matchesSearch && matchesStatus;
  });

  const handleDelete = (definition: WorkflowDefinition) => {
    if (
      window.confirm(
        `Are you sure you want to delete workflow "${definition.name}"? This action cannot be undone.`,
      )
    ) {
      deleteMutation.mutate(definition.id);
    }
  };

  const handleToggleActive = (definition: WorkflowDefinition) => {
    const action = definition.isActive ? "deactivate" : "activate";
    if (
      window.confirm(
        `Are you sure you want to ${action} workflow "${definition.name}"?`,
      )
    ) {
      toggleActiveMutation.mutate({
        id: definition.id,
        isActive: !definition.isActive,
      });
    }
  };

  const handleViewSteps = (definition: WorkflowDefinition) => {
    setSelectedDefinition(definition);
    setIsViewStepsModalOpen(true);
  };

  const handleCreateWorkflow = () => {
    if (!newWorkflowName.trim()) return;

    createMutation.mutate({
      name: newWorkflowName,
      description: newWorkflowDescription || undefined,
      category: newWorkflowCategory || undefined,
      steps: [],
      triggers: [],
      isActive: false,
      version: 1,
    });
  };

  const getStepTypeLabel = (type: WorkflowStepType): string => {
    const labels = {
      [WorkflowStepType.Manual]: "Manual",
      [WorkflowStepType.Automatic]: "Automatic",
      [WorkflowStepType.Approval]: "Approval",
      [WorkflowStepType.Notification]: "Notification",
    };
    return labels[type] || "Unknown";
  };

  const getStepTypeBadge = (type: WorkflowStepType) => {
    const config: Record<number, { className: string }> = {
      [WorkflowStepType.Manual]: { className: "bg-blue-100 text-blue-800" },
      [WorkflowStepType.Automatic]: {
        className: "bg-green-100 text-green-800",
      },
      [WorkflowStepType.Approval]: {
        className: "bg-purple-100 text-purple-800",
      },
      [WorkflowStepType.Notification]: {
        className: "bg-yellow-100 text-yellow-800",
      },
    };
    const c = config[type] || { className: "bg-gray-100 text-gray-800" };
    return (
      <span
        className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}
      >
        {getStepTypeLabel(type)}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Workflow Definitions
          </h1>
          <p className="text-gray-600 mt-1">
            Manage workflow definitions and templates
          </p>
        </div>
        <button
          onClick={() => setIsCreateModalOpen(true)}
          className="btn btn-primary"
        >
          Create Workflow
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Workflows</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {stats?.totalDefinitions || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Active Workflows</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {stats?.activeDefinitions || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Total Instances</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {stats?.totalInstances || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completion Rate</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">
            {stats?.completionRate
              ? `${stats.completionRate.toFixed(1)}%`
              : "0%"}
          </p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4 flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <input
              type="text"
              placeholder="Search workflows..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input w-full"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="input"
          >
            <option value="">All Status</option>
            <option value="active">Active Only</option>
            <option value="inactive">Inactive Only</option>
          </select>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading workflows...</p>
          </div>
        ) : filteredDefinitions && filteredDefinitions.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Name
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Description
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Steps
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Version
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Updated
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredDefinitions.map((definition) => (
                  <tr key={definition.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <div className="text-sm font-medium text-gray-900">
                        {definition.name}
                      </div>
                      {definition.category && (
                        <div className="text-xs text-gray-500">
                          {definition.category}
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600 max-w-xs truncate">
                      {definition.description || "-"}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      <button
                        onClick={() => handleViewSteps(definition)}
                        className="text-primary-600 hover:text-primary-800"
                      >
                        {definition.steps.length} steps
                      </button>
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleToggleActive(definition)}
                          className={`px-2 py-1 rounded-full text-xs font-medium ${
                            definition.isActive
                              ? "bg-green-100 text-green-800"
                              : "bg-gray-100 text-gray-800"
                          }`}
                        >
                          {definition.isActive ? "Active" : "Inactive"}
                        </button>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      v{definition.version}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {format(new Date(definition.updatedAt), "MMM d, yyyy")}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewSteps(definition)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          View
                        </button>
                        <button
                          onClick={() => handleToggleActive(definition)}
                          className="text-blue-600 hover:text-blue-800"
                        >
                          {definition.isActive ? "Deactivate" : "Activate"}
                        </button>
                        <button
                          onClick={() => handleDelete(definition)}
                          className="text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm || statusFilter
              ? "No workflows found matching your filters."
              : "No workflows found. Create your first workflow to get started."}
          </div>
        )}
      </div>

      {/* Create Workflow Modal */}
      <Dialog
        open={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-lg w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Create New Workflow
              </Dialog.Title>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Workflow Name *
                  </label>
                  <input
                    type="text"
                    value={newWorkflowName}
                    onChange={(e) => setNewWorkflowName(e.target.value)}
                    placeholder="Enter workflow name"
                    className="input w-full"
                    autoFocus
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Description
                  </label>
                  <textarea
                    value={newWorkflowDescription}
                    onChange={(e) => setNewWorkflowDescription(e.target.value)}
                    placeholder="Enter workflow description"
                    rows={3}
                    className="input w-full"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Category
                  </label>
                  <input
                    type="text"
                    value={newWorkflowCategory}
                    onChange={(e) => setNewWorkflowCategory(e.target.value)}
                    placeholder="e.g., Patient Care, Billing, Laboratory"
                    className="input w-full"
                  />
                </div>
              </div>
              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => setIsCreateModalOpen(false)}
                  className="px-4 py-2 text-gray-700 hover:text-gray-900"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreateWorkflow}
                  disabled={!newWorkflowName.trim() || createMutation.isPending}
                  className="btn btn-primary disabled:opacity-50"
                >
                  {createMutation.isPending ? "Creating..." : "Create Workflow"}
                </button>
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>

      {/* View Steps Modal */}
      <Dialog
        open={isViewStepsModalOpen}
        onClose={() => setIsViewStepsModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-3xl w-full bg-white rounded-lg shadow-xl">
            <div className="p-6">
              <Dialog.Title className="text-lg font-medium text-gray-900 mb-4">
                Workflow Steps: {selectedDefinition?.name}
              </Dialog.Title>
              {selectedDefinition && selectedDefinition.steps.length > 0 ? (
                <div className="space-y-3">
                  {selectedDefinition.steps
                    .sort((a, b) => (a.order || 0) - (b.order || 0))
                    .map((step, index) => (
                      <div
                        key={step.id}
                        className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50"
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <span className="flex items-center justify-center w-6 h-6 rounded-full bg-primary-100 text-primary-600 text-sm font-medium">
                                {index + 1}
                              </span>
                              <h4 className="font-medium text-gray-900">
                                {step.name}
                              </h4>
                            </div>
                            {step.description && (
                              <p className="text-sm text-gray-600 mt-2 ml-8">
                                {step.description}
                              </p>
                            )}
                            <div className="flex items-center gap-3 mt-2 ml-8">
                              {getStepTypeBadge(step.type)}
                              {step.assigneeRole && (
                                <span className="text-xs text-gray-600">
                                  Assigned to: {step.assigneeRole}
                                </span>
                              )}
                            </div>
                            {step.actions && step.actions.length > 0 && (
                              <div className="mt-2 ml-8">
                                <p className="text-xs text-gray-500">
                                  Actions: {step.actions.join(", ")}
                                </p>
                              </div>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">
                  No steps defined for this workflow.
                </div>
              )}
              <div className="mt-6 flex justify-end">
                <button
                  onClick={() => setIsViewStepsModalOpen(false)}
                  className="btn btn-primary"
                >
                  Close
                </button>
              </div>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
