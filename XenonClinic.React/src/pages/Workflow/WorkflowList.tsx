import { useState, useEffect, useCallback } from "react";
import { Link, useNavigate } from "react-router-dom";
import { workflowDesignerApi } from "../../components/workflow/workflowApi";
import type {
  WorkflowDesignSummary,
  WorkflowDesignListResult,
} from "../../components/workflow/types";
import { Modal } from "../../components/ui/Modal";

export function WorkflowList() {
  const navigate = useNavigate();
  const [workflows, setWorkflows] = useState<WorkflowDesignListResult | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [category, setCategory] = useState("");
  const [showDrafts, setShowDrafts] = useState<boolean | undefined>(undefined);
  const [page, setPage] = useState(1);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newWorkflowName, setNewWorkflowName] = useState("");
  const [newWorkflowDescription, setNewWorkflowDescription] = useState("");
  const [creating, setCreating] = useState(false);

  const fetchWorkflows = useCallback(async () => {
    try {
      setLoading(true);
      const result = await workflowDesignerApi.listWorkflows({
        search: searchTerm || undefined,
        category: category || undefined,
        isDraft: showDrafts,
        page,
        pageSize: 20,
        orderDesc: true,
      });
      setWorkflows(result);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load workflows");
    } finally {
      setLoading(false);
    }
  }, [searchTerm, category, showDrafts, page]);

  useEffect(() => {
    fetchWorkflows();
  }, [fetchWorkflows]);

  const handleCreateWorkflow = async () => {
    if (!newWorkflowName.trim()) return;

    try {
      setCreating(true);
      const created = await workflowDesignerApi.createWorkflow({
        name: newWorkflowName,
        description: newWorkflowDescription || undefined,
        category: "General",
      });
      setShowCreateModal(false);
      setNewWorkflowName("");
      setNewWorkflowDescription("");
      navigate(`/workflow/designer/${created.id}`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create workflow",
      );
    } finally {
      setCreating(false);
    }
  };

  const handleDeleteWorkflow = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"?`)) return;

    try {
      await workflowDesignerApi.deleteWorkflow(id);
      fetchWorkflows();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete workflow",
      );
    }
  };

  const handleCloneWorkflow = async (id: string, originalName: string) => {
    const newName = window.prompt(
      "Enter name for the cloned workflow:",
      `${originalName} (Copy)`,
    );
    if (!newName) return;

    try {
      const cloned = await workflowDesignerApi.cloneWorkflow(id, newName);
      navigate(`/workflow/designer/${cloned.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to clone workflow");
    }
  };

  const getStatusBadge = (workflow: WorkflowDesignSummary) => {
    if (workflow.isDraft) {
      return (
        <span className="px-2 py-1 text-xs rounded-full bg-yellow-100 text-yellow-800">
          Draft
        </span>
      );
    }
    if (workflow.isActive) {
      return (
        <span className="px-2 py-1 text-xs rounded-full bg-green-100 text-green-800">
          Published
        </span>
      );
    }
    return (
      <span className="px-2 py-1 text-xs rounded-full bg-gray-100 text-gray-800">
        Inactive
      </span>
    );
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Workflow Designer
          </h1>
          <p className="text-gray-600 mt-1">
            Create and manage automated workflows
          </p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center gap-2"
        >
          <span>+</span>
          <span>New Workflow</span>
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-6">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <input
              type="text"
              placeholder="Search workflows..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg"
          >
            <option value="">All Categories</option>
            <option value="General">General</option>
            <option value="Appointments">Appointments</option>
            <option value="Patients">Patients</option>
            <option value="Billing">Billing</option>
            <option value="Laboratory">Laboratory</option>
          </select>
          <select
            value={
              showDrafts === undefined ? "" : showDrafts ? "true" : "false"
            }
            onChange={(e) =>
              setShowDrafts(
                e.target.value === "" ? undefined : e.target.value === "true",
              )
            }
            className="px-4 py-2 border border-gray-300 rounded-lg"
          >
            <option value="">All Status</option>
            <option value="true">Drafts Only</option>
            <option value="false">Published Only</option>
          </select>
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
          {error}
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      )}

      {/* Workflow List */}
      {!loading && workflows && (
        <>
          {workflows.items.length === 0 ? (
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
              <div className="text-gray-400 text-5xl mb-4">📋</div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                No workflows found
              </h3>
              <p className="text-gray-600 mb-4">
                Get started by creating your first workflow
              </p>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Create Workflow
              </button>
            </div>
          ) : (
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Category
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Version
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Nodes
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Modified
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {workflows.items.map((workflow) => (
                    <tr key={workflow.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <Link
                          to={`/workflow/designer/${workflow.id}`}
                          className="text-blue-600 hover:text-blue-800 font-medium"
                        >
                          {workflow.name}
                        </Link>
                        {workflow.description && (
                          <p className="text-sm text-gray-500 truncate max-w-xs">
                            {workflow.description}
                          </p>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {workflow.category || "-"}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getStatusBadge(workflow)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        v{workflow.version}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {workflow.nodeCount}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {workflow.modifiedAt
                          ? new Date(workflow.modifiedAt).toLocaleDateString()
                          : new Date(workflow.createdAt).toLocaleDateString()}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex justify-end gap-2">
                          <Link
                            to={`/workflow/designer/${workflow.id}`}
                            className="text-blue-600 hover:text-blue-800"
                          >
                            Edit
                          </Link>
                          <button
                            onClick={() =>
                              handleCloneWorkflow(workflow.id, workflow.name)
                            }
                            className="text-gray-600 hover:text-gray-800"
                          >
                            Clone
                          </button>
                          <button
                            onClick={() =>
                              handleDeleteWorkflow(workflow.id, workflow.name)
                            }
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

              {/* Pagination */}
              {workflows.totalPages > 1 && (
                <div className="bg-gray-50 px-6 py-3 flex items-center justify-between border-t border-gray-200">
                  <div className="text-sm text-gray-500">
                    Showing {(page - 1) * 20 + 1} to{" "}
                    {Math.min(page * 20, workflows.totalCount)} of{" "}
                    {workflows.totalCount} results
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                      className="px-3 py-1 border border-gray-300 rounded text-sm disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <button
                      onClick={() =>
                        setPage((p) => Math.min(workflows.totalPages, p + 1))
                      }
                      disabled={page === workflows.totalPages}
                      className="px-3 py-1 border border-gray-300 rounded text-sm disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
        </>
      )}

      {/* Create Workflow Modal */}
      <Modal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        title="Create New Workflow"
      >
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
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
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
              placeholder="Enter workflow description (optional)"
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
          <div className="flex justify-end gap-2 pt-4">
            <button
              onClick={() => setShowCreateModal(false)}
              className="px-4 py-2 text-gray-600 hover:text-gray-800"
            >
              Cancel
            </button>
            <button
              onClick={handleCreateWorkflow}
              disabled={!newWorkflowName.trim() || creating}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {creating ? "Creating..." : "Create Workflow"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default WorkflowList;
