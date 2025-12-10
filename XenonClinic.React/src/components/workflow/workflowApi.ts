import axios from 'axios';
import type {
  WorkflowDesignModel,
  WorkflowDesignListResult,
  WorkflowValidationResult,
  NodeTypeCatalog,
  WorkflowStatistics,
} from './types';

const API_BASE = '/api/v1/workflow';

// Designer API

export const workflowDesignerApi = {
  getNodeTypes: async (): Promise<NodeTypeCatalog> => {
    const response = await axios.get<NodeTypeCatalog>(`${API_BASE}/designer/node-types`);
    return response.data;
  },

  listWorkflows: async (params?: {
    search?: string;
    category?: string;
    isDraft?: boolean;
    page?: number;
    pageSize?: number;
    orderBy?: string;
    orderDesc?: boolean;
  }): Promise<WorkflowDesignListResult> => {
    const response = await axios.get<WorkflowDesignListResult>(`${API_BASE}/designer/workflows`, {
      params,
    });
    return response.data;
  },

  getWorkflow: async (id: string, version?: number): Promise<WorkflowDesignModel> => {
    const response = await axios.get<WorkflowDesignModel>(`${API_BASE}/designer/workflows/${id}`, {
      params: version ? { version } : undefined,
    });
    return response.data;
  },

  createWorkflow: async (data: {
    name: string;
    description?: string;
    category?: string;
    tags?: string[];
  }): Promise<WorkflowDesignModel> => {
    const response = await axios.post<WorkflowDesignModel>(`${API_BASE}/designer/workflows`, data);
    return response.data;
  },

  saveWorkflow: async (id: string, workflow: WorkflowDesignModel): Promise<WorkflowDesignModel> => {
    const response = await axios.put<WorkflowDesignModel>(
      `${API_BASE}/designer/workflows/${id}`,
      workflow
    );
    return response.data;
  },

  validateWorkflow: async (workflow: WorkflowDesignModel): Promise<WorkflowValidationResult> => {
    const response = await axios.post<WorkflowValidationResult>(
      `${API_BASE}/designer/workflows/validate`,
      workflow
    );
    return response.data;
  },

  publishWorkflow: async (id: string, version: number): Promise<WorkflowDesignModel> => {
    const response = await axios.post<WorkflowDesignModel>(
      `${API_BASE}/designer/workflows/${id}/publish`,
      null,
      { params: { version } }
    );
    return response.data;
  },

  unpublishWorkflow: async (id: string, version: number): Promise<void> => {
    await axios.post(`${API_BASE}/designer/workflows/${id}/unpublish`, null, {
      params: { version },
    });
  },

  deleteWorkflow: async (id: string): Promise<void> => {
    await axios.delete(`${API_BASE}/designer/workflows/${id}`);
  },

  cloneWorkflow: async (id: string, newName: string): Promise<WorkflowDesignModel> => {
    const response = await axios.post<WorkflowDesignModel>(
      `${API_BASE}/designer/workflows/${id}/clone`,
      { newName }
    );
    return response.data;
  },

  exportWorkflow: async (id: string, version?: number): Promise<string> => {
    const response = await axios.get<string>(`${API_BASE}/designer/workflows/${id}/export`, {
      params: version ? { version } : undefined,
    });
    return response.data;
  },

  importWorkflow: async (json: string, overwrite?: boolean): Promise<WorkflowDesignModel> => {
    const response = await axios.post<WorkflowDesignModel>(`${API_BASE}/designer/workflows/import`, {
      json,
      overwrite,
    });
    return response.data;
  },

  getStatistics: async (id: string): Promise<WorkflowStatistics> => {
    const response = await axios.get<WorkflowStatistics>(
      `${API_BASE}/designer/workflows/${id}/statistics`
    );
    return response.data;
  },
};

// Execution API

export interface WorkflowInstance {
  id: string;
  workflowId: string;
  version: number;
  name?: string;
  status: string;
  correlationId?: string;
  priority: number;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  currentActivityId?: string;
  bookmarks: Array<{
    name: string;
    activityId?: string;
    createdAt: string;
  }>;
  error?: {
    code: string;
    message: string;
    activityId?: string;
  };
}

export interface WorkflowExecutionResult {
  instanceId: string;
  status: string;
  output?: Record<string, unknown>;
  error?: {
    code: string;
    message: string;
    activityId?: string;
    stackTrace?: string;
  };
  bookmarks?: Array<{
    name: string;
    activityId?: string;
    createdAt: string;
  }>;
  duration: string;
  activitiesExecuted: number;
}

export const workflowExecutionApi = {
  startWorkflow: async (data: {
    workflowId: string;
    name?: string;
    correlationId?: string;
    priority?: number;
    scheduledStartTime?: string;
    input?: Record<string, unknown>;
    metadata?: Record<string, unknown>;
  }): Promise<WorkflowExecutionResult> => {
    const response = await axios.post<WorkflowExecutionResult>(`${API_BASE}/execution/start`, data);
    return response.data;
  },

  getInstance: async (instanceId: string): Promise<WorkflowInstance> => {
    const response = await axios.get<WorkflowInstance>(
      `${API_BASE}/execution/instances/${instanceId}`
    );
    return response.data;
  },

  listInstances: async (params?: {
    workflowId?: string;
    status?: string;
    correlationId?: string;
    createdAfter?: string;
    createdBefore?: string;
    page?: number;
    pageSize?: number;
    orderDesc?: boolean;
  }): Promise<{
    items: WorkflowInstance[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }> => {
    const response = await axios.get(`${API_BASE}/execution/instances`, { params });
    return response.data;
  },

  resumeWorkflow: async (
    instanceId: string,
    bookmarkName: string,
    input?: Record<string, unknown>
  ): Promise<WorkflowExecutionResult> => {
    const response = await axios.post<WorkflowExecutionResult>(
      `${API_BASE}/execution/instances/${instanceId}/resume`,
      { bookmarkName, input }
    );
    return response.data;
  },

  cancelWorkflow: async (instanceId: string, reason?: string): Promise<void> => {
    await axios.post(`${API_BASE}/execution/instances/${instanceId}/cancel`, { reason });
  },

  terminateWorkflow: async (instanceId: string, reason?: string): Promise<void> => {
    await axios.post(`${API_BASE}/execution/instances/${instanceId}/terminate`, { reason });
  },

  retryWorkflow: async (instanceId: string): Promise<WorkflowExecutionResult> => {
    const response = await axios.post<WorkflowExecutionResult>(
      `${API_BASE}/execution/instances/${instanceId}/retry`
    );
    return response.data;
  },

  sendSignal: async (instanceId: string, signalName: string, data?: unknown): Promise<void> => {
    await axios.post(`${API_BASE}/execution/instances/${instanceId}/signal`, {
      signalName,
      data,
    });
  },

  broadcastSignal: async (
    signalName: string,
    data?: unknown,
    workflowId?: string
  ): Promise<void> => {
    await axios.post(`${API_BASE}/execution/signal/broadcast`, {
      signalName,
      data,
      workflowId,
    });
  },

  triggerEvent: async (
    eventName: string,
    eventData?: unknown
  ): Promise<WorkflowExecutionResult[]> => {
    const response = await axios.post<WorkflowExecutionResult[]>(
      `${API_BASE}/execution/events/trigger`,
      { eventName, eventData }
    );
    return response.data;
  },

  getHistory: async (
    instanceId: string
  ): Promise<
    Array<{
      id: string;
      instanceId: string;
      activityId: string;
      activityName: string;
      activityType: string;
      type: string;
      timestamp: string;
      duration?: string;
      input?: Record<string, unknown>;
      output?: Record<string, unknown>;
      error?: {
        code: string;
        message: string;
      };
    }>
  > => {
    const response = await axios.get(`${API_BASE}/execution/instances/${instanceId}/history`);
    return response.data;
  },
};
