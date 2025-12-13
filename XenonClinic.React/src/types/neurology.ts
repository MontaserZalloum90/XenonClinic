// Neurology Types for neurological clinics

// ============================================
// NEUROLOGICAL EXAM TYPES
// ============================================

export const ExamStatus = {
  Pending: 'pending',
  InProgress: 'in_progress',
  Completed: 'completed',
  Reviewed: 'reviewed',
} as const;

export type ExamStatus = typeof ExamStatus[keyof typeof ExamStatus];

export const MentalStatus = {
  Alert: 'alert',
  Confused: 'confused',
  Drowsy: 'drowsy',
  Lethargic: 'lethargic',
  Stuporous: 'stuporous',
  Comatose: 'comatose',
} as const;

export type MentalStatus = typeof MentalStatus[keyof typeof MentalStatus];

export interface NeurologicalExam {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  mentalStatus: MentalStatus;
  cranialNerves: string;
  motorFunction: string;
  sensory: string;
  reflexes: string;
  coordination: string;
  gait: string;
  diagnosis?: string;
  performedBy: string;
  status?: ExamStatus;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateNeurologicalExamRequest {
  patientId: number;
  examDate: string;
  mentalStatus: MentalStatus;
  cranialNerves: string;
  motorFunction: string;
  sensory: string;
  reflexes: string;
  coordination: string;
  gait: string;
  diagnosis?: string;
  performedBy: string;
  notes?: string;
}

// ============================================
// EEG TYPES
// ============================================

export const EEGStatus = {
  Scheduled: 'scheduled',
  InProgress: 'in_progress',
  Completed: 'completed',
  Interpreted: 'interpreted',
} as const;

export type EEGStatus = typeof EEGStatus[keyof typeof EEGStatus];

export const EEGFinding = {
  Normal: 'normal',
  Abnormal: 'abnormal',
  EpilepticActivity: 'epileptic_activity',
  Slowing: 'slowing',
  AsymmetricActivity: 'asymmetric_activity',
  Other: 'other',
} as const;

export type EEGFinding = typeof EEGFinding[keyof typeof EEGFinding];

export interface EEGRecord {
  id: number;
  patientId: number;
  patientName?: string;
  recordDate: string;
  duration: number; // minutes
  findings: EEGFinding;
  interpretation: string;
  abnormalities?: string[];
  performedBy: string;
  interpretedBy?: string;
  status: EEGStatus;
  reportUrl?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateEEGRequest {
  patientId: number;
  recordDate: string;
  duration: number;
  findings: EEGFinding;
  interpretation: string;
  abnormalities?: string[];
  performedBy: string;
  notes?: string;
}

// ============================================
// EPILEPSY DIARY TYPES
// ============================================

export const SeizureType = {
  GeneralizedTonicClonic: 'generalized_tonic_clonic',
  Absence: 'absence',
  Myoclonic: 'myoclonic',
  Atonic: 'atonic',
  FocalAware: 'focal_aware',
  FocalImpairedAwareness: 'focal_impaired_awareness',
  FocalToGeneralized: 'focal_to_generalized',
  Unknown: 'unknown',
} as const;

export type SeizureType = typeof SeizureType[keyof typeof SeizureType];

export interface EpilepsyDiary {
  id: number;
  patientId: number;
  patientName?: string;
  seizureDate: string;
  seizureType: SeizureType;
  duration: number; // minutes
  triggers?: string[];
  preSymptoms?: string; // Aura or warning signs
  postSymptoms?: string; // Post-ictal symptoms
  medications?: string[];
  severity?: 'mild' | 'moderate' | 'severe';
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateEpilepsyDiaryRequest {
  patientId: number;
  seizureDate: string;
  seizureType: SeizureType;
  duration: number;
  triggers?: string[];
  preSymptoms?: string;
  postSymptoms?: string;
  medications?: string[];
  severity?: 'mild' | 'moderate' | 'severe';
  notes?: string;
}

// ============================================
// STROKE ASSESSMENT TYPES
// ============================================

export const StrokeType = {
  Ischemic: 'ischemic',
  Hemorrhagic: 'hemorrhagic',
  TIA: 'tia', // Transient Ischemic Attack
  Unknown: 'unknown',
} as const;

export type StrokeType = typeof StrokeType[keyof typeof StrokeType];

export const StrokeSeverity = {
  Minor: 'minor', // NIHSS 0-4
  Moderate: 'moderate', // NIHSS 5-15
  ModerateToSevere: 'moderate_to_severe', // NIHSS 16-20
  Severe: 'severe', // NIHSS 21-42
} as const;

export type StrokeSeverity = typeof StrokeSeverity[keyof typeof StrokeSeverity];

export interface NIHSSScore {
  // Level of consciousness (0-3)
  consciousness: number;
  // LOC questions (0-2)
  locQuestions: number;
  // LOC commands (0-2)
  locCommands: number;
  // Best gaze (0-2)
  bestGaze: number;
  // Visual fields (0-3)
  visualFields: number;
  // Facial palsy (0-3)
  facialPalsy: number;
  // Motor arm left (0-4)
  motorArmLeft: number;
  // Motor arm right (0-4)
  motorArmRight: number;
  // Motor leg left (0-4)
  motorLegLeft: number;
  // Motor leg right (0-4)
  motorLegRight: number;
  // Limb ataxia (0-2)
  limbAtaxia: number;
  // Sensory (0-2)
  sensory: number;
  // Best language (0-3)
  bestLanguage: number;
  // Dysarthria (0-2)
  dysarthria: number;
  // Extinction/inattention (0-2)
  extinction: number;
}

export interface StrokeAssessment {
  id: number;
  patientId: number;
  patientName?: string;
  assessmentDate: string;
  nihssScore: number; // Total score (0-42)
  nihssDetails?: NIHSSScore;
  strokeType: StrokeType;
  affectedArea?: string;
  onsetTime?: string;
  symptoms?: string[];
  treatment?: string;
  performedBy: string;
  severity?: StrokeSeverity;
  recommendations?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateStrokeAssessmentRequest {
  patientId: number;
  assessmentDate: string;
  nihssScore: number;
  nihssDetails?: NIHSSScore;
  strokeType: StrokeType;
  affectedArea?: string;
  onsetTime?: string;
  symptoms?: string[];
  treatment?: string;
  performedBy: string;
  recommendations?: string;
  notes?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface NeurologyStatistics {
  // Patient stats
  totalPatients: number;
  newPatientsThisMonth: number;

  // Exam stats
  examsToday: number;
  examsThisWeek: number;
  examsThisMonth: number;
  pendingReviews: number;

  // EEG stats
  eegsThisWeek: number;
  eegsThisMonth: number;
  abnormalEEGs: number;

  // Epilepsy stats
  seizuresThisMonth: number;
  epilepsyPatients: number;

  // Stroke stats
  strokeAssessmentsThisMonth: number;
  acuteStrokes: number; // Strokes in last 24-48 hours

  // Distributions
  seizureTypeDistribution?: Record<string, number>;
  strokeTypeDistribution?: Record<string, number>;
  examStatusDistribution?: Record<string, number>;

  // Revenue (if applicable)
  monthlyRevenue?: number;
}
