// Workflow Designer Types

export interface NodePosition {
  x: number;
  y: number;
}

export interface NodeDimensions {
  width: number;
  height: number;
}

export interface NodeStyle {
  backgroundColor?: string;
  borderColor?: string;
  textColor?: string;
  iconClass?: string;
  borderWidth?: number;
  borderRadius?: number;
}

export interface EdgeStyle {
  strokeColor?: string;
  strokeWidth?: number;
  strokeDasharray?: string;
  labelBackgroundColor?: string;
}

export interface DesignerNode {
  id: string;
  type: string;
  name: string;
  description?: string;
  position: NodePosition;
  dimensions?: NodeDimensions;
  style?: NodeStyle;
  config: Record<string, unknown>;
  inputMappings: Record<string, string>;
  outputMappings: Record<string, string>;
  boundaryEvents?: BoundaryEventDefinition[];
  isStart: boolean;
  isEnd: boolean;
  data: Record<string, unknown>;
}

export interface DesignerEdge {
  id: string;
  source: string;
  target: string;
  sourceHandle?: string;
  targetHandle?: string;
  label?: string;
  condition?: string;
  isDefault: boolean;
  priority: number;
  type: string;
  style?: EdgeStyle;
  waypoints?: NodePosition[];
  animated: boolean;
}

export interface BoundaryEventDefinition {
  id: string;
  type: string;
  name?: string;
  cancelActivity: boolean;
  config: Record<string, unknown>;
}

export interface ParameterDefinition {
  name: string;
  displayName?: string;
  description?: string;
  type: string;
  isRequired: boolean;
  defaultValue?: unknown;
  validationSchema?: string;
  options?: OptionDefinition[];
}

export interface VariableDefinition {
  name: string;
  type: string;
  defaultValue?: unknown;
  scope: string;
}

export interface TriggerDefinition {
  type: string;
  name: string;
  isEnabled: boolean;
  config: Record<string, unknown>;
}

export interface ErrorHandlerDefinition {
  errorCodes?: string[];
  handlerNodeId?: string;
  compensate: boolean;
  terminate: boolean;
  retry?: RetryConfig;
}

export interface RetryConfig {
  maxRetries: number;
  initialDelayMs: number;
  maxDelayMs: number;
  backoffMultiplier: number;
}

export interface OptionDefinition {
  value: string;
  label: string;
}

export interface ViewportSettings {
  x: number;
  y: number;
  zoom: number;
}

export interface WorkflowDesignModel {
  id: string;
  name: string;
  description?: string;
  version: number;
  category?: string;
  tags: string[];
  isDraft: boolean;
  isActive: boolean;
  tenantId?: number;
  nodes: DesignerNode[];
  edges: DesignerEdge[];
  inputParameters: ParameterDefinition[];
  outputParameters: ParameterDefinition[];
  variables: VariableDefinition[];
  triggers: TriggerDefinition[];
  errorHandlers: ErrorHandlerDefinition[];
  viewport?: ViewportSettings;
  metadata: Record<string, unknown>;
  createdAt: string;
  modifiedAt?: string;
  createdBy?: string;
  modifiedBy?: string;
}

// Node Type Catalog

export interface NodeTypeCatalog {
  categories: NodeTypeCategory[];
}

export interface NodeTypeCategory {
  id: string;
  name: string;
  description?: string;
  iconClass?: string;
  displayOrder: number;
  nodeTypes: NodeTypeDefinition[];
}

export interface NodeTypeDefinition {
  type: string;
  name: string;
  description?: string;
  iconClass?: string;
  iconSvg?: string;
  category: string;
  defaultStyle?: NodeStyle;
  defaultDimensions?: NodeDimensions;
  properties: PropertyDefinition[];
  inputPorts?: PortDefinition[];
  outputPorts?: PortDefinition[];
  supportsMultipleInputs: boolean;
  supportsMultipleOutputs: boolean;
  canHaveBoundaryEvents: boolean;
}

export interface PropertyDefinition {
  name: string;
  displayName: string;
  description?: string;
  type: string;
  isRequired: boolean;
  defaultValue?: unknown;
  placeholder?: string;
  options?: OptionDefinition[];
  validationRegex?: string;
  group?: string;
  displayOrder: number;
  isAdvanced: boolean;
  editorConfig?: Record<string, unknown>;
}

export interface PortDefinition {
  id: string;
  name?: string;
  position: string;
  type: string;
  maxConnections: number;
}

// Validation Result

export interface WorkflowValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  code: string;
  message: string;
  nodeId?: string;
  edgeId?: string;
  propertyPath?: string;
  severity: 'Warning' | 'Error' | 'Critical';
}

export interface ValidationWarning {
  code: string;
  message: string;
  nodeId?: string;
  edgeId?: string;
  propertyPath?: string;
}

// API Response Types

export interface WorkflowDesignListResult {
  items: WorkflowDesignSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface WorkflowDesignSummary {
  id: string;
  name: string;
  description?: string;
  version: number;
  category?: string;
  tags: string[];
  isDraft: boolean;
  isActive: boolean;
  nodeCount: number;
  createdAt: string;
  modifiedAt?: string;
  modifiedBy?: string;
}

export interface WorkflowStatistics {
  workflowId: string;
  totalInstances: number;
  runningInstances: number;
  completedInstances: number;
  faultedInstances: number;
  cancelledInstances: number;
  averageExecutionTime?: string;
  lastExecutedAt?: string;
}
