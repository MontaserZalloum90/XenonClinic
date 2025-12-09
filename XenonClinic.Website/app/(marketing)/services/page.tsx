import type { Metadata } from 'next';
import Link from 'next/link';
import {
  ArrowRight,
  Check,
  Rocket,
  Users,
  Settings,
  Headphones,
  BookOpen,
  RefreshCw,
  Shield,
  Globe,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Services',
  description: 'Professional services from XENON - implementation, training, data migration, custom development, and ongoing support.',
};

const services = [
  {
    icon: Rocket,
    title: 'Implementation & Setup',
    description: 'Our team will configure XENON to match your business processes and workflows.',
    features: [
      'Business process analysis',
      'System configuration',
      'Custom field setup',
      'Workflow automation',
      'Integration configuration',
      'Go-live support',
    ],
    pricing: 'From 2,500 AED',
  },
  {
    icon: RefreshCw,
    title: 'Data Migration',
    description: 'Seamlessly migrate your existing data from spreadsheets or other systems.',
    features: [
      'Data assessment',
      'Field mapping',
      'Data cleansing',
      'Validation & testing',
      'Parallel running support',
      'Historical data import',
    ],
    pricing: 'From 1,500 AED',
  },
  {
    icon: BookOpen,
    title: 'Training & Onboarding',
    description: 'Comprehensive training to ensure your team gets the most out of XENON.',
    features: [
      'Admin training',
      'End-user training',
      'Video tutorials',
      'Quick reference guides',
      'Train-the-trainer sessions',
      'Refresher courses',
    ],
    pricing: 'From 1,000 AED',
  },
  {
    icon: Settings,
    title: 'Custom Development',
    description: 'Need something special? Our team can build custom features and integrations.',
    features: [
      'Custom modules',
      'API integrations',
      'Report development',
      'Workflow customization',
      'Third-party connectors',
      'Mobile app extensions',
    ],
    pricing: 'Quote on request',
  },
  {
    icon: Headphones,
    title: 'Managed Support',
    description: 'Dedicated support packages for businesses that need extra assistance.',
    features: [
      'Priority response',
      'Dedicated account manager',
      'Proactive monitoring',
      'Quarterly reviews',
      'Direct phone support',
      'On-site visits available',
    ],
    pricing: 'From 500 AED/month',
  },
  {
    icon: Shield,
    title: 'Compliance Consulting',
    description: 'Expert guidance on healthcare regulations and data protection requirements.',
    features: [
      'Compliance assessment',
      'Policy development',
      'Audit preparation',
      'Staff training',
      'Documentation review',
      'Ongoing compliance monitoring',
    ],
    pricing: 'Quote on request',
  },
];

const packages = [
  {
    name: 'Quick Start',
    description: 'For small teams ready to go',
    price: '3,500 AED',
    features: [
      'Basic configuration',
      'Up to 500 records migrated',
      '2-hour admin training',
      '30-day email support',
    ],
    cta: 'Get Started',
    popular: false,
  },
  {
    name: 'Professional',
    description: 'Full implementation for growing businesses',
    price: '8,500 AED',
    features: [
      'Complete configuration',
      'Unlimited data migration',
      'Full team training (up to 10 users)',
      'Workflow automation setup',
      '2 custom integrations',
      '60-day priority support',
    ],
    cta: 'Contact Sales',
    popular: true,
  },
  {
    name: 'Enterprise',
    description: 'White-glove service for large organizations',
    price: 'Custom',
    features: [
      'Multi-branch setup',
      'Complex data migration',
      'Unlimited training sessions',
      'Custom development',
      'Dedicated project manager',
      'On-premises deployment option',
      '90-day on-site support',
    ],
    cta: 'Contact Sales',
    popular: false,
  },
];

export default function ServicesPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Professional Services</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Expert help to get you
              <span className="text-primary-600 block">up and running</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              From implementation to ongoing support, our team of experts is here
              to ensure your success with XENON.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/contact" className="btn-primary btn-lg">
                Discuss Your Needs
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
              <a href="#packages" className="btn-secondary btn-lg">
                View Packages
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* Services Grid */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Our Services
            </h2>
            <p className="text-lg text-gray-600">
              Comprehensive professional services to support your journey.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {services.map((service) => (
              <div key={service.title} className="card">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                  <service.icon className="h-6 w-6" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {service.title}
                </h3>
                <p className="text-gray-600 text-sm mb-4">
                  {service.description}
                </p>
                <ul className="space-y-2 mb-6">
                  {service.features.slice(0, 4).map((feature) => (
                    <li key={feature} className="flex items-center gap-2 text-sm text-gray-600">
                      <Check className="h-4 w-4 text-primary-600 flex-shrink-0" />
                      {feature}
                    </li>
                  ))}
                </ul>
                <div className="pt-4 border-t">
                  <span className="text-sm text-gray-500">Starting at</span>
                  <div className="font-semibold text-gray-900">{service.pricing}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Packages */}
      <section id="packages" className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Implementation Packages
            </h2>
            <p className="text-lg text-gray-600">
              Bundled services to get you started quickly at a great value.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
            {packages.map((pkg) => (
              <div
                key={pkg.name}
                className={`card relative ${
                  pkg.popular ? 'border-primary-500 ring-2 ring-primary-500' : ''
                }`}
              >
                {pkg.popular && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <span className="badge bg-primary-600 text-white px-3 py-1">
                      Most Popular
                    </span>
                  </div>
                )}

                <div className="text-center mb-6">
                  <h3 className="text-xl font-bold text-gray-900 mb-2">
                    {pkg.name}
                  </h3>
                  <p className="text-sm text-gray-600 mb-4">
                    {pkg.description}
                  </p>
                  <div className="text-3xl font-bold text-gray-900">
                    {pkg.price}
                  </div>
                </div>

                <ul className="space-y-3 mb-6">
                  {pkg.features.map((feature) => (
                    <li key={feature} className="flex items-start gap-2 text-sm">
                      <Check className="h-4 w-4 text-primary-600 mt-0.5 flex-shrink-0" />
                      <span className="text-gray-600">{feature}</span>
                    </li>
                  ))}
                </ul>

                <Link
                  href="/contact"
                  className={`w-full ${pkg.popular ? 'btn-primary' : 'btn-secondary'}`}
                >
                  {pkg.cta}
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Process */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              How We Work
            </h2>
            <p className="text-lg text-gray-600">
              A proven process to ensure a smooth implementation.
            </p>
          </div>

          <div className="max-w-4xl mx-auto">
            <div className="grid md:grid-cols-4 gap-8">
              {[
                { step: '01', title: 'Discovery', desc: 'We learn about your business and requirements' },
                { step: '02', title: 'Planning', desc: 'Detailed project plan and timeline' },
                { step: '03', title: 'Implementation', desc: 'Configuration, migration, and training' },
                { step: '04', title: 'Launch', desc: 'Go-live with ongoing support' },
              ].map((item, index) => (
                <div key={item.step} className="relative text-center">
                  <div className="text-4xl font-bold text-primary-100 mb-2">
                    {item.step}
                  </div>
                  <h3 className="font-semibold text-gray-900 mb-1">
                    {item.title}
                  </h3>
                  <p className="text-sm text-gray-600">
                    {item.desc}
                  </p>
                  {index < 3 && (
                    <div className="hidden md:block absolute top-6 left-full w-full h-0.5 bg-primary-100 -translate-x-1/2" />
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Regional Support */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <div className="h-14 w-14 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-6">
                <Globe className="h-7 w-7" />
              </div>
              <h2 className="heading-2 text-gray-900 mb-6">
                Local expertise, regional reach
              </h2>
              <p className="text-lg text-gray-600 mb-6">
                Our team is based in Dubai with consultants across the GCC. We understand
                the unique requirements of businesses in the Gulf region.
              </p>
              <ul className="space-y-3">
                <li className="flex items-center gap-2 text-gray-700">
                  <Check className="h-5 w-5 text-primary-600" />
                  On-site implementation available in UAE
                </li>
                <li className="flex items-center gap-2 text-gray-700">
                  <Check className="h-5 w-5 text-primary-600" />
                  Remote support across GCC
                </li>
                <li className="flex items-center gap-2 text-gray-700">
                  <Check className="h-5 w-5 text-primary-600" />
                  Arabic and English support
                </li>
                <li className="flex items-center gap-2 text-gray-700">
                  <Check className="h-5 w-5 text-primary-600" />
                  Local billing and contracts
                </li>
              </ul>
            </div>
            <div className="card bg-primary-600 text-white">
              <Users className="h-12 w-12 mb-4 opacity-90" />
              <h3 className="text-xl font-bold mb-2">Need help deciding?</h3>
              <p className="text-primary-100 mb-6">
                Book a free consultation with our team. We'll assess your needs
                and recommend the right services for your business.
              </p>
              <Link
                href="/contact"
                className="btn bg-white text-primary-600 hover:bg-gray-100"
              >
                Book Free Consultation
              </Link>
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
            Contact our team to discuss your requirements and get a custom quote.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/contact" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Contact Us
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/demo" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              Start Free Trial
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
