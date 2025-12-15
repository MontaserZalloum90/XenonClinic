/**
 * Data Encryption Utilities for Patient Health Information
 * Uses Web Crypto API for client-side encryption of sensitive data
 *
 * Note: This provides client-side encryption for defense in depth.
 * Server-side encryption at rest should also be implemented in the backend.
 */

// ============================================
// ENCRYPTION CONFIGURATION
// ============================================

const ALGORITHM = 'AES-GCM';
const KEY_LENGTH = 256;
const IV_LENGTH = 12; // 96 bits for GCM
const SALT_LENGTH = 16;
const ITERATIONS = 100000;

// Fields that should be encrypted
export const SENSITIVE_FIELDS = [
  'emiratesId',
  'ssn',
  'medicalHistory',
  'diagnosis',
  'treatmentNotes',
  'medications',
  'allergies',
  'labResults',
  'geneticInfo',
  'mentalHealthNotes',
  'substanceAbuseInfo',
  'hivStatus',
  'sexualHealth',
  'bankAccount',
  'insuranceNumber',
] as const;

export type SensitiveField = (typeof SENSITIVE_FIELDS)[number];

// ============================================
// UTILITY FUNCTIONS
// ============================================

/**
 * Convert ArrayBuffer to Base64 string
 */
const arrayBufferToBase64 = (buffer: ArrayBuffer): string => {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
};

/**
 * Convert Base64 string to ArrayBuffer
 */
const base64ToArrayBuffer = (base64: string): ArrayBuffer => {
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes.buffer;
};

/**
 * Generate a random initialization vector
 */
const generateIV = (): Uint8Array => {
  return crypto.getRandomValues(new Uint8Array(IV_LENGTH));
};

/**
 * Generate a random salt
 */
const generateSalt = (): Uint8Array => {
  return crypto.getRandomValues(new Uint8Array(SALT_LENGTH));
};

// ============================================
// KEY DERIVATION
// ============================================

/**
 * Derive an encryption key from a password using PBKDF2
 */
const deriveKey = async (
  password: string,
  salt: Uint8Array
): Promise<CryptoKey> => {
  const encoder = new TextEncoder();
  const passwordBuffer = encoder.encode(password);

  // Import password as key material
  const keyMaterial = await crypto.subtle.importKey(
    'raw',
    passwordBuffer,
    'PBKDF2',
    false,
    ['deriveBits', 'deriveKey']
  );

  // Derive the actual encryption key
  return crypto.subtle.deriveKey(
    {
      name: 'PBKDF2',
      salt: salt,
      iterations: ITERATIONS,
      hash: 'SHA-256',
    },
    keyMaterial,
    { name: ALGORITHM, length: KEY_LENGTH },
    false,
    ['encrypt', 'decrypt']
  );
};

/**
 * Get or generate the encryption key
 * In production, this should be derived from a secure source
 */
const getEncryptionKey = async (): Promise<{ key: CryptoKey; salt: Uint8Array }> => {
  // Check for existing key material in session
  const saltBase64 = sessionStorage.getItem('encryption_salt');
  let salt: Uint8Array;

  if (saltBase64) {
    salt = new Uint8Array(base64ToArrayBuffer(saltBase64));
  } else {
    salt = generateSalt();
    sessionStorage.setItem('encryption_salt', arrayBufferToBase64(salt));
  }

  // In production, this passphrase should come from:
  // 1. User's password (for user-specific encryption)
  // 2. Server-provided session key
  // 3. Hardware security module (HSM)
  // For demo purposes, we use a combination of session token and fixed phrase
  const token = localStorage.getItem('token') || '';
  const passphrase = `xenon_clinic_${token.substring(0, 32)}_healthcare_data`;

  const key = await deriveKey(passphrase, salt);
  return { key, salt };
};

// ============================================
// ENCRYPTION FUNCTIONS
// ============================================

export interface EncryptedData {
  ciphertext: string;
  iv: string;
  salt: string;
  algorithm: string;
  version: number;
}

/**
 * Encrypt a string value
 */
export const encryptString = async (plaintext: string): Promise<EncryptedData> => {
  if (!plaintext) {
    throw new Error('Cannot encrypt empty value');
  }

  const { key, salt } = await getEncryptionKey();
  const iv = generateIV();
  const encoder = new TextEncoder();
  const data = encoder.encode(plaintext);

  const ciphertext = await crypto.subtle.encrypt(
    { name: ALGORITHM, iv },
    key,
    data
  );

  return {
    ciphertext: arrayBufferToBase64(ciphertext),
    iv: arrayBufferToBase64(iv),
    salt: arrayBufferToBase64(salt),
    algorithm: ALGORITHM,
    version: 1,
  };
};

/**
 * Decrypt an encrypted value
 */
export const decryptString = async (encrypted: EncryptedData): Promise<string> => {
  if (!encrypted || !encrypted.ciphertext) {
    throw new Error('Invalid encrypted data');
  }

  const salt = new Uint8Array(base64ToArrayBuffer(encrypted.salt));
  const iv = new Uint8Array(base64ToArrayBuffer(encrypted.iv));
  const ciphertext = base64ToArrayBuffer(encrypted.ciphertext);

  // Get encryption key with the stored salt
  const token = localStorage.getItem('token') || '';
  const passphrase = `xenon_clinic_${token.substring(0, 32)}_healthcare_data`;
  const key = await deriveKey(passphrase, salt);

  const decrypted = await crypto.subtle.decrypt(
    { name: ALGORITHM, iv },
    key,
    ciphertext
  );

  const decoder = new TextDecoder();
  return decoder.decode(decrypted);
};

/**
 * Check if a value is encrypted
 */
export const isEncrypted = (value: unknown): value is EncryptedData => {
  if (!value || typeof value !== 'object') return false;
  const obj = value as Record<string, unknown>;
  return (
    typeof obj.ciphertext === 'string' &&
    typeof obj.iv === 'string' &&
    typeof obj.salt === 'string' &&
    typeof obj.algorithm === 'string'
  );
};

// ============================================
// OBJECT ENCRYPTION
// ============================================

/**
 * Encrypt sensitive fields in an object
 */
export const encryptSensitiveFields = async <T extends Record<string, unknown>>(
  data: T,
  fieldsToEncrypt: readonly string[] = SENSITIVE_FIELDS
): Promise<T> => {
  const result = { ...data };

  for (const field of fieldsToEncrypt) {
    if (field in result && result[field] != null) {
      const value = result[field];
      if (typeof value === 'string' && value.length > 0) {
        (result as Record<string, unknown>)[field] = await encryptString(value);
      } else if (typeof value === 'object' && !isEncrypted(value)) {
        // Encrypt object by stringifying
        (result as Record<string, unknown>)[field] = await encryptString(
          JSON.stringify(value)
        );
      }
    }
  }

  return result;
};

/**
 * Decrypt sensitive fields in an object
 */
export const decryptSensitiveFields = async <T extends Record<string, unknown>>(
  data: T,
  fieldsToDecrypt: readonly string[] = SENSITIVE_FIELDS
): Promise<T> => {
  const result = { ...data };

  for (const field of fieldsToDecrypt) {
    if (field in result && isEncrypted(result[field])) {
      try {
        const decrypted = await decryptString(result[field] as EncryptedData);
        // Try to parse as JSON, otherwise use as string
        try {
          (result as Record<string, unknown>)[field] = JSON.parse(decrypted);
        } catch {
          (result as Record<string, unknown>)[field] = decrypted;
        }
      } catch (error) {
        console.error(`Failed to decrypt field ${field}:`, error);
        // Leave as encrypted if decryption fails
      }
    }
  }

  return result;
};

// ============================================
// DATA MASKING (for display)
// ============================================

/**
 * Mask sensitive data for display
 */
export const maskSensitiveData = (
  value: string,
  options: {
    visibleStart?: number;
    visibleEnd?: number;
    maskChar?: string;
  } = {}
): string => {
  // Handle empty or null values
  if (!value) return '';

  const { visibleStart = 0, visibleEnd = 4, maskChar = '*' } = options;

  if (value.length <= visibleStart + visibleEnd) {
    return maskChar.repeat(value.length);
  }

  const start = value.substring(0, visibleStart);
  const end = value.substring(value.length - visibleEnd);
  const masked = maskChar.repeat(value.length - visibleStart - visibleEnd);

  return `${start}${masked}${end}`;
};

/**
 * Mask Emirates ID (show last 4 digits)
 */
export const maskEmiratesId = (emiratesId: string): string => {
  if (!emiratesId) return '';
  // Format: 784-YYYY-NNNNNNN-C
  return maskSensitiveData(emiratesId.replace(/-/g, ''), {
    visibleStart: 0,
    visibleEnd: 4,
  }).replace(/(.{3})(.{4})(.{7})(.)/, '$1-$2-$3-$4');
};

/**
 * Mask phone number (show last 4 digits)
 */
export const maskPhoneNumber = (phone: string): string => {
  if (!phone) return '';
  const digitsOnly = phone.replace(/\D/g, '');
  return maskSensitiveData(digitsOnly, { visibleStart: 0, visibleEnd: 4 });
};

/**
 * Mask email (show first 2 chars and domain)
 */
export const maskEmail = (email: string): string => {
  if (!email) return '';
  const [local, domain] = email.split('@');
  if (!domain) return maskSensitiveData(email);
  const maskedLocal =
    local.length <= 2 ? maskSensitiveData(local) : `${local.substring(0, 2)}${maskSensitiveData(local.substring(2))}`;
  return `${maskedLocal}@${domain}`;
};

// ============================================
// SECURE DATA HANDLING
// ============================================

/**
 * Securely clear sensitive data from memory
 * Note: JavaScript doesn't guarantee memory clearing, but this helps
 */
export const secureClear = (data: Record<string, unknown>, fields: string[]): void => {
  for (const field of fields) {
    if (field in data) {
      const value = data[field];
      if (typeof value === 'string') {
        // Overwrite string content
        (data as Record<string, unknown>)[field] = '\0'.repeat(value.length);
      }
      delete data[field];
    }
  }
};

/**
 * Create a secure wrapper that auto-clears after use
 */
export const withSecureData = async <T, R>(
  data: T & Record<string, unknown>,
  sensitiveFields: string[],
  operation: (data: T) => Promise<R>
): Promise<R> => {
  try {
    return await operation(data);
  } finally {
    secureClear(data, sensitiveFields);
  }
};

// ============================================
// HASH FUNCTIONS (for checksums, not passwords)
// ============================================

/**
 * Generate SHA-256 hash of data
 */
export const hashData = async (data: string): Promise<string> => {
  const encoder = new TextEncoder();
  const dataBuffer = encoder.encode(data);
  const hashBuffer = await crypto.subtle.digest('SHA-256', dataBuffer);
  return arrayBufferToBase64(hashBuffer);
};

/**
 * Verify data against a hash
 */
export const verifyHash = async (data: string, expectedHash: string): Promise<boolean> => {
  const actualHash = await hashData(data);
  return actualHash === expectedHash;
};
