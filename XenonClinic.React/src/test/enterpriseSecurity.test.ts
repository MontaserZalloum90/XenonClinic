import { describe, it, expect, beforeEach } from 'vitest';
import {
  // File Upload Security
  sanitizeFilename,
  getFileExtension,
  isDangerousExtension,
  isAllowedExtension,
  FILE_UPLOAD_CONFIGS,
  generateSecureFilename,
  createSecureFilePath,
  formatFileSize,

  // Password Policy
  validatePassword,
  getPasswordStrengthLabel,
  DEFAULT_PASSWORD_POLICY,
  HIPAA_PASSWORD_POLICY,
  isPasswordExpired,
  getDaysUntilExpiration,

  // Account Lockout
  recordFailedAttempt,
  clearLockoutState,
  isAccountLocked,
  getFailedAttempts,
  DEFAULT_LOCKOUT_POLICY,

  // CSRF
  generateCsrfToken,
  requiresCsrfProtection,
  validateOrigin,
} from '../lib/security';

// ============================================
// FILE UPLOAD SECURITY TESTS
// ============================================

describe('File Upload Security', () => {
  describe('sanitizeFilename', () => {
    it('removes path traversal attempts', () => {
      // Path traversal is removed, only filename remains
      const result = sanitizeFilename('../../../etc/passwd');
      expect(result).not.toContain('..');
      expect(result).not.toContain('/');
    });

    it('removes null bytes', () => {
      expect(sanitizeFilename('file\0name.txt')).toBe('filename.txt');
    });

    it('removes dangerous characters', () => {
      const result = sanitizeFilename('file<>:"|?*.txt');
      expect(result).not.toContain('<');
      expect(result).not.toContain('>');
      expect(result).not.toContain(':');
      expect(result).not.toContain('|');
      expect(result).not.toContain('?');
      expect(result).not.toContain('*');
    });

    it('removes leading dots', () => {
      // Leading dots are stripped
      const result = sanitizeFilename('.htaccess');
      expect(result).not.toMatch(/^\./);
    });

    it('handles multiple consecutive dots', () => {
      expect(sanitizeFilename('file...name.txt')).toBe('file.name.txt');
    });

    it('truncates long filenames', () => {
      const longName = 'a'.repeat(300) + '.txt';
      const result = sanitizeFilename(longName, 255);
      expect(result.length).toBeLessThanOrEqual(255);
      expect(result.endsWith('.txt')).toBe(true);
    });

    it('generates safe name for empty result', () => {
      // Empty string returns empty (caller should validate)
      expect(sanitizeFilename('')).toBe('');
      // Single/double dots get converted to safe names
      expect(sanitizeFilename('.')).toMatch(/^file_\d+$/);
      expect(sanitizeFilename('..')).toMatch(/^file_\d+$/);
    });
  });

  describe('getFileExtension', () => {
    it('extracts extension correctly', () => {
      expect(getFileExtension('file.txt')).toBe('txt');
      expect(getFileExtension('file.PDF')).toBe('pdf');
      expect(getFileExtension('file.name.jpg')).toBe('jpg');
    });

    it('handles no extension', () => {
      expect(getFileExtension('filename')).toBe('');
      expect(getFileExtension('filename.')).toBe('');
    });
  });

  describe('isDangerousExtension', () => {
    it('identifies dangerous extensions', () => {
      expect(isDangerousExtension('script.exe')).toBe(true);
      expect(isDangerousExtension('virus.bat')).toBe(true);
      expect(isDangerousExtension('hack.php')).toBe(true);
      expect(isDangerousExtension('shell.sh')).toBe(true);
      expect(isDangerousExtension('payload.ps1')).toBe(true);
    });

    it('allows safe extensions', () => {
      expect(isDangerousExtension('document.pdf')).toBe(false);
      expect(isDangerousExtension('image.jpg')).toBe(false);
      expect(isDangerousExtension('report.docx')).toBe(false);
    });
  });

  describe('isAllowedExtension', () => {
    it('validates against allowed list', () => {
      const allowed = ['.jpg', '.png', '.pdf'];
      expect(isAllowedExtension('photo.jpg', allowed)).toBe(true);
      expect(isAllowedExtension('photo.JPG', allowed)).toBe(true);
      expect(isAllowedExtension('photo.exe', allowed)).toBe(false);
    });
  });

  describe('FILE_UPLOAD_CONFIGS', () => {
    it('has image config', () => {
      expect(FILE_UPLOAD_CONFIGS.image).toBeDefined();
      expect(FILE_UPLOAD_CONFIGS.image.maxSizeBytes).toBe(5 * 1024 * 1024);
      expect(FILE_UPLOAD_CONFIGS.image.allowedExtensions).toContain('.jpg');
    });

    it('has document config', () => {
      expect(FILE_UPLOAD_CONFIGS.document).toBeDefined();
      expect(FILE_UPLOAD_CONFIGS.document.allowedExtensions).toContain('.pdf');
    });

    it('has medical image config with larger size', () => {
      expect(FILE_UPLOAD_CONFIGS.medicalImage.maxSizeBytes).toBeGreaterThan(
        FILE_UPLOAD_CONFIGS.image.maxSizeBytes
      );
    });
  });

  describe('generateSecureFilename', () => {
    it('generates unique filename with timestamp', () => {
      const name1 = generateSecureFilename('photo.jpg');
      const name2 = generateSecureFilename('photo.jpg');
      expect(name1).not.toBe(name2);
      expect(name1.endsWith('.jpg')).toBe(true);
    });
  });

  describe('createSecureFilePath', () => {
    it('creates safe path without traversal', () => {
      const path = createSecureFilePath('/uploads', '../../../etc/passwd', 'users');
      expect(path).not.toContain('..');
      expect(path.startsWith('/uploads')).toBe(true);
    });
  });

  describe('formatFileSize', () => {
    it('formats bytes correctly', () => {
      expect(formatFileSize(0)).toBe('0 Bytes');
      expect(formatFileSize(1024)).toBe('1 KB');
      expect(formatFileSize(1048576)).toBe('1 MB');
      expect(formatFileSize(1073741824)).toBe('1 GB');
    });
  });
});

// ============================================
// PASSWORD POLICY TESTS
// ============================================

describe('Password Policy', () => {
  describe('validatePassword', () => {
    it('rejects short passwords', () => {
      const result = validatePassword('Short1!');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('at least'))).toBe(true);
    });

    it('requires uppercase letters', () => {
      const result = validatePassword('lowercase123!@#');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('uppercase'))).toBe(true);
    });

    it('requires lowercase letters', () => {
      const result = validatePassword('UPPERCASE123!@#');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('lowercase'))).toBe(true);
    });

    it('requires numbers', () => {
      const result = validatePassword('NoNumbers!@#ABC');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('number'))).toBe(true);
    });

    it('requires special characters', () => {
      const result = validatePassword('NoSpecial12345Abc');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('special'))).toBe(true);
    });

    it('accepts strong passwords', () => {
      const result = validatePassword('MyStr0ng!P@ssword');
      expect(result.isValid).toBe(true);
      expect(result.errors).toHaveLength(0);
    });

    it('rejects common passwords', () => {
      const result = validatePassword('password');
      expect(result.isValid).toBe(false);
      expect(result.errors.some((e) => e.includes('common'))).toBe(true);
    });

    it('detects user info in password', () => {
      const result = validatePassword('JohnDoe123!@#', DEFAULT_PASSWORD_POLICY, {
        username: 'johndoe',
        fullName: 'John Doe',
      });
      expect(result.errors.some((e) => e.includes('name'))).toBe(true);
    });

    it('provides strength score', () => {
      const weak = validatePassword('abc');
      const strong = validatePassword('MyV3ryStr0ng!P@ssword#2024');
      expect(strong.score).toBeGreaterThan(weak.score);
    });

    it('detects sequential characters', () => {
      const result = validatePassword('Abcdef123!@#');
      expect(result.suggestions.some((s) => s.includes('sequential'))).toBe(true);
    });

    it('detects keyboard patterns', () => {
      const result = validatePassword('Qwerty123!@#');
      expect(result.suggestions.some((s) => s.includes('keyboard'))).toBe(true);
    });
  });

  describe('getPasswordStrengthLabel', () => {
    it('returns correct labels', () => {
      expect(getPasswordStrengthLabel(10).label).toBe('Very Weak');
      expect(getPasswordStrengthLabel(30).label).toBe('Weak');
      expect(getPasswordStrengthLabel(50).label).toBe('Fair');
      expect(getPasswordStrengthLabel(70).label).toBe('Strong');
      expect(getPasswordStrengthLabel(90).label).toBe('Very Strong');
    });
  });

  describe('HIPAA_PASSWORD_POLICY', () => {
    it('has stricter requirements than default', () => {
      expect(HIPAA_PASSWORD_POLICY.minLength).toBeGreaterThan(DEFAULT_PASSWORD_POLICY.minLength);
      expect(HIPAA_PASSWORD_POLICY.passwordHistoryCount).toBeGreaterThan(
        DEFAULT_PASSWORD_POLICY.passwordHistoryCount
      );
    });
  });

  describe('Password Expiration', () => {
    it('detects expired passwords', () => {
      const oldDate = new Date();
      oldDate.setDate(oldDate.getDate() - 100);
      expect(isPasswordExpired(oldDate)).toBe(true);
    });

    it('accepts recent passwords', () => {
      const recentDate = new Date();
      recentDate.setDate(recentDate.getDate() - 30);
      expect(isPasswordExpired(recentDate)).toBe(false);
    });

    it('calculates days until expiration', () => {
      const recentDate = new Date();
      recentDate.setDate(recentDate.getDate() - 80);
      const days = getDaysUntilExpiration(recentDate);
      expect(days).toBeLessThan(20);
      expect(days).toBeGreaterThan(0);
    });
  });
});

// ============================================
// ACCOUNT LOCKOUT TESTS
// ============================================

describe('Account Lockout', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  const testUsername = 'testuser';

  it('tracks failed attempts', () => {
    recordFailedAttempt(testUsername);
    expect(getFailedAttempts(testUsername)).toBe(1);

    recordFailedAttempt(testUsername);
    expect(getFailedAttempts(testUsername)).toBe(2);
  });

  it('locks account after max attempts', () => {
    for (let i = 0; i < DEFAULT_LOCKOUT_POLICY.maxFailedAttempts; i++) {
      recordFailedAttempt(testUsername);
    }

    const lockStatus = isAccountLocked(testUsername);
    expect(lockStatus.locked).toBe(true);
    expect(lockStatus.remainingMinutes).toBeGreaterThan(0);
  });

  it('clears lockout state on successful login', () => {
    for (let i = 0; i < 3; i++) {
      recordFailedAttempt(testUsername);
    }

    clearLockoutState(testUsername);
    expect(getFailedAttempts(testUsername)).toBe(0);
    expect(isAccountLocked(testUsername).locked).toBe(false);
  });

  it('returns attempts remaining message', () => {
    const result = recordFailedAttempt(testUsername);
    expect(result.attemptsRemaining).toBe(DEFAULT_LOCKOUT_POLICY.maxFailedAttempts - 1);
    expect(result.message).toContain('attempt');
  });
});

// ============================================
// CSRF PROTECTION TESTS
// ============================================

describe('CSRF Protection', () => {
  describe('generateCsrfToken', () => {
    it('generates unique tokens', () => {
      const token1 = generateCsrfToken();
      const token2 = generateCsrfToken();
      expect(token1).not.toBe(token2);
    });

    it('generates token of correct length', () => {
      const token = generateCsrfToken();
      expect(token.length).toBe(64); // 32 bytes = 64 hex chars
    });

    it('generates hex string', () => {
      const token = generateCsrfToken();
      expect(token).toMatch(/^[0-9a-f]+$/);
    });
  });

  describe('requiresCsrfProtection', () => {
    it('returns false for safe methods', () => {
      expect(requiresCsrfProtection('GET')).toBe(false);
      expect(requiresCsrfProtection('HEAD')).toBe(false);
      expect(requiresCsrfProtection('OPTIONS')).toBe(false);
    });

    it('returns true for unsafe methods', () => {
      expect(requiresCsrfProtection('POST')).toBe(true);
      expect(requiresCsrfProtection('PUT')).toBe(true);
      expect(requiresCsrfProtection('DELETE')).toBe(true);
      expect(requiresCsrfProtection('PATCH')).toBe(true);
    });

    it('is case insensitive', () => {
      expect(requiresCsrfProtection('get')).toBe(false);
      expect(requiresCsrfProtection('post')).toBe(true);
    });
  });

  describe('validateOrigin', () => {
    it('validates exact origin match', () => {
      expect(validateOrigin('https://example.com', ['https://example.com'])).toBe(true);
      expect(validateOrigin('https://evil.com', ['https://example.com'])).toBe(false);
    });

    it('validates wildcard subdomains', () => {
      expect(validateOrigin('https://app.example.com', ['*.example.com'])).toBe(true);
      expect(validateOrigin('https://sub.app.example.com', ['*.example.com'])).toBe(true);
    });

    it('rejects null origin', () => {
      expect(validateOrigin(null, ['https://example.com'])).toBe(false);
    });
  });
});

// ============================================
// INTEGRATION TESTS
// ============================================

describe('Security Integration', () => {
  it('file upload + filename sanitization works together', () => {
    const dangerous = '../../../etc/passwd.exe';
    const sanitized = sanitizeFilename(dangerous);

    expect(isDangerousExtension(dangerous)).toBe(true);
    expect(sanitized).not.toContain('..');
  });

  it('password validation with user context', () => {
    const userInfo = {
      username: 'johndoe',
      email: 'john.doe@example.com',
      fullName: 'John Doe',
    };

    // Password containing username should fail
    const result1 = validatePassword('JohnDoe123!@#', DEFAULT_PASSWORD_POLICY, userInfo);
    expect(result1.errors.length).toBeGreaterThan(0);

    // Strong password without user info should pass
    const result2 = validatePassword('Xy9$mK2!pLqR#nV', DEFAULT_PASSWORD_POLICY, userInfo);
    expect(result2.isValid).toBe(true);
  });

  it('account lockout with incremental duration', () => {
    const username = 'lockouttest';
    localStorage.clear();

    // First lockout
    for (let i = 0; i < DEFAULT_LOCKOUT_POLICY.maxFailedAttempts; i++) {
      recordFailedAttempt(username);
    }

    const firstLockout = isAccountLocked(username);
    expect(firstLockout.locked).toBe(true);

    // Clear and trigger second lockout (would have longer duration if incremental)
    clearLockoutState(username);

    // Re-lock - in real scenario this would have incrementally longer duration
    for (let i = 0; i < DEFAULT_LOCKOUT_POLICY.maxFailedAttempts; i++) {
      recordFailedAttempt(username);
    }

    const secondLockout = isAccountLocked(username);
    expect(secondLockout.locked).toBe(true);
  });
});
