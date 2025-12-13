import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Dialog } from '@headlessui/react';
import { format } from 'date-fns';
import { workflowInstancesApi } from '../../lib/api';
import type { WorkflowInstance, WorkflowHistory, WorkflowStatistics } from '../../types/workflow';
import { WorkflowStatus } from '../../types/workflow';

export const WorkflowInstances = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [workflowFilter, setWorkflowFilter] = useState<string>('');
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [selectedInstance, setSelectedInstance] = useState<WorkflowInstance | null>(null);
  const [instanceHistory, setInstanceHistory] = useState<WorkflowHistory[]>([]);

  const { data: instancesData, isLoading } = useQuery<{ data: WorkflowInstance[] }>({
    queryKey: ['workflow-instances'],
    queryFn: workflowInstancesApi.getAll,
  });

  const { data: statsData } = useQuery<{ data: WorkflowStatistics }>({
    queryKey: ['workflow-statistics'],
    queryFn: workflowInstancesApi.getStatistics,
  });

  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      workflowInstancesApi.cancel(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workflow-instances'] });
      queryClient.invalidateQueries({ queryKey: ['workflow-statistics'] });
    },
  });

  const instances = instancesData?.data || [];
  const stats = statsData?.data;

  // Get unique workflow names for filter
  const uniqueWorkflows = Array.from(new Set(instances.map((i) => i.definitionName)));

  const filteredInstances = instances.filter((instance) => {
    const matchesSearch =
      !searchTerm ||
      instance.id.toString().includes(searchTerm) ||
      instance.definitionName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      instance.entityType.toLowerCase().includes(searchTerm.toLowerCase()) ||
      instance.entityId.toString().includes(searchTerm);
    const matchesStatus = !statusFilter || instance.status.toString() === statusFilter;
    const matchesWorkflow =
      !workflowFilter || instance.definitionName === workflowFilter;
    return matchesSearch && matchesStatus && matchesWorkflow;
  });

  const handleViewHistory = async (instance: WorkflowInstance) => {
    setSelectedInstance(instance);
    if (instance.history) {
      setInstanceHistory(instance.history);
    } else {
      const response = await workflowInstancesApi.getHistory(instance.id);
      setInstanceHistory(response.data);
    }
    setIsHistoryModalOpen(true);
  };

  const handleCancelInstance = (instance: WorkflowInstance) => {
    const reason = window.prompt(
      `Are you sure you want to cancel workflow instance #${instance.id}?\n\nPlease provide a reason (optional):`
    );
    if (reason !== null) {
      // null means user clicked Cancel
      cancelMutation.mutate({ id: instance.id, reason: reason || undefined });
    }
  };

  const getStatusLabel = (status: WorkflowStatus): string => {
    const labels = {
      [WorkflowStatus.Pending]: 'Pending',
      [WorkflowStatus.InProgress]: 'In Progress',
      [WorkflowStatus.Completed]: 'Completed',
      [WorkflowStatus.Cancelled]: 'Cancelled',
      [WorkflowStatus.Failed]: 'Failed',
    };
    return labels[status] || 'Unknown';
  };

  const getStatusBadge = (status: WorkflowStatus) => {
    const config: Record<number, { className: string }> = {
      [WorkflowStatus.Pending]: { className: 'bg-gray-100 text-gray-800' },
      [WorkflowStatus.InProgress]: { className: 'bg-blue-100 text-blue-800' },
      [WorkflowStatus.Completed]: { className: 'bg-green-100 text-green-800' },
      [WorkflowStatus.Cancelled]: { className: 'bg-yellow-100 text-yellow-800' },
      [WorkflowStatus.Failed]: { className: 'bg-red-100 text-red-800' },
    };
    const c = config[status] || { className: 'bg-gray-100 text-gray-800' };
    return (
      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
        {getStatusLabel(status)}
      </span>
    );
  };

  const formatDuration = (startedAt: string, completedAt?: string) => {
    const start = new Date(startedAt);
    const end = completedAt ? new Date(completedAt) : new Date();
    const diffMs = end.getTime() - start.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) return `${diffDays}d ${diffHours % 24}h`;
    if (diffHours > 0) return `${diffHours}h ${diffMins % 60}m`;
    return `${diffMins}m`;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Workflow Instances</h1>
          <p className="text-gray-600 mt-1">Track and manage running workflow instances</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="card">
          <p className="text-sm text-gray-600">Total Instances</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats?.totalInstances || 0}</p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">In Progress</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">
            {stats?.inProgressInstances || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Completed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">
            {stats?.completedInstances || 0}
          </p>
        </div>
        <div className="card">
          <p className="text-sm text-gray-600">Failed</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{stats?.failedInstances || 0}</p>
        </div>
      </div>

      <div className="card">
        <div className="mb-4 flex flex-wrap gap-4">
          <div className="flex-1 min-w-[200px]">
            <input
              type="text"
              placeholder="Search by instance ID, workflow, entity..."
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
            {Object.entries(WorkflowStatus).map(([key, value]) => (
              <option key={value} value={value}>
                {key}
              </option>
            ))}
          </select>
          <select
            value={workflowFilter}
            onChange={(e) => setWorkflowFilter(e.target.value)}
            className="input"
          >
            <option value="">All Workflows</option>
            {uniqueWorkflows.map((workflow) => (
              <option key={workflow} value={workflow}>
                {workflow}
              </option>
            ))}
          </select>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            <p className="text-gray-600 mt-2">Loading instances...</p>
          </div>
        ) : filteredInstances && filteredInstances.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Instance ID
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Workflow
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Entity
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Current Step
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Started
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Duration
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredInstances.map((instance) => (
                  <tr key={instance.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      #{instance.id}
                    </td>
                    <td className="px-4 py-3">
                      <div className="text-sm font-medium text-gray-900">
                        {instance.definitionName}
                      </div>
                      {instance.assignedToName && (
                        <div className="text-xs text-gray-500">
                          Assigned to: {instance.assignedToName}
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      <div>{instance.entityType}</div>
                      <div className="text-xs text-gray-500">ID: {instance.entityId}</div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {instance.currentStepName || instance.currentStep || '-'}
                    </td>
                    <td className="px-4 py-3 text-sm">{getStatusBadge(instance.status)}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      <div>{format(new Date(instance.startedAt), 'MMM d, yyyy')}</div>
                      <div className="text-xs text-gray-500">
                        {format(new Date(instance.startedAt), 'HH:mm')}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {formatDuration(instance.startedAt, instance.completedAt)}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewHistory(instance)}
                          className="text-primary-600 hover:text-primary-800"
                        >
                          History
                        </button>
                        {instance.status === WorkflowStatus.InProgress && (
                          <button
                            onClick={() => handleCancelInstance(instance)}
                            className="text-red-600 hover:text-red-800"
                          >
                            Cancel
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            {searchTerm || statusFilter || workflowFilter
              ? 'No instances found matching your filters.'
              : 'No workflow instances found.'}
          </div>
        )}
      </div>

      {/* History Modal */}
      <Dialog
        open={isHistoryModalOpen}
        onClose={() => setIsHistoryModalOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-4xl w-full bg-white rounded-lg shadow-xl max-h-[80vh] flex flex-col">
            <div className="p-6 border-b border-gray-200">
              <Dialog.Title className="text-lg font-medium text-gray-900">
                Workflow History: Instance #{selectedInstance?.id}
              </Dialog.Title>
              {selectedInstance && (
                <div className="mt-2 text-sm text-gray-600">
                  <div>Workflow: {selectedInstance.definitionName}</div>
                  <div>
                    Entity: {selectedInstance.entityType} (ID: {selectedInstance.entityId})
                  </div>
                  <div className="flex items-center gap-2 mt-1">
                    Status: {getStatusBadge(selectedInstance.status)}
                  </div>
                </div>
              )}
            </div>
            <div className="flex-1 overflow-y-auto p-6">
              {instanceHistory.length > 0 ? (
                <div className="space-y-4">
                  {instanceHistory
                    .sort(
                      (a, b) =>
                        new Date(b.performedAt).getTime() - new Date(a.performedAt).getTime()
                    )
                    .map((history) => (
                      <div
                        key={history.id}
                        className="border-l-2 border-primary-300 pl-4 pb-4 relative"
                      >
                        <div className="absolute -left-2 top-0 w-4 h-4 rounded-full bg-primary-500"></div>
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <h4 className="font-medium text-gray-900">{history.stepName}</h4>
                              <span className="text-xs text-gray-500">{history.action}</span>
                            </div>
                            <div className="mt-1 text-sm text-gray-600">
                              {history.performedByName || history.performedBy || 'System'}
                            </div>
                            {history.notes && (
                              <p className="mt-2 text-sm text-gray-600 bg-gray-50 p-2 rounded">
                                {history.notes}
                              </p>
                            )}
                            {history.previousStatus !== undefined &&
                              history.newStatus !== undefined && (
                                <div className="mt-2 flex items-center gap-2 text-xs">
                                  <span>Status changed:</span>
                                  {getStatusBadge(history.previousStatus)}
                                  <span>→</span>
                                  {getStatusBadge(history.newStatus)}
                                </div>
                              )}
                          </div>
                          <div className="text-xs text-gray-500">
                            <div>{format(new Date(history.performedAt), 'MMM d, yyyy')}</div>
                            <div>{format(new Date(history.performedAt), 'HH:mm:ss')}</div>
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">No history available.</div>
              )}
            </div>
            <div className="p-6 border-t border-gray-200 flex justify-end">
              <button onClick={() => setIsHistoryModalOpen(false)} className="btn btn-primary">
                Close
              </button>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  );
};
