import axios from "axios";
import {
  configureAxiosInstance,
  tokenStorage,
  getAxiosErrorMessage,
} from "@xenon/ui";

// API Base URL - adjust based on environment
const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:5001";

// Create axios instance with shared configuration
export const api = configureAxiosInstance(axios.create(), {
  baseURL: API_BASE_URL,
  withCredentials: false,
  onUnauthorized: () => {
    window.location.href = "/login";
  },
});

// Re-export utilities for use in auth contexts
export { tokenStorage, getAxiosErrorMessage };

// API endpoints
export const authApi = {
  login: (username: string, password: string) =>
    api.post("/api/AuthApi/login", { username, password }),

  register: (
    username: string,
    email: string,
    fullName: string,
    password: string,
  ) =>
    api.post("/api/AuthApi/register", { username, email, fullName, password }),

  getCurrentUser: () => api.get("/api/AuthApi/me"),

  refreshToken: () => api.post("/api/AuthApi/refresh"),
};

export const appointmentsApi = {
  getAll: () => api.get("/api/AppointmentsApi"),
  getById: (id: number) => api.get(`/api/AppointmentsApi/${id}`),
  getByDate: (date: Date) => {
    const dateStr = date.toISOString().split("T")[0];
    return api.get(`/api/AppointmentsApi/date/${dateStr}`);
  },
  getToday: () => api.get("/api/AppointmentsApi/today"),
  getUpcoming: (days: number = 7) =>
    api.get(`/api/AppointmentsApi/upcoming?days=${days}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/AppointmentsApi", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AppointmentsApi/${id}`, data),
  delete: (id: number) => api.delete(`/api/AppointmentsApi/${id}`),
  confirm: (id: number) => api.post(`/api/AppointmentsApi/${id}/confirm`),
  cancel: (id: number, reason?: string) =>
    api.post(`/api/AppointmentsApi/${id}/cancel`, { reason }),
  checkIn: (id: number) => api.post(`/api/AppointmentsApi/${id}/checkin`),
  complete: (id: number) => api.post(`/api/AppointmentsApi/${id}/complete`),
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get("/api/AppointmentsApi/statistics", {
      params: { startDate, endDate },
    }),
};

export const patientsApi = {
  getAll: () => api.get("/api/PatientsApi"),
  getById: (id: number) => api.get(`/api/PatientsApi/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/PatientsApi/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getByEmiratesId: (emiratesId: string) =>
    api.get(`/api/PatientsApi/emirates/${encodeURIComponent(emiratesId)}`),
  create: (data: Record<string, unknown>) => api.post("/api/PatientsApi", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PatientsApi/${id}`, data),
  delete: (id: number) => api.delete(`/api/PatientsApi/${id}`),
  getMedicalHistory: (id: number) =>
    api.get(`/api/PatientsApi/${id}/medical-history`),
  getDocuments: (id: number) => api.get(`/api/PatientsApi/${id}/documents`),
  getStatistics: () => api.get("/api/PatientsApi/statistics"),
};

export const laboratoryApi = {
  getAllOrders: () => api.get("/api/LaboratoryApi/orders"),
  getOrderById: (id: number) => api.get(`/api/LaboratoryApi/orders/${id}`),
  getPendingOrders: () => api.get("/api/LaboratoryApi/orders/pending"),
  getUrgentOrders: () => api.get("/api/LaboratoryApi/orders/urgent"),
  getOrdersByPatient: (patientId: number) =>
    api.get(`/api/LaboratoryApi/orders/patient/${patientId}`),
  createOrder: (data: Record<string, unknown>) =>
    api.post("/api/LaboratoryApi/orders", data),
  updateOrder: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/LaboratoryApi/orders/${id}`, data),
  updateStatus: (id: number, status: number) =>
    api.post(`/api/LaboratoryApi/orders/${id}/status`, { status }),
  deleteOrder: (id: number) => api.delete(`/api/LaboratoryApi/orders/${id}`),
  getAllTests: () => api.get("/api/LaboratoryApi/tests"),
  getStatistics: () => api.get("/api/LaboratoryApi/statistics"),
};

export const hrApi = {
  getAll: () => api.get("/api/HRApi/employees"),
  getById: (id: number) => api.get(`/api/HRApi/employees/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/HRApi/employees/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getByDepartment: (department: string) =>
    api.get(
      `/api/HRApi/employees/department/${encodeURIComponent(department)}`,
    ),
  getActive: () => api.get("/api/HRApi/employees/active"),
  create: (data: Record<string, unknown>) =>
    api.post("/api/HRApi/employees", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/HRApi/employees/${id}`, data),
  delete: (id: number) => api.delete(`/api/HRApi/employees/${id}`),
  getStatistics: () => api.get("/api/HRApi/statistics"),
};

export const financialApi = {
  getAllInvoices: () => api.get("/api/FinancialApi/invoices"),
  getById: (id: number) => api.get(`/api/FinancialApi/invoices/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/FinancialApi/invoices/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getUnpaid: () => api.get("/api/FinancialApi/invoices/unpaid"),
  getOverdue: () => api.get("/api/FinancialApi/invoices/overdue"),
  getByPatient: (patientId: number) =>
    api.get(`/api/FinancialApi/invoices/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/FinancialApi/invoices", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/FinancialApi/invoices/${id}`, data),
  delete: (id: number) => api.delete(`/api/FinancialApi/invoices/${id}`),
  recordPayment: (id: number, amount: number, method: number) =>
    api.post(`/api/FinancialApi/invoices/${id}/payment`, { amount, method }),
  getStatistics: () => api.get("/api/FinancialApi/statistics"),
};

// ============================================
// SALES API ENDPOINTS
// ============================================

export const salesApi = {
  // Sales
  getAllSales: () => api.get("/api/SalesApi/sales"),
  getSaleById: (id: number) => api.get(`/api/SalesApi/sales/${id}`),
  getSaleByInvoiceNumber: (invoiceNumber: string) =>
    api.get(`/api/SalesApi/sales/invoice/${encodeURIComponent(invoiceNumber)}`),
  getSalesByPatient: (patientId: number) =>
    api.get(`/api/SalesApi/sales/patient/${patientId}`),
  getSalesByStatus: (status: number) =>
    api.get(`/api/SalesApi/sales/status/${status}`),
  getSalesByPaymentStatus: (status: number) =>
    api.get(`/api/SalesApi/sales/payment-status/${status}`),
  getSalesByDateRange: (startDate: string, endDate: string) =>
    api.get("/api/SalesApi/sales/date-range", {
      params: { startDate, endDate },
    }),
  getOverdueSales: () => api.get("/api/SalesApi/sales/overdue"),
  createSale: (data: Record<string, unknown>) =>
    api.post("/api/SalesApi/sales", data),
  updateSale: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/SalesApi/sales/${id}`, data),
  deleteSale: (id: number) => api.delete(`/api/SalesApi/sales/${id}`),
  confirmSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/confirm`),
  completeSale: (id: number) => api.post(`/api/SalesApi/sales/${id}/complete`),
  cancelSale: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/sales/${id}/cancel`, { reason }),

  // Sale Items
  getSaleItems: (saleId: number) =>
    api.get(`/api/SalesApi/sales/${saleId}/items`),
  addSaleItem: (saleId: number, data: Record<string, unknown>) =>
    api.post(`/api/SalesApi/sales/${saleId}/items`, data),
  updateSaleItem: (
    saleId: number,
    itemId: number,
    data: Record<string, unknown>,
  ) => api.put(`/api/SalesApi/sales/${saleId}/items/${itemId}`, data),
  deleteSaleItem: (saleId: number, itemId: number) =>
    api.delete(`/api/SalesApi/sales/${saleId}/items/${itemId}`),

  // Payments
  getPaymentsBySale: (saleId: number) =>
    api.get(`/api/SalesApi/sales/${saleId}/payments`),
  recordPayment: (saleId: number, data: Record<string, unknown>) =>
    api.post(`/api/SalesApi/sales/${saleId}/payments`, data),
  getPaymentById: (id: number) => api.get(`/api/SalesApi/payments/${id}`),
  updatePayment: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/SalesApi/payments/${id}`, data),
  deletePayment: (id: number) => api.delete(`/api/SalesApi/payments/${id}`),
  refundPayment: (id: number, amount: number, reason?: string) =>
    api.post(`/api/SalesApi/payments/${id}/refund`, { amount, reason }),

  // Statistics
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get("/api/SalesApi/statistics", { params: { startDate, endDate } }),
};

export const quotationsApi = {
  // Quotations
  getAll: () => api.get("/api/SalesApi/quotations"),
  getById: (id: number) => api.get(`/api/SalesApi/quotations/${id}`),
  getByNumber: (quotationNumber: string) =>
    api.get(
      `/api/SalesApi/quotations/number/${encodeURIComponent(quotationNumber)}`,
    ),
  getByPatient: (patientId: number) =>
    api.get(`/api/SalesApi/quotations/patient/${patientId}`),
  getByStatus: (status: number) =>
    api.get(`/api/SalesApi/quotations/status/${status}`),
  getActive: () => api.get("/api/SalesApi/quotations/active"),
  getExpired: () => api.get("/api/SalesApi/quotations/expired"),
  create: (data: Record<string, unknown>) =>
    api.post("/api/SalesApi/quotations", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/SalesApi/quotations/${id}`, data),
  delete: (id: number) => api.delete(`/api/SalesApi/quotations/${id}`),
  send: (id: number) => api.post(`/api/SalesApi/quotations/${id}/send`),
  accept: (id: number) => api.post(`/api/SalesApi/quotations/${id}/accept`),
  reject: (id: number, reason?: string) =>
    api.post(`/api/SalesApi/quotations/${id}/reject`, { reason }),
  convertToSale: (id: number) =>
    api.post(`/api/SalesApi/quotations/${id}/convert`),

  // Quotation Items
  getItems: (quotationId: number) =>
    api.get(`/api/SalesApi/quotations/${quotationId}/items`),
  addItem: (quotationId: number, data: Record<string, unknown>) =>
    api.post(`/api/SalesApi/quotations/${quotationId}/items`, data),
  updateItem: (
    quotationId: number,
    itemId: number,
    data: Record<string, unknown>,
  ) => api.put(`/api/SalesApi/quotations/${quotationId}/items/${itemId}`, data),
  deleteItem: (quotationId: number, itemId: number) =>
    api.delete(`/api/SalesApi/quotations/${quotationId}/items/${itemId}`),
};

export const inventoryApi = {
  getAllItems: () => api.get("/api/InventoryApi/items"),
  getById: (id: number) => api.get(`/api/InventoryApi/items/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/InventoryApi/items/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getLowStock: () => api.get("/api/InventoryApi/items/low-stock"),
  getOutOfStock: () => api.get("/api/InventoryApi/items/out-of-stock"),
  getByCategory: (category: number) =>
    api.get(`/api/InventoryApi/items/category/${category}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/InventoryApi/items", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/InventoryApi/items/${id}`, data),
  delete: (id: number) => api.delete(`/api/InventoryApi/items/${id}`),
  adjustStock: (id: number, quantity: number, reason: string) =>
    api.post(`/api/InventoryApi/items/${id}/adjust`, { quantity, reason }),
  getStatistics: () => api.get("/api/InventoryApi/statistics"),
};

export const pharmacyApi = {
  getAllPrescriptions: () => api.get("/api/PharmacyApi/prescriptions"),
  getById: (id: number) => api.get(`/api/PharmacyApi/prescriptions/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/PharmacyApi/prescriptions/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getPending: () => api.get("/api/PharmacyApi/prescriptions/pending"),
  getByPatient: (patientId: number) =>
    api.get(`/api/PharmacyApi/prescriptions/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PharmacyApi/prescriptions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PharmacyApi/prescriptions/${id}`, data),
  delete: (id: number) => api.delete(`/api/PharmacyApi/prescriptions/${id}`),
  dispense: (id: number, dispensedBy: string) =>
    api.post(`/api/PharmacyApi/prescriptions/${id}/dispense`, { dispensedBy }),
  getStatistics: () => api.get("/api/PharmacyApi/statistics"),
};

export const radiologyApi = {
  getAllOrders: () => api.get("/api/RadiologyApi/orders"),
  getById: (id: number) => api.get(`/api/RadiologyApi/orders/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/RadiologyApi/orders/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getPending: () => api.get("/api/RadiologyApi/orders/pending"),
  getScheduled: () => api.get("/api/RadiologyApi/orders/scheduled"),
  getByPatient: (patientId: number) =>
    api.get(`/api/RadiologyApi/orders/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/RadiologyApi/orders", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/RadiologyApi/orders/${id}`, data),
  delete: (id: number) => api.delete(`/api/RadiologyApi/orders/${id}`),
  updateStatus: (id: number, status: number) =>
    api.post(`/api/RadiologyApi/orders/${id}/status`, { status }),
  getStatistics: () => api.get("/api/RadiologyApi/statistics"),
};

// ============================================
// AUDIOLOGY API ENDPOINTS
// ============================================

export const audiogramApi = {
  getAll: () => api.get("/api/AudiologyApi/audiograms"),
  getById: (id: number) => api.get(`/api/AudiologyApi/audiograms/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/audiograms/patient/${patientId}`),
  getLatestByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/audiograms/patient/${patientId}/latest`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/AudiologyApi/audiograms", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AudiologyApi/audiograms/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/audiograms/${id}`),
  compare: (patientId: number, audiogramIds: number[]) =>
    api.post(`/api/AudiologyApi/audiograms/patient/${patientId}/compare`, {
      audiogramIds,
    }),
};

export const hearingAidApi = {
  getAll: () => api.get("/api/AudiologyApi/hearing-aids"),
  getById: (id: number) => api.get(`/api/AudiologyApi/hearing-aids/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/hearing-aids/patient/${patientId}`),
  getBySerialNumber: (serialNumber: string) =>
    api.get(
      `/api/AudiologyApi/hearing-aids/serial/${encodeURIComponent(serialNumber)}`,
    ),
  getWarrantyExpiring: (days: number = 30) =>
    api.get(`/api/AudiologyApi/hearing-aids/warranty-expiring?days=${days}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/AudiologyApi/hearing-aids", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AudiologyApi/hearing-aids/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/hearing-aids/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/hearing-aids/${id}/status`, { status }),

  // Fittings
  getFittings: (hearingAidId: number) =>
    api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`),
  createFitting: (hearingAidId: number, data: Record<string, unknown>) =>
    api.post(`/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings`, data),
  updateFitting: (
    hearingAidId: number,
    fittingId: number,
    data: Record<string, unknown>,
  ) =>
    api.put(
      `/api/AudiologyApi/hearing-aids/${hearingAidId}/fittings/${fittingId}`,
      data,
    ),

  // Adjustments
  getAdjustments: (hearingAidId: number) =>
    api.get(`/api/AudiologyApi/hearing-aids/${hearingAidId}/adjustments`),
  createAdjustment: (hearingAidId: number, data: Record<string, unknown>) =>
    api.post(
      `/api/AudiologyApi/hearing-aids/${hearingAidId}/adjustments`,
      data,
    ),
};

export const encounterApi = {
  getAll: () => api.get("/api/AudiologyApi/encounters"),
  getById: (id: number) => api.get(`/api/AudiologyApi/encounters/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/encounters/patient/${patientId}`),
  getToday: () => api.get("/api/AudiologyApi/encounters/today"),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get("/api/AudiologyApi/encounters", { params: { startDate, endDate } }),
  getByStatus: (status: string) =>
    api.get(`/api/AudiologyApi/encounters/status/${status}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/AudiologyApi/encounters", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AudiologyApi/encounters/${id}`, data),
  delete: (id: number) => api.delete(`/api/AudiologyApi/encounters/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/AudiologyApi/encounters/${id}/status`, { status }),
  complete: (id: number) =>
    api.post(`/api/AudiologyApi/encounters/${id}/complete`),

  // Tasks
  getTasks: (encounterId: number) =>
    api.get(`/api/AudiologyApi/encounters/${encounterId}/tasks`),
  createTask: (encounterId: number, data: Record<string, unknown>) =>
    api.post(`/api/AudiologyApi/encounters/${encounterId}/tasks`, data),
  updateTask: (
    encounterId: number,
    taskId: number,
    data: Record<string, unknown>,
  ) =>
    api.put(
      `/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}`,
      data,
    ),
  completeTask: (encounterId: number, taskId: number) =>
    api.post(
      `/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}/complete`,
    ),
  deleteTask: (encounterId: number, taskId: number) =>
    api.delete(`/api/AudiologyApi/encounters/${encounterId}/tasks/${taskId}`),

  // All tasks (across encounters)
  getAllPendingTasks: () => api.get("/api/AudiologyApi/tasks/pending"),
  getOverdueTasks: () => api.get("/api/AudiologyApi/tasks/overdue"),
  getTasksByAssignee: (assignee: string) =>
    api.get(`/api/AudiologyApi/tasks/assignee/${encodeURIComponent(assignee)}`),
};

export const consentApi = {
  getAll: () => api.get("/api/AudiologyApi/consents"),
  getById: (id: number) => api.get(`/api/AudiologyApi/consents/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/consents/patient/${patientId}`),
  getPending: (patientId: number) =>
    api.get(`/api/AudiologyApi/consents/patient/${patientId}/pending`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/AudiologyApi/consents", data),
  sign: (id: number, data: Record<string, unknown>) =>
    api.post(`/api/AudiologyApi/consents/${id}/sign`, data),
  revoke: (id: number, reason: string) =>
    api.post(`/api/AudiologyApi/consents/${id}/revoke`, { reason }),
  delete: (id: number) => api.delete(`/api/AudiologyApi/consents/${id}`),
  getFormTemplate: (consentType: string) =>
    api.get(`/api/AudiologyApi/consents/templates/${consentType}`),
};

export const attachmentApi = {
  getByPatient: (patientId: number) =>
    api.get(`/api/AudiologyApi/attachments/patient/${patientId}`),
  getByEncounter: (encounterId: number) =>
    api.get(`/api/AudiologyApi/attachments/encounter/${encounterId}`),
  getById: (id: number) => api.get(`/api/AudiologyApi/attachments/${id}`),
  upload: (data: FormData) =>
    api.post("/api/AudiologyApi/attachments", data, {
      headers: { "Content-Type": "multipart/form-data" },
    }),
  download: (id: number) =>
    api.get(`/api/AudiologyApi/attachments/${id}/download`, {
      responseType: "blob",
    }),
  delete: (id: number) => api.delete(`/api/AudiologyApi/attachments/${id}`),
  updateMetadata: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AudiologyApi/attachments/${id}`, data),
};

export const audiologyStatsApi = {
  getDashboard: () => api.get("/api/AudiologyApi/statistics/dashboard"),
  getEncounterStats: (startDate?: string, endDate?: string) =>
    api.get("/api/AudiologyApi/statistics/encounters", {
      params: { startDate, endDate },
    }),
  getHearingAidStats: () =>
    api.get("/api/AudiologyApi/statistics/hearing-aids"),
  getPatientStats: () => api.get("/api/AudiologyApi/statistics/patients"),
};

// ============================================
// MARKETING API ENDPOINTS
// ============================================

export const campaignApi = {
  getAll: () => api.get("/api/MarketingApi/campaigns"),
  getById: (id: number) => api.get(`/api/MarketingApi/campaigns/${id}`),
  getActive: () => api.get("/api/MarketingApi/campaigns/active"),
  getByStatus: (status: string) =>
    api.get(`/api/MarketingApi/campaigns/status/${status}`),
  getByType: (type: string) =>
    api.get(`/api/MarketingApi/campaigns/type/${type}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/MarketingApi/campaigns/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  create: (data: Record<string, unknown>) =>
    api.post("/api/MarketingApi/campaigns", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/MarketingApi/campaigns/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/campaigns/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/MarketingApi/campaigns/${id}/status`, { status }),
  activate: (id: number) =>
    api.post(`/api/MarketingApi/campaigns/${id}/activate`),
  pause: (id: number) => api.post(`/api/MarketingApi/campaigns/${id}/pause`),
  complete: (id: number) =>
    api.post(`/api/MarketingApi/campaigns/${id}/complete`),
  getPerformance: (id: number) =>
    api.get(`/api/MarketingApi/campaigns/${id}/performance`),
};

export const leadApi = {
  getAll: () => api.get("/api/MarketingApi/leads"),
  getById: (id: number) => api.get(`/api/MarketingApi/leads/${id}`),
  getByStatus: (status: string) =>
    api.get(`/api/MarketingApi/leads/status/${status}`),
  getBySource: (source: string) =>
    api.get(`/api/MarketingApi/leads/source/${source}`),
  getByCampaign: (campaignId: number) =>
    api.get(`/api/MarketingApi/leads/campaign/${campaignId}`),
  getNew: () => api.get("/api/MarketingApi/leads/new"),
  getQualified: () => api.get("/api/MarketingApi/leads/qualified"),
  getNeedingFollowUp: () => api.get("/api/MarketingApi/leads/follow-up"),
  getOverdueFollowUps: () => api.get("/api/MarketingApi/leads/overdue"),
  search: (searchTerm: string) =>
    api.get(
      `/api/MarketingApi/leads/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  create: (data: Record<string, unknown>) =>
    api.post("/api/MarketingApi/leads", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/MarketingApi/leads/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/leads/${id}`),
  updateStatus: (id: number, status: string) =>
    api.post(`/api/MarketingApi/leads/${id}/status`, { status }),
  convert: (id: number, patientData?: Record<string, unknown>) =>
    api.post(`/api/MarketingApi/leads/${id}/convert`, patientData),
  markAsLost: (id: number, reason: string) =>
    api.post(`/api/MarketingApi/leads/${id}/lost`, { reason }),
  logContact: (id: number, notes?: string) =>
    api.post(`/api/MarketingApi/leads/${id}/contact`, { notes }),
  scheduleFollowUp: (id: number, date: string, notes?: string) =>
    api.post(`/api/MarketingApi/leads/${id}/schedule-follow-up`, {
      date,
      notes,
    }),
  assign: (id: number, userId: string) =>
    api.post(`/api/MarketingApi/leads/${id}/assign`, { userId }),
};

export const marketingActivityApi = {
  getAll: () => api.get("/api/MarketingApi/activities"),
  getById: (id: number) => api.get(`/api/MarketingApi/activities/${id}`),
  getByLead: (leadId: number) =>
    api.get(`/api/MarketingApi/activities/lead/${leadId}`),
  getByCampaign: (campaignId: number) =>
    api.get(`/api/MarketingApi/activities/campaign/${campaignId}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/MarketingApi/activities/patient/${patientId}`),
  getToday: () => api.get("/api/MarketingApi/activities/today"),
  getScheduled: () => api.get("/api/MarketingApi/activities/scheduled"),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get("/api/MarketingApi/activities", { params: { startDate, endDate } }),
  create: (data: Record<string, unknown>) =>
    api.post("/api/MarketingApi/activities", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/MarketingApi/activities/${id}`, data),
  delete: (id: number) => api.delete(`/api/MarketingApi/activities/${id}`),
  complete: (id: number, outcome?: string) =>
    api.post(`/api/MarketingApi/activities/${id}/complete`, { outcome }),
  cancel: (id: number, reason?: string) =>
    api.post(`/api/MarketingApi/activities/${id}/cancel`, { reason }),
};

export const marketingStatsApi = {
  getDashboard: () => api.get("/api/MarketingApi/statistics/dashboard"),
  getCampaignStats: (startDate?: string, endDate?: string) =>
    api.get("/api/MarketingApi/statistics/campaigns", {
      params: { startDate, endDate },
    }),
  getLeadStats: () => api.get("/api/MarketingApi/statistics/leads"),
  getLeadFunnel: () => api.get("/api/MarketingApi/statistics/funnel"),
  getLeadTrends: (days?: number) =>
    api.get("/api/MarketingApi/statistics/trends", { params: { days } }),
  getConversionRates: () =>
    api.get("/api/MarketingApi/statistics/conversion-rates"),
  getROIReport: (startDate?: string, endDate?: string) =>
    api.get("/api/MarketingApi/statistics/roi", {
      params: { startDate, endDate },
    }),
  getCampaignPerformance: () =>
    api.get("/api/MarketingApi/statistics/campaign-performance"),
};

// ============================================
// CLINICAL VISITS API ENDPOINTS
// ============================================

export const clinicalVisitsApi = {
  getAll: () => api.get("/api/ClinicalVisitsApi/visits"),
  getById: (id: number) => api.get(`/api/ClinicalVisitsApi/visits/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/ClinicalVisitsApi/visits/patient/${patientId}`),
  getByDoctor: (doctorId: number) =>
    api.get(`/api/ClinicalVisitsApi/visits/doctor/${doctorId}`),
  getToday: () => api.get("/api/ClinicalVisitsApi/visits/today"),
  getByStatus: (status: number) =>
    api.get(`/api/ClinicalVisitsApi/visits/status/${status}`),
  getByType: (type: number) =>
    api.get(`/api/ClinicalVisitsApi/visits/type/${type}`),
  getByDateRange: (startDate: string, endDate: string) =>
    api.get("/api/ClinicalVisitsApi/visits/date-range", {
      params: { startDate, endDate },
    }),
  search: (searchTerm: string) =>
    api.get(
      `/api/ClinicalVisitsApi/visits/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  create: (data: Record<string, unknown>) =>
    api.post("/api/ClinicalVisitsApi/visits", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/ClinicalVisitsApi/visits/${id}`, data),
  delete: (id: number) => api.delete(`/api/ClinicalVisitsApi/visits/${id}`),
  updateStatus: (id: number, status: number) =>
    api.post(`/api/ClinicalVisitsApi/visits/${id}/status`, { status }),
  getStatistics: (startDate?: string, endDate?: string) =>
    api.get("/api/ClinicalVisitsApi/statistics", {
      params: { startDate, endDate },
    }),
};

// ============================================
// WORKFLOW API ENDPOINTS
// ============================================

export const workflowDefinitionsApi = {
  getAll: () => api.get("/api/WorkflowApi/definitions"),
  getById: (id: number) => api.get(`/api/WorkflowApi/definitions/${id}`),
  search: (searchTerm: string) =>
    api.get(
      `/api/WorkflowApi/definitions/search?searchTerm=${encodeURIComponent(searchTerm)}`,
    ),
  getActive: () => api.get("/api/WorkflowApi/definitions/active"),
  getByCategory: (category: string) =>
    api.get(
      `/api/WorkflowApi/definitions/category/${encodeURIComponent(category)}`,
    ),
  create: (data: Record<string, unknown>) =>
    api.post("/api/WorkflowApi/definitions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/WorkflowApi/definitions/${id}`, data),
  delete: (id: number) => api.delete(`/api/WorkflowApi/definitions/${id}`),
  toggleActive: (id: number, isActive: boolean) =>
    api.post(`/api/WorkflowApi/definitions/${id}/toggle-active`, { isActive }),
  getStatistics: () => api.get("/api/WorkflowApi/definitions/statistics"),
};

export const workflowInstancesApi = {
  getAll: () => api.get("/api/WorkflowApi/instances"),
  getById: (id: number) => api.get(`/api/WorkflowApi/instances/${id}`),
  getByDefinition: (definitionId: number) =>
    api.get(`/api/WorkflowApi/instances/definition/${definitionId}`),
  getByEntity: (entityType: string, entityId: number) =>
    api.get(`/api/WorkflowApi/instances/entity/${entityType}/${entityId}`),
  getByStatus: (status: number) =>
    api.get(`/api/WorkflowApi/instances/status/${status}`),
  getPending: () => api.get("/api/WorkflowApi/instances/pending"),
  getInProgress: () => api.get("/api/WorkflowApi/instances/in-progress"),
  create: (data: Record<string, unknown>) =>
    api.post("/api/WorkflowApi/instances", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/WorkflowApi/instances/${id}`, data),
  cancel: (id: number, reason?: string) =>
    api.post(`/api/WorkflowApi/instances/${id}/cancel`, { reason }),
  complete: (id: number) =>
    api.post(`/api/WorkflowApi/instances/${id}/complete`),
  getHistory: (id: number) =>
    api.get(`/api/WorkflowApi/instances/${id}/history`),
  addHistoryEntry: (id: number, data: Record<string, unknown>) =>
    api.post(`/api/WorkflowApi/instances/${id}/history`, data),
  getStatistics: () => api.get("/api/WorkflowApi/instances/statistics"),
};

// ============================================
// ANALYTICS API ENDPOINTS
// ============================================

export const analyticsApi = {
  // Dashboard
  getDashboard: (startDate?: string, endDate?: string, departmentId?: number) =>
    api.get("/api/AnalyticsApi/dashboard", {
      params: { startDate, endDate, departmentId },
    }),

  // Reports
  getAllReports: () => api.get("/api/AnalyticsApi/reports"),
  getReportById: (id: number) => api.get(`/api/AnalyticsApi/reports/${id}`),
  getRecentReports: (limit?: number) =>
    api.get("/api/AnalyticsApi/reports/recent", { params: { limit } }),
  getReportsByType: (type: number) =>
    api.get(`/api/AnalyticsApi/reports/type/${type}`),
  getReportsByStatus: (status: number) =>
    api.get(`/api/AnalyticsApi/reports/status/${status}`),
  createReport: (data: Record<string, unknown>) =>
    api.post("/api/AnalyticsApi/reports", data),
  updateReport: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/AnalyticsApi/reports/${id}`, data),
  deleteReport: (id: number) => api.delete(`/api/AnalyticsApi/reports/${id}`),
  downloadReport: (id: number) =>
    api.get(`/api/AnalyticsApi/reports/${id}/download`, {
      responseType: "blob",
    }),
  regenerateReport: (id: number) =>
    api.post(`/api/AnalyticsApi/reports/${id}/regenerate`),

  // Statistics
  getStatistics: () => api.get("/api/AnalyticsApi/statistics"),
  getKPIs: (startDate?: string, endDate?: string) =>
    api.get("/api/AnalyticsApi/kpis", { params: { startDate, endDate } }),

  // Custom Analytics
  getPatientAnalytics: (startDate?: string, endDate?: string) =>
    api.get("/api/AnalyticsApi/patients", { params: { startDate, endDate } }),
  getAppointmentAnalytics: (startDate?: string, endDate?: string) =>
    api.get("/api/AnalyticsApi/appointments", {
      params: { startDate, endDate },
    }),
  getRevenueAnalytics: (startDate?: string, endDate?: string) =>
    api.get("/api/AnalyticsApi/revenue", { params: { startDate, endDate } }),
  getDepartmentAnalytics: (
    departmentId?: number,
    startDate?: string,
    endDate?: string,
  ) =>
    api.get("/api/AnalyticsApi/departments", {
      params: { departmentId, startDate, endDate },
    }),
};

// ============================================
// ONCOLOGY API ENDPOINTS
// ============================================

export const oncologyDiagnosisApi = {
  getAll: () => api.get("/api/OncologyApi/diagnoses"),
  getById: (id: number) => api.get(`/api/OncologyApi/diagnoses/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OncologyApi/diagnoses/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OncologyApi/diagnoses", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OncologyApi/diagnoses/${id}`, data),
  delete: (id: number) => api.delete(`/api/OncologyApi/diagnoses/${id}`),
  getStatistics: () => api.get("/api/OncologyApi/diagnoses/statistics"),
};

export const oncologyTreatmentPlanApi = {
  getAll: () => api.get("/api/OncologyApi/treatment-plans"),
  getById: (id: number) => api.get(`/api/OncologyApi/treatment-plans/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OncologyApi/treatment-plans/patient/${patientId}`),
  getByDiagnosis: (diagnosisId: number) =>
    api.get(`/api/OncologyApi/treatment-plans/diagnosis/${diagnosisId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OncologyApi/treatment-plans", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OncologyApi/treatment-plans/${id}`, data),
  delete: (id: number) => api.delete(`/api/OncologyApi/treatment-plans/${id}`),
};

export const oncologyChemotherapyApi = {
  getAll: () => api.get("/api/OncologyApi/chemotherapy-sessions"),
  getById: (id: number) =>
    api.get(`/api/OncologyApi/chemotherapy-sessions/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OncologyApi/chemotherapy-sessions/patient/${patientId}`),
  getByTreatmentPlan: (planId: number) =>
    api.get(`/api/OncologyApi/chemotherapy-sessions/plan/${planId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OncologyApi/chemotherapy-sessions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OncologyApi/chemotherapy-sessions/${id}`, data),
  delete: (id: number) =>
    api.delete(`/api/OncologyApi/chemotherapy-sessions/${id}`),
};

export const oncologyStatsApi = {
  getDashboard: () => api.get("/api/OncologyApi/statistics/dashboard"),
  getRecentActivity: () => api.get("/api/OncologyApi/statistics/activity"),
};

// ============================================
// DENTAL API ENDPOINTS
// ============================================

export const dentalTreatmentApi = {
  getAll: () => api.get("/api/DentalApi/treatments"),
  getById: (id: number) => api.get(`/api/DentalApi/treatments/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DentalApi/treatments/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DentalApi/treatments", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DentalApi/treatments/${id}`, data),
  delete: (id: number) => api.delete(`/api/DentalApi/treatments/${id}`),
};

export const dentalChartApi = {
  getAll: () => api.get("/api/DentalApi/charts"),
  getById: (id: number) => api.get(`/api/DentalApi/charts/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DentalApi/charts/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DentalApi/charts", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DentalApi/charts/${id}`, data),
  delete: (id: number) => api.delete(`/api/DentalApi/charts/${id}`),
};

export const dentalPeriodontalApi = {
  getAll: () => api.get("/api/DentalApi/periodontal-exams"),
  getById: (id: number) => api.get(`/api/DentalApi/periodontal-exams/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DentalApi/periodontal-exams/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DentalApi/periodontal-exams", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DentalApi/periodontal-exams/${id}`, data),
  delete: (id: number) => api.delete(`/api/DentalApi/periodontal-exams/${id}`),
};

export const dentalStatsApi = {
  getDashboard: () => api.get("/api/DentalApi/statistics/dashboard"),
};

// ============================================
// DERMATOLOGY API ENDPOINTS
// ============================================

export const dermatologyApi = {
  getDashboard: () => api.get("/api/DermatologyApi/dashboard"),
  getStatistics: () => api.get("/api/DermatologyApi/statistics"),
};

export const skinExamsApi = {
  getAll: () => api.get("/api/DermatologyApi/skin-exams"),
  getById: (id: number) => api.get(`/api/DermatologyApi/skin-exams/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DermatologyApi/skin-exams/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DermatologyApi/skin-exams", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DermatologyApi/skin-exams/${id}`, data),
  delete: (id: number) => api.delete(`/api/DermatologyApi/skin-exams/${id}`),
};

export const skinPhotosApi = {
  getAll: () => api.get("/api/DermatologyApi/skin-photos"),
  getById: (id: number) => api.get(`/api/DermatologyApi/skin-photos/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DermatologyApi/skin-photos/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DermatologyApi/skin-photos", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DermatologyApi/skin-photos/${id}`, data),
  delete: (id: number) => api.delete(`/api/DermatologyApi/skin-photos/${id}`),
};

export const moleMappingsApi = {
  getAll: () => api.get("/api/DermatologyApi/mole-mappings"),
  getById: (id: number) => api.get(`/api/DermatologyApi/mole-mappings/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DermatologyApi/mole-mappings/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DermatologyApi/mole-mappings", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DermatologyApi/mole-mappings/${id}`, data),
  delete: (id: number) => api.delete(`/api/DermatologyApi/mole-mappings/${id}`),
};

export const biopsiesApi = {
  getAll: () => api.get("/api/DermatologyApi/biopsies"),
  getById: (id: number) => api.get(`/api/DermatologyApi/biopsies/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DermatologyApi/biopsies/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DermatologyApi/biopsies", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DermatologyApi/biopsies/${id}`, data),
  delete: (id: number) => api.delete(`/api/DermatologyApi/biopsies/${id}`),
};

// ============================================
// OPHTHALMOLOGY API ENDPOINTS
// ============================================

export const ophthalmologyApi = {
  getDashboard: () => api.get("/api/OphthalmologyApi/dashboard"),
  getStatistics: () => api.get("/api/OphthalmologyApi/statistics"),
};

export const visualAcuityApi = {
  getAll: () => api.get("/api/OphthalmologyApi/visual-acuity"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/visual-acuity/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/visual-acuity/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/visual-acuity", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/visual-acuity/${id}`, data),
  delete: (id: number) =>
    api.delete(`/api/OphthalmologyApi/visual-acuity/${id}`),
};

export const refractionApi = {
  getAll: () => api.get("/api/OphthalmologyApi/refraction"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/refraction/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/refraction/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/refraction", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/refraction/${id}`, data),
  delete: (id: number) => api.delete(`/api/OphthalmologyApi/refraction/${id}`),
};

export const iopApi = {
  getAll: () => api.get("/api/OphthalmologyApi/iop"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/iop/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/iop/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/iop", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/iop/${id}`, data),
  delete: (id: number) => api.delete(`/api/OphthalmologyApi/iop/${id}`),
};

export const slitLampApi = {
  getAll: () => api.get("/api/OphthalmologyApi/slit-lamp"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/slit-lamp/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/slit-lamp/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/slit-lamp", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/slit-lamp/${id}`, data),
  delete: (id: number) => api.delete(`/api/OphthalmologyApi/slit-lamp/${id}`),
};

export const fundusApi = {
  getAll: () => api.get("/api/OphthalmologyApi/fundus"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/fundus/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/fundus/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/fundus", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/fundus/${id}`, data),
  delete: (id: number) => api.delete(`/api/OphthalmologyApi/fundus/${id}`),
};

export const glassesPrescriptionsApi = {
  getAll: () => api.get("/api/OphthalmologyApi/prescriptions"),
  getById: (id: number) => api.get(`/api/OphthalmologyApi/prescriptions/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OphthalmologyApi/prescriptions/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OphthalmologyApi/prescriptions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OphthalmologyApi/prescriptions/${id}`, data),
  delete: (id: number) =>
    api.delete(`/api/OphthalmologyApi/prescriptions/${id}`),
};

// ============================================
// CARDIOLOGY API ENDPOINTS
// ============================================

export const cardiologyApi = {
  getDashboard: () => api.get("/api/CardiologyApi/dashboard"),
  getStatistics: () => api.get("/api/CardiologyApi/statistics"),
};

export const ecgApi = {
  getAll: () => api.get("/api/CardiologyApi/ecg"),
  getById: (id: number) => api.get(`/api/CardiologyApi/ecg/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/CardiologyApi/ecg/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/CardiologyApi/ecg", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/CardiologyApi/ecg/${id}`, data),
  delete: (id: number) => api.delete(`/api/CardiologyApi/ecg/${id}`),
};

export const echoApi = {
  getAll: () => api.get("/api/CardiologyApi/echo"),
  getById: (id: number) => api.get(`/api/CardiologyApi/echo/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/CardiologyApi/echo/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/CardiologyApi/echo", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/CardiologyApi/echo/${id}`, data),
  delete: (id: number) => api.delete(`/api/CardiologyApi/echo/${id}`),
};

export const stressTestsApi = {
  getAll: () => api.get("/api/CardiologyApi/stress-tests"),
  getById: (id: number) => api.get(`/api/CardiologyApi/stress-tests/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/CardiologyApi/stress-tests/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/CardiologyApi/stress-tests", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/CardiologyApi/stress-tests/${id}`, data),
  delete: (id: number) => api.delete(`/api/CardiologyApi/stress-tests/${id}`),
};

export const cathLabApi = {
  getAll: () => api.get("/api/CardiologyApi/cath-lab"),
  getById: (id: number) => api.get(`/api/CardiologyApi/cath-lab/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/CardiologyApi/cath-lab/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/CardiologyApi/cath-lab", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/CardiologyApi/cath-lab/${id}`, data),
  delete: (id: number) => api.delete(`/api/CardiologyApi/cath-lab/${id}`),
};

// ============================================
// NEUROLOGY API ENDPOINTS
// ============================================

export const neurologyApi = {
  getDashboard: () => api.get("/api/NeurologyApi/dashboard"),
  getStatistics: () => api.get("/api/NeurologyApi/statistics"),
};

export const neurologicalExamsApi = {
  getAll: () => api.get("/api/NeurologyApi/exams"),
  getById: (id: number) => api.get(`/api/NeurologyApi/exams/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/NeurologyApi/exams/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/NeurologyApi/exams", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/NeurologyApi/exams/${id}`, data),
  delete: (id: number) => api.delete(`/api/NeurologyApi/exams/${id}`),
};

export const eegApi = {
  getAll: () => api.get("/api/NeurologyApi/eeg"),
  getById: (id: number) => api.get(`/api/NeurologyApi/eeg/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/NeurologyApi/eeg/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/NeurologyApi/eeg", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/NeurologyApi/eeg/${id}`, data),
  delete: (id: number) => api.delete(`/api/NeurologyApi/eeg/${id}`),
};

export const emgApi = {
  getAll: () => api.get("/api/NeurologyApi/emg"),
  getById: (id: number) => api.get(`/api/NeurologyApi/emg/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/NeurologyApi/emg/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/NeurologyApi/emg", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/NeurologyApi/emg/${id}`, data),
  delete: (id: number) => api.delete(`/api/NeurologyApi/emg/${id}`),
};

// ============================================
// OBGYN API ENDPOINTS
// ============================================

export const obgynApi = {
  getDashboard: () => api.get("/api/OBGYNApi/dashboard"),
  getStatistics: () => api.get("/api/OBGYNApi/statistics"),
};

export const pregnanciesApi = {
  getAll: () => api.get("/api/OBGYNApi/pregnancies"),
  getById: (id: number) => api.get(`/api/OBGYNApi/pregnancies/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OBGYNApi/pregnancies/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OBGYNApi/pregnancies", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OBGYNApi/pregnancies/${id}`, data),
  delete: (id: number) => api.delete(`/api/OBGYNApi/pregnancies/${id}`),
};

export const prenatalVisitsApi = {
  getAll: () => api.get("/api/OBGYNApi/prenatal-visits"),
  getById: (id: number) => api.get(`/api/OBGYNApi/prenatal-visits/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OBGYNApi/prenatal-visits/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OBGYNApi/prenatal-visits", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OBGYNApi/prenatal-visits/${id}`, data),
  delete: (id: number) => api.delete(`/api/OBGYNApi/prenatal-visits/${id}`),
};

export const ultrasoundsApi = {
  getAll: () => api.get("/api/OBGYNApi/ultrasounds"),
  getById: (id: number) => api.get(`/api/OBGYNApi/ultrasounds/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OBGYNApi/ultrasounds/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OBGYNApi/ultrasounds", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OBGYNApi/ultrasounds/${id}`, data),
  delete: (id: number) => api.delete(`/api/OBGYNApi/ultrasounds/${id}`),
};

// ============================================
// PEDIATRICS API ENDPOINTS
// ============================================

export const pediatricsApi = {
  getDashboard: () => api.get("/api/PediatricsApi/dashboard"),
  getStatistics: () => api.get("/api/PediatricsApi/statistics"),
};

export const growthChartsApi = {
  getAll: () => api.get("/api/PediatricsApi/growth-charts"),
  getById: (id: number) => api.get(`/api/PediatricsApi/growth-charts/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/PediatricsApi/growth-charts/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PediatricsApi/growth-charts", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PediatricsApi/growth-charts/${id}`, data),
  delete: (id: number) => api.delete(`/api/PediatricsApi/growth-charts/${id}`),
};

export const vaccinationsApi = {
  getAll: () => api.get("/api/PediatricsApi/vaccinations"),
  getById: (id: number) => api.get(`/api/PediatricsApi/vaccinations/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/PediatricsApi/vaccinations/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PediatricsApi/vaccinations", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PediatricsApi/vaccinations/${id}`, data),
  delete: (id: number) => api.delete(`/api/PediatricsApi/vaccinations/${id}`),
};

export const developmentMilestonesApi = {
  getAll: () => api.get("/api/PediatricsApi/milestones"),
  getById: (id: number) => api.get(`/api/PediatricsApi/milestones/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/PediatricsApi/milestones/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PediatricsApi/milestones", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PediatricsApi/milestones/${id}`, data),
  delete: (id: number) => api.delete(`/api/PediatricsApi/milestones/${id}`),
};

// ============================================
// ENT API ENDPOINTS
// ============================================

export const entApi = {
  getDashboard: () => api.get("/api/ENTApi/dashboard"),
  getStatistics: () => api.get("/api/ENTApi/statistics"),
};

export const audiometryApi = {
  getAll: () => api.get("/api/ENTApi/audiometry"),
  getById: (id: number) => api.get(`/api/ENTApi/audiometry/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/ENTApi/audiometry/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/ENTApi/audiometry", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/ENTApi/audiometry/${id}`, data),
  delete: (id: number) => api.delete(`/api/ENTApi/audiometry/${id}`),
};

export const entHearingAidsApi = {
  getAll: () => api.get("/api/ENTApi/hearing-aids"),
  getById: (id: number) => api.get(`/api/ENTApi/hearing-aids/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/ENTApi/hearing-aids/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/ENTApi/hearing-aids", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/ENTApi/hearing-aids/${id}`, data),
  delete: (id: number) => api.delete(`/api/ENTApi/hearing-aids/${id}`),
};

// ============================================
// DIALYSIS API ENDPOINTS
// ============================================

export const dialysisApi = {
  getDashboard: () => api.get("/api/DialysisApi/dashboard"),
  getStatistics: () => api.get("/api/DialysisApi/statistics"),
};

export const dialysisSessionsApi = {
  getAll: () => api.get("/api/DialysisApi/sessions"),
  getById: (id: number) => api.get(`/api/DialysisApi/sessions/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DialysisApi/sessions/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DialysisApi/sessions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DialysisApi/sessions/${id}`, data),
  delete: (id: number) => api.delete(`/api/DialysisApi/sessions/${id}`),
};

export const vascularAccessApi = {
  getAll: () => api.get("/api/DialysisApi/vascular-access"),
  getById: (id: number) => api.get(`/api/DialysisApi/vascular-access/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/DialysisApi/vascular-access/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/DialysisApi/vascular-access", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/DialysisApi/vascular-access/${id}`, data),
  delete: (id: number) => api.delete(`/api/DialysisApi/vascular-access/${id}`),
};

// ============================================
// FERTILITY API ENDPOINTS
// ============================================

export const fertilityApi = {
  getDashboard: () => api.get("/api/FertilityApi/dashboard"),
  getStatistics: () => api.get("/api/FertilityApi/statistics"),
};

export const ivfCyclesApi = {
  getAll: () => api.get("/api/FertilityApi/ivf-cycles"),
  getById: (id: number) => api.get(`/api/FertilityApi/ivf-cycles/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/FertilityApi/ivf-cycles/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/FertilityApi/ivf-cycles", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/FertilityApi/ivf-cycles/${id}`, data),
  delete: (id: number) => api.delete(`/api/FertilityApi/ivf-cycles/${id}`),
};

export const embryoRecordsApi = {
  getAll: () => api.get("/api/FertilityApi/embryos"),
  getById: (id: number) => api.get(`/api/FertilityApi/embryos/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/FertilityApi/embryos/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/FertilityApi/embryos", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/FertilityApi/embryos/${id}`, data),
  delete: (id: number) => api.delete(`/api/FertilityApi/embryos/${id}`),
};

// ============================================
// PHYSIOTHERAPY API ENDPOINTS
// ============================================

export const physiotherapyApi = {
  getDashboard: () => api.get("/api/PhysiotherapyApi/dashboard"),
  getStatistics: () => api.get("/api/PhysiotherapyApi/statistics"),
};

export const therapySessionsApi = {
  getAll: () => api.get("/api/PhysiotherapyApi/sessions"),
  getById: (id: number) => api.get(`/api/PhysiotherapyApi/sessions/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/PhysiotherapyApi/sessions/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PhysiotherapyApi/sessions", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PhysiotherapyApi/sessions/${id}`, data),
  delete: (id: number) => api.delete(`/api/PhysiotherapyApi/sessions/${id}`),
};

export const exerciseProgramsApi = {
  getAll: () => api.get("/api/PhysiotherapyApi/exercise-programs"),
  getById: (id: number) =>
    api.get(`/api/PhysiotherapyApi/exercise-programs/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/PhysiotherapyApi/exercise-programs/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/PhysiotherapyApi/exercise-programs", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/PhysiotherapyApi/exercise-programs/${id}`, data),
  delete: (id: number) =>
    api.delete(`/api/PhysiotherapyApi/exercise-programs/${id}`),
};

// ============================================
// ORTHOPEDICS API ENDPOINTS
// ============================================

export const orthopedicsApi = {
  getDashboard: () => api.get("/api/OrthopedicsApi/dashboard"),
  getStatistics: () => api.get("/api/OrthopedicsApi/statistics"),
};

export const orthopedicExamsApi = {
  getAll: () => api.get("/api/OrthopedicsApi/exams"),
  getById: (id: number) => api.get(`/api/OrthopedicsApi/exams/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OrthopedicsApi/exams/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OrthopedicsApi/exams", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OrthopedicsApi/exams/${id}`, data),
  delete: (id: number) => api.delete(`/api/OrthopedicsApi/exams/${id}`),
};

export const fracturesApi = {
  getAll: () => api.get("/api/OrthopedicsApi/fractures"),
  getById: (id: number) => api.get(`/api/OrthopedicsApi/fractures/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OrthopedicsApi/fractures/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OrthopedicsApi/fractures", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OrthopedicsApi/fractures/${id}`, data),
  delete: (id: number) => api.delete(`/api/OrthopedicsApi/fractures/${id}`),
};

export const surgeriesApi = {
  getAll: () => api.get("/api/OrthopedicsApi/surgeries"),
  getById: (id: number) => api.get(`/api/OrthopedicsApi/surgeries/${id}`),
  getByPatient: (patientId: number) =>
    api.get(`/api/OrthopedicsApi/surgeries/patient/${patientId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/OrthopedicsApi/surgeries", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/OrthopedicsApi/surgeries/${id}`, data),
  delete: (id: number) => api.delete(`/api/OrthopedicsApi/surgeries/${id}`),
};

// ============================================
// HR ADDITIONAL API ENDPOINTS
// ============================================

export const payrollApi = {
  getAll: () => api.get("/api/HRApi/payroll"),
  getById: (id: number) => api.get(`/api/HRApi/payroll/${id}`),
  getByEmployee: (employeeId: number) =>
    api.get(`/api/HRApi/payroll/employee/${employeeId}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/HRApi/payroll", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/HRApi/payroll/${id}`, data),
  delete: (id: number) => api.delete(`/api/HRApi/payroll/${id}`),
  process: (month: number, year: number) =>
    api.post("/api/HRApi/payroll/process", { month, year }),
};

export const salaryStructuresApi = {
  getAll: () => api.get("/api/HRApi/salary-structures"),
  getById: (id: number) => api.get(`/api/HRApi/salary-structures/${id}`),
  create: (data: Record<string, unknown>) =>
    api.post("/api/HRApi/salary-structures", data),
  update: (id: number, data: Record<string, unknown>) =>
    api.put(`/api/HRApi/salary-structures/${id}`, data),
  delete: (id: number) => api.delete(`/api/HRApi/salary-structures/${id}`),
};

// ============================================
// PORTAL API ENDPOINTS
// ============================================

export const portalApi = {
  getDashboard: () => api.get("/api/PortalApi/dashboard"),
  getProfile: () => api.get("/api/PortalApi/profile"),
  updateProfile: (data: Record<string, unknown>) =>
    api.put("/api/PortalApi/profile", data),
  getAppointments: () => api.get("/api/PortalApi/appointments"),
  getDocuments: () => api.get("/api/PortalApi/documents"),
  getDocument: (id: number) => api.get(`/api/PortalApi/documents/${id}`),
};

export default api;
