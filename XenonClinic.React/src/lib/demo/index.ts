/**
 * Demo Module
 * Sales demonstration tools for XenonClinic
 */

export {
  demoService,
  DemoService,
  type DemoStatus,
  type LoadDemoResult,
} from "./demoService";

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
} from "./demoData";

// Specialty Demo Data
export {
  orthopedicsData,
  oncologyData,
  laboratoryData,
  radiologyData,
  pharmacyData,
  hrData,
  financialData,
  dentalData,
  cardiologyData,
  dermatologyData,
  pediatricsData,
  allSpecialtyData,
} from "./specialtyData";

// Business Module Demo Data
export {
  marketingData,
  salesData,
  inventoryData,
  physiotherapyData,
  neurologyData,
  ophthalmologyData,
  allBusinessData,
} from "./businessData";

// Medical Services Demo Data
export {
  dialysisData,
  entData,
  fertilityData,
  obgynData,
  clinicalVisitData,
  analyticsData,
  workflowData,
  portalData,
  allMedicalServicesData,
} from "./medicalServicesData";

// Core Modules Demo Data
export {
  patientsData,
  appointmentsData,
  multiTenancyData,
  usersData,
  securityData,
  payrollData,
  allCoreModulesData,
} from "./coreModulesData";
