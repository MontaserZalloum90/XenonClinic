import { Link } from 'react-router-dom';
import { brand } from '@/lib/brand';
import { ArrowRight, Target, Users, Globe, Award } from 'lucide-react';

const values = [
  {
    icon: Target,
    title: 'Customer First',
    description: 'We build for our customers, listening to their needs and delivering solutions that matter.',
  },
  {
    icon: Users,
    title: 'Team Excellence',
    description: 'Our diverse team brings expertise from healthcare, technology, and business domains.',
  },
  {
    icon: Globe,
    title: 'Regional Focus',
    description: 'Deep understanding of Gulf business practices, regulations, and cultural nuances.',
  },
  {
    icon: Award,
    title: 'Quality First',
    description: 'Enterprise-grade security and reliability that healthcare providers trust.',
  },
];

export default function AboutPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">About Us</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Building the future of Gulf enterprise software
            </h1>
            <p className="text-lg md:text-xl text-gray-600">
              {brand.name} is on a mission to empower Gulf businesses with modern, configurable
              enterprise software that respects local practices while enabling global best practices.
            </p>
          </div>
        </div>
      </section>

      {/* Mission */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="heading-2 text-gray-900 mb-6">Our Mission</h2>
              <p className="text-lg text-gray-600 mb-4">
                We believe every Gulf business deserves access to world-class enterprise software.
                That's why we built {brand.name} - a configurable ERP platform that adapts to your
                business, not the other way around.
              </p>
              <p className="text-gray-600 mb-6">
                From audiology clinics in Dubai to trading companies in Riyadh, we serve businesses
                of all sizes with the same enterprise-grade features, security, and support.
              </p>
              <Link to="/demo" className="btn-primary">
                Start Free Trial
                <ArrowRight className="ml-2 h-5 w-5" />
              </Link>
            </div>
            <div className="bg-gradient-to-br from-primary-100 to-primary-200 rounded-2xl p-8 lg:p-12">
              <div className="grid grid-cols-2 gap-6">
                <div className="text-center">
                  <div className="text-4xl font-bold text-primary-600">500+</div>
                  <div className="text-sm text-gray-600 mt-1">Active Businesses</div>
                </div>
                <div className="text-center">
                  <div className="text-4xl font-bold text-primary-600">6</div>
                  <div className="text-sm text-gray-600 mt-1">GCC Countries</div>
                </div>
                <div className="text-center">
                  <div className="text-4xl font-bold text-primary-600">99.9%</div>
                  <div className="text-sm text-gray-600 mt-1">Uptime SLA</div>
                </div>
                <div className="text-center">
                  <div className="text-4xl font-bold text-primary-600">24/7</div>
                  <div className="text-sm text-gray-600 mt-1">Support</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Values */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center mb-12">
            <h2 className="heading-2 text-gray-900 mb-4">Our Values</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              The principles that guide everything we do.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {values.map((value) => (
              <div key={value.title} className="card text-center">
                <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                  <value.icon className="h-6 w-6" />
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">{value.title}</h3>
                <p className="text-sm text-gray-600">{value.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">Join the {brand.name} family</h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your free trial today and see why Gulf businesses choose {brand.name}.
          </p>
          <Link to="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
            Get Started Free
            <ArrowRight className="ml-2 h-5 w-5" />
          </Link>
        </div>
      </section>
    </>
  );
}
