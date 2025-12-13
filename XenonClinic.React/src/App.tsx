import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Suspense, lazy } from "react";
import { AuthProvider } from "./contexts/AuthContext";
import { ProtectedRoute, Roles } from "./components/ProtectedRoute";
import { Layout } from "./components/layout/Layout";
import { ToastProvider } from "./components/ui/Toast";

// Loading component for Suspense fallback
const PageLoader = () => (
  <div className="flex items-center justify-center min-h-screen">
    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
);

// Core Pages - loaded eagerly (small, frequently used)
import { Login } from "./pages/Login";
import { Dashboard } from "./pages/Dashboard";
import { NotFound, Forbidden } from "./pages/Error";

// Lazy-loaded modules - split by feature area
// Core Modules
const AppointmentsList = lazy(() =>
  import("./pages/Appointments/AppointmentsList").then((m) => ({
    default: m.AppointmentsList,
  })),
);
const PatientsList = lazy(() =>
  import("./pages/Patients/PatientsList").then((m) => ({
    default: m.PatientsList,
  })),
);
const LaboratoryList = lazy(() =>
  import("./pages/Laboratory/LaboratoryList").then((m) => ({
    default: m.LaboratoryList,
  })),
);
const FinancialList = lazy(() =>
  import("./pages/Financial/FinancialList").then((m) => ({
    default: m.FinancialList,
  })),
);
const InventoryList = lazy(() =>
  import("./pages/Inventory/InventoryList").then((m) => ({
    default: m.InventoryList,
  })),
);
const PharmacyList = lazy(() =>
  import("./pages/Pharmacy/PharmacyList").then((m) => ({
    default: m.PharmacyList,
  })),
);
const RadiologyList = lazy(() =>
  import("./pages/Radiology/RadiologyList").then((m) => ({
    default: m.RadiologyList,
  })),
);
const AudiologyList = lazy(() =>
  import("./pages/Audiology/AudiologyList").then((m) => ({
    default: m.AudiologyList,
  })),
);
const MarketingList = lazy(() =>
  import("./pages/Marketing/MarketingList").then((m) => ({
    default: m.MarketingList,
  })),
);
const ClinicalVisitsList = lazy(() =>
  import("./pages/ClinicalVisits").then((m) => ({
    default: m.ClinicalVisitsList,
  })),
);

// HR Module
const HRList = lazy(() =>
  import("./pages/HR/HRList").then((m) => ({ default: m.HRList })),
);
const EmployeesList = lazy(() =>
  import("./pages/HR").then((m) => ({ default: m.EmployeesList })),
);
const PayrollList = lazy(() =>
  import("./pages/HR").then((m) => ({ default: m.PayrollList })),
);
const SalaryStructures = lazy(() =>
  import("./pages/HR").then((m) => ({ default: m.SalaryStructures })),
);

// Admin Module
const AdminDashboard = lazy(() =>
  import("./pages/Admin").then((m) => ({ default: m.AdminDashboard })),
);
const TranslationManagement = lazy(() =>
  import("./pages/Admin").then((m) => ({ default: m.TranslationManagement })),
);

// Analytics Module
const AnalyticsDashboard = lazy(() =>
  import("./pages/Analytics").then((m) => ({ default: m.AnalyticsDashboard })),
);
const ReportsList = lazy(() =>
  import("./pages/Analytics").then((m) => ({ default: m.ReportsList })),
);

// Workflow Module
const WorkflowDefinitions = lazy(() =>
  import("./pages/Workflow").then((m) => ({ default: m.WorkflowDefinitions })),
);
const WorkflowInstances = lazy(() =>
  import("./pages/Workflow").then((m) => ({ default: m.WorkflowInstances })),
);

// Portal Module
const PortalRegistration = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalRegistration })),
);
const PortalLogin = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalLogin })),
);
const PortalDashboard = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalDashboard })),
);
const PortalProfile = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalProfile })),
);
const PortalDocuments = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalDocuments })),
);
const PortalAppointments = lazy(() =>
  import("./pages/Portal").then((m) => ({ default: m.PortalAppointments })),
);

// Dental Module
const DentalDashboard = lazy(() =>
  import("./pages/Dental").then((m) => ({ default: m.DentalDashboard })),
);
const DentalCharts = lazy(() =>
  import("./pages/Dental").then((m) => ({ default: m.DentalCharts })),
);
const DentalTreatments = lazy(() =>
  import("./pages/Dental").then((m) => ({ default: m.DentalTreatments })),
);
const PeriodontalExams = lazy(() =>
  import("./pages/Dental").then((m) => ({ default: m.PeriodontalExams })),
);

// Cardiology Module
const CardiologyDashboard = lazy(() =>
  import("./pages/Cardiology").then((m) => ({
    default: m.CardiologyDashboard,
  })),
);
const ECGRecords = lazy(() =>
  import("./pages/Cardiology").then((m) => ({ default: m.ECGRecords })),
);
const EchoRecords = lazy(() =>
  import("./pages/Cardiology").then((m) => ({ default: m.EchoRecords })),
);
const StressTests = lazy(() =>
  import("./pages/Cardiology").then((m) => ({ default: m.StressTests })),
);
const CathLab = lazy(() =>
  import("./pages/Cardiology").then((m) => ({ default: m.CathLab })),
);
const RiskCalculator = lazy(() =>
  import("./pages/Cardiology").then((m) => ({ default: m.RiskCalculator })),
);

// Ophthalmology Module
const OphthalmologyDashboard = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({
    default: m.OphthalmologyDashboard,
  })),
);
const VisualAcuityTests = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({
    default: m.VisualAcuityTests,
  })),
);
const RefractionTests = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({ default: m.RefractionTests })),
);
const IOPMeasurements = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({ default: m.IOPMeasurements })),
);
const SlitLampExams = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({ default: m.SlitLampExams })),
);
const FundusExams = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({ default: m.FundusExams })),
);
const GlassesPrescriptions = lazy(() =>
  import("./pages/Ophthalmology").then((m) => ({
    default: m.GlassesPrescriptions,
  })),
);

// Orthopedics Module
const OrthopedicsDashboard = lazy(() =>
  import("./pages/Orthopedics").then((m) => ({
    default: m.OrthopedicsDashboard,
  })),
);
const OrthopedicExams = lazy(() =>
  import("./pages/Orthopedics").then((m) => ({ default: m.OrthopedicExams })),
);
const FractureRecords = lazy(() =>
  import("./pages/Orthopedics").then((m) => ({ default: m.FractureRecords })),
);
const Surgeries = lazy(() =>
  import("./pages/Orthopedics").then((m) => ({ default: m.Surgeries })),
);

// Dermatology Module
const DermatologyDashboard = lazy(() =>
  import("./pages/Dermatology").then((m) => ({
    default: m.DermatologyDashboard,
  })),
);
const SkinExams = lazy(() =>
  import("./pages/Dermatology").then((m) => ({ default: m.SkinExams })),
);
const SkinPhotos = lazy(() =>
  import("./pages/Dermatology").then((m) => ({ default: m.SkinPhotos })),
);
const MoleMappings = lazy(() =>
  import("./pages/Dermatology").then((m) => ({ default: m.MoleMappings })),
);
const Biopsies = lazy(() =>
  import("./pages/Dermatology").then((m) => ({ default: m.Biopsies })),
);

// Oncology Module
const OncologyDashboard = lazy(() =>
  import("./pages/Oncology").then((m) => ({ default: m.OncologyDashboard })),
);
const Diagnoses = lazy(() =>
  import("./pages/Oncology").then((m) => ({ default: m.Diagnoses })),
);
const ChemotherapySessions = lazy(() =>
  import("./pages/Oncology").then((m) => ({ default: m.ChemotherapySessions })),
);
const TreatmentPlans = lazy(() =>
  import("./pages/Oncology").then((m) => ({ default: m.TreatmentPlans })),
);

// Neurology Module
const NeurologyDashboard = lazy(() =>
  import("./pages/Neurology").then((m) => ({ default: m.NeurologyDashboard })),
);
const NeurologicalExams = lazy(() =>
  import("./pages/Neurology").then((m) => ({ default: m.NeurologicalExams })),
);
const EEGRecords = lazy(() =>
  import("./pages/Neurology").then((m) => ({ default: m.EEGRecords })),
);
const EMGStudies = lazy(() =>
  import("./pages/Neurology").then((m) => ({ default: m.EMGStudies })),
);

// Pediatrics Module
const PediatricsDashboard = lazy(() =>
  import("./pages/Pediatrics").then((m) => ({
    default: m.PediatricsDashboard,
  })),
);
const GrowthCharts = lazy(() =>
  import("./pages/Pediatrics").then((m) => ({ default: m.GrowthCharts })),
);
const Vaccinations = lazy(() =>
  import("./pages/Pediatrics").then((m) => ({ default: m.Vaccinations })),
);
const DevelopmentMilestones = lazy(() =>
  import("./pages/Pediatrics").then((m) => ({
    default: m.DevelopmentMilestones,
  })),
);

// OB/GYN Module
const OBGYNDashboard = lazy(() =>
  import("./pages/OBGYN").then((m) => ({ default: m.OBGYNDashboard })),
);
const Pregnancies = lazy(() =>
  import("./pages/OBGYN").then((m) => ({ default: m.Pregnancies })),
);
const PrenatalVisits = lazy(() =>
  import("./pages/OBGYN").then((m) => ({ default: m.PrenatalVisits })),
);
const Ultrasounds = lazy(() =>
  import("./pages/OBGYN").then((m) => ({ default: m.Ultrasounds })),
);

// Physiotherapy Module
const PhysiotherapyDashboard = lazy(() =>
  import("./pages/Physiotherapy").then((m) => ({
    default: m.PhysiotherapyDashboard,
  })),
);
const TherapySessions = lazy(() =>
  import("./pages/Physiotherapy").then((m) => ({ default: m.TherapySessions })),
);
const ExercisePrograms = lazy(() =>
  import("./pages/Physiotherapy").then((m) => ({
    default: m.ExercisePrograms,
  })),
);

// ENT Module
const ENTDashboard = lazy(() =>
  import("./pages/ENT").then((m) => ({ default: m.ENTDashboard })),
);
const AudiometryTests = lazy(() =>
  import("./pages/ENT").then((m) => ({ default: m.AudiometryTests })),
);
const HearingAids = lazy(() =>
  import("./pages/ENT").then((m) => ({ default: m.HearingAids })),
);

// Fertility Module
const FertilityDashboard = lazy(() =>
  import("./pages/Fertility").then((m) => ({ default: m.FertilityDashboard })),
);
const IVFCycles = lazy(() =>
  import("./pages/Fertility").then((m) => ({ default: m.IVFCycles })),
);
const EmbryoRecords = lazy(() =>
  import("./pages/Fertility").then((m) => ({ default: m.EmbryoRecords })),
);

// Dialysis Module
const DialysisDashboard = lazy(() =>
  import("./pages/Dialysis").then((m) => ({ default: m.DialysisDashboard })),
);
const DialysisSessions = lazy(() =>
  import("./pages/Dialysis").then((m) => ({ default: m.DialysisSessions })),
);
const VascularAccess = lazy(() =>
  import("./pages/Dialysis").then((m) => ({ default: m.VascularAccess })),
);

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
    <Layout>
      <Suspense fallback={<PageLoader />}>{children}</Suspense>
    </Layout>
  </ProtectedRoute>
);

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ToastProvider>
          <BrowserRouter>
            <Suspense fallback={<PageLoader />}>
              <Routes>
                {/* Public Routes */}
                <Route path="/login" element={<Login />} />

                {/* Portal Public Routes */}
                <Route
                  path="/portal/register"
                  element={<PortalRegistration />}
                />
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
                    <ProtectedPage
                      roles={[Roles.ADMIN, Roles.MARKETING_MANAGER]}
                    >
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
            </Suspense>
          </BrowserRouter>
        </ToastProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
