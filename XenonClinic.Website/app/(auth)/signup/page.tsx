import type { Metadata } from 'next';
import Link from 'next/link';
import SignupWizard from '@/components/signup/SignupWizard';

export const metadata: Metadata = {
  title: 'Start Free Trial',
  description: 'Start your 30-day free trial of XENON. No credit card required.',
};

export default function SignupPage() {
  return (
    <div className="w-full max-w-2xl px-4 py-8">
      <div className="text-center mb-8">
        <Link href="/" className="inline-block mb-6">
          <span className="text-2xl font-bold text-primary-600">XENON</span>
        </Link>
        <h1 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
          Start Your Free Trial
        </h1>
        <p className="text-gray-600">
          30 days free. No credit card required.
        </p>
      </div>

      <SignupWizard />

      <p className="mt-8 text-center text-sm text-gray-600">
        Already have an account?{' '}
        <Link href="/login" className="text-primary-600 font-medium hover:underline">
          Sign in
        </Link>
      </p>
    </div>
  );
}
