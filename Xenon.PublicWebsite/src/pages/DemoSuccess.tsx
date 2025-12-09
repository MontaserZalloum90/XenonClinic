import { Link } from 'react-router-dom';
import { CheckCircle, ArrowRight } from 'lucide-react';
import { brand } from '@/lib/brand';

export default function DemoSuccessPage() {
  return (
    <div className="min-h-[calc(100vh-4rem)] bg-gradient-to-b from-gray-50 to-white flex items-center">
      <div className="container-marketing py-12 md:py-20">
        <div className="max-w-lg mx-auto text-center">
          <div className="h-20 w-20 rounded-full bg-green-100 text-green-600 flex items-center justify-center mx-auto mb-8">
            <CheckCircle className="h-10 w-10" />
          </div>
          <h1 className="heading-2 text-gray-900 mb-4">You're all set!</h1>
          <p className="text-lg text-gray-600 mb-8">
            Thank you for signing up for {brand.name}. We've sent you an email with instructions to
            access your trial account.
          </p>

          <div className="card text-left mb-8">
            <h3 className="font-semibold text-gray-900 mb-4">What's next?</h3>
            <ul className="space-y-3 text-sm text-gray-600">
              <li className="flex items-start gap-3">
                <span className="h-6 w-6 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
                  1
                </span>
                <span>Check your email for login credentials</span>
              </li>
              <li className="flex items-start gap-3">
                <span className="h-6 w-6 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
                  2
                </span>
                <span>Log in to your {brand.name} dashboard</span>
              </li>
              <li className="flex items-start gap-3">
                <span className="h-6 w-6 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
                  3
                </span>
                <span>Start adding your data and exploring features</span>
              </li>
            </ul>
          </div>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/login" className="btn-primary">
              Go to Login
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link to="/" className="btn-secondary">
              Back to Home
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
