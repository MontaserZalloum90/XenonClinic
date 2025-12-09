import type { Metadata } from 'next';
import Link from 'next/link';
import { CheckCircle, ArrowRight, Mail, Clock, Calendar } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Welcome to XENON',
  description: 'Your free trial has been created. Check your email to get started.',
};

export default function DemoSuccessPage() {
  return (
    <div className="min-h-[calc(100vh-4rem)] bg-gradient-to-b from-gray-50 to-white flex items-center">
      <div className="container-marketing py-12">
        <div className="max-w-2xl mx-auto text-center">
          <div className="h-20 w-20 rounded-full bg-green-100 text-green-600 flex items-center justify-center mx-auto mb-8">
            <CheckCircle className="h-10 w-10" />
          </div>

          <h1 className="heading-1 text-gray-900 mb-4">
            You're all set!
          </h1>
          <p className="text-lg text-gray-600 mb-8">
            Your 30-day free trial has been created. Check your email for login instructions.
          </p>

          {/* Next Steps */}
          <div className="card text-left mb-8">
            <h2 className="font-semibold text-gray-900 mb-4">What happens next?</h2>
            <div className="space-y-4">
              <div className="flex items-start gap-4">
                <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                  <Mail className="h-5 w-5" />
                </div>
                <div>
                  <div className="font-medium text-gray-900">Check your email</div>
                  <div className="text-sm text-gray-600">
                    We've sent you a verification email with your login credentials and a link to access your dashboard.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-4">
                <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                  <Clock className="h-5 w-5" />
                </div>
                <div>
                  <div className="font-medium text-gray-900">Complete setup</div>
                  <div className="text-sm text-gray-600">
                    Follow the quick setup wizard to configure your organization, add your first branch, and invite team members.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-4">
                <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                  <Calendar className="h-5 w-5" />
                </div>
                <div>
                  <div className="font-medium text-gray-900">Book a demo call (optional)</div>
                  <div className="text-sm text-gray-600">
                    Schedule a personalized walkthrough with our team to get the most out of XENON.
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/login" className="btn-primary btn-lg">
              Go to Dashboard
              <ArrowRight className="ml-2 h-5 w-5" />
            </Link>
            <Link href="/docs/getting-started" className="btn-secondary btn-lg">
              Read Getting Started Guide
            </Link>
          </div>

          <p className="text-sm text-gray-500 mt-8">
            Didn't receive the email?{' '}
            <button className="link">Resend verification</button>
            {' '}or{' '}
            <Link href="/contact" className="link">contact support</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
