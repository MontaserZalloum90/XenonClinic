import { Routes, Route } from 'react-router-dom';
import { MarketingLayout } from './components/layouts/MarketingLayout';
import { AuthLayout } from './components/layouts/AuthLayout';
import { ToastProvider } from './components/ui/Toast';

// Marketing Pages
import HomePage from './pages/Home';
import AboutPage from './pages/About';
import FeaturesPage from './pages/Features';
import MarketingModulePage from './pages/Marketing';
import ModuleJourneysPage from './pages/ModuleJourneys';
import PricingPage from './pages/Pricing';
import ContactPage from './pages/Contact';
import DemoPage from './pages/Demo';
import DemoSuccessPage from './pages/DemoSuccess';

// Auth Pages
import LoginPage from './pages/Login';
import SignupPage from './pages/Signup';

// Documentation Pages
import DocsLayout from './components/docs/DocsLayout';
import DocsHome from './pages/docs/DocsHome';
import GettingStarted from './pages/docs/GettingStarted';
import ModulesIndex from './pages/docs/ModulesIndex';
import ModulePage from './pages/docs/ModulePage';
import PersonasIndex from './pages/docs/PersonasIndex';
import PersonaPage from './pages/docs/PersonaPage';
import JourneysIndex from './pages/docs/JourneysIndex';
import JourneyPage from './pages/docs/JourneyPage';
import SecurityRBAC from './pages/docs/SecurityRBAC';
import APIReference from './pages/docs/APIReference';
import AdminConfiguration from './pages/docs/AdminConfiguration';
import FAQTroubleshooting from './pages/docs/FAQTroubleshooting';
import ReleaseNotes from './pages/docs/ReleaseNotes';
import Glossary from './pages/docs/Glossary';

// Error Pages
import NotFoundPage from './pages/NotFound';

export default function App() {
  return (
    <ToastProvider>
      <Routes>
      {/* Marketing Routes */}
      <Route element={<MarketingLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/features" element={<FeaturesPage />} />
        <Route path="/features/marketing" element={<MarketingModulePage />} />
        <Route path="/features/journeys" element={<ModuleJourneysPage />} />
        <Route path="/pricing" element={<PricingPage />} />
        <Route path="/contact" element={<ContactPage />} />
        <Route path="/demo" element={<DemoPage />} />
        <Route path="/demo/success" element={<DemoSuccessPage />} />
      </Route>

      {/* Auth Routes */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/signup" element={<SignupPage />} />
      </Route>

      {/* Documentation Routes */}
      <Route path="/docs" element={<DocsLayout />}>
        <Route index element={<DocsHome />} />
        <Route path="getting-started" element={<GettingStarted />} />
        <Route path="modules" element={<ModulesIndex />} />
        <Route path="modules/:moduleId" element={<ModulePage />} />
        <Route path="personas" element={<PersonasIndex />} />
        <Route path="personas/:personaId" element={<PersonaPage />} />
        <Route path="journeys" element={<JourneysIndex />} />
        <Route path="journeys/:journeyId" element={<JourneyPage />} />
        <Route path="security-rbac" element={<SecurityRBAC />} />
        <Route path="api-reference" element={<APIReference />} />
        <Route path="admin-configuration" element={<AdminConfiguration />} />
        <Route path="faq-troubleshooting" element={<FAQTroubleshooting />} />
        <Route path="release-notes" element={<ReleaseNotes />} />
        <Route path="glossary" element={<Glossary />} />
      </Route>

      {/* 404 */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
    </ToastProvider>
  );
}
