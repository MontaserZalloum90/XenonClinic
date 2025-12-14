import { Link } from 'react-router-dom';
import { modulesData } from '@/lib/docs/docsData';
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

const categoryLabels: Record<string, string> = {
  core: 'Core Modules',
  clinical: 'Clinical Modules',
  business: 'Business Modules',
  platform: 'Platform Modules',
  specialty: 'Specialty Modules',
};

const categoryColors: Record<string, string> = {
  core: 'bg-blue-100 text-blue-700',
  clinical: 'bg-green-100 text-green-700',
  business: 'bg-purple-100 text-purple-700',
  platform: 'bg-orange-100 text-orange-700',
  specialty: 'bg-pink-100 text-pink-700',
};

export default function ModulesIndex() {
  const categories = [...new Set(modulesData.map((m) => m.category))];

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Product Modules</h1>
        <p className="text-lg text-gray-600">
          XenonClinic includes 38+ integrated modules covering healthcare operations,
          business management, and platform administration. Each module is designed
          to work seamlessly with others.
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">38+</div>
          <div className="text-sm text-gray-500">Modules</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">98</div>
          <div className="text-sm text-gray-500">User Journeys</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">200+</div>
          <div className="text-sm text-gray-500">API Endpoints</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-4 text-center">
          <div className="text-3xl font-bold text-primary-600">52+</div>
          <div className="text-sm text-gray-500">Permissions</div>
        </div>
      </div>

      {/* Modules by Category */}
      {categories.map((category) => {
        const categoryModules = modulesData.filter((m) => m.category === category);
        return (
          <section key={category}>
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              {categoryLabels[category] || category}
            </h2>
            <div className="grid sm:grid-cols-2 gap-4">
              {categoryModules.map((module) => {
                const Icon = iconMap[module.icon] || Users;
                return (
                  <Link
                    key={module.id}
                    to={`/docs/modules/${module.id}`}
                    className="group flex items-start gap-4 p-5 bg-white border border-gray-200 rounded-xl hover:border-primary-300 hover:shadow-md transition-all"
                  >
                    <div className={`p-3 rounded-lg ${categoryColors[category]}`}>
                      <Icon className="h-6 w-6" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">
                          {module.name}
                        </h3>
                        <span className={`text-xs px-2 py-0.5 rounded-full ${categoryColors[category]}`}>
                          {module.category}
                        </span>
                      </div>
                      <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                        {module.description}
                      </p>
                      <div className="flex items-center gap-4 mt-2 text-xs text-gray-400">
                        <span>{module.features.length} features</span>
                        <span>{module.journeyCount} journeys</span>
                      </div>
                    </div>
                    <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 group-hover:translate-x-1 transition-all flex-shrink-0" />
                  </Link>
                );
              })}
            </div>
          </section>
        );
      })}
    </div>
  );
}
