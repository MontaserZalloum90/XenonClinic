import { Navigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Forbidden } from "../pages/Error";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
  requireAllRoles?: boolean;
}

export const ProtectedRoute = ({
  children,
  requiredRoles = [],
  requireAllRoles = false,
}: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check role-based access if roles are specified
  if (requiredRoles.length > 0 && user) {
    const userRoles = user.roles || [];

    let hasAccess: boolean;
    if (requireAllRoles) {
      // User must have ALL specified roles
      hasAccess = requiredRoles.every((role) => userRoles.includes(role));
    } else {
      // User must have AT LEAST ONE of the specified roles
      hasAccess = requiredRoles.some((role) => userRoles.includes(role));
    }

    if (!hasAccess) {
      return <Forbidden />;
    }
  }

  return <>{children}</>;
};

// Helper function to check if user has specific role(s)
export const hasRole = (
  userRoles: string[],
  requiredRoles: string[],
  requireAll = false,
): boolean => {
  if (requiredRoles.length === 0) return true;

  if (requireAll) {
    return requiredRoles.every((role) => userRoles.includes(role));
  }
  return requiredRoles.some((role) => userRoles.includes(role));
};

// Common role constants
export const Roles = {
  ADMIN: "Admin",
  DOCTOR: "Doctor",
  NURSE: "Nurse",
  RECEPTIONIST: "Receptionist",
  LAB_TECHNICIAN: "LabTechnician",
  PHARMACIST: "Pharmacist",
  RADIOLOGIST: "Radiologist",
  HR_MANAGER: "HRManager",
  ACCOUNTANT: "Accountant",
  MARKETING_MANAGER: "MarketingManager",
} as const;
