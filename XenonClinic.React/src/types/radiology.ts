export const RadiologyOrderStatus = {
  Pending: 0,
  Scheduled: 1,
  InProgress: 2,
  Completed: 3,
  Reported: 4,
  Cancelled: 5,
} as const;

export type RadiologyOrderStatus = typeof RadiologyOrderStatus[keyof typeof RadiologyOrderStatus];

export const ImagingType = {
  XRay: 0,
  CT: 1,
  MRI: 2,
  Ultrasound: 3,
  Mammography: 4,
  Fluoroscopy: 5,
  Other: 6,
} as const;

export type ImagingType = typeof ImagingType[keyof typeof ImagingType];

export interface RadiologyOrder {
  id: number;
  orderNumber: string;
  patientId: number;
  patientName?: string;
  doctorId?: number;
  doctorName?: string;
  imagingType: ImagingType;
  bodyPart?: string;
  orderDate: string;
  scheduledDate?: string;
  status: RadiologyOrderStatus;
  priority: boolean;
  clinicalNotes?: string;
  radiologistNotes?: string;
  reportUrl?: string;
  totalAmount?: number;
  isPaid: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface RadiologyStatistics {
  pendingOrders: number;
  completedToday: number;
  totalOrders?: number;
  scheduledOrders?: number;
  imagingTypeDistribution?: Record<string, number>;
  statusDistribution?: Record<string, number>;
}

export interface RadiologyOrderFormData {
  orderNumber: string;
  patientId: number;
  doctorId?: number;
  imagingType: ImagingType;
  bodyPart?: string;
  orderDate: string;
  scheduledDate?: string;
  status: RadiologyOrderStatus;
  priority: boolean;
  clinicalNotes?: string;
  totalAmount?: number;
  isPaid: boolean;
}
