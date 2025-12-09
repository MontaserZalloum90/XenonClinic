import type { Metadata } from 'next';
import Link from 'next/link';
import { features, featureCategories } from '@/content/features';
import { ArrowRight } from 'lucide-react';
import * as LucideIcons from 'lucide-react';

export const metadata: Metadata = {
  title: 'Features',
  description: 'Explore all XENON features - patient management, scheduling, billing, inventory, analytics, and more. Built for healthcare and trading businesses.',
};

function getIcon(iconName: string) {
  const IconComponent = (LucideIcons as Record<string, React.FC<{ className?: string }>>)[iconName];
  return IconComponent || LucideIcons.Box;
}

export default function FeaturesPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-20 md:py-28 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Features</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Powerful features for
              <span className="text-primary-600 block">modern businesses</span>
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              Everything you need to manage your clinic or trading business efficiently.
              Explore our comprehensive suite of tools designed for Gulf SMBs.
            </p>
          </div>
        </div>
      </section>

      {/* Features by Category */}
      {featureCategories.map((category) => {
        const categoryFeatures = features.filter(f => f.category === category);
        if (categoryFeatures.length === 0) return null;

        return (
          <section key={category} className="section-padding bg-white even:bg-gray-50">
            <div className="container-marketing">
              <h2 className="heading-3 text-gray-900 mb-8">{category}</h2>
              <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
                {categoryFeatures.map((feature) => {
                  const Icon = getIcon(feature.icon);
                  return (
                    <Link
                      key={feature.slug}
                      href={`/features/${feature.slug}`}
                      className="card card-hover group"
                    >
                      <div className="h-12 w-12 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-4 group-hover:bg-primary-600 group-hover:text-white transition-colors">
                        <Icon className="h-6 w-6" />
                      </div>
                      <h3 className="text-lg font-semibold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">
                        {feature.title}
                      </h3>
                      <p className="text-gray-600 text-sm mb-4">
                        {feature.description}
                      </p>
                      <span className="text-primary-600 text-sm font-medium inline-flex items-center group-hover:gap-2 transition-all">
                        Learn more
                        <ArrowRight className="h-4 w-4 ml-1" />
                      </span>
                    </Link>
                  );
                })}
              </div>
            </div>
          </section>
        );
      })}

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            See all features in action
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your free trial and explore every feature. No credit card required.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/pricing" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              View Pricing
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
