// Security & Audit Types

export const AuditActionType = {
  Create: 0,
  Read: 1,
  Update: 2,
  Delete: 3,
  Login: 4,
  Logout: 5,
  Export: 6,
  Print: 7,
  PermissionChange: 8,
  PasswordChange: 9,
  Failed: 10,
} as const;

export type AuditActionType =
  (typeof AuditActionType)[keyof typeof AuditActionType];

export const SecurityAlertSeverity = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3,
} as const;

export type SecurityAlertSeverity =
  (typeof SecurityAlertSeverity)[keyof typeof SecurityAlertSeverity];

export interface AuditLog {
  id: number;
  timestamp: string;
  userId: number;
  username: string;
  userRole: string;
  action: AuditActionType;
  module: string;
  entityType: string;
  entityId?: number;
  description: string;
  ipAddress: string;
  userAgent?: string;
  oldValue?: string;
  newValue?: string;
  success: boolean;
  errorMessage?: string;
}

export interface UserAccessReview {
  id: number;
  userId: number;
  username: string;
  fullName: string;
  email: string;
  role: string;
  department?: string;
  permissions: Permission[];
  lastLogin?: string;
  lastPasswordChange?: string;
  accountStatus: "active" | "inactive" | "locked" | "expired";
  mfaEnabled: boolean;
  reviewStatus: "pending" | "approved" | "revoked" | "modified";
  reviewedBy?: string;
  reviewedAt?: string;
  reviewNotes?: string;
}

export interface Permission {
  id: number;
  name: string;
  module: string;
  description: string;
  granted: boolean;
  grantedAt?: string;
  grantedBy?: string;
}

export interface PasswordPolicy {
  id: number;
  name: string;
  minLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
  preventReuse: number;
  maxAge: number;
  lockoutThreshold: number;
  lockoutDuration: number;
  mfaRequired: boolean;
  sessionTimeout: number;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface SecurityIncident {
  id: number;
  incidentNumber: string;
  title: string;
  description: string;
  severity: SecurityAlertSeverity;
  status: "open" | "investigating" | "resolved" | "closed";
  type: string;
  affectedUsers?: string[];
  affectedSystems?: string[];
  detectedAt: string;
  reportedBy: string;
  assignedTo?: string;
  resolvedAt?: string;
  resolution?: string;
  preventiveMeasures?: string;
  createdAt: string;
}

export interface ComplianceReport {
  id: number;
  reportName: string;
  reportType: "HIPAA" | "GDPR" | "SOC2" | "ISO27001" | "Custom";
  period: string;
  status: "draft" | "pending" | "approved" | "published";
  complianceScore: number;
  findings: ComplianceFinding[];
  generatedAt: string;
  generatedBy: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface ComplianceFinding {
  id: number;
  category: string;
  requirement: string;
  status: "compliant" | "non-compliant" | "partial" | "not-applicable";
  evidence?: string;
  remediation?: string;
  dueDate?: string;
  priority: "low" | "medium" | "high" | "critical";
}

export interface SecurityStatistics {
  totalUsers: number;
  activeUsers: number;
  lockedAccounts: number;
  pendingAccessReviews: number;
  openIncidents: number;
  criticalIncidents: number;
  failedLogins24h: number;
  complianceScore: number;
  mfaAdoption: number;
  passwordExpiringUsers: number;
}

export interface LoginAttempt {
  id: number;
  username: string;
  ipAddress: string;
  timestamp: string;
  success: boolean;
  failureReason?: string;
  userAgent?: string;
  location?: string;
}

export interface Session {
  id: string;
  userId: number;
  username: string;
  ipAddress: string;
  userAgent: string;
  startedAt: string;
  lastActivity: string;
  expiresAt: string;
  isActive: boolean;
}
