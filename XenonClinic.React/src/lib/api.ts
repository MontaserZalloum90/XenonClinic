import axios from 'axios';
import { configureAxiosInstance, tokenStorage, getAxiosErrorMessage } from '@xenon/ui';
import type { CreateAppointmentRequest, UpdateAppointmentRequest } from '../types/appointment';
import type { CreatePatientRequest, UpdatePatientRequest } from '../types/patient';
import type { CreateLabOrderRequest } from '../types/laboratory';
import type { EmployeeFormData } from '../types/hr';
import type { InvoiceFormData } from '../types/financial';
import type { SaleFormData, SaleItemFormData, PaymentFormData, QuotationFormData, QuotationItemFormData } from '../types/sales';
import type { InventoryItemFormData } from '../types/inventory';
import type { CreateWorkflowDefinitionRequest, UpdateWorkflowDefinitionRequest, CreateWorkflowInstanceRequest, UpdateWorkflowInstanceRequest } from '../types/workflow';
import type { PrescriptionFormData } from '../types/pharmacy';
import type { RadiologyOrderFormData } from '../types/radiology';
import type { CreateAudiogramRequest, CreateHearingAidRequest, CreateEncounterRequest, CreateEncounterTaskRequest, CreateConsentRequest, SignConsentRequest } from '../types/audiology';
import type { CreateCampaignRequest, UpdateCampaignRequest, CreateLeadRequest, UpdateLeadRequest, CreateActivityRequest, UpdateActivityRequest } from '../types/marketing';
import type { ClinicalVisitFormData } from '../types/clinical-visit';
import type { CreateReportRequest } from '../types/analytics';

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
  create: (data: CreateAppointmentRequest) => api.post('/api/AppointmentsApi', data),
  update: (id: number, data: UpdateAppointmentRequest) => api.put(`/api/AppointmentsApi/${id}`, data),
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
  create: (data: CreatePatientRequest) => api.post('/api/PatientsApi', data),
  update: (id: number, data: UpdatePatientRequest) => api.put(`/api/PatientsApi/${id}`, data),
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
  createOrder: (data: CreateLabOrderRequest) => api.post('/api/LaboratoryApi/orders', data),
  updateOrder: (id: number, data: Partial<CreateLabOrderRequest>) => api.put(`/api/LaboratoryApi/orders/${id}`, data),
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
  create: (data: EmployeeFormData) => api.post('/api/HRApi/employees', data),
  update: (id: number, data: EmployeeFormData) => api.put(`/api/HRApi/employees/${id}`, data),
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
  create: (data: InvoiceFormData) => api.post('/api/FinancialApi/invoices', data),
  update: (id: number, data: InvoiceFormData) => api.put(`/api/FinancialApi/invoices/${id}`, data),
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
  createSale: (data: SaleFormData) => api.post('/api/SalesApi/sales', data),
  updateSale: (id: number, data: SaleFormData) => api.put(`/api/SalesApi/sales/${id}`, data),
  deleteSale: (id: number) => api.delete(`/api/SalesApi/sales/${id}`),
  confirmSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/confirm`),
  completeSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/complete`),
  cancelSale: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/sales/${id}/cancel`, { reason }),

  // Sale Items
  getSaleItems: (saleId: number) => api.get(`/api/SalesApi/sales/${saleId}/items`),
  addSaleItem: (saleId: number, data: SaleItemFormData) => api.post(`/api/SalesApi/sales/${saleId}/items`, data),
  updateSaleItem: (saleId: number, itemId: number, data: SaleItemFormData) =>
    api.put(`/api/SalesApi/sales/${saleId}/items/${itemId}`, data),
  deleteSaleItem: (saleId: number, itemId: number) =>
    api.delete(`/api/SalesApi/sales/${saleId}/items/${itemId}`),

  // Payments
  getPaymentsBySale: (saleId: number) => api.get(`/api/SalesApi/sales/${saleId}/payments`),
  recordPayment: (saleId: number, data: PaymentFormData) =>
    api.post(`/api/SalesApi/sales/${saleId}/payments`, data),
  getPaymentById: (id: number) => api.get(`/api/SalesApi/payments/${id}`),
  updatePayment: (id: number, data: PaymentFormData) => api.put(`/api/SalesApi/payments/${id}`, data),
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
  create: (data: QuotationFormData) => api.post('/api/SalesApi/quotations', data),
  update: (id: number, data: QuotationFormData) => api.put(`/api/SalesApi/quotations/${id}`, data),
  delete: (id: number) => api.delete(`/api/SalesApi/quotations/${id}`),
  send: (id: number) => api.post(`/api/SalesApi/quotations/${id}/send`),
  accept: (id: number) => api.post(`/api/SalesApi/quotations/${id}/accept`),
  reject: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/quotations/${id}/reject`, { reason }),
  convertToSale: (id: number) => api.post(`/api/SalesApi/quotations/${id}/convert`),

  // Quotation Items
  getItems: (quotationId: number) => api.get(`/api/SalesApi/quotations/${quotationId}/items`),
  addItem: (quotationId: number, data: QuotationItemFormData) =>
    api.post(`/api/SalesApi/quotations/${quotationId}/items`, data),
  updateItem: (quotationId: number, itemId: number, data: QuotationItemFormData) =>
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
  create: (data: InventoryItemFormData) => api.post('/api/InventoryApi/items', data),
  update: (id: number, data: InventoryItemFormData) => api.put(`/api/InventoryApi/items/${id}`, data),
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
  create: (data: PrescriptionFormData) => api.post('/api/PharmacyApi/prescriptions', data),
  update: (id: number, data: PrescriptionFormData) => api.put(`/api/PharmacyApi/prescriptions/${id}`, data),
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
  create: (data: RadiologyOrderFormData) => api.post('/api/RadiologyApi/orders', data),
  update: (id: number, data: RadiologyOrderFormData) => api.put(`/api/RadiologyApi/orders/${id}`, data),
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
  create: (data: CreateAudiogramRequest) => api.post('/api/AudiologyApi/audiograms', data),
  update: (id: number, data: Partial<CreateAudiogramRequest>) => api.put(`/api/AudiologyApi/audiograms/${id}`, data),
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
  create: (data: CreateHearingAidRequest) => api.post('/api/AudiologyApi/hearing-aids', data),
  update: (id: number, data: Partial<CreateHearingAidRequest>) => api.put(`/api/AudiologyApi/hearing-aids/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/hearing-aids/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/hearing-aids/${id}/status`, { status }),

  // Fittings
  getFittings: (hearingAidId: number) => api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`),
  createFitting: (hearingAidId: number, data: Record<string, unknown>) =>
    api.post(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`, data),
  updateFitting: (hearingAidId: number, fittingId: number, data: Record<string, unknown>) =>
    api.put(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings/${fittingId}`, data),

  // Adjustments
  getAdjustments: (hearingAidId: number) => api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/adjustments`),
  createAdjustment: (hearingAidId: number, data: Record<string, unknown>) =>
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
  create: (data: CreateEncounterRequest) => api.post('/api/AudiologyApi/encounters', data),
  update: (id: number, data: Partial<CreateEncounterRequest>) => api.put(`/api/AudiologyApi/encounters/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/encounters/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/encounters/${id}/status`, { status }),
  complete: (id: number) => api.post(`/api/AudiologyApi/encounters/${id}/complete`),

  // Tasks
  getTasks: (encounterId: number) => api.get(`/api/AudiologyApi/encounters/${encounterId}/tasks`),
  createTask: (encounterId: number, data: CreateEncounterTaskRequest) =>
    api.post(`/api/AudiologyApi/encounters/${encounterId}/tasks`, data),
  updateTask: (encounterId: number, taskId: number, data: Partial<CreateEncounterTaskRequest>) =>
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
  create: (data: CreateConsentRequest) => api.post('/api/AudiologyApi/consents', data),
  sign: (id: number, data: SignConsentRequest) => api.post(`/api/AudiologyApi/consents/${id}/sign`, data),
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
  updateMetadata: (id: number, data: { title?: string; description?: string }) => api.put(`/api/AudiologyApi/attachments/${id}`, data),
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
  create: (data: CreateCampaignRequest) => api.post('/api/MarketingApi/campaigns', data),
  update: (id: number, data: UpdateCampaignRequest) => api.put(`/api/MarketingApi/campaigns/${id}`, data),
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
  create: (data: CreateLeadRequest) => api.post('/api/MarketingApi/leads', data),
  update: (id: number, data: UpdateLeadRequest) => api.put(`/api/MarketingApi/leads/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/leads/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/MarketingApi/leads/${id}/status`, { status }),
  convert: (id: number, patientData?: CreatePatientRequest) =>
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
  create: (data: CreateActivityRequest) => api.post('/api/MarketingApi/activities', data),
  update: (id: number, data: UpdateActivityRequest) => api.put(`/api/MarketingApi/activities/${id}`, data),
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

// ============================================
// CLINICAL VISITS API ENDPOINTS
// ============================================

export const clinicalVisitsApi = {
  getAll: () => api.get('/api/ClinicalVisitsApi/visits'),
  getById: (id: number) => api.get(`/api/ClinicalVisitsApi/visits/${id}`),
  getByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/visits/patient/${patientId}`),
  getByDoctor: (doctorId: number) => api.get(`/api/ClinicalVisitsApi/visits/doctor/${doctorId}`),
  getToday: () => api.get('/api/ClinicalVisitsApi/visits/today'),
  getByStatus: (status: number) => api.get(`/api/ClinicalVisitsApi/visits/status/${status}`),
  getByType: (type: number) => api.get(`/api/ClinicalVisitsApi/visits/type/${type}`),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get('/api/ClinicalVisitsApi/visits/date-range', { params: { startDate, endDate } }),
  search: (searchTerm: string) =>
    api.get(`/api/ClinicalVisitsApi/visits/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  create: (data: ClinicalVisitFormData) => api.post('/api/ClinicalVisitsApi/visits', data),
  update: (id: number, data: ClinicalVisitFormData) => api.put(`/api/ClinicalVisitsApi/visits/${id}`, data),
  delete: (id: number) => api.delete(`/api/ClinicalVisitsApi/visits/${id}`),
  updateStatus: (id: number, status: number) =>
    api.post(`/api/ClinicalVisitsApi/visits/${id}/status`, { status }),
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get('/api/ClinicalVisitsApi/statistics', { params: { startDate, endDate } }),
  getStatisticsBySpecialty: (fromDate?: string, toDate?: string) =>
    api.get('/api/ClinicalVisitsApi/statistics/by-specialty', { params: { fromDate, toDate } }),
};

// ============================================
// CARDIOLOGY API ENDPOINTS
// ============================================

export const cardiologyApi = {
  // Cardiology visits
  getVisitById: (id: number) => api.get(`/api/ClinicalVisitsApi/cardiology/${id}`),
  getVisitsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/cardiology/patient/${patientId}`),
  createVisit: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/cardiology', data),

  // ECG Records
  getECGsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/cardiology/ecg/patient/${patientId}`),
  createECG: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/cardiology/ecg', data),
  updateECG: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/cardiology/ecg/${id}`, data),
  deleteECG: (id: number) => api.delete(`/api/ClinicalVisitsApi/cardiology/ecg/${id}`),

  // Echo Records
  getEchosByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/cardiology/echo/patient/${patientId}`),
  createEcho: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/cardiology/echo', data),
  updateEcho: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/cardiology/echo/${id}`, data),
  deleteEcho: (id: number) => api.delete(`/api/ClinicalVisitsApi/cardiology/echo/${id}`),

  // Stress Tests
  getStressTestsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/cardiology/stress-test/patient/${patientId}`),
  createStressTest: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/cardiology/stress-test', data),
  updateStressTest: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/cardiology/stress-test/${id}`, data),
  deleteStressTest: (id: number) => api.delete(`/api/ClinicalVisitsApi/cardiology/stress-test/${id}`),

  // Cath Lab Procedures
  getCathProceduresByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/cardiology/cath/patient/${patientId}`),
  createCathProcedure: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/cardiology/cath', data),
  updateCathProcedure: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/cardiology/cath/${id}`, data),
  deleteCathProcedure: (id: number) => api.delete(`/api/ClinicalVisitsApi/cardiology/cath/${id}`),
};

// ============================================
// NEUROLOGY API ENDPOINTS
// ============================================

export const neurologyApi = {
  // Neurology visits
  getVisitById: (id: number) => api.get(`/api/ClinicalVisitsApi/neurology/${id}`),
  getVisitsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/neurology/patient/${patientId}`),
  createVisit: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/neurology', data),
  deleteVisit: (id: number) => api.delete(`/api/ClinicalVisitsApi/neurology/${id}`),

  // Neurological Exams
  getExamsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/neurology/exams/patient/${patientId}`),
  createExam: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/neurology/exams', data),
  updateExam: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/neurology/exams/${id}`, data),
  deleteExam: (id: number) => api.delete(`/api/ClinicalVisitsApi/neurology/exams/${id}`),

  // EEG Records
  getEEGsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/neurology/eeg/patient/${patientId}`),
  createEEG: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/neurology/eeg', data),
  updateEEG: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/neurology/eeg/${id}`, data),
  deleteEEG: (id: number) => api.delete(`/api/ClinicalVisitsApi/neurology/eeg/${id}`),

  // EMG Studies
  getEMGsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/neurology/emg/patient/${patientId}`),
  createEMG: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/neurology/emg', data),
  updateEMG: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/neurology/emg/${id}`, data),
  deleteEMG: (id: number) => api.delete(`/api/ClinicalVisitsApi/neurology/emg/${id}`),
};

// ============================================
// PEDIATRICS API ENDPOINTS
// ============================================

export const pediatricsApi = {
  // Pediatric visits
  getVisitById: (id: number) => api.get(`/api/ClinicalVisitsApi/pediatrics/${id}`),
  getVisitsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/pediatrics/patient/${patientId}`),
  createVisit: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/pediatrics', data),
  deleteVisit: (id: number) => api.delete(`/api/ClinicalVisitsApi/pediatrics/${id}`),

  // Vaccinations
  getVaccinationsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/pediatrics/vaccinations/patient/${patientId}`),
  createVaccination: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/pediatrics/vaccinations', data),
  updateVaccination: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/pediatrics/vaccinations/${id}`, data),
  deleteVaccination: (id: number) => api.delete(`/api/ClinicalVisitsApi/pediatrics/vaccinations/${id}`),

  // Developmental Milestones
  getMilestonesByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/pediatrics/milestones/patient/${patientId}`),
  createMilestone: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/pediatrics/milestones', data),
  updateMilestone: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/pediatrics/milestones/${id}`, data),
  deleteMilestone: (id: number) => api.delete(`/api/ClinicalVisitsApi/pediatrics/milestones/${id}`),

  // Growth Records
  getGrowthRecordsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/pediatrics/growth/patient/${patientId}`),
  createGrowthRecord: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/pediatrics/growth', data),
  updateGrowthRecord: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/pediatrics/growth/${id}`, data),
  deleteGrowthRecord: (id: number) => api.delete(`/api/ClinicalVisitsApi/pediatrics/growth/${id}`),
};

// ============================================
// DERMATOLOGY API ENDPOINTS
// ============================================

export const dermatologyApi = {
  // Dermatology visits
  getVisitById: (id: number) => api.get(`/api/ClinicalVisitsApi/dermatology/${id}`),
  getVisitsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dermatology/patient/${patientId}`),
  createVisit: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/dermatology', data),
  deleteVisit: (id: number) => api.delete(`/api/ClinicalVisitsApi/dermatology/${id}`),

  // Skin Exams
  getSkinExamsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dermatology/skin-exams/patient/${patientId}`),
  createSkinExam: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/dermatology/skin-exams', data),
  updateSkinExam: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/dermatology/skin-exams/${id}`, data),
  deleteSkinExam: (id: number) => api.delete(`/api/ClinicalVisitsApi/dermatology/skin-exams/${id}`),

  // Mole Mappings
  getMoleMappingsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dermatology/mole-mappings/patient/${patientId}`),
  createMoleMapping: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/dermatology/mole-mappings', data),
  updateMoleMapping: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/dermatology/mole-mappings/${id}`, data),
  deleteMoleMapping: (id: number) => api.delete(`/api/ClinicalVisitsApi/dermatology/mole-mappings/${id}`),

  // Biopsies
  getBiopsiesByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dermatology/biopsies/patient/${patientId}`),
  createBiopsy: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/dermatology/biopsies', data),
  updateBiopsy: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/dermatology/biopsies/${id}`, data),
  deleteBiopsy: (id: number) => api.delete(`/api/ClinicalVisitsApi/dermatology/biopsies/${id}`),

  // Skin Photos
  getSkinPhotosByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dermatology/photos/patient/${patientId}`),
  uploadSkinPhoto: (data: FormData) => api.post('/api/ClinicalVisitsApi/dermatology/photos', data, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  deleteSkinPhoto: (id: number) => api.delete(`/api/ClinicalVisitsApi/dermatology/photos/${id}`),

  // Statistics
  getStatistics: () => api.get('/api/ClinicalVisitsApi/dermatology/statistics'),
};

// ============================================
// PHYSIOTHERAPY API ENDPOINTS
// ============================================

export const physiotherapyApi = {
  // Therapy Sessions
  getSessionsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/physiotherapy/sessions/patient/${patientId}`),
  getAllSessions: () => api.get('/api/ClinicalVisitsApi/physiotherapy/sessions'),
  createSession: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/physiotherapy/sessions', data),
  updateSession: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/physiotherapy/sessions/${id}`, data),
  deleteSession: (id: number) => api.delete(`/api/ClinicalVisitsApi/physiotherapy/sessions/${id}`),

  // Statistics
  getStatistics: () => api.get('/api/ClinicalVisitsApi/physiotherapy/statistics'),
};

// ============================================
// DIALYSIS API ENDPOINTS
// ============================================

export const dialysisApi = {
  // Dialysis Sessions
  getSessionsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/dialysis/sessions/patient/${patientId}`),
  getAllSessions: () => api.get('/api/ClinicalVisitsApi/dialysis/sessions'),
  createSession: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/dialysis/sessions', data),
  updateSession: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/dialysis/sessions/${id}`, data),
  deleteSession: (id: number) => api.delete(`/api/ClinicalVisitsApi/dialysis/sessions/${id}`),

  // Statistics
  getStatistics: () => api.get('/api/ClinicalVisitsApi/dialysis/statistics'),
};

// ============================================
// OBGYN API ENDPOINTS
// ============================================

export const obgynApi = {
  // Prenatal Visits
  getPrenatalVisitsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/obgyn/prenatal/patient/${patientId}`),
  getAllPrenatalVisits: () => api.get('/api/ClinicalVisitsApi/obgyn/prenatal'),
  createPrenatalVisit: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/obgyn/prenatal', data),
  updatePrenatalVisit: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/obgyn/prenatal/${id}`, data),
  deletePrenatalVisit: (id: number) => api.delete(`/api/ClinicalVisitsApi/obgyn/prenatal/${id}`),

  // Ultrasounds
  getUltrasoundsByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/obgyn/ultrasounds/patient/${patientId}`),
  getAllUltrasounds: () => api.get('/api/ClinicalVisitsApi/obgyn/ultrasounds'),
  createUltrasound: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/obgyn/ultrasounds', data),
  updateUltrasound: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/obgyn/ultrasounds/${id}`, data),
  deleteUltrasound: (id: number) => api.delete(`/api/ClinicalVisitsApi/obgyn/ultrasounds/${id}`),

  // Statistics
  getStatistics: () => api.get('/api/ClinicalVisitsApi/obgyn/statistics'),
};

// ============================================
// ONCOLOGY API ENDPOINTS
// ============================================

export const oncologyApi = {
  // Treatment Plans
  getTreatmentPlansByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/oncology/treatment-plans/patient/${patientId}`),
  getAllTreatmentPlans: () => api.get('/api/ClinicalVisitsApi/oncology/treatment-plans'),
  createTreatmentPlan: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/oncology/treatment-plans', data),
  updateTreatmentPlan: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/oncology/treatment-plans/${id}`, data),
  deleteTreatmentPlan: (id: number) => api.delete(`/api/ClinicalVisitsApi/oncology/treatment-plans/${id}`),

  // Diagnoses
  getDiagnosesByPatient: (patientId: number) => api.get(`/api/ClinicalVisitsApi/oncology/diagnoses/patient/${patientId}`),
  getAllDiagnoses: () => api.get('/api/ClinicalVisitsApi/oncology/diagnoses'),
  createDiagnosis: (data: Record<string, unknown>) => api.post('/api/ClinicalVisitsApi/oncology/diagnoses', data),
  updateDiagnosis: (id: number, data: Record<string, unknown>) => api.put(`/api/ClinicalVisitsApi/oncology/diagnoses/${id}`, data),
  deleteDiagnosis: (id: number) => api.delete(`/api/ClinicalVisitsApi/oncology/diagnoses/${id}`),

  // Statistics
  getStatistics: () => api.get('/api/ClinicalVisitsApi/oncology/statistics'),
};

// ============================================
// WORKFLOW API ENDPOINTS
// ============================================

export const workflowDefinitionsApi = {
  getAll: () => api.get('/api/WorkflowApi/definitions'),
  getById: (id: number) => api.get(`/api/WorkflowApi/definitions/${id}`),
  search: (searchTerm: string) =>
    api.get(`/api/WorkflowApi/definitions/search?searchTerm=${encodeURIComponent(searchTerm)}`),
  getActive: () => api.get('/api/WorkflowApi/definitions/active'),
  getByCategory: (category: string) =>
    api.get(`/api/WorkflowApi/definitions/category/${encodeURIComponent(category)}`),
  create: (data: CreateWorkflowDefinitionRequest) => api.post('/api/WorkflowApi/definitions', data),
  update: (id: number, data: UpdateWorkflowDefinitionRequest) => api.put(`/api/WorkflowApi/definitions/${id}`, data),
  delete: (id: number) => api.delete(`/api/WorkflowApi/definitions/${id}`),
  toggleActive: (id: number, isActive: boolean) =>
    api.post(`/api/WorkflowApi/definitions/${id}/toggle-active`, { isActive }),
  getStatistics: () => api.get('/api/WorkflowApi/definitions/statistics'),
};

export const workflowInstancesApi = {
  getAll: () => api.get('/api/WorkflowApi/instances'),
  getById: (id: number) => api.get(`/api/WorkflowApi/instances/${id}`),
  getByDefinition: (definitionId: number) =>
    api.get(`/api/WorkflowApi/instances/definition/${definitionId}`),
  getByEntity: (entityType: string, entityId: number) =>
    api.get(`/api/WorkflowApi/instances/entity/${entityType}/${entityId}`),
  getByStatus: (status: number) => api.get(`/api/WorkflowApi/instances/status/${status}`),
  getPending: () => api.get('/api/WorkflowApi/instances/pending'),
  getInProgress: () => api.get('/api/WorkflowApi/instances/in-progress'),
  create: (data: CreateWorkflowInstanceRequest) => api.post('/api/WorkflowApi/instances', data),
  update: (id: number, data: UpdateWorkflowInstanceRequest) => api.put(`/api/WorkflowApi/instances/${id}`, data),
  cancel: (id: number, reason?: string) =>
    api.post(`/api/WorkflowApi/instances/${id}/cancel`, { reason }),
  complete: (id: number) => api.post(`/api/WorkflowApi/instances/${id}/complete`),
  getHistory: (id: number) => api.get(`/api/WorkflowApi/instances/${id}/history`),
  addHistoryEntry: (id: number, data: { stepId: string; action: string; notes?: string }) =>
    api.post(`/api/WorkflowApi/instances/${id}/history`, data),
  getStatistics: () => api.get('/api/WorkflowApi/instances/statistics'),
};

// ============================================
// ANALYTICS API ENDPOINTS
// ============================================

export const analyticsApi = {
  // Dashboard
  getDashboard: (startDate?: string, endDate?: string, departmentId?: number) =>
    api.get('/api/AnalyticsApi/dashboard', { params: { startDate, endDate, departmentId } }),

  // Reports
  getAllReports: () => api.get('/api/AnalyticsApi/reports'),
  getReportById: (id: number) => api.get(`/api/AnalyticsApi/reports/${id}`),
  getRecentReports: (limit?: number) => api.get('/api/AnalyticsApi/reports/recent', { params: { limit } }),
  getReportsByType: (type: number) => api.get(`/api/AnalyticsApi/reports/type/${type}`),
  getReportsByStatus: (status: number) => api.get(`/api/AnalyticsApi/reports/status/${status}`),
  createReport: (data: CreateReportRequest) => api.post('/api/AnalyticsApi/reports', data),
  updateReport: (id: number, data: Partial<CreateReportRequest>) => api.put(`/api/AnalyticsApi/reports/${id}`, data),
  deleteReport: (id: number) => api.delete(`/api/AnalyticsApi/reports/${id}`),
  downloadReport: (id: number) =>
    api.get(`/api/AnalyticsApi/reports/${id}/download`, { responseType: 'blob' }),
  regenerateReport: (id: number) => api.post(`/api/AnalyticsApi/reports/${id}/regenerate`),

  // Statistics
  getStatistics: () => api.get('/api/AnalyticsApi/statistics'),
  getKPIs: (startDate?: string, endDate?: string) =>
    api.get('/api/AnalyticsApi/kpis', { params: { startDate, endDate } }),

  // Custom Analytics
  getPatientAnalytics: (startDate?: string, endDate?: string) =>
    api.get('/api/AnalyticsApi/patients', { params: { startDate, endDate } }),
  getAppointmentAnalytics: (startDate?: string, endDate?: string) =>
    api.get('/api/AnalyticsApi/appointments', { params: { startDate, endDate } }),
  getRevenueAnalytics: (startDate?: string, endDate?: string) =>
    api.get('/api/AnalyticsApi/revenue', { params: { startDate, endDate } }),
  getDepartmentAnalytics: (departmentId?: number, startDate?: string, endDate?: string) =>
    api.get('/api/AnalyticsApi/departments', { params: { departmentId, startDate, endDate } }),
};

export default api;
