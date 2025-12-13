export const ClinicalVisitStatus = {
  Scheduled: 0,
  InProgress: 1,
  Completed: 2,
  Cancelled: 3,
  NoShow: 4,
} as const;

export type ClinicalVisitStatus = typeof ClinicalVisitStatus[keyof typeof ClinicalVisitStatus];

export const VisitType = {
  Consultation: 0,
  FollowUp: 1,
  Emergency: 2,
  Procedure: 3,
  Checkup: 4,
} as const;

export type VisitType = typeof VisitType[keyof typeof VisitType];

export interface VitalSigns {
  bloodPressure?: string;
  heartRate?: number;
  temperature?: number;
  weight?: number;
  height?: number;
  oxygenSaturation?: number;
}

export interface ClinicalVisit {
  id: number;
  visitNumber: string;
  patientId: number;
  patientName?: string;
  doctorId: number;
  doctorName?: string;
  visitDate: string;
  visitType: VisitType;
  chiefComplaint?: string;
  diagnosis?: string;
  treatmentPlan?: string;
  notes?: string;
  vitalSigns?: VitalSigns;
  status: ClinicalVisitStatus;
  followUpDate?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface ClinicalVisitStatistics {
  totalVisits: number;
  todaysVisits: number;
  inProgressVisits: number;
  completedToday?: number;
  scheduledToday?: number;
  visitTypeDistribution?: Record<string, number>;
}

export interface ClinicalVisitFormData {
  visitNumber: string;
  patientId: number;
  doctorId: number;
  visitDate: string;
  visitType: VisitType;
  chiefComplaint?: string;
  diagnosis?: string;
  treatmentPlan?: string;
  notes?: string;
  vitalSigns?: VitalSigns;
  status: ClinicalVisitStatus;
  followUpDate?: string;
}
