import axios, { AxiosError } from 'axios';
import type { InternalAxiosRequestConfig } from 'axios';

// API Base URL - adjust based on environment
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001';

// Create axios instance
export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // Important: Allow credentials for CORS
  withCredentials: false,
});

// Request interceptor - Add auth token to requests
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors globally
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear token and redirect to login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// API endpoints
export const authApi = {
  login: (username: string, password: string) =>
    api.post('/api/AuthApi/login', { username, password }),

  register: (username: string, email: string, fullName: string, password: string) =>
    api.post('/api/AuthApi/register', { username, email, fullName, password }),

  getCurrentUser: () => api.get('/api/AuthApi/me'),

  refreshToken: () => api.post('/api/AuthApi/refresh'),
};

export const appointmentsApi = {
  getAll: () => api.get('/api/AppointmentsApi'),
  getById: (id: number) => api.get(`/api/AppointmentsApi/${id}`),
  getByDate: (date: Date) => {
    const dateStr = date.toISOString().split('T')[0];
    return api.get(`/api/AppointmentsApi/date/${dateStr}`);
  },
  getToday: () => api.get('/api/AppointmentsApi/today'),
  getUpcoming: (days: number = 7) => api.get(`/api/AppointmentsApi/upcoming?days=${days}`),
  create: (data: any) => api.post('/api/AppointmentsApi', data),
  update: (id: number, data: any) => api.put(`/api/AppointmentsApi/${id}`, data),
  delete: (id: number) => api.delete(`/api/AppointmentsApi/${id}`),
  confirm: (id: number) => api.post(`/api/AppointmentsApi/${id}/confirm`),
  cancel: (id: number, reason?: string) => api.post(`/api/AppointmentsApi/${id}/cancel`, { reason }),
  checkIn: (id: number) => api.post(`/api/AppointmentsApi/${id}/checkin`),
  complete: (id: number) => api.post(`/api/AppointmentsApi/${id}/complete`),
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get('/api/AppointmentsApi/statistics', { params: { startDate, endDate } }),
};

export default api;
