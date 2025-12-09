/**
 * Demo Module
 * Sales demonstration tools for XenonClinic
 */

export {
  demoService,
  DemoService,
  type DemoStatus,
  type LoadDemoResult,
} from './demoService';

export {
  type ClinicType,
  type DemoDataset,
  type DemoPatient,
  type DemoAppointment,
  type DemoEncounter,
  type DemoAudiogram,
  type DemoHearingAid,
  type DemoEmployee,
  type DemoInventoryItem,
  getDatasetByType,
  availableDatasets,
  audiologyDemoData,
  generalClinicDemoData,
  dentalClinicDemoData,
} from './demoData';
