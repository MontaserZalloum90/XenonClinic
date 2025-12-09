import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { docs, getDocBySlug, getDocNavigation } from '@/content/docs';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface DocPageProps {
  params: { slug: string };
}

export async function generateStaticParams() {
  return docs.map((doc) => ({
    slug: doc.slug,
  }));
}

export async function generateMetadata({ params }: DocPageProps): Promise<Metadata> {
  const doc = getDocBySlug(params.slug);

  if (!doc) {
    return {
      title: 'Page Not Found',
    };
  }

  return {
    title: doc.title,
    description: doc.description,
  };
}

export default function DocPage({ params }: DocPageProps) {
  const doc = getDocBySlug(params.slug);

  if (!doc) {
    notFound();
  }

  const navigation = getDocNavigation();
  const allDocs = docs.sort((a, b) => a.order - b.order);
  const currentIndex = allDocs.findIndex((d) => d.slug === doc.slug);
  const prevDoc = currentIndex > 0 ? allDocs[currentIndex - 1] : null;
  const nextDoc = currentIndex < allDocs.length - 1 ? allDocs[currentIndex + 1] : null;

  return (
    <div className="bg-white">
      <div className="container-marketing py-12">
        <div className="grid lg:grid-cols-4 gap-12">
          {/* Sidebar */}
          <div className="hidden lg:block lg:col-span-1">
            <nav className="sticky top-24 space-y-6">
              {navigation.map(({ category, pages }) => (
                <div key={category}>
                  <h4 className="font-semibold text-gray-900 mb-2">{category}</h4>
                  <ul className="space-y-1">
                    {pages.map((page) => (
                      <li key={page.slug}>
                        <Link
                          href={`/docs/${page.slug}`}
                          className={`block text-sm py-1.5 px-2 rounded transition-colors ${
                            page.slug === doc.slug
                              ? 'bg-primary-50 text-primary-600 font-medium'
                              : 'text-gray-600 hover:text-primary-600 hover:bg-gray-50'
                          }`}
                        >
                          {page.title}
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
            {/* Breadcrumb */}
            <nav className="mb-8 text-sm">
              <ol className="flex items-center gap-2">
                <li>
                  <Link href="/docs" className="text-gray-500 hover:text-gray-700">
                    Docs
                  </Link>
                </li>
                <li className="text-gray-400">/</li>
                <li>
                  <span className="text-gray-500">{doc.category}</span>
                </li>
                <li className="text-gray-400">/</li>
                <li>
                  <span className="text-gray-900 font-medium">{doc.title}</span>
                </li>
              </ol>
            </nav>

            {/* Article */}
            <article className="prose-docs">
              <div
                dangerouslySetInnerHTML={{
                  __html: doc.content
                    .replace(/^# /gm, '<h1>')
                    .replace(/\n## /g, '</p><h2>')
                    .replace(/\n### /g, '</p><h3>')
                    .replace(/\n- \*\*/g, '</p><li><strong>')
                    .replace(/\*\*/g, '</strong>')
                    .replace(/\n- /g, '</li><li>')
                    .replace(/\n\n/g, '</p><p>')
                    .replace(/\n(\d+)\. /g, '</li><li>')
                }}
              />
            </article>

            {/* Navigation */}
            <div className="mt-12 pt-8 border-t flex justify-between">
              {prevDoc ? (
                <Link
                  href={`/docs/${prevDoc.slug}`}
                  className="group flex items-center gap-2 text-gray-600 hover:text-primary-600"
                >
                  <ChevronLeft className="h-4 w-4" />
                  <div>
                    <div className="text-xs text-gray-500">Previous</div>
                    <div className="font-medium">{prevDoc.title}</div>
                  </div>
                </Link>
              ) : (
                <div />
              )}

              {nextDoc ? (
                <Link
                  href={`/docs/${nextDoc.slug}`}
                  className="group flex items-center gap-2 text-right text-gray-600 hover:text-primary-600"
                >
                  <div>
                    <div className="text-xs text-gray-500">Next</div>
                    <div className="font-medium">{nextDoc.title}</div>
                  </div>
                  <ChevronRight className="h-4 w-4" />
                </Link>
              ) : (
                <div />
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
