/**
 * Axios adapter for shared API utilities
 * Provides axios configuration that integrates with shared token storage
 */

import type { AxiosInstance, AxiosError, InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import { tokenStorage } from './api-base';

export interface AxiosAdapterOptions {
  baseURL: string;
  onUnauthorized?: () => void;
  withCredentials?: boolean;
}

/**
 * Configure an axios instance with shared token storage and standard interceptors
 */
export function configureAxiosInstance(
  axios: AxiosInstance,
  options: AxiosAdapterOptions
): AxiosInstance {
  // Set base configuration
  axios.defaults.baseURL = options.baseURL;
  axios.defaults.headers.common['Content-Type'] = 'application/json';
  axios.defaults.withCredentials = options.withCredentials ?? false;

  // Request interceptor - Add auth token
  axios.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const token = tokenStorage.getToken();
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error: AxiosError) => Promise.reject(error)
  );

  // Response interceptor - Handle 401 errors
  axios.interceptors.response.use(
    (response: AxiosResponse) => response,
    async (error: AxiosError) => {
      if (error.response?.status === 401) {
        tokenStorage.clearToken();
        tokenStorage.clearUserData();
        options.onUnauthorized?.();
      }
      return Promise.reject(error);
    }
  );

  return axios;
}

/**
 * Extract error message from axios error
 */
export function getAxiosErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    const axiosError = error as AxiosError<{ message?: string; error?: string }>;
    if (axiosError.response?.data) {
      const data = axiosError.response.data;
      return data.message || data.error || axiosError.message;
    }
    return axiosError.message;
  }
  return 'An unexpected error occurred';
}

/**
 * Check if an error is an axios error
 */
export function isAxiosError(error: unknown): error is AxiosError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'isAxiosError' in error &&
    (error as AxiosError).isAxiosError === true
  );
}
