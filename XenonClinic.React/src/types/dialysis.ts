export const DialysisSessionStatus = {
  Scheduled: 0,
  InProgress: 1,
  Completed: 2,
  Cancelled: 3,
  Incomplete: 4,
} as const;

export type DialysisSessionStatus = (typeof DialysisSessionStatus)[keyof typeof DialysisSessionStatus];

export interface DialysisPatient {
  id: number;
  patientId: number;
  patientName?: string;
  dialysisType: 'hemodialysis' | 'peritoneal';
  accessType: string;
  dryWeight: number;
  schedule: string;
  startDate: string;
  nephrologist?: string;
  notes?: string;
}

export interface DialysisSession {
  id: number;
  patientId: number;
  patientName?: string;
  sessionDate: string;
  machineNumber?: string;
  preWeight: number;
  postWeight: number;
  ufGoal: number;
  ufAchieved: number;
  duration: number;
  bloodFlowRate: number;
  dialysateFlowRate: number;
  preBP: string;
  postBP: string;
  accessUsed: string;
  complications?: string;
  medications?: string;
  status: DialysisSessionStatus;
  performedBy?: string;
  notes?: string;
}

export interface DialysisSchedule {
  id: number;
  patientId: number;
  patientName?: string;
  dayOfWeek: number;
  shift: 'morning' | 'afternoon' | 'evening';
  machineNumber?: string;
  startTime: string;
  duration: number;
  isActive: boolean;
}

export interface DialysisLabResult {
  id: number;
  patientId: number;
  patientName?: string;
  labDate: string;
  bun: number;
  creatinine: number;
  potassium: number;
  phosphorus: number;
  calcium: number;
  hemoglobin: number;
  albumin?: number;
  pth?: number;
  ktv?: number;
  urr?: number;
  notes?: string;
}

export interface DialysisStatistics {
  activePatients: number;
  sessionsToday: number;
  sessionsThisWeek: number;
  averageKtV: number;
  averageURR: number;
  incompleteSessionsThisMonth: number;
}
