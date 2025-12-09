import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowRight, Check, Stethoscope, ShoppingCart, AlertCircle } from 'lucide-react';
import { submitDemoRequest } from '@/lib/platform-api';

const companyTypes = [
  { code: 'CLINIC', name: 'Healthcare Clinic', icon: Stethoscope },
  { code: 'TRADING', name: 'Trading Company', icon: ShoppingCart },
];

const clinicTypes = [
  { code: 'AUDIOLOGY', name: 'Audiology' },
  { code: 'DENTAL', name: 'Dental' },
  { code: 'OPTICAL', name: 'Optical' },
  { code: 'VET', name: 'Veterinary' },
  { code: 'GENERAL', name: 'General Practice' },
];

type Step = 'company-type' | 'clinic-type' | 'details' | 'processing';

export default function DemoPage() {
  const navigate = useNavigate();
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
  const [error, setError] = useState('');

  const handleCompanyTypeSelect = (code: string) => {
    setFormData((prev) => ({ ...prev, companyType: code }));
    setStep(code === 'CLINIC' ? 'clinic-type' : 'details');
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
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setStep('processing');
    setError('');

    try {
      const response = await submitDemoRequest({
        name: formData.fullName,
        email: formData.email,
        phone: formData.phone || undefined,
        company: formData.companyName,
        companyType: formData.companyType,
        clinicType: formData.companyType === 'CLINIC' ? formData.clinicType : undefined,
        inquiryType: 'demo',
        source: 'demo-page',
      });

      if (response.success) {
        navigate('/demo/success');
      } else {
        setError(response.error || 'Failed to submit. Please try again.');
        setStep('details');
      }
    } catch {
      setError('Network error. Please try again.');
      setStep('details');
    }
  };

  const canProceed = formData.companyName && formData.fullName && formData.email && formData.agreeTerms;

  return (
    <div className="min-h-[calc(100vh-4rem)] bg-gradient-to-b from-gray-50 to-white">
      <div className="container-marketing py-12 md:py-20">
        <div className="max-w-2xl mx-auto">
          {/* Step: Company Type */}
          {step === 'company-type' && (
            <div className="animate-fade-in">
              <div className="text-center mb-8">
                <h1 className="heading-2 text-gray-900 mb-4">Start your free trial</h1>
                <p className="text-lg text-gray-600">What type of business are you running?</p>
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
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">{type.name}</h3>
                  </button>
                ))}
              </div>
              <p className="text-center text-sm text-gray-500 mt-8">
                Already have an account? <Link to="/login" className="link">Sign in</Link>
              </p>
            </div>
          )}

          {/* Step: Clinic Type */}
          {step === 'clinic-type' && (
            <div className="animate-fade-in">
              <div className="text-center mb-8">
                <button onClick={() => setStep('company-type')} className="text-sm text-gray-500 hover:text-gray-700 mb-4">
                  &larr; Back
                </button>
                <h1 className="heading-2 text-gray-900 mb-4">What type of clinic?</h1>
              </div>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {clinicTypes.map((type) => (
                  <button
                    key={type.code}
                    onClick={() => handleClinicTypeSelect(type.code)}
                    className="card card-hover text-center p-6"
                  >
                    <span className="font-medium text-gray-900">{type.name}</span>
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
                  onClick={() => setStep(formData.companyType === 'CLINIC' ? 'clinic-type' : 'company-type')}
                  className="text-sm text-gray-500 hover:text-gray-700 mb-4"
                >
                  &larr; Back
                </button>
                <h1 className="heading-2 text-gray-900 mb-4">Almost there!</h1>
              </div>
              <form onSubmit={handleSubmit} className="card">
                <div className="space-y-6">
                  {error && (
                    <div className="p-3 rounded-lg bg-red-50 border border-red-200 flex items-center gap-2 text-sm text-red-700">
                      <AlertCircle className="h-4 w-4" />
                      {error}
                    </div>
                  )}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Company Name *</label>
                    <input type="text" name="companyName" required value={formData.companyName} onChange={handleInputChange} className="input" />
                  </div>
                  <div className="grid md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Your Name *</label>
                      <input type="text" name="fullName" required value={formData.fullName} onChange={handleInputChange} className="input" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
                      <input type="email" name="email" required value={formData.email} onChange={handleInputChange} className="input" />
                    </div>
                  </div>
                  <div className="grid md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                      <input type="tel" name="phone" value={formData.phone} onChange={handleInputChange} className="input" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Country</label>
                      <select name="country" value={formData.country} onChange={handleInputChange} className="input">
                        <option value="UAE">United Arab Emirates</option>
                        <option value="SA">Saudi Arabia</option>
                        <option value="QA">Qatar</option>
                        <option value="KW">Kuwait</option>
                        <option value="BH">Bahrain</option>
                        <option value="OM">Oman</option>
                      </select>
                    </div>
                  </div>
                  <label className="flex items-start gap-3">
                    <input type="checkbox" name="agreeTerms" checked={formData.agreeTerms} onChange={handleInputChange} className="mt-1" required />
                    <span className="text-sm text-gray-600">
                      I agree to the <Link to="/terms" className="link">Terms</Link> and <Link to="/privacy" className="link">Privacy Policy</Link>
                    </span>
                  </label>
                  <button type="submit" disabled={!canProceed} className="btn-primary w-full btn-lg">
                    Start Free Trial <ArrowRight className="ml-2 h-5 w-5" />
                  </button>
                </div>
                <div className="mt-6 pt-6 border-t flex items-center justify-center gap-6 text-sm text-gray-500">
                  <div className="flex items-center gap-1"><Check className="h-4 w-4 text-green-500" />30-day trial</div>
                  <div className="flex items-center gap-1"><Check className="h-4 w-4 text-green-500" />No credit card</div>
                </div>
              </form>
            </div>
          )}

          {/* Step: Processing */}
          {step === 'processing' && (
            <div className="animate-fade-in text-center py-12">
              <div className="h-16 w-16 rounded-full bg-primary-100 flex items-center justify-center mx-auto mb-6">
                <div className="animate-spin h-8 w-8 border-4 border-primary-600 border-t-transparent rounded-full" />
              </div>
              <h2 className="text-xl font-bold text-gray-900 mb-2">Creating your account...</h2>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
