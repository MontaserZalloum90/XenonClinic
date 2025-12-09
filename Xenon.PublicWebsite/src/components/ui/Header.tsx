import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { brand } from '@/lib/brand';
import { Menu, X, ChevronDown } from 'lucide-react';

const navigation = {
  main: [
    { name: 'Product', href: '/product' },
    {
      name: 'Features',
      href: '/features',
      children: [
        { name: 'All Features', href: '/features' },
        { name: 'Patient Management', href: '/features/patient-management' },
        { name: 'Appointments', href: '/features/appointments' },
        { name: 'Billing & Invoicing', href: '/features/billing' },
        { name: 'Inventory', href: '/features/inventory' },
        { name: 'Analytics', href: '/features/analytics' },
        { name: 'Multi-Branch', href: '/features/multi-branch' },
      ],
    },
    { name: 'Pricing', href: '/pricing' },
    {
      name: 'Company',
      href: '/about',
      children: [
        { name: 'About Us', href: '/about' },
        { name: 'Contact', href: '/contact' },
      ],
    },
  ],
};

export function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const location = useLocation();
  const pathname = location.pathname;

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 10);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    setMobileMenuOpen(false);
    setActiveDropdown(null);
  }, [pathname]);

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        scrolled ? 'bg-white/95 backdrop-blur-sm shadow-sm' : 'bg-transparent'
      }`}
    >
      <nav className="container-marketing" aria-label="Main navigation">
        <div className="flex h-16 items-center justify-between">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <div className="h-8 w-8 rounded-lg bg-primary-600 flex items-center justify-center">
              <span className="text-white font-bold text-lg">X</span>
            </div>
            <span className="font-bold text-xl text-gray-900">{brand.name}</span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden lg:flex lg:items-center lg:gap-1">
            {navigation.main.map((item) => (
              <div
                key={item.name}
                className="relative"
                onMouseEnter={() => item.children && setActiveDropdown(item.name)}
                onMouseLeave={() => setActiveDropdown(null)}
              >
                {item.children ? (
                  <>
                    <button
                      className={`flex items-center gap-1 px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
                        pathname.startsWith(item.href)
                          ? 'text-primary-600'
                          : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100'
                      }`}
                    >
                      {item.name}
                      <ChevronDown className="h-4 w-4" />
                    </button>
                    {activeDropdown === item.name && (
                      <div className="absolute top-full left-0 mt-1 w-56 rounded-xl bg-white shadow-lg ring-1 ring-gray-100 py-2">
                        {item.children.map((child) => (
                          <Link
                            key={child.href}
                            to={child.href}
                            className={`block px-4 py-2 text-sm transition-colors ${
                              pathname === child.href
                                ? 'bg-primary-50 text-primary-600'
                                : 'text-gray-700 hover:bg-gray-50'
                            }`}
                          >
                            {child.name}
                          </Link>
                        ))}
                      </div>
                    )}
                  </>
                ) : (
                  <Link
                    to={item.href}
                    className={`px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
                      pathname === item.href
                        ? 'text-primary-600'
                        : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100'
                    }`}
                  >
                    {item.name}
                  </Link>
                )}
              </div>
            ))}
          </div>

          {/* CTA Buttons */}
          <div className="hidden lg:flex lg:items-center lg:gap-3">
            <Link
              to="/login"
              className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900 transition-colors"
            >
              Sign in
            </Link>
            <Link to="/demo" className="btn-primary">
              Get Started Free
            </Link>
          </div>

          {/* Mobile menu button */}
          <button
            type="button"
            className="lg:hidden p-2 rounded-lg text-gray-700 hover:bg-gray-100"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            aria-expanded={mobileMenuOpen}
            aria-controls="mobile-menu"
          >
            <span className="sr-only">Toggle menu</span>
            {mobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
          </button>
        </div>

        {/* Mobile menu */}
        {mobileMenuOpen && (
          <div id="mobile-menu" className="lg:hidden py-4 border-t">
            <div className="space-y-1">
              {navigation.main.map((item) => (
                <div key={item.name}>
                  {item.children ? (
                    <>
                      <button
                        onClick={() =>
                          setActiveDropdown(activeDropdown === item.name ? null : item.name)
                        }
                        className="flex w-full items-center justify-between px-3 py-2 text-base font-medium text-gray-700 rounded-lg hover:bg-gray-100"
                      >
                        {item.name}
                        <ChevronDown
                          className={`h-4 w-4 transition-transform ${
                            activeDropdown === item.name ? 'rotate-180' : ''
                          }`}
                        />
                      </button>
                      {activeDropdown === item.name && (
                        <div className="pl-4 space-y-1 mt-1">
                          {item.children.map((child) => (
                            <Link
                              key={child.href}
                              to={child.href}
                              className={`block px-3 py-2 text-sm rounded-lg ${
                                pathname === child.href
                                  ? 'bg-primary-50 text-primary-600'
                                  : 'text-gray-600 hover:bg-gray-50'
                              }`}
                            >
                              {child.name}
                            </Link>
                          ))}
                        </div>
                      )}
                    </>
                  ) : (
                    <Link
                      to={item.href}
                      className={`block px-3 py-2 text-base font-medium rounded-lg ${
                        pathname === item.href
                          ? 'bg-primary-50 text-primary-600'
                          : 'text-gray-700 hover:bg-gray-100'
                      }`}
                    >
                      {item.name}
                    </Link>
                  )}
                </div>
              ))}
            </div>
            <div className="mt-4 pt-4 border-t space-y-2">
              <Link
                to="/login"
                className="block w-full px-3 py-2 text-center text-base font-medium text-gray-700 rounded-lg hover:bg-gray-100"
              >
                Sign in
              </Link>
              <Link to="/demo" className="block w-full btn-primary text-center">
                Get Started Free
              </Link>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}

export default Header;
