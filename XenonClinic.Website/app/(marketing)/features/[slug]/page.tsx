import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { features, getFeatureBySlug } from '@/content/features';
import { ArrowRight, ArrowLeft, Check } from 'lucide-react';
import * as LucideIcons from 'lucide-react';

interface FeaturePageProps {
  params: { slug: string };
}

export async function generateStaticParams() {
  return features.map((feature) => ({
    slug: feature.slug,
  }));
}

export async function generateMetadata({ params }: FeaturePageProps): Promise<Metadata> {
  const feature = getFeatureBySlug(params.slug);

  if (!feature) {
    return {
      title: 'Feature Not Found',
    };
  }

  return {
    title: feature.title,
    description: feature.description,
  };
}

function getIcon(iconName: string) {
  const IconComponent = (LucideIcons as Record<string, React.FC<{ className?: string }>>)[iconName];
  return IconComponent || LucideIcons.Box;
}

export default function FeaturePage({ params }: FeaturePageProps) {
  const feature = getFeatureBySlug(params.slug);

  if (!feature) {
    notFound();
  }

  const Icon = getIcon(feature.icon);

  // Find related features
  const relatedFeatures = features
    .filter(f => f.slug !== feature.slug && f.category === feature.category)
    .slice(0, 3);

  return (
    <>
      {/* Breadcrumb */}
      <div className="bg-gray-50 border-b">
        <div className="container-marketing py-4">
          <nav className="flex items-center gap-2 text-sm">
            <Link href="/features" className="text-gray-500 hover:text-gray-700">
              Features
            </Link>
            <span className="text-gray-400">/</span>
            <span className="text-gray-900 font-medium">{feature.title}</span>
          </nav>
        </div>
      </div>

      {/* Hero */}
      <section className="relative py-16 md:py-24 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <div className="h-14 w-14 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center mb-6">
                <Icon className="h-7 w-7" />
              </div>
              <span className="text-sm font-medium text-primary-600 mb-2 block">
                {feature.category}
              </span>
              <h1 className="heading-1 text-gray-900 mb-6">
                {feature.title}
              </h1>
              <p className="text-lg md:text-xl text-gray-600 mb-8">
                {feature.longDescription || feature.description}
              </p>
              <div className="flex flex-col sm:flex-row gap-4">
                <Link href="/demo" className="btn-primary btn-lg">
                  Try it Free
                  <ArrowRight className="ml-2 h-5 w-5" />
                </Link>
                <Link href="/contact" className="btn-secondary btn-lg">
                  Request Demo
                </Link>
              </div>
            </div>

            {/* Feature visual placeholder */}
            <div className="card bg-gray-50 aspect-[4/3] flex items-center justify-center">
              <div className="text-center text-gray-400">
                <Icon className="h-16 w-16 mx-auto mb-4 opacity-50" />
                <p className="text-sm">Feature screenshot</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Highlights */}
      {feature.highlights && feature.highlights.length > 0 && (
        <section className="section-padding bg-white">
          <div className="container-marketing">
            <h2 className="heading-2 text-gray-900 mb-8 text-center">
              Key Capabilities
            </h2>
            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
              {feature.highlights.map((highlight, index) => (
                <div key={index} className="flex gap-4">
                  <div className="h-8 w-8 rounded-lg bg-secondary-100 text-secondary-600 flex items-center justify-center flex-shrink-0">
                    <Check className="h-5 w-5" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900 mb-1">
                      {highlight.title}
                    </h3>
                    <p className="text-gray-600 text-sm">
                      {highlight.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* Benefits */}
      {feature.benefits && feature.benefits.length > 0 && (
        <section className="section-padding bg-gray-50">
          <div className="container-marketing">
            <h2 className="heading-2 text-gray-900 mb-8 text-center">
              Benefits
            </h2>
            <div className="max-w-3xl mx-auto">
              <ul className="space-y-4">
                {feature.benefits.map((benefit, index) => (
                  <li key={index} className="flex items-start gap-3 p-4 bg-white rounded-lg">
                    <Check className="h-5 w-5 text-primary-600 mt-0.5 flex-shrink-0" />
                    <span className="text-gray-700">{benefit}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </section>
      )}

      {/* Use Cases */}
      {feature.useCases && feature.useCases.length > 0 && (
        <section className="section-padding bg-white">
          <div className="container-marketing">
            <h2 className="heading-2 text-gray-900 mb-8 text-center">
              Use Cases
            </h2>
            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
              {feature.useCases.map((useCase, index) => (
                <div key={index} className="card">
                  <h3 className="font-semibold text-gray-900 mb-2">
                    {useCase.title}
                  </h3>
                  <p className="text-gray-600 text-sm">
                    {useCase.description}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* Related Features */}
      {relatedFeatures.length > 0 && (
        <section className="section-padding bg-gray-50">
          <div className="container-marketing">
            <h2 className="heading-3 text-gray-900 mb-8">
              Related Features
            </h2>
            <div className="grid md:grid-cols-3 gap-6">
              {relatedFeatures.map((related) => {
                const RelatedIcon = getIcon(related.icon);
                return (
                  <Link
                    key={related.slug}
                    href={`/features/${related.slug}`}
                    className="card card-hover group"
                  >
                    <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center mb-3 group-hover:bg-primary-600 group-hover:text-white transition-colors">
                      <RelatedIcon className="h-5 w-5" />
                    </div>
                    <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                      {related.title}
                    </h3>
                    <p className="text-gray-600 text-sm mt-1">
                      {related.description}
                    </p>
                  </Link>
                );
              })}
            </div>
          </div>
        </section>
      )}

      {/* CTA */}
      <section className="section-padding bg-primary-600">
        <div className="container-marketing text-center">
          <h2 className="heading-2 text-white mb-4">
            Ready to try {feature.title}?
          </h2>
          <p className="text-lg text-primary-100 mb-8 max-w-2xl mx-auto">
            Start your 30-day free trial and experience all features. No credit card required.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/demo" className="btn bg-white text-primary-600 hover:bg-gray-100 btn-lg">
              Start Free Trial
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/features" className="btn bg-primary-700 text-white hover:bg-primary-800 btn-lg">
              <ArrowLeft className="mr-2 h-5 w-5" />
              All Features
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
