import { useState, useEffect, useCallback } from "react";
import { useParams, Link } from "react-router-dom";
import { workflowDesignerApi } from "../../components/workflow/workflowApi";
import { WorkflowDesigner } from "../../components/workflow/WorkflowDesigner";
import type {
  WorkflowDesignModel,
  NodeTypeCatalog,
  WorkflowValidationResult,
} from "../../components/workflow/types";
import { useToast } from "../../components/ui/Toast";

export function WorkflowEditor() {
  const { id } = useParams<{ id: string }>();
  const { showToast } = useToast();
  const [workflow, setWorkflow] = useState<WorkflowDesignModel | null>(null);
  const [nodeTypes, setNodeTypes] = useState<NodeTypeCatalog | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Fetch workflow and node types
  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;

      try {
        setLoading(true);
        const [workflowData, nodeTypesData] = await Promise.all([
          workflowDesignerApi.getWorkflow(id),
          workflowDesignerApi.getNodeTypes(),
        ]);
        setWorkflow(workflowData);
        setNodeTypes(nodeTypesData);
        setError(null);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load workflow",
        );
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id]);

  // Warn before leaving with unsaved changes
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (hasUnsavedChanges) {
        e.preventDefault();
        e.returnValue = "";
      }
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [hasUnsavedChanges]);

  // Handle workflow changes
  const handleWorkflowChange = useCallback(
    (updatedWorkflow: WorkflowDesignModel) => {
      setWorkflow(updatedWorkflow);
      setHasUnsavedChanges(true);
    },
    [],
  );

  // Save workflow
  const handleSave = async () => {
    if (!workflow || !id) return;

    try {
      setSaving(true);
      const saved = await workflowDesignerApi.saveWorkflow(id, workflow);
      setWorkflow(saved);
      setHasUnsavedChanges(false);
      showToast("success", "Workflow saved successfully");
    } catch (err) {
      showToast(
        "error",
        err instanceof Error ? err.message : "Failed to save workflow",
      );
    } finally {
      setSaving(false);
    }
  };

  // Validate workflow
  const handleValidate = async (): Promise<WorkflowValidationResult> => {
    if (!workflow) {
      return { isValid: false, errors: [], warnings: [] };
    }

    try {
      const result = await workflowDesignerApi.validateWorkflow(workflow);
      if (result.isValid) {
        showToast("success", "Workflow is valid");
      } else {
        showToast(
          "error",
          `Validation failed: ${result.errors.length} error(s)`,
        );
      }
      return result;
    } catch (err) {
      showToast(
        "error",
        err instanceof Error ? err.message : "Validation failed",
      );
      return { isValid: false, errors: [], warnings: [] };
    }
  };

  // Publish workflow
  const handlePublish = async () => {
    if (!workflow || !id) return;

    // First save any unsaved changes
    if (hasUnsavedChanges) {
      await handleSave();
    }

    try {
      const published = await workflowDesignerApi.publishWorkflow(
        id,
        workflow.version,
      );
      setWorkflow(published);
      showToast("success", "Workflow published successfully");
    } catch (err) {
      showToast(
        "error",
        err instanceof Error ? err.message : "Failed to publish workflow",
      );
    }
  };

  // Export workflow
  const handleExport = async () => {
    if (!workflow || !id) return;

    try {
      const json = await workflowDesignerApi.exportWorkflow(id);
      const blob = new Blob([json], { type: "application/json" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${workflow.name.replace(/[^a-z0-9]/gi, "_")}_v${workflow.version}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (err) {
      showToast(
        "error",
        err instanceof Error ? err.message : "Failed to export workflow",
      );
    }
  };

  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error || !workflow || !nodeTypes) {
    return (
      <div className="h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 text-5xl mb-4">⚠️</div>
          <h2 className="text-xl font-semibold text-gray-900 mb-2">
            Failed to Load Workflow
          </h2>
          <p className="text-gray-600 mb-4">{error || "Workflow not found"}</p>
          <Link
            to="/workflow"
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Back to Workflows
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link
            to="/workflow"
            className="text-gray-600 hover:text-gray-900"
            onClick={(e) => {
              if (hasUnsavedChanges) {
                if (
                  !window.confirm(
                    "You have unsaved changes. Are you sure you want to leave?",
                  )
                ) {
                  e.preventDefault();
                }
              }
            }}
          >
            ← Back
          </Link>
          <div className="border-l border-gray-300 pl-4">
            <div className="flex items-center gap-2">
              <h1 className="text-lg font-semibold text-gray-900">
                {workflow.name}
              </h1>
              <span className="text-sm text-gray-500">v{workflow.version}</span>
              {workflow.isDraft ? (
                <span className="px-2 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-800">
                  Draft
                </span>
              ) : (
                <span className="px-2 py-0.5 text-xs rounded-full bg-green-100 text-green-800">
                  Published
                </span>
              )}
              {hasUnsavedChanges && (
                <span className="text-sm text-orange-500">
                  • Unsaved changes
                </span>
              )}
            </div>
            {workflow.description && (
              <p className="text-sm text-gray-500">{workflow.description}</p>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExport}
            className="px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100 rounded"
          >
            Export
          </button>
          <button
            onClick={handleSave}
            disabled={saving || !hasUnsavedChanges}
            className="px-4 py-1.5 text-sm bg-gray-100 text-gray-700 hover:bg-gray-200 rounded disabled:opacity-50"
          >
            {saving ? "Saving..." : "Save"}
          </button>
          {workflow.isDraft && (
            <button
              onClick={handlePublish}
              className="px-4 py-1.5 text-sm bg-green-600 text-white hover:bg-green-700 rounded"
            >
              Publish
            </button>
          )}
        </div>
      </div>

      {/* Designer */}
      <div className="flex-1 overflow-hidden">
        <WorkflowDesigner
          workflow={workflow}
          nodeTypes={nodeTypes}
          onChange={handleWorkflowChange}
          onValidate={handleValidate}
          onSave={handleSave}
        />
      </div>
    </div>
  );
}

export default WorkflowEditor;
