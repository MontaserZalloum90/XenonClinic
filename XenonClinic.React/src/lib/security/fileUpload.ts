/**
 * File Upload Security Utilities
 * Provides hardened file upload with size limits, extension validation, and path traversal protection
 */

// ============================================
// CONFIGURATION
// ============================================

export interface FileUploadConfig {
  maxSizeBytes: number;
  allowedExtensions: string[];
  allowedMimeTypes: string[];
  maxFilenameLength: number;
}

// Default configurations for different file types
export const FILE_UPLOAD_CONFIGS: Record<string, FileUploadConfig> = {
  // Profile photos, logos
  image: {
    maxSizeBytes: 5 * 1024 * 1024, // 5MB
    allowedExtensions: ['.jpg', '.jpeg', '.png', '.gif', '.webp'],
    allowedMimeTypes: ['image/jpeg', 'image/png', 'image/gif', 'image/webp'],
    maxFilenameLength: 255,
  },

  // Medical documents, reports
  document: {
    maxSizeBytes: 25 * 1024 * 1024, // 25MB
    allowedExtensions: ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt', '.rtf'],
    allowedMimeTypes: [
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'text/plain',
      'application/rtf',
    ],
    maxFilenameLength: 255,
  },

  // Medical images (X-rays, audiograms)
  medicalImage: {
    maxSizeBytes: 50 * 1024 * 1024, // 50MB
    allowedExtensions: ['.jpg', '.jpeg', '.png', '.dcm', '.dicom', '.tiff', '.tif'],
    allowedMimeTypes: [
      'image/jpeg',
      'image/png',
      'application/dicom',
      'image/tiff',
    ],
    maxFilenameLength: 255,
  },

  // Audio files (hearing tests)
  audio: {
    maxSizeBytes: 20 * 1024 * 1024, // 20MB
    allowedExtensions: ['.mp3', '.wav', '.ogg', '.m4a'],
    allowedMimeTypes: ['audio/mpeg', 'audio/wav', 'audio/ogg', 'audio/mp4'],
    maxFilenameLength: 255,
  },

  // Signature captures
  signature: {
    maxSizeBytes: 500 * 1024, // 500KB
    allowedExtensions: ['.png', '.svg'],
    allowedMimeTypes: ['image/png', 'image/svg+xml'],
    maxFilenameLength: 100,
  },

  // General attachments
  attachment: {
    maxSizeBytes: 10 * 1024 * 1024, // 10MB
    allowedExtensions: ['.pdf', '.jpg', '.jpeg', '.png', '.doc', '.docx', '.xls', '.xlsx'],
    allowedMimeTypes: [
      'application/pdf',
      'image/jpeg',
      'image/png',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    ],
    maxFilenameLength: 255,
  },
};

// Dangerous extensions that should never be allowed
const DANGEROUS_EXTENSIONS = [
  '.exe', '.bat', '.cmd', '.com', '.msi', '.scr',
  '.js', '.vbs', '.vbe', '.jse', '.ws', '.wsf',
  '.ps1', '.psm1', '.psd1',
  '.sh', '.bash', '.zsh',
  '.php', '.phtml', '.php3', '.php4', '.php5', '.phps',
  '.asp', '.aspx', '.cer', '.crt',
  '.py', '.pyc', '.pyo',
  '.rb', '.pl', '.cgi',
  '.jar', '.war',
  '.html', '.htm', '.xhtml', '.svg', // Can contain scripts (except for signature)
];

// ============================================
// VALIDATION TYPES
// ============================================

export interface FileValidationResult {
  isValid: boolean;
  errors: string[];
  sanitizedFilename?: string;
  detectedMimeType?: string;
}

export interface FileValidationOptions {
  configType?: keyof typeof FILE_UPLOAD_CONFIGS;
  customConfig?: Partial<FileUploadConfig>;
  checkMagicBytes?: boolean;
}

// ============================================
// VALIDATION FUNCTIONS
// ============================================

/**
 * Sanitize filename to prevent path traversal and other attacks
 */
export const sanitizeFilename = (filename: string, maxLength: number = 255): string => {
  if (!filename) return '';

  let sanitized = filename;

  // Remove path components (path traversal prevention)
  sanitized = sanitized.replace(/^.*[\\\/]/, '');

  // Remove null bytes
  sanitized = sanitized.replace(/\0/g, '');

  // Remove control characters
  sanitized = sanitized.replace(/[\x00-\x1f\x7f]/g, '');

  // Remove potentially dangerous characters
  sanitized = sanitized.replace(/[<>:"|?*]/g, '_');

  // Remove leading/trailing dots and spaces
  sanitized = sanitized.replace(/^[\s.]+|[\s.]+$/g, '');

  // Replace multiple consecutive dots with single dot
  sanitized = sanitized.replace(/\.{2,}/g, '.');

  // Replace multiple spaces/underscores with single
  sanitized = sanitized.replace(/[\s_]+/g, '_');

  // Ensure filename doesn't start with a dot (hidden files)
  if (sanitized.startsWith('.')) {
    sanitized = '_' + sanitized.slice(1);
  }

  // Truncate to max length while preserving extension
  if (sanitized.length > maxLength) {
    const ext = getFileExtension(sanitized);
    const nameWithoutExt = sanitized.slice(0, -(ext.length || 0) - 1);
    const maxNameLength = maxLength - (ext.length || 0) - 1;
    sanitized = nameWithoutExt.slice(0, maxNameLength) + (ext ? '.' + ext : '');
  }

  // If filename is empty after sanitization, generate a safe name
  if (!sanitized || sanitized === '.' || sanitized === '..') {
    sanitized = `file_${Date.now()}`;
  }

  return sanitized;
};

/**
 * Get file extension (lowercase, without dot)
 */
export const getFileExtension = (filename: string): string => {
  if (!filename) return '';
  const lastDot = filename.lastIndexOf('.');
  if (lastDot === -1 || lastDot === filename.length - 1) return '';
  return filename.slice(lastDot + 1).toLowerCase();
};

/**
 * Check if extension is in the dangerous list
 */
export const isDangerousExtension = (filename: string): boolean => {
  const ext = '.' + getFileExtension(filename);
  return DANGEROUS_EXTENSIONS.includes(ext.toLowerCase());
};

/**
 * Check if extension is allowed
 */
export const isAllowedExtension = (filename: string, allowedExtensions: string[]): boolean => {
  const ext = '.' + getFileExtension(filename);
  return allowedExtensions.some((allowed) => allowed.toLowerCase() === ext.toLowerCase());
};

/**
 * Check MIME type against magic bytes (file signature)
 * This helps detect disguised files
 */
export const detectMimeType = async (file: File): Promise<string> => {
  const signatures: Record<string, number[][]> = {
    'image/jpeg': [[0xff, 0xd8, 0xff]],
    'image/png': [[0x89, 0x50, 0x4e, 0x47]],
    'image/gif': [[0x47, 0x49, 0x46, 0x38]],
    'image/webp': [[0x52, 0x49, 0x46, 0x46]], // Also check for WEBP
    'application/pdf': [[0x25, 0x50, 0x44, 0x46]],
    'application/zip': [[0x50, 0x4b, 0x03, 0x04]],
    'audio/mpeg': [[0xff, 0xfb], [0xff, 0xfa], [0x49, 0x44, 0x33]], // MP3
    'audio/wav': [[0x52, 0x49, 0x46, 0x46]], // Also check for WAVE
  };

  try {
    const buffer = await file.slice(0, 16).arrayBuffer();
    const bytes = new Uint8Array(buffer);

    for (const [mimeType, sigs] of Object.entries(signatures)) {
      for (const sig of sigs) {
        if (sig.every((byte, i) => bytes[i] === byte)) {
          return mimeType;
        }
      }
    }
  } catch {
    // Fall back to browser-reported type
  }

  return file.type || 'application/octet-stream';
};

/**
 * Comprehensive file validation
 */
export const validateFile = async (
  file: File,
  options: FileValidationOptions = {}
): Promise<FileValidationResult> => {
  const errors: string[] = [];

  const { configType = 'attachment', customConfig, checkMagicBytes = true } = options;

  // Get config
  const baseConfig = FILE_UPLOAD_CONFIGS[configType] || FILE_UPLOAD_CONFIGS.attachment;
  const config: FileUploadConfig = { ...baseConfig, ...customConfig };

  // 1. Check file exists and has content
  if (!file) {
    return { isValid: false, errors: ['No file provided'] };
  }

  if (file.size === 0) {
    errors.push('File is empty');
  }

  // 2. Check file size
  if (file.size > config.maxSizeBytes) {
    const maxSizeMB = (config.maxSizeBytes / (1024 * 1024)).toFixed(1);
    errors.push(`File size exceeds maximum allowed (${maxSizeMB}MB)`);
  }

  // 3. Sanitize and validate filename
  const sanitizedFilename = sanitizeFilename(file.name, config.maxFilenameLength);

  // 4. Check for dangerous extensions
  if (isDangerousExtension(file.name)) {
    errors.push('File type is not allowed for security reasons');
  }

  // 5. Check allowed extensions
  if (!isAllowedExtension(file.name, config.allowedExtensions)) {
    errors.push(`File type not allowed. Allowed types: ${config.allowedExtensions.join(', ')}`);
  }

  // 6. Check MIME type
  let detectedMimeType = file.type;

  if (checkMagicBytes) {
    detectedMimeType = await detectMimeType(file);
  }

  if (!config.allowedMimeTypes.includes(detectedMimeType)) {
    // Allow if browser type matches even if magic bytes differ
    if (!config.allowedMimeTypes.includes(file.type)) {
      errors.push(`Invalid file type detected`);
    }
  }

  // 7. Check for double extensions (file.pdf.exe)
  const parts = file.name.split('.');
  if (parts.length > 2) {
    const potentialDangerous = parts.slice(1).some(
      (ext) => DANGEROUS_EXTENSIONS.includes('.' + ext.toLowerCase())
    );
    if (potentialDangerous) {
      errors.push('File appears to have a dangerous hidden extension');
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
    sanitizedFilename,
    detectedMimeType,
  };
};

/**
 * Validate multiple files
 */
export const validateFiles = async (
  files: FileList | File[],
  options: FileValidationOptions & { maxFiles?: number } = {}
): Promise<{
  validFiles: Array<{ file: File; sanitizedFilename: string }>;
  invalidFiles: Array<{ file: File; errors: string[] }>;
  overallErrors: string[];
}> => {
  const { maxFiles = 10, ...validationOptions } = options;

  const validFiles: Array<{ file: File; sanitizedFilename: string }> = [];
  const invalidFiles: Array<{ file: File; errors: string[] }> = [];
  const overallErrors: string[] = [];

  const fileArray = Array.from(files);

  if (fileArray.length > maxFiles) {
    overallErrors.push(`Too many files. Maximum allowed: ${maxFiles}`);
  }

  for (const file of fileArray.slice(0, maxFiles)) {
    const result = await validateFile(file, validationOptions);
    if (result.isValid && result.sanitizedFilename) {
      validFiles.push({ file, sanitizedFilename: result.sanitizedFilename });
    } else {
      invalidFiles.push({ file, errors: result.errors });
    }
  }

  return { validFiles, invalidFiles, overallErrors };
};

// ============================================
// SECURE UPLOAD HELPERS
// ============================================

/**
 * Generate a secure random filename
 */
export const generateSecureFilename = (originalFilename: string): string => {
  const ext = getFileExtension(originalFilename);
  const timestamp = Date.now();
  const random = Math.random().toString(36).substring(2, 15);
  return `${timestamp}_${random}${ext ? '.' + ext : ''}`;
};

/**
 * Create a secure file path (prevents path traversal)
 */
export const createSecureFilePath = (
  baseDir: string,
  filename: string,
  subDir?: string
): string => {
  // Sanitize all components
  const sanitizedFilename = sanitizeFilename(filename);
  const sanitizedSubDir = subDir
    ? subDir.replace(/[^a-zA-Z0-9_-]/g, '_').replace(/\.{2,}/g, '')
    : '';

  // Build path without allowing traversal
  const parts = [baseDir];
  if (sanitizedSubDir) {
    parts.push(sanitizedSubDir);
  }
  parts.push(sanitizedFilename);

  return parts.join('/');
};

/**
 * Get human-readable file size
 */
export const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes';

  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

/**
 * Check if file is an image
 */
export const isImageFile = (file: File): boolean => {
  return file.type.startsWith('image/');
};

/**
 * Create a preview URL for image files (cleanup required)
 */
export const createImagePreview = (file: File): string | null => {
  if (!isImageFile(file)) return null;
  return URL.createObjectURL(file);
};

/**
 * Cleanup preview URL to prevent memory leaks
 */
export const revokeImagePreview = (url: string): void => {
  URL.revokeObjectURL(url);
};
