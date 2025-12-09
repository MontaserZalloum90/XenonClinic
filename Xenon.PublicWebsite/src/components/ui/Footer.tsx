import { Link } from 'react-router-dom';
import { brand } from '@/lib/brand';
import { Mail, Phone, MapPin, Linkedin, Twitter } from 'lucide-react';

const footerNavigation = {
  product: [
    { name: 'Features', href: '/features' },
    { name: 'Pricing', href: '/pricing' },
  ],
  solutions: [
    { name: 'Audiology Clinics', href: '/features' },
    { name: 'Dental Practices', href: '/features' },
    { name: 'Trading Companies', href: '/features' },
  ],
  company: [
    { name: 'About', href: '/about' },
    { name: 'Contact', href: '/contact' },
  ],
  legal: [
    { name: 'Privacy Policy', href: '/privacy' },
    { name: 'Terms of Service', href: '/terms' },
  ],
};

export function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-slate-900 text-gray-300" aria-labelledby="footer-heading">
      <h2 id="footer-heading" className="sr-only">
        Footer
      </h2>

      <div className="container-marketing py-12 lg:py-16">
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-8 lg:gap-12">
          {/* Brand column */}
          <div className="col-span-2 md:col-span-3 lg:col-span-2">
            <Link to="/" className="flex items-center gap-2">
              <div className="h-8 w-8 rounded-lg bg-primary-600 flex items-center justify-center">
                <span className="text-white font-bold text-lg">X</span>
              </div>
              <span className="font-bold text-xl text-white">{brand.name}</span>
            </Link>
            <p className="mt-4 text-sm text-gray-400 max-w-xs">
              {brand.tagline}. Serving healthcare providers and trading companies across the Gulf region.
            </p>

            {/* Contact info */}
            <div className="mt-6 space-y-2">
              <a
                href={`mailto:${brand.contact.email}`}
                className="flex items-center gap-2 text-sm hover:text-white transition-colors"
              >
                <Mail className="h-4 w-4" />
                {brand.contact.email}
              </a>
              <a
                href={`tel:${brand.contact.phone.replace(/\s/g, '')}`}
                className="flex items-center gap-2 text-sm hover:text-white transition-colors"
              >
                <Phone className="h-4 w-4" />
                {brand.contact.phone}
              </a>
              <div className="flex items-start gap-2 text-sm">
                <MapPin className="h-4 w-4 mt-0.5 flex-shrink-0" />
                <span>{brand.legal.address}</span>
              </div>
            </div>

            {/* Social links */}
            <div className="mt-6 flex gap-4">
              <a
                href="https://linkedin.com/company/xenon"
                target="_blank"
                rel="noopener noreferrer"
                className="p-2 rounded-lg bg-slate-800 hover:bg-slate-700 transition-colors"
                aria-label="LinkedIn"
              >
                <Linkedin className="h-4 w-4" />
              </a>
              <a
                href="https://twitter.com/xenon_ae"
                target="_blank"
                rel="noopener noreferrer"
                className="p-2 rounded-lg bg-slate-800 hover:bg-slate-700 transition-colors"
                aria-label="Twitter"
              >
                <Twitter className="h-4 w-4" />
              </a>
            </div>
          </div>

          {/* Product links */}
          <div>
            <h3 className="text-sm font-semibold text-white">Product</h3>
            <ul className="mt-4 space-y-2">
              {footerNavigation.product.map((item) => (
                <li key={item.name}>
                  <Link to={item.href} className="text-sm hover:text-white transition-colors">
                    {item.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Solutions links */}
          <div>
            <h3 className="text-sm font-semibold text-white">Solutions</h3>
            <ul className="mt-4 space-y-2">
              {footerNavigation.solutions.map((item) => (
                <li key={item.name}>
                  <Link to={item.href} className="text-sm hover:text-white transition-colors">
                    {item.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Company links */}
          <div>
            <h3 className="text-sm font-semibold text-white">Company</h3>
            <ul className="mt-4 space-y-2">
              {footerNavigation.company.map((item) => (
                <li key={item.name}>
                  <Link to={item.href} className="text-sm hover:text-white transition-colors">
                    {item.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* Bottom section */}
        <div className="mt-12 pt-8 border-t border-slate-800">
          <div className="flex flex-col md:flex-row items-center justify-between gap-4">
            <div className="flex flex-wrap justify-center gap-4 text-sm">
              {footerNavigation.legal.map((item) => (
                <Link key={item.name} to={item.href} className="hover:text-white transition-colors">
                  {item.name}
                </Link>
              ))}
            </div>
            <p className="text-sm text-gray-500">
              &copy; {currentYear} {brand.name}. All rights reserved.
            </p>
          </div>
        </div>
      </div>
    </footer>
  );
}

export default Footer;
