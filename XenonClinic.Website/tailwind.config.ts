import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Brand primary - deep blue
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
          DEFAULT: '#2451E0',
        },
        // Brand secondary - teal/healthcare
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
          DEFAULT: '#05C4AE',
        },
        // Brand accent - vibrant orange
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
          DEFAULT: '#F97316',
        },
      },
      fontFamily: {
        sans: [
          'system-ui',
          '-apple-system',
          'BlinkMacSystemFont',
          'Segoe UI',
          'Roboto',
          'Helvetica Neue',
          'Arial',
          'sans-serif',
        ],
        heading: [
          'system-ui',
          '-apple-system',
          'BlinkMacSystemFont',
          'Segoe UI',
          'Roboto',
          'Helvetica Neue',
          'Arial',
          'sans-serif',
        ],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'monospace'],
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-hero': 'linear-gradient(135deg, #2451E0 0%, #1A3FC7 50%, #141F4D 100%)',
        'gradient-cta': 'linear-gradient(135deg, #05C4AE 0%, #009E8F 100%)',
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-out forwards',
        'slide-up': 'slideUp 0.5s ease-out forwards',
        'slide-in-right': 'slideInRight 0.5s ease-out forwards',
        'pulse-slow': 'pulse 4s cubic-bezier(0.4, 0, 0.6, 1) infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideInRight: {
          '0%': { opacity: '0', transform: 'translateX(20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
      },
    },
  },
  plugins: [],
};

export default config;
