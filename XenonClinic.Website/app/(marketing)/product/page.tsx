import type { Metadata } from 'next';
import Link from 'next/link';
import {
  ArrowRight,
  Check,
  Users,
  Calendar,
  Receipt,
  Package,
  BarChart3,
  Building2,
  Shield,
  Settings,
  Layers,
  Smartphone,
  Cloud,
  Lock,
  RefreshCw,
  Headphones,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Product',
  description: 'Discover XENON - the configurable ERP and Clinic CRM platform designed for Gulf SMBs. Multi-tenant, multi-branch, and fully customizable.',
};

const capabilities = [
  {
    title: 'Multi-Tenant Architecture',
    description: 'Each organization operates in complete isolation with their own data, configurations, and customizations.',
    icon: Layers,
  },
  {
    title: 'Configuration-Driven',
    description: 'Customize terminology, forms, workflows, and UI without writing code. Adapt to any clinic type or trading business.',
    icon: Settings,
  },
  {
    title: 'Multi-Branch Support',
    description: 'Manage unlimited locations with centralized control. Each branch can have its own settings, staff, and inventory.',
    icon: Building2,
  },
  {
    title: 'Role-Based Access',
    description: 'Fine-grained permissions system. Control exactly what each team member can see and do.',
    icon: Lock,
  },
  {
    title: 'Cloud or On-Premises',
    description: 'Deploy in our secure cloud or on your own infrastructure. UAE data residency available.',
    icon: Cloud,
  },
  {
    title: 'Mobile Ready',
    description: 'Responsive design works on any device. Access your business from anywhere.',
    icon: Smartphone,
  },
];

const modules = [
  {
    name: 'Patient/Customer Management',
    description: 'Complete records with medical history, demographics, communications, and document storage.',
    icon: Users,
    features: ['Patient/Customer profiles', 'Medical history', 'Document storage', 'Communication log', 'Custom fields'],
  },
  {
    name: 'Appointment Scheduling',
    description: 'Smart scheduling with resource management, automated reminders, and calendar integration.',
    icon: Calendar,
    features: ['Resource scheduling', 'SMS/Email reminders', 'Calendar sync', 'Recurring appointments', 'Waitlist management'],
  },
  {
    name: 'Billing & Invoicing',
    description: 'Generate invoices, process payments, manage insurance claims, and track outstanding balances.',
    icon: Receipt,
    features: ['Invoice generation', 'Payment processing', 'Insurance claims', 'Payment plans', 'Tax reporting'],
  },
  {
    name: 'Inventory Management',
    description: 'Track stock levels, manage suppliers, automate reordering, and monitor expiration dates.',
    icon: Package,
    features: ['Stock tracking', 'Auto-reorder', 'Supplier management', 'Expiry alerts', 'Multi-location'],
  },
  {
    name: 'Analytics & Reporting',
    description: 'Customizable dashboards, KPI tracking, and exportable reports for data-driven decisions.',
    icon: BarChart3,
    features: ['Custom dashboards', 'KPI tracking', 'Trend analysis', 'Export to Excel', 'Scheduled reports'],
  },
  {
    name: 'Security & Compliance',
    description: 'Enterprise-grade security with audit logs, encryption, and compliance features.',
    icon: Shield,
    features: ['Audit logging', 'Data encryption', 'Access controls', 'HIPAA-aligned', 'Data backup'],
  },
];

const integrations = [
  { name: 'Stripe', category: 'Payments' },
  { name: 'Network International', category: 'Payments' },
  { name: 'PayTabs', category: 'Payments' },
  { name: 'Twilio', category: 'Communications' },
  { name: 'WhatsApp Business', category: 'Communications' },
  { name: 'Google Calendar', category: 'Productivity' },
  { name: 'Microsoft 365', category: 'Productivity' },
  { name: 'QuickBooks', category: 'Accounting' },
  { name: 'Xero', category: 'Accounting' },
];

export default function ProductPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Platform Overview</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              One platform for all your
              <span className="text-primary-600 block">business operations</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              XENON is a modern, configurable ERP and CRM platform that adapts to your business.
              Whether you run a clinic, trading company, or multi-branch operation.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/demo" className="btn-primary btn-lg">
                Start Free Trial
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
              <Link href="/pricing" className="btn-secondary btn-lg">
                View Pricing
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Platform Capabilities */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Built for modern businesses
            </h2>
            <p className="text-lg text-gray-600">
              XENON's architecture is designed to scale with your business while remaining simple to use.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {capabilities.map((capability) => (
              <div key={capability.title} className="flex gap-4">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                  <capability.icon className="h-6 w-6" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900 mb-2">
                    {capability.title}
                  </h3>
                  <p className="text-gray-600 text-sm">
                    {capability.description}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Modules */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Core modules
            </h2>
            <p className="text-lg text-gray-600">
              Everything you need to manage patients, appointments, billing, inventory, and more.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {modules.map((module) => (
              <div key={module.name} className="card">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                  <module.icon className="h-6 w-6" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {module.name}
                </h3>
                <p className="text-gray-600 text-sm mb-4">
                  {module.description}
                </p>
                <ul className="space-y-2">
                  {module.features.map((feature) => (
                    <li key={feature} className="flex items-center gap-2 text-sm text-gray-600">
                      <Check className="h-4 w-4 text-secondary-500 flex-shrink-0" />
                      {feature}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Configuration */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
            <div>
              <div className="badge-primary mb-4">Configuration</div>
              <h2 className="heading-2 text-gray-900 mb-6">
                Adapts to your business, not the other way around
              </h2>
              <p className="text-lg text-gray-600 mb-6">
                XENON's configuration-driven architecture means you can customize the platform
                to match your exact needs without writing a single line of code.
              </p>
              <ul className="space-y-4">
                <li className="flex items-start gap-3">
                  <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                  <div>
                    <span className="font-medium text-gray-900">Dynamic Terminology</span>
                    <p className="text-sm text-gray-600">
                      "Patient" becomes "Customer" for trading companies. Labels adapt to your business type.
                    </p>
                  </div>
                </li>
                <li className="flex items-start gap-3">
                  <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                  <div>
                    <span className="font-medium text-gray-900">Custom Forms</span>
                    <p className="text-sm text-gray-600">
                      Add custom fields, rearrange layouts, and configure validation without development.
                    </p>
                  </div>
                </li>
                <li className="flex items-start gap-3">
                  <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                  <div>
                    <span className="font-medium text-gray-900">Feature Flags</span>
                    <p className="text-sm text-gray-600">
                      Enable only the modules you need. Hide features that don't apply to your business.
                    </p>
                  </div>
                </li>
                <li className="flex items-start gap-3">
                  <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                  <div>
                    <span className="font-medium text-gray-900">Workflow Automation</span>
                    <p className="text-sm text-gray-600">
                      Set up automated reminders, status transitions, and notification rules.
                    </p>
                  </div>
                </li>
              </ul>
            </div>
            <div className="card bg-gray-50 border-gray-200">
              <div className="text-sm font-mono text-gray-500 mb-4">Terminology Configuration</div>
              <div className="space-y-3 text-sm">
                <div className="flex justify-between p-3 bg-white rounded-lg border">
                  <span className="text-gray-600">patient.singular</span>
                  <span className="font-medium text-gray-900">"Patient"</span>
                </div>
                <div className="flex justify-between p-3 bg-white rounded-lg border">
                  <span className="text-gray-600">patient.plural</span>
                  <span className="font-medium text-gray-900">"Patients"</span>
                </div>
                <div className="flex justify-between p-3 bg-white rounded-lg border">
                  <span className="text-gray-600">appointment.singular</span>
                  <span className="font-medium text-gray-900">"Appointment"</span>
                </div>
                <div className="flex justify-between p-3 bg-white rounded-lg border">
                  <span className="text-gray-600">nav.dashboard</span>
                  <span className="font-medium text-gray-900">"Dashboard"</span>
                </div>
              </div>
              <div className="mt-4 pt-4 border-t text-xs text-gray-500">
                Trading companies see "Customer" instead of "Patient"
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Integrations */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">
              Connect with your favorite tools
            </h2>
            <p className="text-lg text-gray-600">
              XENON integrates with leading payment gateways, communication platforms, and business tools.
            </p>
          </div>

          <div className="flex flex-wrap justify-center gap-4">
            {integrations.map((integration) => (
              <div
                key={integration.name}
                className="px-6 py-3 rounded-xl bg-white border border-gray-200 text-sm"
              >
                <span className="font-medium text-gray-900">{integration.name}</span>
                <span className="text-gray-400 ml-2">/ {integration.category}</span>
              </div>
            ))}
          </div>

          <div className="text-center mt-8">
            <Link href="/docs/integrations" className="link">
              View all integrations
            </Link>
          </div>
        </div>
      </section>

      {/* Support */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-3 gap-8">
            <div className="card text-center">
              <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                <Headphones className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                24/7 Support
              </h3>
              <p className="text-gray-600 text-sm">
                Our support team is available around the clock to help you succeed.
              </p>
            </div>
            <div className="card text-center">
              <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                <RefreshCw className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Regular Updates
              </h3>
              <p className="text-gray-600 text-sm">
                New features and improvements released every month with zero downtime.
              </p>
            </div>
            <div className="card text-center">
              <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                <Users className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Onboarding Team
              </h3>
              <p className="text-gray-600 text-sm">
                Dedicated specialists to help you set up and migrate your data.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Ready to get started?
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your 30-day free trial today. No credit card required.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/contact" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              Contact Sales
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
