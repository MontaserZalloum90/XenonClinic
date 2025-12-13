/**
 * CSRF (Cross-Site Request Forgery) Protection
 * Implements token-based CSRF protection for state-changing requests
 */

// ============================================
// CONFIGURATION
// ============================================

const CSRF_TOKEN_KEY = "xenon_csrf_token";
const CSRF_HEADER_NAME = "X-CSRF-Token";
const CSRF_COOKIE_NAME = "XSRF-TOKEN";
const TOKEN_EXPIRY_MS = 60 * 60 * 1000; // 1 hour

// ============================================
// TOKEN GENERATION
// ============================================

/**
 * Generate a cryptographically secure random token
 */
export const generateCsrfToken = (): string => {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return Array.from(array, (byte) => byte.toString(16).padStart(2, "0")).join(
    "",
  );
};

// ============================================
// TOKEN STORAGE
// ============================================

interface StoredToken {
  token: string;
  timestamp: number;
}

/**
 * Store CSRF token in sessionStorage
 */
export const storeCsrfToken = (token: string): void => {
  const timedToken: StoredToken = {
    token,
    timestamp: Date.now(),
  };
  sessionStorage.setItem(CSRF_TOKEN_KEY, JSON.stringify(timedToken));
};

/**
 * Retrieve CSRF token from storage
 */
export const getCsrfToken = (): string | null => {
  try {
    const stored = sessionStorage.getItem(CSRF_TOKEN_KEY);
    if (!stored) return null;

    const timedToken: StoredToken = JSON.parse(stored);

    // Check if token has expired
    if (Date.now() - timedToken.timestamp > TOKEN_EXPIRY_MS) {
      sessionStorage.removeItem(CSRF_TOKEN_KEY);
      return null;
    }

    return timedToken.token;
  } catch {
    return null;
  }
};

/**
 * Get or create CSRF token
 */
export const getOrCreateCsrfToken = (): string => {
  let token = getCsrfToken();

  if (!token) {
    token = generateCsrfToken();
    storeCsrfToken(token);
  }

  return token;
};

/**
 * Refresh CSRF token
 */
export const refreshCsrfToken = (): string => {
  const token = generateCsrfToken();
  storeCsrfToken(token);
  return token;
};

/**
 * Clear CSRF token (call on logout)
 */
export const clearCsrfToken = (): void => {
  sessionStorage.removeItem(CSRF_TOKEN_KEY);
};

// ============================================
// REQUEST ENHANCEMENT
// ============================================

/**
 * Get CSRF header object for fetch requests
 */
export const getCsrfHeader = (): Record<string, string> => {
  const token = getOrCreateCsrfToken();
  return { [CSRF_HEADER_NAME]: token };
};

/**
 * Enhance fetch options with CSRF token
 */
export const withCsrfProtection = (options: RequestInit = {}): RequestInit => {
  const headers = new Headers(options.headers);
  headers.set(CSRF_HEADER_NAME, getOrCreateCsrfToken());

  return {
    ...options,
    headers,
  };
};

/**
 * Check if request method requires CSRF protection
 */
export const requiresCsrfProtection = (method: string): boolean => {
  const safeMethodsRegex = /^(GET|HEAD|OPTIONS|TRACE)$/i;
  return !safeMethodsRegex.test(method);
};

// ============================================
// AXIOS INTERCEPTOR HELPERS
// ============================================

/**
 * Create Axios request interceptor for CSRF
 */
export const createAxiosCsrfInterceptor = () => {
  return (config: { method?: string; headers?: Record<string, string> }) => {
    if (config.method && requiresCsrfProtection(config.method)) {
      config.headers = {
        ...config.headers,
        [CSRF_HEADER_NAME]: getOrCreateCsrfToken(),
      };
    }
    return config;
  };
};

// ============================================
// FORM PROTECTION
// ============================================

/**
 * Get hidden input for forms
 */
export const getCsrfFormField = (): { name: string; value: string } => {
  return {
    name: "_csrf",
    value: getOrCreateCsrfToken(),
  };
};

/**
 * Validate CSRF token from form submission
 */
export const validateFormCsrfToken = (submittedToken: string): boolean => {
  const storedToken = getCsrfToken();
  if (!storedToken || !submittedToken) return false;

  // Constant-time comparison to prevent timing attacks
  return timingSafeEqual(storedToken, submittedToken);
};

/**
 * Constant-time string comparison
 */
const timingSafeEqual = (a: string, b: string): boolean => {
  if (a.length !== b.length) return false;

  let result = 0;
  for (let i = 0; i < a.length; i++) {
    result |= a.charCodeAt(i) ^ b.charCodeAt(i);
  }
  return result === 0;
};

// ============================================
// DOUBLE-SUBMIT COOKIE PATTERN
// ============================================

/**
 * Set CSRF token as cookie (for double-submit pattern)
 * The server should compare the cookie value with the header value
 */
export const setCsrfCookie = (token?: string): void => {
  const csrfToken = token || getOrCreateCsrfToken();

  // Set cookie with security attributes
  document.cookie = `${CSRF_COOKIE_NAME}=${csrfToken}; path=/; SameSite=Strict; Secure`;
};

/**
 * Get CSRF token from cookie
 */
export const getCsrfCookie = (): string | null => {
  const match = document.cookie.match(
    new RegExp(`${CSRF_COOKIE_NAME}=([^;]+)`),
  );
  return match ? match[1] : null;
};

/**
 * Initialize double-submit CSRF protection
 * Call this on app initialization
 */
export const initializeCsrfProtection = (): void => {
  // Get or create token
  const token = getOrCreateCsrfToken();

  // Set cookie for double-submit pattern
  setCsrfCookie(token);

  // Refresh token periodically
  setInterval(() => {
    const newToken = refreshCsrfToken();
    setCsrfCookie(newToken);
  }, TOKEN_EXPIRY_MS / 2);
};

// ============================================
// REACT HOOK
// ============================================

/**
 * React hook for CSRF protection
 * Returns token and helper functions
 */
export const useCsrfToken = () => {
  const token = getOrCreateCsrfToken();

  return {
    token,
    headerName: CSRF_HEADER_NAME,
    header: { [CSRF_HEADER_NAME]: token },
    formField: getCsrfFormField(),
    refresh: refreshCsrfToken,
  };
};

// ============================================
// FETCH WRAPPER WITH CSRF
// ============================================

/**
 * Secure fetch wrapper that automatically adds CSRF token
 */
export const secureFetch = async (
  url: string,
  options: RequestInit = {},
): Promise<Response> => {
  const method = options.method || "GET";

  // Add CSRF token for non-safe methods
  if (requiresCsrfProtection(method)) {
    options = withCsrfProtection(options);
  }

  // Add credentials for cookie handling
  options.credentials = options.credentials || "same-origin";

  return fetch(url, options);
};

// ============================================
// ORIGIN VALIDATION (additional protection)
// ============================================

/**
 * Validate request origin matches expected origin
 * Useful for additional server-side validation
 */
export const validateOrigin = (
  requestOrigin: string | null,
  allowedOrigins: string[],
): boolean => {
  if (!requestOrigin) return false;

  return allowedOrigins.some((allowed) => {
    // Exact match
    if (allowed === requestOrigin) return true;

    // Wildcard subdomain matching
    if (allowed.startsWith("*.")) {
      const domain = allowed.slice(2);
      return (
        requestOrigin.endsWith(domain) || requestOrigin === `https://${domain}`
      );
    }

    return false;
  });
};

/**
 * Get current origin
 */
export const getCurrentOrigin = (): string => {
  return window.location.origin;
};
