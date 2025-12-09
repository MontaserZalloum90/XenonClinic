import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute, Roles } from './components/ProtectedRoute';
import { Layout } from './components/layout/Layout';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import { AppointmentsList } from './pages/Appointments/AppointmentsList';
import { PatientsList } from './pages/Patients/PatientsList';
import { LaboratoryList } from './pages/Laboratory/LaboratoryList';
import { HRList } from './pages/HR/HRList';
import { FinancialList } from './pages/Financial/FinancialList';
import { InventoryList } from './pages/Inventory/InventoryList';
import { PharmacyList } from './pages/Pharmacy/PharmacyList';
import { RadiologyList } from './pages/Radiology/RadiologyList';
import { AudiologyList } from './pages/Audiology/AudiologyList';
import { AdminDashboard } from './pages/Admin';
import { NotFound, Forbidden } from './pages/Error';

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

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<Login />} />

            {/* Error Pages */}
            <Route path="/forbidden" element={<Forbidden />} />
            <Route path="/404" element={<NotFound />} />

            {/* Protected Routes with Layout */}
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <Layout>
                    <Dashboard />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/appointments"
              element={
                <ProtectedRoute>
                  <Layout>
                    <AppointmentsList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/patients"
              element={
                <ProtectedRoute>
                  <Layout>
                    <PatientsList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/laboratory"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.DOCTOR, Roles.NURSE, Roles.LAB_TECHNICIAN]}>
                  <Layout>
                    <LaboratoryList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/hr"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.HR_MANAGER]}>
                  <Layout>
                    <HRList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/financial"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.ACCOUNTANT]}>
                  <Layout>
                    <FinancialList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/inventory"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.PHARMACIST, Roles.NURSE]}>
                  <Layout>
                    <InventoryList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/pharmacy"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.DOCTOR, Roles.PHARMACIST]}>
                  <Layout>
                    <PharmacyList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/radiology"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN, Roles.DOCTOR, Roles.RADIOLOGIST]}>
                  <Layout>
                    <RadiologyList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/audiology"
              element={
                <ProtectedRoute>
                  <Layout>
                    <AudiologyList />
                  </Layout>
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin"
              element={
                <ProtectedRoute requiredRoles={[Roles.ADMIN]}>
                  <Layout>
                    <AdminDashboard />
                  </Layout>
                </ProtectedRoute>
              }
            />

            {/* Catch all - show 404 page */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
