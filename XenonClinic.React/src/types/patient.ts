// Patient Types matching backend entities

export interface Patient {
  id: number;
  branchId: number;
  emiratesId: string;
  fullNameEn: string;
  fullNameAr?: string;
  dateOfBirth: string; // ISO date string
  gender: string; // M or F
  phoneNumber?: string;
  email?: string;
  hearingLossType?: string;
  notes?: string;
  branch?: {
    id: number;
    name: string;
  };
}

export interface CreatePatientRequest {
  emiratesId: string;
  fullNameEn: string;
  fullNameAr?: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber?: string;
  email?: string;
  hearingLossType?: string;
  notes?: string;
}

export interface UpdatePatientRequest extends CreatePatientRequest {
  id: number;
  branchId: number;
}

export interface PatientStatistics {
  totalPatients: number;
  newPatientsThisMonth: number;
  activeCases: number;
  overdueCases: number;
  genderDistribution: Record<string, number>;
}
