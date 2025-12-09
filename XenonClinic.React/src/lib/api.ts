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

export const patientsApi = {
  getAll: () => api.get('/api/PatientsApi'),
  getById: (id: number) => api.get(`/api/PatientsApi/${id}`),
  search: (searchTerm: string) => api.get(`/api/PatientsApi/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getByEmiratesId: (emiratesId: string) => api.get(`/api/PatientsApi/emirates/${encodeURIComponent(emiratesId)}`),
  create: (data: any) => api.post('/api/PatientsApi', data),
  update: (id: number, data: any) => api.put(`/api/PatientsApi/${id}`, data),
  delete: (id: number) => api.delete(`/api/PatientsApi/${id}`),
  getMedicalHistory: (id: number) => api.get(`/api/PatientsApi/${id}/medical-history`),
  getDocuments: (id: number) => api.get(`/api/PatientsApi/${id}/documents`),
  getStatistics: () => api.get('/api/PatientsApi/statistics'),
};

export const laboratoryApi = {
  getAllOrders: () => api.get('/api/LaboratoryApi/orders'),
  getOrderById: (id: number) => api.get(`/api/LaboratoryApi/orders/${id}`),
  getPendingOrders: () => api.get('/api/LaboratoryApi/orders/pending'),
  getUrgentOrders: () => api.get('/api/LaboratoryApi/orders/urgent'),
  getOrdersByPatient: (patientId: number) => api.get(`/api/LaboratoryApi/orders/patient/${patientId}`),
  createOrder: (data: any) => api.post('/api/LaboratoryApi/orders', data),
  updateOrder: (id: number, data: any) => api.put(`/api/LaboratoryApi/orders/${id}`, data),
  updateStatus: (id: number, status: number) => api.post(`/api/LaboratoryApi/orders/${id}/status`, { status }),
  deleteOrder: (id: number) => api.delete(`/api/LaboratoryApi/orders/${id}`),
  getAllTests: () => api.get('/api/LaboratoryApi/tests'),
  getStatistics: () => api.get('/api/LaboratoryApi/statistics'),
};

export const hrApi = {
  getAll: () => api.get('/api/HRApi/employees'),
  getById: (id: number) => api.get(`/api/HRApi/employees/${id}`),
  search: (searchTerm: string) => api.get(`/api/HRApi/employees/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getByDepartment: (department: string) => api.get(`/api/HRApi/employees/department/${encodeURIComponent(department)}`),
  getActive: () => api.get('/api/HRApi/employees/active'),
  create: (data: any) => api.post('/api/HRApi/employees', data),
  update: (id: number, data: any) => api.put(`/api/HRApi/employees/${id}`, data),
  delete: (id: number) => api.delete(`/api/HRApi/employees/${id}`),
  getStatistics: () => api.get('/api/HRApi/statistics'),
};

export const financialApi = {
  getAllInvoices: () => api.get('/api/FinancialApi/invoices'),
  getById: (id: number) => api.get(`/api/FinancialApi/invoices/${id}`),
  search: (searchTerm: string) => api.get(`/api/FinancialApi/invoices/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getUnpaid: () => api.get('/api/FinancialApi/invoices/unpaid'),
  getOverdue: () => api.get('/api/FinancialApi/invoices/overdue'),
  getByPatient: (patientId: number) => api.get(`/api/FinancialApi/invoices/patient/${patientId}`),
  create: (data: any) => api.post('/api/FinancialApi/invoices', data),
  update: (id: number, data: any) => api.put(`/api/FinancialApi/invoices/${id}`, data),
  delete: (id: number) => api.delete(`/api/FinancialApi/invoices/${id}`),
  recordPayment: (id: number, amount: number, method: number) =>
    api.post(`/api/FinancialApi/invoices/${id}/payment`, { amount, method }),
  getStatistics: () => api.get('/api/FinancialApi/statistics'),
};

export const inventoryApi = {
  getAllItems: () => api.get('/api/InventoryApi/items'),
  getById: (id: number) => api.get(`/api/InventoryApi/items/${id}`),
  search: (searchTerm: string) => api.get(`/api/InventoryApi/items/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getLowStock: () => api.get('/api/InventoryApi/items/low-stock'),
  getOutOfStock: () => api.get('/api/InventoryApi/items/out-of-stock'),
  getByCategory: (category: number) => api.get(`/api/InventoryApi/items/category/${category}`),
  create: (data: any) => api.post('/api/InventoryApi/items', data),
  update: (id: number, data: any) => api.put(`/api/InventoryApi/items/${id}`, data),
  delete: (id: number) => api.delete(`/api/InventoryApi/items/${id}`),
  adjustStock: (id: number, quantity: number, reason: string) =>
    api.post(`/api/InventoryApi/items/${id}/adjust`, { quantity, reason }),
  getStatistics: () => api.get('/api/InventoryApi/statistics'),
};

export const pharmacyApi = {
  getAllPrescriptions: () => api.get('/api/PharmacyApi/prescriptions'),
  getById: (id: number) => api.get(`/api/PharmacyApi/prescriptions/${id}`),
  search: (searchTerm: string) => api.get(`/api/PharmacyApi/prescriptions/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getPending: () => api.get('/api/PharmacyApi/prescriptions/pending'),
  getByPatient: (patientId: number) => api.get(`/api/PharmacyApi/prescriptions/patient/${patientId}`),
  create: (data: any) => api.post('/api/PharmacyApi/prescriptions', data),
  update: (id: number, data: any) => api.put(`/api/PharmacyApi/prescriptions/${id}`, data),
  delete: (id: number) => api.delete(`/api/PharmacyApi/prescriptions/${id}`),
  dispense: (id: number, dispensedBy: string) =>
    api.post(`/api/PharmacyApi/prescriptions/${id}/dispense`, { dispensedBy }),
  getStatistics: () => api.get('/api/PharmacyApi/statistics'),
};

export const radiologyApi = {
  getAllOrders: () => api.get('/api/RadiologyApi/orders'),
  getById: (id: number) => api.get(`/api/RadiologyApi/orders/${id}`),
  search: (searchTerm: string) => api.get(`/api/RadiologyApi/orders/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getPending: () => api.get('/api/RadiologyApi/orders/pending'),
  getScheduled: () => api.get('/api/RadiologyApi/orders/scheduled'),
  getByPatient: (patientId: number) => api.get(`/api/RadiologyApi/orders/patient/${patientId}`),
  create: (data: any) => api.post('/api/RadiologyApi/orders', data),
  update: (id: number, data: any) => api.put(`/api/RadiologyApi/orders/${id}`, data),
  delete: (id: number) => api.delete(`/api/RadiologyApi/orders/${id}`),
  updateStatus: (id: number, status: number) =>
    api.post(`/api/RadiologyApi/orders/${id}/status`, { status }),
  getStatistics: () => api.get('/api/RadiologyApi/statistics'),
};

// ============================================
// AUDIOLOGY API ENDPOINTS
// ============================================

export const audiogramApi = {
  getAll: () => api.get('/api/AudiologyApi/audiograms'),
  getById: (id: number) => api.get(`/api/AudiologyApi/audiograms/${id}`),
  getByPatient: (patientId: number) => api.get(`/api/AudiologyApi/audiograms/patient/${patientId}`),
  getLatestByPatient: (patientId: number) => api.get(`/api/AudiologyApi/audiograms/patient/${patientId}/latest`),
  create: (data: any) => api.post('/api/AudiologyApi/audiograms', data),
  update: (id: number, data: any) => api.put(`/api/AudiologyApi/audiograms/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/audiograms/${id}`),
  compare: (patientId: number, audiogramIds: number[]) =>
    api.post(`/api/AudiologyApi/audiograms/patient/${patientId}/compare`, { audiogramIds }),
};

export const hearingAidApi = {
  getAll: () => api.get('/api/AudiologyApi/hearing-aids'),
  getById: (id: number) => api.get(`/api/AudiologyApi/hearing-aids/${id}`),
  getByPatient: (patientId: number) => api.get(`/api/AudiologyApi/hearing-aids/patient/${patientId}`),
  getBySerialNumber: (serialNumber: string) =>
    api.get(`/api/AudiologyApi/hearing-aids/serial/${encodeURIComponent(serialNumber)}`),
  getWarrantyExpiring: (days: number = 30) =>
    api.get(`/api/AudiologyApi/hearing-aids/warranty-expiring?days=${days}`),
  create: (data: any) => api.post('/api/AudiologyApi/hearing-aids', data),
  update: (id: number, data: any) => api.put(`/api/AudiologyApi/hearing-aids/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/hearing-aids/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/hearing-aids/${id}/status`, { status }),

  // Fittings
  getFittings: (hearingAidId: number) => api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`),
  createFitting: (hearingAidId: number, data: any) =>
    api.post(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`, data),
  updateFitting: (hearingAidId: number, fittingId: number, data: any) =>
    api.put(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings/${fittingId}`, data),

  // Adjustments
  getAdjustments: (hearingAidId: number) => api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/adjustments`),
  createAdjustment: (hearingAidId: number, data: any) =>
    api.post(`/api/AudiologyApi/hearing-aids/${hearingAidId}/adjustments`, data),
};

export const encounterApi = {
  getAll: () => api.get('/api/AudiologyApi/encounters'),
  getById: (id: number) => api.get(`/api/AudiologyApi/encounters/${id}`),
  getByPatient: (patientId: number) => api.get(`/api/AudiologyApi/encounters/patient/${patientId}`),
  getToday: () => api.get('/api/AudiologyApi/encounters/today'),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get('/api/AudiologyApi/encounters', { params: { startDate, endDate } }),
  getByStatus: (status: string) => api.get(`/api/AudiologyApi/encounters/status/${status}`),
  create: (data: any) => api.post('/api/AudiologyApi/encounters', data),
  update: (id: number, data: any) => api.put(`/api/AudiologyApi/encounters/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/encounters/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/encounters/${id}/status`, { status }),
  complete: (id: number) => api.post(`/api/AudiologyApi/encounters/${id}/complete`),

  // Tasks
  getTasks: (encounterId: number) => api.get(`/api/AudiologyApi/encounters/${encounterId}/tasks`),
  createTask: (encounterId: number, data: any) =>
    api.post(`/api/AudiologyApi/encounters/${encounterId}/tasks`, data),
  updateTask: (encounterId: number, taskId: number, data: any) =>
    api.put(`/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}`, data),
  completeTask: (encounterId: number, taskId: number) =>
    api.post(`/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}/complete`),
  deleteTask: (encounterId: number, taskId: number) =>
    api.delete(`/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}`),

  // All tasks (across encounters)
  getAllPendingTasks: () => api.get('/api/AudiologyApi/tasks/pending'),
  getOverdueTasks: () => api.get('/api/AudiologyApi/tasks/overdue'),
  getTasksByAssignee: (assignee: string) =>
    api.get(`/api/AudiologyApi/tasks/assignee/${encodeURIComponent(assignee)}`),
};

export const consentApi = {
  getAll: () => api.get('/api/AudiologyApi/consents'),
  getById: (id: number) => api.get(`/api/AudiologyApi/consents/${id}`),
  getByPatient: (patientId: number) => api.get(`/api/AudiologyApi/consents/patient/${patientId}`),
  getPending: (patientId: number) => api.get(`/api/AudiologyApi/consents/patient/${patientId}/pending`),
  create: (data: any) => api.post('/api/AudiologyApi/consents', data),
  sign: (id: number, data: any) => api.post(`/api/AudiologyApi/consents/${id}/sign`, data),
  revoke: (id: number, reason: string) =>
    api.post(`/api/AudiologyApi/consents/${id}/revoke`, { reason }),
  delete: (id: number) => api.delete(`/api/AudiologyApi/consents/${id}`),
  getFormTemplate: (consentType: string) =>
    api.get(`/api/AudiologyApi/consents/templates/${consentType}`),
};

export const attachmentApi = {
  getByPatient: (patientId: number) => api.get(`/api/AudiologyApi/attachments/patient/${patientId}`),
  getByEncounter: (encounterId: number) => api.get(`/api/AudiologyApi/attachments/encounter/${encounterId}`),
  getById: (id: number) => api.get(`/api/AudiologyApi/attachments/${id}`),
  upload: (data: FormData) =>
    api.post('/api/AudiologyApi/attachments', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  download: (id: number) =>
    api.get(`/api/AudiologyApi/attachments/${id}/download`, { responseType: 'blob' }),
  delete: (id: number) => api.delete(`/api/AudiologyApi/attachments/${id}`),
  updateMetadata: (id: number, data: any) => api.put(`/api/AudiologyApi/attachments/${id}`, data),
};

export const audiologyStatsApi = {
  getDashboard: () => api.get('/api/AudiologyApi/statistics/dashboard'),
  getEncounterStats: (startDate?: string, endDate?: string) =>
    api.get('/api/AudiologyApi/statistics/encounters', { params: { startDate, endDate } }),
  getHearingAidStats: () => api.get('/api/AudiologyApi/statistics/hearing-aids'),
  getPatientStats: () => api.get('/api/AudiologyApi/statistics/patients'),
};

export default api;
