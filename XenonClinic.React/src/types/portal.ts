// Patient Portal Types

export interface PortalUser {
  id: number;
  email: string;
  patientId: number;
  isVerified: boolean;
  isProfileComplete: boolean;
  registeredAt: string; // ISO date string
  lastLoginAt?: string; // ISO date string
  patient?: {
    id: number;
    fullNameEn: string;
    fullNameAr?: string;
    phoneNumber?: string;
    email?: string;
    dateOfBirth: string;
    gender: string;
  };
}

export interface PortalRegistration {
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber: string;
  acceptTerms: boolean;
}

export const PortalDocumentType = {
  MedicalRecord: 0,
  LabResult: 1,
  Prescription: 2,
  ImagingReport: 3,
  Insurance: 4,
  Other: 5,
} as const;

export type PortalDocumentType = typeof PortalDocumentType[keyof typeof PortalDocumentType];

export interface PortalDocument {
  id: number;
  patientId: number;
  name: string;
  type: PortalDocumentType;
  uploadedAt: string; // ISO date string
  size: number; // in bytes
  url: string;
  description?: string;
}

export interface PortalAppointment {
  id: number;
  patientId: number;
  startTime: string; // ISO date string
  endTime: string; // ISO date string
  status: number; // Appointment status
  type: number; // Appointment type
  notes?: string;
  provider?: {
    id: number;
    fullName: string;
  };
  branch?: {
    id: number;
    name: string;
  };
}

export interface PortalStatistics {
  totalAppointments: number;
  upcomingAppointments: number;
  totalDocuments: number;
  lastVisit?: string; // ISO date string
  memberSince: string; // ISO date string
}

export interface PortalProfileUpdate {
  fullNameEn: string;
  fullNameAr?: string;
  phoneNumber: string;
  email: string;
  dateOfBirth: string;
  gender: string;
  medicalHistory?: string;
  allergies?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
}

export interface UploadDocumentRequest {
  name: string;
  type: PortalDocumentType;
  file: File;
  description?: string;
}

export interface BookAppointmentRequest {
  startTime: string;
  endTime: string;
  type: number;
  notes?: string;
  providerId?: number;
}
