import React, { useState, useMemo } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { useNavigation, useT, useTenant } from "../../contexts/TenantContext";
import type { NavItem } from "../../types/tenant";
import * as LucideIcons from "lucide-react";

/**
 * DynamicNavigation - Renders navigation from tenant context
 * Automatically filters by enabled features and user roles
 */

interface DynamicNavigationProps {
  /** Whether sidebar is collapsed */
  collapsed?: boolean;
  /** Additional className */
  className?: string;
  /** Callback when nav item is clicked */
  onItemClick?: (item: NavItem) => void;
}

export const DynamicNavigation: React.FC<DynamicNavigationProps> = ({
  collapsed = false,
  className = "",
  onItemClick,
}) => {
  const navigation = useNavigation();
  const t = useT();

  if (!navigation || navigation.length === 0) {
    return null;
  }

  return (
    <nav className={`space-y-1 ${className}`}>
      {navigation.map((item) => (
        <NavItemComponent
          key={item.id}
          item={item}
          collapsed={collapsed}
          t={t}
          onItemClick={onItemClick}
        />
      ))}
    </nav>
  );
};

// ============================================
// NavItem Component
// ============================================

interface NavItemComponentProps {
  item: NavItem;
  collapsed: boolean;
  t: (key: string, fallback?: string) => string;
  onItemClick?: (item: NavItem) => void;
  depth?: number;
}

const NavItemComponent: React.FC<NavItemComponentProps> = ({
  item,
  collapsed,
  t,
  onItemClick,
  depth = 0,
}) => {
  const location = useLocation();
  const [isExpanded, setIsExpanded] = useState(false);

  const hasChildren = item.children && item.children.length > 0;
  const isActive =
    location.pathname === item.route ||
    (hasChildren &&
      item.children?.some((child) =>
        location.pathname.startsWith(child.route),
      ));

  const label = t(item.label, item.label);
  const Icon = useMemo(() => getIcon(item.icon), [item.icon]);

  const handleClick = (e: React.MouseEvent) => {
    if (hasChildren) {
      e.preventDefault();
      setIsExpanded(!isExpanded);
    }
    onItemClick?.(item);
  };

  const baseClasses = `
    group flex items-center px-3 py-2 text-sm font-medium rounded-md
    transition-colors duration-150 ease-in-out
  `;

  const activeClasses = isActive
    ? "bg-primary-100 text-primary-700 dark:bg-primary-900/50 dark:text-primary-300"
    : "text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800";

  const depthPadding = depth > 0 ? `pl-${4 + depth * 4}` : "";

  // If collapsed and has no children, just show icon
  if (collapsed && !hasChildren) {
    return (
      <NavLink
        to={item.route}
        className={`${baseClasses} ${activeClasses} justify-center`}
        title={label}
        onClick={() => onItemClick?.(item)}
      >
        {Icon && <Icon className="h-5 w-5" />}
      </NavLink>
    );
  }

  return (
    <div className={depthPadding}>
      {hasChildren ? (
        <>
          <button
            onClick={handleClick}
            className={`${baseClasses} ${activeClasses} w-full justify-between`}
          >
            <div className="flex items-center">
              {Icon && (
                <Icon className={`h-5 w-5 ${collapsed ? "" : "mr-3"}`} />
              )}
              {!collapsed && <span>{label}</span>}
            </div>
            {!collapsed && (
              <LucideIcons.ChevronDown
                className={`h-4 w-4 transition-transform ${isExpanded ? "rotate-180" : ""}`}
              />
            )}
          </button>

          {!collapsed && isExpanded && (
            <div className="mt-1 space-y-1">
              {item.children?.map((child) => (
                <NavItemComponent
                  key={child.id}
                  item={child}
                  collapsed={collapsed}
                  t={t}
                  onItemClick={onItemClick}
                  depth={depth + 1}
                />
              ))}
            </div>
          )}
        </>
      ) : (
        <NavLink
          to={item.route}
          className={({ isActive: linkActive }) => `
            ${baseClasses}
            ${linkActive ? "bg-primary-100 text-primary-700 dark:bg-primary-900/50 dark:text-primary-300" : activeClasses}
          `}
          onClick={() => onItemClick?.(item)}
        >
          {Icon && <Icon className={`h-5 w-5 ${collapsed ? "" : "mr-3"}`} />}
          {!collapsed && (
            <>
              <span className="flex-1">{label}</span>
              {item.badge && <NavBadge badge={item.badge} />}
            </>
          )}
        </NavLink>
      )}
    </div>
  );
};

// ============================================
// Badge Component
// ============================================

interface NavBadgeProps {
  badge: NonNullable<NavItem["badge"]>;
}

const NavBadge: React.FC<NavBadgeProps> = ({ badge }) => {
  // In a real app, you'd fetch the count from an API or context
  const count = 0; // Placeholder

  if (badge.type === "dot") {
    return <span className="ml-2 h-2 w-2 rounded-full bg-red-500" />;
  }

  if (count === 0) return null;

  return (
    <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
      {count > 99 ? "99+" : count}
    </span>
  );
};

// ============================================
// Icon Helper
// ============================================

type IconComponent = React.FC<{ className?: string }>;

const getIcon = (iconName: string): IconComponent | null => {
  // Map icon names to Lucide icons
  const iconMap: Record<string, IconComponent> = {
    LayoutDashboard: LucideIcons.LayoutDashboard,
    Users: LucideIcons.Users,
    Calendar: LucideIcons.Calendar,
    ClipboardList: LucideIcons.ClipboardList,
    Receipt: LucideIcons.Receipt,
    Package: LucideIcons.Package,
    FlaskConical: LucideIcons.FlaskConical,
    BarChart: LucideIcons.BarChart,
    UserCog: LucideIcons.UserCog,
    Settings: LucideIcons.Settings,
    Target: LucideIcons.Target,
    ShoppingCart: LucideIcons.ShoppingCart,
    FileText: LucideIcons.FileText,
    Warehouse: LucideIcons.Warehouse,
    Ear: LucideIcons.Ear,
    Activity: LucideIcons.Activity,
    Headphones: LucideIcons.Headphones,
    Wrench: LucideIcons.Wrench,
    Mic: LucideIcons.Mic,
    Smile: LucideIcons.Smile,
    Grid3x3: LucideIcons.Grid3x3,
    ListChecks: LucideIcons.ListChecks,
    Stethoscope: LucideIcons.Stethoscope,
    Scan: LucideIcons.Scan,
    LineChart: LucideIcons.LineChart,
    PawPrint: LucideIcons.PawPrint,
    Syringe: LucideIcons.Syringe,
    Scissors: LucideIcons.Scissors,
    Home: LucideIcons.Home,
    Eye: LucideIcons.Eye,
    Sparkles: LucideIcons.Sparkles,
    Building: LucideIcons.Building,
    CreditCard: LucideIcons.CreditCard,
    TrendingUp: LucideIcons.TrendingUp,
    Plus: LucideIcons.Plus,
    Pencil: LucideIcons.Pencil,
    Trash: LucideIcons.Trash,
    Download: LucideIcons.Download,
    Upload: LucideIcons.Upload,
    ChevronDown: LucideIcons.ChevronDown,
    ChevronRight: LucideIcons.ChevronRight,
  };

  return iconMap[iconName] || null;
};

// ============================================
// Sidebar Component with Dynamic Navigation
// ============================================

interface DynamicSidebarProps {
  collapsed?: boolean;
  onCollapsedChange?: (collapsed: boolean) => void;
  className?: string;
}

export const DynamicSidebar: React.FC<DynamicSidebarProps> = ({
  collapsed = false,
  onCollapsedChange,
  className = "",
}) => {
  const { context, isLoading } = useTenant();

  if (isLoading) {
    return (
      <div
        className={`flex flex-col h-full bg-white dark:bg-gray-900 ${className}`}
      >
        <div className="flex items-center justify-center h-16 border-b">
          <div className="animate-pulse h-8 w-32 bg-gray-200 rounded" />
        </div>
        <div className="flex-1 p-4 space-y-2">
          {[...Array(8)].map((_, i) => (
            <div key={i} className="animate-pulse h-10 bg-gray-100 rounded" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div
      className={`flex flex-col h-full bg-white dark:bg-gray-900 border-r ${className}`}
    >
      {/* Logo/Brand */}
      <div className="flex items-center h-16 px-4 border-b">
        {context?.logoUrl ? (
          <img
            src={context.logoUrl}
            alt={context.companyName}
            className={`h-8 ${collapsed ? "w-8" : "w-auto max-w-[150px]"}`}
          />
        ) : (
          <span
            className={`font-bold text-primary-600 ${collapsed ? "text-lg" : "text-xl"}`}
          >
            {collapsed ? context?.companyName?.charAt(0) : context?.companyName}
          </span>
        )}

        {onCollapsedChange && (
          <button
            onClick={() => onCollapsedChange(!collapsed)}
            className="ml-auto p-1 rounded hover:bg-gray-100 dark:hover:bg-gray-800"
          >
            {collapsed ? (
              <LucideIcons.ChevronRight className="h-5 w-5" />
            ) : (
              <LucideIcons.ChevronLeft className="h-5 w-5" />
            )}
          </button>
        )}
      </div>

      {/* Navigation */}
      <div className="flex-1 overflow-y-auto p-3">
        <DynamicNavigation collapsed={collapsed} />
      </div>

      {/* Footer */}
      {!collapsed && (
        <div className="p-4 border-t text-xs text-gray-500">
          <div>{context?.branchName}</div>
          <div className="text-gray-400">{context?.companyType}</div>
        </div>
      )}
    </div>
  );
};

export default DynamicNavigation;
