import React, { createContext, useContext, useEffect, useState, useCallback, useMemo } from 'react';
import type {
  TenantContext,
  TenantContextState,
  FeatureMap,
  UISchema,
  FormLayout,
  ListLayout,
  NavItem,
} from '../types/tenant';
import { useAuth } from './AuthContext';

// ============================================
// Context Interface
// ============================================

interface TenantContextValue extends TenantContextState {
  // Terminology
  t: (key: string, fallback?: string) => string;

  // Features
  hasFeature: (featureCode: string) => boolean;
  getFeatureSettings: <T = Record<string, unknown>>(featureCode: string) => T | undefined;

  // Schemas & Layouts
  getSchema: (entityName: string) => UISchema | undefined;
  getFormLayout: (entityName: string) => FormLayout | undefined;
  getListLayout: (entityName: string) => ListLayout | undefined;

  // Navigation
  navigation: NavItem[];

  // Company/Clinic type helpers
  isClinic: boolean;
  isTrading: boolean;
  clinicType: string | null;

  // Refresh
  refresh: () => Promise<void>;
}

const TenantContextContext = createContext<TenantContextValue | undefined>(undefined);

// ============================================
// Provider Component
// ============================================

interface TenantContextProviderProps {
  children: React.ReactNode;
}

export const TenantContextProvider: React.FC<TenantContextProviderProps> = ({ children }) => {
  const { isAuthenticated, token } = useAuth();
  const [state, setState] = useState<TenantContextState>({
    context: null,
    isLoading: false,
    error: null,
    isInitialized: false,
  });

  // Fetch tenant context from API
  const fetchContext = useCallback(async () => {
    if (!isAuthenticated || !token) {
      setState(prev => ({ ...prev, isLoading: false, isInitialized: true }));
      return;
    }

    setState(prev => ({ ...prev, isLoading: true, error: null }));

    try {
      const response = await fetch('/api/tenant/context', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to load configuration: ${response.status}`);
      }

      const context: TenantContext = await response.json();

      setState({
        context,
        isLoading: false,
        error: null,
        isInitialized: true,
      });

      // Cache in localStorage (safe subset only)
      try {
        localStorage.setItem('tenantContext', JSON.stringify({
          terminology: context.terminology,
          companyType: context.companyType,
          clinicType: context.clinicType,
          settings: context.settings,
        }));
      } catch {
        // Ignore storage errors
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to load configuration';
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: message,
        isInitialized: true,
      }));
    }
  }, [isAuthenticated, token]);

  // Initial fetch
  useEffect(() => {
    if (isAuthenticated && !state.isInitialized) {
      fetchContext();
    }
  }, [isAuthenticated, state.isInitialized, fetchContext]);

  // Clear context on logout
  useEffect(() => {
    if (!isAuthenticated && state.context) {
      setState({
        context: null,
        isLoading: false,
        error: null,
        isInitialized: false,
      });
      localStorage.removeItem('tenantContext');
    }
  }, [isAuthenticated, state.context]);

  // Terminology function
  const t = useCallback((key: string, fallback?: string): string => {
    if (!state.context?.terminology) {
      return fallback ?? key;
    }
    return state.context.terminology[key] ?? fallback ?? key;
  }, [state.context?.terminology]);

  // Feature checks
  const hasFeature = useCallback((featureCode: string): boolean => {
    if (!state.context?.features) return false;
    const feature = state.context.features[featureCode];
    return feature?.enabled ?? false;
  }, [state.context?.features]);

  const getFeatureSettings = useCallback(<T = Record<string, unknown>>(featureCode: string): T | undefined => {
    if (!state.context?.features) return undefined;
    const feature = state.context.features[featureCode];
    return feature?.settings as T | undefined;
  }, [state.context?.features]);

  // Schema getters
  const getSchema = useCallback((entityName: string): UISchema | undefined => {
    return state.context?.uiSchemas?.[entityName];
  }, [state.context?.uiSchemas]);

  const getFormLayout = useCallback((entityName: string): FormLayout | undefined => {
    return state.context?.formLayouts?.[entityName];
  }, [state.context?.formLayouts]);

  const getListLayout = useCallback((entityName: string): ListLayout | undefined => {
    return state.context?.listLayouts?.[entityName];
  }, [state.context?.listLayouts]);

  // Computed values
  const value = useMemo<TenantContextValue>(() => ({
    ...state,
    t,
    hasFeature,
    getFeatureSettings,
    getSchema,
    getFormLayout,
    getListLayout,
    navigation: state.context?.navigation ?? [],
    isClinic: state.context?.companyType === 'CLINIC',
    isTrading: state.context?.companyType === 'TRADING',
    clinicType: state.context?.clinicType ?? null,
    refresh: fetchContext,
  }), [state, t, hasFeature, getFeatureSettings, getSchema, getFormLayout, getListLayout, fetchContext]);

  return (
    <TenantContextContext.Provider value={value}>
      {children}
    </TenantContextContext.Provider>
  );
};

// ============================================
// Hooks
// ============================================

/**
 * Main hook to access tenant context
 */
export const useTenant = (): TenantContextValue => {
  const context = useContext(TenantContextContext);
  if (!context) {
    throw new Error('useTenant must be used within a TenantContextProvider');
  }
  return context;
};

/**
 * Hook for terminology/translations
 * Returns the t() function directly
 */
export const useT = (): ((key: string, fallback?: string) => string) => {
  const { t } = useTenant();
  return t;
};

/**
 * Hook for feature checks
 */
export const useFeature = (featureCode: string): {
  enabled: boolean;
  settings: Record<string, unknown> | undefined
} => {
  const { hasFeature, getFeatureSettings } = useTenant();
  return {
    enabled: hasFeature(featureCode),
    settings: getFeatureSettings(featureCode),
  };
};

/**
 * Hook for multiple feature checks
 */
export const useFeatures = (featureCodes: string[]): Record<string, boolean> => {
  const { hasFeature } = useTenant();
  return useMemo(() => {
    const result: Record<string, boolean> = {};
    for (const code of featureCodes) {
      result[code] = hasFeature(code);
    }
    return result;
  }, [featureCodes, hasFeature]);
};

/**
 * Hook for navigation items
 */
export const useNavigation = (): NavItem[] => {
  const { navigation } = useTenant();
  return navigation;
};

/**
 * Hook for entity schema
 */
export const useSchema = (entityName: string): UISchema | undefined => {
  const { getSchema } = useTenant();
  return getSchema(entityName);
};

/**
 * Hook for form layout
 */
export const useFormLayout = (entityName: string): FormLayout | undefined => {
  const { getFormLayout } = useTenant();
  return getFormLayout(entityName);
};

/**
 * Hook for list layout
 */
export const useListLayout = (entityName: string): ListLayout | undefined => {
  const { getListLayout } = useTenant();
  return getListLayout(entityName);
};

/**
 * Hook for company type checks
 */
export const useCompanyType = (): {
  isClinic: boolean;
  isTrading: boolean;
  clinicType: string | null;
  companyType: string;
} => {
  const { context, isClinic, isTrading, clinicType } = useTenant();
  return {
    isClinic,
    isTrading,
    clinicType,
    companyType: context?.companyType ?? 'CLINIC',
  };
};
