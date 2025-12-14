import { useParams, Link } from 'react-router-dom';
import { getJourneyById, getModuleById, personasData } from '@/lib/docs/docsData';
import {
  Route,
  ArrowRight,
  CheckCircle,
  Clock,
  Users,
  AlertCircle,
  Shield,
  Bell,
  FileText,
  ChevronRight,
  Target,
  AlertTriangle,
  Zap,
} from 'lucide-react';

export default function JourneyPage() {
  const { journeyId } = useParams<{ journeyId: string }>();
  const journey = journeyId ? getJourneyById(journeyId) : undefined;

  if (!journey) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Journey Not Found</h1>
        <p className="text-gray-600 mb-4">The requested journey documentation does not exist.</p>
        <Link to="/docs/journeys" className="text-primary-600 hover:underline">
          View all journeys
        </Link>
      </div>
    );
  }

  const module = getModuleById(journey.module);
  const journeyPersonas = personasData.filter((p) => journey.personas.includes(p.id));

  return (
    <div className="space-y-10">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 text-sm text-gray-500 mb-3">
          <Link to="/docs/journeys" className="hover:text-primary-600">Journeys</Link>
          <ChevronRight className="h-4 w-4" />
          <span>{journey.name}</span>
        </div>
        <div className="flex items-start gap-4">
          <div className="p-4 bg-primary-100 text-primary-600 rounded-xl">
            <Route className="h-8 w-8" />
          </div>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{journey.name}</h1>
            <p className="text-lg text-gray-600 mt-2">{journey.description}</p>
            <div className="flex flex-wrap items-center gap-4 mt-3">
              <span className="inline-flex items-center gap-1 text-sm text-gray-500">
                <CheckCircle className="h-4 w-4" />
                {journey.steps.length} steps
              </span>
              <span className="inline-flex items-center gap-1 text-sm text-gray-500">
                <Clock className="h-4 w-4" />
                {journey.estimatedDuration}
              </span>
              <span className={`text-xs px-2.5 py-0.5 rounded-full ${
                journey.priority === 'high' ? 'bg-red-100 text-red-700' :
                journey.priority === 'medium' ? 'bg-yellow-100 text-yellow-700' :
                'bg-green-100 text-green-700'
              }`}>
                {journey.priority} priority
              </span>
              {module && (
                <Link
                  to={`/docs/modules/${module.id}`}
                  className="text-sm text-primary-600 hover:underline"
                >
                  {module.name} Module
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Goal */}
      <section className="bg-gradient-to-r from-primary-50 to-blue-50 rounded-xl p-6">
        <div className="flex items-start gap-3">
          <Target className="h-6 w-6 text-primary-600 flex-shrink-0" />
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Goal</h2>
            <p className="text-gray-700 mt-1">{journey.goal}</p>
          </div>
        </div>
      </section>

      {/* Personas Involved */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Users className="h-6 w-6 text-gray-400" />
          Personas Involved
        </h2>
        <div className="flex flex-wrap gap-3">
          {journeyPersonas.length > 0 ? (
            journeyPersonas.map((persona) => (
              <Link
                key={persona.id}
                to={`/docs/personas/${persona.id}`}
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all"
              >
                <Users className="h-4 w-4 text-gray-500" />
                <span className="font-medium text-gray-900">{persona.name}</span>
              </Link>
            ))
          ) : (
            journey.personas.map((personaId) => (
              <span
                key={personaId}
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg"
              >
                <Users className="h-4 w-4 text-gray-500" />
                <span className="font-medium text-gray-900 capitalize">{personaId}</span>
              </span>
            ))
          )}
        </div>
      </section>

      {/* Preconditions */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Preconditions</h2>
        <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-4">
          <ul className="space-y-2">
            {journey.preconditions.map((condition, index) => (
              <li key={index} className="flex items-start gap-2">
                <AlertTriangle className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                <span className="text-gray-700">{condition}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      {/* Step-by-Step Flow */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-6">Step-by-Step Flow</h2>
        <div className="space-y-4">
          {journey.steps.map((step, index) => (
            <div
              key={step.number}
              className="relative bg-white border border-gray-200 rounded-xl p-5"
            >
              {/* Step number and connector line */}
              <div className="absolute left-0 top-0 bottom-0 w-12 flex flex-col items-center">
                <div className="w-10 h-10 bg-primary-600 text-white rounded-full flex items-center justify-center font-bold text-lg z-10">
                  {step.number}
                </div>
                {index < journey.steps.length - 1 && (
                  <div className="flex-1 w-0.5 bg-gray-200 mt-2" />
                )}
              </div>

              <div className="ml-14">
                {/* Step header */}
                <div className="flex flex-wrap items-center gap-3 mb-3">
                  <h3 className="text-lg font-semibold text-gray-900">{step.action}</h3>
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                    {step.owner}
                  </span>
                  {step.rbac && (
                    <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
                      <Shield className="h-3 w-3" />
                      {step.rbac}
                    </span>
                  )}
                </div>

                {/* System Response */}
                <div className="bg-gray-50 rounded-lg p-3 mb-3">
                  <div className="flex items-start gap-2">
                    <Zap className="h-4 w-4 text-gray-500 mt-0.5" />
                    <div>
                      <span className="text-xs font-medium text-gray-500 uppercase">System Response</span>
                      <p className="text-gray-700">{step.systemResponse}</p>
                    </div>
                  </div>
                </div>

                {/* Validations */}
                {step.validations.length > 0 && (
                  <div className="mb-3">
                    <span className="text-xs font-medium text-gray-500 uppercase">Validations</span>
                    <ul className="mt-1 space-y-1">
                      {step.validations.map((validation, vIndex) => (
                        <li key={vIndex} className="flex items-center gap-2 text-sm text-gray-600">
                          <CheckCircle className="h-4 w-4 text-green-500" />
                          {validation}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                {/* Notifications */}
                {step.notifications && step.notifications.length > 0 && (
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Bell className="h-4 w-4 text-blue-500" />
                    <span>Notifications: {step.notifications.join(', ')}</span>
                  </div>
                )}

                {/* Audit Log */}
                {step.auditLog && (
                  <div className="flex items-center gap-2 text-sm text-gray-600 mt-1">
                    <FileText className="h-4 w-4 text-gray-500" />
                    <span>Audit: {step.auditLog}</span>
                  </div>
                )}

                {/* Screenshot placeholder */}
                {step.screenshot && (
                  <div className="mt-3 bg-gray-100 border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
                    <p className="text-sm text-gray-500">
                      Screenshot: {step.screenshot}
                    </p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Edge Cases */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Edge Cases & Exceptions</h2>
        <div className="space-y-3">
          {journey.edgeCases.map((edgeCase, index) => (
            <div
              key={index}
              className="flex items-start gap-3 p-4 bg-orange-50 border border-orange-200 rounded-lg"
            >
              <AlertTriangle className="h-5 w-5 text-orange-500 flex-shrink-0 mt-0.5" />
              <span className="text-gray-700">{edgeCase}</span>
            </div>
          ))}
        </div>
      </section>

      {/* Success Criteria */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Success Criteria</h2>
        <div className="bg-green-50 border border-green-200 rounded-xl p-4">
          <ul className="space-y-2">
            {journey.successCriteria.map((criteria, index) => (
              <li key={index} className="flex items-start gap-2">
                <CheckCircle className="h-5 w-5 text-green-600 flex-shrink-0 mt-0.5" />
                <span className="text-gray-700">{criteria}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      {/* Related Journeys */}
      {journey.relatedJourneys.length > 0 && (
        <section>
          <h2 className="text-2xl font-semibold text-gray-900 mb-4">Related Journeys</h2>
          <div className="flex flex-wrap gap-3">
            {journey.relatedJourneys.map((relatedId) => (
              <Link
                key={relatedId}
                to={`/docs/journeys/${relatedId}`}
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <Route className="h-4 w-4 text-gray-500 group-hover:text-primary-600" />
                <span className="font-medium text-gray-900 group-hover:text-primary-600 capitalize">
                  {relatedId.replace(/-/g, ' ')}
                </span>
                <ArrowRight className="h-4 w-4 text-gray-400 group-hover:text-primary-600" />
              </Link>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
