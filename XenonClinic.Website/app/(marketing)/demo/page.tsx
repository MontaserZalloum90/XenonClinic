'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  ArrowRight,
  Check,
  Building2,
  Stethoscope,
  ShoppingCart,
  Ear,
  Eye,
  Smile,
  PawPrint,
  Sparkles,
  Scissors,
} from 'lucide-react';

const companyTypes = [
  {
    code: 'CLINIC',
    name: 'Healthcare Clinic',
    description: 'Medical practices, clinics, and healthcare providers',
    icon: Stethoscope,
  },
  {
    code: 'TRADING',
    name: 'Trading Company',
    description: 'Retail, wholesale, and distribution businesses',
    icon: ShoppingCart,
  },
];

const clinicTypes = [
  { code: 'AUDIOLOGY', name: 'Audiology', icon: Ear },
  { code: 'DENTAL', name: 'Dental', icon: Smile },
  { code: 'OPTICAL', name: 'Optical', icon: Eye },
  { code: 'VET', name: 'Veterinary', icon: PawPrint },
  { code: 'DERMATOLOGY', name: 'Dermatology', icon: Sparkles },
  { code: 'GENERAL', name: 'General Practice', icon: Stethoscope },
  { code: 'SALON', name: 'Beauty Salon', icon: Scissors },
];

type Step = 'company-type' | 'clinic-type' | 'details' | 'processing';

export default function DemoPage() {
  const router = useRouter();
  const [step, setStep] = useState<Step>('company-type');
  const [formData, setFormData] = useState({
    companyType: '',
    clinicType: '',
    companyName: '',
    fullName: '',
    email: '',
    phone: '',
    country: 'UAE',
    agreeTerms: false,
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleCompanyTypeSelect = (code: string) => {
    setFormData((prev) => ({ ...prev, companyType: code }));
    if (code === 'CLINIC') {
      setStep('clinic-type');
    } else {
      setStep('details');
    }
  };

  const handleClinicTypeSelect = (code: string) => {
    setFormData((prev) => ({ ...prev, clinicType: code }));
    setStep('details');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setStep('processing');

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 3000));

    // Redirect to success or dashboard
    router.push('/demo/success');
  };

  const canProceed = () => {
    if (step === 'details') {
      return (
        formData.companyName &&
        formData.fullName &&
        formData.email &&
        formData.agreeTerms
      );
    }
    return true;
  };

  return (
    <div className="min-h-[calc(100vh-4rem)] bg-gradient-to-b from-gray-50 to-white">
      <div className="container-marketing py-12 md:py-20">
        <div className="max-w-2xl mx-auto">
          {/* Progress indicator */}
          <div className="mb-8">
            <div className="flex items-center justify-center gap-2 mb-4">
              {['company-type', 'clinic-type', 'details'].map((s, i) => {
                const isActive = s === step;
                const isPast =
                  (step === 'clinic-type' && i === 0) ||
                  (step === 'details' && i <= 1) ||
                  step === 'processing';
                const showStep = s !== 'clinic-type' || formData.companyType === 'CLINIC';

                if (!showStep) return null;

                return (
                  <React.Fragment key={s}>
                    {i > 0 && showStep && (
                      <div className={`w-8 h-0.5 ${isPast ? 'bg-primary-600' : 'bg-gray-200'}`} />
                    )}
                    <div
                      className={`h-8 w-8 rounded-full flex items-center justify-center text-sm font-medium ${
                        isActive
                          ? 'bg-primary-600 text-white'
                          : isPast
                          ? 'bg-primary-100 text-primary-600'
                          : 'bg-gray-100 text-gray-400'
                      }`}
                    >
                      {isPast && !isActive ? <Check className="h-4 w-4" /> : i + 1}
                    </div>
                  </React.Fragment>
                );
              })}
            </div>
          </div>

          {/* Step: Company Type */}
          {step === 'company-type' && (
            <div className="animate-fade-in">
              <div className="text-center mb-8">
                <h1 className="heading-2 text-gray-900 mb-4">
                  Start your free trial
                </h1>
                <p className="text-lg text-gray-600">
                  What type of business are you running?
                </p>
              </div>

              <div className="grid md:grid-cols-2 gap-6">
                {companyTypes.map((type) => (
                  <button
                    key={type.code}
                    onClick={() => handleCompanyTypeSelect(type.code)}
                    className="card card-hover text-left p-8 group"
                  >
                    <div className="h-14 w-14 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4 group-hover:bg-primary-600 group-hover:text-white transition-colors">
                      <type.icon className="h-7 w-7" />
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                      {type.name}
                    </h3>
                    <p className="text-gray-600 text-sm">
                      {type.description}
                    </p>
                  </button>
                ))}
              </div>

              <p className="text-center text-sm text-gray-500 mt-8">
                Already have an account?{' '}
                <Link href="/login" className="link">
                  Sign in
                </Link>
              </p>
            </div>
          )}

          {/* Step: Clinic Type */}
          {step === 'clinic-type' && (
            <div className="animate-fade-in">
              <div className="text-center mb-8">
                <button
                  onClick={() => setStep('company-type')}
                  className="text-sm text-gray-500 hover:text-gray-700 mb-4"
                >
                  &larr; Back
                </button>
                <h1 className="heading-2 text-gray-900 mb-4">
                  What type of clinic?
                </h1>
                <p className="text-lg text-gray-600">
                  Select your specialty to get the right configuration
                </p>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {clinicTypes.map((type) => (
                  <button
                    key={type.code}
                    onClick={() => handleClinicTypeSelect(type.code)}
                    className="card card-hover text-center p-6 group"
                  >
                    <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-3 mx-auto group-hover:bg-primary-600 group-hover:text-white transition-colors">
                      <type.icon className="h-6 w-6" />
                    </div>
                    <span className="font-medium text-gray-900 group-hover:text-primary-600">
                      {type.name}
                    </span>
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Step: Details */}
          {step === 'details' && (
            <div className="animate-fade-in">
              <div className="text-center mb-8">
                <button
                  onClick={() =>
                    setStep(formData.companyType === 'CLINIC' ? 'clinic-type' : 'company-type')
                  }
                  className="text-sm text-gray-500 hover:text-gray-700 mb-4"
                >
                  &larr; Back
                </button>
                <h1 className="heading-2 text-gray-900 mb-4">
                  Almost there!
                </h1>
                <p className="text-lg text-gray-600">
                  Tell us a bit about yourself
                </p>
              </div>

              <form onSubmit={handleSubmit} className="card">
                <div className="space-y-6">
                  <div>
                    <label htmlFor="companyName" className="block text-sm font-medium text-gray-700 mb-1">
                      Company/Clinic Name *
                    </label>
                    <input
                      type="text"
                      id="companyName"
                      name="companyName"
                      required
                      value={formData.companyName}
                      onChange={handleInputChange}
                      className="input"
                      placeholder="Your business name"
                    />
                  </div>

                  <div className="grid md:grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="fullName" className="block text-sm font-medium text-gray-700 mb-1">
                        Your Name *
                      </label>
                      <input
                        type="text"
                        id="fullName"
                        name="fullName"
                        required
                        value={formData.fullName}
                        onChange={handleInputChange}
                        className="input"
                        placeholder="Full name"
                      />
                    </div>
                    <div>
                      <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                        Work Email *
                      </label>
                      <input
                        type="email"
                        id="email"
                        name="email"
                        required
                        value={formData.email}
                        onChange={handleInputChange}
                        className="input"
                        placeholder="you@company.com"
                      />
                    </div>
                  </div>

                  <div className="grid md:grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-1">
                        Phone Number
                      </label>
                      <input
                        type="tel"
                        id="phone"
                        name="phone"
                        value={formData.phone}
                        onChange={handleInputChange}
                        className="input"
                        placeholder="+971 50 123 4567"
                      />
                    </div>
                    <div>
                      <label htmlFor="country" className="block text-sm font-medium text-gray-700 mb-1">
                        Country *
                      </label>
                      <select
                        id="country"
                        name="country"
                        required
                        value={formData.country}
                        onChange={handleInputChange}
                        className="input"
                      >
                        <option value="UAE">United Arab Emirates</option>
                        <option value="SA">Saudi Arabia</option>
                        <option value="QA">Qatar</option>
                        <option value="KW">Kuwait</option>
                        <option value="BH">Bahrain</option>
                        <option value="OM">Oman</option>
                        <option value="OTHER">Other</option>
                      </select>
                    </div>
                  </div>

                  <div className="flex items-start gap-3">
                    <input
                      type="checkbox"
                      id="agreeTerms"
                      name="agreeTerms"
                      checked={formData.agreeTerms}
                      onChange={handleInputChange}
                      className="mt-1"
                      required
                    />
                    <label htmlFor="agreeTerms" className="text-sm text-gray-600">
                      I agree to the{' '}
                      <Link href="/terms" className="link">
                        Terms of Service
                      </Link>{' '}
                      and{' '}
                      <Link href="/privacy" className="link">
                        Privacy Policy
                      </Link>
                    </label>
                  </div>

                  <button
                    type="submit"
                    disabled={!canProceed() || isSubmitting}
                    className="btn-primary w-full btn-lg"
                  >
                    Start Free Trial
                    <ArrowRight className="ml-2 h-5 w-5" />
                  </button>
                </div>

                <div className="mt-6 pt-6 border-t">
                  <div className="flex items-center justify-center gap-6 text-sm text-gray-500">
                    <div className="flex items-center gap-1">
                      <Check className="h-4 w-4 text-green-500" />
                      30-day trial
                    </div>
                    <div className="flex items-center gap-1">
                      <Check className="h-4 w-4 text-green-500" />
                      No credit card
                    </div>
                    <div className="flex items-center gap-1">
                      <Check className="h-4 w-4 text-green-500" />
                      Cancel anytime
                    </div>
                  </div>
                </div>
              </form>

              {/* Google Sign-in alternative */}
              <div className="mt-6 text-center">
                <p className="text-sm text-gray-500 mb-3">Or continue with</p>
                <button className="btn-secondary w-full max-w-sm mx-auto flex items-center justify-center gap-2">
                  <svg className="h-5 w-5" viewBox="0 0 24 24">
                    <path
                      fill="currentColor"
                      d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                    />
                    <path
                      fill="currentColor"
                      d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                    />
                    <path
                      fill="currentColor"
                      d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                    />
                    <path
                      fill="currentColor"
                      d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                    />
                  </svg>
                  Continue with Google
                </button>
              </div>
            </div>
          )}

          {/* Step: Processing */}
          {step === 'processing' && (
            <div className="animate-fade-in text-center py-12">
              <div className="h-16 w-16 rounded-full bg-primary-100 flex items-center justify-center mx-auto mb-6">
                <div className="animate-spin h-8 w-8 border-4 border-primary-600 border-t-transparent rounded-full" />
              </div>
              <h2 className="text-xl font-bold text-gray-900 mb-2">
                Creating your account...
              </h2>
              <p className="text-gray-600">
                This will only take a moment
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
