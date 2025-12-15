/**
 * Input Validation Utilities for Healthcare Data
 * Ensures patient data is properly sanitized and validated
 */

// ============================================
// EMIRATES ID VALIDATION
// ============================================

/**
 * UAE Emirates ID format: 784-YYYY-NNNNNNN-C
 * - 784: Country code for UAE
 * - YYYY: Year of birth
 * - NNNNNNN: 7-digit serial number
 * - C: Check digit
 */
export const EMIRATES_ID_REGEX = /^784-\d{4}-\d{7}-\d$/;

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
  sanitizedValue?: string;
}

/**
 * Validates and sanitizes UAE Emirates ID
 */
export const validateEmiratesId = (value: string): ValidationResult => {
  const errors: string[] = [];

  if (!value || typeof value !== 'string') {
    return { isValid: false, errors: ['Emirates ID is required'] };
  }

  // Remove any whitespace and normalize dashes
  const sanitized = value.trim().replace(/\s+/g, '').replace(/[-–—]/g, '-');

  // Check format
  if (!EMIRATES_ID_REGEX.test(sanitized)) {
    errors.push('Emirates ID must be in format: 784-YYYY-NNNNNNN-N');
  }

  // Extract year and validate it's reasonable (1900-current year)
  const yearMatch = sanitized.match(/^784-(\d{4})/);
  if (yearMatch) {
    const year = parseInt(yearMatch[1], 10);
    const currentYear = new Date().getFullYear();
    if (year < 1900 || year > currentYear) {
      errors.push(`Birth year must be between 1900 and ${currentYear}`);
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
    sanitizedValue: sanitized,
  };
};

// ============================================
// PATIENT DATA VALIDATION
// ============================================

export interface PatientValidationRules {
  fullNameEn: { required: boolean; minLength: number; maxLength: number };
  fullNameAr: { required: boolean; minLength: number; maxLength: number };
  dateOfBirth: { required: boolean; minAge: number; maxAge: number };
  phoneNumber: { required: boolean; pattern: RegExp };
  email: { required: boolean; pattern: RegExp };
}

export const DEFAULT_PATIENT_RULES: PatientValidationRules = {
  fullNameEn: { required: true, minLength: 2, maxLength: 100 },
  fullNameAr: { required: false, minLength: 2, maxLength: 100 },
  dateOfBirth: { required: true, minAge: 0, maxAge: 150 },
  phoneNumber: { required: false, pattern: /^\+?[\d\s-]{7,20}$/ },
  email: { required: false, pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/ },
};

/**
 * Sanitizes text input by removing potentially dangerous characters
 */
export const sanitizeText = (value: string, options?: {
  allowUnicode?: boolean;
  maxLength?: number;
  trim?: boolean;
}): string => {
  if (!value || typeof value !== 'string') return '';

  const { maxLength = 1000, trim = true } = options || {};

  let sanitized = value;

  // Trim whitespace
  if (trim) {
    sanitized = sanitized.trim();
  }

  // Remove null bytes
  sanitized = sanitized.replace(/\0/g, '');

  // Remove control characters (except newlines and tabs if needed)
  // eslint-disable-next-line no-control-regex
  sanitized = sanitized.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]/g, '');

  // Remove potential SQL injection patterns BEFORE escaping
  // This prevents dangerous SQL constructs while preserving legitimate characters
  sanitized = sanitized.replace(/(--|\/\*|\*\/|\|\|)/g, '');

  // Escape HTML entities to prevent XSS
  sanitized = sanitized
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;');

  // Truncate to max length
  if (sanitized.length > maxLength) {
    sanitized = sanitized.substring(0, maxLength);
  }

  return sanitized;
};

/**
 * Validates a full name (English)
 */
export const validateNameEn = (
  value: string,
  rules = DEFAULT_PATIENT_RULES.fullNameEn
): ValidationResult => {
  const errors: string[] = [];
  const sanitized = sanitizeText(value, { allowUnicode: false, maxLength: rules.maxLength });

  if (rules.required && !sanitized) {
    errors.push('Name is required');
  }

  if (sanitized && sanitized.length < rules.minLength) {
    errors.push(`Name must be at least ${rules.minLength} characters`);
  }

  // Check for valid characters (letters, spaces, hyphens, apostrophes)
  if (sanitized && !/^[a-zA-Z\s\-']+$/.test(sanitized.replace(/&[^;]+;/g, ''))) {
    errors.push('Name can only contain letters, spaces, hyphens, and apostrophes');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};

/**
 * Validates a full name (Arabic)
 */
export const validateNameAr = (
  value: string,
  rules = DEFAULT_PATIENT_RULES.fullNameAr
): ValidationResult => {
  const errors: string[] = [];
  const sanitized = sanitizeText(value, { allowUnicode: true, maxLength: rules.maxLength });

  if (rules.required && !sanitized) {
    errors.push('Arabic name is required');
  }

  if (sanitized && sanitized.length < rules.minLength) {
    errors.push(`Arabic name must be at least ${rules.minLength} characters`);
  }

  // Check for valid Arabic characters
  if (sanitized && !/^[\u0600-\u06FF\s-]+$/.test(sanitized.replace(/&[^;]+;/g, ''))) {
    errors.push('Arabic name can only contain Arabic characters, spaces, and hyphens');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};

/**
 * Validates date of birth
 */
export const validateDateOfBirth = (
  value: string | Date,
  rules = DEFAULT_PATIENT_RULES.dateOfBirth
): ValidationResult => {
  const errors: string[] = [];

  if (rules.required && !value) {
    return { isValid: false, errors: ['Date of birth is required'] };
  }

  if (!value) {
    return { isValid: true, errors: [] };
  }

  const date = typeof value === 'string' ? new Date(value) : value;

  if (isNaN(date.getTime())) {
    errors.push('Invalid date format');
    return { isValid: false, errors };
  }

  const today = new Date();
  const age = Math.floor((today.getTime() - date.getTime()) / (365.25 * 24 * 60 * 60 * 1000));

  if (date > today) {
    errors.push('Date of birth cannot be in the future');
  }

  if (age < rules.minAge) {
    errors.push(`Patient must be at least ${rules.minAge} years old`);
  }

  if (age > rules.maxAge) {
    errors.push(`Age cannot exceed ${rules.maxAge} years`);
  }

  return {
    isValid: errors.length === 0,
    errors,
    sanitizedValue: date.toISOString().split('T')[0],
  };
};

/**
 * Validates phone number
 */
export const validatePhoneNumber = (
  value: string,
  rules = DEFAULT_PATIENT_RULES.phoneNumber
): ValidationResult => {
  const errors: string[] = [];

  if (!value) {
    if (rules.required) {
      errors.push('Phone number is required');
    }
    return { isValid: !rules.required, errors };
  }

  // Sanitize: keep only digits, +, spaces, and hyphens
  const sanitized = value.replace(/[^\d+\s-]/g, '').trim();

  if (!rules.pattern.test(sanitized)) {
    errors.push('Invalid phone number format');
  }

  // Check for reasonable length
  const digitsOnly = sanitized.replace(/\D/g, '');
  if (digitsOnly.length < 7) {
    errors.push('Phone number must have at least 7 digits');
  }
  if (digitsOnly.length > 15) {
    errors.push('Phone number cannot exceed 15 digits');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};

/**
 * Validates email address
 */
export const validateEmail = (
  value: string,
  rules = DEFAULT_PATIENT_RULES.email
): ValidationResult => {
  const errors: string[] = [];

  if (!value) {
    if (rules.required) {
      errors.push('Email is required');
    }
    return { isValid: !rules.required, errors };
  }

  const sanitized = value.trim().toLowerCase();

  if (!rules.pattern.test(sanitized)) {
    errors.push('Invalid email address format');
  }

  if (sanitized.length > 254) {
    errors.push('Email address is too long');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};

/**
 * Validates gender field
 */
export const validateGender = (value: string): ValidationResult => {
  const validValues = ['M', 'F', 'male', 'female', 'Male', 'Female'];
  const sanitized = value?.trim();

  if (!sanitized) {
    return { isValid: false, errors: ['Gender is required'] };
  }

  if (!validValues.includes(sanitized)) {
    return { isValid: false, errors: ['Gender must be M or F'] };
  }

  // Normalize to M/F
  const normalized = sanitized.toLowerCase().startsWith('m') ? 'M' : 'F';

  return { isValid: true, errors: [], sanitizedValue: normalized };
};

// ============================================
// COMPLETE PATIENT VALIDATION
// ============================================

export interface PatientData {
  emiratesId: string;
  fullNameEn: string;
  fullNameAr?: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber?: string;
  email?: string;
  notes?: string;
}

export interface PatientValidationResult {
  isValid: boolean;
  errors: Record<string, string[]>;
  sanitizedData: Partial<PatientData>;
}

/**
 * Validates complete patient data object
 */
export const validatePatientData = (
  data: Partial<PatientData>,
  rules = DEFAULT_PATIENT_RULES
): PatientValidationResult => {
  const errors: Record<string, string[]> = {};
  const sanitizedData: Partial<PatientData> = {};

  // Emirates ID
  const emiratesIdResult = validateEmiratesId(data.emiratesId || '');
  if (!emiratesIdResult.isValid) {
    errors.emiratesId = emiratesIdResult.errors;
  }
  sanitizedData.emiratesId = emiratesIdResult.sanitizedValue;

  // Full Name (English)
  const nameEnResult = validateNameEn(data.fullNameEn || '', rules.fullNameEn);
  if (!nameEnResult.isValid) {
    errors.fullNameEn = nameEnResult.errors;
  }
  sanitizedData.fullNameEn = nameEnResult.sanitizedValue;

  // Full Name (Arabic)
  if (data.fullNameAr) {
    const nameArResult = validateNameAr(data.fullNameAr, rules.fullNameAr);
    if (!nameArResult.isValid) {
      errors.fullNameAr = nameArResult.errors;
    }
    sanitizedData.fullNameAr = nameArResult.sanitizedValue;
  }

  // Date of Birth
  const dobResult = validateDateOfBirth(data.dateOfBirth || '', rules.dateOfBirth);
  if (!dobResult.isValid) {
    errors.dateOfBirth = dobResult.errors;
  }
  sanitizedData.dateOfBirth = dobResult.sanitizedValue;

  // Gender
  const genderResult = validateGender(data.gender || '');
  if (!genderResult.isValid) {
    errors.gender = genderResult.errors;
  }
  sanitizedData.gender = genderResult.sanitizedValue;

  // Phone Number
  if (data.phoneNumber) {
    const phoneResult = validatePhoneNumber(data.phoneNumber, rules.phoneNumber);
    if (!phoneResult.isValid) {
      errors.phoneNumber = phoneResult.errors;
    }
    sanitizedData.phoneNumber = phoneResult.sanitizedValue;
  }

  // Email
  if (data.email) {
    const emailResult = validateEmail(data.email, rules.email);
    if (!emailResult.isValid) {
      errors.email = emailResult.errors;
    }
    sanitizedData.email = emailResult.sanitizedValue;
  }

  // Notes
  if (data.notes) {
    sanitizedData.notes = sanitizeText(data.notes, { maxLength: 5000 });
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
    sanitizedData,
  };
};

// ============================================
// MEDICAL DATA VALIDATION
// ============================================

/**
 * Validates ICD-10 diagnosis code format
 */
export const validateICD10Code = (code: string): ValidationResult => {
  const errors: string[] = [];
  const sanitized = code?.trim().toUpperCase();

  if (!sanitized) {
    return { isValid: false, errors: ['ICD-10 code is required'] };
  }

  // ICD-10 format: Letter followed by 2 digits, optionally followed by decimal and more digits
  // Examples: H90, H90.3, H91.23
  const icd10Regex = /^[A-Z]\d{2}(\.\d{1,4})?$/;

  if (!icd10Regex.test(sanitized)) {
    errors.push('Invalid ICD-10 code format (e.g., H90.3)');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};

/**
 * Validates hearing threshold value
 */
export const validateHearingThreshold = (value: number): ValidationResult => {
  const errors: string[] = [];

  if (typeof value !== 'number' || isNaN(value)) {
    errors.push('Threshold must be a number');
    return { isValid: false, errors };
  }

  if (value < -10 || value > 120) {
    errors.push('Threshold must be between -10 and 120 dB');
  }

  // Thresholds are typically in 5 dB steps
  if (value % 5 !== 0) {
    errors.push('Threshold should be in 5 dB increments');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: String(value) };
};

/**
 * Validates serial number format
 */
export const validateSerialNumber = (value: string): ValidationResult => {
  const errors: string[] = [];
  const sanitized = value?.trim().toUpperCase().replace(/[^A-Z0-9-]/g, '');

  if (!sanitized) {
    return { isValid: false, errors: ['Serial number is required'] };
  }

  if (sanitized.length < 5) {
    errors.push('Serial number must be at least 5 characters');
  }

  if (sanitized.length > 30) {
    errors.push('Serial number cannot exceed 30 characters');
  }

  return { isValid: errors.length === 0, errors, sanitizedValue: sanitized };
};
