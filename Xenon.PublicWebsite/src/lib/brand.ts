/**
 * XENON Brand Configuration
 */

export const brand = {
  name: 'XENON',
  tagline: 'Configurable ERP & Clinic CRM for Gulf SMB',
  description: 'Multi-tenant, multi-branch, configurable business management platform',

  logo: {
    primary: '/brand/logo.svg',
    light: '/brand/logo-light.svg',
    dark: '/brand/logo-dark.svg',
    icon: '/brand/icon.svg',
  },

  colors: {
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
  },

  contact: {
    email: 'hello@xenon.ae',
    sales: 'sales@xenon.ae',
    support: 'support@xenon.ae',
    phone: '+971 4 234 5678',
  },

  legal: {
    company: 'XENON Software LLC',
    address: 'Dubai, United Arab Emirates',
  },
} as const;

export type BrandConfig = typeof brand;
export default brand;
