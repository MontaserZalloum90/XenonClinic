import type { Metadata } from 'next';
import Link from 'next/link';
import { ArrowRight, Target, Heart, Zap, Users } from 'lucide-react';

export const metadata: Metadata = {
  title: 'About Us',
  description: 'Learn about XENON - our mission to empower Gulf SMBs with modern, configurable ERP and CRM software.',
};

const values = [
  {
    icon: Target,
    title: 'Customer First',
    description: 'Every decision we make starts with understanding our customers\' needs. Your success is our success.',
  },
  {
    icon: Heart,
    title: 'Simplicity',
    description: 'Powerful software doesn\'t have to be complicated. We strive to make complex tasks simple.',
  },
  {
    icon: Zap,
    title: 'Innovation',
    description: 'We continuously improve our platform, embracing new technologies to deliver better solutions.',
  },
  {
    icon: Users,
    title: 'Partnership',
    description: 'We see ourselves as partners in your growth, not just a software vendor.',
  },
];

const milestones = [
  { year: '2020', event: 'Founded in Dubai with a vision to modernize Gulf SMB software' },
  { year: '2021', event: 'Launched first version of XENON Clinic CRM' },
  { year: '2022', event: 'Expanded to support trading companies and multi-branch operations' },
  { year: '2023', event: 'Reached 500+ active businesses across the GCC' },
  { year: '2024', event: 'Introduced configuration-driven architecture for ultimate flexibility' },
];

const team = [
  {
    name: 'Ahmed Al-Rashid',
    role: 'Co-Founder & CEO',
    bio: '15+ years in healthcare IT. Previously led product at a major UAE hospital group.',
  },
  {
    name: 'Sarah Chen',
    role: 'Co-Founder & CTO',
    bio: 'Ex-Microsoft engineer. Passionate about building scalable, user-friendly software.',
  },
  {
    name: 'Mohammed Al-Hassan',
    role: 'VP of Sales',
    bio: 'Deep expertise in Gulf B2B sales. Built sales teams at 3 successful SaaS startups.',
  },
  {
    name: 'Fatima Al-Mahmoud',
    role: 'Head of Customer Success',
    bio: 'Background in healthcare operations. Ensures every customer achieves their goals.',
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
              Empowering Gulf businesses
              <span className="text-primary-600 block">with modern software</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600">
              XENON was founded with a simple mission: to give small and medium businesses
              in the Gulf region access to the same powerful software tools that large enterprises use.
            </p>
          </div>
        </div>
      </section>

      {/* Mission */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="heading-2 text-gray-900 mb-6">
                Our Mission
              </h2>
              <p className="text-lg text-gray-600 mb-6">
                We believe that every business, regardless of size, deserves software that's
                powerful, flexible, and easy to use. Too many SMBs in the Gulf region are
                stuck with outdated systems or expensive enterprise solutions that don't fit
                their needs.
              </p>
              <p className="text-lg text-gray-600 mb-6">
                XENON changes that. We've built a platform that adapts to your business,
                not the other way around. Whether you're running a single clinic or managing
                a multi-branch trading operation, XENON grows with you.
              </p>
              <p className="text-lg text-gray-600">
                Our team combines deep expertise in healthcare IT, Gulf business practices,
                and modern software development to deliver a product that truly serves our customers.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-6">
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">500+</div>
                <div className="text-gray-600">Active Businesses</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">6</div>
                <div className="text-gray-600">GCC Countries</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">50K+</div>
                <div className="text-gray-600">Users</div>
              </div>
              <div className="card text-center">
                <div className="text-4xl font-bold text-primary-600 mb-2">30+</div>
                <div className="text-gray-600">Team Members</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Values */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Our Values
            </h2>
            <p className="text-lg text-gray-600">
              The principles that guide everything we do.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {values.map((value) => (
              <div key={value.title} className="text-center">
                <div className="h-14 w-14 rounded-2xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4">
                  <value.icon className="h-7 w-7" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {value.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {value.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Timeline */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Our Journey
            </h2>
            <p className="text-lg text-gray-600">
              From startup to trusted platform for Gulf businesses.
            </p>
          </div>

          <div className="max-w-2xl mx-auto">
            <div className="relative">
              <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200" />
              <div className="space-y-8">
                {milestones.map((milestone, index) => (
                  <div key={index} className="relative pl-12">
                    <div className="absolute left-0 top-1 h-8 w-8 rounded-full bg-primary-600 text-white flex items-center justify-center text-sm font-medium">
                      {milestone.year.slice(-2)}
                    </div>
                    <div className="card">
                      <div className="font-semibold text-primary-600 mb-1">
                        {milestone.year}
                      </div>
                      <div className="text-gray-700">{milestone.event}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Team */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing">
          <div className="text-center max-w-2xl mx-auto mb-12 lg:mb-16">
            <h2 className="heading-2 text-gray-900 mb-4">
              Leadership Team
            </h2>
            <p className="text-lg text-gray-600">
              Experienced leaders passionate about helping Gulf businesses succeed.
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {team.map((member) => (
              <div key={member.name} className="card text-center">
                <div className="h-20 w-20 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-4 text-2xl font-bold">
                  {member.name.split(' ').map(n => n[0]).join('')}
                </div>
                <h3 className="font-semibold text-gray-900 mb-1">
                  {member.name}
                </h3>
                <div className="text-sm text-primary-600 mb-3">
                  {member.role}
                </div>
                <p className="text-gray-600 text-sm">
                  {member.bio}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Join our growing community
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Become one of the hundreds of Gulf businesses transforming their operations with XENON.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/contact" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              Contact Us
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
