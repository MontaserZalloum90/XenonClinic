import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  useMemo,
} from "react";
import type {
  TenantContext,
  TenantContextState,
  UISchema,
  FormLayout,
  ListLayout,
  NavItem,
} from "../types/tenant";
import { useAuth } from "./AuthContext";

// ============================================
// Cache Constants
// ============================================

const CACHE_KEY = "tenantContext";
const CACHE_TIMESTAMP_KEY = "tenantContextTimestamp";
const CACHE_VERSION_KEY = "tenantContextVersion";
const CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes cache TTL
const CURRENT_CACHE_VERSION = "1.0";

// ============================================
// Cache Utilities
// ============================================

interface CachedContext {
  terminology: Record<string, string>;
  companyType: string;
  clinicType: string | null;
  settings: TenantContext["settings"];
}

const getCachedContext = (): CachedContext | null => {
  try {
    const version = localStorage.getItem(CACHE_VERSION_KEY);
    if (version !== CURRENT_CACHE_VERSION) {
      // Clear old cache if version mismatch
      clearCache();
      return null;
    }

    const timestamp = localStorage.getItem(CACHE_TIMESTAMP_KEY);
    if (timestamp) {
      const cacheAge = Date.now() - parseInt(timestamp, 10);
      if (cacheAge > CACHE_TTL_MS) {
        // Cache expired
        return null;
      }
    }

    const cached = localStorage.getItem(CACHE_KEY);
    if (cached) {
      return JSON.parse(cached) as CachedContext;
    }
  } catch {
    // Ignore parse errors
  }
  return null;
};

const setCachedContext = (context: TenantContext): void => {
  try {
    localStorage.setItem(
      CACHE_KEY,
      JSON.stringify({
        terminology: context.terminology,
        companyType: context.companyType,
        clinicType: context.clinicType,
        settings: context.settings,
      }),
    );
    localStorage.setItem(CACHE_TIMESTAMP_KEY, Date.now().toString());
    localStorage.setItem(CACHE_VERSION_KEY, CURRENT_CACHE_VERSION);
  } catch {
    // Ignore storage errors
  }
};

const clearCache = (): void => {
  try {
    localStorage.removeItem(CACHE_KEY);
    localStorage.removeItem(CACHE_TIMESTAMP_KEY);
    localStorage.removeItem(CACHE_VERSION_KEY);
  } catch {
    // Ignore storage errors
  }
};

const invalidateCache = (): void => {
  // Set timestamp to 0 to force refresh on next access
  try {
    localStorage.setItem(CACHE_TIMESTAMP_KEY, "0");
  } catch {
    // Ignore storage errors
  }
};

// ============================================
// Context Interface
// ============================================

interface TenantContextValue extends TenantContextState {
  // Terminology
  t: (key: string, fallback?: string) => string;

  // Features
  hasFeature: (featureCode: string) => boolean;
  getFeatureSettings: <T = Record<string, unknown>>(
    featureCode: string,
  ) => T | undefined;

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

  // Cache management
  refresh: () => Promise<void>;
  updateTerminology: (terminology: Record<string, string>) => Promise<void>;
  invalidateCache: () => void;
  isCacheValid: boolean;
}

const TenantContextContext = createContext<TenantContextValue | undefined>(
  undefined,
);

// ============================================
// Provider Component
// ============================================

interface TenantContextProviderProps {
  children: React.ReactNode;
}

export const TenantContextProvider: React.FC<TenantContextProviderProps> = ({
  children,
}) => {
  const { isAuthenticated, token } = useAuth();
  const [state, setState] = useState<TenantContextState>(() => {
    // Try to load from cache on initial mount for faster first render
    const cached = getCachedContext();
    if (cached) {
      return {
        context: {
          tenantId: 0,
          tenantName: "",
          companyId: 0,
          companyName: "",
          companyType: cached.companyType as TenantContext["companyType"],
          clinicType: cached.clinicType as TenantContext["clinicType"],
          branchId: 0,
          branchName: "",
          logoUrl: null,
          primaryColor: "#3B82F6",
          secondaryColor: "#1E40AF",
          userId: "",
          userName: "",
          userRoles: [],
          userPermissions: [],
          features: {},
          terminology: cached.terminology,
          navigation: [],
          uiSchemas: {},
          formLayouts: {},
          listLayouts: {},
          settings: cached.settings,
        },
        isLoading: false,
        error: null,
        isInitialized: false, // Still need to fetch full context
      };
    }
    return {
      context: null,
      isLoading: false,
      error: null,
      isInitialized: false,
    };
  });
  const [isCacheValid, setIsCacheValid] = useState<boolean>(
    () => getCachedContext() !== null,
  );

  // Fetch tenant context from API
  const fetchContext = useCallback(
    async (forceRefresh = false) => {
      if (!isAuthenticated || !token) {
        setState((prev) => ({
          ...prev,
          isLoading: false,
          isInitialized: true,
        }));
        return;
      }

      // Check cache validity unless forced refresh
      if (!forceRefresh) {
        const cached = getCachedContext();
        if (cached && state.context?.terminology) {
          // Cache is still valid, no need to fetch
          setIsCacheValid(true);
          setState((prev) => ({ ...prev, isInitialized: true }));
          return;
        }
      }

      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const response = await fetch("/api/tenant/context", {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
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

        // Update cache
        setCachedContext(context);
        setIsCacheValid(true);
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "Failed to load configuration";
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: message,
          isInitialized: true,
        }));
        setIsCacheValid(false);
      }
    },
    [isAuthenticated, token, state.context?.terminology],
  );

  // Update terminology and refresh cache
  const updateTerminology = useCallback(
    async (terminology: Record<string, string>) => {
      if (!isAuthenticated || !token) {
        throw new Error("Not authenticated");
      }

      const response = await fetch("/api/admin/terminology", {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(terminology),
      });

      if (!response.ok) {
        throw new Error(`Failed to update terminology: ${response.status}`);
      }

      // Invalidate cache and force refresh
      invalidateCache();
      setIsCacheValid(false);

      // Fetch fresh context from server
      await fetchContext(true);
    },
    [isAuthenticated, token, fetchContext],
  );

  // Handle cache invalidation
  const handleInvalidateCache = useCallback(() => {
    invalidateCache();
    setIsCacheValid(false);
  }, []);

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
      clearCache();
      setIsCacheValid(false);
    }
  }, [isAuthenticated, state.context]);

  // Terminology function
  const t = useCallback(
    (key: string, fallback?: string): string => {
      if (!state.context?.terminology) {
        return fallback ?? key;
      }
      return state.context.terminology[key] ?? fallback ?? key;
    },
    [state.context?.terminology],
  );

  // Feature checks
  const hasFeature = useCallback(
    (featureCode: string): boolean => {
      if (!state.context?.features) return false;
      const feature = state.context.features[featureCode];
      return feature?.enabled ?? false;
    },
    [state.context?.features],
  );

  const getFeatureSettings = useCallback(
    <T = Record<string, unknown>,>(featureCode: string): T | undefined => {
      if (!state.context?.features) return undefined;
      const feature = state.context.features[featureCode];
      return feature?.settings as T | undefined;
    },
    [state.context?.features],
  );

  // Schema getters
  const getSchema = useCallback(
    (entityName: string): UISchema | undefined => {
      return state.context?.uiSchemas?.[entityName];
    },
    [state.context?.uiSchemas],
  );

  const getFormLayout = useCallback(
    (entityName: string): FormLayout | undefined => {
      return state.context?.formLayouts?.[entityName];
    },
    [state.context?.formLayouts],
  );

  const getListLayout = useCallback(
    (entityName: string): ListLayout | undefined => {
      return state.context?.listLayouts?.[entityName];
    },
    [state.context?.listLayouts],
  );

  // Computed values
  const value = useMemo<TenantContextValue>(
    () => ({
      ...state,
      t,
      hasFeature,
      getFeatureSettings,
      getSchema,
      getFormLayout,
      getListLayout,
      navigation: state.context?.navigation ?? [],
      isClinic: state.context?.companyType === "CLINIC",
      isTrading: state.context?.companyType === "TRADING",
      clinicType: state.context?.clinicType ?? null,
      refresh: () => fetchContext(true),
      updateTerminology,
      invalidateCache: handleInvalidateCache,
      isCacheValid,
    }),
    [
      state,
      t,
      hasFeature,
      getFeatureSettings,
      getSchema,
      getFormLayout,
      getListLayout,
      fetchContext,
      updateTerminology,
      handleInvalidateCache,
      isCacheValid,
    ],
  );

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
    throw new Error("useTenant must be used within a TenantContextProvider");
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
export const useFeature = (
  featureCode: string,
): {
  enabled: boolean;
  settings: Record<string, unknown> | undefined;
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
export const useFeatures = (
  featureCodes: string[],
): Record<string, boolean> => {
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
    companyType: context?.companyType ?? "CLINIC",
  };
};
