import type { Metadata } from 'next';
import Link from 'next/link';
import {
  Shield,
  Lock,
  Server,
  FileCheck,
  Eye,
  RefreshCw,
  Users,
  Globe,
  Check,
  ArrowRight,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Compliance & Security',
  description: 'Learn about XENON\'s security practices, data protection, and compliance with healthcare regulations in the Gulf region.',
};

const securityFeatures = [
  {
    icon: Lock,
    title: 'End-to-End Encryption',
    description: 'All data is encrypted in transit (TLS 1.3) and at rest (AES-256). Your sensitive information is always protected.',
  },
  {
    icon: Users,
    title: 'Role-Based Access Control',
    description: 'Granular permissions system ensures users only access data relevant to their role. Audit every access.',
  },
  {
    icon: Server,
    title: 'UAE Data Residency',
    description: 'Data stored in UAE data centers. Full compliance with local data sovereignty requirements.',
  },
  {
    icon: Eye,
    title: 'Comprehensive Audit Logs',
    description: 'Every action is logged with timestamps, user IDs, and IP addresses. Complete visibility into system usage.',
  },
  {
    icon: RefreshCw,
    title: 'Automatic Backups',
    description: 'Daily encrypted backups with 30-day retention. Point-in-time recovery available.',
  },
  {
    icon: FileCheck,
    title: 'Regular Security Assessments',
    description: 'Annual penetration testing and quarterly vulnerability scans by certified third parties.',
  },
];

const complianceStandards = [
  {
    name: 'HIPAA Aligned',
    description: 'Our security practices align with HIPAA requirements for healthcare data protection.',
    status: 'Aligned',
  },
  {
    name: 'SOC 2 Type II',
    description: 'Following SOC 2 security, availability, and confidentiality principles.',
    status: 'In Progress',
  },
  {
    name: 'UAE PDPL',
    description: 'Compliant with UAE Personal Data Protection Law requirements.',
    status: 'Compliant',
  },
  {
    name: 'Saudi PDPL',
    description: 'Prepared for Saudi Personal Data Protection Law requirements.',
    status: 'Compliant',
  },
  {
    name: 'ISO 27001',
    description: 'Information security management system aligned with ISO 27001 standards.',
    status: 'Aligned',
  },
  {
    name: 'GDPR',
    description: 'For customers with EU data subjects, GDPR-compliant data handling.',
    status: 'Compliant',
  },
];

const dataProtectionPractices = [
  'Data minimization - we only collect what\'s necessary',
  'Purpose limitation - data used only for stated purposes',
  'Storage limitation - automatic data retention policies',
  'Right to erasure - complete data deletion on request',
  'Data portability - export your data in standard formats',
  'Privacy by design - security built into every feature',
];

export default function CompliancePage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Security & Compliance</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Your data security is our
              <span className="text-primary-600 block">top priority</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              XENON is built with enterprise-grade security and designed to meet healthcare
              compliance requirements in the Gulf region.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/contact" className="btn-primary btn-lg">
                Request Security Review
              </Link>
              <a href="#security-features" className="btn-secondary btn-lg">
                Learn More
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* Security Features */}
      <section id="security-features" className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Enterprise-grade security
            </h2>
            <p className="text-lg text-gray-600">
              Every layer of XENON is designed with security in mind.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {securityFeatures.map((feature) => (
              <div key={feature.title} className="card">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                  <feature.icon className="h-6 w-6" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {feature.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {feature.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Compliance Standards */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Compliance certifications
            </h2>
            <p className="text-lg text-gray-600">
              We continuously work to meet and exceed industry standards and regional regulations.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto">
            {complianceStandards.map((standard) => (
              <div key={standard.name} className="card">
                <div className="flex items-start justify-between mb-3">
                  <h3 className="font-semibold text-gray-900">
                    {standard.name}
                  </h3>
                  <span
                    className={`badge text-xs ${
                      standard.status === 'Compliant'
                        ? 'bg-green-100 text-green-700'
                        : standard.status === 'Aligned'
                        ? 'bg-blue-100 text-blue-700'
                        : 'bg-yellow-100 text-yellow-700'
                    }`}
                  >
                    {standard.status}
                  </span>
                </div>
                <p className="text-gray-600 text-sm">
                  {standard.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Data Protection */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <div className="badge-secondary mb-4">Data Protection</div>
              <h2 className="heading-2 text-gray-900 mb-6">
                Your data, your control
              </h2>
              <p className="text-lg text-gray-600 mb-8">
                We follow strict data protection principles to ensure your information
                is handled responsibly and transparently.
              </p>
              <ul className="space-y-4">
                {dataProtectionPractices.map((practice, index) => (
                  <li key={index} className="flex items-start gap-3">
                    <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                    <span className="text-gray-700">{practice}</span>
                  </li>
                ))}
              </ul>
            </div>

            <div className="card bg-gray-50">
              <div className="flex items-center gap-3 mb-6">
                <div className="h-12 w-12 rounded-xl bg-primary-600 text-white flex items-center justify-center">
                  <Globe className="h-6 w-6" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">Data Residency</h3>
                  <p className="text-sm text-gray-600">Choose where your data lives</p>
                </div>
              </div>
              <div className="space-y-4">
                <div className="p-4 bg-white rounded-lg border">
                  <div className="font-medium text-gray-900 mb-1">UAE (Dubai)</div>
                  <div className="text-sm text-gray-600">Primary data center for GCC customers</div>
                </div>
                <div className="p-4 bg-white rounded-lg border">
                  <div className="font-medium text-gray-900 mb-1">On-Premises</div>
                  <div className="text-sm text-gray-600">Deploy on your own infrastructure</div>
                </div>
                <div className="p-4 bg-white rounded-lg border border-dashed">
                  <div className="font-medium text-gray-500 mb-1">Saudi Arabia</div>
                  <div className="text-sm text-gray-400">Coming soon</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Incident Response */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="h-16 w-16 rounded-2xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-6">
              <Shield className="h-8 w-8" />
            </div>
            <h2 className="heading-2 text-gray-900 mb-4">
              Incident Response
            </h2>
            <p className="text-lg text-gray-600 mb-8">
              In the unlikely event of a security incident, we have comprehensive procedures
              in place to protect your data and keep you informed.
            </p>
            <div className="grid md:grid-cols-3 gap-6 text-left">
              <div className="card">
                <div className="text-3xl font-bold text-primary-600 mb-2">15 min</div>
                <div className="font-medium text-gray-900 mb-1">Initial Response</div>
                <div className="text-sm text-gray-600">
                  Security team alerted and investigation begins
                </div>
              </div>
              <div className="card">
                <div className="text-3xl font-bold text-primary-600 mb-2">1 hour</div>
                <div className="font-medium text-gray-900 mb-1">Customer Notification</div>
                <div className="text-sm text-gray-600">
                  Affected customers notified with initial assessment
                </div>
              </div>
              <div className="card">
                <div className="text-3xl font-bold text-primary-600 mb-2">24 hours</div>
                <div className="font-medium text-gray-900 mb-1">Detailed Report</div>
                <div className="text-sm text-gray-600">
                  Full incident report with remediation steps
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Have security questions?
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Our security team is happy to discuss our practices, provide documentation,
            or schedule a security review.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/contact" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Contact Security Team
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <a
              href="/security-whitepaper.pdf"
              className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg"
            >
              Download Security Whitepaper
            </a>
          </div>
        </div>
      </section>
    </>
  );
}
