// Tenant Context and Hooks
export {
  TenantContextProvider,
  useTenant,
  useT,
  useFeature,
  useFeatures,
  useNavigation,
  useSchema,
  useFormLayout,
  useListLayout,
  useCompanyType,
} from '../../contexts/TenantContext';

// Terminology Components
export { T, TInterpolate, TPlural, interpolateTerminology } from './T';

// Feature Guard Components
export {
  FeatureGuard,
  MultiFeatureGuard,
  FeatureRoute,
  FeatureDisabledMessage,
  useFeatureEnabled,
} from './FeatureGuard';

// Navigation Components
export { DynamicNavigation, DynamicSidebar } from './DynamicNavigation';

// Form Components
export { DynamicForm } from './DynamicForm';

// List Components
export { EntityList } from './EntityList';

// Re-export types
export type {
  TenantContext,
  TenantContextState,
  CompanyTypeCode,
  ClinicTypeCode,
  FeatureConfig,
  FeatureMap,
  NavItem,
  NavBadge,
  UISchema,
  FieldDefinition,
  FieldType,
  FieldValidation,
  FieldOption,
  ConditionalRule,
  FormLayout,
  FormSection,
  ListLayout,
  ListColumn,
  ListAction,
  ListActions,
  ListFilter,
  TenantSettings,
} from '../../types/tenant';
