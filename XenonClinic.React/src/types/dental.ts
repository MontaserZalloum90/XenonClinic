// Dental Types for dental clinics

// ============================================
// DENTAL TREATMENT STATUS
// ============================================

export const DentalTreatmentStatus = {
  Planned: 'planned',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
} as const;

export type DentalTreatmentStatus = typeof DentalTreatmentStatus[keyof typeof DentalTreatmentStatus];

// ============================================
// TOOTH CONDITION TYPES
// ============================================

export const ToothConditionType = {
  Healthy: 'healthy',
  Cavity: 'cavity',
  Filled: 'filled',
  Crown: 'crown',
  Bridge: 'bridge',
  Implant: 'implant',
  Missing: 'missing',
  RootCanal: 'root_canal',
  Fractured: 'fractured',
  Worn: 'worn',
  Decayed: 'decayed',
} as const;

export type ToothConditionType = typeof ToothConditionType[keyof typeof ToothConditionType];

export const ToothSurface = {
  Occlusal: 'occlusal',      // Chewing surface
  Mesial: 'mesial',          // Front facing surface
  Distal: 'distal',          // Back facing surface
  Buccal: 'buccal',          // Cheek side
  Lingual: 'lingual',        // Tongue side
  Facial: 'facial',          // Front side
} as const;

export type ToothSurface = typeof ToothSurface[keyof typeof ToothSurface];

// ============================================
// DENTAL CHART TYPES
// ============================================

export interface ToothCondition {
  toothNumber: number;       // Universal numbering system (1-32)
  condition: ToothConditionType;
  surface?: ToothSurface[];  // Affected surfaces
  notes?: string;
  treatmentPlan?: string;
  severity?: 'mild' | 'moderate' | 'severe';
}

export interface DentalChart {
  id: number;
  patientId: number;
  patientName?: string;
  chartDate: string;
  teeth: ToothCondition[];
  notes?: string;
  createdBy: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDentalChartRequest {
  patientId: number;
  chartDate: string;
  teeth: ToothCondition[];
  notes?: string;
}

// ============================================
// DENTAL TREATMENT TYPES
// ============================================

export const TreatmentType = {
  Cleaning: 'cleaning',
  Filling: 'filling',
  Extraction: 'extraction',
  RootCanal: 'root_canal',
  Crown: 'crown',
  Bridge: 'bridge',
  Implant: 'implant',
  Veneer: 'veneer',
  Whitening: 'whitening',
  Orthodontics: 'orthodontics',
  Denture: 'denture',
  Scaling: 'scaling',
  Polishing: 'polishing',
  XRay: 'xray',
  Consultation: 'consultation',
  Emergency: 'emergency',
  Other: 'other',
} as const;

export type TreatmentType = typeof TreatmentType[keyof typeof TreatmentType];

export interface DentalTreatment {
  id: number;
  patientId: number;
  patientName?: string;
  toothNumber?: number;      // Optional, some treatments don't apply to specific tooth
  treatmentType: TreatmentType;
  description?: string;
  status: DentalTreatmentStatus;
  performedBy?: string;      // Dentist name
  performedDate?: string;
  scheduledDate?: string;
  cost?: number;
  isPaid?: boolean;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDentalTreatmentRequest {
  patientId: number;
  toothNumber?: number;
  treatmentType: TreatmentType;
  description?: string;
  status: DentalTreatmentStatus;
  performedBy?: string;
  performedDate?: string;
  scheduledDate?: string;
  cost?: number;
  isPaid?: boolean;
  notes?: string;
}

// ============================================
// PERIODONTAL EXAM TYPES
// ============================================

export interface PocketDepth {
  toothNumber: number;
  // Probing depths in mm (6 sites per tooth)
  mesialBuccal?: number;
  buccal?: number;
  distalBuccal?: number;
  mesialLingual?: number;
  lingual?: number;
  distalLingual?: number;
}

export interface BleedingPoint {
  toothNumber: number;
  sites: string[];  // e.g., ['MB', 'B', 'DB', 'ML', 'L', 'DL']
}

export interface PeriodontalExam {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  pocketDepths: PocketDepth[];
  bleedingPoints: BleedingPoint[];
  plaqueScore?: number;       // Percentage
  gingivalIndex?: number;     // 0-3 scale
  mobility?: {                // Tooth mobility
    toothNumber: number;
    degree: number;           // 0-3 scale
  }[];
  furcationInvolvement?: {
    toothNumber: number;
    grade: number;            // 1-3 grade
  }[];
  notes?: string;
  diagnosis?: string;
  treatmentPlan?: string;
  examinedBy: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePeriodontalExamRequest {
  patientId: number;
  examDate: string;
  pocketDepths: PocketDepth[];
  bleedingPoints: BleedingPoint[];
  plaqueScore?: number;
  gingivalIndex?: number;
  mobility?: {
    toothNumber: number;
    degree: number;
  }[];
  furcationInvolvement?: {
    toothNumber: number;
    grade: number;
  }[];
  notes?: string;
  diagnosis?: string;
  treatmentPlan?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface DentalStatistics {
  // Patient stats
  totalPatients: number;
  newPatientsThisMonth: number;

  // Treatment stats
  treatmentsToday: number;
  treatmentsThisWeek: number;
  treatmentsThisMonth: number;
  pendingTreatments: number;
  completedTreatments: number;

  // Revenue stats
  monthlyRevenue?: number;
  outstandingPayments?: number;

  // Chart stats
  chartsThisMonth: number;
  periodontalExamsThisMonth: number;

  // Treatment type distribution
  treatmentTypeDistribution?: Record<string, number>;

  // Status distribution
  statusDistribution?: Record<string, number>;
}
