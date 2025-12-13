/**
 * Medical Services Demo Data
 * Comprehensive demo data for dialysis, ENT, fertility, OB/GYN,
 * clinical visits, analytics, workflow, and portal modules
 */

import type {
  DialysisPatient,
  DialysisSession,
  DialysisSchedule,
  DialysisLabResult,
  DialysisSessionStatus,
} from "../../types/dialysis";
import type {
  EarExam,
  NasalEndoscopy,
  Laryngoscopy,
  TympanometryTest,
} from "../../types/ent";
import type {
  FertilityAssessment,
  IVFCycle,
  IVFMonitoring,
  OocyteRetrieval,
  EmbryoRecord,
  IVFCycleStatus,
} from "../../types/fertility";
import type {
  Pregnancy,
  PrenatalVisit,
  ObstetricUltrasound,
  PapSmear,
  PregnancyStatus,
} from "../../types/obgyn";
import type {
  ClinicalVisit,
  ClinicalVisitStatus,
  VisitType,
} from "../../types/clinical-visit";
import type {
  AnalyticsDashboard,
  Report,
  ReportType,
  ReportStatus,
} from "../../types/analytics";
import type {
  WorkflowDefinition,
  WorkflowInstance,
  WorkflowStatus,
  WorkflowStepType,
} from "../../types/workflow";
import type {
  PortalUser,
  PortalDocument,
  PortalDocumentType,
} from "../../types/portal";

// ============================================================
// HELPER FUNCTIONS
// ============================================================

const getDateString = (daysOffset: number): string => {
  const date = new Date();
  date.setDate(date.getDate() + daysOffset);
  return date.toISOString().split("T")[0];
};

// Helper functions for readable date generation
const daysAgo = (days: number) => getDateString(-days);
const daysFromNow = (days: number) => getDateString(days);
const weeksAgo = (weeks: number) => getDateString(-weeks * 7);
const monthsAgo = (months: number) => getDateString(-months * 30);

// ============================================================
// DIALYSIS MODULE
// ============================================================

const dialysisPatients: DialysisPatient[] = [
  {
    id: 1,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    dialysisType: "hemodialysis",
    accessType: "AV Fistula - Left Arm",
    dryWeight: 72.5,
    schedule: "Mon/Wed/Fri - Morning",
    startDate: monthsAgo(6),
    nephrologist: "Dr. Fatima Al Hashimi",
    notes: "Stable on current regimen. Good fistula maturation.",
  },
  {
    id: 2,
    patientId: 102,
    patientName: "Maryam Hassan",
    dialysisType: "hemodialysis",
    accessType: "Tunneled Catheter - Right IJ",
    dryWeight: 58.0,
    schedule: "Tue/Thu/Sat - Afternoon",
    startDate: monthsAgo(2),
    nephrologist: "Dr. Ahmed Rashid",
    notes: "Awaiting fistula creation. Catheter functioning well.",
  },
  {
    id: 3,
    patientId: 103,
    patientName: "Omar Farooq",
    dialysisType: "peritoneal",
    accessType: "PD Catheter",
    dryWeight: 85.0,
    schedule: "CAPD - 4 exchanges daily",
    startDate: monthsAgo(12),
    nephrologist: "Dr. Fatima Al Hashimi",
    notes: "Experienced PD patient. Self-manages well at home.",
  },
];

const dialysisSessions: DialysisSession[] = [
  {
    id: 1,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    sessionDate: daysAgo(0),
    machineNumber: "HD-05",
    preWeight: 74.8,
    postWeight: 72.6,
    ufGoal: 2.2,
    ufAchieved: 2.2,
    duration: 240,
    bloodFlowRate: 350,
    dialysateFlowRate: 500,
    preBP: "145/90",
    postBP: "130/80",
    accessUsed: "AV Fistula - Left Arm",
    status: 2 as DialysisSessionStatus, // Completed
    performedBy: "Nurse Aisha",
    notes: "Session completed without complications.",
  },
  {
    id: 2,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    sessionDate: daysAgo(2),
    machineNumber: "HD-05",
    preWeight: 75.1,
    postWeight: 72.5,
    ufGoal: 2.5,
    ufAchieved: 2.6,
    duration: 240,
    bloodFlowRate: 350,
    dialysateFlowRate: 500,
    preBP: "150/95",
    postBP: "125/78",
    accessUsed: "AV Fistula - Left Arm",
    status: 2 as DialysisSessionStatus,
    performedBy: "Nurse Aisha",
    notes: "Minor hypotension at 3hr mark, resolved with saline.",
  },
  {
    id: 3,
    patientId: 102,
    patientName: "Maryam Hassan",
    sessionDate: daysAgo(0),
    machineNumber: "HD-03",
    preWeight: 60.5,
    postWeight: 58.2,
    ufGoal: 2.3,
    ufAchieved: 2.3,
    duration: 210,
    bloodFlowRate: 300,
    dialysateFlowRate: 500,
    preBP: "138/85",
    postBP: "128/80",
    accessUsed: "Tunneled Catheter - Right IJ",
    status: 1 as DialysisSessionStatus, // InProgress
    performedBy: "Nurse Mohammed",
    notes: "Session in progress. Patient comfortable.",
  },
];

const dialysisSchedules: DialysisSchedule[] = [
  {
    id: 1,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    dayOfWeek: 1, // Monday
    shift: "morning",
    machineNumber: "HD-05",
    startTime: "08:00",
    duration: 240,
    isActive: true,
  },
  {
    id: 2,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    dayOfWeek: 3, // Wednesday
    shift: "morning",
    machineNumber: "HD-05",
    startTime: "08:00",
    duration: 240,
    isActive: true,
  },
  {
    id: 3,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    dayOfWeek: 5, // Friday
    shift: "morning",
    machineNumber: "HD-05",
    startTime: "08:00",
    duration: 240,
    isActive: true,
  },
  {
    id: 4,
    patientId: 102,
    patientName: "Maryam Hassan",
    dayOfWeek: 2, // Tuesday
    shift: "afternoon",
    machineNumber: "HD-03",
    startTime: "14:00",
    duration: 210,
    isActive: true,
  },
];

const dialysisLabResults: DialysisLabResult[] = [
  {
    id: 1,
    patientId: 101,
    patientName: "Khalid Al Mansoori",
    labDate: weeksAgo(1),
    bun: 45,
    creatinine: 8.2,
    potassium: 4.8,
    phosphorus: 5.2,
    calcium: 9.1,
    hemoglobin: 10.5,
    albumin: 3.8,
    pth: 280,
    ktv: 1.4,
    urr: 72,
    notes: "Kt/V and URR within target range.",
  },
  {
    id: 2,
    patientId: 102,
    patientName: "Maryam Hassan",
    labDate: weeksAgo(1),
    bun: 52,
    creatinine: 9.5,
    potassium: 5.5,
    phosphorus: 6.1,
    calcium: 8.8,
    hemoglobin: 9.8,
    albumin: 3.5,
    pth: 450,
    ktv: 1.2,
    urr: 68,
    notes:
      "Phosphorus elevated. Adjust binders. PTH high - consider vitamin D.",
  },
];

export const dialysisData = {
  patients: dialysisPatients,
  sessions: dialysisSessions,
  schedules: dialysisSchedules,
  labResults: dialysisLabResults,
};

// ============================================================
// ENT MODULE
// ============================================================

const earExams: EarExam[] = [
  {
    id: 1,
    patientId: 201,
    patientName: "Layla Al Nuaimi",
    examDate: daysAgo(1),
    ear: "both",
    externalCanal: "Clear bilaterally, no debris or discharge",
    tympanicMembrane: "Intact bilaterally, good light reflex, no effusion",
    middleEar: "Normal appearance",
    hearingAssessment: "Subjectively normal, formal audiometry recommended",
    diagnosis: "Normal ear examination",
    treatment: "No treatment required",
    performedBy: "Dr. Hassan Al Mazrouei",
  },
  {
    id: 2,
    patientId: 202,
    patientName: "Abdullah Rashid",
    examDate: daysAgo(3),
    ear: "right",
    externalCanal: "Mild erythema, minimal debris",
    tympanicMembrane: "Bulging, erythematous, loss of light reflex",
    middleEar: "Effusion present",
    hearingAssessment: "Conductive hearing loss noted",
    diagnosis: "Acute otitis media - right ear",
    treatment: "Amoxicillin 500mg TID x 10 days, analgesics PRN",
    performedBy: "Dr. Hassan Al Mazrouei",
  },
];

const nasalEndoscopies: NasalEndoscopy[] = [
  {
    id: 1,
    patientId: 203,
    patientName: "Fatima Al Shamsi",
    examDate: daysAgo(5),
    indication: "Chronic nasal obstruction, post-nasal drip",
    septum: "Mild leftward deviation in anterior portion",
    turbinates: "Bilateral inferior turbinate hypertrophy",
    nasopharynx: "Patent, no adenoid hypertrophy",
    findings:
      "Deviated septum with compensatory turbinate hypertrophy. No polyps.",
    diagnosis: "Deviated nasal septum with chronic rhinitis",
    recommendations:
      "Trial of nasal steroids. Consider septoplasty if medical management fails.",
    performedBy: "Dr. Hassan Al Mazrouei",
  },
  {
    id: 2,
    patientId: 204,
    patientName: "Mohammed Al Kaabi",
    examDate: daysAgo(2),
    indication: "Recurrent sinusitis, facial pressure",
    septum: "Midline",
    turbinates: "Moderate bilateral edema",
    nasopharynx: "Clear",
    findings:
      "Mucopurulent discharge from middle meatus bilaterally. Polypoid changes noted.",
    diagnosis: "Chronic rhinosinusitis with nasal polyposis",
    recommendations:
      "CT sinuses recommended. Consider FESS if medical management fails.",
    performedBy: "Dr. Hassan Al Mazrouei",
  },
];

const laryngoscopies: Laryngoscopy[] = [
  {
    id: 1,
    patientId: 205,
    patientName: "Sara Al Dhaheri",
    examDate: daysAgo(7),
    type: "flexible",
    indication: "Persistent hoarseness for 3 months",
    vocalCords:
      "Bilateral vocal fold nodules at junction of anterior and middle third",
    epiglottis: "Normal",
    arytenoids: "Mobile, no edema",
    mobility: "Normal bilateral vocal cord movement",
    findings: "Singer's nodules consistent with vocal abuse",
    diagnosis: "Bilateral vocal fold nodules",
    performedBy: "Dr. Amal Al Suwaidi",
  },
  {
    id: 2,
    patientId: 206,
    patientName: "Rashid Al Ketbi",
    examDate: daysAgo(4),
    type: "flexible",
    indication: "Voice change, smoker",
    vocalCords: "Right vocal cord leukoplakia, irregular surface",
    epiglottis: "Normal",
    arytenoids: "Normal",
    mobility: "Normal movement bilaterally",
    findings: "Suspicious lesion on right vocal cord requiring biopsy",
    diagnosis: "Right vocal cord leukoplakia - rule out malignancy",
    performedBy: "Dr. Amal Al Suwaidi",
  },
];

const tympanometryTests: TympanometryTest[] = [
  {
    id: 1,
    patientId: 202,
    patientName: "Abdullah Rashid",
    testDate: daysAgo(3),
    ear: "right",
    tympanogramType: "B",
    peakPressure: undefined,
    compliance: 0.2,
    earCanalVolume: 1.1,
    interpretation: "Flat tympanogram consistent with middle ear effusion",
    performedBy: "Audiologist Huda",
  },
  {
    id: 2,
    patientId: 202,
    patientName: "Abdullah Rashid",
    testDate: daysAgo(3),
    ear: "left",
    tympanogramType: "A",
    peakPressure: -20,
    compliance: 0.8,
    earCanalVolume: 1.2,
    interpretation: "Normal middle ear function",
    performedBy: "Audiologist Huda",
  },
  {
    id: 3,
    patientId: 201,
    patientName: "Layla Al Nuaimi",
    testDate: daysAgo(1),
    ear: "right",
    tympanogramType: "A",
    peakPressure: 5,
    compliance: 0.7,
    earCanalVolume: 1.0,
    interpretation: "Normal middle ear function",
    performedBy: "Audiologist Huda",
  },
];

export const entData = {
  earExams,
  nasalEndoscopies,
  laryngoscopies,
  tympanometryTests,
};

// ============================================================
// FERTILITY MODULE
// ============================================================

const fertilityAssessments: FertilityAssessment[] = [
  {
    id: 1,
    patientId: 301,
    patientName: "Noura Al Hammadi",
    partnerId: 302,
    partnerName: "Ahmed Al Hammadi",
    assessmentDate: monthsAgo(3),
    infertilityDuration: 24,
    primaryInfertility: true,
    femaleFactors: ["PCOS", "Irregular cycles"],
    maleFactors: [],
    diagnosis: "Primary infertility - PCOS with anovulation",
    recommendedTreatment:
      "Ovulation induction with letrozole, timed intercourse",
    prognosis: "Good prognosis with treatment",
    performedBy: "Dr. Maha Al Ansari",
  },
  {
    id: 2,
    patientId: 303,
    patientName: "Hessa Al Maktoum",
    partnerId: 304,
    partnerName: "Saeed Al Maktoum",
    assessmentDate: monthsAgo(1),
    infertilityDuration: 36,
    primaryInfertility: false,
    femaleFactors: ["Tubal factor", "Age >35"],
    maleFactors: ["Oligospermia"],
    diagnosis: "Secondary infertility - Tubal factor + Male factor",
    recommendedTreatment: "IVF with ICSI recommended",
    prognosis: "Moderate prognosis given combined factors",
    performedBy: "Dr. Maha Al Ansari",
  },
];

const ivfCycles: IVFCycle[] = [
  {
    id: 1,
    patientId: 303,
    patientName: "Hessa Al Maktoum",
    cycleNumber: 1,
    startDate: weeksAgo(3),
    protocol: "Antagonist protocol",
    stimulationDays: 10,
    triggerDate: weeksAgo(2),
    retrievalDate: daysAgo(12),
    transferDate: daysAgo(7),
    status: 5 as IVFCycleStatus, // WaitingResult
    notes: "Good response to stimulation. 2 embryos transferred.",
    managedBy: "Dr. Maha Al Ansari",
  },
  {
    id: 2,
    patientId: 305,
    patientName: "Aisha Al Falasi",
    cycleNumber: 2,
    startDate: weeksAgo(1),
    protocol: "Long agonist protocol",
    stimulationDays: undefined,
    status: 1 as IVFCycleStatus, // Stimulation
    notes: "Day 5 of stimulation. Good follicle development.",
    managedBy: "Dr. Maha Al Ansari",
  },
];

const ivfMonitorings: IVFMonitoring[] = [
  {
    id: 1,
    cycleId: 2,
    patientId: 305,
    monitoringDate: daysAgo(2),
    dayOfStimulation: 3,
    e2Level: 450,
    lhLevel: 2.1,
    progesterone: 0.5,
    endometriumThickness: 6.5,
    leftFollicles: "8, 9, 10, 11, 12mm",
    rightFollicles: "7, 8, 10, 11mm",
    medications: "Gonal-F 225 IU, Cetrotide 0.25mg started",
    nextAppointment: daysFromNow(2),
    performedBy: "Dr. Maha Al Ansari",
  },
  {
    id: 2,
    cycleId: 2,
    patientId: 305,
    monitoringDate: daysAgo(0),
    dayOfStimulation: 5,
    e2Level: 980,
    lhLevel: 3.2,
    progesterone: 0.6,
    endometriumThickness: 8.2,
    leftFollicles: "12, 13, 14, 15, 16mm",
    rightFollicles: "11, 12, 14, 15mm",
    medications: "Continue Gonal-F 225 IU, Cetrotide 0.25mg",
    nextAppointment: daysFromNow(2),
    performedBy: "Dr. Maha Al Ansari",
  },
];

const oocyteRetrievals: OocyteRetrieval[] = [
  {
    id: 1,
    cycleId: 1,
    patientId: 303,
    retrievalDate: daysAgo(12),
    oocytesRetrieved: 12,
    matureOocytes: 9,
    immatureOocytes: 2,
    abnormalOocytes: 1,
    complications: undefined,
    performedBy: "Dr. Maha Al Ansari",
  },
];

const embryoRecords: EmbryoRecord[] = [
  {
    id: 1,
    cycleId: 1,
    patientId: 303,
    embryoNumber: 1,
    fertilizationDate: daysAgo(11),
    day: 5,
    grade: "4AA",
    cellCount: undefined,
    fragmentation: "<5%",
    status: "transferred",
    notes: "Excellent quality blastocyst",
  },
  {
    id: 2,
    cycleId: 1,
    patientId: 303,
    embryoNumber: 2,
    fertilizationDate: daysAgo(11),
    day: 5,
    grade: "4AB",
    cellCount: undefined,
    fragmentation: "5-10%",
    status: "transferred",
    notes: "Good quality blastocyst",
  },
  {
    id: 3,
    cycleId: 1,
    patientId: 303,
    embryoNumber: 3,
    fertilizationDate: daysAgo(11),
    day: 5,
    grade: "3BB",
    cellCount: undefined,
    fragmentation: "10-15%",
    status: "frozen",
    notes: "Vitrified for future use",
  },
];

export const fertilityData = {
  assessments: fertilityAssessments,
  ivfCycles,
  monitoring: ivfMonitorings,
  retrievals: oocyteRetrievals,
  embryos: embryoRecords,
};

// ============================================================
// OB/GYN MODULE
// ============================================================

const pregnancies: Pregnancy[] = [
  {
    id: 1,
    patientId: 401,
    patientName: "Shamma Al Nahyan",
    lmp: weeksAgo(28),
    edd: daysFromNow(84),
    gestationalAge: 28,
    gravida: 2,
    para: 1,
    status: 0 as PregnancyStatus, // Active
    riskFactors: [],
    notes: "Low risk pregnancy. Previous uncomplicated SVD.",
    createdBy: "Dr. Amina Al Blooshi",
    createdAt: weeksAgo(20),
  },
  {
    id: 2,
    patientId: 402,
    patientName: "Mouza Al Qasimi",
    lmp: weeksAgo(34),
    edd: daysFromNow(42),
    gestationalAge: 34,
    gravida: 3,
    para: 2,
    status: 0 as PregnancyStatus, // Active
    riskFactors: ["GDM", "Previous cesarean section"],
    notes:
      "High risk - gestational diabetes controlled with diet. Plan for repeat CS.",
    createdBy: "Dr. Amina Al Blooshi",
    createdAt: weeksAgo(26),
  },
  {
    id: 3,
    patientId: 403,
    patientName: "Latifa Al Suwaidi",
    lmp: weeksAgo(12),
    edd: daysFromNow(196),
    gestationalAge: 12,
    gravida: 1,
    para: 0,
    status: 0 as PregnancyStatus, // Active
    riskFactors: ["Advanced maternal age"],
    notes: "First pregnancy at age 38. NT scan and NIPT completed - low risk.",
    createdBy: "Dr. Amina Al Blooshi",
    createdAt: weeksAgo(4),
  },
];

const prenatalVisits: PrenatalVisit[] = [
  {
    id: 1,
    pregnancyId: 1,
    patientId: 401,
    patientName: "Shamma Al Nahyan",
    visitDate: daysAgo(0),
    gestationalAge: 28,
    weight: 68.5,
    bloodPressure: "118/75",
    fundalHeight: 28,
    fetalHeartRate: 145,
    fetalMovement: true,
    urineProtein: "Negative",
    urineGlucose: "Negative",
    edema: "None",
    complaints: "None",
    notes: "Routine 28-week visit. GCT scheduled today.",
    nextVisitDate: daysFromNow(14),
    performedBy: "Dr. Amina Al Blooshi",
  },
  {
    id: 2,
    pregnancyId: 2,
    patientId: 402,
    patientName: "Mouza Al Qasimi",
    visitDate: daysAgo(3),
    gestationalAge: 34,
    weight: 82.0,
    bloodPressure: "125/80",
    fundalHeight: 33,
    fetalHeartRate: 140,
    fetalMovement: true,
    urineProtein: "Negative",
    urineGlucose: "Trace",
    edema: "Mild pedal edema",
    complaints: "Mild back pain",
    notes: "GDM controlled with diet. Blood sugars within target.",
    nextVisitDate: daysFromNow(7),
    performedBy: "Dr. Amina Al Blooshi",
  },
];

const obstetricUltrasounds: ObstetricUltrasound[] = [
  {
    id: 1,
    pregnancyId: 1,
    patientId: 401,
    patientName: "Shamma Al Nahyan",
    ultrasoundDate: weeksAgo(2),
    gestationalAge: 26,
    fetalHeartRate: 148,
    bpd: 68,
    hc: 245,
    ac: 220,
    fl: 50,
    estimatedWeight: 920,
    amnioticFluid: "Normal AFI 14cm",
    placentaLocation: "Anterior, clear of os",
    findings: "Normal fetal anatomy. Growth on 50th centile.",
    performedBy: "Dr. Amina Al Blooshi",
  },
  {
    id: 2,
    pregnancyId: 2,
    patientId: 402,
    patientName: "Mouza Al Qasimi",
    ultrasoundDate: weeksAgo(1),
    gestationalAge: 33,
    fetalHeartRate: 142,
    bpd: 85,
    hc: 305,
    ac: 310,
    fl: 65,
    estimatedWeight: 2350,
    amnioticFluid: "Polyhydramnios - AFI 26cm",
    placentaLocation: "Posterior",
    findings: "EFW on 75th centile. Mild polyhydramnios - monitor.",
    performedBy: "Dr. Amina Al Blooshi",
  },
];

const papSmears: PapSmear[] = [
  {
    id: 1,
    patientId: 404,
    patientName: "Mariam Al Dhaheri",
    collectionDate: monthsAgo(1),
    indication: "Routine screening",
    result: "NILM",
    bethesdaCategory: "Negative for intraepithelial lesion",
    hpvStatus: "Negative",
    recommendations: "Routine screening in 3 years",
    nextDueDate: daysFromNow(1095),
    performedBy: "Dr. Amina Al Blooshi",
    reportedDate: weeksAgo(3),
  },
  {
    id: 2,
    patientId: 405,
    patientName: "Salama Al Rumaithi",
    collectionDate: monthsAgo(2),
    indication: "Routine screening",
    result: "LSIL",
    bethesdaCategory: "Low-grade squamous intraepithelial lesion",
    hpvStatus: "HPV 16/18 Negative, Other HR-HPV Positive",
    recommendations: "Colposcopy recommended",
    nextDueDate: daysFromNow(30),
    performedBy: "Dr. Amina Al Blooshi",
    reportedDate: monthsAgo(2) + 7,
  },
];

export const obgynData = {
  pregnancies,
  prenatalVisits,
  ultrasounds: obstetricUltrasounds,
  papSmears,
};

// ============================================================
// CLINICAL VISIT MODULE
// ============================================================

const clinicalVisits: ClinicalVisit[] = [
  {
    id: 1,
    visitNumber: "V-2024-001234",
    patientId: 501,
    patientName: "Hamad Al Zaabi",
    doctorId: 1,
    doctorName: "Dr. Khalid Mohammed",
    visitDate: daysAgo(0),
    visitType: 0 as VisitType, // Consultation
    chiefComplaint: "Persistent headache for 5 days, worse in the morning",
    diagnosis: "Tension-type headache",
    treatmentPlan:
      "Paracetamol 500mg PRN, stress management, adequate hydration",
    notes: "BP normal. No neurological deficits. Red flags negative.",
    vitalSigns: {
      bloodPressure: "122/78",
      heartRate: 72,
      temperature: 36.8,
      weight: 78,
      height: 175,
      oxygenSaturation: 98,
    },
    status: 2 as ClinicalVisitStatus, // Completed
    followUpDate: daysFromNow(14),
    createdAt: daysAgo(0),
  },
  {
    id: 2,
    visitNumber: "V-2024-001235",
    patientId: 502,
    patientName: "Nouf Al Muhairi",
    doctorId: 2,
    doctorName: "Dr. Sara Al Ameri",
    visitDate: daysAgo(0),
    visitType: 1 as VisitType, // FollowUp
    chiefComplaint: "Follow-up for diabetes management",
    diagnosis: "Type 2 Diabetes Mellitus - controlled",
    treatmentPlan: "Continue Metformin 1000mg BD. Diet and exercise.",
    notes: "HbA1c improved to 6.8%. Continue current regimen.",
    vitalSigns: {
      bloodPressure: "128/82",
      heartRate: 76,
      temperature: 36.6,
      weight: 85,
      height: 165,
      oxygenSaturation: 97,
    },
    status: 2 as ClinicalVisitStatus,
    followUpDate: daysFromNow(90),
    createdAt: daysAgo(0),
  },
  {
    id: 3,
    visitNumber: "V-2024-001236",
    patientId: 503,
    patientName: "Sultan Al Romaithi",
    doctorId: 1,
    doctorName: "Dr. Khalid Mohammed",
    visitDate: daysAgo(0),
    visitType: 2 as VisitType, // Emergency
    chiefComplaint: "Acute chest pain, started 2 hours ago",
    diagnosis: undefined,
    treatmentPlan: "ECG stat, cardiac enzymes, chest X-ray",
    notes: "Patient appears anxious. ECG pending. Monitoring vitals.",
    vitalSigns: {
      bloodPressure: "145/92",
      heartRate: 98,
      temperature: 37.0,
      weight: 92,
      oxygenSaturation: 96,
    },
    status: 1 as ClinicalVisitStatus, // InProgress
    createdAt: daysAgo(0),
  },
  {
    id: 4,
    visitNumber: "V-2024-001230",
    patientId: 504,
    patientName: "Reem Al Falahi",
    doctorId: 3,
    doctorName: "Dr. Ahmed Al Hashemi",
    visitDate: daysAgo(1),
    visitType: 4 as VisitType, // Checkup
    chiefComplaint: "Annual health checkup",
    diagnosis: "Healthy - no significant findings",
    treatmentPlan: "Continue healthy lifestyle. Annual labs completed.",
    notes: "All vitals normal. Labs pending.",
    vitalSigns: {
      bloodPressure: "115/72",
      heartRate: 68,
      temperature: 36.5,
      weight: 62,
      height: 160,
      oxygenSaturation: 99,
    },
    status: 2 as ClinicalVisitStatus,
    createdAt: daysAgo(1),
  },
];

export const clinicalVisitData = {
  visits: clinicalVisits,
};

// ============================================================
// ANALYTICS MODULE
// ============================================================

const analyticsDashboard: AnalyticsDashboard = {
  totalPatients: 2847,
  totalAppointments: 12453,
  totalRevenue: 4250000,
  averageWaitTime: 12.5,
  patientSatisfaction: 4.6,
  appointmentsByDay: [
    { date: daysAgo(6), count: 45, label: "Sunday" },
    { date: daysAgo(5), count: 52, label: "Monday" },
    { date: daysAgo(4), count: 48, label: "Tuesday" },
    { date: daysAgo(3), count: 55, label: "Wednesday" },
    { date: daysAgo(2), count: 42, label: "Thursday" },
    { date: daysAgo(1), count: 28, label: "Friday" },
    { date: daysAgo(0), count: 38, label: "Saturday" },
  ],
  revenueByMonth: [
    { month: "2024-07", revenue: 380000, label: "July" },
    { month: "2024-08", revenue: 410000, label: "August" },
    { month: "2024-09", revenue: 395000, label: "September" },
    { month: "2024-10", revenue: 425000, label: "October" },
    { month: "2024-11", revenue: 445000, label: "November" },
    { month: "2024-12", revenue: 320000, label: "December" },
  ],
  topServices: [
    { serviceName: "General Consultation", count: 3250, revenue: 487500 },
    { serviceName: "Dental Services", count: 1840, revenue: 920000 },
    { serviceName: "Laboratory Tests", count: 4120, revenue: 618000 },
    { serviceName: "Radiology", count: 980, revenue: 490000 },
    { serviceName: "Specialist Consultation", count: 2100, revenue: 630000 },
  ],
  departmentStats: [
    {
      departmentName: "General Medicine",
      patientCount: 850,
      revenue: 425000,
      appointmentCount: 1200,
    },
    {
      departmentName: "Pediatrics",
      patientCount: 420,
      revenue: 210000,
      appointmentCount: 580,
    },
    {
      departmentName: "Gynecology",
      patientCount: 380,
      revenue: 285000,
      appointmentCount: 450,
    },
    {
      departmentName: "Orthopedics",
      patientCount: 290,
      revenue: 435000,
      appointmentCount: 380,
    },
    {
      departmentName: "Cardiology",
      patientCount: 310,
      revenue: 465000,
      appointmentCount: 420,
    },
  ],
};

const reports: Report[] = [
  {
    id: 1,
    name: "Monthly Revenue Report - November 2024",
    type: 2 as ReportType, // Monthly
    parameters: {
      startDate: "2024-11-01",
      endDate: "2024-11-30",
      includeCharts: true,
      includeDetails: true,
    },
    generatedAt: daysAgo(5),
    generatedBy: "Admin User",
    data: {},
    status: 2 as ReportStatus, // Completed
    fileUrl: "/reports/monthly-revenue-nov-2024.pdf",
    description:
      "Comprehensive monthly revenue analysis with department breakdown",
  },
  {
    id: 2,
    name: "Patient Demographics Report Q4 2024",
    type: 3 as ReportType, // Quarterly
    parameters: {
      startDate: "2024-10-01",
      endDate: "2024-12-31",
      includeCharts: true,
      includeDetails: true,
    },
    generatedAt: daysAgo(2),
    generatedBy: "Admin User",
    data: {},
    status: 2 as ReportStatus,
    fileUrl: "/reports/demographics-q4-2024.pdf",
    description: "Patient age, gender, and nationality distribution analysis",
  },
  {
    id: 3,
    name: "Daily Appointment Summary",
    type: 0 as ReportType, // Daily
    parameters: {
      startDate: daysAgo(1),
      endDate: daysAgo(1),
      includeCharts: false,
      includeDetails: true,
    },
    generatedAt: daysAgo(0),
    generatedBy: "System",
    data: {},
    status: 1 as ReportStatus, // Generating
    description: "Auto-generated daily appointment summary",
  },
];

export const analyticsData = {
  dashboard: analyticsDashboard,
  reports,
};

// ============================================================
// WORKFLOW MODULE
// ============================================================

const workflowDefinitions: WorkflowDefinition[] = [
  {
    id: 1,
    name: "Patient Admission Workflow",
    description: "Standard workflow for admitting new patients",
    steps: [
      {
        id: "step-1",
        name: "Registration",
        type: 0 as WorkflowStepType, // Manual
        assigneeRole: "Receptionist",
        actions: [
          "Collect patient details",
          "Verify insurance",
          "Create patient record",
        ],
        nextSteps: ["step-2"],
        order: 1,
        description: "Initial patient registration at front desk",
      },
      {
        id: "step-2",
        name: "Triage Assessment",
        type: 0 as WorkflowStepType,
        assigneeRole: "Nurse",
        actions: ["Record vitals", "Assess urgency", "Assign to department"],
        nextSteps: ["step-3"],
        order: 2,
        description: "Nursing assessment and vital signs",
      },
      {
        id: "step-3",
        name: "Doctor Consultation",
        type: 0 as WorkflowStepType,
        assigneeRole: "Doctor",
        actions: [
          "Examine patient",
          "Document findings",
          "Create treatment plan",
        ],
        nextSteps: ["step-4"],
        order: 3,
        description: "Medical consultation and diagnosis",
      },
      {
        id: "step-4",
        name: "Billing",
        type: 1 as WorkflowStepType, // Automatic
        actions: ["Generate invoice", "Process insurance claim"],
        nextSteps: [],
        order: 4,
        description: "Automatic billing generation",
      },
    ],
    triggers: ["New patient registration"],
    isActive: true,
    version: 2,
    category: "Patient Care",
    createdAt: monthsAgo(6),
    updatedAt: monthsAgo(1),
    createdBy: "System Admin",
    updatedBy: "System Admin",
  },
  {
    id: 2,
    name: "Lab Order Approval",
    description: "Workflow for approving laboratory test orders",
    steps: [
      {
        id: "step-1",
        name: "Order Creation",
        type: 0 as WorkflowStepType,
        assigneeRole: "Doctor",
        actions: ["Select tests", "Add clinical notes"],
        nextSteps: ["step-2"],
        order: 1,
      },
      {
        id: "step-2",
        name: "Insurance Pre-auth",
        type: 2 as WorkflowStepType, // Approval
        assigneeRole: "Insurance Coordinator",
        actions: ["Verify coverage", "Submit pre-authorization"],
        nextSteps: ["step-3"],
        order: 2,
      },
      {
        id: "step-3",
        name: "Sample Collection",
        type: 0 as WorkflowStepType,
        assigneeRole: "Lab Technician",
        actions: ["Collect sample", "Label specimen", "Process order"],
        nextSteps: ["step-4"],
        order: 3,
      },
      {
        id: "step-4",
        name: "Result Notification",
        type: 3 as WorkflowStepType, // Notification
        actions: ["Notify ordering physician", "Update patient portal"],
        nextSteps: [],
        order: 4,
      },
    ],
    triggers: ["Lab order created"],
    isActive: true,
    version: 1,
    category: "Laboratory",
    createdAt: monthsAgo(3),
    updatedAt: monthsAgo(3),
    createdBy: "System Admin",
  },
];

const workflowInstances: WorkflowInstance[] = [
  {
    id: 1,
    definitionId: 1,
    definitionName: "Patient Admission Workflow",
    entityType: "Patient",
    entityId: 501,
    currentStep: "step-3",
    currentStepName: "Doctor Consultation",
    status: 1 as WorkflowStatus, // InProgress
    startedAt: daysAgo(0),
    startedBy: "User001",
    startedByName: "Fatima Receptionist",
    assignedTo: "Doc001",
    assignedToName: "Dr. Khalid Mohammed",
    history: [
      {
        id: 1,
        instanceId: 1,
        stepId: "step-1",
        stepName: "Registration",
        action: "Completed",
        performedBy: "User001",
        performedByName: "Fatima Receptionist",
        performedAt: daysAgo(0),
        notes: "Patient registered successfully",
        newStatus: 1 as WorkflowStatus,
      },
      {
        id: 2,
        instanceId: 1,
        stepId: "step-2",
        stepName: "Triage Assessment",
        action: "Completed",
        performedBy: "Nurse001",
        performedByName: "Aisha Nurse",
        performedAt: daysAgo(0),
        notes: "Vitals recorded. Non-urgent case.",
        newStatus: 1 as WorkflowStatus,
      },
    ],
  },
  {
    id: 2,
    definitionId: 2,
    definitionName: "Lab Order Approval",
    entityType: "LabOrder",
    entityId: 1234,
    currentStep: "step-2",
    currentStepName: "Insurance Pre-auth",
    status: 0 as WorkflowStatus, // Pending
    startedAt: daysAgo(1),
    startedBy: "Doc002",
    startedByName: "Dr. Sara Al Ameri",
    assignedTo: "Ins001",
    assignedToName: "Ahmed Insurance Coord",
    history: [
      {
        id: 3,
        instanceId: 2,
        stepId: "step-1",
        stepName: "Order Creation",
        action: "Completed",
        performedBy: "Doc002",
        performedByName: "Dr. Sara Al Ameri",
        performedAt: daysAgo(1),
        notes: "Comprehensive metabolic panel ordered",
        newStatus: 0 as WorkflowStatus,
      },
    ],
  },
];

export const workflowData = {
  definitions: workflowDefinitions,
  instances: workflowInstances,
};

// ============================================================
// PORTAL MODULE
// ============================================================

const portalUsers: PortalUser[] = [
  {
    id: 1,
    email: "hamad.zaabi@email.com",
    patientId: 501,
    isVerified: true,
    isProfileComplete: true,
    registeredAt: monthsAgo(6),
    lastLoginAt: daysAgo(0),
    patient: {
      id: 501,
      fullNameEn: "Hamad Al Zaabi",
      fullNameAr: "حمد الزعابي",
      phoneNumber: "+971501234567",
      email: "hamad.zaabi@email.com",
      dateOfBirth: "1985-03-15",
      gender: "Male",
    },
  },
  {
    id: 2,
    email: "nouf.muhairi@email.com",
    patientId: 502,
    isVerified: true,
    isProfileComplete: true,
    registeredAt: monthsAgo(3),
    lastLoginAt: daysAgo(2),
    patient: {
      id: 502,
      fullNameEn: "Nouf Al Muhairi",
      fullNameAr: "نوف المهيري",
      phoneNumber: "+971502345678",
      email: "nouf.muhairi@email.com",
      dateOfBirth: "1990-07-22",
      gender: "Female",
    },
  },
  {
    id: 3,
    email: "reem.falahi@email.com",
    patientId: 504,
    isVerified: false,
    isProfileComplete: false,
    registeredAt: daysAgo(2),
    patient: {
      id: 504,
      fullNameEn: "Reem Al Falahi",
      fullNameAr: "ريم الفلاحي",
      phoneNumber: "+971503456789",
      email: "reem.falahi@email.com",
      dateOfBirth: "1988-11-08",
      gender: "Female",
    },
  },
];

const portalDocuments: PortalDocument[] = [
  {
    id: 1,
    patientId: 501,
    name: "Lab Results - Complete Blood Count",
    type: 1 as PortalDocumentType, // LabResult
    uploadedAt: weeksAgo(2),
    size: 245000,
    url: "/documents/patient-501/cbc-results.pdf",
    description: "CBC test results from routine checkup",
  },
  {
    id: 2,
    patientId: 501,
    name: "Prescription - November 2024",
    type: 2 as PortalDocumentType, // Prescription
    uploadedAt: monthsAgo(1),
    size: 125000,
    url: "/documents/patient-501/prescription-nov.pdf",
    description: "Monthly prescription for blood pressure medication",
  },
  {
    id: 3,
    patientId: 502,
    name: "HbA1c Report",
    type: 1 as PortalDocumentType,
    uploadedAt: weeksAgo(1),
    size: 180000,
    url: "/documents/patient-502/hba1c-report.pdf",
    description: "Quarterly diabetes monitoring",
  },
  {
    id: 4,
    patientId: 502,
    name: "Insurance Card - Daman",
    type: 4 as PortalDocumentType, // Insurance
    uploadedAt: monthsAgo(2),
    size: 520000,
    url: "/documents/patient-502/insurance-card.pdf",
    description: "Health insurance card scan",
  },
  {
    id: 5,
    patientId: 501,
    name: "Chest X-Ray Report",
    type: 3 as PortalDocumentType, // ImagingReport
    uploadedAt: monthsAgo(3),
    size: 1250000,
    url: "/documents/patient-501/chest-xray.pdf",
    description: "Annual chest X-ray - Normal findings",
  },
];

export const portalData = {
  users: portalUsers,
  documents: portalDocuments,
};

// ============================================================
// COMBINED EXPORT
// ============================================================

export const allMedicalServicesData = {
  dialysis: dialysisData,
  ent: entData,
  fertility: fertilityData,
  obgyn: obgynData,
  clinicalVisit: clinicalVisitData,
  analytics: analyticsData,
  workflow: workflowData,
  portal: portalData,
};
