// Pediatrics Types for pediatric clinics

// ============================================
// GROWTH MEASUREMENT TYPES
// ============================================

export interface GrowthMeasurement {
  id: number;
  patientId: number;
  patientName?: string;
  measurementDate: string;
  age: number; // months
  weight: number; // kg
  height: number; // cm
  headCircumference?: number; // cm (typically for infants/toddlers)
  bmi: number;
  weightPercentile?: number;
  heightPercentile?: number;
  bmiPercentile?: number;
  headCircumferencePercentile?: number;
  recordedBy: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateGrowthMeasurementRequest {
  patientId: number;
  measurementDate: string;
  age: number;
  weight: number;
  height: number;
  headCircumference?: number;
  recordedBy: string;
  notes?: string;
}

// ============================================
// DEVELOPMENT MILESTONE TYPES
// ============================================

export const MilestoneCategory = {
  Social: 'social',
  Language: 'language',
  Cognitive: 'cognitive',
  Motor: 'motor',
  Fine_Motor: 'fine_motor',
  Gross_Motor: 'gross_motor',
  Emotional: 'emotional',
} as const;

export type MilestoneCategory = typeof MilestoneCategory[keyof typeof MilestoneCategory];

export const MilestoneStatus = {
  NotYetAchieved: 'not_yet_achieved',
  Achieved: 'achieved',
  Delayed: 'delayed',
  Concerning: 'concerning',
} as const;

export type MilestoneStatus = typeof MilestoneStatus[keyof typeof MilestoneStatus];

export interface DevelopmentMilestone {
  id: number;
  patientId: number;
  patientName?: string;
  milestoneCategory: MilestoneCategory;
  milestoneName: string;
  expectedAge: number; // months
  achievedDate?: string;
  status: MilestoneStatus;
  notes?: string;
  assessedBy: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateDevelopmentMilestoneRequest {
  patientId: number;
  milestoneCategory: MilestoneCategory;
  milestoneName: string;
  expectedAge: number;
  achievedDate?: string;
  status: MilestoneStatus;
  notes?: string;
  assessedBy: string;
}

// ============================================
// VACCINATION RECORD TYPES
// ============================================

export const VaccinationStatus = {
  Scheduled: 'scheduled',
  Administered: 'administered',
  Missed: 'missed',
  Declined: 'declined',
  Delayed: 'delayed',
} as const;

export type VaccinationStatus = typeof VaccinationStatus[keyof typeof VaccinationStatus];

export const VaccineName = {
  // Routine childhood vaccines
  DTaP: 'DTaP', // Diphtheria, Tetanus, Pertussis
  Tdap: 'Tdap',
  IPV: 'IPV', // Polio
  MMR: 'MMR', // Measles, Mumps, Rubella
  Varicella: 'Varicella', // Chickenpox
  HepA: 'HepA', // Hepatitis A
  HepB: 'HepB', // Hepatitis B
  HIB: 'HIB', // Haemophilus influenzae type b
  PCV: 'PCV', // Pneumococcal
  RV: 'RV', // Rotavirus
  Flu: 'Flu', // Influenza
  MenACWY: 'MenACWY', // Meningococcal
  MenB: 'MenB',
  HPV: 'HPV', // Human Papillomavirus
  COVID19: 'COVID-19',
  Other: 'Other',
} as const;

export type VaccineName = typeof VaccineName[keyof typeof VaccineName];

export interface VaccinationRecord {
  id: number;
  patientId: number;
  patientName?: string;
  vaccineName: VaccineName | string;
  doseNumber: number;
  administeredDate?: string;
  scheduledDate?: string;
  batchNumber?: string;
  site?: string; // injection site (e.g., "left deltoid", "right thigh")
  administeredBy?: string;
  nextDueDate?: string;
  status?: VaccinationStatus;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateVaccinationRecordRequest {
  patientId: number;
  vaccineName: VaccineName | string;
  doseNumber: number;
  administeredDate?: string;
  scheduledDate?: string;
  batchNumber?: string;
  site?: string;
  administeredBy?: string;
  nextDueDate?: string;
  status?: VaccinationStatus;
  notes?: string;
}

// ============================================
// DOSING CALCULATION TYPES
// ============================================

export interface DosingCalculation {
  medicationName: string;
  patientWeight: number; // kg
  dosePerKg: number; // mg/kg
  frequency: string; // e.g., "every 8 hours", "twice daily"
  route: string; // e.g., "oral", "IV", "IM"
  calculatedDose: number; // mg
  maximumDose?: number; // mg
  finalDose: number; // mg (considering max dose)
  warnings?: string[];
  notes?: string;
}

export interface DosingRequest {
  medicationName: string;
  patientWeight: number;
  dosePerKg: number;
  frequency: string;
  route: string;
  maximumDose?: number;
}

// ============================================
// STATISTICS
// ============================================

export interface PediatricStatistics {
  // Patient stats
  totalPediatricPatients: number;
  newPatientsThisMonth?: number;

  // Growth tracking
  growthMeasurementsThisMonth: number;
  growthMeasurementsToday?: number;

  // Milestones
  milestonesTracked: number;
  milestonesDelayed?: number;
  milestonesAchievedThisMonth?: number;

  // Vaccinations
  vaccinationsThisMonth: number;
  vaccinationsToday?: number;
  overdueVaccinations: number;
  upcomingVaccinations?: number;

  // Age distribution
  ageDistribution?: {
    infants?: number; // 0-12 months
    toddlers?: number; // 1-3 years
    preschool?: number; // 3-5 years
    schoolAge?: number; // 5-12 years
    adolescents?: number; // 12-18 years
  };

  // Concerns
  growthConcerns?: number;
  developmentConcerns?: number;
}

// ============================================
// VISIT TYPES
// ============================================

export const PediatricVisitType = {
  WellChild: 'well_child',
  SickVisit: 'sick_visit',
  FollowUp: 'follow_up',
  Vaccination: 'vaccination',
  Development: 'development',
  Nutrition: 'nutrition',
} as const;

export type PediatricVisitType = typeof PediatricVisitType[keyof typeof PediatricVisitType];

export interface PediatricVisit {
  id: number;
  patientId: number;
  patientName?: string;
  visitDate: string;
  visitType: PediatricVisitType;
  age: number; // months
  weight?: number;
  height?: number;
  chiefComplaint?: string;
  diagnosis?: string;
  treatment?: string;
  vaccinations?: string[];
  nextVisitDate?: string;
  notes?: string;
  performedBy: string;
  createdAt?: string;
  updatedAt?: string;
}
