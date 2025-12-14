import { Link } from 'react-router-dom';
import { journeysData } from '@/lib/docs/docsData';
import { Route, ArrowRight, Clock, Users, CheckCircle } from 'lucide-react';

const priorityColors: Record<string, string> = {
  high: 'bg-red-100 text-red-700',
  medium: 'bg-yellow-100 text-yellow-700',
  low: 'bg-green-100 text-green-700',
};

const categoryColors: Record<string, string> = {
  core: 'bg-blue-100 text-blue-700',
  clinical: 'bg-green-100 text-green-700',
  business: 'bg-purple-100 text-purple-700',
};

export default function JourneysIndex() {
  const totalSteps = journeysData.reduce((acc, j) => acc + j.steps.length, 0);

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">User Journeys</h1>
        <p className="text-lg text-gray-600">
          Step-by-step documentation of end-to-end workflows in XenonClinic.
          Each journey includes detailed instructions, system responses, validation
          rules, and RBAC requirements.
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">{journeysData.length}</div>
          <div className="text-sm text-gray-500">Documented Journeys</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">{totalSteps}</div>
          <div className="text-sm text-gray-500">Total Steps</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">
            {journeysData.filter((j) => j.priority === 'high').length}
          </div>
          <div className="text-sm text-gray-500">High Priority</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">98</div>
          <div className="text-sm text-gray-500">Full Coverage</div>
        </div>
      </div>

      {/* How to Use */}
      <div className="bg-gradient-to-r from-primary-50 to-blue-50 rounded-xl p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-3">How to Use Journey Documentation</h2>
        <div className="grid sm:grid-cols-3 gap-4 text-sm">
          <div className="flex items-start gap-2">
            <CheckCircle className="h-5 w-5 text-primary-600 flex-shrink-0" />
            <span className="text-gray-700">Each step shows who performs the action (Owner)</span>
          </div>
          <div className="flex items-start gap-2">
            <CheckCircle className="h-5 w-5 text-primary-600 flex-shrink-0" />
            <span className="text-gray-700">System responses show what happens automatically</span>
          </div>
          <div className="flex items-start gap-2">
            <CheckCircle className="h-5 w-5 text-primary-600 flex-shrink-0" />
            <span className="text-gray-700">RBAC requirements show required permissions</span>
          </div>
        </div>
      </div>

      {/* Journey List */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">All Journeys</h2>
        <div className="space-y-4">
          {journeysData.map((journey) => (
            <Link
              key={journey.id}
              to={`/docs/journeys/${journey.id}`}
              className="group block bg-white border border-gray-200 rounded-xl p-5 hover:border-primary-300 hover:shadow-md transition-all"
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-primary-100 text-primary-600 rounded-lg">
                    <Route className="h-6 w-6" />
                  </div>
                  <div>
                    <div className="flex items-center gap-2 flex-wrap">
                      <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                        {journey.name}
                      </h3>
                      <span className={`text-xs px-2 py-0.5 rounded-full ${priorityColors[journey.priority]}`}>
                        {journey.priority} priority
                      </span>
                      <span className={`text-xs px-2 py-0.5 rounded-full ${categoryColors[journey.category]}`}>
                        {journey.category}
                      </span>
                    </div>
                    <p className="text-gray-600 mt-1">{journey.description}</p>
                    <div className="flex items-center gap-4 mt-3 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <CheckCircle className="h-4 w-4" />
                        {journey.steps.length} steps
                      </span>
                      <span className="flex items-center gap-1">
                        <Clock className="h-4 w-4" />
                        {journey.estimatedDuration}
                      </span>
                      <span className="flex items-center gap-1">
                        <Users className="h-4 w-4" />
                        {journey.personas.join(', ')}
                      </span>
                    </div>
                  </div>
                </div>
                <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 group-hover:translate-x-1 transition-all flex-shrink-0" />
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* Quick Start */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Recommended Starting Points</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <Link
            to="/docs/journeys/patient-registration"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Patient Registration
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Start here to understand the patient onboarding process
            </p>
          </Link>
          <Link
            to="/docs/journeys/appointment-booking"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Appointment Booking
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Learn the scheduling workflow end-to-end
            </p>
          </Link>
          <Link
            to="/docs/journeys/clinical-visit"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Complete Clinical Visit
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Full encounter from check-in to discharge
            </p>
          </Link>
          <Link
            to="/docs/journeys/lab-order-to-results"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Lab Order to Results
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Laboratory workflow from order to reporting
            </p>
          </Link>
        </div>
      </section>
    </div>
  );
}
