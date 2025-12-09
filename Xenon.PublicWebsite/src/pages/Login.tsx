import { useState } from 'react';
import { Link } from 'react-router-dom';
import { AlertCircle } from 'lucide-react';
import { login, setAuthToken } from '@/lib/platform-api';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await login({ email, password });

      if (response.success && response.data) {
        setAuthToken(response.data.token);
        localStorage.setItem('tenant_info', JSON.stringify(response.data.tenant));
        localStorage.setItem('user_info', JSON.stringify(response.data.user));

        // Redirect to ERP app
        window.location.href = import.meta.env.VITE_ERP_URL || 'http://localhost:5173';
      } else {
        setError(response.error || 'Invalid email or password');
      }
    } catch {
      setError('Network error. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-white py-8 px-4 shadow-lg rounded-xl sm:px-10">
      <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">Sign in to your account</h2>

      {error && (
        <div className="mb-4 p-3 rounded-lg bg-red-50 border border-red-200 flex items-center gap-2 text-sm text-red-700">
          <AlertCircle className="h-4 w-4" />
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
            Email address
          </label>
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="input mt-1"
            placeholder="you@company.com"
          />
        </div>

        <div>
          <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            Password
          </label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="input mt-1"
            placeholder="Enter your password"
          />
        </div>

        <div className="flex items-center justify-between">
          <div className="flex items-center">
            <input
              id="remember-me"
              name="remember-me"
              type="checkbox"
              className="h-4 w-4 text-primary-600 border-gray-300 rounded"
            />
            <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-700">
              Remember me
            </label>
          </div>
          <Link to="/forgot-password" className="text-sm link">
            Forgot password?
          </Link>
        </div>

        <button type="submit" disabled={isLoading} className="btn-primary w-full">
          {isLoading ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      <div className="mt-6 text-center">
        <p className="text-sm text-gray-600">
          Don't have an account?{' '}
          <Link to="/demo" className="link">
            Start free trial
          </Link>
        </p>
      </div>
    </div>
  );
}
