/**
 * Demo Data Service
 * Handles loading, resetting, and managing demo datasets
 * Provides idempotent operations safe to run multiple times
 */

import { ClinicType, DemoDataset, getDatasetByType, availableDatasets } from './demoData';

// ============================================
// TYPES
// ============================================

export interface DemoStatus {
  isLoaded: boolean;
  clinicType: ClinicType | null;
  clinicName: string | null;
  loadedAt: string | null;
  stats: {
    patients: number;
    appointments: number;
    encounters: number;
    audiograms: number;
    hearingAids: number;
    employees: number;
    inventory: number;
  };
}

export interface LoadDemoResult {
  success: boolean;
  message: string;
  stats?: DemoStatus['stats'];
  error?: string;
}

const DEMO_STATUS_KEY = 'xenon_demo_status';
const DEMO_DATA_PREFIX = 'xenon_demo_';

// ============================================
// STORAGE HELPERS
// ============================================

const getStorageKey = (entity: string): string => `${DEMO_DATA_PREFIX}${entity}`;

const saveToStorage = <T>(entity: string, data: T[]): void => {
  try {
    localStorage.setItem(getStorageKey(entity), JSON.stringify(data));
  } catch (error) {
    console.error(`Failed to save ${entity} to storage:`, error);
    throw new Error(`Storage quota exceeded. Cannot save ${entity}.`);
  }
};

const loadFromStorage = <T>(entity: string): T[] => {
  try {
    const data = localStorage.getItem(getStorageKey(entity));
    return data ? JSON.parse(data) : [];
  } catch (error) {
    console.error(`Failed to load ${entity} from storage:`, error);
    return [];
  }
};

const clearFromStorage = (entity: string): void => {
  localStorage.removeItem(getStorageKey(entity));
};

// ============================================
// DEMO SERVICE
// ============================================

class DemoService {
  /**
   * Get current demo status
   */
  getStatus(): DemoStatus {
    try {
      const statusStr = localStorage.getItem(DEMO_STATUS_KEY);
      if (statusStr) {
        return JSON.parse(statusStr);
      }
    } catch {
      // Ignore parse errors
    }

    return {
      isLoaded: false,
      clinicType: null,
      clinicName: null,
      loadedAt: null,
      stats: {
        patients: 0,
        appointments: 0,
        encounters: 0,
        audiograms: 0,
        hearingAids: 0,
        employees: 0,
        inventory: 0,
      },
    };
  }

  /**
   * Save demo status
   */
  private saveStatus(status: DemoStatus): void {
    localStorage.setItem(DEMO_STATUS_KEY, JSON.stringify(status));
  }

  /**
   * Check if a specific demo dataset is already loaded
   * This enables idempotent operations
   */
  isDatasetLoaded(clinicType: ClinicType): boolean {
    const status = this.getStatus();
    return status.isLoaded && status.clinicType === clinicType;
  }

  /**
   * Get available clinic types and their descriptions
   */
  getAvailableClinicTypes(): Array<{
    type: ClinicType;
    name: string;
    description: string;
  }> {
    return Object.entries(availableDatasets).map(([type, dataset]) => ({
      type: type as ClinicType,
      name: dataset.clinicName,
      description: dataset.description,
    }));
  }

  /**
   * Load demo dataset
   * Idempotent: If same dataset is already loaded, returns success without changes
   * If different dataset is loaded, clears it first
   */
  async loadDemoData(
    clinicType: ClinicType,
    options: { force?: boolean } = {}
  ): Promise<LoadDemoResult> {
    const { force = false } = options;

    try {
      // Check if same dataset is already loaded (idempotent check)
      if (!force && this.isDatasetLoaded(clinicType)) {
        const status = this.getStatus();
        return {
          success: true,
          message: `Demo data for ${status.clinicName} is already loaded.`,
          stats: status.stats,
        };
      }

      // If different dataset is loaded or force reload, clear first
      const currentStatus = this.getStatus();
      if (currentStatus.isLoaded) {
        await this.clearDemoData();
      }

      // Get the dataset
      const dataset = getDatasetByType(clinicType);

      // Load each entity type
      saveToStorage('patients', dataset.patients);
      saveToStorage('appointments', dataset.appointments);
      saveToStorage('employees', dataset.employees);
      saveToStorage('inventory', dataset.inventory);

      // Load audiology-specific data if available
      if (dataset.encounters) {
        saveToStorage('encounters', dataset.encounters);
      }
      if (dataset.audiograms) {
        saveToStorage('audiograms', dataset.audiograms);
      }
      if (dataset.hearingAids) {
        saveToStorage('hearingAids', dataset.hearingAids);
      }

      // Calculate stats
      const stats: DemoStatus['stats'] = {
        patients: dataset.patients.length,
        appointments: dataset.appointments.length,
        encounters: dataset.encounters?.length || 0,
        audiograms: dataset.audiograms?.length || 0,
        hearingAids: dataset.hearingAids?.length || 0,
        employees: dataset.employees.length,
        inventory: dataset.inventory.length,
      };

      // Save status
      const newStatus: DemoStatus = {
        isLoaded: true,
        clinicType,
        clinicName: dataset.clinicName,
        loadedAt: new Date().toISOString(),
        stats,
      };
      this.saveStatus(newStatus);

      return {
        success: true,
        message: `Successfully loaded demo data for ${dataset.clinicName}`,
        stats,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        success: false,
        message: 'Failed to load demo data',
        error: errorMessage,
      };
    }
  }

  /**
   * Reset demo data to fresh state
   * Clears current data and optionally reloads
   */
  async resetDemoData(clinicType?: ClinicType): Promise<LoadDemoResult> {
    try {
      const currentStatus = this.getStatus();
      const typeToLoad = clinicType || currentStatus.clinicType || 'audiology';

      // Clear existing data
      await this.clearDemoData();

      // Reload fresh data
      return this.loadDemoData(typeToLoad as ClinicType, { force: true });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        success: false,
        message: 'Failed to reset demo data',
        error: errorMessage,
      };
    }
  }

  /**
   * Clear all demo data
   */
  async clearDemoData(): Promise<LoadDemoResult> {
    try {
      // Clear all entity types
      const entities = [
        'patients',
        'appointments',
        'encounters',
        'audiograms',
        'hearingAids',
        'employees',
        'inventory',
      ];

      for (const entity of entities) {
        clearFromStorage(entity);
      }

      // Clear status
      localStorage.removeItem(DEMO_STATUS_KEY);

      return {
        success: true,
        message: 'Demo data cleared successfully',
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        success: false,
        message: 'Failed to clear demo data',
        error: errorMessage,
      };
    }
  }

  /**
   * Get loaded demo data for a specific entity
   */
  getData<T>(entity: string): T[] {
    return loadFromStorage<T>(entity);
  }

  /**
   * Add a record to demo data (for mutations during demo)
   */
  addRecord<T extends { id: number }>(entity: string, record: Omit<T, 'id'>): T {
    const existing = loadFromStorage<T>(entity);
    const maxId = existing.reduce((max, item) => Math.max(max, item.id), 0);
    const newRecord = { ...record, id: maxId + 1 } as T;
    saveToStorage(entity, [...existing, newRecord]);
    return newRecord;
  }

  /**
   * Update a record in demo data
   */
  updateRecord<T extends { id: number }>(entity: string, id: number, updates: Partial<T>): T | null {
    const existing = loadFromStorage<T>(entity);
    const index = existing.findIndex((item) => item.id === id);
    if (index === -1) return null;

    const updated = { ...existing[index], ...updates };
    existing[index] = updated;
    saveToStorage(entity, existing);
    return updated;
  }

  /**
   * Delete a record from demo data
   */
  deleteRecord<T extends { id: number }>(entity: string, id: number): boolean {
    const existing = loadFromStorage<T>(entity);
    const filtered = existing.filter((item) => item.id !== id);
    if (filtered.length === existing.length) return false;

    saveToStorage(entity, filtered);
    return true;
  }

  /**
   * Export current demo data for backup
   */
  exportData(): string {
    const status = this.getStatus();
    const entities = [
      'patients',
      'appointments',
      'encounters',
      'audiograms',
      'hearingAids',
      'employees',
      'inventory',
    ];

    const exportObj: Record<string, unknown> = {
      exportedAt: new Date().toISOString(),
      status,
      data: {},
    };

    for (const entity of entities) {
      (exportObj.data as Record<string, unknown>)[entity] = loadFromStorage(entity);
    }

    return JSON.stringify(exportObj, null, 2);
  }

  /**
   * Import demo data from backup
   */
  async importData(jsonString: string): Promise<LoadDemoResult> {
    try {
      const imported = JSON.parse(jsonString);

      if (!imported.data || !imported.status) {
        return {
          success: false,
          message: 'Invalid import format',
          error: 'Missing required data or status fields',
        };
      }

      // Clear existing data
      await this.clearDemoData();

      // Import each entity
      const data = imported.data as Record<string, unknown[]>;
      for (const [entity, records] of Object.entries(data)) {
        if (Array.isArray(records) && records.length > 0) {
          saveToStorage(entity, records);
        }
      }

      // Restore status
      this.saveStatus(imported.status);

      return {
        success: true,
        message: 'Demo data imported successfully',
        stats: imported.status.stats,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Invalid JSON';
      return {
        success: false,
        message: 'Failed to import demo data',
        error: errorMessage,
      };
    }
  }
}

// Export singleton instance
export const demoService = new DemoService();

// Export class for testing
export { DemoService };
