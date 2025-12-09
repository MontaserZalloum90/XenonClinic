import Link from 'next/link';
import { brand } from '@/lib/brand';
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
  Globe,
  Zap,
  Clock,
  Star,
  ChevronRight,
} from 'lucide-react';

const features = [
  {
    name: 'Patient Management',
    description: 'Complete patient records, medical history, and communication in one place.',
    icon: Users,
  },
  {
    name: 'Smart Scheduling',
    description: 'Intelligent appointment booking with automated reminders and calendar sync.',
    icon: Calendar,
  },
  {
    name: 'Billing & Invoicing',
    description: 'Generate invoices, track payments, and manage insurance claims effortlessly.',
    icon: Receipt,
  },
  {
    name: 'Inventory Control',
    description: 'Real-time stock tracking, automated reordering, and supplier management.',
    icon: Package,
  },
  {
    name: 'Analytics Dashboard',
    description: 'Actionable insights with customizable reports and KPI tracking.',
    icon: BarChart3,
  },
  {
    name: 'Multi-Branch Support',
    description: 'Manage multiple locations with centralized control and per-branch customization.',
    icon: Building2,
  },
];

const benefits = [
  {
    title: 'Built for Gulf SMBs',
    description: 'Designed specifically for healthcare and trading businesses in UAE, Saudi Arabia, and the wider Gulf region.',
    icon: Globe,
  },
  {
    title: 'Enterprise Security',
    description: 'Bank-grade encryption, role-based access, and compliance with regional healthcare regulations.',
    icon: Shield,
  },
  {
    title: 'Lightning Fast',
    description: 'Optimized performance with regional data centers ensuring sub-100ms response times.',
    icon: Zap,
  },
  {
    title: 'Quick Setup',
    description: 'Get started in minutes with guided onboarding and data import from existing systems.',
    icon: Clock,
  },
];

const testimonials = [
  {
    quote: "XENON transformed how we manage our audiology practice. The automated follow-ups alone have increased our patient retention by 35%.",
    author: 'Dr. Fatima Al-Hassan',
    role: 'Medical Director',
    company: 'Gulf Hearing Center',
  },
  {
    quote: "Finally, a system that understands the needs of multi-branch operations. We manage 5 dental clinics seamlessly from one dashboard.",
    author: 'Mohammed Al-Rashid',
    role: 'Operations Manager',
    company: 'Smile Dental Group',
  },
  {
    quote: "The inventory management and billing integration saved us countless hours. Our team can focus on patients, not paperwork.",
    author: 'Sarah Chen',
    role: 'Practice Manager',
    company: 'Emirates Veterinary Hospital',
  },
];

const companyTypes = [
  { name: 'Audiology Clinics', href: '/solutions/audiology' },
  { name: 'Dental Practices', href: '/solutions/dental' },
  { name: 'Veterinary Clinics', href: '/solutions/veterinary' },
  { name: 'Optical Centers', href: '/solutions/optical' },
  { name: 'Beauty & Dermatology', href: '/solutions/dermatology' },
  { name: 'Trading Companies', href: '/solutions/trading' },
];

export default function HomePage() {
  return (
    <>
      {/* Hero Section */}
      <section className="relative overflow-hidden gradient-hero">
        <div className="absolute inset-0 gradient-mesh" />
        <div className="container-marketing relative py-20 md:py-28 lg:py-36">
          <div className="max-w-3xl">
            <div className="flex items-center gap-2 mb-6">
              <span className="badge bg-primary-500/20 text-primary-300 border border-primary-500/30">
                New
              </span>
              <span className="text-sm text-gray-400">
                Multi-branch management now available
              </span>
            </div>

            <h1 className="heading-1 text-white mb-6">
              The Complete ERP for
              <span className="text-gradient block mt-2">Gulf Healthcare & Trading</span>
            </h1>

            <p className="text-lg md:text-xl text-gray-300 mb-8 max-w-2xl">
              {brand.description}. Start your free trial today and see why hundreds of businesses trust XENON.
            </p>

            <div className="flex flex-col sm:flex-row gap-4">
              <Link href="/demo" className="btn-primary btn-xl">
                Start Free Trial
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
              <Link href="/product" className="btn bg-white/10 text-white hover:bg-white/20 btn-xl">
                Watch Demo
              </Link>
            </div>

            <div className="mt-10 flex items-center gap-6 text-sm text-gray-400">
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-secondary-400" />
                <span>30-day free trial</span>
              </div>
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-secondary-400" />
                <span>No credit card required</span>
              </div>
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-secondary-400" />
                <span>Cancel anytime</span>
              </div>
            </div>
          </div>

          {/* Hero image placeholder */}
          <div className="hidden lg:block absolute right-0 top-1/2 -translate-y-1/2 w-1/2">
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-r from-slate-900 to-transparent z-10" />
              <div className="aspect-[4/3] rounded-l-2xl bg-gradient-to-br from-slate-800 to-slate-900 border border-slate-700/50 shadow-2xl overflow-hidden">
                <div className="p-6 space-y-4">
                  <div className="h-8 w-32 bg-slate-700/50 rounded-lg" />
                  <div className="grid grid-cols-3 gap-4">
                    <div className="h-24 bg-slate-700/30 rounded-lg" />
                    <div className="h-24 bg-slate-700/30 rounded-lg" />
                    <div className="h-24 bg-slate-700/30 rounded-lg" />
                  </div>
                  <div className="h-48 bg-slate-700/20 rounded-lg" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Company Types */}
      <section className="py-12 bg-gray-50 border-y">
        <div className="container-marketing">
          <p className="text-center text-sm text-gray-500 mb-6">
            Trusted by healthcare providers and trading companies across the Gulf
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            {companyTypes.map((type) => (
              <Link
                key={type.name}
                href={type.href}
                className="px-4 py-2 rounded-full bg-white border border-gray-200 text-sm text-gray-700 hover:border-primary-300 hover:text-primary-600 transition-colors"
              >
                {type.name}
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Features Grid */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Everything you need to run your business
            </h2>
            <p className="text-lg text-gray-600">
              A complete suite of tools designed for modern healthcare and trading operations in the Gulf region.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature) => (
              <div
                key={feature.name}
                className="card card-hover group"
              >
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4 group-hover:bg-primary-600 group-hover:text-white transition-colors">
                  <feature.icon className="h-6 w-6" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {feature.name}
                </h3>
                <p className="text-gray-600">{feature.description}</p>
              </div>
            ))}
          </div>

          <div className="text-center mt-12">
            <Link href="/features" className="link inline-flex items-center font-medium">
              Explore all features
              <ChevronRight className="ml-1 h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
            <div>
              <h2 className="heading-2 text-gray-900 mb-6">
                Why businesses choose {brand.name}
              </h2>
              <p className="text-lg text-gray-600 mb-8">
                We understand the unique challenges of running a business in the Gulf region.
                That's why we built XENON to address your specific needs.
              </p>

              <div className="space-y-6">
                {benefits.map((benefit) => (
                  <div key={benefit.title} className="flex gap-4">
                    <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                      <benefit.icon className="h-5 w-5" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900 mb-1">
                        {benefit.title}
                      </h3>
                      <p className="text-gray-600 text-sm">
                        {benefit.description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-2 gap-6">
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">500+</div>
                <div className="text-gray-600">Active Businesses</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">50K+</div>
                <div className="text-gray-600">Patients Managed</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">99.9%</div>
                <div className="text-gray-600">Uptime SLA</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">4.9</div>
                <div className="text-gray-600 flex items-center justify-center gap-1">
                  <Star className="h-4 w-4 text-yellow-400 fill-current" />
                  Rating
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Loved by businesses across the Gulf
            </h2>
            <p className="text-lg text-gray-600">
              See what our customers have to say about their experience with XENON.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            {testimonials.map((testimonial, index) => (
              <div key={index} className="card">
                <div className="flex gap-1 mb-4">
                  {[...Array(5)].map((_, i) => (
                    <Star key={i} className="h-4 w-4 text-yellow-400 fill-current" />
                  ))}
                </div>
                <blockquote className="text-gray-700 mb-6">
                  "{testimonial.quote}"
                </blockquote>
                <div className="flex items-center gap-3">
                  <div className="h-10 w-10 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center font-medium">
                    {testimonial.author.charAt(0)}
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">
                      {testimonial.author}
                    </div>
                    <div className="text-sm text-gray-500">
                      {testimonial.role}, {testimonial.company}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Ready to transform your business?
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Join hundreds of Gulf businesses already using XENON to streamline their operations.
            Start your free 30-day trial today.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-xl">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/contact" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-xl">
              Talk to Sales
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
