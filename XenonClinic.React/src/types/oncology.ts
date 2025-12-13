// Oncology Types for cancer care and treatment

// ============================================
// CANCER DIAGNOSIS TYPES
// ============================================

export const CancerStage = {
  Stage0: 'stage_0',
  Stage1: 'stage_1',
  Stage2: 'stage_2',
  Stage3: 'stage_3',
  Stage4: 'stage_4',
  Unknown: 'unknown',
} as const;

export type CancerStage = typeof CancerStage[keyof typeof CancerStage];

export const CancerGrade = {
  G1: 'g1', // Well differentiated
  G2: 'g2', // Moderately differentiated
  G3: 'g3', // Poorly differentiated
  G4: 'g4', // Undifferentiated
  GX: 'gx', // Cannot be assessed
} as const;

export type CancerGrade = typeof CancerGrade[keyof typeof CancerGrade];

export const CancerType = {
  Breast: 'breast',
  Lung: 'lung',
  Colorectal: 'colorectal',
  Prostate: 'prostate',
  Pancreatic: 'pancreatic',
  Liver: 'liver',
  Stomach: 'stomach',
  Kidney: 'kidney',
  Bladder: 'bladder',
  Thyroid: 'thyroid',
  Melanoma: 'melanoma',
  Leukemia: 'leukemia',
  Lymphoma: 'lymphoma',
  Ovarian: 'ovarian',
  Cervical: 'cervical',
  Uterine: 'uterine',
  Brain: 'brain',
  Esophageal: 'esophageal',
  Other: 'other',
} as const;

export type CancerType = typeof CancerType[keyof typeof CancerType];

export interface CancerDiagnosis {
  id: number;
  patientId: number;
  patientName?: string;
  diagnosisDate: string;
  cancerType: CancerType;
  stage: CancerStage;
  grade: CancerGrade;
  primarySite: string;
  metastasis: boolean;
  metastaticSites?: string[];
  histology: string;
  diagnosedBy: string;
  tnmStaging?: {
    t: string; // Tumor size
    n: string; // Lymph node involvement
    m: string; // Metastasis
  };
  biomarkers?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCancerDiagnosisRequest {
  patientId: number;
  diagnosisDate: string;
  cancerType: CancerType;
  stage: CancerStage;
  grade: CancerGrade;
  primarySite: string;
  metastasis: boolean;
  metastaticSites?: string[];
  histology: string;
  diagnosedBy: string;
  tnmStaging?: {
    t: string;
    n: string;
    m: string;
  };
  biomarkers?: string;
  notes?: string;
}

// ============================================
// CHEMOTHERAPY PROTOCOL TYPES
// ============================================

export const ChemoProtocolStatus = {
  Planned: 'planned',
  Active: 'active',
  Completed: 'completed',
  Discontinued: 'discontinued',
  OnHold: 'on_hold',
} as const;

export type ChemoProtocolStatus = typeof ChemoProtocolStatus[keyof typeof ChemoProtocolStatus];

export interface DrugDosage {
  drugName: string;
  dosage: number;
  unit: string; // mg, mg/m2, etc.
  route: string; // IV, oral, etc.
  frequency?: string;
}

export interface ChemotherapyProtocol {
  id: number;
  patientId: number;
  patientName?: string;
  diagnosisId: number;
  protocolName: string;
  drugs: DrugDosage[];
  schedule: string;
  cycles: number;
  completedCycles?: number;
  startDate: string;
  expectedEndDate?: string;
  actualEndDate?: string;
  status: ChemoProtocolStatus;
  prescribedBy?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateChemotherapyProtocolRequest {
  patientId: number;
  diagnosisId: number;
  protocolName: string;
  drugs: DrugDosage[];
  schedule: string;
  cycles: number;
  startDate: string;
  expectedEndDate?: string;
  prescribedBy?: string;
  notes?: string;
}

// ============================================
// CHEMOTHERAPY ADMINISTRATION TYPES
// ============================================

export const AdverseReactionSeverity = {
  Mild: 'mild',
  Moderate: 'moderate',
  Severe: 'severe',
  LifeThreatening: 'life_threatening',
} as const;

export type AdverseReactionSeverity =
  typeof AdverseReactionSeverity[keyof typeof AdverseReactionSeverity];

export interface AdverseReaction {
  reaction: string;
  severity: AdverseReactionSeverity;
  onset?: string;
  treatment?: string;
}

export interface ChemotherapyAdministration {
  id: number;
  protocolId: number;
  patientId: number;
  patientName?: string;
  cycleNumber: number;
  administeredDate: string;
  premedications?: string[];
  vitalSigns?: {
    bloodPressure?: string;
    heartRate?: number;
    temperature?: number;
    weight?: number;
  };
  adverseReactions?: AdverseReaction[];
  performedBy: string;
  duration?: number; // minutes
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateChemotherapyAdministrationRequest {
  protocolId: number;
  patientId: number;
  cycleNumber: number;
  administeredDate: string;
  premedications?: string[];
  vitalSigns?: {
    bloodPressure?: string;
    heartRate?: number;
    temperature?: number;
    weight?: number;
  };
  adverseReactions?: AdverseReaction[];
  performedBy: string;
  duration?: number;
  notes?: string;
}

// ============================================
// RADIATION THERAPY TYPES
// ============================================

export const RadiationStatus = {
  Planned: 'planned',
  InProgress: 'in_progress',
  Completed: 'completed',
  Discontinued: 'discontinued',
  OnHold: 'on_hold',
} as const;

export type RadiationStatus = typeof RadiationStatus[keyof typeof RadiationStatus];

export const RadiationTechnique = {
  EBRT: 'ebrt', // External Beam Radiation Therapy
  IMRT: 'imrt', // Intensity-Modulated Radiation Therapy
  IGRT: 'igrt', // Image-Guided Radiation Therapy
  Brachytherapy: 'brachytherapy',
  SRS: 'srs', // Stereotactic Radiosurgery
  SBRT: 'sbrt', // Stereotactic Body Radiation Therapy
  Proton: 'proton',
  Other: 'other',
} as const;

export type RadiationTechnique = typeof RadiationTechnique[keyof typeof RadiationTechnique];

export interface RadiationTherapy {
  id: number;
  patientId: number;
  patientName?: string;
  diagnosisId: number;
  treatmentSite: string;
  totalDose: number; // Gray (Gy)
  fractions: number;
  dosePerFraction?: number; // Gray (Gy)
  technique: RadiationTechnique;
  startDate: string;
  endDate?: string;
  completedFractions?: number;
  status: RadiationStatus;
  prescribedBy?: string;
  physicianNotes?: string;
  sideEffects?: string[];
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateRadiationTherapyRequest {
  patientId: number;
  diagnosisId: number;
  treatmentSite: string;
  totalDose: number;
  fractions: number;
  dosePerFraction?: number;
  technique: RadiationTechnique;
  startDate: string;
  endDate?: string;
  prescribedBy?: string;
  notes?: string;
}

// ============================================
// TUMOR MARKER TYPES
// ============================================

export const TumorMarkerType = {
  AFP: 'afp', // Alpha-fetoprotein
  CA125: 'ca125',
  CA153: 'ca153',
  CA199: 'ca199',
  CEA: 'cea', // Carcinoembryonic antigen
  PSA: 'psa', // Prostate-specific antigen
  HCG: 'hcg', // Human chorionic gonadotropin
  LDH: 'ldh', // Lactate dehydrogenase
  Calcitonin: 'calcitonin',
  Thyroglobulin: 'thyroglobulin',
  Other: 'other',
} as const;

export type TumorMarkerType = typeof TumorMarkerType[keyof typeof TumorMarkerType];

export const TumorMarkerTrend = {
  Increasing: 'increasing',
  Decreasing: 'decreasing',
  Stable: 'stable',
  Unknown: 'unknown',
} as const;

export type TumorMarkerTrend = typeof TumorMarkerTrend[keyof typeof TumorMarkerTrend];

export interface TumorMarker {
  id: number;
  patientId: number;
  patientName?: string;
  markerType: TumorMarkerType;
  value: number;
  unit: string;
  referenceRange: string;
  testDate: string;
  trend?: TumorMarkerTrend;
  previousValue?: number;
  orderedBy?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateTumorMarkerRequest {
  patientId: number;
  markerType: TumorMarkerType;
  value: number;
  unit: string;
  referenceRange: string;
  testDate: string;
  orderedBy?: string;
  notes?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface OncologyStatistics {
  // Patient stats
  totalPatients: number;
  activePatients?: number;
  newPatientsThisMonth?: number;

  // Diagnosis stats
  newDiagnosesThisMonth: number;
  diagnosedByStage?: Record<string, number>;
  diagnosedByType?: Record<string, number>;

  // Treatment stats
  activeTreatments: number;
  activeChemoProtocols?: number;
  activeRadiationTreatments?: number;
  completedTreatmentsThisMonth?: number;

  // Administration stats
  chemoAdministrationsToday?: number;
  chemoAdministrationsThisWeek?: number;
  radiationSessionsThisWeek?: number;

  // Marker stats
  tumorMarkersThisMonth?: number;
  abnormalMarkers?: number;

  // Trends
  cancerTypeDistribution?: Record<string, number>;
  stageDistribution?: Record<string, number>;
  treatmentTypeDistribution?: Record<string, number>;

  // Clinical outcomes
  remissionRate?: number;
  adverseReactionRate?: number;
}
