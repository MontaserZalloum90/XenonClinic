export interface EarExam {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  ear: 'left' | 'right' | 'both';
  externalCanal: string;
  tympanicMembrane: string;
  middleEar?: string;
  hearingAssessment?: string;
  diagnosis?: string;
  treatment?: string;
  performedBy?: string;
}

export interface NasalEndoscopy {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  indication: string;
  septum: string;
  turbinates: string;
  nasopharynx?: string;
  findings: string;
  diagnosis?: string;
  recommendations?: string;
  performedBy?: string;
}

export interface Laryngoscopy {
  id: number;
  patientId: number;
  patientName?: string;
  examDate: string;
  type: 'indirect' | 'flexible' | 'rigid';
  indication: string;
  vocalCords: string;
  epiglottis?: string;
  arytenoids?: string;
  mobility: string;
  findings: string;
  diagnosis?: string;
  performedBy?: string;
}

export interface TympanometryTest {
  id: number;
  patientId: number;
  patientName?: string;
  testDate: string;
  ear: 'left' | 'right';
  tympanogramType: 'A' | 'As' | 'Ad' | 'B' | 'C';
  peakPressure?: number;
  compliance?: number;
  earCanalVolume?: number;
  interpretation: string;
  performedBy?: string;
}

export interface ENTStatistics {
  examsToday: number;
  endoscopiesThisWeek: number;
  laryngoscopiesThisMonth: number;
  tympanometriesThisWeek: number;
  pendingFollowUps: number;
}
