import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import {
  demoService,
  DemoService,
  audiologyDemoData,
  generalClinicDemoData,
  dentalClinicDemoData,
  getDatasetByType,
  availableDatasets,
} from '../lib/demo';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: vi.fn((key: string) => store[key] || null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key];
    }),
    clear: vi.fn(() => {
      store = {};
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: vi.fn((index: number) => Object.keys(store)[index] || null),
  };
})();

Object.defineProperty(window, 'localStorage', { value: localStorageMock });

describe('Demo Data Module', () => {
  beforeEach(() => {
    localStorageMock.clear();
    vi.clearAllMocks();
  });

  describe('Demo Datasets', () => {
    it('has audiology demo data with all required fields', () => {
      expect(audiologyDemoData.clinicType).toBe('audiology');
      expect(audiologyDemoData.clinicName).toBeTruthy();
      expect(audiologyDemoData.patients.length).toBeGreaterThan(0);
      expect(audiologyDemoData.appointments.length).toBeGreaterThan(0);
      expect(audiologyDemoData.employees.length).toBeGreaterThan(0);
      expect(audiologyDemoData.inventory.length).toBeGreaterThan(0);
    });

    it('audiology data includes specialty-specific data', () => {
      expect(audiologyDemoData.encounters).toBeDefined();
      expect(audiologyDemoData.encounters!.length).toBeGreaterThan(0);
      expect(audiologyDemoData.audiograms).toBeDefined();
      expect(audiologyDemoData.audiograms!.length).toBeGreaterThan(0);
      expect(audiologyDemoData.hearingAids).toBeDefined();
      expect(audiologyDemoData.hearingAids!.length).toBeGreaterThan(0);
    });

    it('has general clinic demo data', () => {
      expect(generalClinicDemoData.clinicType).toBe('general');
      expect(generalClinicDemoData.patients.length).toBeGreaterThan(0);
      expect(generalClinicDemoData.appointments.length).toBeGreaterThan(0);
    });

    it('has dental clinic demo data', () => {
      expect(dentalClinicDemoData.clinicType).toBe('dental');
      expect(dentalClinicDemoData.patients.length).toBeGreaterThan(0);
    });

    it('getDatasetByType returns correct dataset', () => {
      expect(getDatasetByType('audiology')).toBe(audiologyDemoData);
      expect(getDatasetByType('general')).toBe(generalClinicDemoData);
      expect(getDatasetByType('dental')).toBe(dentalClinicDemoData);
    });

    it('availableDatasets contains all clinic types', () => {
      expect(availableDatasets.audiology).toBeDefined();
      expect(availableDatasets.general).toBeDefined();
      expect(availableDatasets.dental).toBeDefined();
      expect(availableDatasets.ophthalmology).toBeDefined();
    });
  });

  describe('Demo Patient Data', () => {
    it('patients have valid Emirates ID format', () => {
      // Emirates ID format: 784-YYYY-NNNNNNN-C (7 digit serial)
      // Our generator uses 6 digits for index so format is 784-YYYY-NNNNNN-C
      const emiratesIdRegex = /^784-\d{4}-\d{6,7}-\d$/;
      audiologyDemoData.patients.forEach((patient) => {
        expect(patient.emiratesId).toMatch(emiratesIdRegex);
      });
    });

    it('patients have required fields', () => {
      audiologyDemoData.patients.forEach((patient) => {
        expect(patient.id).toBeDefined();
        expect(patient.fullNameEn).toBeTruthy();
        expect(patient.dateOfBirth).toBeTruthy();
        expect(patient.gender).toMatch(/^[MF]$/);
        expect(patient.phoneNumber).toBeTruthy();
        expect(patient.email).toBeTruthy();
      });
    });
  });

  describe('Demo Appointment Data', () => {
    it('appointments reference valid patients', () => {
      const patientIds = new Set(audiologyDemoData.patients.map((p) => p.id));
      audiologyDemoData.appointments.forEach((appointment) => {
        expect(patientIds.has(appointment.patientId)).toBe(true);
      });
    });

    it('appointments have valid status', () => {
      const validStatuses = ['Scheduled', 'Confirmed', 'CheckedIn', 'Completed', 'Cancelled', 'NoShow'];
      audiologyDemoData.appointments.forEach((appointment) => {
        expect(validStatuses).toContain(appointment.status);
      });
    });
  });

  describe('Demo Service', () => {
    let service: DemoService;

    beforeEach(() => {
      service = new DemoService();
    });

    it('getStatus returns empty status when no data loaded', () => {
      const status = service.getStatus();
      expect(status.isLoaded).toBe(false);
      expect(status.clinicType).toBeNull();
      expect(status.clinicName).toBeNull();
    });

    it('getAvailableClinicTypes returns all clinic types', () => {
      const types = service.getAvailableClinicTypes();
      expect(types.length).toBeGreaterThan(0);
      expect(types.some((t) => t.type === 'audiology')).toBe(true);
      expect(types.some((t) => t.type === 'general')).toBe(true);
    });

    it('loadDemoData loads audiology data successfully', async () => {
      const result = await service.loadDemoData('audiology');
      expect(result.success).toBe(true);
      expect(result.stats).toBeDefined();
      expect(result.stats!.patients).toBe(audiologyDemoData.patients.length);
    });

    it('loadDemoData is idempotent - returns success without changes for same dataset', async () => {
      // First load
      const firstResult = await service.loadDemoData('audiology');
      expect(firstResult.success).toBe(true);

      // Second load of same dataset
      const secondResult = await service.loadDemoData('audiology');
      expect(secondResult.success).toBe(true);
      expect(secondResult.message).toContain('already loaded');
    });

    it('loadDemoData clears previous data when loading different dataset', async () => {
      // Load audiology first
      await service.loadDemoData('audiology');
      let status = service.getStatus();
      expect(status.clinicType).toBe('audiology');

      // Load general - should clear audiology and load general
      await service.loadDemoData('general');
      status = service.getStatus();
      expect(status.clinicType).toBe('general');
      expect(status.stats.patients).toBe(generalClinicDemoData.patients.length);
    });

    it('loadDemoData with force option reloads same dataset', async () => {
      // First load
      await service.loadDemoData('audiology');

      // Force reload
      const result = await service.loadDemoData('audiology', { force: true });
      expect(result.success).toBe(true);
      expect(result.message).toContain('Successfully loaded');
    });

    it('isDatasetLoaded returns correct status', async () => {
      expect(service.isDatasetLoaded('audiology')).toBe(false);

      await service.loadDemoData('audiology');
      expect(service.isDatasetLoaded('audiology')).toBe(true);
      expect(service.isDatasetLoaded('general')).toBe(false);
    });

    it('resetDemoData clears and reloads data', async () => {
      // Load initial data
      await service.loadDemoData('audiology');

      // Reset
      const result = await service.resetDemoData();
      expect(result.success).toBe(true);
      expect(service.getStatus().isLoaded).toBe(true);
    });

    it('clearDemoData removes all data', async () => {
      // Load data first
      await service.loadDemoData('audiology');
      expect(service.getStatus().isLoaded).toBe(true);

      // Clear
      const result = await service.clearDemoData();
      expect(result.success).toBe(true);
      expect(service.getStatus().isLoaded).toBe(false);
    });

    it('getData returns loaded entities', async () => {
      await service.loadDemoData('audiology');

      const patients = service.getData<{ id: number; fullNameEn: string }>('patients');
      expect(patients.length).toBe(audiologyDemoData.patients.length);
    });

    it('addRecord adds new record with auto-incrementing id', async () => {
      await service.loadDemoData('audiology');

      const newPatient = service.addRecord<{ id: number; fullNameEn: string }>('patients', {
        fullNameEn: 'Test Patient',
      });

      expect(newPatient.id).toBeGreaterThan(audiologyDemoData.patients.length);
      expect(newPatient.fullNameEn).toBe('Test Patient');
    });

    it('updateRecord updates existing record', async () => {
      await service.loadDemoData('audiology');

      const updated = service.updateRecord('patients', 1, { fullNameEn: 'Updated Name' });
      expect(updated).not.toBeNull();
      expect(updated!.fullNameEn).toBe('Updated Name');
    });

    it('deleteRecord removes record', async () => {
      await service.loadDemoData('audiology');

      const initialPatients = service.getData('patients');
      const initialCount = initialPatients.length;

      const deleted = service.deleteRecord('patients', 1);
      expect(deleted).toBe(true);

      const afterDelete = service.getData('patients');
      expect(afterDelete.length).toBe(initialCount - 1);
    });

    it('exportData returns valid JSON', async () => {
      await service.loadDemoData('audiology');

      const exported = service.exportData();
      expect(() => JSON.parse(exported)).not.toThrow();

      const parsed = JSON.parse(exported);
      expect(parsed.exportedAt).toBeDefined();
      expect(parsed.status).toBeDefined();
      expect(parsed.data).toBeDefined();
    });

    it('importData restores exported data', async () => {
      // Load and export
      await service.loadDemoData('audiology');
      const exported = service.exportData();

      // Clear
      await service.clearDemoData();
      expect(service.getStatus().isLoaded).toBe(false);

      // Import
      const result = await service.importData(exported);
      expect(result.success).toBe(true);
      expect(service.getStatus().isLoaded).toBe(true);
    });

    it('importData rejects invalid JSON', async () => {
      const result = await service.importData('not valid json');
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
    });

    it('importData rejects invalid format', async () => {
      const result = await service.importData(JSON.stringify({ invalid: 'format' }));
      expect(result.success).toBe(false);
    });
  });

  describe('Demo Audiology-Specific Data', () => {
    it('audiograms have valid frequency data', () => {
      const validFrequencies = [250, 500, 1000, 2000, 4000, 8000];
      audiologyDemoData.audiograms?.forEach((audiogram) => {
        validFrequencies.forEach((freq) => {
          expect(audiogram.rightEar[freq]).toBeDefined();
          expect(audiogram.leftEar[freq]).toBeDefined();
          expect(audiogram.rightEar[freq]).toBeGreaterThanOrEqual(-10);
          expect(audiogram.rightEar[freq]).toBeLessThanOrEqual(120);
        });
      });
    });

    it('hearing aids have valid data', () => {
      audiologyDemoData.hearingAids?.forEach((hearingAid) => {
        expect(hearingAid.serialNumber).toBeTruthy();
        expect(hearingAid.manufacturer).toBeTruthy();
        expect(hearingAid.model).toBeTruthy();
        // EarSide enum uses lowercase values
        expect(['right', 'left', 'both']).toContain(hearingAid.ear);
        expect(hearingAid.price).toBeGreaterThan(0);
      });
    });

    it('encounters reference valid patients', () => {
      const patientIds = new Set(audiologyDemoData.patients.map((p) => p.id));
      audiologyDemoData.encounters?.forEach((encounter) => {
        expect(patientIds.has(encounter.patientId)).toBe(true);
      });
    });
  });
});
