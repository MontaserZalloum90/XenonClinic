/**
 * XENON Brand Configuration
 *
 * All brand colors, tokens, and assets are defined here.
 * Colors are derived from the XENON logo design.
 */

export const brand = {
  name: 'XENON',
  tagline: 'Configurable ERP & Clinic CRM for Gulf SMB',
  description: 'Multi-tenant, multi-branch, configurable business management platform',

  // Logo paths
  logo: {
    primary: '/brand/logo.svg',
    light: '/brand/logo-light.svg',
    dark: '/brand/logo-dark.svg',
    icon: '/brand/icon.svg',
  },

  // Brand colors - derived from logo
  // Primary: Deep blue representing trust, professionalism, technology
  // Secondary: Teal/cyan for healthcare, freshness, innovation
  // Accent: Vibrant blue for CTAs and highlights
  colors: {
    // Primary palette
    primary: {
      50: '#EEF4FF',
      100: '#DAE6FF',
      200: '#BDD3FF',
      300: '#90B8FF',
      400: '#5C92FF',
      500: '#3B6EF5',
      600: '#2451E0',
      700: '#1A3FC7',
      800: '#1A35A1',
      900: '#1B327F',
      950: '#141F4D',
    },
    // Secondary palette - teal/healthcare
    secondary: {
      50: '#EFFEFB',
      100: '#C8FFF4',
      200: '#91FFEA',
      300: '#52F5DC',
      400: '#1DE0C8',
      500: '#05C4AE',
      600: '#009E8F',
      700: '#057D73',
      800: '#0A635D',
      900: '#0D524D',
      950: '#003332',
    },
    // Accent - vibrant action color
    accent: {
      50: '#FFF7ED',
      100: '#FFEDD5',
      200: '#FED7AA',
      300: '#FDBA74',
      400: '#FB923C',
      500: '#F97316',
      600: '#EA580C',
      700: '#C2410C',
      800: '#9A3412',
      900: '#7C2D12',
      950: '#431407',
    },
    // Neutral palette
    neutral: {
      50: '#F8FAFC',
      100: '#F1F5F9',
      200: '#E2E8F0',
      300: '#CBD5E1',
      400: '#94A3B8',
      500: '#64748B',
      600: '#475569',
      700: '#334155',
      800: '#1E293B',
      900: '#0F172A',
      950: '#020617',
    },
    // Semantic colors
    success: '#10B981',
    warning: '#F59E0B',
    error: '#EF4444',
    info: '#3B82F6',
  },

  // CSS variables for theming
  cssVariables: {
    light: {
      '--brand-primary': '#2451E0',
      '--brand-primary-hover': '#1A3FC7',
      '--brand-secondary': '#05C4AE',
      '--brand-secondary-hover': '#009E8F',
      '--brand-accent': '#F97316',
      '--brand-accent-hover': '#EA580C',
      '--brand-bg': '#FFFFFF',
      '--brand-bg-alt': '#F8FAFC',
      '--brand-text': '#0F172A',
      '--brand-text-muted': '#64748B',
      '--brand-border': '#E2E8F0',
    },
    dark: {
      '--brand-primary': '#5C92FF',
      '--brand-primary-hover': '#3B6EF5',
      '--brand-secondary': '#1DE0C8',
      '--brand-secondary-hover': '#05C4AE',
      '--brand-accent': '#FB923C',
      '--brand-accent-hover': '#F97316',
      '--brand-bg': '#0F172A',
      '--brand-bg-alt': '#1E293B',
      '--brand-text': '#F8FAFC',
      '--brand-text-muted': '#94A3B8',
      '--brand-border': '#334155',
    },
  },

  // Typography
  fonts: {
    heading: 'Inter, system-ui, sans-serif',
    body: 'Inter, system-ui, sans-serif',
    mono: 'JetBrains Mono, monospace',
  },

  // Social links
  social: {
    linkedin: 'https://linkedin.com/company/xenon-software',
    twitter: 'https://twitter.com/xenonsoftware',
  },

  // Contact
  contact: {
    email: 'hello@xenon.ae',
    sales: 'sales@xenon.ae',
    support: 'support@xenon.ae',
    phone: '+971 4 XXX XXXX',
  },

  // Legal
  legal: {
    company: 'XENON Software LLC',
    address: 'Dubai, United Arab Emirates',
  },
} as const;

export type BrandColors = typeof brand.colors;
export type BrandConfig = typeof brand;

export default brand;
