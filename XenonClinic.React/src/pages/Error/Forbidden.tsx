import { Link } from 'react-router-dom';
import { ShieldExclamationIcon, HomeIcon, ArrowLeftIcon } from '@heroicons/react/24/outline';
import { useAuth } from '../../contexts/AuthContext';

export const Forbidden = () => {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center px-4">
      <div className="text-center">
        <div className="flex justify-center mb-6">
          <div className="bg-red-100 rounded-full p-4">
            <ShieldExclamationIcon className="h-16 w-16 text-red-600" />
          </div>
        </div>

        <h1 className="text-6xl font-bold text-gray-900 mb-4">403</h1>
        <h2 className="text-2xl font-semibold text-gray-700 mb-2">Access Forbidden</h2>
        <p className="text-gray-500 mb-4 max-w-md">
          Sorry, you don't have permission to access this page. This area is restricted
          to users with specific roles or permissions.
        </p>

        {user && (
          <div className="mb-8 p-4 bg-gray-100 rounded-lg inline-block">
            <p className="text-sm text-gray-600">
              Logged in as: <span className="font-medium">{user.fullName}</span>
            </p>
            <p className="text-xs text-gray-500 mt-1">
              Current roles: {user.roles.length > 0 ? user.roles.join(', ') : 'None assigned'}
            </p>
          </div>
        )}

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Link
            to="/"
            className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
          >
            <HomeIcon className="h-5 w-5 mr-2" />
            Go to Dashboard
          </Link>
          <button
            onClick={() => window.history.back()}
            className="inline-flex items-center justify-center px-6 py-3 border border-gray-300 text-base font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
          >
            <ArrowLeftIcon className="h-5 w-5 mr-2" />
            Go Back
          </button>
        </div>
      </div>

      <div className="mt-12 text-center">
        <p className="text-sm text-gray-400">
          If you need access to this area, please contact your administrator.
        </p>
      </div>
    </div>
  );
};
