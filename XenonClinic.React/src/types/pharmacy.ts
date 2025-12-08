export const PrescriptionStatus = {
  Pending: 0,
  Approved: 1,
  Dispensed: 2,
  PartiallyDispensed: 3,
  Cancelled: 4,
  Rejected: 5,
} as const;

export type PrescriptionStatus = typeof PrescriptionStatus[keyof typeof PrescriptionStatus];

export interface Prescription {
  id: number;
  prescriptionNumber: string;
  patientId: number;
  patientName?: string;
  doctorId?: number;
  doctorName?: string;
  prescriptionDate: string;
  status: PrescriptionStatus;
  medications: string; // JSON string or comma-separated
  dosage?: string;
  duration?: string;
  refills?: number;
  notes?: string;
  dispensedDate?: string;
  dispensedBy?: string;
  totalAmount?: number;
  isPaid: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PharmacyStatistics {
  pendingPrescriptions: number;
  dispensedToday: number;
  totalPrescriptions?: number;
  monthlyDispensed?: number;
  statusDistribution?: Record<string, number>;
}

export interface PrescriptionFormData {
  prescriptionNumber: string;
  patientId: number;
  doctorId?: number;
  prescriptionDate: string;
  status: PrescriptionStatus;
  medications: string;
  dosage?: string;
  duration?: string;
  refills?: number;
  notes?: string;
  totalAmount?: number;
  isPaid: boolean;
}
