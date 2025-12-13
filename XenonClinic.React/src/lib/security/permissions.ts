/**
 * Enhanced Role-Based Authorization System
 * Provides granular permission control for healthcare resources
 */

// ============================================
// PERMISSION DEFINITIONS
// ============================================

export const Permission = {
  // Patient Management
  PATIENTS_VIEW: "patients:view",
  PATIENTS_CREATE: "patients:create",
  PATIENTS_UPDATE: "patients:update",
  PATIENTS_DELETE: "patients:delete",
  PATIENTS_EXPORT: "patients:export",

  // Appointment Management
  APPOINTMENTS_VIEW: "appointments:view",
  APPOINTMENTS_CREATE: "appointments:create",
  APPOINTMENTS_UPDATE: "appointments:update",
  APPOINTMENTS_DELETE: "appointments:delete",
  APPOINTMENTS_RESCHEDULE: "appointments:reschedule",

  // Audiology
  AUDIOGRAMS_VIEW: "audiograms:view",
  AUDIOGRAMS_CREATE: "audiograms:create",
  AUDIOGRAMS_UPDATE: "audiograms:update",
  AUDIOGRAMS_DELETE: "audiograms:delete",

  HEARING_AIDS_VIEW: "hearing_aids:view",
  HEARING_AIDS_CREATE: "hearing_aids:create",
  HEARING_AIDS_UPDATE: "hearing_aids:update",
  HEARING_AIDS_DELETE: "hearing_aids:delete",
  HEARING_AIDS_FIT: "hearing_aids:fit",
  HEARING_AIDS_ADJUST: "hearing_aids:adjust",

  ENCOUNTERS_VIEW: "encounters:view",
  ENCOUNTERS_CREATE: "encounters:create",
  ENCOUNTERS_UPDATE: "encounters:update",
  ENCOUNTERS_COMPLETE: "encounters:complete",
  ENCOUNTERS_DELETE: "encounters:delete",

  // Consent Management
  CONSENTS_VIEW: "consents:view",
  CONSENTS_CREATE: "consents:create",
  CONSENTS_SIGN: "consents:sign",
  CONSENTS_REVOKE: "consents:revoke",

  // Laboratory
  LAB_VIEW: "lab:view",
  LAB_CREATE: "lab:create",
  LAB_UPDATE: "lab:update",
  LAB_APPROVE: "lab:approve",
  LAB_DELETE: "lab:delete",

  // Pharmacy
  PHARMACY_VIEW: "pharmacy:view",
  PHARMACY_DISPENSE: "pharmacy:dispense",
  PHARMACY_MANAGE: "pharmacy:manage",

  // Radiology
  RADIOLOGY_VIEW: "radiology:view",
  RADIOLOGY_CREATE: "radiology:create",
  RADIOLOGY_REPORT: "radiology:report",
  RADIOLOGY_DELETE: "radiology:delete",

  // Financial
  FINANCIAL_VIEW: "financial:view",
  FINANCIAL_CREATE: "financial:create",
  FINANCIAL_UPDATE: "financial:update",
  FINANCIAL_APPROVE: "financial:approve",
  FINANCIAL_REPORTS: "financial:reports",

  // Inventory
  INVENTORY_VIEW: "inventory:view",
  INVENTORY_CREATE: "inventory:create",
  INVENTORY_UPDATE: "inventory:update",
  INVENTORY_DELETE: "inventory:delete",
  INVENTORY_ORDER: "inventory:order",

  // HR
  HR_VIEW: "hr:view",
  HR_CREATE: "hr:create",
  HR_UPDATE: "hr:update",
  HR_DELETE: "hr:delete",
  HR_PAYROLL: "hr:payroll",

  // Administration
  ADMIN_USERS: "admin:users",
  ADMIN_ROLES: "admin:roles",
  ADMIN_SETTINGS: "admin:settings",
  ADMIN_AUDIT_LOGS: "admin:audit_logs",
  ADMIN_SYSTEM: "admin:system",

  // Attachments & Documents
  ATTACHMENTS_VIEW: "attachments:view",
  ATTACHMENTS_UPLOAD: "attachments:upload",
  ATTACHMENTS_DOWNLOAD: "attachments:download",
  ATTACHMENTS_DELETE: "attachments:delete",
} as const;
export type Permission = (typeof Permission)[keyof typeof Permission];

// ============================================
// ROLE DEFINITIONS WITH PERMISSIONS
// ============================================

export const RolePermissions: Record<string, Permission[]> = {
  Admin: Object.values(Permission), // Admin has all permissions

  Doctor: [
    // Patient access
    Permission.PATIENTS_VIEW,
    Permission.PATIENTS_CREATE,
    Permission.PATIENTS_UPDATE,
    Permission.PATIENTS_EXPORT,

    // Appointments
    Permission.APPOINTMENTS_VIEW,
    Permission.APPOINTMENTS_CREATE,
    Permission.APPOINTMENTS_UPDATE,
    Permission.APPOINTMENTS_RESCHEDULE,

    // Audiology - full access
    Permission.AUDIOGRAMS_VIEW,
    Permission.AUDIOGRAMS_CREATE,
    Permission.AUDIOGRAMS_UPDATE,
    Permission.HEARING_AIDS_VIEW,
    Permission.HEARING_AIDS_CREATE,
    Permission.HEARING_AIDS_FIT,
    Permission.HEARING_AIDS_ADJUST,
    Permission.ENCOUNTERS_VIEW,
    Permission.ENCOUNTERS_CREATE,
    Permission.ENCOUNTERS_UPDATE,
    Permission.ENCOUNTERS_COMPLETE,

    // Consent
    Permission.CONSENTS_VIEW,
    Permission.CONSENTS_CREATE,

    // Labs & Radiology
    Permission.LAB_VIEW,
    Permission.LAB_CREATE,
    Permission.LAB_APPROVE,
    Permission.RADIOLOGY_VIEW,
    Permission.RADIOLOGY_CREATE,
    Permission.RADIOLOGY_REPORT,

    // Pharmacy - view prescriptions
    Permission.PHARMACY_VIEW,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  Nurse: [
    // Patient access
    Permission.PATIENTS_VIEW,
    Permission.PATIENTS_UPDATE,

    // Appointments
    Permission.APPOINTMENTS_VIEW,
    Permission.APPOINTMENTS_CREATE,
    Permission.APPOINTMENTS_UPDATE,

    // Audiology - limited
    Permission.AUDIOGRAMS_VIEW,
    Permission.AUDIOGRAMS_CREATE,
    Permission.ENCOUNTERS_VIEW,
    Permission.ENCOUNTERS_CREATE,
    Permission.ENCOUNTERS_UPDATE,

    // Consent
    Permission.CONSENTS_VIEW,
    Permission.CONSENTS_CREATE,

    // Labs
    Permission.LAB_VIEW,
    Permission.LAB_CREATE,

    // Inventory
    Permission.INVENTORY_VIEW,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  Receptionist: [
    // Patient - basic access
    Permission.PATIENTS_VIEW,
    Permission.PATIENTS_CREATE,
    Permission.PATIENTS_UPDATE,

    // Appointments - full access
    Permission.APPOINTMENTS_VIEW,
    Permission.APPOINTMENTS_CREATE,
    Permission.APPOINTMENTS_UPDATE,
    Permission.APPOINTMENTS_RESCHEDULE,

    // Consent
    Permission.CONSENTS_VIEW,
    Permission.CONSENTS_CREATE,

    // Financial - limited
    Permission.FINANCIAL_VIEW,
    Permission.FINANCIAL_CREATE,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
  ],

  LabTechnician: [
    // Patient - view only
    Permission.PATIENTS_VIEW,

    // Lab - full access
    Permission.LAB_VIEW,
    Permission.LAB_CREATE,
    Permission.LAB_UPDATE,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  Pharmacist: [
    // Patient - view only
    Permission.PATIENTS_VIEW,

    // Pharmacy - full access
    Permission.PHARMACY_VIEW,
    Permission.PHARMACY_DISPENSE,
    Permission.PHARMACY_MANAGE,

    // Inventory
    Permission.INVENTORY_VIEW,
    Permission.INVENTORY_CREATE,
    Permission.INVENTORY_UPDATE,
    Permission.INVENTORY_ORDER,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
  ],

  Radiologist: [
    // Patient - view only
    Permission.PATIENTS_VIEW,

    // Radiology - full access
    Permission.RADIOLOGY_VIEW,
    Permission.RADIOLOGY_CREATE,
    Permission.RADIOLOGY_REPORT,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  Audiologist: [
    // Patient access
    Permission.PATIENTS_VIEW,
    Permission.PATIENTS_UPDATE,

    // Appointments
    Permission.APPOINTMENTS_VIEW,
    Permission.APPOINTMENTS_CREATE,
    Permission.APPOINTMENTS_UPDATE,

    // Audiology - full access
    Permission.AUDIOGRAMS_VIEW,
    Permission.AUDIOGRAMS_CREATE,
    Permission.AUDIOGRAMS_UPDATE,
    Permission.HEARING_AIDS_VIEW,
    Permission.HEARING_AIDS_CREATE,
    Permission.HEARING_AIDS_UPDATE,
    Permission.HEARING_AIDS_FIT,
    Permission.HEARING_AIDS_ADJUST,
    Permission.ENCOUNTERS_VIEW,
    Permission.ENCOUNTERS_CREATE,
    Permission.ENCOUNTERS_UPDATE,
    Permission.ENCOUNTERS_COMPLETE,

    // Consent
    Permission.CONSENTS_VIEW,
    Permission.CONSENTS_CREATE,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  HRManager: [
    // HR - full access
    Permission.HR_VIEW,
    Permission.HR_CREATE,
    Permission.HR_UPDATE,
    Permission.HR_DELETE,
    Permission.HR_PAYROLL,

    // Admin - limited
    Permission.ADMIN_USERS,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],

  Accountant: [
    // Financial - full access
    Permission.FINANCIAL_VIEW,
    Permission.FINANCIAL_CREATE,
    Permission.FINANCIAL_UPDATE,
    Permission.FINANCIAL_APPROVE,
    Permission.FINANCIAL_REPORTS,

    // Inventory - view only
    Permission.INVENTORY_VIEW,

    // Attachments
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_DOWNLOAD,
  ],
};

// ============================================
// PERMISSION CHECKING UTILITIES
// ============================================

export interface User {
  id: number;
  username: string;
  fullName: string;
  roles: string[];
  permissions?: Permission[];
}

/**
 * Get all permissions for a user based on their roles
 */
export const getUserPermissions = (user: User | null): Permission[] => {
  if (!user) return [];

  // If user has explicit permissions, use those
  if (user.permissions && user.permissions.length > 0) {
    return user.permissions;
  }

  // Otherwise, derive from roles
  const permissions = new Set<Permission>();

  for (const role of user.roles || []) {
    const rolePerms = RolePermissions[role] || [];
    for (const perm of rolePerms) {
      permissions.add(perm);
    }
  }

  return Array.from(permissions);
};

/**
 * Check if user has a specific permission
 */
export const hasPermission = (
  user: User | null,
  permission: Permission,
): boolean => {
  const permissions = getUserPermissions(user);
  return permissions.includes(permission);
};

/**
 * Check if user has any of the specified permissions
 */
export const hasAnyPermission = (
  user: User | null,
  permissions: Permission[],
): boolean => {
  const userPermissions = getUserPermissions(user);
  return permissions.some((p) => userPermissions.includes(p));
};

/**
 * Check if user has all of the specified permissions
 */
export const hasAllPermissions = (
  user: User | null,
  permissions: Permission[],
): boolean => {
  const userPermissions = getUserPermissions(user);
  return permissions.every((p) => userPermissions.includes(p));
};

/**
 * Check if user can access a resource
 */
export const canAccess = (
  user: User | null,
  resource: string,
  action: "view" | "create" | "update" | "delete",
): boolean => {
  const permissionKey = `${resource}:${action}` as Permission;
  return hasPermission(user, permissionKey);
};

// ============================================
// PERMISSION GROUPS (for UI organization)
// ============================================

export const PermissionGroups = {
  "Patient Management": [
    Permission.PATIENTS_VIEW,
    Permission.PATIENTS_CREATE,
    Permission.PATIENTS_UPDATE,
    Permission.PATIENTS_DELETE,
    Permission.PATIENTS_EXPORT,
  ],
  Appointments: [
    Permission.APPOINTMENTS_VIEW,
    Permission.APPOINTMENTS_CREATE,
    Permission.APPOINTMENTS_UPDATE,
    Permission.APPOINTMENTS_DELETE,
    Permission.APPOINTMENTS_RESCHEDULE,
  ],
  Audiology: [
    Permission.AUDIOGRAMS_VIEW,
    Permission.AUDIOGRAMS_CREATE,
    Permission.AUDIOGRAMS_UPDATE,
    Permission.AUDIOGRAMS_DELETE,
    Permission.HEARING_AIDS_VIEW,
    Permission.HEARING_AIDS_CREATE,
    Permission.HEARING_AIDS_UPDATE,
    Permission.HEARING_AIDS_DELETE,
    Permission.HEARING_AIDS_FIT,
    Permission.HEARING_AIDS_ADJUST,
    Permission.ENCOUNTERS_VIEW,
    Permission.ENCOUNTERS_CREATE,
    Permission.ENCOUNTERS_UPDATE,
    Permission.ENCOUNTERS_COMPLETE,
    Permission.ENCOUNTERS_DELETE,
  ],
  "Consent & Documents": [
    Permission.CONSENTS_VIEW,
    Permission.CONSENTS_CREATE,
    Permission.CONSENTS_SIGN,
    Permission.CONSENTS_REVOKE,
    Permission.ATTACHMENTS_VIEW,
    Permission.ATTACHMENTS_UPLOAD,
    Permission.ATTACHMENTS_DOWNLOAD,
    Permission.ATTACHMENTS_DELETE,
  ],
  Laboratory: [
    Permission.LAB_VIEW,
    Permission.LAB_CREATE,
    Permission.LAB_UPDATE,
    Permission.LAB_APPROVE,
    Permission.LAB_DELETE,
  ],
  Pharmacy: [
    Permission.PHARMACY_VIEW,
    Permission.PHARMACY_DISPENSE,
    Permission.PHARMACY_MANAGE,
  ],
  Radiology: [
    Permission.RADIOLOGY_VIEW,
    Permission.RADIOLOGY_CREATE,
    Permission.RADIOLOGY_REPORT,
    Permission.RADIOLOGY_DELETE,
  ],
  Financial: [
    Permission.FINANCIAL_VIEW,
    Permission.FINANCIAL_CREATE,
    Permission.FINANCIAL_UPDATE,
    Permission.FINANCIAL_APPROVE,
    Permission.FINANCIAL_REPORTS,
  ],
  Inventory: [
    Permission.INVENTORY_VIEW,
    Permission.INVENTORY_CREATE,
    Permission.INVENTORY_UPDATE,
    Permission.INVENTORY_DELETE,
    Permission.INVENTORY_ORDER,
  ],
  "Human Resources": [
    Permission.HR_VIEW,
    Permission.HR_CREATE,
    Permission.HR_UPDATE,
    Permission.HR_DELETE,
    Permission.HR_PAYROLL,
  ],
  Administration: [
    Permission.ADMIN_USERS,
    Permission.ADMIN_ROLES,
    Permission.ADMIN_SETTINGS,
    Permission.ADMIN_AUDIT_LOGS,
    Permission.ADMIN_SYSTEM,
  ],
};

/**
 * Get human-readable label for a permission
 */
export const getPermissionLabel = (permission: Permission): string => {
  const labels: Record<Permission, string> = {
    [Permission.PATIENTS_VIEW]: "View Patients",
    [Permission.PATIENTS_CREATE]: "Create Patients",
    [Permission.PATIENTS_UPDATE]: "Update Patients",
    [Permission.PATIENTS_DELETE]: "Delete Patients",
    [Permission.PATIENTS_EXPORT]: "Export Patient Data",

    [Permission.APPOINTMENTS_VIEW]: "View Appointments",
    [Permission.APPOINTMENTS_CREATE]: "Create Appointments",
    [Permission.APPOINTMENTS_UPDATE]: "Update Appointments",
    [Permission.APPOINTMENTS_DELETE]: "Delete Appointments",
    [Permission.APPOINTMENTS_RESCHEDULE]: "Reschedule Appointments",

    [Permission.AUDIOGRAMS_VIEW]: "View Audiograms",
    [Permission.AUDIOGRAMS_CREATE]: "Create Audiograms",
    [Permission.AUDIOGRAMS_UPDATE]: "Update Audiograms",
    [Permission.AUDIOGRAMS_DELETE]: "Delete Audiograms",

    [Permission.HEARING_AIDS_VIEW]: "View Hearing Aids",
    [Permission.HEARING_AIDS_CREATE]: "Register Hearing Aids",
    [Permission.HEARING_AIDS_UPDATE]: "Update Hearing Aids",
    [Permission.HEARING_AIDS_DELETE]: "Delete Hearing Aids",
    [Permission.HEARING_AIDS_FIT]: "Fit Hearing Aids",
    [Permission.HEARING_AIDS_ADJUST]: "Adjust Hearing Aids",

    [Permission.ENCOUNTERS_VIEW]: "View Encounters",
    [Permission.ENCOUNTERS_CREATE]: "Create Encounters",
    [Permission.ENCOUNTERS_UPDATE]: "Update Encounters",
    [Permission.ENCOUNTERS_COMPLETE]: "Complete Encounters",
    [Permission.ENCOUNTERS_DELETE]: "Delete Encounters",

    [Permission.CONSENTS_VIEW]: "View Consent Forms",
    [Permission.CONSENTS_CREATE]: "Create Consent Forms",
    [Permission.CONSENTS_SIGN]: "Sign Consent Forms",
    [Permission.CONSENTS_REVOKE]: "Revoke Consent Forms",

    [Permission.LAB_VIEW]: "View Lab Results",
    [Permission.LAB_CREATE]: "Create Lab Orders",
    [Permission.LAB_UPDATE]: "Update Lab Results",
    [Permission.LAB_APPROVE]: "Approve Lab Results",
    [Permission.LAB_DELETE]: "Delete Lab Results",

    [Permission.PHARMACY_VIEW]: "View Prescriptions",
    [Permission.PHARMACY_DISPENSE]: "Dispense Medications",
    [Permission.PHARMACY_MANAGE]: "Manage Pharmacy",

    [Permission.RADIOLOGY_VIEW]: "View Radiology",
    [Permission.RADIOLOGY_CREATE]: "Create Radiology Orders",
    [Permission.RADIOLOGY_REPORT]: "Write Radiology Reports",
    [Permission.RADIOLOGY_DELETE]: "Delete Radiology Records",

    [Permission.FINANCIAL_VIEW]: "View Financial Records",
    [Permission.FINANCIAL_CREATE]: "Create Invoices",
    [Permission.FINANCIAL_UPDATE]: "Update Financial Records",
    [Permission.FINANCIAL_APPROVE]: "Approve Payments",
    [Permission.FINANCIAL_REPORTS]: "View Financial Reports",

    [Permission.INVENTORY_VIEW]: "View Inventory",
    [Permission.INVENTORY_CREATE]: "Add Inventory Items",
    [Permission.INVENTORY_UPDATE]: "Update Inventory",
    [Permission.INVENTORY_DELETE]: "Delete Inventory Items",
    [Permission.INVENTORY_ORDER]: "Create Purchase Orders",

    [Permission.HR_VIEW]: "View Employee Records",
    [Permission.HR_CREATE]: "Create Employee Records",
    [Permission.HR_UPDATE]: "Update Employee Records",
    [Permission.HR_DELETE]: "Delete Employee Records",
    [Permission.HR_PAYROLL]: "Manage Payroll",

    [Permission.ADMIN_USERS]: "Manage Users",
    [Permission.ADMIN_ROLES]: "Manage Roles",
    [Permission.ADMIN_SETTINGS]: "System Settings",
    [Permission.ADMIN_AUDIT_LOGS]: "View Audit Logs",
    [Permission.ADMIN_SYSTEM]: "System Administration",

    [Permission.ATTACHMENTS_VIEW]: "View Attachments",
    [Permission.ATTACHMENTS_UPLOAD]: "Upload Attachments",
    [Permission.ATTACHMENTS_DOWNLOAD]: "Download Attachments",
    [Permission.ATTACHMENTS_DELETE]: "Delete Attachments",
  };

  return labels[permission] || permission;
};
