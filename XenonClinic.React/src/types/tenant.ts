// ============================================
// Company & Clinic Types
// ============================================

export type CompanyTypeCode = 'CLINIC' | 'TRADING';
export type ClinicTypeCode = 'AUDIOLOGY' | 'DENTAL' | 'VET' | 'DERMATOLOGY' | 'OPHTHALMOLOGY' | 'GENERAL';

// ============================================
// Feature System
// ============================================

export interface FeatureConfig {
  enabled: boolean;
  settings?: Record<string, unknown>;
}

export type FeatureMap = Record<string, FeatureConfig>;

// ============================================
// Navigation
// ============================================

export interface NavBadge {
  type: 'count' | 'dot';
  countKey?: string;
}

export interface NavItem {
  id: string;
  label: string; // terminology key
  icon: string;
  route: string;
  featureCode: string;
  requiredRoles?: string[];
  children?: NavItem[];
  badge?: NavBadge;
  sortOrder: number;
}

// ============================================
// UI Schema (Field Definitions)
// ============================================

export type FieldType =
  | 'text' | 'textarea' | 'number' | 'currency' | 'percentage'
  | 'date' | 'datetime' | 'time'
  | 'select' | 'multiselect' | 'radio' | 'checkbox' | 'toggle'
  | 'lookup' | 'phone' | 'email' | 'url'
  | 'file' | 'image'
  | 'emiratesId' | 'passport'
  | 'address' | 'coordinates'
  | 'richtext' | 'markdown'
  | 'json' | 'code';

export interface FieldValidation {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: string;
  patternMessage?: string;
  custom?: string;
}

export interface ConditionalRule {
  field: string;
  operator: 'eq' | 'neq' | 'gt' | 'gte' | 'lt' | 'lte' | 'in' | 'notIn' | 'contains' | 'empty' | 'notEmpty';
  value?: unknown;
}

export interface FieldOption {
  value: unknown;
  label: string;
}

export interface FieldDefinition {
  name: string;
  type: FieldType;
  label?: string; // terminology key override
  placeholder?: string;
  helpText?: string;
  defaultValue?: unknown;
  validation?: FieldValidation;

  // For select/lookup
  options?: FieldOption[];
  lookupEndpoint?: string;
  lookupDisplayField?: string;
  lookupValueField?: string;

  // Visibility/behavior
  visible?: boolean | ConditionalRule[];
  disabled?: boolean | ConditionalRule[];
  readOnly?: boolean;

  // Display
  width?: 'full' | 'half' | 'third' | 'quarter';
  sortable?: boolean;
  filterable?: boolean;
  searchable?: boolean;

  // For currency/number
  currency?: string;
  decimals?: number;

  // For file upload
  accept?: string;
  maxSize?: number;
  multiple?: boolean;
}

export interface DefaultSort {
  field: string;
  direction: 'asc' | 'desc';
}

export interface UISchema {
  entityName: string;
  displayName: string; // terminology key
  displayNamePlural: string;
  primaryField: string;
  fields: FieldDefinition[];
  defaultSort?: DefaultSort;
}

// ============================================
// Form Layout
// ============================================

export interface FormSection {
  id: string;
  title: string; // terminology key
  description?: string;
  collapsible?: boolean;
  defaultCollapsed?: boolean;
  visible?: boolean | ConditionalRule[];
  columns?: 1 | 2 | 3 | 4;
  fields: string[]; // field names from UISchema
}

export interface FormLayout {
  entityName: string;
  sections: FormSection[];
  submitLabel?: string;
  cancelLabel?: string;
  showDelete?: boolean;
  deleteConfirmMessage?: string;
}

// ============================================
// List/Table Layout
// ============================================

export interface ListColumn {
  field: string;
  width?: number | string;
  align?: 'left' | 'center' | 'right';
  format?: 'currency' | 'date' | 'datetime' | 'percentage' | 'badge' | 'avatar';
  sortable?: boolean;
  hidden?: boolean;
}

export interface ListAction {
  id: string;
  label: string; // terminology key
  icon: string;
  type: 'primary' | 'secondary' | 'danger';
  requiresSelection?: boolean;
  confirmMessage?: string;
  featureCode?: string;
  requiredRoles?: string[];
}

export interface ListActions {
  row: ListAction[];
  bulk: ListAction[];
  header: ListAction[];
}

export interface ListFilter {
  field: string;
  type: 'text' | 'select' | 'date' | 'dateRange' | 'boolean';
  options?: FieldOption[];
}

export interface ListLayout {
  entityName: string;
  columns: ListColumn[];
  actions: ListActions;
  filters: ListFilter[];
  defaultPageSize: number;
  pageSizeOptions: number[];
  showSearch: boolean;
  searchFields: string[];
}

// ============================================
// Tenant Settings
// ============================================

export interface TenantSettings {
  currency: string;
  timezone: string;
  dateFormat: string;
  timeFormat: string;
  language: string;
}

// ============================================
// Tenant Context (API Response)
// ============================================

export interface TenantContext {
  tenantId: number;
  tenantName: string;
  companyId: number;
  companyName: string;
  companyType: CompanyTypeCode;
  clinicType: ClinicTypeCode | null;
  branchId: number;
  branchName: string;

  // Branding
  logoUrl: string | null;
  primaryColor: string;
  secondaryColor: string;

  // User context
  userId: string;
  userName: string;
  userRoles: string[];
  userPermissions: string[];

  // Configuration (merged based on precedence)
  features: FeatureMap;
  terminology: Record<string, string>;
  navigation: NavItem[];

  // UI configuration
  uiSchemas: Record<string, UISchema>;
  formLayouts: Record<string, FormLayout>;
  listLayouts: Record<string, ListLayout>;

  // Settings
  settings: TenantSettings;
}

// ============================================
// Context State
// ============================================

export interface TenantContextState {
  context: TenantContext | null;
  isLoading: boolean;
  error: string | null;
  isInitialized: boolean;
}
