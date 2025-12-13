// Dermatology Types for skin clinics

// ============================================
// SKIN EXAM TYPES
// ============================================

export const SkinType = {
  Type1: 'type1', // Very fair, always burns
  Type2: 'type2', // Fair, usually burns
  Type3: 'type3', // Medium, sometimes burns
  Type4: 'type4', // Olive, rarely burns
  Type5: 'type5', // Brown, very rarely burns
  Type6: 'type6', // Dark brown/black, never burns
} as const;

export type SkinType = typeof SkinType[keyof typeof SkinType];

export interface SkinExam {
  id: number;
  patientId: number;
  encounterId?: number;
  examDate: string;
  skinType?: SkinType;

  // Chief complaint and concerns
  concerns?: string;

  // Findings
  findings?: string;
  bodyAreas?: string[]; // e.g., ['face', 'back', 'arms']

  // Assessment
  diagnosis?: string;

  // Treatment plan
  treatment?: string;

  // Follow-up
  followUp?: string;
  followUpDate?: string;

  // Provider
  performedBy: string;

  notes?: string;
  createdAt: string;
  updatedAt?: string;

  // Nested data
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
    email?: string;
  };
}

export interface CreateSkinExamRequest {
  patientId: number;
  encounterId?: number;
  examDate: string;
  skinType?: SkinType;
  concerns?: string;
  findings?: string;
  bodyAreas?: string[];
  diagnosis?: string;
  treatment?: string;
  followUp?: string;
  followUpDate?: string;
  notes?: string;
}

// ============================================
// SKIN PHOTO TYPES
// ============================================

export interface SkinPhoto {
  id: number;
  patientId: number;
  examId?: number;
  photoDate: string;

  // Location
  bodyLocation: string; // e.g., 'Left arm', 'Back', 'Face - right cheek'

  // Description
  description?: string;

  // Photo storage
  photoUrl: string;
  thumbnailUrl?: string;

  notes?: string;

  // Metadata
  takenBy: string;
  createdAt: string;
  updatedAt?: string;

  // Nested data
  patient?: {
    id: number;
    fullNameEn: string;
  };
}

export interface CreateSkinPhotoRequest {
  patientId: number;
  examId?: number;
  photoDate: string;
  bodyLocation: string;
  description?: string;
  notes?: string;
  photo: File;
}

// ============================================
// MOLE MAPPING TYPES
// ============================================

export const RiskLevel = {
  Low: 'low',
  Moderate: 'moderate',
  High: 'high',
} as const;

export type RiskLevel = typeof RiskLevel[keyof typeof RiskLevel];

export interface MoleMapping {
  id: number;
  patientId: number;
  encounterId?: number;
  mapDate: string;

  // Mole counts
  totalMoles: number;
  atypicalMoles?: number;

  // Location mapping
  locations?: MoleLocation[];

  // Risk assessment
  riskAssessment?: RiskLevel;

  // Recommendations
  recommendations?: string;
  nextMappingDate?: string;

  // Provider
  performedBy: string;

  notes?: string;
  createdAt: string;
  updatedAt?: string;

  // Nested data
  patient?: {
    id: number;
    fullNameEn: string;
  };
}

export interface MoleLocation {
  bodyArea: string; // e.g., 'Upper back', 'Left forearm'
  count: number;
  hasAtypical?: boolean;
  description?: string;
}

export interface CreateMoleMappingRequest {
  patientId: number;
  encounterId?: number;
  mapDate: string;
  totalMoles: number;
  atypicalMoles?: number;
  locations?: MoleLocation[];
  riskAssessment?: RiskLevel;
  recommendations?: string;
  nextMappingDate?: string;
  notes?: string;
}

// ============================================
// SKIN BIOPSY TYPES
// ============================================

export const BiopsyStatus = {
  Pending: 'pending',
  Processing: 'processing',
  Completed: 'completed',
  RequiresFollowUp: 'requires_followup',
} as const;

export type BiopsyStatus = typeof BiopsyStatus[keyof typeof BiopsyStatus];

export const BiopsyTechnique = {
  Shave: 'shave',
  Punch: 'punch',
  Excisional: 'excisional',
  Incisional: 'incisional',
} as const;

export type BiopsyTechnique = typeof BiopsyTechnique[keyof typeof BiopsyTechnique];

export interface SkinBiopsy {
  id: number;
  patientId: number;
  encounterId?: number;
  biopsyDate: string;

  // Site
  site: string; // e.g., 'Left upper arm', 'Back - midline'

  // Clinical indication
  indication: string;

  // Technique
  technique: BiopsyTechnique;

  // Specimen
  specimenDescription?: string;

  // Results
  pathologyResult?: string;
  diagnosis?: string;

  // Status
  status: BiopsyStatus;

  // Follow-up
  resultDate?: string;
  followUpRequired?: boolean;
  followUpDate?: string;

  // Provider
  performedBy: string;

  notes?: string;
  createdAt: string;
  updatedAt?: string;

  // Nested data
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
  };
}

export interface CreateSkinBiopsyRequest {
  patientId: number;
  encounterId?: number;
  biopsyDate: string;
  site: string;
  indication: string;
  technique: BiopsyTechnique;
  specimenDescription?: string;
  notes?: string;
}

export interface UpdateBiopsyResultRequest {
  biopsyId: number;
  pathologyResult: string;
  diagnosis?: string;
  status: BiopsyStatus;
  resultDate: string;
  followUpRequired?: boolean;
  followUpDate?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface DermatologyStatistics {
  // Patient stats
  totalPatients: number;
  newPatientsThisMonth: number;

  // Exam stats
  examsThisWeek: number;
  examsThisMonth: number;

  // Photo stats
  totalPhotos: number;
  photosThisMonth: number;

  // Mole mapping stats
  moleMappingsThisMonth: number;
  highRiskPatients: number;

  // Biopsy stats
  biopsiesThisMonth: number;
  pendingBiopsies: number;
  requiresFollowUp: number;

  // Distributions
  skinTypeDistribution?: Record<string, number>;
  biopsyStatusDistribution?: Record<string, number>;
  riskLevelDistribution?: Record<string, number>;
}
