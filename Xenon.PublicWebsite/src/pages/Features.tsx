import { Link } from 'react-router-dom';
import {
  ArrowRight,
  Users,
  Calendar,
  Beaker,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Building2,
  Shield,
  BarChart3,
  Settings,
  Ear,
  Megaphone,
} from 'lucide-react';

const modules = [
  {
    icon: Users,
    name: 'Patient Management',
    description: 'Complete patient records, medical history, documents, and demographics.',
  },
  {
    icon: Calendar,
    name: 'Appointments',
    description: 'Scheduling, calendar views, status tracking, and automated reminders.',
  },
  {
    icon: Beaker,
    name: 'Laboratory',
    description: 'Lab orders, specimen tracking, results entry, and external lab integration.',
  },
  {
    icon: Ear,
    name: 'Audiology',
    description: 'Audiograms, hearing device management, and clinical documentation.',
  },
  {
    icon: Pill,
    name: 'Pharmacy',
    description: 'Prescription management, dispensing, and inventory tracking.',
  },
  {
    icon: DollarSign,
    name: 'Financial',
    description: 'Invoicing, payments, accounts, expenses, and financial reporting.',
  },
  {
    icon: Package,
    name: 'Inventory',
    description: 'Stock management, goods receipt, transactions, and reorder alerts.',
  },
  {
    icon: UserCog,
    name: 'HR Management',
    description: 'Employees, attendance, leave management, and payroll.',
  },
  {
    icon: Megaphone,
    name: 'Marketing',
    description: 'Campaign management, lead tracking, patient outreach, and marketing analytics.',
    link: '/features/marketing',
  },
];

const platformFeatures = [
  {
    icon: Building2,
    name: 'Multi-Branch',
    description: 'Manage multiple locations with branch-specific settings and consolidated reporting.',
  },
  {
    icon: Shield,
    name: 'Enterprise Security',
    description: 'Role-based access control, audit logging, data encryption, and HIPAA compliance.',
  },
  {
    icon: BarChart3,
    name: 'Analytics & Reports',
    description: 'Real-time dashboards, custom reports, and data export capabilities.',
  },
  {
    icon: Settings,
    name: 'Configurable',
    description: 'Custom fields, workflows, terminology, and branding per tenant.',
  },
];

export default function FeaturesPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Features</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Everything you need in one platform
            </h1>
            <p className="text-lg md:text-xl text-gray-600">
              A complete suite of modules designed for healthcare clinics and trading companies,
              with enterprise-grade features at every tier.
            </p>
          </div>
        </div>
      </section>

      {/* Modules */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Core Modules</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Comprehensive modules covering every aspect of your business operations.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
            {modules.map((module) => {
              const CardContent = (
                <>
                  <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                    <module.icon className="h-6 w-6" />
                  </div>
                  <h3 className="font-semibold text-gray-900 mb-2">{module.name}</h3>
                  <p className="text-sm text-gray-600">{module.description}</p>
                  {'link' in module && (
                    <div className="mt-4 text-primary-600 text-sm font-medium flex items-center">
                      Learn more
                      <ArrowRight className="ml-1 h-4 w-4" />
                    </div>
                  )}
                </>
              );

              if ('link' in module) {
                return (
                  <Link key={module.name} to={module.link} className="card card-hover block">
                    {CardContent}
                  </Link>
                );
              }

              return (
                <div key={module.name} className="card card-hover">
                  {CardContent}
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Platform Features */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Platform Features</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Enterprise capabilities that scale with your business.
            </p>
          </div>

          <div className="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
            {platformFeatures.map((feature) => (
              <div key={feature.name} className="card">
                <div className="flex gap-4">
                  <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                    <feature.icon className="h-6 w-6" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900 mb-2">{feature.name}</h3>
                    <p className="text-sm text-gray-600">{feature.description}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">See it in action</h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your free trial and explore all features with your own data.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link to="/pricing" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              View Pricing
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
