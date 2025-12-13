// Ophthalmology Types for eye clinic specialty

// ============================================
// VISUAL ACUITY TEST TYPES
// ============================================

export interface VisualAcuityTest {
  id: number;
  patientId: number;
  testDate: string;
  rightEyeUncorrected: string; // e.g., "20/20", "6/6"
  rightEyeCorrected?: string;
  leftEyeUncorrected: string;
  leftEyeCorrected?: string;
  notes?: string;
  performedBy: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateVisualAcuityTestRequest {
  patientId: number;
  testDate: string;
  rightEyeUncorrected: string;
  rightEyeCorrected?: string;
  leftEyeUncorrected: string;
  leftEyeCorrected?: string;
  notes?: string;
  performedBy: string;
}

// ============================================
// REFRACTION TEST TYPES
// ============================================

export interface RefractionTest {
  id: number;
  patientId: number;
  date: string;
  rightSphere: number; // Diopters
  rightCylinder: number;
  rightAxis: number; // 0-180 degrees
  leftSphere: number;
  leftCylinder: number;
  leftAxis: number;
  pupillaryDistance: number; // in mm
  performedBy: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateRefractionTestRequest {
  patientId: number;
  date: string;
  rightSphere: number;
  rightCylinder: number;
  rightAxis: number;
  leftSphere: number;
  leftCylinder: number;
  leftAxis: number;
  pupillaryDistance: number;
  performedBy: string;
  notes?: string;
}

// ============================================
// IOP MEASUREMENT TYPES
// ============================================

export const IOPMethod = {
  GoldmannApplanation: 'goldmann',
  Tonopen: 'tonopen',
  NonContact: 'noncontact',
  Rebound: 'rebound',
  Other: 'other',
} as const;

export type IOPMethod = typeof IOPMethod[keyof typeof IOPMethod];

export interface IOPMeasurement {
  id: number;
  patientId: number;
  date: string;
  time: string;
  rightEye: number; // mmHg
  leftEye: number; // mmHg
  method: IOPMethod;
  performedBy: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateIOPMeasurementRequest {
  patientId: number;
  date: string;
  time: string;
  rightEye: number;
  leftEye: number;
  method: IOPMethod;
  performedBy: string;
  notes?: string;
}

// ============================================
// SLIT LAMP EXAM TYPES
// ============================================

export interface SlitLampExam {
  id: number;
  patientId: number;
  date: string;
  findings: string;
  cornea?: string;
  lens?: string;
  anteriorChamber?: string;
  iris?: string;
  notes?: string;
  performedBy: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateSlitLampExamRequest {
  patientId: number;
  date: string;
  findings: string;
  cornea?: string;
  lens?: string;
  anteriorChamber?: string;
  iris?: string;
  notes?: string;
  performedBy: string;
}

// ============================================
// FUNDUS EXAM TYPES
// ============================================

export interface FundusExam {
  id: number;
  patientId: number;
  date: string;
  rightEyeFindings: string;
  leftEyeFindings: string;
  opticDisc?: string;
  macula?: string;
  vessels?: string;
  performedBy: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateFundusExamRequest {
  patientId: number;
  date: string;
  rightEyeFindings: string;
  leftEyeFindings: string;
  opticDisc?: string;
  macula?: string;
  vessels?: string;
  performedBy: string;
  notes?: string;
}

// ============================================
// GLASSES PRESCRIPTION TYPES
// ============================================

export const PrescriptionStatus = {
  Active: 0,
  Expired: 1,
  Cancelled: 2,
} as const;

export type PrescriptionStatus = typeof PrescriptionStatus[keyof typeof PrescriptionStatus];

export interface GlassesPrescription {
  id: number;
  patientId: number;
  date: string;
  rightSphere: number;
  rightCylinder: number;
  rightAxis: number;
  rightAdd?: number; // For progressive/bifocal
  leftSphere: number;
  leftCylinder: number;
  leftAxis: number;
  leftAdd?: number;
  pupillaryDistance: number;
  prescribedBy: string;
  expiryDate: string;
  status?: PrescriptionStatus;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
    dateOfBirth?: string;
  };
}

export interface CreateGlassesPrescriptionRequest {
  patientId: number;
  date: string;
  rightSphere: number;
  rightCylinder: number;
  rightAxis: number;
  rightAdd?: number;
  leftSphere: number;
  leftCylinder: number;
  leftAxis: number;
  leftAdd?: number;
  pupillaryDistance: number;
  prescribedBy: string;
  expiryDate: string;
  notes?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface OphthalmologyStatistics {
  totalPatients: number;
  visualAcuityTestsThisMonth: number;
  refractionTestsThisMonth: number;
  iopMeasurementsThisMonth: number;
  slitLampExamsThisMonth: number;
  fundusExamsThisMonth: number;
  prescriptionsThisMonth: number;
  activePrescriptions: number;
  expiringPrescriptionsSoon: number; // Within 30 days
  highIOPCases?: number; // IOP > 21 mmHg
  recentExams: number;
  monthlyRevenue?: number;
}
