export const PregnancyStatus = {
  Active: 0,
  Delivered: 1,
  Miscarriage: 2,
  Ectopic: 3,
  Terminated: 4,
} as const;

export type PregnancyStatus = (typeof PregnancyStatus)[keyof typeof PregnancyStatus];

export interface Pregnancy {
  id: number;
  patientId: number;
  patientName?: string;
  lmp: string;
  edd: string;
  gestationalAge: number;
  gravida: number;
  para: number;
  status: PregnancyStatus;
  riskFactors?: string[];
  notes?: string;
  createdBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface PrenatalVisit {
  id: number;
  pregnancyId: number;
  patientId: number;
  patientName?: string;
  visitDate: string;
  gestationalAge: number;
  weight: number;
  bloodPressure: string;
  fundalHeight?: number;
  fetalHeartRate?: number;
  fetalMovement?: boolean;
  urineProtein?: string;
  urineGlucose?: string;
  edema?: string;
  complaints?: string;
  notes?: string;
  nextVisitDate?: string;
  performedBy?: string;
}

export interface ObstetricUltrasound {
  id: number;
  pregnancyId: number;
  patientId: number;
  patientName?: string;
  ultrasoundDate: string;
  gestationalAge: number;
  fetalHeartRate?: number;
  bpd?: number;
  hc?: number;
  ac?: number;
  fl?: number;
  estimatedWeight?: number;
  amnioticFluid?: string;
  placentaLocation?: string;
  findings?: string;
  performedBy?: string;
}

export interface PapSmear {
  id: number;
  patientId: number;
  patientName?: string;
  collectionDate: string;
  indication?: string;
  result?: string;
  bethesdaCategory?: string;
  hpvStatus?: string;
  recommendations?: string;
  nextDueDate?: string;
  performedBy?: string;
  reportedDate?: string;
}

export interface OBGYNStatistics {
  activePregnancies: number;
  prenatalVisitsToday: number;
  ultrasoundsThisWeek: number;
  papSmearsThisMonth: number;
  deliveriesThisMonth: number;
  highRiskPregnancies: number;
}
