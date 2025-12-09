import { Link } from 'react-router-dom';
import {
  ArrowRight,
  Megaphone,
  Users,
  Target,
  BarChart3,
  Mail,
  MessageSquare,
  Calendar,
  TrendingUp,
  Sparkles,
  CheckCircle,
  ArrowUpRight,
} from 'lucide-react';

const features = [
  {
    icon: Megaphone,
    title: 'Campaign Management',
    description:
      'Create, schedule, and manage multi-channel marketing campaigns. Track performance with real-time analytics and optimize your marketing spend.',
    highlights: [
      'Email, SMS, and social media campaigns',
      'Automated scheduling and triggers',
      'A/B testing capabilities',
      'Campaign templates library',
    ],
  },
  {
    icon: Users,
    title: 'Lead Management',
    description:
      'Capture, nurture, and convert leads into patients with our comprehensive lead management system. Never miss a follow-up again.',
    highlights: [
      'Lead scoring and prioritization',
      'Automated follow-up reminders',
      'Lead source tracking',
      'Conversion pipeline visualization',
    ],
  },
  {
    icon: Target,
    title: 'Patient Outreach',
    description:
      'Re-engage existing patients with targeted recall campaigns. Increase retention and lifetime value with personalized communication.',
    highlights: [
      'Appointment recall automation',
      'Birthday and anniversary messages',
      'Treatment completion reminders',
      'Reactivation campaigns',
    ],
  },
  {
    icon: BarChart3,
    title: 'Marketing Analytics',
    description:
      'Make data-driven decisions with comprehensive marketing analytics. Track ROI, conversion rates, and campaign performance at a glance.',
    highlights: [
      'Real-time dashboards',
      'ROI tracking per campaign',
      'Conversion funnel analysis',
      'Custom report builder',
    ],
  },
];

const capabilities = [
  {
    icon: Mail,
    title: 'Email Campaigns',
    description: 'Design and send professional email campaigns with our drag-and-drop builder.',
  },
  {
    icon: MessageSquare,
    title: 'SMS Marketing',
    description: 'Reach patients instantly with SMS campaigns and appointment reminders.',
  },
  {
    icon: Calendar,
    title: 'Event Marketing',
    description: 'Promote webinars, open houses, and health awareness events.',
  },
  {
    icon: TrendingUp,
    title: 'Referral Programs',
    description: 'Grow your practice with automated referral tracking and rewards.',
  },
  {
    icon: Sparkles,
    title: 'Social Media',
    description: 'Manage and schedule social media posts across multiple platforms.',
  },
  {
    icon: Target,
    title: 'Digital Advertising',
    description: 'Track and measure the effectiveness of your digital ad campaigns.',
  },
];

const stats = [
  { value: '35%', label: 'Average increase in patient appointments' },
  { value: '50%', label: 'Reduction in no-show rates' },
  { value: '3x', label: 'Return on marketing investment' },
  { value: '60%', label: 'Improvement in lead conversion' },
];

export default function MarketingPage() {
  return (
    <>
      {/* Hero Section */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-primary-50 to-white overflow-hidden">
        <div className="absolute inset-0 bg-grid-primary-100/50 [mask-image:radial-gradient(ellipse_at_center,transparent_20%,white)]" />
        <div className="container-marketing relative">
          <div className="max-w-4xl mx-auto text-center">
            <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary-100 text-primary-700 rounded-full text-sm font-medium mb-6">
              <Megaphone className="h-4 w-4" />
              Marketing Module
            </div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Grow your practice with intelligent marketing automation
            </h1>
            <p className="text-xl text-gray-600 mb-8 max-w-3xl mx-auto">
              Attract new patients, retain existing ones, and maximize your marketing ROI with
              XenonClinic's comprehensive marketing module. From campaigns to analytics, everything
              you need to grow your healthcare business.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/demo"
                className="btn bg-primary-600 text-white hover:bg-primary-700 btn-lg"
              >
                Start Free Trial
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
              <Link
                to="/contact"
                className="btn bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 btn-lg"
              >
                Talk to Sales
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-12 bg-white border-b border-gray-100">
        <div className="container-marketing">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            {stats.map((stat) => (
              <div key={stat.label} className="text-center">
                <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">
                  {stat.value}
                </div>
                <div className="text-sm text-gray-600">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Everything you need to grow your practice
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Our marketing module provides all the tools you need to attract, engage, and retain
              patients.
            </p>
          </div>

          <div className="space-y-16">
            {features.map((feature, index) => (
              <div
                key={feature.title}
                className={`flex flex-col ${
                  index % 2 === 0 ? 'lg:flex-row' : 'lg:flex-row-reverse'
                } gap-12 items-center`}
              >
                <div className="flex-1">
                  <div className="h-14 w-14 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-6">
                    <feature.icon className="h-7 w-7" />
                  </div>
                  <h3 className="text-2xl font-bold text-gray-900 mb-4">{feature.title}</h3>
                  <p className="text-gray-600 mb-6">{feature.description}</p>
                  <ul className="space-y-3">
                    {feature.highlights.map((highlight) => (
                      <li key={highlight} className="flex items-start gap-3">
                        <CheckCircle className="h-5 w-5 text-green-500 flex-shrink-0 mt-0.5" />
                        <span className="text-gray-700">{highlight}</span>
                      </li>
                    ))}
                  </ul>
                </div>
                <div className="flex-1 w-full">
                  <div className="bg-gradient-to-br from-gray-100 to-gray-50 rounded-2xl p-8 aspect-video flex items-center justify-center">
                    <feature.icon className="h-24 w-24 text-gray-300" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Capabilities Grid */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Marketing Capabilities</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Reach your patients wherever they are with multi-channel marketing tools.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {capabilities.map((capability) => (
              <div key={capability.title} className="card card-hover">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4">
                  <capability.icon className="h-6 w-6" />
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">{capability.title}</h3>
                <p className="text-sm text-gray-600">{capability.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Integration Section */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="bg-gradient-to-r from-primary-600 to-primary-700 rounded-3xl p-8 md:p-12">
            <div className="max-w-3xl mx-auto text-center">
              <h2 className="heading-2 text-white mb-4">
                Seamlessly integrated with your clinic workflow
              </h2>
              <p className="text-lg text-primary-100 mb-8">
                The marketing module works hand-in-hand with patient records, appointments, and
                financial data. Convert leads to patients, track the full patient journey, and
                measure the true ROI of your marketing efforts.
              </p>
              <div className="flex flex-wrap gap-4 justify-center">
                <div className="flex items-center gap-2 bg-white/10 rounded-full px-4 py-2 text-white text-sm">
                  <CheckCircle className="h-4 w-4" />
                  Patient Records Integration
                </div>
                <div className="flex items-center gap-2 bg-white/10 rounded-full px-4 py-2 text-white text-sm">
                  <CheckCircle className="h-4 w-4" />
                  Appointment Scheduling
                </div>
                <div className="flex items-center gap-2 bg-white/10 rounded-full px-4 py-2 text-white text-sm">
                  <CheckCircle className="h-4 w-4" />
                  Financial Tracking
                </div>
                <div className="flex items-center gap-2 bg-white/10 rounded-full px-4 py-2 text-white text-sm">
                  <CheckCircle className="h-4 w-4" />
                  Multi-Branch Support
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-gray-900 mb-4">
            Ready to transform your marketing?
          </h2>
          <p className="text-lg text-gray-600 mb-8 max-w-2xl mx-auto">
            Join hundreds of healthcare providers who are growing their practices with XenonClinic's
            marketing module.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/demo"
              className="btn bg-primary-600 text-white hover:bg-primary-700 btn-lg"
            >
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link
              to="/pricing"
              className="btn bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 btn-lg"
            >
              View Pricing
              <ArrowUpRight className="ml-2 h-5 w-5" />
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
