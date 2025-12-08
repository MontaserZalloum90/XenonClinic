import { Fragment } from 'react';
import { Menu, Transition } from '@headlessui/react';
import { GlobeAltIcon, ChevronDownIcon } from '@heroicons/react/24/outline';
import { useTranslation } from 'react-i18next';

const languages = [
  { code: 'en', name: 'English', nativeName: 'English', flag: '🇺🇸' },
  { code: 'ar', name: 'Arabic', nativeName: 'العربية', flag: '🇦🇪' },
];

interface LanguageSwitcherProps {
  variant?: 'dropdown' | 'buttons';
  showFlag?: boolean;
  showNativeName?: boolean;
  className?: string;
}

export function LanguageSwitcher({
  variant = 'dropdown',
  showFlag = true,
  showNativeName = true,
  className = '',
}: LanguageSwitcherProps) {
  const { i18n, t } = useTranslation();

  const currentLanguage = languages.find((lang) => lang.code === i18n.language) || languages[0];

  const changeLanguage = (langCode: string) => {
    i18n.changeLanguage(langCode);
  };

  if (variant === 'buttons') {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        {languages.map((lang) => (
          <button
            key={lang.code}
            onClick={() => changeLanguage(lang.code)}
            className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
              i18n.language === lang.code
                ? 'bg-primary-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-gray-700 dark:text-gray-200 dark:hover:bg-gray-600'
            }`}
            aria-label={`Switch to ${lang.name}`}
          >
            {showFlag && <span className="me-1">{lang.flag}</span>}
            {showNativeName ? lang.nativeName : lang.name}
          </button>
        ))}
      </div>
    );
  }

  return (
    <Menu as="div" className={`relative inline-block text-left ${className}`}>
      <div>
        <Menu.Button className="inline-flex items-center justify-center w-full px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 dark:bg-gray-800 dark:text-gray-200 dark:border-gray-600 dark:hover:bg-gray-700">
          <GlobeAltIcon className="w-5 h-5 me-2 text-gray-400" aria-hidden="true" />
          {showFlag && <span className="me-1">{currentLanguage.flag}</span>}
          <span className="hidden sm:inline">
            {showNativeName ? currentLanguage.nativeName : currentLanguage.name}
          </span>
          <ChevronDownIcon className="w-4 h-4 ms-2 -me-1 text-gray-400" aria-hidden="true" />
        </Menu.Button>
      </div>

      <Transition
        as={Fragment}
        enter="transition ease-out duration-100"
        enterFrom="transform opacity-0 scale-95"
        enterTo="transform opacity-100 scale-100"
        leave="transition ease-in duration-75"
        leaveFrom="transform opacity-100 scale-100"
        leaveTo="transform opacity-0 scale-95"
      >
        <Menu.Items className="absolute end-0 z-50 mt-2 w-48 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none dark:bg-gray-800 dark:ring-gray-700">
          <div className="py-1">
            {languages.map((lang) => (
              <Menu.Item key={lang.code}>
                {({ active }) => (
                  <button
                    onClick={() => changeLanguage(lang.code)}
                    className={`${
                      active ? 'bg-gray-100 dark:bg-gray-700' : ''
                    } ${
                      i18n.language === lang.code
                        ? 'bg-primary-50 text-primary-700 dark:bg-primary-900/20 dark:text-primary-400'
                        : 'text-gray-700 dark:text-gray-200'
                    } group flex w-full items-center px-4 py-2 text-sm`}
                  >
                    {showFlag && <span className="me-3 text-lg">{lang.flag}</span>}
                    <span className="flex-1 text-start">{lang.nativeName}</span>
                    {i18n.language === lang.code && (
                      <svg
                        className="w-4 h-4 text-primary-600 dark:text-primary-400"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fillRule="evenodd"
                          d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                          clipRule="evenodd"
                        />
                      </svg>
                    )}
                  </button>
                )}
              </Menu.Item>
            ))}
          </div>
        </Menu.Items>
      </Transition>
    </Menu>
  );
}

export default LanguageSwitcher;
