import { Link } from 'react-router-dom';
import { personasData } from '@/lib/docs/docsData';
import { Users, ArrowRight, Stethoscope, UserCog, Briefcase, Laptop, Heart } from 'lucide-react';

const categoryIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  clinical: Stethoscope,
  'front-office': Users,
  business: Briefcase,
  platform: Laptop,
  specialty: Heart,
  portal: Users,
};

const categoryLabels: Record<string, string> = {
  clinical: 'Clinical Staff',
  'front-office': 'Front Office',
  business: 'Business Operations',
  platform: 'Platform Administration',
  specialty: 'Specialty',
  portal: 'Patient Portal',
};

const categoryColors: Record<string, string> = {
  clinical: 'bg-green-100 text-green-700 border-green-200',
  'front-office': 'bg-blue-100 text-blue-700 border-blue-200',
  business: 'bg-purple-100 text-purple-700 border-purple-200',
  platform: 'bg-orange-100 text-orange-700 border-orange-200',
  specialty: 'bg-pink-100 text-pink-700 border-pink-200',
  portal: 'bg-cyan-100 text-cyan-700 border-cyan-200',
};

export default function PersonasIndex() {
  const categories = [...new Set(personasData.map((p) => p.category))];

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">User Personas</h1>
        <p className="text-lg text-gray-600">
          XenonClinic supports {personasData.length} distinct user personas across clinical, operational,
          and administrative roles. Each persona has specific permissions, access
          scopes, and workflows tailored to their responsibilities.
        </p>
      </div>

      {/* Overview Cards */}
      <div className="grid sm:grid-cols-3 gap-4">
        <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-5 border border-green-200">
          <Stethoscope className="h-8 w-8 text-green-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Clinical Staff</h3>
          <p className="text-sm text-gray-600 mt-1">
            Doctors, nurses, and specialists providing patient care
          </p>
        </div>
        <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl p-5 border border-purple-200">
          <Briefcase className="h-8 w-8 text-purple-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Business Operations</h3>
          <p className="text-sm text-gray-600 mt-1">
            HR, finance, marketing, and inventory management
          </p>
        </div>
        <div className="bg-gradient-to-br from-orange-50 to-orange-100 rounded-xl p-5 border border-orange-200">
          <Laptop className="h-8 w-8 text-orange-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Platform Admin</h3>
          <p className="text-sm text-gray-600 mt-1">
            System configuration, security, and tenant management
          </p>
        </div>
      </div>

      {/* Personas by Category */}
      {categories.map((category) => {
        const categoryPersonas = personasData.filter((p) => p.category === category);
        const Icon = categoryIcons[category] || Users;
        const colorClass = categoryColors[category] || 'bg-gray-100 text-gray-700 border-gray-200';

        return (
          <section key={category}>
            <div className="flex items-center gap-3 mb-4">
              <div className={`p-2 rounded-lg ${colorClass}`}>
                <Icon className="h-5 w-5" />
              </div>
              <h2 className="text-2xl font-semibold text-gray-900">
                {categoryLabels[category] || category}
              </h2>
            </div>

            <div className="grid sm:grid-cols-2 gap-4">
              {categoryPersonas.map((persona) => (
                <Link
                  key={persona.id}
                  to={`/docs/personas/${persona.id}`}
                  className="group flex items-start gap-4 p-5 bg-white border border-gray-200 rounded-xl hover:border-primary-300 hover:shadow-md transition-all"
                >
                  <div className={`p-3 rounded-lg ${colorClass}`}>
                    <UserCog className="h-6 w-6" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                      {persona.name}
                    </h3>
                    <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                      {persona.description}
                    </p>
                    <div className="flex flex-wrap gap-2 mt-3">
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                        {persona.permissions.length} permissions
                      </span>
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                        {persona.primaryModules.length} modules
                      </span>
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                        {persona.coreJourneys.length} journeys
                      </span>
                    </div>
                  </div>
                  <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 group-hover:translate-x-1 transition-all flex-shrink-0" />
                </Link>
              ))}
            </div>
          </section>
        );
      })}

      {/* RBAC Note */}
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
        <h3 className="font-semibold text-gray-900 mb-2">Understanding Role-Based Access</h3>
        <p className="text-gray-600">
          Each persona has a defined set of permissions that control what features
          they can access and what actions they can perform. Permissions are additive -
          a user can have multiple roles to combine access rights. Custom roles can
          also be created to match your organization's specific needs.
        </p>
        <Link
          to="/docs/security-rbac"
          className="inline-flex items-center gap-1 mt-3 text-primary-600 font-medium hover:underline"
        >
          Learn more about RBAC <ArrowRight className="h-4 w-4" />
        </Link>
      </div>
    </div>
  );
}
