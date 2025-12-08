// Appointment Types matching backend entities

export const AppointmentStatus = {
  Booked: 0,
  Confirmed: 1,
  CheckedIn: 2,
  Completed: 3,
  Cancelled: 4,
  NoShow: 5,
} as const;

export type AppointmentStatus = typeof AppointmentStatus[keyof typeof AppointmentStatus];

export const AppointmentType = {
  Consultation: 0,
  FollowUp: 1,
  Procedure: 2,
  Emergency: 3,
} as const;

export type AppointmentType = typeof AppointmentType[keyof typeof AppointmentType];

export interface Appointment {
  id: number;
  patientId: number;
  branchId: number;
  providerId?: number;
  startTime: string; // ISO date string
  endTime: string; // ISO date string
  type: AppointmentType;
  status: AppointmentStatus;
  notes?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
    email?: string;
  };
  provider?: {
    id: number;
    fullName: string;
  };
  branch?: {
    id: number;
    name: string;
  };
}

export interface CreateAppointmentRequest {
  patientId: number;
  providerId?: number;
  startTime: string;
  endTime: string;
  type: AppointmentType;
  notes?: string;
}

export interface UpdateAppointmentRequest {
  id: number;
  patientId: number;
  providerId?: number;
  startTime: string;
  endTime: string;
  type: AppointmentType;
  status: AppointmentStatus;
  notes?: string;
}

export interface AppointmentStatistics {
  total: number;
  today: number;
  upcoming: number;
  completed: number;
  cancelled: number;
  noShow: number;
  statusDistribution: Record<AppointmentStatus, number>;
  typeDistribution: Record<AppointmentType, number>;
  completionRate: number;
}

export interface AvailabilitySlot {
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}
