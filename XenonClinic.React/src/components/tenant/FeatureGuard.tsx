import React from 'react';
import { Navigate } from 'react-router-dom';
import { useTenant, useFeature } from '../../contexts/TenantContext';
import { useAuth } from '../../contexts/AuthContext';

/**
 * FeatureGuard Component - Conditionally renders children based on feature flags
 *
 * Usage:
 * <FeatureGuard feature="audiogram">
 *   <AudiogramList />
 * </FeatureGuard>
 *
 * With fallback:
 * <FeatureGuard feature="audiogram" fallback={<FeatureDisabled />}>
 *   <AudiogramList />
 * </FeatureGuard>
 */

interface FeatureGuardProps {
  /** Feature code to check */
  feature: string;
  /** Children to render if feature is enabled */
  children: React.ReactNode;
  /** Optional fallback if feature is disabled */
  fallback?: React.ReactNode;
  /** Whether to redirect to forbidden page instead of showing fallback */
  redirect?: boolean;
}

export const FeatureGuard: React.FC<FeatureGuardProps> = ({
  feature,
  children,
  fallback = null,
  redirect = false,
}) => {
  const { enabled } = useFeature(feature);
  const { isLoading, isInitialized } = useTenant();

  // Show nothing while loading
  if (isLoading || !isInitialized) {
    return null;
  }

  if (!enabled) {
    if (redirect) {
      return <Navigate to="/forbidden" replace />;
    }
    return <>{fallback}</>;
  }

  return <>{children}</>;
};

/**
 * MultiFeatureGuard - Requires multiple features to be enabled
 *
 * Usage:
 * <MultiFeatureGuard features={['audiogram', 'hearingDevices']} requireAll>
 *   <AudiologyDashboard />
 * </MultiFeatureGuard>
 */

interface MultiFeatureGuardProps {
  /** Feature codes to check */
  features: string[];
  /** If true, ALL features must be enabled. If false, ANY feature enables. */
  requireAll?: boolean;
  children: React.ReactNode;
  fallback?: React.ReactNode;
  redirect?: boolean;
}

export const MultiFeatureGuard: React.FC<MultiFeatureGuardProps> = ({
  features,
  requireAll = true,
  children,
  fallback = null,
  redirect = false,
}) => {
  const { hasFeature, isLoading, isInitialized } = useTenant();

  if (isLoading || !isInitialized) {
    return null;
  }

  const enabledFeatures = features.map(f => hasFeature(f));
  const isEnabled = requireAll
    ? enabledFeatures.every(Boolean)
    : enabledFeatures.some(Boolean);

  if (!isEnabled) {
    if (redirect) {
      return <Navigate to="/forbidden" replace />;
    }
    return <>{fallback}</>;
  }

  return <>{children}</>;
};

/**
 * FeatureRoute - Protected route component that checks feature + role
 *
 * Usage in router:
 * <Route path="/audiology/*" element={
 *   <FeatureRoute feature="audiogram" requiredRoles={['Admin', 'Doctor']}>
 *     <AudiologyLayout />
 *   </FeatureRoute>
 * } />
 */

interface FeatureRouteProps {
  feature: string;
  children: React.ReactNode;
  requiredRoles?: string[];
  /** Custom forbidden page */
  forbiddenComponent?: React.ReactNode;
  /** Custom loading component */
  loadingComponent?: React.ReactNode;
}

export const FeatureRoute: React.FC<FeatureRouteProps> = ({
  feature,
  children,
  requiredRoles,
  forbiddenComponent,
  loadingComponent,
}) => {
  const { hasFeature, isLoading, isInitialized, context } = useTenant();
  const { isAuthenticated } = useAuth();

  // Not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Loading state
  if (isLoading || !isInitialized) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    );
  }

  // Check feature
  if (!hasFeature(feature)) {
    if (forbiddenComponent) {
      return <>{forbiddenComponent}</>;
    }
    return <Navigate to="/forbidden" state={{ reason: 'feature_disabled', feature }} replace />;
  }

  // Check roles if specified
  if (requiredRoles && requiredRoles.length > 0) {
    const userRoles = context?.userRoles ?? [];
    const hasRole = requiredRoles.some(role => userRoles.includes(role));

    if (!hasRole) {
      if (forbiddenComponent) {
        return <>{forbiddenComponent}</>;
      }
      return <Navigate to="/forbidden" state={{ reason: 'insufficient_role' }} replace />;
    }
  }

  return <>{children}</>;
};

/**
 * useFeatureEnabled - Hook to check if feature is enabled
 * Returns loading state along with enabled status
 */
export const useFeatureEnabled = (feature: string): {
  enabled: boolean;
  isLoading: boolean;
} => {
  const { hasFeature, isLoading, isInitialized } = useTenant();

  return {
    enabled: hasFeature(feature),
    isLoading: isLoading || !isInitialized,
  };
};

/**
 * FeatureDisabledMessage - Standard message when feature is disabled
 */
interface FeatureDisabledMessageProps {
  featureName?: string;
  className?: string;
}

export const FeatureDisabledMessage: React.FC<FeatureDisabledMessageProps> = ({
  featureName,
  className = '',
}) => {
  return (
    <div className={`text-center py-12 ${className}`}>
      <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-gray-100">
        <svg
          className="h-6 w-6 text-gray-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
          />
        </svg>
      </div>
      <h3 className="mt-4 text-lg font-medium text-gray-900">
        {featureName ? `${featureName} is not enabled` : 'Module not enabled'}
      </h3>
      <p className="mt-2 text-sm text-gray-500">
        This feature is not available for your organization.
        Please contact your administrator for access.
      </p>
    </div>
  );
};

export default FeatureGuard;
