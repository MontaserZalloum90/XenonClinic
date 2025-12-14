import { Link } from 'react-router-dom';
import { CheckCircle, ArrowRight, User, Settings, Key, Monitor, Database, AlertTriangle, Play, RefreshCw, Trash2 } from 'lucide-react';

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

      {/* Sample Data Loading */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Sample Data Loading</h2>
        <p className="text-gray-600 mb-4">
          XenonClinic provides comprehensive sample data to help you explore the system, train users,
          and test workflows without affecting production data. This is especially useful for new
          implementations and demonstration environments.
        </p>

        {/* Warning Box */}
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <AlertTriangle className="h-5 w-5 text-amber-600 flex-shrink-0 mt-0.5" />
            <div>
              <h4 className="font-medium text-amber-800">Important Notice</h4>
              <p className="text-amber-700 text-sm mt-1">
                Sample data loading is only available in sandbox/demo environments. Never load sample
                data in production systems. Sample data cannot be selectively removed - a full reset
                is required.
              </p>
            </div>
          </div>
        </div>

        {/* How to Load Sample Data */}
        <div className="bg-white border border-gray-200 rounded-lg p-6 mb-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4 flex items-center gap-2">
            <Play className="h-5 w-5 text-primary-600" />
            How to Load Sample Data
          </h3>
          <ol className="space-y-3 text-gray-600">
            <li className="flex gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-medium">1</span>
              <span>Navigate to <code className="bg-gray-100 px-2 py-0.5 rounded text-sm">Administration → System Settings → Sample Data</code></span>
            </li>
            <li className="flex gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-medium">2</span>
              <span>Review the data categories and select which modules to populate</span>
            </li>
            <li className="flex gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-medium">3</span>
              <span>Click "Load Sample Data" and confirm the action</span>
            </li>
            <li className="flex gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-medium">4</span>
              <span>Wait for the process to complete (typically 2-5 minutes depending on selection)</span>
            </li>
            <li className="flex gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-medium">5</span>
              <span>A summary report will display showing records created in each module</span>
            </li>
          </ol>
        </div>

        {/* Available Sample Data */}
        <div className="space-y-4 mb-6">
          <h3 className="text-lg font-medium text-gray-900 flex items-center gap-2">
            <Database className="h-5 w-5 text-primary-600" />
            Available Sample Data Categories
          </h3>

          <div className="grid md:grid-cols-2 gap-4">
            {/* Core Data */}
            <div className="bg-white border border-gray-200 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 text-primary-700">Core Master Data</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>50+ Patients</strong> - Diverse demographics, insurance types, medical histories</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>20+ Providers</strong> - Doctors across multiple specialties</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>10+ Staff Members</strong> - Nurses, receptionists, technicians</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>3 Branches</strong> - Main clinic, satellite, and specialized center</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>8 Departments</strong> - General, specialty, and support departments</span>
                </li>
              </ul>
            </div>

            {/* Clinical Data */}
            <div className="bg-white border border-gray-200 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 text-blue-700">Clinical Data</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>200+ Appointments</strong> - Past, current, and scheduled future visits</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>150+ Clinical Visits</strong> - Complete with vitals, diagnoses, notes</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>300+ Lab Orders</strong> - Various test types with results</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>100+ Imaging Studies</strong> - X-ray, ultrasound, CT samples</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>250+ Prescriptions</strong> - With dispensing records</span>
                </li>
              </ul>
            </div>

            {/* Financial Data */}
            <div className="bg-white border border-gray-200 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 text-green-700">Financial Data</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>500+ Invoices</strong> - Paid, pending, and partial payments</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>200+ Insurance Claims</strong> - Submitted, approved, denied samples</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>Price Lists</strong> - Standard, insurance, and VIP pricing</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>5 Insurance Providers</strong> - With contract configurations</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>AR Aging Data</strong> - 30/60/90+ day aging scenarios</span>
                </li>
              </ul>
            </div>

            {/* Operational Data */}
            <div className="bg-white border border-gray-200 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 text-purple-700">Operational Data</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>1,000+ Inventory Items</strong> - Medications, supplies, consumables</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>50+ Vendors</strong> - Pharmaceutical and supply vendors</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>100+ Purchase Orders</strong> - Complete procurement cycle</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>30+ Employees</strong> - With attendance and leave records</span>
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="h-4 w-4 text-green-500" />
                  <span><strong>Marketing Campaigns</strong> - Email and SMS campaign samples</span>
                </li>
              </ul>
            </div>
          </div>
        </div>

        {/* Sample Data Management */}
        <div className="grid md:grid-cols-2 gap-4">
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-3 flex items-center gap-2">
              <RefreshCw className="h-5 w-5 text-blue-600" />
              Refreshing Sample Data
            </h4>
            <p className="text-sm text-gray-600 mb-2">
              To refresh sample data with new records or reset to original state:
            </p>
            <ul className="text-sm text-gray-600 space-y-1">
              <li>• Navigate to System Settings → Sample Data</li>
              <li>• Click "Reset Sample Data"</li>
              <li>• Confirm to clear existing and reload fresh data</li>
              <li>• All sample-flagged records will be replaced</li>
            </ul>
          </div>

          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-3 flex items-center gap-2">
              <Trash2 className="h-5 w-5 text-red-600" />
              Removing Sample Data
            </h4>
            <p className="text-sm text-gray-600 mb-2">
              To completely remove all sample data from your environment:
            </p>
            <ul className="text-sm text-gray-600 space-y-1">
              <li>• Navigate to System Settings → Sample Data</li>
              <li>• Click "Remove All Sample Data"</li>
              <li>• Confirm the action (requires admin password)</li>
              <li>• Only sample-flagged records are removed</li>
            </ul>
          </div>
        </div>

        {/* Tips for Demo Environments */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mt-6">
          <h4 className="font-medium text-blue-800 mb-2">Tips for Demo Environments</h4>
          <ul className="text-sm text-blue-700 space-y-1">
            <li>• Use sample data to demonstrate end-to-end workflows to stakeholders</li>
            <li>• Sample patients have diverse scenarios for testing edge cases</li>
            <li>• Historical data enables analytics and reporting demonstrations</li>
            <li>• Insurance claim samples cover approval, denial, and pending states</li>
            <li>• Lab results include normal, abnormal, and critical value examples</li>
            <li>• Schedule a daily refresh to keep demo data consistent</li>
          </ul>
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
