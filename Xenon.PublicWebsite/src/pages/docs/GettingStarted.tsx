import { Link } from 'react-router-dom';
import { CheckCircle, ArrowRight, User, Settings, Key, Monitor } from 'lucide-react';

export default function GettingStarted() {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Getting Started</h1>
        <p className="text-lg text-gray-600">
          Welcome to XenonClinic! This guide will help you understand the platform
          and get started with your first tasks.
        </p>
      </div>

      {/* Prerequisites */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Prerequisites</h2>
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <ul className="space-y-2">
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-blue-600" />
              <span>Active XenonClinic account with assigned role</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-blue-600" />
              <span>Modern web browser (Chrome, Firefox, Safari, Edge)</span>
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-blue-600" />
              <span>Stable internet connection</span>
            </li>
          </ul>
        </div>
      </section>

      {/* First Login */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">First Login</h2>
        <div className="space-y-4">
          <div className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg">
            <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold">
              1
            </div>
            <div>
              <h3 className="font-medium text-gray-900">Navigate to Login Page</h3>
              <p className="text-gray-600 mt-1">
                Open your browser and go to your organization's XenonClinic URL
                (e.g., <code className="bg-gray-100 px-1 rounded">yourcompany.xenonclinic.com</code>).
              </p>
            </div>
          </div>

          <div className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg">
            <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold">
              2
            </div>
            <div>
              <h3 className="font-medium text-gray-900">Enter Credentials</h3>
              <p className="text-gray-600 mt-1">
                Enter your username and temporary password provided by your administrator.
                You'll be prompted to change your password on first login.
              </p>
            </div>
          </div>

          <div className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg">
            <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold">
              3
            </div>
            <div>
              <h3 className="font-medium text-gray-900">Set Up MFA (if required)</h3>
              <p className="text-gray-600 mt-1">
                If your organization requires multi-factor authentication, follow the
                prompts to set up your authenticator app or SMS verification.
              </p>
            </div>
          </div>

          <div className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg">
            <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold">
              4
            </div>
            <div>
              <h3 className="font-medium text-gray-900">Access Dashboard</h3>
              <p className="text-gray-600 mt-1">
                After successful login, you'll be directed to your personalized dashboard
                showing relevant information based on your role.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Understanding the Interface */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Understanding the Interface</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <div className="p-4 bg-white border border-gray-200 rounded-lg">
            <Monitor className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-medium text-gray-900">Dashboard</h3>
            <p className="text-sm text-gray-600 mt-1">
              Your home screen with key metrics, pending tasks, and quick access to
              frequently used features.
            </p>
          </div>
          <div className="p-4 bg-white border border-gray-200 rounded-lg">
            <Settings className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-medium text-gray-900">Navigation Menu</h3>
            <p className="text-sm text-gray-600 mt-1">
              Left sidebar with access to all modules. Menu items are filtered based
              on your role permissions.
            </p>
          </div>
          <div className="p-4 bg-white border border-gray-200 rounded-lg">
            <User className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-medium text-gray-900">User Profile</h3>
            <p className="text-sm text-gray-600 mt-1">
              Top-right corner contains your profile, settings, notifications, and
              logout options.
            </p>
          </div>
          <div className="p-4 bg-white border border-gray-200 rounded-lg">
            <Key className="h-6 w-6 text-primary-600 mb-3" />
            <h3 className="font-medium text-gray-900">Quick Actions</h3>
            <p className="text-sm text-gray-600 mt-1">
              Context-sensitive action buttons appear at the top of each module for
              creating new records.
            </p>
          </div>
        </div>
      </section>

      {/* Key Concepts */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Key Concepts</h2>
        <div className="space-y-4">
          <div className="p-4 bg-gray-50 rounded-lg">
            <h3 className="font-medium text-gray-900">Multi-Tenancy</h3>
            <p className="text-gray-600 mt-1">
              XenonClinic supports multiple organizations (tenants) with isolated data.
              Each tenant can have multiple companies and branches.
            </p>
          </div>
          <div className="p-4 bg-gray-50 rounded-lg">
            <h3 className="font-medium text-gray-900">Role-Based Access Control (RBAC)</h3>
            <p className="text-gray-600 mt-1">
              Access to features and data is controlled by roles. Your role determines
              what modules you can access and what actions you can perform.
            </p>
          </div>
          <div className="p-4 bg-gray-50 rounded-lg">
            <h3 className="font-medium text-gray-900">Branches</h3>
            <p className="text-gray-600 mt-1">
              Physical locations or departments within your organization. Users can be
              assigned to one or more branches.
            </p>
          </div>
          <div className="p-4 bg-gray-50 rounded-lg">
            <h3 className="font-medium text-gray-900">Audit Trail</h3>
            <p className="text-gray-600 mt-1">
              All actions are logged for compliance and security. View audit logs to
              track who did what and when.
            </p>
          </div>
        </div>
      </section>

      {/* Next Steps */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Next Steps</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <Link
            to="/docs/personas"
            className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all group"
          >
            <div>
              <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                Find Your Persona
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                Explore documentation specific to your role
              </p>
            </div>
            <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
          </Link>
          <Link
            to="/docs/modules"
            className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all group"
          >
            <div>
              <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                Explore Modules
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                Learn about available product features
              </p>
            </div>
            <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
          </Link>
          <Link
            to="/docs/journeys/patient-registration"
            className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all group"
          >
            <div>
              <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                Try a Journey
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                Follow a step-by-step workflow guide
              </p>
            </div>
            <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
          </Link>
          <Link
            to="/docs/faq-troubleshooting"
            className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all group"
          >
            <div>
              <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                Get Help
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                Find answers to common questions
              </p>
            </div>
            <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
          </Link>
        </div>
      </section>
    </div>
  );
}
