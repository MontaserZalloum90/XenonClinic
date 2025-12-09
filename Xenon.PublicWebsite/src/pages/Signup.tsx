import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';

export default function SignupPage() {
  // Redirect to demo page which has the full signup wizard
  return (
    <div className="bg-white py-8 px-4 shadow-lg rounded-xl sm:px-10">
      <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">Create your account</h2>
      <p className="text-center text-gray-600 mb-6">
        Start your 30-day free trial. No credit card required.
      </p>

      <Link to="/demo" className="btn-primary w-full">
        Start Free Trial
        <ArrowRight className="ml-2 h-5 w-5" />
      </Link>

      <div className="mt-6 text-center">
        <p className="text-sm text-gray-600">
          Already have an account?{' '}
          <Link to="/login" className="link">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
