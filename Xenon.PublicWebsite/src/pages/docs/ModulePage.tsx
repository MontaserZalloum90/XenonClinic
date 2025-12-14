import { useParams, Link } from 'react-router-dom';
import { getModuleById, getJourneysByModule, personasData } from '@/lib/docs/docsData';
import {
  Users,
  Calendar,
  Stethoscope,
  FlaskConical,
  ScanLine,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Megaphone,
  GitBranch,
  UserCircle,
  BarChart3,
  Heart,
  ArrowRight,
  CheckCircle,
  Shield,
  AlertCircle,
  ExternalLink,
} from 'lucide-react';

const iconMap: Record<string, React.ComponentType<{ className?: string }>> = {
  Users,
  Calendar,
  Stethoscope,
  FlaskConical,
  ScanLine,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Megaphone,
  GitBranch,
  UserCircle,
  BarChart3,
  Heart,
};

export default function ModulePage() {
  const { moduleId } = useParams<{ moduleId: string }>();
  const module = moduleId ? getModuleById(moduleId) : undefined;
  const journeys = moduleId ? getJourneysByModule(moduleId) : [];

  if (!module) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Module Not Found</h1>
        <p className="text-gray-600 mb-4">The requested module documentation does not exist.</p>
        <Link to="/docs/modules" className="text-primary-600 hover:underline">
          View all modules
        </Link>
      </div>
    );
  }

  const Icon = iconMap[module.icon] || Users;
  const modulePersonas = personasData.filter((p) => module.personas.includes(p.id));

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-start gap-4">
        <div className="p-4 bg-primary-100 text-primary-600 rounded-xl">
          <Icon className="h-8 w-8" />
        </div>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{module.name}</h1>
          <p className="text-lg text-gray-600 mt-2">{module.description}</p>
          <div className="flex items-center gap-4 mt-3">
            <span className="text-sm text-gray-500">Route: <code className="bg-gray-100 px-2 py-0.5 rounded">{module.route}</code></span>
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              {module.category}
            </span>
          </div>
        </div>
      </div>

      {/* Business Value */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Business Value</h2>
        <div className="bg-gradient-to-r from-primary-50 to-blue-50 rounded-xl p-6">
          <p className="text-gray-700 leading-relaxed">{module.businessValue}</p>
        </div>
      </section>

      {/* Who Uses This */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Who Uses This Module</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {modulePersonas.length > 0 ? (
            modulePersonas.map((persona) => (
              <Link
                key={persona.id}
                to={`/docs/personas/${persona.id}`}
                className="flex items-center gap-3 p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <div className="p-2 bg-gray-100 rounded-lg group-hover:bg-primary-100 transition-colors">
                  <Users className="h-5 w-5 text-gray-600 group-hover:text-primary-600" />
                </div>
                <div>
                  <div className="font-medium text-gray-900 group-hover:text-primary-600">
                    {persona.name}
                  </div>
                  <div className="text-xs text-gray-500">{persona.category}</div>
                </div>
              </Link>
            ))
          ) : (
            module.personas.map((personaId) => (
              <div key={personaId} className="flex items-center gap-3 p-4 bg-white border border-gray-200 rounded-lg">
                <div className="p-2 bg-gray-100 rounded-lg">
                  <Users className="h-5 w-5 text-gray-600" />
                </div>
                <div className="font-medium text-gray-900 capitalize">
                  {personaId.replace(/-/g, ' ')}
                </div>
              </div>
            ))
          )}
        </div>
      </section>

      {/* Features */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Features</h2>
        <div className="space-y-4">
          {module.features.map((feature, index) => (
            <div key={feature.id} className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg">
              <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold text-sm">
                {index + 1}
              </div>
              <div>
                <h3 className="font-medium text-gray-900">{feature.name}</h3>
                <p className="text-gray-600 mt-1">{feature.description}</p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Required Permissions */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Required Roles & Permissions</h2>
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Permission
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Description
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {module.permissions.map((permission) => (
                <tr key={permission}>
                  <td className="px-4 py-3">
                    <code className="bg-gray-100 text-sm px-2 py-0.5 rounded">{permission}</code>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">
                    {permission.replace(':', ' ').replace(/_/g, ' ')}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <p className="text-sm text-gray-500 mt-2 flex items-center gap-1">
          <Shield className="h-4 w-4" />
          See <Link to="/docs/security-rbac" className="text-primary-600 hover:underline">Security & RBAC</Link> for the full permission matrix.
        </p>
      </section>

      {/* Related Journeys */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Related User Journeys</h2>
        {journeys.length > 0 ? (
          <div className="space-y-3">
            {journeys.map((journey) => (
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
          <p className="text-gray-500 italic">
            Detailed journey documentation for this module coming soon.
          </p>
        )}
      </section>

      {/* API Reference */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">API Reference</h2>
        <div className="bg-gray-900 text-gray-100 rounded-lg p-4 overflow-x-auto">
          <pre className="text-sm">
            <code>
{`# ${module.name} API Endpoints

Base URL: /api/${module.route.replace('/', '')}

GET    /api/${module.route.replace('/', '')}          # List all
GET    /api/${module.route.replace('/', '')}/:id      # Get by ID
POST   /api/${module.route.replace('/', '')}          # Create new
PUT    /api/${module.route.replace('/', '')}/:id      # Update
DELETE /api/${module.route.replace('/', '')}/:id      # Delete`}
            </code>
          </pre>
        </div>
        <p className="text-sm text-gray-500 mt-2 flex items-center gap-1">
          <ExternalLink className="h-4 w-4" />
          See <Link to="/docs/api-reference" className="text-primary-600 hover:underline">API Reference</Link> for detailed documentation.
        </p>
      </section>
    </div>
  );
}
