import { useParams, Link } from 'react-router-dom';
import { getPersonaById, modulesData, journeysData } from '@/lib/docs/docsData';
import {
  Users,
  ArrowRight,
  CheckCircle,
  Shield,
  Briefcase,
  MapPin,
  Lightbulb,
  AlertCircle,
} from 'lucide-react';

export default function PersonaPage() {
  const { personaId } = useParams<{ personaId: string }>();
  const persona = personaId ? getPersonaById(personaId) : undefined;

  if (!persona) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Persona Not Found</h1>
        <p className="text-gray-600 mb-4">The requested persona documentation does not exist.</p>
        <Link to="/docs/personas" className="text-primary-600 hover:underline">
          View all personas
        </Link>
      </div>
    );
  }

  const personaModules = modulesData.filter((m) =>
    persona.primaryModules.includes(m.id)
  );

  const personaJourneys = journeysData.filter((j) =>
    persona.coreJourneys.includes(j.id)
  );

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-start gap-4">
        <div className="p-4 bg-primary-100 text-primary-600 rounded-xl">
          <Users className="h-8 w-8" />
        </div>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{persona.name}</h1>
          <p className="text-lg text-gray-600 mt-2">{persona.description}</p>
          <div className="flex items-center gap-4 mt-3">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              {persona.category}
            </span>
            <span className="text-sm text-gray-500">
              Role Type: <code className="bg-gray-100 px-2 py-0.5 rounded">{persona.accessScope.roleType}</code>
            </span>
          </div>
        </div>
      </div>

      {/* Responsibilities */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Briefcase className="h-6 w-6 text-gray-400" />
          Responsibilities
        </h2>
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <ul className="grid sm:grid-cols-2 gap-3">
            {persona.responsibilities.map((responsibility, index) => (
              <li key={index} className="flex items-start gap-2">
                <CheckCircle className="h-5 w-5 text-green-500 flex-shrink-0 mt-0.5" />
                <span className="text-gray-700">{responsibility}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      {/* Access Scope */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Shield className="h-6 w-6 text-gray-400" />
          Access Scope
        </h2>
        <div className="grid sm:grid-cols-3 gap-4">
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <div className="text-sm text-gray-500">Access Level</div>
            <div className="text-lg font-medium text-gray-900 capitalize mt-1">
              {persona.accessScope.level}
            </div>
          </div>
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <div className="text-sm text-gray-500">Data Access</div>
            <div className="text-lg font-medium text-gray-900 mt-1">
              {persona.accessScope.dataAccess}
            </div>
          </div>
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <div className="text-sm text-gray-500">Role Type</div>
            <div className="text-lg font-medium text-gray-900 mt-1">
              {persona.accessScope.roleType}
            </div>
          </div>
        </div>
      </section>

      {/* Permissions */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Permissions</h2>
        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex flex-wrap gap-2">
            {persona.permissions.map((permission) => (
              <span
                key={permission}
                className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-gray-100 text-gray-700"
              >
                {permission}
              </span>
            ))}
          </div>
        </div>
        <p className="text-sm text-gray-500 mt-2">
          See <Link to="/docs/security-rbac" className="text-primary-600 hover:underline">Security & RBAC</Link> for detailed permission descriptions.
        </p>
      </section>

      {/* Primary Modules */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Primary Modules</h2>
        <div className="grid sm:grid-cols-2 gap-3">
          {personaModules.length > 0 ? (
            personaModules.map((module) => (
              <Link
                key={module.id}
                to={`/docs/modules/${module.id}`}
                className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <div>
                  <div className="font-medium text-gray-900 group-hover:text-primary-600">
                    {module.name}
                  </div>
                  <div className="text-sm text-gray-500">
                    {module.features.length} features
                  </div>
                </div>
                <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
              </Link>
            ))
          ) : (
            persona.primaryModules.map((moduleId) => (
              <div key={moduleId} className="p-4 bg-white border border-gray-200 rounded-lg">
                <div className="font-medium text-gray-900 capitalize">
                  {moduleId.replace(/-/g, ' ')}
                </div>
              </div>
            ))
          )}
        </div>
      </section>

      {/* Core Journeys */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Core Journeys</h2>
        {personaJourneys.length > 0 ? (
          <div className="space-y-3">
            {personaJourneys.map((journey) => (
              <Link
                key={journey.id}
                to={`/docs/journeys/${journey.id}`}
                className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 hover:shadow transition-all group"
              >
                <div className="flex items-center gap-3">
                  <CheckCircle className="h-5 w-5 text-green-500" />
                  <div>
                    <div className="font-medium text-gray-900 group-hover:text-primary-600">
                      {journey.name}
                    </div>
                    <div className="text-sm text-gray-500">
                      {journey.steps.length} steps &middot; {journey.estimatedDuration}
                    </div>
                  </div>
                </div>
                <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600" />
              </Link>
            ))}
          </div>
        ) : (
          <div className="space-y-2">
            {persona.coreJourneys.map((journeyId) => (
              <div key={journeyId} className="p-4 bg-white border border-gray-200 rounded-lg">
                <div className="font-medium text-gray-900 capitalize">
                  {journeyId.replace(/-/g, ' ')}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Entry Points */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <MapPin className="h-6 w-6 text-gray-400" />
          Entry Points
        </h2>
        <div className="flex flex-wrap gap-2">
          {persona.entryPoints.map((entry) => (
            <code
              key={entry}
              className="inline-flex items-center px-3 py-1.5 bg-gray-900 text-gray-100 rounded text-sm"
            >
              {entry}
            </code>
          ))}
        </div>
        <p className="text-sm text-gray-500 mt-2">
          Primary navigation routes this persona will access most frequently.
        </p>
      </section>

      {/* Common Tasks */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Common Tasks</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <ul className="divide-y divide-gray-200">
            {persona.commonTasks.map((task, index) => (
              <li key={index} className="flex items-center gap-3 px-4 py-3">
                <span className="flex-shrink-0 w-6 h-6 bg-gray-100 text-gray-600 rounded-full flex items-center justify-center text-xs font-medium">
                  {index + 1}
                </span>
                <span className="text-gray-700">{task}</span>
              </li>
            ))}
          </ul>
        </div>
      </section>

      {/* Tips */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Lightbulb className="h-6 w-6 text-yellow-500" />
          Tips & Best Practices
        </h2>
        <div className="space-y-3">
          {persona.tips.map((tip, index) => (
            <div
              key={index}
              className="flex items-start gap-3 p-4 bg-yellow-50 border border-yellow-200 rounded-lg"
            >
              <Lightbulb className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" />
              <span className="text-gray-700">{tip}</span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
