/**
 * Password Policy and Account Lockout Utilities
 * Enterprise-grade password requirements and brute-force protection
 */

// ============================================
// PASSWORD POLICY CONFIGURATION
// ============================================

export interface PasswordPolicy {
  minLength: number;
  maxLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
  specialChars: string;
  preventCommonPasswords: boolean;
  preventUserInfoInPassword: boolean;
  passwordHistoryCount: number; // How many old passwords to remember
  expirationDays: number; // 0 = never expires
}

export const DEFAULT_PASSWORD_POLICY: PasswordPolicy = {
  minLength: 12,
  maxLength: 128,
  requireUppercase: true,
  requireLowercase: true,
  requireNumbers: true,
  requireSpecialChars: true,
  specialChars: '!@#$%^&*()_+-=[]{}|;:,.<>?',
  preventCommonPasswords: true,
  preventUserInfoInPassword: true,
  passwordHistoryCount: 5,
  expirationDays: 90,
};

// Healthcare-specific stricter policy (HIPAA compliance)
export const HIPAA_PASSWORD_POLICY: PasswordPolicy = {
  minLength: 14,
  maxLength: 128,
  requireUppercase: true,
  requireLowercase: true,
  requireNumbers: true,
  requireSpecialChars: true,
  specialChars: '!@#$%^&*()_+-=[]{}|;:,.<>?',
  preventCommonPasswords: true,
  preventUserInfoInPassword: true,
  passwordHistoryCount: 10,
  expirationDays: 60,
};

// ============================================
// ACCOUNT LOCKOUT CONFIGURATION
// ============================================

export interface AccountLockoutPolicy {
  maxFailedAttempts: number;
  lockoutDurationMinutes: number;
  resetCounterAfterMinutes: number;
  incrementalLockout: boolean; // Each lockout increases duration
  maxLockoutDurationMinutes: number;
  notifyOnLockout: boolean;
}

export const DEFAULT_LOCKOUT_POLICY: AccountLockoutPolicy = {
  maxFailedAttempts: 5,
  lockoutDurationMinutes: 15,
  resetCounterAfterMinutes: 30,
  incrementalLockout: true,
  maxLockoutDurationMinutes: 60,
  notifyOnLockout: true,
};

// ============================================
// COMMON PASSWORDS LIST (subset)
// ============================================

const COMMON_PASSWORDS = new Set([
  'password', 'password1', 'password123', '123456', '12345678', '123456789',
  'qwerty', 'abc123', 'monkey', 'master', 'dragon', 'letmein', 'login',
  'admin', 'administrator', 'welcome', 'welcome1', 'passw0rd', 'p@ssword',
  'p@ssw0rd', 'sunshine', 'princess', 'football', 'baseball', 'iloveyou',
  'trustno1', 'superman', 'batman', 'shadow', 'michael', 'jennifer',
  'qwerty123', 'password!', '123qwe', 'zaq1zaq1', 'qazwsx', 'test123',
  'guest', 'guest123', 'changeme', 'changeme123', 'temp123', 'default',
]);

// ============================================
// PASSWORD VALIDATION
// ============================================

export interface PasswordValidationResult {
  isValid: boolean;
  score: number; // 0-100 strength score
  errors: string[];
  suggestions: string[];
}

/**
 * Calculate password entropy (bits)
 */
const calculateEntropy = (password: string): number => {
  let charsetSize = 0;

  if (/[a-z]/.test(password)) charsetSize += 26;
  if (/[A-Z]/.test(password)) charsetSize += 26;
  if (/[0-9]/.test(password)) charsetSize += 10;
  if (/[^a-zA-Z0-9]/.test(password)) charsetSize += 32;

  if (charsetSize === 0) return 0;

  return Math.log2(Math.pow(charsetSize, password.length));
};

/**
 * Check for sequential characters (abc, 123)
 */
const hasSequentialChars = (password: string, length: number = 3): boolean => {
  const lower = password.toLowerCase();

  for (let i = 0; i <= lower.length - length; i++) {
    let isSequential = true;
    for (let j = 1; j < length; j++) {
      if (lower.charCodeAt(i + j) !== lower.charCodeAt(i + j - 1) + 1) {
        isSequential = false;
        break;
      }
    }
    if (isSequential) return true;
  }

  return false;
};

/**
 * Check for repeated characters (aaa, 111)
 */
const hasRepeatedChars = (password: string, count: number = 3): boolean => {
  for (let i = 0; i <= password.length - count; i++) {
    if (password.substring(i, i + count) === password[i].repeat(count)) {
      return true;
    }
  }
  return false;
};

/**
 * Check for keyboard patterns (qwerty, asdf)
 */
const hasKeyboardPattern = (password: string): boolean => {
  const patterns = [
    'qwerty', 'asdfgh', 'zxcvbn', 'qwertyuiop', 'asdfghjkl', 'zxcvbnm',
    '1234567890', '0987654321', 'qazwsx', 'wsxedc', 'edcrfv',
  ];

  const lower = password.toLowerCase();
  return patterns.some((pattern) => lower.includes(pattern));
};

/**
 * Validate password against policy
 */
export const validatePassword = (
  password: string,
  policy: PasswordPolicy = DEFAULT_PASSWORD_POLICY,
  userInfo?: { username?: string; email?: string; fullName?: string }
): PasswordValidationResult => {
  const errors: string[] = [];
  const suggestions: string[] = [];

  // Length checks
  if (password.length < policy.minLength) {
    errors.push(`Password must be at least ${policy.minLength} characters`);
  }

  if (password.length > policy.maxLength) {
    errors.push(`Password cannot exceed ${policy.maxLength} characters`);
  }

  // Character requirements
  if (policy.requireUppercase && !/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase letter');
  }

  if (policy.requireLowercase && !/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase letter');
  }

  if (policy.requireNumbers && !/[0-9]/.test(password)) {
    errors.push('Password must contain at least one number');
  }

  if (policy.requireSpecialChars) {
    const hasSpecial = [...policy.specialChars].some((char) =>
      password.includes(char)
    );
    if (!hasSpecial) {
      errors.push(`Password must contain at least one special character (${policy.specialChars})`);
    }
  }

  // Common password check
  if (policy.preventCommonPasswords) {
    const lowerPassword = password.toLowerCase();
    if (COMMON_PASSWORDS.has(lowerPassword)) {
      errors.push('This password is too common. Please choose a more unique password.');
    }
  }

  // User info in password check
  if (policy.preventUserInfoInPassword && userInfo) {
    const lowerPassword = password.toLowerCase();

    if (userInfo.username && lowerPassword.includes(userInfo.username.toLowerCase())) {
      errors.push('Password cannot contain your username');
    }

    if (userInfo.email) {
      const emailLocal = userInfo.email.split('@')[0].toLowerCase();
      if (emailLocal.length > 3 && lowerPassword.includes(emailLocal)) {
        errors.push('Password cannot contain parts of your email');
      }
    }

    if (userInfo.fullName) {
      const nameParts = userInfo.fullName.toLowerCase().split(/\s+/);
      for (const part of nameParts) {
        if (part.length > 3 && lowerPassword.includes(part)) {
          errors.push('Password cannot contain your name');
          break;
        }
      }
    }
  }

  // Pattern checks with suggestions
  if (hasSequentialChars(password)) {
    suggestions.push('Avoid sequential characters (abc, 123)');
  }

  if (hasRepeatedChars(password)) {
    suggestions.push('Avoid repeated characters (aaa, 111)');
  }

  if (hasKeyboardPattern(password)) {
    suggestions.push('Avoid keyboard patterns (qwerty, asdf)');
  }

  // Calculate strength score
  const entropy = calculateEntropy(password);
  let score = Math.min(100, Math.round((entropy / 60) * 100));

  // Reduce score for patterns
  if (hasSequentialChars(password)) score -= 10;
  if (hasRepeatedChars(password)) score -= 10;
  if (hasKeyboardPattern(password)) score -= 15;
  if (COMMON_PASSWORDS.has(password.toLowerCase())) score = 0;

  score = Math.max(0, score);

  // Add strength-based suggestions
  if (score < 50 && errors.length === 0) {
    suggestions.push('Consider using a longer password for better security');
  }

  if (score < 30 && errors.length === 0) {
    suggestions.push('Try mixing more character types (uppercase, lowercase, numbers, symbols)');
  }

  return {
    isValid: errors.length === 0,
    score,
    errors,
    suggestions,
  };
};

/**
 * Get password strength label
 */
export const getPasswordStrengthLabel = (
  score: number
): { label: string; color: string } => {
  if (score < 20) return { label: 'Very Weak', color: 'red' };
  if (score < 40) return { label: 'Weak', color: 'orange' };
  if (score < 60) return { label: 'Fair', color: 'yellow' };
  if (score < 80) return { label: 'Strong', color: 'blue' };
  return { label: 'Very Strong', color: 'green' };
};

// ============================================
// ACCOUNT LOCKOUT
// ============================================

interface LockoutState {
  failedAttempts: number;
  lockoutCount: number;
  lockedUntil: number | null;
  lastFailedAttempt: number | null;
}

const LOCKOUT_STORAGE_KEY = 'xenon_lockout_state';

/**
 * Get current lockout state for a user
 */
export const getLockoutState = (username: string): LockoutState => {
  try {
    const statesStr = localStorage.getItem(LOCKOUT_STORAGE_KEY);
    if (statesStr) {
      const states = JSON.parse(statesStr);
      return states[username] || createInitialLockoutState();
    }
  } catch {
    // Ignore parse errors
  }
  return createInitialLockoutState();
};

const createInitialLockoutState = (): LockoutState => ({
  failedAttempts: 0,
  lockoutCount: 0,
  lockedUntil: null,
  lastFailedAttempt: null,
});

/**
 * Save lockout state
 */
const saveLockoutState = (username: string, state: LockoutState): void => {
  try {
    const statesStr = localStorage.getItem(LOCKOUT_STORAGE_KEY);
    const states = statesStr ? JSON.parse(statesStr) : {};
    states[username] = state;
    localStorage.setItem(LOCKOUT_STORAGE_KEY, JSON.stringify(states));
  } catch {
    // Ignore storage errors
  }
};

/**
 * Check if account is currently locked
 */
export const isAccountLocked = (username: string): {
  locked: boolean;
  remainingMinutes: number;
  message: string;
} => {
  const state = getLockoutState(username);

  if (!state.lockedUntil) {
    return { locked: false, remainingMinutes: 0, message: '' };
  }

  const now = Date.now();
  if (now >= state.lockedUntil) {
    // Lockout expired, clear it
    saveLockoutState(username, {
      ...state,
      lockedUntil: null,
      failedAttempts: 0,
    });
    return { locked: false, remainingMinutes: 0, message: '' };
  }

  const remainingMs = state.lockedUntil - now;
  const remainingMinutes = Math.ceil(remainingMs / 60000);

  return {
    locked: true,
    remainingMinutes,
    message: `Account is locked. Try again in ${remainingMinutes} minute${remainingMinutes !== 1 ? 's' : ''}.`,
  };
};

/**
 * Record a failed login attempt
 */
export const recordFailedAttempt = (
  username: string,
  policy: AccountLockoutPolicy = DEFAULT_LOCKOUT_POLICY
): {
  locked: boolean;
  attemptsRemaining: number;
  lockoutDuration: number;
  message: string;
} => {
  let state = getLockoutState(username);
  const now = Date.now();

  // Reset counter if enough time has passed since last failure
  if (
    state.lastFailedAttempt &&
    now - state.lastFailedAttempt > policy.resetCounterAfterMinutes * 60000
  ) {
    state = createInitialLockoutState();
  }

  // Increment failed attempts
  state.failedAttempts += 1;
  state.lastFailedAttempt = now;

  // Check if we should lock
  if (state.failedAttempts >= policy.maxFailedAttempts) {
    // Calculate lockout duration
    let lockoutMinutes = policy.lockoutDurationMinutes;

    if (policy.incrementalLockout) {
      lockoutMinutes *= Math.pow(2, state.lockoutCount);
      lockoutMinutes = Math.min(lockoutMinutes, policy.maxLockoutDurationMinutes);
    }

    state.lockedUntil = now + lockoutMinutes * 60000;
    state.lockoutCount += 1;

    saveLockoutState(username, state);

    return {
      locked: true,
      attemptsRemaining: 0,
      lockoutDuration: lockoutMinutes,
      message: `Too many failed attempts. Account locked for ${lockoutMinutes} minutes.`,
    };
  }

  saveLockoutState(username, state);

  const attemptsRemaining = policy.maxFailedAttempts - state.failedAttempts;

  return {
    locked: false,
    attemptsRemaining,
    lockoutDuration: 0,
    message: `Invalid credentials. ${attemptsRemaining} attempt${attemptsRemaining !== 1 ? 's' : ''} remaining.`,
  };
};

/**
 * Clear lockout state on successful login
 */
export const clearLockoutState = (username: string): void => {
  saveLockoutState(username, createInitialLockoutState());
};

/**
 * Get failed attempts count
 */
export const getFailedAttempts = (
  username: string,
  policy: AccountLockoutPolicy = DEFAULT_LOCKOUT_POLICY
): number => {
  const state = getLockoutState(username);
  const now = Date.now();

  // Reset if enough time has passed
  if (
    state.lastFailedAttempt &&
    now - state.lastFailedAttempt > policy.resetCounterAfterMinutes * 60000
  ) {
    return 0;
  }

  return state.failedAttempts;
};

// ============================================
// PASSWORD EXPIRATION
// ============================================

/**
 * Check if password has expired
 */
export const isPasswordExpired = (
  lastPasswordChange: Date | string,
  policy: PasswordPolicy = DEFAULT_PASSWORD_POLICY
): boolean => {
  if (policy.expirationDays === 0) return false;

  const lastChange = new Date(lastPasswordChange);
  const expirationDate = new Date(lastChange);
  expirationDate.setDate(expirationDate.getDate() + policy.expirationDays);

  return new Date() > expirationDate;
};

/**
 * Get days until password expires
 */
export const getDaysUntilExpiration = (
  lastPasswordChange: Date | string,
  policy: PasswordPolicy = DEFAULT_PASSWORD_POLICY
): number => {
  if (policy.expirationDays === 0) return Infinity;

  const lastChange = new Date(lastPasswordChange);
  const expirationDate = new Date(lastChange);
  expirationDate.setDate(expirationDate.getDate() + policy.expirationDays);

  const diffMs = expirationDate.getTime() - Date.now();
  return Math.ceil(diffMs / (1000 * 60 * 60 * 24));
};
