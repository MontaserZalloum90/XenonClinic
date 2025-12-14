import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { docsNavigation } from '@/lib/docs/docsData';
import {
  ChevronDown,
  ChevronRight,
  Rocket,
  Users,
  LayoutGrid,
  Route,
  Settings,
  Shield,
  Code,
  HelpCircle,
  FileText,
  Book,
} from 'lucide-react';

const iconMap: Record<string, React.ComponentType<{ className?: string }>> = {
  Rocket,
  Users,
  LayoutGrid,
  Route,
  Settings,
  Shield,
  Code,
  HelpCircle,
  FileText,
  Book,
};

interface SidebarItemProps {
  section: {
    id: string;
    title: string;
    path: string;
    icon?: string;
    children?: { id: string; title: string; path: string }[];
  };
  depth?: number;
}

function SidebarItem({ section, depth = 0 }: SidebarItemProps) {
  const location = useLocation();
  const [isOpen, setIsOpen] = useState(() => {
    // Auto-expand if current path is in children
    if (section.children) {
      return section.children.some((child) => location.pathname.startsWith(child.path));
    }
    return false;
  });

  const isActive = location.pathname === section.path;
  const isParentActive = section.children?.some((child) =>
    location.pathname.startsWith(child.path)
  );
  const hasChildren = section.children && section.children.length > 0;
  const Icon = section.icon ? iconMap[section.icon] : null;

  return (
    <div>
      <div className="flex items-center">
        {hasChildren ? (
          <button
            onClick={() => setIsOpen(!isOpen)}
            className={`flex items-center w-full gap-2 px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
              isParentActive
                ? 'text-primary-700 bg-primary-50'
                : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100'
            }`}
          >
            {Icon && <Icon className="h-4 w-4 flex-shrink-0" />}
            <span className="flex-1 text-left">{section.title}</span>
            {isOpen ? (
              <ChevronDown className="h-4 w-4 text-gray-400" />
            ) : (
              <ChevronRight className="h-4 w-4 text-gray-400" />
            )}
          </button>
        ) : (
          <Link
            to={section.path}
            className={`flex items-center w-full gap-2 px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
              isActive
                ? 'text-primary-700 bg-primary-50'
                : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100'
            }`}
          >
            {Icon && <Icon className="h-4 w-4 flex-shrink-0" />}
            <span className="flex-1">{section.title}</span>
          </Link>
        )}
      </div>

      {hasChildren && isOpen && (
        <div className="ml-4 mt-1 space-y-1 border-l border-gray-200 pl-3">
          {section.children?.map((child) => {
            const isChildActive = location.pathname === child.path;
            return (
              <Link
                key={child.id}
                to={child.path}
                className={`block px-3 py-1.5 text-sm rounded-lg transition-colors ${
                  isChildActive
                    ? 'text-primary-700 bg-primary-50 font-medium'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                }`}
              >
                {child.title}
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}

export function DocsSidebar() {
  return (
    <nav className="p-4 space-y-1">
      {docsNavigation.map((section) => (
        <SidebarItem key={section.id} section={section} />
      ))}
    </nav>
  );
}

export default DocsSidebar;
