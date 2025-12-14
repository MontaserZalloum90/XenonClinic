import { Link } from 'react-router-dom';
import {
  Rocket,
  Users,
  LayoutGrid,
  Route,
  Shield,
  Code,
  HelpCircle,
  Book,
  ArrowRight,
  CheckCircle,
} from 'lucide-react';
import { docsStatistics } from '../../lib/docs/docsData';

const quickLinks = [
  {
    icon: Rocket,
    title: 'Getting Started',
    description: 'Quick start guide to get up and running',
    path: '/docs/getting-started',
    color: 'bg-green-100 text-green-600',
  },
  {
    icon: Users,
    title: 'User Personas',
    description: 'Documentation organized by user role',
    path: '/docs/personas',
    color: 'bg-blue-100 text-blue-600',
  },
  {
    icon: LayoutGrid,
    title: 'Product Modules',
    description: `Explore all ${docsStatistics.totalModules} product modules`,
    path: '/docs/modules',
    color: 'bg-purple-100 text-purple-600',
  },
  {
    icon: Route,
    title: 'User Journeys',
    description: 'Step-by-step workflows and processes',
    path: '/docs/journeys',
    color: 'bg-orange-100 text-orange-600',
  },
];

const resources = [
  {
    icon: Shield,
    title: 'Security & RBAC',
    description: 'Role-based access control and permissions',
    path: '/docs/security-rbac',
  },
  {
    icon: Code,
    title: 'API Reference',
    description: 'REST API endpoints and integration guides',
    path: '/docs/api-reference',
  },
  {
    icon: HelpCircle,
    title: 'FAQ & Troubleshooting',
    description: 'Common questions and solutions',
    path: '/docs/faq-troubleshooting',
  },
  {
    icon: Book,
    title: 'Glossary',
    description: 'Healthcare and ERP terminology',
    path: '/docs/glossary',
  },
];

// Dynamic highlights using actual statistics
const getHighlights = () => [
  `${docsStatistics.totalModules} integrated modules for healthcare and business operations`,
  `${docsStatistics.totalPersonas} user personas with role-specific documentation`,
  `${docsStatistics.totalJourneys} documented user journeys with ${docsStatistics.totalSteps}+ steps`,
  `${docsStatistics.totalFeatures}+ features across all modules`,
  'HIPAA-ready security and audit logging',
  'Multi-tenant, multi-branch architecture',
];

export default function DocsHome() {
  return (
    <div className="space-y-12">
      {/* Hero */}
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          XenonClinic Documentation
        </h1>
        <p className="text-xl text-gray-600 max-w-2xl mx-auto">
          Enterprise-grade documentation for the complete healthcare and business
          management platform. Everything you need to configure, use, and integrate
          XenonClinic.
        </p>
      </div>

      {/* Quick Links */}
      <div className="grid sm:grid-cols-2 gap-4">
        {quickLinks.map((link) => (
          <Link
            key={link.path}
            to={link.path}
            className="group flex items-start gap-4 p-5 bg-white border border-gray-200 rounded-xl hover:border-primary-300 hover:shadow-md transition-all"
          >
            <div className={`p-3 rounded-lg ${link.color}`}>
              <link.icon className="h-6 w-6" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                {link.title}
              </h3>
              <p className="text-sm text-gray-500 mt-1">{link.description}</p>
            </div>
            <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 group-hover:translate-x-1 transition-all" />
          </Link>
        ))}
      </div>

      {/* Platform Highlights */}
      <div className="bg-gradient-to-br from-primary-50 to-blue-50 rounded-2xl p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-6">Platform Highlights</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          {getHighlights().map((highlight, index) => (
            <div key={index} className="flex items-start gap-3">
              <CheckCircle className="h-5 w-5 text-primary-600 flex-shrink-0 mt-0.5" />
              <span className="text-gray-700">{highlight}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Popular Modules */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900 mb-6">Popular Modules</h2>
        <div className="grid sm:grid-cols-3 gap-4">
          {[
            { name: 'Patient Management', path: '/docs/modules/patient-management', count: '6 features', category: 'Core' },
            { name: 'Appointments', path: '/docs/modules/appointments', count: '7 features', category: 'Core' },
            { name: 'Clinical Visits', path: '/docs/modules/clinical-visits', count: '7 features', category: 'Clinical' },
            { name: 'Laboratory', path: '/docs/modules/laboratory', count: '7 features', category: 'Clinical' },
            { name: 'Pharmacy', path: '/docs/modules/pharmacy', count: '6 features', category: 'Clinical' },
            { name: 'Financial', path: '/docs/modules/financial', count: '6 features', category: 'Business' },
            { name: 'Procurement', path: '/docs/modules/procurement', count: '6 features', category: 'Business' },
            { name: 'HR Management', path: '/docs/modules/hr-management', count: '6 features', category: 'Business' },
            { name: 'Marketing & CRM', path: '/docs/modules/marketing', count: '6 features', category: 'Business' },
          ].map((module) => (
            <Link
              key={module.path}
              to={module.path}
              className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all"
            >
              <div className="flex items-center justify-between mb-1">
                <h3 className="font-medium text-gray-900">{module.name}</h3>
                <span className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded">{module.category}</span>
              </div>
              <p className="text-sm text-gray-500">{module.count}</p>
            </Link>
          ))}
        </div>
      </div>

      {/* Additional Resources */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900 mb-6">Additional Resources</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {resources.map((resource) => (
            <Link
              key={resource.path}
              to={resource.path}
              className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
            >
              <resource.icon className="h-6 w-6 text-gray-400 group-hover:text-primary-600 transition-colors mb-3" />
              <h3 className="font-medium text-gray-900 group-hover:text-primary-600 transition-colors">
                {resource.title}
              </h3>
              <p className="text-sm text-gray-500 mt-1">{resource.description}</p>
            </Link>
          ))}
        </div>
      </div>

      {/* Need Help */}
      <div className="bg-gray-900 text-white rounded-2xl p-8 text-center">
        <h2 className="text-2xl font-bold mb-3">Need Help?</h2>
        <p className="text-gray-300 mb-6 max-w-xl mx-auto">
          Our support team is here to help you get the most out of XenonClinic.
          Contact us for implementation assistance, training, or technical support.
        </p>
        <div className="flex flex-wrap justify-center gap-4">
          <Link
            to="/contact"
            className="px-6 py-2.5 bg-white text-gray-900 font-medium rounded-lg hover:bg-gray-100 transition-colors"
          >
            Contact Support
          </Link>
          <Link
            to="/demo"
            className="px-6 py-2.5 bg-primary-600 text-white font-medium rounded-lg hover:bg-primary-700 transition-colors"
          >
            Request Demo
          </Link>
        </div>
      </div>
    </div>
  );
}
