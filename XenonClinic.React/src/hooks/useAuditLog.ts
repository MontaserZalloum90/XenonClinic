/**
 * Audit Logging Hooks
 * React hooks for integrating audit logging into components and API calls
 */

import { useCallback, useEffect, useRef } from 'react';
import { auditLog, AuditAction, AuditResourceType } from '../lib/security';

// ============================================
// PATIENT RECORD ACCESS HOOK
// ============================================

/**
 * Hook to automatically log patient record access when viewing
 */
export const usePatientRecordAccess = (
  patientId: number | undefined,
  patientName: string | undefined,
  options: { enabled?: boolean } = {}
) => {
  const { enabled = true } = options;
  const loggedRef = useRef(false);

  useEffect(() => {
    if (enabled && patientId && patientName && !loggedRef.current) {
      auditLog.logPatientAccess(patientId, patientName, AuditAction.VIEW_DETAIL);
      loggedRef.current = true;
    }
  }, [patientId, patientName, enabled]);

  // Reset when patient changes
  useEffect(() => {
    loggedRef.current = false;
  }, [patientId]);
};

// ============================================
// RESOURCE ACCESS HOOK
// ============================================

/**
 * Generic hook for logging resource access
 */
export const useResourceAccess = (
  resourceType: AuditResourceType,
  resourceId: number | string | undefined,
  resourceDescription: string | undefined,
  options: { enabled?: boolean; action?: AuditAction } = {}
) => {
  const { enabled = true, action = AuditAction.VIEW_DETAIL } = options;
  const loggedRef = useRef(false);

  useEffect(() => {
    if (enabled && resourceId && !loggedRef.current) {
      auditLog.log(action, resourceType, {
        resourceId,
        resourceDescription,
      });
      loggedRef.current = true;
    }
  }, [resourceId, resourceDescription, resourceType, action, enabled]);

  useEffect(() => {
    loggedRef.current = false;
  }, [resourceId]);
};

// ============================================
// AUDITED API HOOK
// ============================================

interface AuditedApiOptions<T> {
  resourceType: AuditResourceType;
  getResourceId?: (data: T) => number | string;
  getResourceDescription?: (data: T) => string;
}

/**
 * Hook that wraps API calls with audit logging
 */
export const useAuditedApi = <T, R>(
  apiFunction: (data: T) => Promise<R>,
  action: AuditAction,
  options: AuditedApiOptions<T>
) => {
  const { resourceType, getResourceId, getResourceDescription } = options;

  const execute = useCallback(
    async (data: T): Promise<R> => {
      const resourceId = getResourceId?.(data);
      const resourceDescription = getResourceDescription?.(data);

      try {
        const result = await apiFunction(data);

        auditLog.log(action, resourceType, {
          resourceId,
          resourceDescription,
          success: true,
        });

        return result;
      } catch (error) {
        auditLog.log(action, resourceType, {
          resourceId,
          resourceDescription,
          success: false,
          errorMessage: error instanceof Error ? error.message : 'Unknown error',
        });
        throw error;
      }
    },
    [apiFunction, action, resourceType, getResourceId, getResourceDescription]
  );

  return execute;
};

// ============================================
// CRUD AUDIT HELPERS
// ============================================

/**
 * Log a create operation
 */
export const logCreate = (
  resourceType: AuditResourceType,
  resourceId: number | string,
  resourceDescription: string,
  details?: Record<string, unknown>
) => {
  auditLog.log(AuditAction.CREATE, resourceType, {
    resourceId,
    resourceDescription,
    details,
    success: true,
  });
};

/**
 * Log an update operation with change tracking
 */
export const logUpdate = (
  resourceType: AuditResourceType,
  resourceId: number | string,
  resourceDescription: string,
  changes: Record<string, { old: unknown; new: unknown }>
) => {
  auditLog.log(AuditAction.UPDATE, resourceType, {
    resourceId,
    resourceDescription,
    details: { changedFields: Object.keys(changes) },
    previousValue: Object.fromEntries(
      Object.entries(changes).map(([k, v]) => [k, v.old])
    ),
    newValue: Object.fromEntries(
      Object.entries(changes).map(([k, v]) => [k, v.new])
    ),
    success: true,
  });
};

/**
 * Log a delete operation
 */
export const logDelete = (
  resourceType: AuditResourceType,
  resourceId: number | string,
  resourceDescription: string
) => {
  auditLog.log(AuditAction.DELETE, resourceType, {
    resourceId,
    resourceDescription,
    success: true,
  });
};

/**
 * Log a view list operation
 */
export const logViewList = (
  resourceType: AuditResourceType,
  filters?: Record<string, unknown>,
  resultCount?: number
) => {
  auditLog.log(AuditAction.VIEW_LIST, resourceType, {
    details: { filters, resultCount },
    success: true,
  });
};

/**
 * Log data export
 */
export const logExport = (
  resourceType: AuditResourceType,
  format: string,
  recordCount: number,
  filters?: Record<string, unknown>
) => {
  auditLog.logDataExport(resourceType, format, recordCount, filters);
};

/**
 * Log print operation
 */
export const logPrint = (
  resourceType: AuditResourceType,
  resourceId: number | string,
  resourceDescription: string
) => {
  auditLog.log(AuditAction.PRINT, resourceType, {
    resourceId,
    resourceDescription,
    success: true,
  });
};

// ============================================
// CLINICAL DATA AUDIT HELPERS
// ============================================

/**
 * Log audiogram access
 */
export const logAudiogramAccess = (
  audiogramId: number,
  patientName: string,
  action: AuditAction = AuditAction.VIEW_DETAIL
) => {
  auditLog.logAudiogramAccess(audiogramId, patientName, action);
};

/**
 * Log encounter access
 */
export const logEncounterAccess = (
  encounterId: number,
  patientName: string,
  encounterType: string
) => {
  auditLog.logEncounterAccess(encounterId, patientName, encounterType);
};

/**
 * Log hearing aid record access
 */
export const logHearingAidAccess = (
  hearingAidId: number,
  serialNumber: string,
  patientName: string
) => {
  auditLog.logHearingAidAccess(hearingAidId, serialNumber, patientName);
};

/**
 * Log consent form signing
 */
export const logConsentSigned = (
  consentId: number,
  patientName: string,
  formType: string
) => {
  auditLog.logConsentFormSigned(consentId, patientName, formType);
};

// ============================================
// SEARCH AUDIT
// ============================================

/**
 * Log search operations (debounced to avoid spam)
 */
let searchTimeout: ReturnType<typeof setTimeout> | null = null;

export const logSearch = (
  resourceType: AuditResourceType,
  searchTerms: Record<string, unknown>,
  resultCount: number,
  debounceMs: number = 1000
) => {
  if (searchTimeout) {
    clearTimeout(searchTimeout);
  }

  searchTimeout = setTimeout(() => {
    auditLog.logSearch(resourceType, searchTerms, resultCount);
  }, debounceMs);
};
