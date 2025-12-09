// Audiology Types for hearing clinics

// ============================================
// AUDIOGRAM TYPES
// ============================================

export const EarSide = {
  Left: 'left',
  Right: 'right',
} as const;

export type EarSide = typeof EarSide[keyof typeof EarSide];

export const TestType = {
  AirConduction: 'air',
  BoneConduction: 'bone',
  SpeechRecognition: 'speech',
  Tympanometry: 'tympanometry',
} as const;

export type TestType = typeof TestType[keyof typeof TestType];

export const HearingLossGrade = {
  Normal: 'normal',           // -10 to 25 dB
  Mild: 'mild',               // 26 to 40 dB
  Moderate: 'moderate',       // 41 to 55 dB
  ModeratelySevere: 'moderately_severe', // 56 to 70 dB
  Severe: 'severe',           // 71 to 90 dB
  Profound: 'profound',       // 91+ dB
} as const;

export type HearingLossGrade = typeof HearingLossGrade[keyof typeof HearingLossGrade];

// Standard audiogram frequencies in Hz
export const AUDIOGRAM_FREQUENCIES = [250, 500, 1000, 2000, 4000, 8000] as const;
export type AudiogramFrequency = typeof AUDIOGRAM_FREQUENCIES[number];

export interface AudiogramDataPoint {
  frequency: AudiogramFrequency;
  threshold: number; // in dB HL (-10 to 120)
  noResponse?: boolean; // true if no response at max output
  masked?: boolean; // true if masking was used
}

export interface Audiogram {
  id: number;
  patientId: number;
  encounterId?: number;
  testDate: string;
  testedBy: string;

  // Air conduction thresholds
  rightEarAir: AudiogramDataPoint[];
  leftEarAir: AudiogramDataPoint[];

  // Bone conduction thresholds (optional)
  rightEarBone?: AudiogramDataPoint[];
  leftEarBone?: AudiogramDataPoint[];

  // Speech audiometry
  rightSRT?: number; // Speech Reception Threshold
  leftSRT?: number;
  rightWRS?: number; // Word Recognition Score (%)
  leftWRS?: number;

  // Tympanometry results
  rightTympanogram?: TympanogramResult;
  leftTympanogram?: TympanogramResult;

  // Calculated values
  rightPTA?: number; // Pure Tone Average
  leftPTA?: number;
  rightHearingLossGrade?: HearingLossGrade;
  leftHearingLossGrade?: HearingLossGrade;

  interpretation?: string;
  recommendations?: string;
  notes?: string;

  createdAt: string;
  updatedAt?: string;
}

export interface TympanogramResult {
  type: 'A' | 'As' | 'Ad' | 'B' | 'C'; // Jerger classification
  peakPressure?: number; // daPa
  compliance?: number; // ml
  earCanalVolume?: number; // ml
}

export interface CreateAudiogramRequest {
  patientId: number;
  encounterId?: number;
  testDate: string;
  rightEarAir: AudiogramDataPoint[];
  leftEarAir: AudiogramDataPoint[];
  rightEarBone?: AudiogramDataPoint[];
  leftEarBone?: AudiogramDataPoint[];
  rightSRT?: number;
  leftSRT?: number;
  rightWRS?: number;
  leftWRS?: number;
  rightTympanogram?: TympanogramResult;
  leftTympanogram?: TympanogramResult;
  interpretation?: string;
  recommendations?: string;
  notes?: string;
}

// ============================================
// HEARING AID TYPES
// ============================================

export const HearingAidStyle = {
  BTE: 'BTE',       // Behind-the-ear
  RIC: 'RIC',       // Receiver-in-canal
  ITE: 'ITE',       // In-the-ear
  ITC: 'ITC',       // In-the-canal
  CIC: 'CIC',       // Completely-in-canal
  IIC: 'IIC',       // Invisible-in-canal
  CROS: 'CROS',     // Contralateral Routing of Signal
  BiCROS: 'BiCROS', // Bilateral CROS
} as const;

export type HearingAidStyle = typeof HearingAidStyle[keyof typeof HearingAidStyle];

export const HearingAidStatus = {
  Active: 'active',
  InRepair: 'in_repair',
  Replaced: 'replaced',
  Lost: 'lost',
  Returned: 'returned',
} as const;

export type HearingAidStatus = typeof HearingAidStatus[keyof typeof HearingAidStatus];

export const FittingStatus = {
  Scheduled: 'scheduled',
  InitialFitting: 'initial_fitting',
  FollowUp: 'follow_up',
  Adjusted: 'adjusted',
  Completed: 'completed',
} as const;

export type FittingStatus = typeof FittingStatus[keyof typeof FittingStatus];

export interface HearingAid {
  id: number;
  patientId: number;

  // Device info
  ear: EarSide;
  manufacturer: string;
  model: string;
  serialNumber: string;
  style: HearingAidStyle;
  technologyLevel?: string; // e.g., "Premium", "Advanced", "Essential"

  // Purchase/warranty
  purchaseDate: string;
  warrantyStartDate: string;
  warrantyEndDate: string;
  warrantyExtended?: boolean;
  purchasePrice?: number;

  // Status
  status: HearingAidStatus;

  // Current settings (simplified)
  currentProgramCount?: number;
  lastProgrammingDate?: string;

  notes?: string;
  createdAt: string;
  updatedAt?: string;

  // Related data
  fittings?: HearingAidFitting[];
  adjustments?: HearingAidAdjustment[];
}

export interface HearingAidFitting {
  id: number;
  hearingAidId: number;
  patientId: number;
  encounterId?: number;

  fittingDate: string;
  fittedBy: string;
  status: FittingStatus;

  // Fitting details
  targetMatch?: number; // % match to prescriptive target
  realEarMeasurement?: boolean;
  verificationMethod?: string;

  // Patient feedback
  comfortRating?: number; // 1-10
  soundQualityRating?: number; // 1-10

  // Settings applied
  gain?: string; // JSON or description
  compression?: string;
  programs?: string[];

  followUpDate?: string;
  notes?: string;

  createdAt: string;
}

export interface HearingAidAdjustment {
  id: number;
  hearingAidId: number;
  patientId: number;
  encounterId?: number;

  adjustmentDate: string;
  adjustedBy: string;

  reason: string;
  changesDescription: string;

  // Before/after (optional)
  previousSettings?: string;
  newSettings?: string;

  patientSatisfaction?: number; // 1-10
  notes?: string;

  createdAt: string;
}

export interface CreateHearingAidRequest {
  patientId: number;
  ear: EarSide;
  manufacturer: string;
  model: string;
  serialNumber: string;
  style: HearingAidStyle;
  technologyLevel?: string;
  purchaseDate: string;
  warrantyStartDate: string;
  warrantyEndDate: string;
  purchasePrice?: number;
  notes?: string;
}

// ============================================
// ENCOUNTER/VISIT TYPES
// ============================================

export const EncounterStatus = {
  Scheduled: 'scheduled',
  CheckedIn: 'checked_in',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
  NoShow: 'no_show',
} as const;

export type EncounterStatus = typeof EncounterStatus[keyof typeof EncounterStatus];

export const EncounterType = {
  InitialConsultation: 'initial_consultation',
  FollowUp: 'follow_up',
  HearingTest: 'hearing_test',
  HearingAidFitting: 'hearing_aid_fitting',
  HearingAidAdjustment: 'hearing_aid_adjustment',
  HearingAidRepair: 'hearing_aid_repair',
  Counseling: 'counseling',
  TinnitusEvaluation: 'tinnitus_evaluation',
  BalanceAssessment: 'balance_assessment',
  CochlearImplantMapping: 'ci_mapping',
} as const;

export type EncounterType = typeof EncounterType[keyof typeof EncounterType];

export const TaskStatus = {
  Pending: 'pending',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
} as const;

export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];

export const TaskPriority = {
  Low: 'low',
  Normal: 'normal',
  High: 'high',
  Urgent: 'urgent',
} as const;

export type TaskPriority = typeof TaskPriority[keyof typeof TaskPriority];

export interface Encounter {
  id: number;
  patientId: number;
  branchId: number;
  appointmentId?: number;

  encounterDate: string;
  encounterType: EncounterType;
  status: EncounterStatus;

  // Provider info
  providerId?: number;
  providerName?: string;

  // SOAP-like structure for audiologists
  chiefComplaint?: string;
  historyOfPresentIllness?: string;

  // Assessment
  assessment?: EncounterAssessment;

  // Plan
  plan?: EncounterPlan;

  // Tasks generated from this encounter
  tasks?: EncounterTask[];

  // Related records
  audiogramId?: number;
  hearingAidFittingId?: number;

  // Timing
  startTime?: string;
  endTime?: string;
  duration?: number; // minutes

  notes?: string;
  internalNotes?: string; // Not shown to patient

  createdBy: string;
  createdAt: string;
  updatedAt?: string;

  // Nested data
  patient?: {
    id: number;
    fullNameEn: string;
    phoneNumber?: string;
    email?: string;
  };
}

export interface EncounterAssessment {
  // Hearing assessment
  hearingStatus?: string;
  hearingChangeFromLast?: 'improved' | 'stable' | 'declined' | 'unknown';

  // Current hearing aids
  currentHearingAids?: string;
  hearingAidPerformance?: string;

  // Diagnoses (ICD-10 codes)
  diagnoses?: Diagnosis[];

  // Clinical findings
  otoscopyRight?: string;
  otoscopyLeft?: string;
  additionalFindings?: string;

  // Summary
  clinicalImpression?: string;
}

export interface Diagnosis {
  code: string; // ICD-10
  description: string;
  isPrimary?: boolean;
}

export interface EncounterPlan {
  // Treatment plan
  treatmentPlan?: string;

  // Recommendations
  recommendations?: string[];

  // Hearing aid recommendation
  hearingAidRecommendation?: {
    recommended: boolean;
    style?: HearingAidStyle;
    technology?: string;
    bilateral?: boolean;
    rationale?: string;
  };

  // Follow-up
  followUpRequired?: boolean;
  followUpInterval?: string; // e.g., "2 weeks", "3 months"
  followUpReason?: string;

  // Referrals
  referrals?: Referral[];

  // Patient education provided
  educationProvided?: string[];

  // Additional orders
  additionalTestsOrdered?: string[];
}

export interface Referral {
  referTo: string;
  specialty?: string;
  reason: string;
  urgent?: boolean;
}

export interface EncounterTask {
  id: number;
  encounterId: number;
  patientId: number;

  title: string;
  description?: string;

  status: TaskStatus;
  priority: TaskPriority;

  assignedTo?: string;
  dueDate?: string;
  completedDate?: string;
  completedBy?: string;

  category?: string; // e.g., "Follow-up call", "Order hearing aids", "Schedule appointment"

  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateEncounterRequest {
  patientId: number;
  appointmentId?: number;
  encounterDate: string;
  encounterType: EncounterType;
  providerId?: number;
  chiefComplaint?: string;
  historyOfPresentIllness?: string;
  notes?: string;
}

export interface CreateEncounterTaskRequest {
  encounterId: number;
  patientId: number;
  title: string;
  description?: string;
  priority: TaskPriority;
  assignedTo?: string;
  dueDate?: string;
  category?: string;
}

// ============================================
// CONSENT & ATTACHMENT TYPES
// ============================================

export const ConsentType = {
  GeneralTreatment: 'general_treatment',
  HearingTest: 'hearing_test',
  HearingAidTrial: 'hearing_aid_trial',
  HearingAidPurchase: 'hearing_aid_purchase',
  ReleaseOfInformation: 'release_of_information',
  Photography: 'photography',
  Research: 'research',
  Telehealth: 'telehealth',
} as const;

export type ConsentType = typeof ConsentType[keyof typeof ConsentType];

export const ConsentStatus = {
  Pending: 'pending',
  Signed: 'signed',
  Declined: 'declined',
  Revoked: 'revoked',
  Expired: 'expired',
} as const;

export type ConsentStatus = typeof ConsentStatus[keyof typeof ConsentStatus];

export const AttachmentCategory = {
  ConsentForm: 'consent_form',
  Audiogram: 'audiogram',
  ReferralLetter: 'referral_letter',
  MedicalReport: 'medical_report',
  InsuranceDocument: 'insurance_document',
  HearingAidManual: 'hearing_aid_manual',
  WarrantyDocument: 'warranty_document',
  PatientPhoto: 'patient_photo',
  CorrespondenceIn: 'correspondence_in',
  CorrespondenceOut: 'correspondence_out',
  Other: 'other',
} as const;

export type AttachmentCategory = typeof AttachmentCategory[keyof typeof AttachmentCategory];

export interface ConsentForm {
  id: number;
  patientId: number;
  encounterId?: number;

  consentType: ConsentType;
  status: ConsentStatus;

  // Form details
  formVersion?: string;
  formContent?: string; // HTML or markdown content shown to patient

  // Signature
  signedAt?: string;
  signedBy?: string; // Patient name as signed
  signatureMethod?: 'electronic' | 'paper' | 'verbal';
  signatureData?: string; // Base64 signature image for electronic

  // Witness (if required)
  witnessName?: string;
  witnessSignedAt?: string;

  // Expiration
  expiresAt?: string;

  // Revocation
  revokedAt?: string;
  revokedReason?: string;

  notes?: string;

  // Attachment reference
  attachmentId?: number;

  createdBy: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Attachment {
  id: number;
  patientId: number;
  encounterId?: number;

  category: AttachmentCategory;

  fileName: string;
  originalFileName: string;
  fileSize: number; // bytes
  mimeType: string;

  title?: string;
  description?: string;

  // Storage
  storagePath: string;
  thumbnailPath?: string;

  // Metadata
  uploadedBy: string;
  uploadedAt: string;

  // Soft delete
  isDeleted?: boolean;
  deletedAt?: string;
  deletedBy?: string;
}

export interface CreateConsentRequest {
  patientId: number;
  encounterId?: number;
  consentType: ConsentType;
  formVersion?: string;
  expiresAt?: string;
}

export interface SignConsentRequest {
  consentId: number;
  signedBy: string;
  signatureMethod: 'electronic' | 'paper' | 'verbal';
  signatureData?: string;
  witnessName?: string;
}

export interface UploadAttachmentRequest {
  patientId: number;
  encounterId?: number;
  category: AttachmentCategory;
  title?: string;
  description?: string;
  file: File;
}

// ============================================
// STATISTICS
// ============================================

export interface AudiologyStatistics {
  // Patient stats
  totalPatients: number;
  newPatientsThisMonth: number;

  // Encounter stats
  encountersToday: number;
  encountersThisWeek: number;
  encountersThisMonth: number;

  // Hearing aid stats
  hearingAidsFittedThisMonth: number;
  activeHearingAids: number;
  warrantyExpiringSoon: number; // Within 30 days

  // Audiogram stats
  audiogramsThisMonth: number;

  // Task stats
  pendingTasks: number;
  overdueTasks: number;

  // Revenue (if applicable)
  monthlyRevenue?: number;

  // Distributions
  encounterTypeDistribution: Record<string, number>;
  hearingLossGradeDistribution: Record<string, number>;
}
