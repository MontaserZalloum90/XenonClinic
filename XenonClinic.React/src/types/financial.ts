export const InvoiceStatus = {
  Draft: 0,
  Issued: 1,
  PartiallyPaid: 2,
  Paid: 3,
  Overdue: 4,
  Cancelled: 5,
} as const;

export type InvoiceStatus = typeof InvoiceStatus[keyof typeof InvoiceStatus];

export const PaymentMethod = {
  Cash: 0,
  Card: 1,
  BankTransfer: 2,
  Insurance: 3,
  Other: 4,
} as const;

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

export interface Invoice {
  id: number;
  invoiceNumber: string;
  patientId: number;
  patientName?: string;
  issueDate: string;
  dueDate: string;
  status: InvoiceStatus;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentMethod?: PaymentMethod;
  description?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface FinancialStatistics {
  monthlyRevenue: number;
  unpaidInvoices: number;
  overdueInvoices: number;
  totalRevenue?: number;
  todayRevenue?: number;
  paymentMethodDistribution?: Record<string, number>;
}

export interface InvoiceFormData {
  invoiceNumber: string;
  patientId: number;
  issueDate: string;
  dueDate: string;
  status: InvoiceStatus;
  totalAmount: number;
  paidAmount: number;
  paymentMethod?: PaymentMethod;
  description?: string;
  notes?: string;
}
