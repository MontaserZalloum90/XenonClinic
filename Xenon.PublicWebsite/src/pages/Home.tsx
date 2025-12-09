import { Link } from 'react-router-dom';
import { brand } from '@/lib/brand';
import {
  ArrowRight,
  Building2,
  Users,
  Shield,
  BarChart3,
  Stethoscope,
  ShoppingCart,
  Check,
} from 'lucide-react';

const features = [
  {
    icon: Building2,
    title: 'Multi-Branch',
    description: 'Manage multiple locations from a single dashboard with branch-specific settings.',
  },
  {
    icon: Users,
    title: 'Multi-Tenant',
    description: 'Isolated data for each business with customizable branding and workflows.',
  },
  {
    icon: Shield,
    title: 'Enterprise Security',
    description: 'HIPAA-ready with role-based access, audit logs, and data encryption.',
  },
  {
    icon: BarChart3,
    title: 'Advanced Analytics',
    description: 'Real-time dashboards and reports to drive business decisions.',
  },
];

const industries = [
  {
    icon: Stethoscope,
    name: 'Healthcare Clinics',
    description: 'Audiology, dental, optical, veterinary, and general practice.',
  },
  {
    icon: ShoppingCart,
    name: 'Trading Companies',
    description: 'Inventory, sales, procurement, and financial management.',
  },
];

export default function HomePage() {
  return (
    <>
      {/* Hero Section */}
      <section className="relative py-20 md:py-32 bg-gradient-to-b from-primary-50 to-white overflow-hidden">
        <div className="container-marketing relative z-10">
          <div className="max-w-4xl mx-auto text-center">
            <div className="badge-primary mb-6">Now serving the Gulf region</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Enterprise ERP for
              <span className="text-primary-600"> Healthcare & Business</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
              {brand.tagline}. Manage patients, appointments, inventory, HR, and finances
              in one powerful platform.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link to="/demo" className="btn-primary btn-lg">
                Start Free Trial
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
              <Link to="/pricing" className="btn-secondary btn-lg">
                View Pricing
              </Link>
            </div>
            <p className="mt-4 text-sm text-gray-500">
              30-day free trial. No credit card required.
            </p>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">
              Everything you need to run your business
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              A complete ERP solution designed for Gulf healthcare providers and trading companies.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {features.map((feature) => (
              <div key={feature.title} className="card card-hover text-center">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                  <feature.icon className="h-6 w-6" />
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">{feature.title}</h3>
                <p className="text-sm text-gray-600">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Industries Section */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Built for your industry</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Pre-configured templates and workflows for healthcare clinics and trading companies.
            </p>
          </div>

          <div className="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
            {industries.map((industry) => (
              <div key={industry.name} className="card card-hover">
                <div className="h-14 w-14 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                  <industry.icon className="h-7 w-7" />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">{industry.name}</h3>
                <p className="text-gray-600 mb-4">{industry.description}</p>
                <Link to="/features" className="link inline-flex items-center">
                  Learn more <ArrowRight className="ml-1 h-4 w-4" />
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Pricing Preview */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Simple, transparent pricing</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Choose the plan that fits your business. All plans include a 30-day free trial.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
            {[
              { name: 'Starter', price: 499, branches: 1, users: 5 },
              { name: 'Growth', price: 999, branches: 3, users: 15, recommended: true },
              { name: 'Enterprise', price: 2499, branches: 10, users: 50 },
            ].map((plan) => (
              <div
                key={plan.name}
                className={`card relative ${
                  plan.recommended ? 'border-primary-500 ring-2 ring-primary-500' : ''
                }`}
              >
                {plan.recommended && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <span className="badge bg-primary-600 text-white px-3 py-1">Recommended</span>
                  </div>
                )}
                <div className="text-center">
                  <h3 className="text-xl font-bold text-gray-900 mb-2">{plan.name}</h3>
                  <div className="flex items-baseline justify-center gap-1 mb-4">
                    <span className="text-4xl font-bold text-gray-900">{plan.price}</span>
                    <span className="text-gray-500">AED/mo</span>
                  </div>
                  <ul className="space-y-2 mb-6 text-sm text-gray-600">
                    <li className="flex items-center justify-center gap-2">
                      <Check className="h-4 w-4 text-green-500" />
                      {plan.branches} {plan.branches === 1 ? 'branch' : 'branches'}
                    </li>
                    <li className="flex items-center justify-center gap-2">
                      <Check className="h-4 w-4 text-green-500" />
                      {plan.users} users
                    </li>
                    <li className="flex items-center justify-center gap-2">
                      <Check className="h-4 w-4 text-green-500" />
                      All features
                    </li>
                  </ul>
                  <Link
                    to="/demo"
                    className={`w-full ${plan.recommended ? 'btn-primary' : 'btn-secondary'}`}
                  >
                    Start Free Trial
                  </Link>
                </div>
              </div>
            ))}
          </div>

          <div className="text-center mt-8">
            <Link to="/pricing" className="link">
              View full pricing details <ArrowRight className="inline ml-1 h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">Ready to get started?</h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Join hundreds of Gulf businesses already using {brand.name}. Start your free trial today.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link to="/contact" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              Contact Sales
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
