/**
 * Security Module
 * Comprehensive security utilities for healthcare data protection
 */

// Input Validation
export {
  // Emirates ID
  EMIRATES_ID_REGEX,
  validateEmiratesId,

  // Patient Data
  DEFAULT_PATIENT_RULES,
  validatePatientData,
  validateNameEn,
  validateNameAr,
  validateDateOfBirth,
  validatePhoneNumber,
  validateEmail,
  validateGender,
  sanitizeText,

  // Medical Data
  validateICD10Code,
  validateHearingThreshold,
  validateSerialNumber,

  // Types
  type ValidationResult,
  type PatientData,
  type PatientValidationResult,
  type PatientValidationRules,
} from './validation';

// Audit Logging
export {
  auditLog,
  AuditLogService,
  AuditAction,
  AuditResourceType,
  AuditSeverity,
  type AuditLogEntry,
  type AuditLogFilter,
} from './auditLog';

// Role-Based Authorization
export {
  Permission,
  RolePermissions,
  PermissionGroups,
  getUserPermissions,
  hasPermission,
  hasAnyPermission,
  hasAllPermissions,
  canAccess,
  getPermissionLabel,
  type User,
} from './permissions';

// Data Encryption
export {
  SENSITIVE_FIELDS,
  encryptString,
  decryptString,
  isEncrypted,
  encryptSensitiveFields,
  decryptSensitiveFields,
  maskSensitiveData,
  maskEmiratesId,
  maskPhoneNumber,
  maskEmail,
  secureClear,
  withSecureData,
  hashData,
  verifyHash,
  type EncryptedData,
  type SensitiveField,
} from './encryption';

// File Upload Security
export {
  FILE_UPLOAD_CONFIGS,
  validateFile,
  validateFiles,
  sanitizeFilename,
  getFileExtension,
  isDangerousExtension,
  isAllowedExtension,
  detectMimeType,
  generateSecureFilename,
  createSecureFilePath,
  formatFileSize,
  isImageFile,
  createImagePreview,
  revokeImagePreview,
  type FileUploadConfig,
  type FileValidationResult,
  type FileValidationOptions,
} from './fileUpload';

// Password Policy & Account Lockout
export {
  DEFAULT_PASSWORD_POLICY,
  HIPAA_PASSWORD_POLICY,
  DEFAULT_LOCKOUT_POLICY,
  validatePassword,
  getPasswordStrengthLabel,
  isAccountLocked,
  recordFailedAttempt,
  clearLockoutState,
  getFailedAttempts,
  isPasswordExpired,
  getDaysUntilExpiration,
  getLockoutState,
  type PasswordPolicy,
  type AccountLockoutPolicy,
  type PasswordValidationResult,
} from './passwordPolicy';

// CSRF Protection
export {
  generateCsrfToken,
  getOrCreateCsrfToken,
  refreshCsrfToken,
  clearCsrfToken,
  getCsrfHeader,
  withCsrfProtection,
  requiresCsrfProtection,
  createAxiosCsrfInterceptor,
  getCsrfFormField,
  validateFormCsrfToken,
  initializeCsrfProtection,
  useCsrfToken,
  secureFetch,
  validateOrigin,
  getCurrentOrigin,
} from './csrf';
