import type { Metadata } from 'next';
import Link from 'next/link';
import { docs, docCategories, getDocsByCategory } from '@/content/docs';
import { Book, ChevronRight, Search } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Documentation',
  description: 'Learn how to use XENON with our comprehensive documentation. Guides, tutorials, and API reference.',
};

export default function DocsPage() {
  return (
    <>
      {/* Hero */}
      <section className="relative py-16 md:py-24 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Documentation</div>
            <h1 className="heading-1 text-gray-900 mb-6">
              Learn how to use XENON
            </h1>
            <p className="text-lg md:text-xl text-gray-600 mb-8">
              Everything you need to get started, configure your tenant, and make the most of the platform.
            </p>

            {/* Search placeholder */}
            <div className="max-w-xl mx-auto">
              <div className="relative">
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search documentation..."
                  className="input input-lg pl-12 w-full"
                  disabled
                />
              </div>
              <p className="text-xs text-gray-500 mt-2">Search coming soon</p>
            </div>
          </div>
        </div>
      </section>

      {/* Quick Links */}
      <section className="py-12 bg-white border-b">
        <div className="container-marketing">
          <div className="grid md:grid-cols-3 gap-6">
            <Link href="/docs/getting-started" className="card card-hover group">
              <h3 className="font-semibold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">
                Getting Started
              </h3>
              <p className="text-sm text-gray-600 mb-3">
                New to XENON? Start here for a quick introduction.
              </p>
              <span className="text-primary-600 text-sm font-medium inline-flex items-center">
                Read guide <ChevronRight className="h-4 w-4 ml-1" />
              </span>
            </Link>
            <Link href="/docs/admin-setup" className="card card-hover group">
              <h3 className="font-semibold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">
                Admin Setup
              </h3>
              <p className="text-sm text-gray-600 mb-3">
                Configure your organization, branches, and settings.
              </p>
              <span className="text-primary-600 text-sm font-medium inline-flex items-center">
                Read guide <ChevronRight className="h-4 w-4 ml-1" />
              </span>
            </Link>
            <Link href="/docs/integrations" className="card card-hover group">
              <h3 className="font-semibold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">
                Integrations
              </h3>
              <p className="text-sm text-gray-600 mb-3">
                Connect XENON with payment gateways, SMS, and more.
              </p>
              <span className="text-primary-600 text-sm font-medium inline-flex items-center">
                Read guide <ChevronRight className="h-4 w-4 ml-1" />
              </span>
            </Link>
          </div>
        </div>
      </section>

      {/* Documentation by Category */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-4 gap-8">
            {/* Sidebar */}
            <div className="lg:col-span-1">
              <nav className="sticky top-24 space-y-6">
                {docCategories.map((category) => (
                  <div key={category}>
                    <h4 className="font-semibold text-gray-900 mb-2">{category}</h4>
                    <ul className="space-y-1">
                      {getDocsByCategory(category).map((doc) => (
                        <li key={doc.slug}>
                          <Link
                            href={`/docs/${doc.slug}`}
                            className="block text-sm text-gray-600 hover:text-primary-600 py-1 transition-colors"
                          >
                            {doc.title}
                          </Link>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}
              </nav>
            </div>

            {/* Content */}
            <div className="lg:col-span-3">
              {docCategories.map((category) => {
                const categoryDocs = getDocsByCategory(category);
                return (
                  <div key={category} className="mb-12 last:mb-0">
                    <h2 className="heading-3 text-gray-900 mb-6">{category}</h2>
                    <div className="grid md:grid-cols-2 gap-6">
                      {categoryDocs.map((doc) => (
                        <Link
                          key={doc.slug}
                          href={`/docs/${doc.slug}`}
                          className="card card-hover group"
                        >
                          <div className="flex items-start gap-3">
                            <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                              <Book className="h-5 w-5" />
                            </div>
                            <div>
                              <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                                {doc.title}
                              </h3>
                              <p className="text-sm text-gray-600 mt-1">
                                {doc.description}
                              </p>
                            </div>
                          </div>
                        </Link>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </section>

      {/* Help CTA */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing text-center">
          <h2 className="heading-3 text-gray-900 mb-4">
            Need more help?
          </h2>
          <p className="text-gray-600 mb-6 max-w-xl mx-auto">
            Can't find what you're looking for? Our support team is here to help.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/contact" className="btn-primary">
              Contact Support
            </Link>
            <Link href="/demo" className="btn-secondary">
              Request Demo
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}
