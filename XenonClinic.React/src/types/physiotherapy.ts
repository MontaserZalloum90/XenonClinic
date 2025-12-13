export const TreatmentPlanStatus = {
  Active: 0,
  Completed: 1,
  OnHold: 2,
  Cancelled: 3,
} as const;

export type TreatmentPlanStatus = (typeof TreatmentPlanStatus)[keyof typeof TreatmentPlanStatus];

export interface PhysiotherapyPlan {
  id: number;
  patientId: number;
  patientName?: string;
  diagnosis: string;
  goals: string[];
  startDate: string;
  endDate?: string;
  totalSessions: number;
  completedSessions: number;
  frequency: string;
  status: TreatmentPlanStatus;
  notes?: string;
  createdBy?: string;
  createdAt?: string;
}

export interface PhysiotherapySession {
  id: number;
  planId: number;
  patientId: number;
  patientName?: string;
  sessionDate: string;
  sessionNumber: number;
  treatments: string[];
  painLevelBefore?: number;
  painLevelAfter?: number;
  progressNotes?: string;
  homeExercises?: string;
  performedBy?: string;
  duration: number;
}

export interface ROMAssessment {
  id: number;
  patientId: number;
  patientName?: string;
  assessmentDate: string;
  joint: string;
  movement: string;
  activeROM: number;
  passiveROM: number;
  normalROM: number;
  painOnMovement?: boolean;
  notes?: string;
  assessedBy?: string;
}

export interface ExerciseProgram {
  id: number;
  patientId: number;
  patientName?: string;
  planId?: number;
  exercises: Exercise[];
  assignedDate: string;
  frequency: string;
  notes?: string;
  assignedBy?: string;
}

export interface Exercise {
  name: string;
  sets: number;
  reps: number;
  holdTime?: number;
  instructions?: string;
}

export interface PhysiotherapyStatistics {
  activePatients: number;
  sessionsToday: number;
  sessionsThisWeek: number;
  activePlans: number;
  completedPlansThisMonth: number;
  averageSessionDuration: number;
}
