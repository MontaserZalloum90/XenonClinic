// ============================================
// SALES MODULE TYPES
// ============================================

export const SaleStatus = {
  Draft: 0,
  Confirmed: 1,
  Completed: 2,
  Cancelled: 3,
  Refunded: 4,
} as const;

export type SaleStatus = typeof SaleStatus[keyof typeof SaleStatus];

export const PaymentStatus = {
  Pending: 0,
  Partial: 1,
  Paid: 2,
  Refunded: 3,
  Overdue: 4,
  Cancelled: 5,
} as const;

export type PaymentStatus = typeof PaymentStatus[keyof typeof PaymentStatus];

export const PaymentMethod = {
  Cash: 0,
  Card: 1,
  BankTransfer: 2,
  Insurance: 3,
  Installment: 4,
  Cheque: 5,
} as const;

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

export const QuotationStatus = {
  Draft: 0,
  Sent: 1,
  Accepted: 2,
  Rejected: 3,
  Expired: 4,
} as const;

export type QuotationStatus = typeof QuotationStatus[keyof typeof QuotationStatus];

// ============================================
// SALE INTERFACES
// ============================================

export interface Sale {
  id: number;
  invoiceNumber: string;
  saleDate: string;
  dueDate?: string;
  status: SaleStatus;
  paymentStatus: PaymentStatus;
  patientId: number;
  patientName?: string;
  branchId: number;
  branchName?: string;
  subTotal: number;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  taxAmount?: number;
  total: number;
  paidAmount: number;
  balance: number;
  isFullyPaid: boolean;
  isOverdue: boolean;
  notes?: string;
  terms?: string;
  quotationId?: number;
  quotationNumber?: string;
  items: SaleItem[];
  payments: Payment[];
  createdBy: string;
  createdAt: string;
  updatedAt?: string;
  updatedBy?: string;
}

export interface SaleItem {
  id: number;
  saleId: number;
  inventoryItemId?: number;
  itemName: string;
  itemDescription?: string;
  itemCode?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage?: number;
  discountAmount?: number;
  subtotal: number;
  taxPercentage?: number;
  taxAmount?: number;
  total: number;
  warrantyStartDate?: string;
  warrantyEndDate?: string;
  serialNumber?: string;
  notes?: string;
  createdAt: string;
  createdBy?: string;
}

export interface Payment {
  id: number;
  paymentNumber: string;
  paymentDate: string;
  amount: number;
  paymentMethod: PaymentMethod;
  saleId: number;
  referenceNumber?: string;
  bankName?: string;
  cardLastFourDigits?: string;
  insuranceCompany?: string;
  insuranceClaimNumber?: string;
  insurancePolicyNumber?: string;
  installmentNumber?: number;
  totalInstallments?: number;
  notes?: string;
  receivedBy: string;
  createdAt: string;
}

// ============================================
// QUOTATION INTERFACES
// ============================================

export interface Quotation {
  id: number;
  quotationNumber: string;
  quotationDate: string;
  expiryDate?: string;
  status: QuotationStatus;
  patientId: number;
  patientName?: string;
  branchId: number;
  branchName?: string;
  subTotal: number;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  taxAmount?: number;
  total: number;
  notes?: string;
  terms?: string;
  validityDays: number;
  isExpired: boolean;
  isActive: boolean;
  canConvertToSale: boolean;
  acceptedDate?: string;
  rejectedDate?: string;
  rejectionReason?: string;
  items: QuotationItem[];
  createdBy: string;
  createdAt: string;
  updatedAt?: string;
  updatedBy?: string;
}

export interface QuotationItem {
  id: number;
  quotationId: number;
  inventoryItemId?: number;
  itemName: string;
  itemDescription?: string;
  itemCode?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage?: number;
  discountAmount?: number;
  subtotal: number;
  taxPercentage?: number;
  taxAmount?: number;
  total: number;
  notes?: string;
  createdAt: string;
  createdBy?: string;
}

// ============================================
// FORM DATA INTERFACES
// ============================================

export interface SaleFormData {
  patientId: number;
  dueDate?: string;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  notes?: string;
  terms?: string;
  quotationId?: number;
}

export interface SaleItemFormData {
  inventoryItemId?: number;
  itemName: string;
  itemDescription?: string;
  itemCode?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  warrantyStartDate?: string;
  warrantyEndDate?: string;
  serialNumber?: string;
  notes?: string;
}

export interface PaymentFormData {
  saleId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  bankName?: string;
  cardLastFourDigits?: string;
  insuranceCompany?: string;
  insuranceClaimNumber?: string;
  insurancePolicyNumber?: string;
  installmentNumber?: number;
  totalInstallments?: number;
  notes?: string;
}

export interface QuotationFormData {
  patientId: number;
  expiryDate?: string;
  validityDays?: number;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  notes?: string;
  terms?: string;
}

export interface QuotationItemFormData {
  inventoryItemId?: number;
  itemName: string;
  itemDescription?: string;
  itemCode?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage?: number;
  discountAmount?: number;
  taxPercentage?: number;
  notes?: string;
}

// ============================================
// STATISTICS INTERFACES
// ============================================

export interface SalesStatistics {
  totalSales: number;
  totalPaid: number;
  totalOutstanding: number;
  salesCount: number;
  pendingSalesCount: number;
  completedSalesCount: number;
  overdueSalesCount: number;
  overdueTotal: number;
  pendingQuotationsCount: number;
  acceptedQuotationsCount: number;
  quotationConversionRate: number;
  averageTransactionValue: number;
  paymentMethodDistribution: Record<string, number>;
}

// ============================================
// HELPER FUNCTIONS
// ============================================

export const getSaleStatusLabel = (status: SaleStatus): string => {
  const labels: Record<SaleStatus, string> = {
    [SaleStatus.Draft]: 'Draft',
    [SaleStatus.Confirmed]: 'Confirmed',
    [SaleStatus.Completed]: 'Completed',
    [SaleStatus.Cancelled]: 'Cancelled',
    [SaleStatus.Refunded]: 'Refunded',
  };
  return labels[status] || 'Unknown';
};

export const getSaleStatusColor = (status: SaleStatus): string => {
  const colors: Record<SaleStatus, string> = {
    [SaleStatus.Draft]: 'text-gray-600 bg-gray-100',
    [SaleStatus.Confirmed]: 'text-blue-600 bg-blue-100',
    [SaleStatus.Completed]: 'text-green-600 bg-green-100',
    [SaleStatus.Cancelled]: 'text-red-600 bg-red-100',
    [SaleStatus.Refunded]: 'text-orange-600 bg-orange-100',
  };
  return colors[status] || 'text-gray-600 bg-gray-100';
};

export const getPaymentStatusLabel = (status: PaymentStatus): string => {
  const labels: Record<PaymentStatus, string> = {
    [PaymentStatus.Pending]: 'Pending',
    [PaymentStatus.Partial]: 'Partially Paid',
    [PaymentStatus.Paid]: 'Paid',
    [PaymentStatus.Refunded]: 'Refunded',
    [PaymentStatus.Overdue]: 'Overdue',
    [PaymentStatus.Cancelled]: 'Cancelled',
  };
  return labels[status] || 'Unknown';
};

export const getPaymentStatusColor = (status: PaymentStatus): string => {
  const colors: Record<PaymentStatus, string> = {
    [PaymentStatus.Pending]: 'text-yellow-600 bg-yellow-100',
    [PaymentStatus.Partial]: 'text-blue-600 bg-blue-100',
    [PaymentStatus.Paid]: 'text-green-600 bg-green-100',
    [PaymentStatus.Refunded]: 'text-orange-600 bg-orange-100',
    [PaymentStatus.Overdue]: 'text-red-600 bg-red-100',
    [PaymentStatus.Cancelled]: 'text-gray-600 bg-gray-100',
  };
  return colors[status] || 'text-gray-600 bg-gray-100';
};

export const getPaymentMethodLabel = (method: PaymentMethod): string => {
  const labels: Record<PaymentMethod, string> = {
    [PaymentMethod.Cash]: 'Cash',
    [PaymentMethod.Card]: 'Card',
    [PaymentMethod.BankTransfer]: 'Bank Transfer',
    [PaymentMethod.Insurance]: 'Insurance',
    [PaymentMethod.Installment]: 'Installment',
    [PaymentMethod.Cheque]: 'Cheque',
  };
  return labels[method] || 'Unknown';
};

export const getQuotationStatusLabel = (status: QuotationStatus): string => {
  const labels: Record<QuotationStatus, string> = {
    [QuotationStatus.Draft]: 'Draft',
    [QuotationStatus.Sent]: 'Sent',
    [QuotationStatus.Accepted]: 'Accepted',
    [QuotationStatus.Rejected]: 'Rejected',
    [QuotationStatus.Expired]: 'Expired',
  };
  return labels[status] || 'Unknown';
};

export const getQuotationStatusColor = (status: QuotationStatus): string => {
  const colors: Record<QuotationStatus, string> = {
    [QuotationStatus.Draft]: 'text-gray-600 bg-gray-100',
    [QuotationStatus.Sent]: 'text-blue-600 bg-blue-100',
    [QuotationStatus.Accepted]: 'text-green-600 bg-green-100',
    [QuotationStatus.Rejected]: 'text-red-600 bg-red-100',
    [QuotationStatus.Expired]: 'text-orange-600 bg-orange-100',
  };
  return colors[status] || 'text-gray-600 bg-gray-100';
};

export const formatCurrency = (amount: number, currency: string = 'AED'): string => {
  return new Intl.NumberFormat('en-AE', {
    style: 'currency',
    currency,
  }).format(amount);
};
