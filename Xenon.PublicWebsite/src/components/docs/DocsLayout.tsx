import { useState, useEffect } from 'react';
import { Outlet, useLocation, Link } from 'react-router-dom';
import { DocsSidebar } from './DocsSidebar';
import { DocsSearch } from './DocsSearch';
import { TableOfContents } from './TableOfContents';
import { Menu, X, ChevronRight, ExternalLink } from 'lucide-react';
import { brand } from '@/lib/brand';

export function DocsLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);
  const location = useLocation();

  useEffect(() => {
    setSidebarOpen(false);
    window.scrollTo(0, 0);
  }, [location.pathname]);

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 10);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Generate breadcrumbs from pathname
  const generateBreadcrumbs = () => {
    const paths = location.pathname.split('/').filter(Boolean);
    const breadcrumbs = [{ label: 'Home', path: '/' }];

    let currentPath = '';
    paths.forEach((segment) => {
      currentPath += `/${segment}`;
      const label = segment
        .split('-')
        .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ');
      breadcrumbs.push({ label, path: currentPath });
    });

    return breadcrumbs;
  };

  const breadcrumbs = generateBreadcrumbs();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header
        className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
          scrolled ? 'bg-white shadow-sm' : 'bg-white/95 backdrop-blur-sm'
        }`}
      >
        <div className="flex h-16 items-center justify-between px-4 lg:px-6">
          {/* Logo and mobile menu */}
          <div className="flex items-center gap-4">
            <button
              type="button"
              className="lg:hidden p-2 rounded-lg text-gray-700 hover:bg-gray-100"
              onClick={() => setSidebarOpen(!sidebarOpen)}
              aria-label="Toggle sidebar"
            >
              {sidebarOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>

            <Link to="/" className="flex items-center gap-2">
              <div className="h-8 w-8 rounded-lg bg-primary-600 flex items-center justify-center">
                <span className="text-white font-bold text-lg">X</span>
              </div>
              <span className="font-bold text-xl text-gray-900">{brand.name}</span>
            </Link>

            <span className="hidden sm:inline-flex items-center px-2 py-1 text-xs font-medium text-primary-700 bg-primary-100 rounded-full">
              Documentation
            </span>
          </div>

          {/* Search */}
          <div className="flex-1 max-w-xl mx-4 hidden md:block">
            <DocsSearch />
          </div>

          {/* Right side */}
          <div className="flex items-center gap-3">
            <span className="hidden lg:inline-flex items-center px-2 py-1 text-xs font-medium text-gray-600 bg-gray-100 rounded">
              v1.0
            </span>
            <Link
              to="/"
              className="hidden sm:inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-gray-700 hover:text-gray-900 transition-colors"
            >
              Back to Website
              <ExternalLink className="h-3.5 w-3.5" />
            </Link>
            <Link to="/demo" className="btn-primary text-sm py-1.5 px-3">
              Get Started
            </Link>
          </div>
        </div>

        {/* Mobile search */}
        <div className="md:hidden px-4 pb-3">
          <DocsSearch />
        </div>
      </header>

      {/* Main layout */}
      <div className="flex pt-16 md:pt-16">
        {/* Sidebar overlay for mobile */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 bg-black/50 z-40 lg:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Sidebar */}
        <aside
          className={`fixed lg:sticky top-16 left-0 z-40 h-[calc(100vh-4rem)] w-72 bg-white border-r border-gray-200 overflow-y-auto transition-transform duration-300 lg:translate-x-0 ${
            sidebarOpen ? 'translate-x-0' : '-translate-x-full'
          }`}
        >
          <DocsSidebar />
        </aside>

        {/* Main content */}
        <main className="flex-1 min-w-0">
          {/* Breadcrumbs */}
          <div className="bg-white border-b border-gray-200 px-6 py-3">
            <nav className="flex items-center text-sm">
              {breadcrumbs.map((crumb, index) => (
                <div key={crumb.path} className="flex items-center">
                  {index > 0 && <ChevronRight className="h-4 w-4 text-gray-400 mx-2" />}
                  {index === breadcrumbs.length - 1 ? (
                    <span className="text-gray-900 font-medium">{crumb.label}</span>
                  ) : (
                    <Link
                      to={crumb.path}
                      className="text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      {crumb.label}
                    </Link>
                  )}
                </div>
              ))}
            </nav>
          </div>

          {/* Content area with TOC */}
          <div className="flex">
            <div className="flex-1 max-w-4xl px-6 py-8">
              <Outlet />

              {/* Footer */}
              <footer className="mt-12 pt-8 border-t border-gray-200">
                <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                  <p className="text-sm text-gray-500">
                    Last updated: December 2024 &middot; Version 1.0
                  </p>
                  <div className="flex items-center gap-4 text-sm">
                    <a
                      href="https://github.com/xenonclinic/docs/issues"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      Report an issue
                    </a>
                    <a
                      href="/contact"
                      className="text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      Contact support
                    </a>
                  </div>
                </div>
              </footer>
            </div>

            {/* Table of Contents */}
            <div className="hidden xl:block w-64 flex-shrink-0">
              <div className="sticky top-24 px-4 py-8">
                <TableOfContents />
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}

export default DocsLayout;
