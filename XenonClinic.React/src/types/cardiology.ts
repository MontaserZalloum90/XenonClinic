// Cardiology Types for cardiac clinics

// ============================================
// ECG TYPES
// ============================================

export const ECGStatus = {
  Pending: 'pending',
  InProgress: 'in_progress',
  Completed: 'completed',
  Reviewed: 'reviewed',
} as const;

export type ECGStatus = typeof ECGStatus[keyof typeof ECGStatus];

export const HeartRhythm = {
  Normal: 'normal',
  Sinus: 'sinus',
  AFib: 'afib',
  AFlutter: 'aflutter',
  SVT: 'svt',
  VTach: 'vtach',
  VFib: 'vfib',
  Bradycardia: 'bradycardia',
  Tachycardia: 'tachycardia',
  Other: 'other',
} as const;

export type HeartRhythm = typeof HeartRhythm[keyof typeof HeartRhythm];

export interface ECGRecord {
  id: number;
  patientId: number;
  patientName?: string;
  recordDate: string;
  heartRate: number;
  rhythm: HeartRhythm;
  interpretation: string;
  abnormalities?: string[];
  performedBy: string;
  reviewedBy?: string;
  reportUrl?: string;
  status: ECGStatus;
  prInterval?: number;
  qrsDuration?: number;
  qtInterval?: number;
  qtcInterval?: number;
  pWavePresent?: boolean;
  stChanges?: string;
  tWaveChanges?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateECGRequest {
  patientId: number;
  recordDate: string;
  heartRate: number;
  rhythm: HeartRhythm;
  interpretation: string;
  abnormalities?: string[];
  performedBy: string;
  prInterval?: number;
  qrsDuration?: number;
  qtInterval?: number;
  qtcInterval?: number;
  stChanges?: string;
  tWaveChanges?: string;
  notes?: string;
}

// ============================================
// ECHOCARDIOGRAM TYPES
// ============================================

export const EchoStatus = {
  Scheduled: 'scheduled',
  InProgress: 'in_progress',
  Completed: 'completed',
  Reported: 'reported',
} as const;

export type EchoStatus = typeof EchoStatus[keyof typeof EchoStatus];

export const ValveFinding = {
  Normal: 'normal',
  Stenosis: 'stenosis',
  Regurgitation: 'regurgitation',
  Prolapse: 'prolapse',
  Mixed: 'mixed',
} as const;

export type ValveFinding = typeof ValveFinding[keyof typeof ValveFinding];

export interface EchocardiogramRecord {
  id: number;
  patientId: number;
  patientName?: string;
  date: string;
  ejectionFraction: number; // percentage
  leftVentricleSize?: string;
  rightVentricleSize?: string;
  wallMotion: string;
  valveFindings: {
    mitral?: ValveFinding;
    aortic?: ValveFinding;
    tricuspid?: ValveFinding;
    pulmonary?: ValveFinding;
  };
  conclusions: string;
  performedBy: string;
  interpretedBy?: string;
  status: EchoStatus;
  diastolicFunction?: string;
  pericardialEffusion?: boolean;
  estimatedPAP?: number; // Pulmonary Artery Pressure
  notes?: string;
  reportUrl?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateEchoRequest {
  patientId: number;
  date: string;
  ejectionFraction: number;
  leftVentricleSize?: string;
  rightVentricleSize?: string;
  wallMotion: string;
  valveFindings: {
    mitral?: ValveFinding;
    aortic?: ValveFinding;
    tricuspid?: ValveFinding;
    pulmonary?: ValveFinding;
  };
  conclusions: string;
  performedBy: string;
  diastolicFunction?: string;
  pericardialEffusion?: boolean;
  estimatedPAP?: number;
  notes?: string;
}

// ============================================
// STRESS TEST TYPES
// ============================================

export const StressTestType = {
  Exercise: 'exercise',
  Pharmacological: 'pharmacological',
  Nuclear: 'nuclear',
  Echo: 'echo',
} as const;

export type StressTestType = typeof StressTestType[keyof typeof StressTestType];

export const StressTestProtocol = {
  Bruce: 'bruce',
  ModifiedBruce: 'modified_bruce',
  Naughton: 'naughton',
  Balke: 'balke',
  Dobutamine: 'dobutamine',
  Adenosine: 'adenosine',
  Dipyridamole: 'dipyridamole',
} as const;

export type StressTestProtocol = typeof StressTestProtocol[keyof typeof StressTestProtocol];

export const StressTestResult = {
  Negative: 'negative',
  Positive: 'positive',
  Equivocal: 'equivocal',
  Uninterpretable: 'uninterpretable',
} as const;

export type StressTestResult = typeof StressTestResult[keyof typeof StressTestResult];

export interface StressTest {
  id: number;
  patientId: number;
  patientName?: string;
  testType: StressTestType;
  protocol: StressTestProtocol;
  date: string;
  duration: number; // minutes
  maxHeartRate: number;
  targetHeartRate?: number;
  percentTargetAchieved?: number;
  bloodPressure: {
    systolic: number;
    diastolic: number;
  };
  peakBloodPressure?: {
    systolic: number;
    diastolic: number;
  };
  symptoms?: string[];
  ecgChanges?: string;
  stSegmentChanges?: string;
  arrhythmias?: string;
  testResult: StressTestResult;
  conclusion: string;
  recommendations?: string;
  performedBy: string;
  interpretedBy?: string;
  reportUrl?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateStressTestRequest {
  patientId: number;
  testType: StressTestType;
  protocol: StressTestProtocol;
  date: string;
  duration: number;
  maxHeartRate: number;
  targetHeartRate?: number;
  bloodPressure: {
    systolic: number;
    diastolic: number;
  };
  peakBloodPressure?: {
    systolic: number;
    diastolic: number;
  };
  symptoms?: string[];
  ecgChanges?: string;
  stSegmentChanges?: string;
  arrhythmias?: string;
  testResult: StressTestResult;
  conclusion: string;
  recommendations?: string;
  performedBy: string;
  notes?: string;
}

// ============================================
// CARDIAC CATHETERIZATION TYPES
// ============================================

export const CathProcedureType = {
  Diagnostic: 'diagnostic',
  PCI: 'pci', // Percutaneous Coronary Intervention
  Angioplasty: 'angioplasty',
  Stent: 'stent',
  Ablation: 'ablation',
  Biopsy: 'biopsy',
} as const;

export type CathProcedureType = typeof CathProcedureType[keyof typeof CathProcedureType];

export const AccessSite = {
  Radial: 'radial',
  Femoral: 'femoral',
  Brachial: 'brachial',
} as const;

export type AccessSite = typeof AccessSite[keyof typeof AccessSite];

export interface CardiacCatheterization {
  id: number;
  patientId: number;
  patientName?: string;
  procedure: CathProcedureType;
  date: string;
  accessSite?: AccessSite;
  coronaryFindings: string;
  findings: string;
  interventions?: string[];
  stentsPlaced?: number;
  complications?: string[];
  contrast?: number; // ml
  fluoroscopyTime?: number; // minutes
  conclusions: string;
  recommendations?: string;
  performedBy: string;
  assistedBy?: string;
  reportUrl?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCathRequest {
  patientId: number;
  procedure: CathProcedureType;
  date: string;
  accessSite?: AccessSite;
  coronaryFindings: string;
  findings: string;
  interventions?: string[];
  stentsPlaced?: number;
  complications?: string[];
  contrast?: number;
  fluoroscopyTime?: number;
  conclusions: string;
  recommendations?: string;
  performedBy: string;
  assistedBy?: string;
  notes?: string;
}

// ============================================
// RISK ASSESSMENT TYPES
// ============================================

export const RiskLevel = {
  Low: 'low',
  Moderate: 'moderate',
  High: 'high',
  VeryHigh: 'very_high',
} as const;

export type RiskLevel = typeof RiskLevel[keyof typeof RiskLevel];

export interface RiskAssessment {
  // Patient demographics
  age: number;
  gender: 'male' | 'female';

  // Risk factors
  totalCholesterol?: number; // mg/dL
  hdlCholesterol?: number; // mg/dL
  ldlCholesterol?: number; // mg/dL
  systolicBP: number;
  diastolicBP: number;
  isSmoker: boolean;
  hasDiabetes: boolean;
  hasHypertension?: boolean;
  familyHistory?: boolean;

  // Calculated values
  tenYearRisk?: number; // percentage
  riskLevel?: RiskLevel;
  riskScore?: number;

  // Recommendations
  recommendations?: string[];
  assessmentDate?: string;
}

export interface CalculateRiskRequest {
  age: number;
  gender: 'male' | 'female';
  totalCholesterol: number;
  hdlCholesterol: number;
  systolicBP: number;
  isSmoker: boolean;
  hasDiabetes: boolean;
  hasHypertension?: boolean;
  familyHistory?: boolean;
}

// ============================================
// STATISTICS
// ============================================

export interface CardiologyStatistics {
  // Patient stats
  totalPatients: number;
  newPatientsThisMonth?: number;

  // Procedure stats
  ecgsToday: number;
  ecgsThisWeek?: number;
  ecgsThisMonth?: number;

  echosThisWeek: number;
  echosThisMonth?: number;

  stressTestsThisMonth?: number;
  cathsThisMonth?: number;

  // Pending items
  pendingResults: number;
  pendingReviews?: number;

  // Clinical findings
  abnormalECGs?: number;
  reducedEF?: number; // EF < 50%
  positiveStressTests?: number;

  // Distributions
  rhythmDistribution?: Record<string, number>;
  testTypeDistribution?: Record<string, number>;
  riskLevelDistribution?: Record<string, number>;

  // Revenue (if applicable)
  monthlyRevenue?: number;
}

// ============================================
// APPOINTMENT/PROCEDURE SCHEDULING
// ============================================

export const ProcedureStatus = {
  Scheduled: 'scheduled',
  Confirmed: 'confirmed',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
  NoShow: 'no_show',
} as const;

export type ProcedureStatus = typeof ProcedureStatus[keyof typeof ProcedureStatus];

export interface CardiologyProcedure {
  id: number;
  patientId: number;
  patientName?: string;
  procedureType: 'ecg' | 'echo' | 'stress_test' | 'cath';
  scheduledDate: string;
  scheduledTime?: string;
  duration?: number; // minutes
  status: ProcedureStatus;
  performedBy?: string;
  roomNumber?: string;
  preparationInstructions?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateProcedureRequest {
  patientId: number;
  procedureType: 'ecg' | 'echo' | 'stress_test' | 'cath';
  scheduledDate: string;
  scheduledTime?: string;
  duration?: number;
  performedBy?: string;
  roomNumber?: string;
  preparationInstructions?: string;
  notes?: string;
}
