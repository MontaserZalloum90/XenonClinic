/**
 * Audit Logging Service for Healthcare Compliance
 * Tracks all access and modifications to patient records (HIPAA compliance)
 */

// ============================================
// AUDIT LOG TYPES
// ============================================

export const AuditAction = {
  // View actions
  VIEW: "VIEW",
  VIEW_LIST: "VIEW_LIST",
  VIEW_DETAIL: "VIEW_DETAIL",
  EXPORT: "EXPORT",
  PRINT: "PRINT",

  // Create/Update/Delete actions
  CREATE: "CREATE",
  UPDATE: "UPDATE",
  DELETE: "DELETE",

  // Authentication actions
  LOGIN: "LOGIN",
  LOGOUT: "LOGOUT",
  LOGIN_FAILED: "LOGIN_FAILED",
  PASSWORD_CHANGE: "PASSWORD_CHANGE",

  // Access control
  ACCESS_DENIED: "ACCESS_DENIED",
  PERMISSION_CHANGE: "PERMISSION_CHANGE",

  // Data operations
  SEARCH: "SEARCH",
  DOWNLOAD: "DOWNLOAD",
  UPLOAD: "UPLOAD",
  SIGN: "SIGN",
} as const;
export type AuditAction = (typeof AuditAction)[keyof typeof AuditAction];

export const AuditResourceType = {
  PATIENT: "PATIENT",
  APPOINTMENT: "APPOINTMENT",
  ENCOUNTER: "ENCOUNTER",
  AUDIOGRAM: "AUDIOGRAM",
  HEARING_AID: "HEARING_AID",
  CONSENT_FORM: "CONSENT_FORM",
  ATTACHMENT: "ATTACHMENT",
  PRESCRIPTION: "PRESCRIPTION",
  LAB_RESULT: "LAB_RESULT",
  RADIOLOGY: "RADIOLOGY",
  INVOICE: "INVOICE",
  EMPLOYEE: "EMPLOYEE",
  INVENTORY: "INVENTORY",
  USER: "USER",
  SYSTEM: "SYSTEM",
} as const;
export type AuditResourceType =
  (typeof AuditResourceType)[keyof typeof AuditResourceType];

export const AuditSeverity = {
  INFO: "INFO",
  WARNING: "WARNING",
  ERROR: "ERROR",
  CRITICAL: "CRITICAL",
} as const;
export type AuditSeverity = (typeof AuditSeverity)[keyof typeof AuditSeverity];

export interface AuditLogEntry {
  id: string;
  timestamp: string;
  userId: number;
  userName: string;
  userRole: string;
  action: AuditAction;
  resourceType: AuditResourceType;
  resourceId?: string | number;
  resourceDescription?: string;
  ipAddress?: string;
  userAgent?: string;
  sessionId?: string;
  severity: AuditSeverity;
  details?: Record<string, unknown>;
  previousValue?: unknown;
  newValue?: unknown;
  success: boolean;
  errorMessage?: string;
}

export interface AuditLogFilter {
  userId?: number;
  userName?: string;
  action?: AuditAction;
  resourceType?: AuditResourceType;
  resourceId?: string | number;
  startDate?: string;
  endDate?: string;
  severity?: AuditSeverity;
  success?: boolean;
}

// ============================================
// AUDIT LOG SERVICE
// ============================================

class AuditLogService {
  private queue: AuditLogEntry[] = [];
  private flushInterval: number = 5000; // 5 seconds
  private maxQueueSize: number = 100;
  private intervalId: ReturnType<typeof setInterval> | null = null;
  private apiEndpoint: string = "/api/audit-logs";

  constructor() {
    // Start the flush interval when service is created
    this.startFlushInterval();

    // Flush on page unload
    if (typeof window !== "undefined") {
      window.addEventListener("beforeunload", () => this.flush());
    }
  }

  /**
   * Start periodic flushing of audit logs
   */
  private startFlushInterval(): void {
    if (this.intervalId) return;

    this.intervalId = setInterval(() => {
      this.flush();
    }, this.flushInterval);
  }

  /**
   * Stop periodic flushing
   */
  public stopFlushInterval(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  /**
   * Generate a unique ID for log entries
   */
  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Get current user info from storage/context
   */
  private getCurrentUser(): { id: number; name: string; role: string } {
    try {
      const userStr = localStorage.getItem("user");
      if (userStr) {
        const user = JSON.parse(userStr);
        return {
          id: user.id || 0,
          name: user.fullName || user.username || "Unknown",
          role: Array.isArray(user.roles)
            ? user.roles.join(", ")
            : user.role || "Unknown",
        };
      }
    } catch {
      // Ignore parse errors
    }
    return { id: 0, name: "Anonymous", role: "None" };
  }

  /**
   * Get session info
   */
  private getSessionInfo(): {
    sessionId?: string;
    ipAddress?: string;
    userAgent?: string;
  } {
    return {
      sessionId: sessionStorage.getItem("sessionId") || undefined,
      userAgent:
        typeof navigator !== "undefined" ? navigator.userAgent : undefined,
    };
  }

  /**
   * Log an audit event
   */
  public log(
    action: AuditAction,
    resourceType: AuditResourceType,
    options: {
      resourceId?: string | number;
      resourceDescription?: string;
      details?: Record<string, unknown>;
      previousValue?: unknown;
      newValue?: unknown;
      success?: boolean;
      errorMessage?: string;
      severity?: AuditSeverity;
    } = {},
  ): void {
    const user = this.getCurrentUser();
    const session = this.getSessionInfo();

    const entry: AuditLogEntry = {
      id: this.generateId(),
      timestamp: new Date().toISOString(),
      userId: user.id,
      userName: user.name,
      userRole: user.role,
      action,
      resourceType,
      resourceId: options.resourceId,
      resourceDescription: options.resourceDescription,
      ipAddress: session.ipAddress,
      userAgent: session.userAgent,
      sessionId: session.sessionId,
      severity:
        options.severity ||
        this.getSeverityForAction(action, options.success ?? true),
      details: options.details,
      previousValue: options.previousValue,
      newValue: options.newValue,
      success: options.success ?? true,
      errorMessage: options.errorMessage,
    };

    this.queue.push(entry);

    // Also log to console in development
    if (import.meta.env.DEV) {
      console.log("[Audit]", entry);
    }

    // Flush if queue is full
    if (this.queue.length >= this.maxQueueSize) {
      this.flush();
    }
  }

  /**
   * Determine severity based on action type
   */
  private getSeverityForAction(
    action: AuditAction,
    success: boolean,
  ): AuditSeverity {
    if (!success) {
      return AuditSeverity.ERROR;
    }

    switch (action) {
      case AuditAction.DELETE:
      case AuditAction.PERMISSION_CHANGE:
        return AuditSeverity.WARNING;
      case AuditAction.LOGIN_FAILED:
      case AuditAction.ACCESS_DENIED:
        return AuditSeverity.WARNING;
      case AuditAction.EXPORT:
      case AuditAction.DOWNLOAD:
        return AuditSeverity.INFO;
      default:
        return AuditSeverity.INFO;
    }
  }

  /**
   * Flush queued logs to the server
   */
  public async flush(): Promise<void> {
    if (this.queue.length === 0) return;

    const logsToSend = [...this.queue];
    this.queue = [];

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(this.apiEndpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify({ logs: logsToSend }),
      });

      if (!response.ok) {
        // Re-queue failed logs
        this.queue = [...logsToSend, ...this.queue];
        console.error("[Audit] Failed to flush logs:", response.statusText);
      }
    } catch (error) {
      // Re-queue failed logs
      this.queue = [...logsToSend, ...this.queue];
      console.error("[Audit] Failed to flush logs:", error);
    }
  }

  // ============================================
  // CONVENIENCE METHODS
  // ============================================

  /**
   * Log patient record access
   */
  public logPatientAccess(
    patientId: number,
    patientName: string,
    action: AuditAction = AuditAction.VIEW_DETAIL,
  ): void {
    this.log(action, AuditResourceType.PATIENT, {
      resourceId: patientId,
      resourceDescription: `Patient: ${patientName}`,
    });
  }

  /**
   * Log patient record modification
   */
  public logPatientModification(
    patientId: number,
    patientName: string,
    changes: Record<string, { old: unknown; new: unknown }>,
  ): void {
    this.log(AuditAction.UPDATE, AuditResourceType.PATIENT, {
      resourceId: patientId,
      resourceDescription: `Patient: ${patientName}`,
      details: { fields: Object.keys(changes) },
      previousValue: Object.fromEntries(
        Object.entries(changes).map(([k, v]) => [k, v.old]),
      ),
      newValue: Object.fromEntries(
        Object.entries(changes).map(([k, v]) => [k, v.new]),
      ),
    });
  }

  /**
   * Log audiogram access
   */
  public logAudiogramAccess(
    audiogramId: number,
    patientName: string,
    action: AuditAction = AuditAction.VIEW_DETAIL,
  ): void {
    this.log(action, AuditResourceType.AUDIOGRAM, {
      resourceId: audiogramId,
      resourceDescription: `Audiogram for: ${patientName}`,
    });
  }

  /**
   * Log encounter access
   */
  public logEncounterAccess(
    encounterId: number,
    patientName: string,
    encounterType: string,
  ): void {
    this.log(AuditAction.VIEW_DETAIL, AuditResourceType.ENCOUNTER, {
      resourceId: encounterId,
      resourceDescription: `${encounterType} for: ${patientName}`,
    });
  }

  /**
   * Log hearing aid record access
   */
  public logHearingAidAccess(
    hearingAidId: number,
    serialNumber: string,
    patientName: string,
  ): void {
    this.log(AuditAction.VIEW_DETAIL, AuditResourceType.HEARING_AID, {
      resourceId: hearingAidId,
      resourceDescription: `Hearing Aid ${serialNumber} for: ${patientName}`,
    });
  }

  /**
   * Log consent form signing
   */
  public logConsentFormSigned(
    consentId: number,
    patientName: string,
    formType: string,
  ): void {
    this.log(AuditAction.SIGN, AuditResourceType.CONSENT_FORM, {
      resourceId: consentId,
      resourceDescription: `${formType} signed by: ${patientName}`,
      severity: AuditSeverity.INFO,
    });
  }

  /**
   * Log file download
   */
  public logFileDownload(
    attachmentId: number,
    fileName: string,
    patientId?: number,
  ): void {
    this.log(AuditAction.DOWNLOAD, AuditResourceType.ATTACHMENT, {
      resourceId: attachmentId,
      resourceDescription: `File: ${fileName}`,
      details: patientId ? { patientId } : undefined,
    });
  }

  /**
   * Log data export
   */
  public logDataExport(
    resourceType: AuditResourceType,
    format: string,
    recordCount: number,
    filters?: Record<string, unknown>,
  ): void {
    this.log(AuditAction.EXPORT, resourceType, {
      resourceDescription: `Exported ${recordCount} records as ${format}`,
      details: { format, recordCount, filters },
      severity: AuditSeverity.INFO,
    });
  }

  /**
   * Log authentication events
   */
  public logLogin(
    success: boolean,
    username: string,
    errorMessage?: string,
  ): void {
    this.log(
      success ? AuditAction.LOGIN : AuditAction.LOGIN_FAILED,
      AuditResourceType.USER,
      {
        resourceDescription: `User: ${username}`,
        success,
        errorMessage,
        severity: success ? AuditSeverity.INFO : AuditSeverity.WARNING,
      },
    );
  }

  public logLogout(): void {
    this.log(AuditAction.LOGOUT, AuditResourceType.USER, {
      severity: AuditSeverity.INFO,
    });
  }

  /**
   * Log access denied
   */
  public logAccessDenied(
    resourceType: AuditResourceType,
    resourceId?: string | number,
    requiredRole?: string,
  ): void {
    this.log(AuditAction.ACCESS_DENIED, resourceType, {
      resourceId,
      success: false,
      severity: AuditSeverity.WARNING,
      details: requiredRole ? { requiredRole } : undefined,
    });
  }

  /**
   * Log search operation (for tracking data access patterns)
   */
  public logSearch(
    resourceType: AuditResourceType,
    searchTerms: Record<string, unknown>,
    resultCount: number,
  ): void {
    this.log(AuditAction.SEARCH, resourceType, {
      details: { searchTerms, resultCount },
      severity: AuditSeverity.INFO,
    });
  }
}

// Export singleton instance
export const auditLog = new AuditLogService();

// Export for testing
export { AuditLogService };
