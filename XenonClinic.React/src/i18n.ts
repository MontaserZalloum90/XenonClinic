import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import translation files
import enTranslation from './locales/en/translation.json';
import arTranslation from './locales/ar/translation.json';

const resources = {
  en: {
    translation: enTranslation,
  },
  ar: {
    translation: arTranslation,
  },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    debug: import.meta.env.DEV,

    interpolation: {
      escapeValue: false, // React already escapes values
    },

    detection: {
      order: ['localStorage', 'cookie', 'navigator', 'htmlTag'],
      caches: ['localStorage', 'cookie'],
      lookupLocalStorage: 'xenonclinic-language',
      lookupCookie: 'xenonclinic-language',
    },
  });

// Set document direction based on language
i18n.on('languageChanged', (lng) => {
  const dir = lng === 'ar' ? 'rtl' : 'ltr';
  document.documentElement.dir = dir;
  document.documentElement.lang = lng;

  // Add/remove RTL class for Tailwind
  if (lng === 'ar') {
    document.documentElement.classList.add('rtl');
  } else {
    document.documentElement.classList.remove('rtl');
  }
});

// Initialize direction on load
const initialDir = i18n.language === 'ar' ? 'rtl' : 'ltr';
document.documentElement.dir = initialDir;
document.documentElement.lang = i18n.language || 'en';
if (i18n.language === 'ar') {
  document.documentElement.classList.add('rtl');
}

export default i18n;
