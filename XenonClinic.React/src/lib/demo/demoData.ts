/**
 * Demo Data Definitions
 * Contains seed data for different clinic types for sales demonstrations
 */

import { EncounterType, EncounterStatus, EarSide, HearingLossGrade, HearingAidStyle, HearingAidStatus } from '../../types/audiology';

// ============================================
// TYPES
// ============================================

export type ClinicType = 'audiology' | 'dental' | 'general' | 'ophthalmology';

export interface DemoPatient {
  id: number;
  emiratesId: string;
  fullNameEn: string;
  fullNameAr?: string;
  dateOfBirth: string;
  gender: 'M' | 'F';
  phoneNumber: string;
  email: string;
  address?: string;
  nationality?: string;
  insuranceProvider?: string;
  insurancePolicyNumber?: string;
}

export interface DemoAppointment {
  id: number;
  patientId: number;
  appointmentDate: string;
  appointmentTime: string;
  duration: number;
  type: string;
  status: 'Scheduled' | 'Confirmed' | 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow';
  notes?: string;
  providerId?: number;
  providerName?: string;
}

export interface DemoEncounter {
  id: number;
  patientId: number;
  encounterDate: string;
  encounterType: EncounterType;
  status: EncounterStatus;
  chiefComplaint: string;
  providerName: string;
  notes?: string;
}

export interface DemoAudiogram {
  id: number;
  patientId: number;
  testDate: string;
  rightEar: Record<number, number>;
  leftEar: Record<number, number>;
  rightEarGrade: HearingLossGrade;
  leftEarGrade: HearingLossGrade;
  notes?: string;
}

export interface DemoHearingAid {
  id: number;
  patientId: number;
  serialNumber: string;
  manufacturer: string;
  model: string;
  style: HearingAidStyle;
  ear: EarSide;
  status: HearingAidStatus;
  purchaseDate: string;
  warrantyExpiry: string;
  price: number;
}

export interface DemoEmployee {
  id: number;
  employeeId: string;
  fullName: string;
  email: string;
  department: string;
  position: string;
  hireDate: string;
  salary: number;
  status: 'Active' | 'OnLeave' | 'Terminated';
}

export interface DemoInventoryItem {
  id: number;
  sku: string;
  name: string;
  category: string;
  quantity: number;
  minQuantity: number;
  unitPrice: number;
  supplier: string;
  lastRestocked: string;
}

export interface DemoDataset {
  clinicType: ClinicType;
  clinicName: string;
  description: string;
  patients: DemoPatient[];
  appointments: DemoAppointment[];
  encounters?: DemoEncounter[];
  audiograms?: DemoAudiogram[];
  hearingAids?: DemoHearingAid[];
  employees: DemoEmployee[];
  inventory: DemoInventoryItem[];
}

// ============================================
// HELPER FUNCTIONS
// ============================================

const generateEmiratesId = (year: number, index: number): string => {
  const serial = String(1000000 + index).slice(1);
  const checkDigit = (index % 10);
  return `784-${year}-${serial}-${checkDigit}`;
};

const getDateString = (daysFromNow: number): string => {
  const date = new Date();
  date.setDate(date.getDate() + daysFromNow);
  return date.toISOString().split('T')[0];
};

const getTimeString = (hour: number, minute: number = 0): string => {
  return `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`;
};

// ============================================
// AUDIOLOGY CLINIC DEMO DATA
// ============================================

export const audiologyDemoData: DemoDataset = {
  clinicType: 'audiology',
  clinicName: 'XenonClinic Audiology Center',
  description: 'Full-featured audiology clinic with hearing tests, hearing aids, and patient management',

  patients: [
    {
      id: 1,
      emiratesId: generateEmiratesId(1965, 1),
      fullNameEn: 'Ahmed Hassan Al Maktoum',
      fullNameAr: 'أحمد حسن المكتوم',
      dateOfBirth: '1965-03-15',
      gender: 'M',
      phoneNumber: '+971501234001',
      email: 'ahmed.hassan@email.com',
      nationality: 'UAE',
      insuranceProvider: 'Daman',
      insurancePolicyNumber: 'DAM-2024-001234',
    },
    {
      id: 2,
      emiratesId: generateEmiratesId(1978, 2),
      fullNameEn: 'Fatima Khalid Al Nahyan',
      fullNameAr: 'فاطمة خالد النهيان',
      dateOfBirth: '1978-07-22',
      gender: 'F',
      phoneNumber: '+971501234002',
      email: 'fatima.khalid@email.com',
      nationality: 'UAE',
      insuranceProvider: 'Oman Insurance',
      insurancePolicyNumber: 'OIC-2024-005678',
    },
    {
      id: 3,
      emiratesId: generateEmiratesId(1952, 3),
      fullNameEn: 'Mohammed Rashid Al Falasi',
      fullNameAr: 'محمد راشد الفلاسي',
      dateOfBirth: '1952-11-08',
      gender: 'M',
      phoneNumber: '+971501234003',
      email: 'mohammed.rashid@email.com',
      nationality: 'UAE',
      insuranceProvider: 'AXA Gulf',
      insurancePolicyNumber: 'AXA-2024-009012',
    },
    {
      id: 4,
      emiratesId: generateEmiratesId(1988, 4),
      fullNameEn: 'Sara Omar Al Ketbi',
      fullNameAr: 'سارة عمر الكتبي',
      dateOfBirth: '1988-04-30',
      gender: 'F',
      phoneNumber: '+971501234004',
      email: 'sara.omar@email.com',
      nationality: 'UAE',
      insuranceProvider: 'ADNIC',
      insurancePolicyNumber: 'ADN-2024-003456',
    },
    {
      id: 5,
      emiratesId: generateEmiratesId(1970, 5),
      fullNameEn: 'Khalid Ibrahim Al Suwaidi',
      fullNameAr: 'خالد إبراهيم السويدي',
      dateOfBirth: '1970-09-12',
      gender: 'M',
      phoneNumber: '+971501234005',
      email: 'khalid.ibrahim@email.com',
      nationality: 'UAE',
      insuranceProvider: 'Daman',
      insurancePolicyNumber: 'DAM-2024-007890',
    },
    {
      id: 6,
      emiratesId: generateEmiratesId(1995, 6),
      fullNameEn: 'Mariam Abdullah Al Mansouri',
      fullNameAr: 'مريم عبدالله المنصوري',
      dateOfBirth: '1995-01-25',
      gender: 'F',
      phoneNumber: '+971501234006',
      email: 'mariam.abdullah@email.com',
      nationality: 'UAE',
      insuranceProvider: 'MetLife',
      insurancePolicyNumber: 'MET-2024-002345',
    },
    {
      id: 7,
      emiratesId: generateEmiratesId(1960, 7),
      fullNameEn: 'Hassan Ali Al Shamsi',
      fullNameAr: 'حسن علي الشامسي',
      dateOfBirth: '1960-06-18',
      gender: 'M',
      phoneNumber: '+971501234007',
      email: 'hassan.ali@email.com',
      nationality: 'UAE',
      insuranceProvider: 'Oman Insurance',
      insurancePolicyNumber: 'OIC-2024-006789',
    },
    {
      id: 8,
      emiratesId: generateEmiratesId(1982, 8),
      fullNameEn: 'Layla Youssef Al Zaabi',
      fullNameAr: 'ليلى يوسف الزعابي',
      dateOfBirth: '1982-12-03',
      gender: 'F',
      phoneNumber: '+971501234008',
      email: 'layla.youssef@email.com',
      nationality: 'UAE',
      insuranceProvider: 'AXA Gulf',
      insurancePolicyNumber: 'AXA-2024-004567',
    },
  ],

  appointments: [
    // Today's appointments
    {
      id: 1,
      patientId: 1,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(9, 0),
      duration: 60,
      type: 'Hearing Test',
      status: 'CheckedIn',
      providerName: 'Dr. Sarah Ahmed',
      notes: 'Annual hearing evaluation',
    },
    {
      id: 2,
      patientId: 2,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(10, 30),
      duration: 45,
      type: 'Hearing Aid Adjustment',
      status: 'Scheduled',
      providerName: 'Dr. Sarah Ahmed',
      notes: 'Follow-up adjustment for new hearing aids',
    },
    {
      id: 3,
      patientId: 3,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(14, 0),
      duration: 90,
      type: 'Hearing Aid Fitting',
      status: 'Scheduled',
      providerName: 'Dr. Mohammed Ali',
      notes: 'Bilateral fitting - Phonak Audeo P90',
    },
    // Tomorrow's appointments
    {
      id: 4,
      patientId: 4,
      appointmentDate: getDateString(1),
      appointmentTime: getTimeString(9, 30),
      duration: 60,
      type: 'Initial Consultation',
      status: 'Confirmed',
      providerName: 'Dr. Sarah Ahmed',
      notes: 'New patient - complaining of hearing difficulties',
    },
    {
      id: 5,
      patientId: 5,
      appointmentDate: getDateString(1),
      appointmentTime: getTimeString(11, 0),
      duration: 45,
      type: 'Tinnitus Evaluation',
      status: 'Confirmed',
      providerName: 'Dr. Mohammed Ali',
    },
    // This week
    {
      id: 6,
      patientId: 6,
      appointmentDate: getDateString(3),
      appointmentTime: getTimeString(10, 0),
      duration: 60,
      type: 'Hearing Test',
      status: 'Scheduled',
      providerName: 'Dr. Sarah Ahmed',
    },
    {
      id: 7,
      patientId: 7,
      appointmentDate: getDateString(4),
      appointmentTime: getTimeString(14, 30),
      duration: 30,
      type: 'Follow-Up',
      status: 'Scheduled',
      providerName: 'Dr. Mohammed Ali',
      notes: 'Post-fitting check at 2 weeks',
    },
    // Past appointments (completed)
    {
      id: 8,
      patientId: 1,
      appointmentDate: getDateString(-7),
      appointmentTime: getTimeString(9, 0),
      duration: 60,
      type: 'Hearing Test',
      status: 'Completed',
      providerName: 'Dr. Sarah Ahmed',
    },
    {
      id: 9,
      patientId: 3,
      appointmentDate: getDateString(-14),
      appointmentTime: getTimeString(10, 0),
      duration: 60,
      type: 'Initial Consultation',
      status: 'Completed',
      providerName: 'Dr. Mohammed Ali',
    },
  ],

  encounters: [
    {
      id: 1,
      patientId: 1,
      encounterDate: getDateString(-7),
      encounterType: EncounterType.HearingTest,
      status: EncounterStatus.Completed,
      chiefComplaint: 'Difficulty hearing in noisy environments',
      providerName: 'Dr. Sarah Ahmed',
      notes: 'Patient reports progressive hearing loss over past 2 years. Recommended bilateral hearing aids.',
    },
    {
      id: 2,
      patientId: 2,
      encounterDate: getDateString(-30),
      encounterType: EncounterType.HearingAidFitting,
      status: EncounterStatus.Completed,
      chiefComplaint: 'New hearing aid fitting',
      providerName: 'Dr. Sarah Ahmed',
      notes: 'Fitted with Oticon More 1 miniRITE. Patient very satisfied with initial fitting.',
    },
    {
      id: 3,
      patientId: 3,
      encounterDate: getDateString(-14),
      encounterType: EncounterType.InitialConsultation,
      status: EncounterStatus.Completed,
      chiefComplaint: 'Family reports patient not hearing well',
      providerName: 'Dr. Mohammed Ali',
      notes: 'Severe bilateral sensorineural hearing loss. Recommend bilateral high-power hearing aids.',
    },
    {
      id: 4,
      patientId: 5,
      encounterDate: getDateString(-21),
      encounterType: EncounterType.TinnitusEvaluation,
      status: EncounterStatus.Completed,
      chiefComplaint: 'Persistent ringing in both ears',
      providerName: 'Dr. Mohammed Ali',
      notes: 'Bilateral tinnitus, moderate severity. Recommended sound therapy and counseling.',
    },
    {
      id: 5,
      patientId: 7,
      encounterDate: getDateString(-14),
      encounterType: EncounterType.HearingAidFitting,
      status: EncounterStatus.Completed,
      chiefComplaint: 'Hearing aid fitting',
      providerName: 'Dr. Mohammed Ali',
      notes: 'Bilateral Phonak Audeo Paradise P90-R fitted. Patient adapting well.',
    },
  ],

  audiograms: [
    {
      id: 1,
      patientId: 1,
      testDate: getDateString(-7),
      rightEar: { 250: 35, 500: 40, 1000: 45, 2000: 55, 4000: 65, 8000: 70 },
      leftEar: { 250: 40, 500: 45, 1000: 50, 2000: 60, 4000: 70, 8000: 75 },
      rightEarGrade: HearingLossGrade.Moderate,
      leftEarGrade: HearingLossGrade.ModeratelySevere,
      notes: 'Bilateral sloping sensorineural hearing loss',
    },
    {
      id: 2,
      patientId: 2,
      testDate: getDateString(-45),
      rightEar: { 250: 25, 500: 30, 1000: 35, 2000: 40, 4000: 50, 8000: 55 },
      leftEar: { 250: 30, 500: 35, 1000: 40, 2000: 45, 4000: 55, 8000: 60 },
      rightEarGrade: HearingLossGrade.Mild,
      leftEarGrade: HearingLossGrade.Moderate,
      notes: 'Mild to moderate bilateral hearing loss',
    },
    {
      id: 3,
      patientId: 3,
      testDate: getDateString(-14),
      rightEar: { 250: 55, 500: 60, 1000: 70, 2000: 80, 4000: 90, 8000: 95 },
      leftEar: { 250: 60, 500: 65, 1000: 75, 2000: 85, 4000: 95, 8000: 100 },
      rightEarGrade: HearingLossGrade.Severe,
      leftEarGrade: HearingLossGrade.Severe,
      notes: 'Severe bilateral sensorineural hearing loss',
    },
    {
      id: 4,
      patientId: 5,
      testDate: getDateString(-21),
      rightEar: { 250: 20, 500: 25, 1000: 30, 2000: 35, 4000: 45, 8000: 50 },
      leftEar: { 250: 25, 500: 30, 1000: 35, 2000: 40, 4000: 50, 8000: 55 },
      rightEarGrade: HearingLossGrade.Mild,
      leftEarGrade: HearingLossGrade.Mild,
      notes: 'Mild bilateral hearing loss with tinnitus',
    },
    {
      id: 5,
      patientId: 7,
      testDate: getDateString(-30),
      rightEar: { 250: 45, 500: 50, 1000: 55, 2000: 65, 4000: 75, 8000: 80 },
      leftEar: { 250: 50, 500: 55, 1000: 60, 2000: 70, 4000: 80, 8000: 85 },
      rightEarGrade: HearingLossGrade.Moderate,
      leftEarGrade: HearingLossGrade.ModeratelySevere,
      notes: 'Moderate to moderately severe bilateral hearing loss',
    },
  ],

  hearingAids: [
    {
      id: 1,
      patientId: 2,
      serialNumber: 'OTI-2024-R001234',
      manufacturer: 'Oticon',
      model: 'More 1 miniRITE R',
      style: HearingAidStyle.RIC,
      ear: EarSide.Right,
      status: HearingAidStatus.Active,
      purchaseDate: getDateString(-30),
      warrantyExpiry: getDateString(730), // 2 years
      price: 12500,
    },
    {
      id: 2,
      patientId: 2,
      serialNumber: 'OTI-2024-L001235',
      manufacturer: 'Oticon',
      model: 'More 1 miniRITE R',
      style: HearingAidStyle.RIC,
      ear: EarSide.Left,
      status: HearingAidStatus.Active,
      purchaseDate: getDateString(-30),
      warrantyExpiry: getDateString(730),
      price: 12500,
    },
    {
      id: 3,
      patientId: 7,
      serialNumber: 'PHO-2024-R005678',
      manufacturer: 'Phonak',
      model: 'Audeo Paradise P90-R',
      style: HearingAidStyle.RIC,
      ear: EarSide.Right,
      status: HearingAidStatus.Active,
      purchaseDate: getDateString(-14),
      warrantyExpiry: getDateString(716),
      price: 14000,
    },
    {
      id: 4,
      patientId: 7,
      serialNumber: 'PHO-2024-L005679',
      manufacturer: 'Phonak',
      model: 'Audeo Paradise P90-R',
      style: HearingAidStyle.RIC,
      ear: EarSide.Left,
      status: HearingAidStatus.Active,
      purchaseDate: getDateString(-14),
      warrantyExpiry: getDateString(716),
      price: 14000,
    },
    // Older hearing aid with warranty expiring soon
    {
      id: 5,
      patientId: 1,
      serialNumber: 'SIG-2022-R009876',
      manufacturer: 'Signia',
      model: 'Pure Charge&Go AX',
      style: HearingAidStyle.RIC,
      ear: EarSide.Right,
      status: HearingAidStatus.Active,
      purchaseDate: getDateString(-700),
      warrantyExpiry: getDateString(30), // Expiring soon!
      price: 11000,
    },
  ],

  employees: [
    {
      id: 1,
      employeeId: 'EMP001',
      fullName: 'Dr. Sarah Ahmed',
      email: 'sarah.ahmed@xenonclinic.com',
      department: 'Audiology',
      position: 'Senior Audiologist',
      hireDate: '2020-03-15',
      salary: 35000,
      status: 'Active',
    },
    {
      id: 2,
      employeeId: 'EMP002',
      fullName: 'Dr. Mohammed Ali',
      email: 'mohammed.ali@xenonclinic.com',
      department: 'Audiology',
      position: 'Audiologist',
      hireDate: '2021-06-01',
      salary: 28000,
      status: 'Active',
    },
    {
      id: 3,
      employeeId: 'EMP003',
      fullName: 'Noura Hassan',
      email: 'noura.hassan@xenonclinic.com',
      department: 'Reception',
      position: 'Senior Receptionist',
      hireDate: '2019-01-10',
      salary: 12000,
      status: 'Active',
    },
    {
      id: 4,
      employeeId: 'EMP004',
      fullName: 'Reem Al Falasi',
      email: 'reem.alfalasi@xenonclinic.com',
      department: 'Audiology',
      position: 'Audiology Assistant',
      hireDate: '2022-09-15',
      salary: 15000,
      status: 'Active',
    },
    {
      id: 5,
      employeeId: 'EMP005',
      fullName: 'Omar Khalid',
      email: 'omar.khalid@xenonclinic.com',
      department: 'Finance',
      position: 'Accountant',
      hireDate: '2020-11-01',
      salary: 18000,
      status: 'Active',
    },
  ],

  inventory: [
    // Hearing Aids
    {
      id: 1,
      sku: 'HA-OTI-MORE1',
      name: 'Oticon More 1 miniRITE R',
      category: 'Hearing Aids',
      quantity: 8,
      minQuantity: 4,
      unitPrice: 12500,
      supplier: 'Oticon Middle East',
      lastRestocked: getDateString(-15),
    },
    {
      id: 2,
      sku: 'HA-PHO-P90R',
      name: 'Phonak Audeo Paradise P90-R',
      category: 'Hearing Aids',
      quantity: 6,
      minQuantity: 4,
      unitPrice: 14000,
      supplier: 'Sonova Gulf',
      lastRestocked: getDateString(-10),
    },
    {
      id: 3,
      sku: 'HA-SIG-PURE',
      name: 'Signia Pure Charge&Go AX',
      category: 'Hearing Aids',
      quantity: 4,
      minQuantity: 4,
      unitPrice: 11000,
      supplier: 'WS Audiology ME',
      lastRestocked: getDateString(-30),
    },
    // Accessories
    {
      id: 4,
      sku: 'ACC-DOME-CL',
      name: 'Closed Domes (Pack of 10)',
      category: 'Accessories',
      quantity: 50,
      minQuantity: 20,
      unitPrice: 150,
      supplier: 'Universal Hearing',
      lastRestocked: getDateString(-7),
    },
    {
      id: 5,
      sku: 'ACC-DOME-OP',
      name: 'Open Domes (Pack of 10)',
      category: 'Accessories',
      quantity: 45,
      minQuantity: 20,
      unitPrice: 150,
      supplier: 'Universal Hearing',
      lastRestocked: getDateString(-7),
    },
    {
      id: 6,
      sku: 'ACC-BATT-312',
      name: 'Batteries Size 312 (Pack of 60)',
      category: 'Batteries',
      quantity: 100,
      minQuantity: 30,
      unitPrice: 80,
      supplier: 'Power Solutions',
      lastRestocked: getDateString(-5),
    },
    {
      id: 7,
      sku: 'ACC-BATT-13',
      name: 'Batteries Size 13 (Pack of 60)',
      category: 'Batteries',
      quantity: 80,
      minQuantity: 30,
      unitPrice: 80,
      supplier: 'Power Solutions',
      lastRestocked: getDateString(-5),
    },
    {
      id: 8,
      sku: 'ACC-CLEAN-KIT',
      name: 'Hearing Aid Cleaning Kit',
      category: 'Accessories',
      quantity: 25,
      minQuantity: 10,
      unitPrice: 75,
      supplier: 'Universal Hearing',
      lastRestocked: getDateString(-14),
    },
    // Low stock item for demo
    {
      id: 9,
      sku: 'ACC-WAX-GUARD',
      name: 'Wax Guards (Pack of 8)',
      category: 'Accessories',
      quantity: 8,
      minQuantity: 15,
      unitPrice: 50,
      supplier: 'Universal Hearing',
      lastRestocked: getDateString(-45),
    },
  ],
};

// ============================================
// GENERAL CLINIC DEMO DATA
// ============================================

export const generalClinicDemoData: DemoDataset = {
  clinicType: 'general',
  clinicName: 'XenonClinic Medical Center',
  description: 'General medical practice with primary care services',

  patients: [
    {
      id: 1,
      emiratesId: generateEmiratesId(1980, 101),
      fullNameEn: 'John Michael Smith',
      dateOfBirth: '1980-05-20',
      gender: 'M',
      phoneNumber: '+971501234101',
      email: 'john.smith@email.com',
      nationality: 'UK',
      insuranceProvider: 'Bupa',
      insurancePolicyNumber: 'BUP-2024-101234',
    },
    {
      id: 2,
      emiratesId: generateEmiratesId(1975, 102),
      fullNameEn: 'Maria Garcia Rodriguez',
      dateOfBirth: '1975-11-12',
      gender: 'F',
      phoneNumber: '+971501234102',
      email: 'maria.garcia@email.com',
      nationality: 'Spain',
      insuranceProvider: 'Cigna',
      insurancePolicyNumber: 'CIG-2024-102345',
    },
    {
      id: 3,
      emiratesId: generateEmiratesId(1990, 103),
      fullNameEn: 'Omar Abdullah',
      fullNameAr: 'عمر عبدالله',
      dateOfBirth: '1990-02-28',
      gender: 'M',
      phoneNumber: '+971501234103',
      email: 'omar.abdullah@email.com',
      nationality: 'Jordan',
      insuranceProvider: 'Daman',
      insurancePolicyNumber: 'DAM-2024-103456',
    },
    {
      id: 4,
      emiratesId: generateEmiratesId(1985, 104),
      fullNameEn: 'Lisa Chen Wong',
      dateOfBirth: '1985-08-15',
      gender: 'F',
      phoneNumber: '+971501234104',
      email: 'lisa.chen@email.com',
      nationality: 'Singapore',
      insuranceProvider: 'AXA Gulf',
      insurancePolicyNumber: 'AXA-2024-104567',
    },
    {
      id: 5,
      emiratesId: generateEmiratesId(1968, 105),
      fullNameEn: 'Robert James Wilson',
      dateOfBirth: '1968-12-03',
      gender: 'M',
      phoneNumber: '+971501234105',
      email: 'robert.wilson@email.com',
      nationality: 'USA',
      insuranceProvider: 'MetLife',
      insurancePolicyNumber: 'MET-2024-105678',
    },
  ],

  appointments: [
    {
      id: 1,
      patientId: 1,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(9, 0),
      duration: 30,
      type: 'General Consultation',
      status: 'CheckedIn',
      providerName: 'Dr. Ahmed Hassan',
    },
    {
      id: 2,
      patientId: 2,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(10, 0),
      duration: 30,
      type: 'Follow-Up',
      status: 'Scheduled',
      providerName: 'Dr. Ahmed Hassan',
    },
    {
      id: 3,
      patientId: 3,
      appointmentDate: getDateString(1),
      appointmentTime: getTimeString(9, 30),
      duration: 30,
      type: 'Vaccination',
      status: 'Confirmed',
      providerName: 'Nurse Fatima',
    },
    {
      id: 4,
      patientId: 4,
      appointmentDate: getDateString(1),
      appointmentTime: getTimeString(11, 0),
      duration: 45,
      type: 'Health Screening',
      status: 'Confirmed',
      providerName: 'Dr. Sarah Khan',
    },
    {
      id: 5,
      patientId: 5,
      appointmentDate: getDateString(2),
      appointmentTime: getTimeString(14, 0),
      duration: 30,
      type: 'Chronic Disease Management',
      status: 'Scheduled',
      providerName: 'Dr. Ahmed Hassan',
    },
  ],

  employees: [
    {
      id: 1,
      employeeId: 'EMP001',
      fullName: 'Dr. Ahmed Hassan',
      email: 'ahmed.hassan@xenonclinic.com',
      department: 'General Medicine',
      position: 'General Practitioner',
      hireDate: '2019-06-01',
      salary: 40000,
      status: 'Active',
    },
    {
      id: 2,
      employeeId: 'EMP002',
      fullName: 'Dr. Sarah Khan',
      email: 'sarah.khan@xenonclinic.com',
      department: 'General Medicine',
      position: 'General Practitioner',
      hireDate: '2020-01-15',
      salary: 38000,
      status: 'Active',
    },
    {
      id: 3,
      employeeId: 'EMP003',
      fullName: 'Nurse Fatima Al Hashmi',
      email: 'fatima.hashmi@xenonclinic.com',
      department: 'Nursing',
      position: 'Registered Nurse',
      hireDate: '2018-03-10',
      salary: 15000,
      status: 'Active',
    },
  ],

  inventory: [
    {
      id: 1,
      sku: 'MED-PARA-500',
      name: 'Paracetamol 500mg (Box of 100)',
      category: 'Medications',
      quantity: 200,
      minQuantity: 50,
      unitPrice: 25,
      supplier: 'Gulf Pharma',
      lastRestocked: getDateString(-7),
    },
    {
      id: 2,
      sku: 'MED-IBUP-400',
      name: 'Ibuprofen 400mg (Box of 50)',
      category: 'Medications',
      quantity: 150,
      minQuantity: 40,
      unitPrice: 35,
      supplier: 'Gulf Pharma',
      lastRestocked: getDateString(-7),
    },
    {
      id: 3,
      sku: 'SUP-SYRINGE-5',
      name: 'Disposable Syringes 5ml (Box of 100)',
      category: 'Medical Supplies',
      quantity: 80,
      minQuantity: 30,
      unitPrice: 45,
      supplier: 'MedSupply UAE',
      lastRestocked: getDateString(-14),
    },
    {
      id: 4,
      sku: 'SUP-BANDAGE',
      name: 'Sterile Bandages (Pack of 50)',
      category: 'Medical Supplies',
      quantity: 60,
      minQuantity: 20,
      unitPrice: 30,
      supplier: 'MedSupply UAE',
      lastRestocked: getDateString(-14),
    },
    {
      id: 5,
      sku: 'VAC-FLU-2024',
      name: 'Influenza Vaccine 2024',
      category: 'Vaccines',
      quantity: 45,
      minQuantity: 20,
      unitPrice: 150,
      supplier: 'VaxDistributors',
      lastRestocked: getDateString(-30),
    },
  ],
};

// ============================================
// DENTAL CLINIC DEMO DATA (Placeholder)
// ============================================

export const dentalClinicDemoData: DemoDataset = {
  clinicType: 'dental',
  clinicName: 'XenonClinic Dental Center',
  description: 'Full-service dental clinic with cosmetic and restorative dentistry',

  patients: [
    {
      id: 1,
      emiratesId: generateEmiratesId(1982, 201),
      fullNameEn: 'Aisha Mohammed Al Rashid',
      fullNameAr: 'عائشة محمد الراشد',
      dateOfBirth: '1982-04-10',
      gender: 'F',
      phoneNumber: '+971501234201',
      email: 'aisha.rashid@email.com',
      nationality: 'UAE',
      insuranceProvider: 'Daman',
      insurancePolicyNumber: 'DAM-2024-201234',
    },
    {
      id: 2,
      emiratesId: generateEmiratesId(1995, 202),
      fullNameEn: 'David Lee Johnson',
      dateOfBirth: '1995-09-22',
      gender: 'M',
      phoneNumber: '+971501234202',
      email: 'david.johnson@email.com',
      nationality: 'USA',
      insuranceProvider: 'Cigna',
      insurancePolicyNumber: 'CIG-2024-202345',
    },
    {
      id: 3,
      emiratesId: generateEmiratesId(1978, 203),
      fullNameEn: 'Priya Sharma',
      dateOfBirth: '1978-01-15',
      gender: 'F',
      phoneNumber: '+971501234203',
      email: 'priya.sharma@email.com',
      nationality: 'India',
      insuranceProvider: 'AXA Gulf',
      insurancePolicyNumber: 'AXA-2024-203456',
    },
  ],

  appointments: [
    {
      id: 1,
      patientId: 1,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(9, 0),
      duration: 45,
      type: 'Dental Cleaning',
      status: 'CheckedIn',
      providerName: 'Dr. Yusuf Al Tamimi',
    },
    {
      id: 2,
      patientId: 2,
      appointmentDate: getDateString(0),
      appointmentTime: getTimeString(10, 30),
      duration: 60,
      type: 'Root Canal',
      status: 'Scheduled',
      providerName: 'Dr. Layla Hassan',
    },
    {
      id: 3,
      patientId: 3,
      appointmentDate: getDateString(1),
      appointmentTime: getTimeString(14, 0),
      duration: 90,
      type: 'Crown Fitting',
      status: 'Confirmed',
      providerName: 'Dr. Yusuf Al Tamimi',
    },
  ],

  employees: [
    {
      id: 1,
      employeeId: 'EMP001',
      fullName: 'Dr. Yusuf Al Tamimi',
      email: 'yusuf.tamimi@xenonclinic.com',
      department: 'Dentistry',
      position: 'General Dentist',
      hireDate: '2019-02-01',
      salary: 45000,
      status: 'Active',
    },
    {
      id: 2,
      employeeId: 'EMP002',
      fullName: 'Dr. Layla Hassan',
      email: 'layla.hassan@xenonclinic.com',
      department: 'Dentistry',
      position: 'Endodontist',
      hireDate: '2020-08-15',
      salary: 50000,
      status: 'Active',
    },
  ],

  inventory: [
    {
      id: 1,
      sku: 'DEN-COMP-A2',
      name: 'Dental Composite A2 Shade',
      category: 'Dental Materials',
      quantity: 30,
      minQuantity: 10,
      unitPrice: 250,
      supplier: 'Dental Supplies ME',
      lastRestocked: getDateString(-10),
    },
    {
      id: 2,
      sku: 'DEN-ANES-LID',
      name: 'Lidocaine Cartridges (Box of 50)',
      category: 'Anesthetics',
      quantity: 25,
      minQuantity: 15,
      unitPrice: 180,
      supplier: 'Dental Supplies ME',
      lastRestocked: getDateString(-7),
    },
    {
      id: 3,
      sku: 'DEN-GLOVE-M',
      name: 'Nitrile Gloves Medium (Box of 100)',
      category: 'Consumables',
      quantity: 50,
      minQuantity: 20,
      unitPrice: 45,
      supplier: 'MedSupply UAE',
      lastRestocked: getDateString(-5),
    },
  ],
};

// ============================================
// AVAILABLE DATASETS
// ============================================

export const availableDatasets: Record<ClinicType, DemoDataset> = {
  audiology: audiologyDemoData,
  general: generalClinicDemoData,
  dental: dentalClinicDemoData,
  ophthalmology: generalClinicDemoData, // Placeholder - uses general for now
};

export const getDatasetByType = (type: ClinicType): DemoDataset => {
  return availableDatasets[type] || generalClinicDemoData;
};
