/**
 * Custom Hooks
 */

// Audit Logging Hooks
export {
  usePatientRecordAccess,
  useResourceAccess,
  useAuditedApi,
  logCreate,
  logUpdate,
  logDelete,
  logViewList,
  logExport,
  logPrint,
  logAudiogramAccess,
  logEncounterAccess,
  logHearingAidAccess,
  logConsentSigned,
  logSearch,
} from './useAuditLog';
