import axios from 'axios';
import { configureAxiosInstance, tokenStorage, getAxiosErrorMessage } from '@xenon/ui';

// API Base URL - adjust based on environment
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001';

// Create axios instance with shared configuration
export const api = configureAxiosInstance(
  axios.create(),
  {
    baseURL: API_BASE_URL,
    withCredentials: false,
    onUnauthorized: () => {
      window.location.href = '/login';
    },
  }
);

// Re-export utilities for use in auth contexts
export { tokenStorage, getAxiosErrorMessage };

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

// ============================================
// SALES API ENDPOINTS
// ============================================

export const salesApi = {
  // Sales
  getAllSales: () => api.get('/api/SalesApi/sales'),
  getSaleById: (id: number) => api.get(`/api/SalesApi/sales/${id}`),
  getSaleByInvoiceNumber: (invoiceNumber: string) =>
    api.get(`/api/SalesApi/sales/invoice/${encodeURIComponent(invoiceNumber)}`),
  getSalesByPatient: (patientId: number) => api.get(`/api/SalesApi/sales/patient/${patientId}`),
  getSalesByStatus: (status: number) => api.get(`/api/SalesApi/sales/status/${status}`),
  getSalesByPaymentStatus: (status: number) => api.get(`/api/SalesApi/sales/payment-status/${status}`),
  getSalesByDateRange: (startDate: string, endDate: string) =>
    api.get('/api/SalesApi/sales/date-range', { params: { startDate, endDate } }),
  getOverdueSales: () => api.get('/api/SalesApi/sales/overdue'),
  createSale: (data: any) => api.post('/api/SalesApi/sales', data),
  updateSale: (id: number, data: any) => api.put(`/api/SalesApi/sales/${id}`, data),
  deleteSale: (id: number) => api.delete(`/api/SalesApi/sales/${id}`),
  confirmSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/confirm`),
  completeSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/complete`),
  cancelSale: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/sales/${id}/cancel`, { reason }),

  // Sale Items
  getSaleItems: (saleId: number) => api.get(`/api/SalesApi/sales/${saleId}/items`),
  addSaleItem: (saleId: number, data: any) => api.post(`/api/SalesApi/sales/${saleId}/items`, data),
  updateSaleItem: (saleId: number, itemId: number, data: any) =>
    api.put(`/api/SalesApi/sales/${saleId}/items/${itemId}`, data),
  deleteSaleItem: (saleId: number, itemId: number) =>
    api.delete(`/api/SalesApi/sales/${saleId}/items/${itemId}`),

  // Payments
  getPaymentsBySale: (saleId: number) => api.get(`/api/SalesApi/sales/${saleId}/payments`),
  recordPayment: (saleId: number, data: any) =>
    api.post(`/api/SalesApi/sales/${saleId}/payments`, data),
  getPaymentById: (id: number) => api.get(`/api/SalesApi/payments/${id}`),
  updatePayment: (id: number, data: any) => api.put(`/api/SalesApi/payments/${id}`, data),
  deletePayment: (id: number) => api.delete(`/api/SalesApi/payments/${id}`),
  refundPayment: (id: number, amount: number, reason?: string) =>
    api.post(`/api/SalesApi/payments/${id}/refund`, { amount, reason }),

  // Statistics
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get('/api/SalesApi/statistics', { params: { startDate, endDate } }),
};

export const quotationsApi = {
  // Quotations
  getAll: () => api.get('/api/SalesApi/quotations'),
  getById: (id: number) => api.get(`/api/SalesApi/quotations/${id}`),
  getByNumber: (quotationNumber: string) =>
    api.get(`/api/SalesApi/quotations/number/${encodeURIComponent(quotationNumber)}`),
  getByPatient: (patientId: number) => api.get(`/api/SalesApi/quotations/patient/${patientId}`),
  getByStatus: (status: number) => api.get(`/api/SalesApi/quotations/status/${status}`),
  getActive: () => api.get('/api/SalesApi/quotations/active'),
  getExpired: () => api.get('/api/SalesApi/quotations/expired'),
  create: (data: any) => api.post('/api/SalesApi/quotations', data),
  update: (id: number, data: any) => api.put(`/api/SalesApi/quotations/${id}`, data),
  delete: (id: number) => api.delete(`/api/SalesApi/quotations/${id}`),
  send: (id: number) => api.post(`/api/SalesApi/quotations/${id}/send`),
  accept: (id: number) => api.post(`/api/SalesApi/quotations/${id}/accept`),
  reject: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/quotations/${id}/reject`, { reason }),
  convertToSale: (id: number) => api.post(`/api/SalesApi/quotations/${id}/convert`),

  // Quotation Items
  getItems: (quotationId: number) => api.get(`/api/SalesApi/quotations/${quotationId}/items`),
  addItem: (quotationId: number, data: any) =>
    api.post(`/api/SalesApi/quotations/${quotationId}/items`, data),
  updateItem: (quotationId: number, itemId: number, data: any) =>
    api.put(`/api/SalesApi/quotations/${quotationId}/items/${itemId}`, data),
  deleteItem: (quotationId: number, itemId: number) =>
    api.delete(`/api/SalesApi/quotations/${quotationId}/items/${itemId}`),
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

// ============================================
// MARKETING API ENDPOINTS
// ============================================

export const campaignApi = {
  getAll: () => api.get('/api/MarketingApi/campaigns'),
  getById: (id: number) => api.get(`/api/MarketingApi/campaigns/${id}`),
  getActive: () => api.get('/api/MarketingApi/campaigns/active'),
  getByStatus: (status: string) => api.get(`/api/MarketingApi/campaigns/status/${status}`),
  getByType: (type: string) => api.get(`/api/MarketingApi/campaigns/type/${type}`),
  search: (searchTerm: string) =>
    api.get(`/api/MarketingApi/campaigns/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  create: (data: any) => api.post('/api/MarketingApi/campaigns', data),
  update: (id: number, data: any) => api.put(`/api/MarketingApi/campaigns/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/campaigns/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/MarketingApi/campaigns/${id}/status`, { status }),
  activate: (id: number) => api.post(`/api/MarketingApi/campaigns/${id}/activate`),
  pause: (id: number) => api.post(`/api/MarketingApi/campaigns/${id}/pause`),
  complete: (id: number) => api.post(`/api/MarketingApi/campaigns/${id}/complete`),
  getPerformance: (id: number) => api.get(`/api/MarketingApi/campaigns/${id}/performance`),
};

export const leadApi = {
  getAll: () => api.get('/api/MarketingApi/leads'),
  getById: (id: number) => api.get(`/api/MarketingApi/leads/${id}`),
  getByStatus: (status: string) => api.get(`/api/MarketingApi/leads/status/${status}`),
  getBySource: (source: string) => api.get(`/api/MarketingApi/leads/source/${source}`),
  getByCampaign: (campaignId: number) => api.get(`/api/MarketingApi/leads/campaign/${campaignId}`),
  getNew: () => api.get('/api/MarketingApi/leads/new'),
  getQualified: () => api.get('/api/MarketingApi/leads/qualified'),
  getNeedingFollowUp: () => api.get('/api/MarketingApi/leads/follow-up'),
  getOverdueFollowUps: () => api.get('/api/MarketingApi/leads/overdue'),
  search: (searchTerm: string) =>
    api.get(`/api/MarketingApi/leads/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  create: (data: any) => api.post('/api/MarketingApi/leads', data),
  update: (id: number, data: any) => api.put(`/api/MarketingApi/leads/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/leads/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/MarketingApi/leads/${id}/status`, { status }),
  convert: (id: number, patientData?: any) =>
    api.post(`/api/MarketingApi/leads/${id}/convert`, patientData),
  markAsLost: (id: number, reason: string) =>
    api.post(`/api/MarketingApi/leads/${id}/lost`, { reason }),
  logContact: (id: number, notes?: string) =>
    api.post(`/api/MarketingApi/leads/${id}/contact`, { notes }),
  scheduleFollowUp: (id: number, date: string, notes?: string) =>
    api.post(`/api/MarketingApi/leads/${id}/schedule-follow-up`, { date, notes }),
  assign: (id: number, userId: string) =>
    api.post(`/api/MarketingApi/leads/${id}/assign`, { userId }),
};

export const marketingActivityApi = {
  getAll: () => api.get('/api/MarketingApi/activities'),
  getById: (id: number) => api.get(`/api/MarketingApi/activities/${id}`),
  getByLead: (leadId: number) => api.get(`/api/MarketingApi/activities/lead/${leadId}`),
  getByCampaign: (campaignId: number) => api.get(`/api/MarketingApi/activities/campaign/${campaignId}`),
  getByPatient: (patientId: number) => api.get(`/api/MarketingApi/activities/patient/${patientId}`),
  getToday: () => api.get('/api/MarketingApi/activities/today'),
  getScheduled: () => api.get('/api/MarketingApi/activities/scheduled'),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get('/api/MarketingApi/activities', { params: { startDate, endDate } }),
  create: (data: any) => api.post('/api/MarketingApi/activities', data),
  update: (id: number, data: any) => api.put(`/api/MarketingApi/activities/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/activities/${id}`),
  complete: (id: number, outcome?: string) =>
    api.post(`/api/MarketingApi/activities/${id}/complete`, { outcome }),
  cancel: (id: number, reason?: string) =>
    api.post(`/api/MarketingApi/activities/${id}/cancel`, { reason }),
};

export const marketingStatsApi = {
  getDashboard: () => api.get('/api/MarketingApi/statistics/dashboard'),
  getCampaignStats: (startDate?: string, endDate?: string) =>
    api.get('/api/MarketingApi/statistics/campaigns', { params: { startDate, endDate } }),
  getLeadStats: () => api.get('/api/MarketingApi/statistics/leads'),
  getLeadFunnel: () => api.get('/api/MarketingApi/statistics/funnel'),
  getLeadTrends: (days?: number) =>
    api.get('/api/MarketingApi/statistics/trends', { params: { days } }),
  getConversionRates: () => api.get('/api/MarketingApi/statistics/conversion-rates'),
  getROIReport: (startDate?: string, endDate?: string) =>
    api.get('/api/MarketingApi/statistics/roi', { params: { startDate, endDate } }),
  getCampaignPerformance: () => api.get('/api/MarketingApi/statistics/campaign-performance'),
};

export default api;
