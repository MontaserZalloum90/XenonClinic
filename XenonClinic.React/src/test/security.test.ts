import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import {
  // Validation
  validateEmiratesId,
  validateNameEn,
  validateNameAr,
  validateDateOfBirth,
  validatePhoneNumber,
  validateEmail,
  validateGender,
  validatePatientData,
  sanitizeText,
  validateICD10Code,
  validateHearingThreshold,
  validateSerialNumber,
  EMIRATES_ID_REGEX,

  // Permissions
  Permission,
  RolePermissions,
  getUserPermissions,
  hasPermission,
  hasAnyPermission,
  hasAllPermissions,
  canAccess,
  getPermissionLabel,

  // Audit
  AuditAction,
  AuditResourceType,
  AuditSeverity,
  AuditLogService,

  // Encryption
  maskSensitiveData,
  maskEmiratesId,
  maskPhoneNumber,
  maskEmail,
  hashData,
  verifyHash,
  isEncrypted,
} from '../lib/security';

// ============================================
// VALIDATION TESTS
// ============================================

describe('Input Validation', () => {
  describe('Emirates ID Validation', () => {
    it('validates correct Emirates ID format', () => {
      const result = validateEmiratesId('784-1990-1234567-1');
      expect(result.isValid).toBe(true);
      expect(result.errors).toHaveLength(0);
    });

    it('rejects invalid format', () => {
      const result = validateEmiratesId('123-1990-1234567-1');
      expect(result.isValid).toBe(false);
      expect(result.errors.length).toBeGreaterThan(0);
    });

    it('rejects empty value', () => {
      const result = validateEmiratesId('');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Emirates ID is required');
    });

    it('normalizes dashes', () => {
      const result = validateEmiratesId('784–1990–1234567–1'); // em-dashes
      expect(result.sanitizedValue).toBe('784-1990-1234567-1');
    });

    it('validates year range', () => {
      const futureYear = validateEmiratesId('784-2099-1234567-1');
      expect(futureYear.isValid).toBe(false);

      const pastYear = validateEmiratesId('784-1899-1234567-1');
      expect(pastYear.isValid).toBe(false);
    });

    it('regex matches valid format', () => {
      expect(EMIRATES_ID_REGEX.test('784-2000-1234567-1')).toBe(true);
      expect(EMIRATES_ID_REGEX.test('784-1985-9876543-0')).toBe(true);
    });
  });

  describe('Name Validation (English)', () => {
    it('validates correct name', () => {
      const result = validateNameEn('John Doe');
      expect(result.isValid).toBe(true);
    });

    it('allows hyphens and apostrophes', () => {
      const result = validateNameEn("Mary-Jane O'Brien");
      expect(result.isValid).toBe(true);
    });

    it('rejects too short names', () => {
      const result = validateNameEn('A');
      expect(result.isValid).toBe(false);
    });

    it('rejects empty when required', () => {
      const result = validateNameEn('', { required: true, minLength: 2, maxLength: 100 });
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Name is required');
    });
  });

  describe('Name Validation (Arabic)', () => {
    it('validates Arabic names', () => {
      const result = validateNameAr('محمد أحمد');
      expect(result.isValid).toBe(true);
    });

    it('accepts empty when not required', () => {
      const result = validateNameAr('', { required: false, minLength: 2, maxLength: 100 });
      expect(result.isValid).toBe(true);
    });
  });

  describe('Date of Birth Validation', () => {
    it('validates correct date', () => {
      const result = validateDateOfBirth('1990-01-15');
      expect(result.isValid).toBe(true);
    });

    it('rejects future dates', () => {
      const futureDate = new Date();
      futureDate.setFullYear(futureDate.getFullYear() + 1);
      const result = validateDateOfBirth(futureDate.toISOString());
      expect(result.isValid).toBe(false);
    });

    it('rejects invalid date format', () => {
      const result = validateDateOfBirth('not-a-date');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Invalid date format');
    });

    it('validates age limits', () => {
      const tooOld = new Date('1800-01-01');
      const result = validateDateOfBirth(tooOld.toISOString());
      expect(result.isValid).toBe(false);
    });
  });

  describe('Phone Number Validation', () => {
    it('validates correct phone number', () => {
      const result = validatePhoneNumber('+971-50-123-4567');
      expect(result.isValid).toBe(true);
    });

    it('validates international format', () => {
      const result = validatePhoneNumber('+1 555 123 4567');
      expect(result.isValid).toBe(true);
    });

    it('rejects too short numbers', () => {
      const result = validatePhoneNumber('123');
      expect(result.isValid).toBe(false);
    });

    it('accepts empty when not required', () => {
      const result = validatePhoneNumber('', { required: false, pattern: /^[\d\s+-]+$/ });
      expect(result.isValid).toBe(true);
    });
  });

  describe('Email Validation', () => {
    it('validates correct email', () => {
      const result = validateEmail('user@example.com');
      expect(result.isValid).toBe(true);
    });

    it('normalizes to lowercase', () => {
      const result = validateEmail('User@Example.COM');
      expect(result.sanitizedValue).toBe('user@example.com');
    });

    it('rejects invalid format', () => {
      const result = validateEmail('not-an-email');
      expect(result.isValid).toBe(false);
    });

    it('accepts empty when not required', () => {
      const result = validateEmail('', { required: false, pattern: /.*/ });
      expect(result.isValid).toBe(true);
    });
  });

  describe('Gender Validation', () => {
    it('validates M and F', () => {
      expect(validateGender('M').isValid).toBe(true);
      expect(validateGender('F').isValid).toBe(true);
    });

    it('normalizes full words', () => {
      expect(validateGender('male').sanitizedValue).toBe('M');
      expect(validateGender('Female').sanitizedValue).toBe('F');
    });

    it('rejects invalid values', () => {
      const result = validateGender('X');
      expect(result.isValid).toBe(false);
    });
  });

  describe('Complete Patient Data Validation', () => {
    it('validates complete patient data', () => {
      const result = validatePatientData({
        emiratesId: '784-1990-1234567-1',
        fullNameEn: 'John Doe',
        dateOfBirth: '1990-01-15',
        gender: 'M',
        phoneNumber: '+971501234567',
        email: 'john@example.com',
      });
      expect(result.isValid).toBe(true);
      expect(Object.keys(result.errors)).toHaveLength(0);
    });

    it('collects all validation errors', () => {
      const result = validatePatientData({
        emiratesId: 'invalid',
        fullNameEn: '',
        dateOfBirth: 'not-a-date',
        gender: 'X',
      });
      expect(result.isValid).toBe(false);
      expect(Object.keys(result.errors).length).toBeGreaterThan(0);
    });
  });

  describe('Text Sanitization', () => {
    it('removes null bytes', () => {
      const result = sanitizeText('hello\0world');
      expect(result).not.toContain('\0');
    });

    it('escapes HTML entities', () => {
      const result = sanitizeText('<script>alert("xss")</script>');
      expect(result).toContain('&lt;script&gt;');
      expect(result).not.toContain('<script>');
      expect(result).not.toContain('</script>');
    });

    it('removes SQL injection patterns', () => {
      const result = sanitizeText("Robert; DROP TABLE users--comment");
      expect(result).not.toContain('--');
      // Note: semicolons are preserved but HTML-escaped for quotes
    });

    it('truncates to max length', () => {
      const result = sanitizeText('a'.repeat(2000), { maxLength: 100 });
      expect(result.length).toBeLessThanOrEqual(100);
    });

    it('trims whitespace by default', () => {
      const result = sanitizeText('  hello world  ');
      expect(result).toBe('hello world');
    });
  });

  describe('Medical Data Validation', () => {
    it('validates ICD-10 codes', () => {
      expect(validateICD10Code('H90').isValid).toBe(true);
      expect(validateICD10Code('H90.3').isValid).toBe(true);
      expect(validateICD10Code('H91.23').isValid).toBe(true);
    });

    it('rejects invalid ICD-10 format', () => {
      expect(validateICD10Code('123').isValid).toBe(false);
      expect(validateICD10Code('ABC').isValid).toBe(false);
    });

    it('validates hearing thresholds', () => {
      expect(validateHearingThreshold(25).isValid).toBe(true);
      expect(validateHearingThreshold(-5).isValid).toBe(true);
      expect(validateHearingThreshold(120).isValid).toBe(true);
    });

    it('rejects out-of-range thresholds', () => {
      expect(validateHearingThreshold(-15).isValid).toBe(false);
      expect(validateHearingThreshold(130).isValid).toBe(false);
    });

    it('warns about non-5dB increments', () => {
      const result = validateHearingThreshold(23);
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Threshold should be in 5 dB increments');
    });

    it('validates serial numbers', () => {
      expect(validateSerialNumber('SN12345').isValid).toBe(true);
      expect(validateSerialNumber('ABC-123-DEF').isValid).toBe(true);
    });

    it('rejects too short serial numbers', () => {
      expect(validateSerialNumber('AB').isValid).toBe(false);
    });
  });
});

// ============================================
// PERMISSION TESTS
// ============================================

describe('Role-Based Authorization', () => {
  const adminUser = {
    id: 1,
    username: 'admin',
    fullName: 'Admin User',
    roles: ['Admin'],
  };

  const doctorUser = {
    id: 2,
    username: 'doctor',
    fullName: 'Dr. Smith',
    roles: ['Doctor'],
  };

  const nurseUser = {
    id: 3,
    username: 'nurse',
    fullName: 'Nurse Jones',
    roles: ['Nurse'],
  };

  const multiRoleUser = {
    id: 4,
    username: 'super',
    fullName: 'Super User',
    roles: ['Doctor', 'HRManager'],
  };

  describe('getUserPermissions', () => {
    it('returns all permissions for admin', () => {
      const perms = getUserPermissions(adminUser);
      expect(perms.length).toBe(Object.values(Permission).length);
    });

    it('returns role-specific permissions', () => {
      const perms = getUserPermissions(doctorUser);
      expect(perms).toContain(Permission.PATIENTS_VIEW);
      expect(perms).toContain(Permission.AUDIOGRAMS_CREATE);
      expect(perms).not.toContain(Permission.HR_PAYROLL);
    });

    it('returns empty array for null user', () => {
      const perms = getUserPermissions(null);
      expect(perms).toHaveLength(0);
    });

    it('combines permissions for multi-role users', () => {
      const perms = getUserPermissions(multiRoleUser);
      expect(perms).toContain(Permission.PATIENTS_VIEW); // Doctor
      expect(perms).toContain(Permission.HR_PAYROLL); // HRManager
    });

    it('uses explicit permissions if provided', () => {
      const userWithExplicit = {
        ...nurseUser,
        permissions: [Permission.ADMIN_SYSTEM],
      };
      const perms = getUserPermissions(userWithExplicit);
      expect(perms).toContain(Permission.ADMIN_SYSTEM);
      expect(perms).toHaveLength(1);
    });
  });

  describe('hasPermission', () => {
    it('returns true when user has permission', () => {
      expect(hasPermission(doctorUser, Permission.PATIENTS_VIEW)).toBe(true);
    });

    it('returns false when user lacks permission', () => {
      expect(hasPermission(nurseUser, Permission.ADMIN_SYSTEM)).toBe(false);
    });

    it('returns false for null user', () => {
      expect(hasPermission(null, Permission.PATIENTS_VIEW)).toBe(false);
    });
  });

  describe('hasAnyPermission', () => {
    it('returns true if user has any of the permissions', () => {
      expect(
        hasAnyPermission(doctorUser, [Permission.HR_PAYROLL, Permission.PATIENTS_VIEW])
      ).toBe(true);
    });

    it('returns false if user has none of the permissions', () => {
      expect(
        hasAnyPermission(nurseUser, [Permission.HR_PAYROLL, Permission.ADMIN_SYSTEM])
      ).toBe(false);
    });
  });

  describe('hasAllPermissions', () => {
    it('returns true if user has all permissions', () => {
      expect(
        hasAllPermissions(doctorUser, [Permission.PATIENTS_VIEW, Permission.AUDIOGRAMS_CREATE])
      ).toBe(true);
    });

    it('returns false if user lacks any permission', () => {
      expect(
        hasAllPermissions(doctorUser, [Permission.PATIENTS_VIEW, Permission.HR_PAYROLL])
      ).toBe(false);
    });
  });

  describe('canAccess', () => {
    it('checks resource and action combination', () => {
      expect(canAccess(doctorUser, 'patients', 'view')).toBe(true);
      expect(canAccess(doctorUser, 'patients', 'delete')).toBe(false);
    });
  });

  describe('getPermissionLabel', () => {
    it('returns human-readable label', () => {
      expect(getPermissionLabel(Permission.PATIENTS_VIEW)).toBe('View Patients');
      expect(getPermissionLabel(Permission.HEARING_AIDS_FIT)).toBe('Fit Hearing Aids');
    });
  });

  describe('RolePermissions', () => {
    it('has all defined roles', () => {
      expect(RolePermissions).toHaveProperty('Admin');
      expect(RolePermissions).toHaveProperty('Doctor');
      expect(RolePermissions).toHaveProperty('Nurse');
      expect(RolePermissions).toHaveProperty('Audiologist');
    });

    it('admin has all permissions', () => {
      expect(RolePermissions.Admin.length).toBe(Object.values(Permission).length);
    });
  });
});

// ============================================
// AUDIT LOGGING TESTS
// ============================================

describe('Audit Logging', () => {
  let auditService: AuditLogService;

  beforeEach(() => {
    auditService = new AuditLogService();
    // Mock localStorage
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key) => {
      if (key === 'user') {
        return JSON.stringify({ id: 1, fullName: 'Test User', roles: ['Doctor'] });
      }
      if (key === 'token') {
        return 'test-token-123';
      }
      return null;
    });
  });

  afterEach(() => {
    auditService.stopFlushInterval();
    vi.restoreAllMocks();
  });

  describe('AuditAction enum', () => {
    it('has all required actions', () => {
      expect(AuditAction.VIEW).toBe('VIEW');
      expect(AuditAction.CREATE).toBe('CREATE');
      expect(AuditAction.UPDATE).toBe('UPDATE');
      expect(AuditAction.DELETE).toBe('DELETE');
      expect(AuditAction.LOGIN).toBe('LOGIN');
      expect(AuditAction.LOGOUT).toBe('LOGOUT');
    });
  });

  describe('AuditResourceType enum', () => {
    it('has all resource types', () => {
      expect(AuditResourceType.PATIENT).toBe('PATIENT');
      expect(AuditResourceType.AUDIOGRAM).toBe('AUDIOGRAM');
      expect(AuditResourceType.HEARING_AID).toBe('HEARING_AID');
      expect(AuditResourceType.ENCOUNTER).toBe('ENCOUNTER');
    });
  });

  describe('AuditSeverity enum', () => {
    it('has severity levels', () => {
      expect(AuditSeverity.INFO).toBe('INFO');
      expect(AuditSeverity.WARNING).toBe('WARNING');
      expect(AuditSeverity.ERROR).toBe('ERROR');
      expect(AuditSeverity.CRITICAL).toBe('CRITICAL');
    });
  });

  describe('Audit service methods', () => {
    it('can log events', () => {
      expect(() => {
        auditService.log(AuditAction.VIEW, AuditResourceType.PATIENT, {
          resourceId: 123,
          resourceDescription: 'Test Patient',
        });
      }).not.toThrow();
    });

    it('can log patient access', () => {
      expect(() => {
        auditService.logPatientAccess(1, 'John Doe');
      }).not.toThrow();
    });

    it('can log patient modification', () => {
      expect(() => {
        auditService.logPatientModification(1, 'John Doe', {
          phoneNumber: { old: '123', new: '456' },
        });
      }).not.toThrow();
    });

    it('can log login events', () => {
      expect(() => {
        auditService.logLogin(true, 'testuser');
        auditService.logLogin(false, 'testuser', 'Invalid password');
      }).not.toThrow();
    });

    it('can log access denied', () => {
      expect(() => {
        auditService.logAccessDenied(AuditResourceType.PATIENT, 123, 'Admin');
      }).not.toThrow();
    });
  });
});

// ============================================
// ENCRYPTION TESTS
// ============================================

describe('Data Encryption', () => {
  describe('Data Masking', () => {
    it('masks data with defaults', () => {
      const result = maskSensitiveData('1234567890');
      expect(result).toBe('******7890');
    });

    it('masks with custom visible chars', () => {
      const result = maskSensitiveData('1234567890', { visibleStart: 2, visibleEnd: 2 });
      expect(result).toBe('12******90');
    });

    it('masks with custom mask character', () => {
      const result = maskSensitiveData('1234567890', { maskChar: 'X' });
      expect(result).toBe('XXXXXX7890');
    });

    it('handles short values', () => {
      const result = maskSensitiveData('123');
      expect(result).toBe('***');
    });

    it('handles empty values', () => {
      const result = maskSensitiveData('');
      expect(result).toBe('');
    });
  });

  describe('Emirates ID Masking', () => {
    it('masks Emirates ID showing last 4 digits', () => {
      const result = maskEmiratesId('784-1990-1234567-1');
      expect(result).toContain('***');
      expect(result).toMatch(/-\d$/);
    });

    it('handles empty value', () => {
      expect(maskEmiratesId('')).toBe('');
    });
  });

  describe('Phone Number Masking', () => {
    it('masks phone showing last 4 digits', () => {
      const result = maskPhoneNumber('+971501234567');
      expect(result).toContain('***');
      expect(result).toMatch(/\d{4}$/);
    });

    it('handles empty value', () => {
      expect(maskPhoneNumber('')).toBe('');
    });
  });

  describe('Email Masking', () => {
    it('masks email preserving domain', () => {
      const result = maskEmail('john.doe@example.com');
      expect(result).toContain('@example.com');
      expect(result).toContain('jo');
    });

    it('handles short local part', () => {
      const result = maskEmail('ab@example.com');
      expect(result).toContain('@example.com');
    });

    it('handles empty value', () => {
      expect(maskEmail('')).toBe('');
    });
  });

  describe('Hash Functions', () => {
    it('generates consistent hashes', async () => {
      const hash1 = await hashData('test data');
      const hash2 = await hashData('test data');
      expect(hash1).toBe(hash2);
    });

    it('generates different hashes for different data', async () => {
      const hash1 = await hashData('data1');
      const hash2 = await hashData('data2');
      expect(hash1).not.toBe(hash2);
    });

    it('verifies correct hash', async () => {
      const data = 'test data';
      const hash = await hashData(data);
      const isValid = await verifyHash(data, hash);
      expect(isValid).toBe(true);
    });

    it('rejects incorrect hash', async () => {
      const isValid = await verifyHash('test data', 'wrong-hash');
      expect(isValid).toBe(false);
    });
  });

  describe('isEncrypted', () => {
    it('identifies encrypted data', () => {
      const encrypted = {
        ciphertext: 'abc123',
        iv: 'xyz789',
        salt: 'salt123',
        algorithm: 'AES-GCM',
        version: 1,
      };
      expect(isEncrypted(encrypted)).toBe(true);
    });

    it('rejects non-encrypted data', () => {
      expect(isEncrypted('plain text')).toBe(false);
      expect(isEncrypted(null)).toBe(false);
      expect(isEncrypted({ ciphertext: 'abc' })).toBe(false);
    });
  });
});
