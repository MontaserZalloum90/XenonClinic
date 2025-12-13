/**
 * Shared API utilities and types
 * Common patterns for API communication across applications
 */

// ============================================
// Types
// ============================================

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface ApiError {
  status: number;
  message: string;
  details?: Record<string, string[]>;
}

export interface RequestConfig {
  headers?: Record<string, string>;
  timeout?: number;
  signal?: AbortSignal;
}

export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

// ============================================
// Token Management
// ============================================

const TOKEN_KEY = "auth_token";
const USER_KEY = "user_info";

export const tokenStorage = {
  getToken: (): string | null => {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(TOKEN_KEY);
  },

  setToken: (token: string): void => {
    if (typeof window === "undefined") return;
    localStorage.setItem(TOKEN_KEY, token);
  },

  removeToken: (): void => {
    if (typeof window === "undefined") return;
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },

  clearToken: (): void => {
    if (typeof window === "undefined") return;
    localStorage.removeItem(TOKEN_KEY);
  },

  clearUserData: (): void => {
    if (typeof window === "undefined") return;
    localStorage.removeItem(USER_KEY);
  },

  isAuthenticated: (): boolean => {
    return !!tokenStorage.getToken();
  },
};

// ============================================
// Fetch-based API Client
// ============================================

export function createApiClient(baseUrl: string) {
  const defaultHeaders: Record<string, string> = {
    "Content-Type": "application/json",
  };

  async function request<T>(
    method: HttpMethod,
    endpoint: string,
    data?: unknown,
    config?: RequestConfig,
  ): Promise<ApiResponse<T>> {
    try {
      const token = tokenStorage.getToken();
      const headers: Record<string, string> = {
        ...defaultHeaders,
        ...(token && { Authorization: `Bearer ${token}` }),
        ...config?.headers,
      };

      const options: RequestInit = {
        method,
        headers,
        signal: config?.signal,
      };

      if (data && method !== "GET") {
        options.body = JSON.stringify(data);
      }

      const response = await fetch(`${baseUrl}${endpoint}`, options);

      if (!response.ok) {
        // Handle 401 Unauthorized
        if (response.status === 401) {
          tokenStorage.removeToken();
          if (typeof window !== "undefined") {
            window.location.href = "/login";
          }
        }

        const errorData = await response.json().catch(() => ({}));
        return {
          success: false,
          error:
            errorData.message ||
            `Request failed with status ${response.status}`,
        };
      }

      // Handle empty responses
      const text = await response.text();
      const responseData = text ? JSON.parse(text) : null;

      return {
        success: true,
        data: responseData,
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : "Network error",
      };
    }
  }

  return {
    get: <T>(endpoint: string, config?: RequestConfig) =>
      request<T>("GET", endpoint, undefined, config),

    post: <T>(endpoint: string, data?: unknown, config?: RequestConfig) =>
      request<T>("POST", endpoint, data, config),

    put: <T>(endpoint: string, data?: unknown, config?: RequestConfig) =>
      request<T>("PUT", endpoint, data, config),

    patch: <T>(endpoint: string, data?: unknown, config?: RequestConfig) =>
      request<T>("PATCH", endpoint, data, config),

    delete: <T>(endpoint: string, config?: RequestConfig) =>
      request<T>("DELETE", endpoint, undefined, config),
  };
}

// ============================================
// Query String Helpers
// ============================================

export function buildQueryString(
  params: Record<string, string | number | boolean | undefined>,
): string {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.append(key, String(value));
    }
  });

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : "";
}

// ============================================
// Error Handling Helpers
// ============================================

export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === "object" &&
    error !== null &&
    "status" in error &&
    "message" in error
  );
}

export function getErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    return error.message;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "An unexpected error occurred";
}

// ============================================
// Retry Logic
// ============================================

export async function withRetry<T>(
  fn: () => Promise<T>,
  options: { maxRetries?: number; delay?: number; backoff?: number } = {},
): Promise<T> {
  const { maxRetries = 3, delay = 1000, backoff = 2 } = options;

  let lastError: Error | undefined;

  for (let attempt = 0; attempt < maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error instanceof Error ? error : new Error(String(error));

      if (attempt < maxRetries - 1) {
        await new Promise((resolve) =>
          setTimeout(resolve, delay * Math.pow(backoff, attempt)),
        );
      }
    }
  }

  throw lastError;
}
