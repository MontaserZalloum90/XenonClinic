import { describe, it, expect } from 'vitest';
import {
  EarSide,
  TestType,
  HearingLossGrade,
  AUDIOGRAM_FREQUENCIES,
  HearingAidStyle,
  HearingAidStatus,
  FittingStatus,
  EncounterStatus,
  EncounterType,
  TaskStatus,
  TaskPriority,
  ConsentType,
  ConsentStatus,
  AttachmentCategory,
} from './audiology';
import type {
  Audiogram,
  AudiogramDataPoint,
  HearingAid,
  Encounter,
  ConsentForm,
  Attachment,
} from './audiology';

describe('Audiology Types', () => {
  describe('EarSide', () => {
    it('has left and right values', () => {
      expect(EarSide.Left).toBe('left');
      expect(EarSide.Right).toBe('right');
    });
  });

  describe('TestType', () => {
    it('has all test types', () => {
      expect(TestType.AirConduction).toBe('air');
      expect(TestType.BoneConduction).toBe('bone');
      expect(TestType.SpeechRecognition).toBe('speech');
      expect(TestType.Tympanometry).toBe('tympanometry');
    });
  });

  describe('HearingLossGrade', () => {
    it('has all hearing loss grades', () => {
      expect(HearingLossGrade.Normal).toBe('normal');
      expect(HearingLossGrade.Mild).toBe('mild');
      expect(HearingLossGrade.Moderate).toBe('moderate');
      expect(HearingLossGrade.ModeratelySevere).toBe('moderately_severe');
      expect(HearingLossGrade.Severe).toBe('severe');
      expect(HearingLossGrade.Profound).toBe('profound');
    });

    it('has 6 grades total', () => {
      expect(Object.keys(HearingLossGrade).length).toBe(6);
    });
  });

  describe('AUDIOGRAM_FREQUENCIES', () => {
    it('has standard audiogram frequencies', () => {
      expect(AUDIOGRAM_FREQUENCIES).toEqual([250, 500, 1000, 2000, 4000, 8000]);
    });

    it('has 6 frequencies', () => {
      expect(AUDIOGRAM_FREQUENCIES.length).toBe(6);
    });
  });

  describe('HearingAidStyle', () => {
    it('has all hearing aid styles', () => {
      expect(HearingAidStyle.BTE).toBe('BTE');
      expect(HearingAidStyle.RIC).toBe('RIC');
      expect(HearingAidStyle.ITE).toBe('ITE');
      expect(HearingAidStyle.ITC).toBe('ITC');
      expect(HearingAidStyle.CIC).toBe('CIC');
      expect(HearingAidStyle.IIC).toBe('IIC');
      expect(HearingAidStyle.CROS).toBe('CROS');
      expect(HearingAidStyle.BiCROS).toBe('BiCROS');
    });

    it('has 8 styles total', () => {
      expect(Object.keys(HearingAidStyle).length).toBe(8);
    });
  });

  describe('HearingAidStatus', () => {
    it('has all status values', () => {
      expect(HearingAidStatus.Active).toBe('active');
      expect(HearingAidStatus.InRepair).toBe('in_repair');
      expect(HearingAidStatus.Replaced).toBe('replaced');
      expect(HearingAidStatus.Lost).toBe('lost');
      expect(HearingAidStatus.Returned).toBe('returned');
    });
  });

  describe('FittingStatus', () => {
    it('has all fitting status values', () => {
      expect(FittingStatus.Scheduled).toBe('scheduled');
      expect(FittingStatus.InitialFitting).toBe('initial_fitting');
      expect(FittingStatus.FollowUp).toBe('follow_up');
      expect(FittingStatus.Adjusted).toBe('adjusted');
      expect(FittingStatus.Completed).toBe('completed');
    });
  });

  describe('EncounterStatus', () => {
    it('has all encounter status values', () => {
      expect(EncounterStatus.Scheduled).toBe('scheduled');
      expect(EncounterStatus.CheckedIn).toBe('checked_in');
      expect(EncounterStatus.InProgress).toBe('in_progress');
      expect(EncounterStatus.Completed).toBe('completed');
      expect(EncounterStatus.Cancelled).toBe('cancelled');
      expect(EncounterStatus.NoShow).toBe('no_show');
    });

    it('has 6 status values', () => {
      expect(Object.keys(EncounterStatus).length).toBe(6);
    });
  });

  describe('EncounterType', () => {
    it('has all encounter types', () => {
      expect(EncounterType.InitialConsultation).toBe('initial_consultation');
      expect(EncounterType.FollowUp).toBe('follow_up');
      expect(EncounterType.HearingTest).toBe('hearing_test');
      expect(EncounterType.HearingAidFitting).toBe('hearing_aid_fitting');
      expect(EncounterType.HearingAidAdjustment).toBe('hearing_aid_adjustment');
      expect(EncounterType.HearingAidRepair).toBe('hearing_aid_repair');
      expect(EncounterType.Counseling).toBe('counseling');
      expect(EncounterType.TinnitusEvaluation).toBe('tinnitus_evaluation');
      expect(EncounterType.BalanceAssessment).toBe('balance_assessment');
      expect(EncounterType.CochlearImplantMapping).toBe('ci_mapping');
    });

    it('has 10 encounter types', () => {
      expect(Object.keys(EncounterType).length).toBe(10);
    });
  });

  describe('TaskStatus', () => {
    it('has all task status values', () => {
      expect(TaskStatus.Pending).toBe('pending');
      expect(TaskStatus.InProgress).toBe('in_progress');
      expect(TaskStatus.Completed).toBe('completed');
      expect(TaskStatus.Cancelled).toBe('cancelled');
    });
  });

  describe('TaskPriority', () => {
    it('has all priority levels', () => {
      expect(TaskPriority.Low).toBe('low');
      expect(TaskPriority.Normal).toBe('normal');
      expect(TaskPriority.High).toBe('high');
      expect(TaskPriority.Urgent).toBe('urgent');
    });
  });

  describe('ConsentType', () => {
    it('has all consent types', () => {
      expect(ConsentType.GeneralTreatment).toBe('general_treatment');
      expect(ConsentType.HearingTest).toBe('hearing_test');
      expect(ConsentType.HearingAidTrial).toBe('hearing_aid_trial');
      expect(ConsentType.HearingAidPurchase).toBe('hearing_aid_purchase');
      expect(ConsentType.ReleaseOfInformation).toBe('release_of_information');
      expect(ConsentType.Photography).toBe('photography');
      expect(ConsentType.Research).toBe('research');
      expect(ConsentType.Telehealth).toBe('telehealth');
    });

    it('has 8 consent types', () => {
      expect(Object.keys(ConsentType).length).toBe(8);
    });
  });

  describe('ConsentStatus', () => {
    it('has all consent status values', () => {
      expect(ConsentStatus.Pending).toBe('pending');
      expect(ConsentStatus.Signed).toBe('signed');
      expect(ConsentStatus.Declined).toBe('declined');
      expect(ConsentStatus.Revoked).toBe('revoked');
      expect(ConsentStatus.Expired).toBe('expired');
    });
  });

  describe('AttachmentCategory', () => {
    it('has all attachment categories', () => {
      expect(AttachmentCategory.ConsentForm).toBe('consent_form');
      expect(AttachmentCategory.Audiogram).toBe('audiogram');
      expect(AttachmentCategory.ReferralLetter).toBe('referral_letter');
      expect(AttachmentCategory.MedicalReport).toBe('medical_report');
      expect(AttachmentCategory.InsuranceDocument).toBe('insurance_document');
      expect(AttachmentCategory.HearingAidManual).toBe('hearing_aid_manual');
      expect(AttachmentCategory.WarrantyDocument).toBe('warranty_document');
      expect(AttachmentCategory.PatientPhoto).toBe('patient_photo');
      expect(AttachmentCategory.CorrespondenceIn).toBe('correspondence_in');
      expect(AttachmentCategory.CorrespondenceOut).toBe('correspondence_out');
      expect(AttachmentCategory.Other).toBe('other');
    });

    it('has 11 attachment categories', () => {
      expect(Object.keys(AttachmentCategory).length).toBe(11);
    });
  });
});

describe('Audiology Interface Tests', () => {
  describe('AudiogramDataPoint', () => {
    it('can create a valid data point', () => {
      const dataPoint: AudiogramDataPoint = {
        frequency: 1000,
        threshold: 25,
        noResponse: false,
        masked: false,
      };

      expect(dataPoint.frequency).toBe(1000);
      expect(dataPoint.threshold).toBe(25);
      expect(dataPoint.noResponse).toBe(false);
    });

    it('can represent no response', () => {
      const dataPoint: AudiogramDataPoint = {
        frequency: 8000,
        threshold: 120,
        noResponse: true,
      };

      expect(dataPoint.noResponse).toBe(true);
    });
  });

  describe('Audiogram', () => {
    it('can create a valid audiogram object', () => {
      const audiogram: Audiogram = {
        id: 1,
        patientId: 100,
        testDate: '2024-06-15',
        testedBy: 'Dr. Smith',
        rightEarAir: [
          { frequency: 250, threshold: 20 },
          { frequency: 500, threshold: 25 },
          { frequency: 1000, threshold: 30 },
          { frequency: 2000, threshold: 35 },
          { frequency: 4000, threshold: 40 },
          { frequency: 8000, threshold: 45 },
        ],
        leftEarAir: [
          { frequency: 250, threshold: 15 },
          { frequency: 500, threshold: 20 },
          { frequency: 1000, threshold: 25 },
          { frequency: 2000, threshold: 30 },
          { frequency: 4000, threshold: 35 },
          { frequency: 8000, threshold: 40 },
        ],
        rightPTA: 33,
        leftPTA: 28,
        rightHearingLossGrade: HearingLossGrade.Mild,
        leftHearingLossGrade: HearingLossGrade.Mild,
        createdAt: '2024-06-15T10:00:00Z',
      };

      expect(audiogram.id).toBe(1);
      expect(audiogram.rightEarAir.length).toBe(6);
      expect(audiogram.leftEarAir.length).toBe(6);
      expect(audiogram.rightPTA).toBe(33);
      expect(audiogram.rightHearingLossGrade).toBe(HearingLossGrade.Mild);
    });
  });

  describe('HearingAid', () => {
    it('can create a valid hearing aid object', () => {
      const hearingAid: HearingAid = {
        id: 1,
        patientId: 100,
        ear: EarSide.Right,
        manufacturer: 'Phonak',
        model: 'Audeo Paradise P90',
        serialNumber: 'PH123456789',
        style: HearingAidStyle.RIC,
        technologyLevel: 'Premium',
        purchaseDate: '2024-01-15',
        warrantyStartDate: '2024-01-15',
        warrantyEndDate: '2027-01-15',
        status: HearingAidStatus.Active,
        createdAt: '2024-01-15T10:00:00Z',
      };

      expect(hearingAid.id).toBe(1);
      expect(hearingAid.manufacturer).toBe('Phonak');
      expect(hearingAid.style).toBe(HearingAidStyle.RIC);
      expect(hearingAid.status).toBe(HearingAidStatus.Active);
    });
  });

  describe('Encounter', () => {
    it('can create a valid encounter object', () => {
      const encounter: Encounter = {
        id: 1,
        patientId: 100,
        branchId: 1,
        encounterDate: '2024-06-15',
        encounterType: EncounterType.InitialConsultation,
        status: EncounterStatus.Completed,
        chiefComplaint: 'Difficulty hearing in noisy environments',
        createdBy: 'admin',
        createdAt: '2024-06-15T09:00:00Z',
      };

      expect(encounter.id).toBe(1);
      expect(encounter.encounterType).toBe(EncounterType.InitialConsultation);
      expect(encounter.status).toBe(EncounterStatus.Completed);
    });
  });

  describe('ConsentForm', () => {
    it('can create a valid consent form object', () => {
      const consent: ConsentForm = {
        id: 1,
        patientId: 100,
        consentType: ConsentType.HearingAidTrial,
        status: ConsentStatus.Signed,
        signedAt: '2024-06-15T10:30:00Z',
        signedBy: 'John Doe',
        signatureMethod: 'electronic',
        createdBy: 'admin',
        createdAt: '2024-06-15T10:00:00Z',
      };

      expect(consent.id).toBe(1);
      expect(consent.consentType).toBe(ConsentType.HearingAidTrial);
      expect(consent.status).toBe(ConsentStatus.Signed);
      expect(consent.signatureMethod).toBe('electronic');
    });
  });

  describe('Attachment', () => {
    it('can create a valid attachment object', () => {
      const attachment: Attachment = {
        id: 1,
        patientId: 100,
        category: AttachmentCategory.Audiogram,
        fileName: 'audiogram_2024_06_15.pdf',
        originalFileName: 'Audiogram Report.pdf',
        fileSize: 102400,
        mimeType: 'application/pdf',
        storagePath: '/uploads/patients/100/audiograms/',
        uploadedBy: 'admin',
        uploadedAt: '2024-06-15T10:00:00Z',
      };

      expect(attachment.id).toBe(1);
      expect(attachment.category).toBe(AttachmentCategory.Audiogram);
      expect(attachment.mimeType).toBe('application/pdf');
    });
  });
});
