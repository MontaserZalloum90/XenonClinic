// Laboratory Types matching backend entities

export const LabOrderStatus = {
  Pending: 0,
  Collected: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
} as const;

export type LabOrderStatus = typeof LabOrderStatus[keyof typeof LabOrderStatus];

export interface LabOrder {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: LabOrderStatus;
  patientId: number;
  branchId: number;
  externalLabId?: number;
  orderedBy?: string;
  collectionDate?: string;
  collectedBy?: string;
  expectedCompletionDate?: string;
  completedDate?: string;
  totalAmount: number;
  isPaid: boolean;
  isUrgent: boolean;
  clinicalNotes?: string;
  notes?: string;
  createdBy: string;
  createdAt: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateLabOrderRequest {
  patientId: number;
  isUrgent: boolean;
  clinicalNotes?: string;
  notes?: string;
}

export interface LabStatistics {
  pendingOrders: number;
  urgentOrders: number;
  monthlyRevenue: number;
  statusDistribution: Record<number, number>;
}
