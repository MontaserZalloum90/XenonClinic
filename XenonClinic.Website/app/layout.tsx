import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { brand } from '@/lib/brand';

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-inter',
});

export const metadata: Metadata = {
  title: {
    default: `${brand.name} - ${brand.tagline}`,
    template: `%s | ${brand.name}`,
  },
  description: brand.description,
  keywords: [
    'ERP',
    'CRM',
    'Clinic Management',
    'Healthcare Software',
    'Trading Software',
    'Gulf SMB',
    'UAE',
    'Saudi Arabia',
    'Medical Practice',
    'Patient Management',
    'Inventory',
    'Billing',
    'Multi-tenant SaaS',
  ],
  authors: [{ name: brand.name }],
  creator: brand.name,
  publisher: brand.name,
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  metadataBase: new URL('https://xenon.ae'),
  alternates: {
    canonical: '/',
  },
  openGraph: {
    title: `${brand.name} - ${brand.tagline}`,
    description: brand.description,
    url: 'https://xenon.ae',
    siteName: brand.name,
    locale: 'en_US',
    type: 'website',
    images: [
      {
        url: '/og-image.png',
        width: 1200,
        height: 630,
        alt: brand.name,
      },
    ],
  },
  twitter: {
    card: 'summary_large_image',
    title: `${brand.name} - ${brand.tagline}`,
    description: brand.description,
    images: ['/og-image.png'],
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      'max-video-preview': -1,
      'max-image-preview': 'large',
      'max-snippet': -1,
    },
  },
  icons: {
    icon: '/favicon.ico',
    shortcut: '/favicon-16x16.png',
    apple: '/apple-touch-icon.png',
  },
  manifest: '/site.webmanifest',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={inter.variable} suppressHydrationWarning>
      <body className="min-h-screen bg-background font-sans antialiased">
        {children}
      </body>
    </html>
  );
}
