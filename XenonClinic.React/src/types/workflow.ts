// Workflow Types matching backend entities

export const WorkflowStatus = {
  Pending: 0,
  InProgress: 1,
  Completed: 2,
  Cancelled: 3,
  Failed: 4,
} as const;

export type WorkflowStatus = typeof WorkflowStatus[keyof typeof WorkflowStatus];

export const WorkflowStepType = {
  Manual: 0,
  Automatic: 1,
  Approval: 2,
  Notification: 3,
} as const;

export type WorkflowStepType = typeof WorkflowStepType[keyof typeof WorkflowStepType];

export interface WorkflowStep {
  id: string;
  name: string;
  type: WorkflowStepType;
  assigneeRole?: string;
  actions?: string[];
  nextSteps?: string[];
  conditions?: Record<string, unknown>;
  order?: number;
  description?: string;
}

export interface WorkflowDefinition {
  id: number;
  name: string;
  description?: string;
  steps: WorkflowStep[];
  triggers?: string[];
  isActive: boolean;
  version: number;
  category?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface WorkflowHistory {
  id: number;
  instanceId: number;
  stepId: string;
  stepName: string;
  action: string;
  performedBy?: string;
  performedByName?: string;
  performedAt: string;
  notes?: string;
  previousStatus?: WorkflowStatus;
  newStatus?: WorkflowStatus;
  data?: Record<string, unknown>;
}

export interface WorkflowInstance {
  id: number;
  definitionId: number;
  definitionName: string;
  entityType: string;
  entityId: number;
  currentStep?: string;
  currentStepName?: string;
  status: WorkflowStatus;
  startedAt: string;
  completedAt?: string;
  cancelledAt?: string;
  failedAt?: string;
  startedBy?: string;
  startedByName?: string;
  assignedTo?: string;
  assignedToName?: string;
  history?: WorkflowHistory[];
  data?: Record<string, unknown>;
  errorMessage?: string;
}

export interface WorkflowStatistics {
  totalDefinitions: number;
  activeDefinitions: number;
  totalInstances: number;
  pendingInstances: number;
  inProgressInstances: number;
  completedInstances: number;
  failedInstances: number;
  completionRate: number;
  averageCompletionTime?: number;
  statusDistribution?: Record<WorkflowStatus, number>;
  categoryDistribution?: Record<string, number>;
  instancesByDefinition?: Record<string, number>;
}

export interface CreateWorkflowDefinitionRequest {
  name: string;
  description?: string;
  steps: WorkflowStep[];
  triggers?: string[];
  category?: string;
}

export interface UpdateWorkflowDefinitionRequest {
  id: number;
  name: string;
  description?: string;
  steps: WorkflowStep[];
  triggers?: string[];
  isActive: boolean;
  category?: string;
}

export interface CreateWorkflowInstanceRequest {
  definitionId: number;
  entityType: string;
  entityId: number;
  data?: Record<string, unknown>;
}

export interface UpdateWorkflowInstanceRequest {
  id: number;
  currentStep?: string;
  status: WorkflowStatus;
  assignedTo?: string;
  data?: Record<string, unknown>;
  notes?: string;
}
