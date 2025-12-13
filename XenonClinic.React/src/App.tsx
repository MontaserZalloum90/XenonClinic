import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./contexts/AuthContext";
import { ProtectedRoute, Roles } from "./components/ProtectedRoute";
import { Layout } from "./components/layout/Layout";
import { ToastProvider } from "./components/ui/Toast";

// Core Pages
import { Login } from "./pages/Login";
import { Dashboard } from "./pages/Dashboard";
import { NotFound, Forbidden } from "./pages/Error";

// Existing Module Pages
import { AppointmentsList } from "./pages/Appointments/AppointmentsList";
import { PatientsList } from "./pages/Patients/PatientsList";
import { LaboratoryList } from "./pages/Laboratory/LaboratoryList";
import { HRList } from "./pages/HR/HRList";
import { FinancialList } from "./pages/Financial/FinancialList";
import { InventoryList } from "./pages/Inventory/InventoryList";
import { PharmacyList } from "./pages/Pharmacy/PharmacyList";
import { RadiologyList } from "./pages/Radiology/RadiologyList";
import { AudiologyList } from "./pages/Audiology/AudiologyList";
import { MarketingList } from "./pages/Marketing/MarketingList";
import { AdminDashboard, TranslationManagement } from "./pages/Admin";

// New Core Module Pages
import { ClinicalVisitsList } from "./pages/ClinicalVisits";
import { AnalyticsDashboard, ReportsList } from "./pages/Analytics";
import { EmployeesList, PayrollList, SalaryStructures } from "./pages/HR";
import { WorkflowDefinitions, WorkflowInstances } from "./pages/Workflow";

// Portal Pages
import {
  PortalRegistration,
  PortalLogin,
  PortalDashboard,
  PortalProfile,
  PortalDocuments,
  PortalAppointments,
} from "./pages/Portal";

// Specialty Module Pages
import {
  DentalDashboard,
  DentalCharts,
  DentalTreatments,
  PeriodontalExams,
} from "./pages/Dental";
import {
  CardiologyDashboard,
  ECGRecords,
  EchoRecords,
  StressTests,
  CathLab,
  RiskCalculator,
} from "./pages/Cardiology";
import {
  OphthalmologyDashboard,
  VisualAcuityTests,
  RefractionTests,
  IOPMeasurements,
  SlitLampExams,
  FundusExams,
  GlassesPrescriptions,
} from "./pages/Ophthalmology";
import {
  OrthopedicsDashboard,
  OrthopedicExams,
  FractureRecords,
  Surgeries,
} from "./pages/Orthopedics";
import {
  DermatologyDashboard,
  SkinExams,
  SkinPhotos,
  MoleMappings,
  Biopsies,
} from "./pages/Dermatology";
import {
  OncologyDashboard,
  Diagnoses,
  ChemotherapySessions,
  TreatmentPlans,
} from "./pages/Oncology";
import {
  NeurologyDashboard,
  NeurologicalExams,
  EEGRecords,
  EMGStudies,
} from "./pages/Neurology";
import {
  PediatricsDashboard,
  GrowthCharts,
  Vaccinations,
  DevelopmentMilestones,
} from "./pages/Pediatrics";
import {
  OBGYNDashboard,
  Pregnancies,
  PrenatalVisits,
  Ultrasounds,
} from "./pages/OBGYN";
import {
  PhysiotherapyDashboard,
  TherapySessions,
  ExercisePrograms,
} from "./pages/Physiotherapy";
import { ENTDashboard, AudiometryTests, HearingAids } from "./pages/ENT";
import {
  FertilityDashboard,
  IVFCycles,
  EmbryoRecords,
} from "./pages/Fertility";
import {
  DialysisDashboard,
  DialysisSessions,
  VascularAccess,
} from "./pages/Dialysis";

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

// Helper component for protected routes with layout
const ProtectedPage = ({
  children,
  roles,
}: {
  children: React.ReactNode;
  roles?: string[];
}) => (
  <ProtectedRoute requiredRoles={roles}>
    <Layout>{children}</Layout>
  </ProtectedRoute>
);

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ToastProvider>
          <BrowserRouter>
            <Routes>
              {/* Public Routes */}
              <Route path="/login" element={<Login />} />

              {/* Portal Public Routes */}
              <Route path="/portal/register" element={<PortalRegistration />} />
              <Route path="/portal/login" element={<PortalLogin />} />

              {/* Error Pages */}
              <Route path="/forbidden" element={<Forbidden />} />
              <Route path="/404" element={<NotFound />} />

              {/* Dashboard */}
              <Route
                path="/"
                element={
                  <ProtectedPage>
                    <Dashboard />
                  </ProtectedPage>
                }
              />

              {/* Core Modules */}
              <Route
                path="/appointments"
                element={
                  <ProtectedPage>
                    <AppointmentsList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/patients"
                element={
                  <ProtectedPage>
                    <PatientsList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/clinical-visits"
                element={
                  <ProtectedPage>
                    <ClinicalVisitsList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/laboratory"
                element={
                  <ProtectedPage
                    roles={[
                      Roles.ADMIN,
                      Roles.DOCTOR,
                      Roles.NURSE,
                      Roles.LAB_TECHNICIAN,
                    ]}
                  >
                    <LaboratoryList />
                  </ProtectedPage>
                }
              />

              {/* HR Module */}
              <Route
                path="/hr"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.HR_MANAGER]}>
                    <HRList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/hr/employees"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.HR_MANAGER]}>
                    <EmployeesList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/hr/payroll"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.HR_MANAGER]}>
                    <PayrollList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/hr/salary-structures"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.HR_MANAGER]}>
                    <SalaryStructures />
                  </ProtectedPage>
                }
              />

              {/* Financial Module */}
              <Route
                path="/financial"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.ACCOUNTANT]}>
                    <FinancialList />
                  </ProtectedPage>
                }
              />

              {/* Inventory & Pharmacy */}
              <Route
                path="/inventory"
                element={
                  <ProtectedPage
                    roles={[Roles.ADMIN, Roles.PHARMACIST, Roles.NURSE]}
                  >
                    <InventoryList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/pharmacy"
                element={
                  <ProtectedPage
                    roles={[Roles.ADMIN, Roles.DOCTOR, Roles.PHARMACIST]}
                  >
                    <PharmacyList />
                  </ProtectedPage>
                }
              />

              {/* Radiology & Audiology */}
              <Route
                path="/radiology"
                element={
                  <ProtectedPage
                    roles={[Roles.ADMIN, Roles.DOCTOR, Roles.RADIOLOGIST]}
                  >
                    <RadiologyList />
                  </ProtectedPage>
                }
              />
              <Route
                path="/audiology"
                element={
                  <ProtectedPage>
                    <AudiologyList />
                  </ProtectedPage>
                }
              />

              {/* Marketing */}
              <Route
                path="/marketing"
                element={
                  <ProtectedPage roles={[Roles.ADMIN, Roles.MARKETING_MANAGER]}>
                    <MarketingList />
                  </ProtectedPage>
                }
              />

              {/* Analytics */}
              <Route
                path="/analytics"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <AnalyticsDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/analytics/reports"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <ReportsList />
                  </ProtectedPage>
                }
              />

              {/* Workflow */}
              <Route
                path="/workflow"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <WorkflowDefinitions />
                  </ProtectedPage>
                }
              />
              <Route
                path="/workflow/definitions"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <WorkflowDefinitions />
                  </ProtectedPage>
                }
              />
              <Route
                path="/workflow/instances"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <WorkflowInstances />
                  </ProtectedPage>
                }
              />

              {/* Admin */}
              <Route
                path="/admin"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <AdminDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/admin/translations"
                element={
                  <ProtectedPage roles={[Roles.ADMIN]}>
                    <TranslationManagement />
                  </ProtectedPage>
                }
              />

              {/* Patient Portal (Protected) */}
              <Route
                path="/portal"
                element={
                  <ProtectedPage>
                    <PortalDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/portal/dashboard"
                element={
                  <ProtectedPage>
                    <PortalDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/portal/profile"
                element={
                  <ProtectedPage>
                    <PortalProfile />
                  </ProtectedPage>
                }
              />
              <Route
                path="/portal/documents"
                element={
                  <ProtectedPage>
                    <PortalDocuments />
                  </ProtectedPage>
                }
              />
              <Route
                path="/portal/appointments"
                element={
                  <ProtectedPage>
                    <PortalAppointments />
                  </ProtectedPage>
                }
              />

              {/* Dental Module */}
              <Route
                path="/dental"
                element={
                  <ProtectedPage>
                    <DentalDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dental/charts"
                element={
                  <ProtectedPage>
                    <DentalCharts />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dental/treatments"
                element={
                  <ProtectedPage>
                    <DentalTreatments />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dental/periodontal"
                element={
                  <ProtectedPage>
                    <PeriodontalExams />
                  </ProtectedPage>
                }
              />

              {/* Cardiology Module */}
              <Route
                path="/cardiology"
                element={
                  <ProtectedPage>
                    <CardiologyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/cardiology/ecg"
                element={
                  <ProtectedPage>
                    <ECGRecords />
                  </ProtectedPage>
                }
              />
              <Route
                path="/cardiology/echo"
                element={
                  <ProtectedPage>
                    <EchoRecords />
                  </ProtectedPage>
                }
              />
              <Route
                path="/cardiology/stress-tests"
                element={
                  <ProtectedPage>
                    <StressTests />
                  </ProtectedPage>
                }
              />
              <Route
                path="/cardiology/cath-lab"
                element={
                  <ProtectedPage>
                    <CathLab />
                  </ProtectedPage>
                }
              />
              <Route
                path="/cardiology/risk-calculator"
                element={
                  <ProtectedPage>
                    <RiskCalculator />
                  </ProtectedPage>
                }
              />

              {/* Ophthalmology Module */}
              <Route
                path="/ophthalmology"
                element={
                  <ProtectedPage>
                    <OphthalmologyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/visual-acuity"
                element={
                  <ProtectedPage>
                    <VisualAcuityTests />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/refraction"
                element={
                  <ProtectedPage>
                    <RefractionTests />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/iop"
                element={
                  <ProtectedPage>
                    <IOPMeasurements />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/slit-lamp"
                element={
                  <ProtectedPage>
                    <SlitLampExams />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/fundus"
                element={
                  <ProtectedPage>
                    <FundusExams />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ophthalmology/prescriptions"
                element={
                  <ProtectedPage>
                    <GlassesPrescriptions />
                  </ProtectedPage>
                }
              />

              {/* Orthopedics Module */}
              <Route
                path="/orthopedics"
                element={
                  <ProtectedPage>
                    <OrthopedicsDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/orthopedics/exams"
                element={
                  <ProtectedPage>
                    <OrthopedicExams />
                  </ProtectedPage>
                }
              />
              <Route
                path="/orthopedics/fractures"
                element={
                  <ProtectedPage>
                    <FractureRecords />
                  </ProtectedPage>
                }
              />
              <Route
                path="/orthopedics/surgeries"
                element={
                  <ProtectedPage>
                    <Surgeries />
                  </ProtectedPage>
                }
              />

              {/* Dermatology Module */}
              <Route
                path="/dermatology"
                element={
                  <ProtectedPage>
                    <DermatologyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dermatology/exams"
                element={
                  <ProtectedPage>
                    <SkinExams />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dermatology/photos"
                element={
                  <ProtectedPage>
                    <SkinPhotos />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dermatology/mole-mapping"
                element={
                  <ProtectedPage>
                    <MoleMappings />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dermatology/biopsies"
                element={
                  <ProtectedPage>
                    <Biopsies />
                  </ProtectedPage>
                }
              />

              {/* Oncology Module */}
              <Route
                path="/oncology"
                element={
                  <ProtectedPage>
                    <OncologyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/oncology/diagnoses"
                element={
                  <ProtectedPage>
                    <Diagnoses />
                  </ProtectedPage>
                }
              />
              <Route
                path="/oncology/chemotherapy"
                element={
                  <ProtectedPage>
                    <ChemotherapySessions />
                  </ProtectedPage>
                }
              />
              <Route
                path="/oncology/treatment-plans"
                element={
                  <ProtectedPage>
                    <TreatmentPlans />
                  </ProtectedPage>
                }
              />

              {/* Neurology Module */}
              <Route
                path="/neurology"
                element={
                  <ProtectedPage>
                    <NeurologyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/neurology/exams"
                element={
                  <ProtectedPage>
                    <NeurologicalExams />
                  </ProtectedPage>
                }
              />
              <Route
                path="/neurology/eeg"
                element={
                  <ProtectedPage>
                    <EEGRecords />
                  </ProtectedPage>
                }
              />
              <Route
                path="/neurology/emg"
                element={
                  <ProtectedPage>
                    <EMGStudies />
                  </ProtectedPage>
                }
              />

              {/* Pediatrics Module */}
              <Route
                path="/pediatrics"
                element={
                  <ProtectedPage>
                    <PediatricsDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/pediatrics/growth"
                element={
                  <ProtectedPage>
                    <GrowthCharts />
                  </ProtectedPage>
                }
              />
              <Route
                path="/pediatrics/vaccinations"
                element={
                  <ProtectedPage>
                    <Vaccinations />
                  </ProtectedPage>
                }
              />
              <Route
                path="/pediatrics/milestones"
                element={
                  <ProtectedPage>
                    <DevelopmentMilestones />
                  </ProtectedPage>
                }
              />

              {/* OB/GYN Module */}
              <Route
                path="/obgyn"
                element={
                  <ProtectedPage>
                    <OBGYNDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/obgyn/pregnancies"
                element={
                  <ProtectedPage>
                    <Pregnancies />
                  </ProtectedPage>
                }
              />
              <Route
                path="/obgyn/prenatal"
                element={
                  <ProtectedPage>
                    <PrenatalVisits />
                  </ProtectedPage>
                }
              />
              <Route
                path="/obgyn/ultrasounds"
                element={
                  <ProtectedPage>
                    <Ultrasounds />
                  </ProtectedPage>
                }
              />

              {/* Physiotherapy Module */}
              <Route
                path="/physiotherapy"
                element={
                  <ProtectedPage>
                    <PhysiotherapyDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/physiotherapy/sessions"
                element={
                  <ProtectedPage>
                    <TherapySessions />
                  </ProtectedPage>
                }
              />
              <Route
                path="/physiotherapy/exercises"
                element={
                  <ProtectedPage>
                    <ExercisePrograms />
                  </ProtectedPage>
                }
              />

              {/* ENT Module */}
              <Route
                path="/ent"
                element={
                  <ProtectedPage>
                    <ENTDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ent/audiometry"
                element={
                  <ProtectedPage>
                    <AudiometryTests />
                  </ProtectedPage>
                }
              />
              <Route
                path="/ent/hearing-aids"
                element={
                  <ProtectedPage>
                    <HearingAids />
                  </ProtectedPage>
                }
              />

              {/* Fertility Module */}
              <Route
                path="/fertility"
                element={
                  <ProtectedPage>
                    <FertilityDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/fertility/ivf-cycles"
                element={
                  <ProtectedPage>
                    <IVFCycles />
                  </ProtectedPage>
                }
              />
              <Route
                path="/fertility/embryos"
                element={
                  <ProtectedPage>
                    <EmbryoRecords />
                  </ProtectedPage>
                }
              />

              {/* Dialysis Module */}
              <Route
                path="/dialysis"
                element={
                  <ProtectedPage>
                    <DialysisDashboard />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dialysis/sessions"
                element={
                  <ProtectedPage>
                    <DialysisSessions />
                  </ProtectedPage>
                }
              />
              <Route
                path="/dialysis/vascular-access"
                element={
                  <ProtectedPage>
                    <VascularAccess />
                  </ProtectedPage>
                }
              />

              {/* Catch all - show 404 page */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </BrowserRouter>
        </ToastProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
