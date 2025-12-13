// Orthopedics Types for orthopedic clinics

// ============================================
// FRACTURE STATUS
// ============================================

export const FractureStatus = {
  Active: 0,
  Healing: 1,
  Healed: 2,
  Complicated: 3,
} as const;

export type FractureStatus = typeof FractureStatus[keyof typeof FractureStatus];

// ============================================
// ORTHOPEDIC EXAM TYPES
// ============================================

export interface OrthopedicExam {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  chiefComplaint: string;
  affectedArea: string;
  rangeOfMotion?: string;
  strength?: string;
  stability?: string;
  specialTests?: string;
  diagnosis: string;
  plan: string;
  performedBy: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateOrthopedicExamRequest {
  patientId: number;
  examDate: string;
  chiefComplaint: string;
  affectedArea: string;
  rangeOfMotion?: string;
  strength?: string;
  stability?: string;
  specialTests?: string;
  diagnosis: string;
  plan: string;
  performedBy: string;
  notes?: string;
}

// ============================================
// FRACTURE RECORD TYPES
// ============================================

export interface FractureRecord {
  id: number;
  patientId: number;
  patientName?: string;
  fractureDate: string;
  boneAffected: string;
  fractureType: string;
  location: string;
  displacement?: string;
  treatment: string;
  castType?: string;
  expectedHealingTime?: number; // in weeks
  followUpDate?: string;
  status: FractureStatus;
  treatedBy: string;
  notes?: string;
  complications?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateFractureRecordRequest {
  patientId: number;
  fractureDate: string;
  boneAffected: string;
  fractureType: string;
  location: string;
  displacement?: string;
  treatment: string;
  castType?: string;
  expectedHealingTime?: number;
  followUpDate?: string;
  status: FractureStatus;
  treatedBy: string;
  notes?: string;
}

// ============================================
// ORTHOPEDIC SURGERY TYPES
// ============================================

export interface OrthopedicSurgery {
  id: number;
  patientId: number;
  patientName?: string;
  surgeryDate: string;
  procedure: string;
  indication: string;
  implants?: string;
  surgeon: string;
  assistant?: string;
  anesthesia?: string;
  duration?: number; // in minutes
  complications?: string;
  postOpInstructions?: string;
  outcome?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateOrthopedicSurgeryRequest {
  patientId: number;
  surgeryDate: string;
  procedure: string;
  indication: string;
  implants?: string;
  surgeon: string;
  assistant?: string;
  anesthesia?: string;
  duration?: number;
  complications?: string;
  postOpInstructions?: string;
  outcome?: string;
  notes?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface OrthopedicStatistics {
  totalExams: number;
  examsThisMonth: number;
  activeFractures: number;
  healedFractures: number;
  totalSurgeries: number;
  surgeriesThisMonth: number;
  pendingFollowUps: number;
  complicatedCases: number;
  fractureTypeDistribution?: Record<string, number>;
  surgeryTypeDistribution?: Record<string, number>;
}
