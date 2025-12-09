import { Routes, Route } from 'react-router-dom';
import { MarketingLayout } from './components/layouts/MarketingLayout';
import { AuthLayout } from './components/layouts/AuthLayout';
import { ToastProvider } from './components/ui/Toast';

// Marketing Pages
import HomePage from './pages/Home';
import AboutPage from './pages/About';
import FeaturesPage from './pages/Features';
import PricingPage from './pages/Pricing';
import ContactPage from './pages/Contact';
import DemoPage from './pages/Demo';
import DemoSuccessPage from './pages/DemoSuccess';

// Auth Pages
import LoginPage from './pages/Login';
import SignupPage from './pages/Signup';

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

      {/* 404 */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
    </ToastProvider>
  );
}
