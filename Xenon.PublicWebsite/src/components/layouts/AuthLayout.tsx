import { Outlet, Link } from 'react-router-dom';
import { brand } from '@/lib/brand';

export function AuthLayout() {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <Link to="/" className="flex justify-center items-center gap-2">
          <div className="h-10 w-10 rounded-lg bg-primary-600 flex items-center justify-center">
            <span className="text-white font-bold text-xl">X</span>
          </div>
          <span className="font-bold text-2xl text-gray-900">{brand.name}</span>
        </Link>
      </div>
      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <Outlet />
      </div>
    </div>
  );
}

export default AuthLayout;
