export const IVFCycleStatus = {
  Planning: 0,
  Stimulation: 1,
  Retrieval: 2,
  Fertilization: 3,
  Transfer: 4,
  WaitingResult: 5,
  Positive: 6,
  Negative: 7,
  Cancelled: 8,
} as const;

export type IVFCycleStatus = (typeof IVFCycleStatus)[keyof typeof IVFCycleStatus];

export interface FertilityAssessment {
  id: number;
  patientId: number;
  patientName?: string;
  partnerId?: number;
  partnerName?: string;
  assessmentDate: string;
  infertilityDuration: number;
  primaryInfertility: boolean;
  femaleFactors?: string[];
  maleFactors?: string[];
  diagnosis: string;
  recommendedTreatment: string;
  prognosis?: string;
  performedBy?: string;
}

export interface IVFCycle {
  id: number;
  patientId: number;
  patientName?: string;
  cycleNumber: number;
  startDate: string;
  protocol: string;
  stimulationDays?: number;
  triggerDate?: string;
  retrievalDate?: string;
  transferDate?: string;
  status: IVFCycleStatus;
  notes?: string;
  managedBy?: string;
}

export interface IVFMonitoring {
  id: number;
  cycleId: number;
  patientId: number;
  monitoringDate: string;
  dayOfStimulation: number;
  e2Level?: number;
  lhLevel?: number;
  progesterone?: number;
  endometriumThickness?: number;
  leftFollicles?: string;
  rightFollicles?: string;
  medications?: string;
  nextAppointment?: string;
  performedBy?: string;
}

export interface OocyteRetrieval {
  id: number;
  cycleId: number;
  patientId: number;
  retrievalDate: string;
  oocytesRetrieved: number;
  matureOocytes: number;
  immatureOocytes: number;
  abnormalOocytes: number;
  complications?: string;
  performedBy?: string;
}

export interface EmbryoRecord {
  id: number;
  cycleId: number;
  patientId: number;
  embryoNumber: number;
  fertilizationDate: string;
  day: number;
  grade: string;
  cellCount?: number;
  fragmentation?: string;
  status: 'developing' | 'transferred' | 'frozen' | 'discarded';
  notes?: string;
}

export interface FertilityStatistics {
  activePatients: number;
  activeCycles: number;
  retrievalsThisMonth: number;
  transfersThisMonth: number;
  positiveResults: number;
  frozenEmbryos: number;
}
