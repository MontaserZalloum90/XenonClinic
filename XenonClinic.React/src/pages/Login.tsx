import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../contexts/AuthContext';
import { LanguageSwitcher } from '../components/LanguageSwitcher';

export const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  const isRTL = i18n.language === 'ar';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await login({ username, password });
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={`min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-primary-100 px-4 ${isRTL ? 'font-arabic' : 'font-sans'}`}>
      {/* Language Switcher - Top Right */}
      <div className="absolute top-4 end-4">
        <LanguageSwitcher />
      </div>

      <div className="max-w-md w-full">
        {/* Logo/Header */}
        <div className="text-center mb-8">
          <div className="flex items-center justify-center mb-4">
            <div className="w-16 h-16 bg-primary-600 rounded-lg flex items-center justify-center">
              <svg
                className="w-10 h-10 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z"
                />
              </svg>
            </div>
          </div>
          <h1 className="text-3xl font-bold text-gray-900">{t('common.appName')}</h1>
          <p className="text-gray-600 mt-2">{t('auth.loginSubtitle')}</p>
        </div>

        {/* Login Form */}
        <div className="bg-white rounded-lg shadow-xl p-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">{t('auth.loginTitle')}</h2>

          {error && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-600">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="username" className="block text-sm font-medium text-gray-700 mb-1">
                {t('auth.username')}
              </label>
              <input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                placeholder={t('auth.username')}
                disabled={isLoading}
                dir={isRTL ? 'rtl' : 'ltr'}
              />
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                {t('auth.password')}
              </label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                placeholder={t('auth.password')}
                disabled={isLoading}
                dir="ltr"
              />
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="remember"
                  type="checkbox"
                  className={`h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded`}
                />
                <label htmlFor="remember" className={`${isRTL ? 'mr-2' : 'ml-2'} block text-sm text-gray-700`}>
                  {t('auth.rememberMe')}
                </label>
              </div>

              <a href="#" className="text-sm text-primary-600 hover:text-primary-500">
                {t('auth.forgotPassword')}
              </a>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full btn btn-primary py-3 text-base font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <div className="flex items-center justify-center">
                  <div className={`animate-spin rounded-full h-5 w-5 border-b-2 border-white ${isRTL ? 'ml-2' : 'mr-2'}`}></div>
                  {t('common.loading')}
                </div>
              ) : (
                t('auth.loginButton')
              )}
            </button>
          </form>

          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              {t('auth.register')}{' '}
              <a href="/register" className="text-primary-600 hover:text-primary-500 font-medium">
                {t('auth.register')}
              </a>
            </p>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-8 text-center text-sm text-gray-600">
          <p>&copy; 2024 {t('common.appName')}. All rights reserved.</p>
        </div>
      </div>
    </div>
  );
};
